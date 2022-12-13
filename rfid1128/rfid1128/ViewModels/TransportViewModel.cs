using rfid1128.Helpers;
using rfid1128.Infrastructure;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TechnologySolutions.Rfid.AsciiProtocol.Extensions;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;

namespace rfid1128.ViewModels
{
    /// <summary>
    /// A visual representation of a <see cref="IAsciiTransport"/>
    /// </summary>
    public class TransportViewModel : ViewModelBase
    {
        /// <summary>
        /// The model we are representing
        /// </summary>
        private readonly IAsciiTransport model;

        /// <summary>
        /// Used to update the view model when the model state changes
        /// </summary>
        private IProgress<IAsciiTransport> modelView;

        /// <summary>
        /// The manager for this transport
        /// </summary>
        private readonly IAsciiTransportsManager transportsManager;

        public TransportViewModel(IAsciiTransportsManager transportsManager, IAsciiTransport model)
        {
            this.transportsManager = transportsManager ?? throw new ArgumentNullException("model");
            this.modelView = new Progress<IAsciiTransport>(this.UpdateFromTransport);
            this.model = model ?? throw new ArgumentNullException("model");
            (this.model as INotifyPropertyChanged).PropertyChanged += this.Model_PropertyChanged; // TODO: refactor out this dependency on insider knowledge

            this.model.StateChanged += (sender, e) => { this.modelView.Report(e.Transport); };

            this.DisplayInfoLine = this.model.DisplayInfoLine;
            this.DisplayName = this.model.DisplayName;
            this.State = this.model.State.ToString(); 

            this.ConnectCommand = new RelayCommand(this.ExecuteConnect, this.CanExecuteConnect);
            this.DisconnectCommand = new RelayCommand(this.ExecuteDisconnect, this.CanExecuteDisconnect);
            this.SleepCommand = new RelayCommand(this.ExecuteSleep, this.CanExecuteDisconnect); // State always matches Disconnect's 
            this.ForgetCommand = new RelayCommand(this.ExecuteForget, this.CanExecuteForget);
        }

        public ICommand ConnectCommand { get; private set; }
        public ICommand DisconnectCommand { get; private set; }
        public ICommand SleepCommand { get; private set; }
        public ICommand ForgetCommand { get; private set; }

        public string DisplayInfoLine
        {
            get => this.displayInfoLine;
            set => this.Set(ref this.displayInfoLine, value);
        }
        private string displayInfoLine;

        public string DisplayName
        {
            get => this.displayName;
            set => this.Set(ref this.displayName, value);
        }
        private string displayName;

        public string Id => this.model.Id;

        public string State
        {
            get => this.state;
            set => this.Set(ref this.state, value);
        }
        private string state;

        public string Transport => this.model.Physical.ToString();

        private void UpdateFromTransport(IAsciiTransport transport)
        {
            // at the moment we should always only raise events about our own transport but considering a change
            if (this.model == transport)
            {
                this.DisplayName = transport.DisplayName;
                this.DisplayInfoLine = transport.DisplayInfoLine;
                this.State = transport.State.ToString();

                // Update the connect and disconnect commands based on state
                this.ConnectCommand.RefreshCanExecute();
                this.DisconnectCommand.RefreshCanExecute();
                this.SleepCommand.RefreshCanExecute();
                this.ForgetCommand.RefreshCanExecute();
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DisplayName":
                    this.DisplayName = this.model.DisplayName;
                    break;

                case "DisplayInfoLine":
                    this.DisplayInfoLine = this.model.DisplayInfoLine;
                    break;

                //case "State":

                default:
                    System.Diagnostics.Debug.WriteLine(string.Format("ModelState: {0}    Model Connection: {1}",
                        this.model.State.ToString() ?? "-", this.model.Connection == null ? "-" : "+"
                        ));
                    break;
            }
        }

        private async void ExecuteConnect()
        {
            try
            {
                await new RfidHelper().Connect(this.model);
            }
            catch (Exception ex)
            {
                this.ReportError(ex);
            }
        }

        private void ExecuteDisconnect()
        {
            try
            {
                new RfidHelper().Disconnect(this.model);
            }
            catch (Exception ex)
            {
                this.ReportError(ex);
            }
        }


        private void ExecuteSleep()
        {
            try
            {
                // Put reader to sleep (this ensures the Bluetooth connection is dropped on iOS connections)
                // 
                this.model.Connection.WriteLine(".sl");

                // Also disconnect directly to prevent commands being sent to the Reader
                // (the soon-to-be-broken connection can take a while to be reported)
                this.model.Disconnect();
            }
            catch (Exception ex)
            {
                this.ReportError(ex);
            }
        }


        private async void ExecuteForget()
        {
            try
            {
                // Only try to remove Bluetooth transports
                if( this.model.Physical == PhysicalTransport.Bluetooth )
                {
                    // Assumes that Info line for Bluetooth transport is (always) the Mac Address
                    BluetoothAddress address = BluetoothAddress.Parse(this.model.DisplayInfoLine);

                    var result = await this.transportsManager.BluetoothSecurity.UnpairAsync(address);
                    if (!result)
                    {
                        throw new ApplicationException(string.Format("Failed to unpair to {0}", this.model.DisplayName));
                    }
                }
            }
            catch (Exception ex)
            {
                this.ReportError(ex);
            }
        }

        private bool CanExecuteConnect()
        {
            return this.model.State == ConnectionState.Available || this.model.State == ConnectionState.Disconnected || this.model.State == ConnectionState.Lost;
        }

        private bool CanExecuteDisconnect()
        {
            return this.model.State == ConnectionState.Connected;
        }

        private bool CanExecuteForget()
        {
            return this.model.Physical == PhysicalTransport.Bluetooth && this.transportsManager.BluetoothSecurity.CanUnpair;
        }

        private void Connection_Received(object sender, EventArgs e)
        {
            var x = sender as TechnologySolutions.Rfid.AsciiProtocol.Transports.IAsciiConnection;
            while(x.IsLineAvailable)
            {
                System.Diagnostics.Debug.WriteLine(x.ReadLine());
            }
        }
    }
}
