using rfid1128.Infrastructure;
using System;
using System.Windows.Input;
using TechnologySolutions.Rfid;
using TechnologySolutions.Rfid.AsciiProtocol;

namespace rfid1128.ViewModels
{
    public class ReaderViewModel : ObservableObject
    {
        private IProgress<IReaderOperationBatteryStatus> reportUpdate;
        private readonly IReaderOperationBatteryStatus batteryStatus;        

        public ReaderViewModel(IReader reader)
        {
            this.reportUpdate = new Progress<IReaderOperationBatteryStatus>(this.BatteryStatus_Update);
            this.Reader = reader;
            this.batteryStatus = this.Reader.OperationOfType<IReaderOperationBatteryStatus>();
            this.batteryStatus.Updated += (sender, e) => this.reportUpdate.Report(this.batteryStatus);
#if DEBUG
            // Update the status frequently when debugging (default is 5 minutes)
            this.batteryStatus.TriggerInterval = TimeSpan.FromSeconds(10);
#endif

            this.MakeActiveCommand = new RelayCommand(this.ExecuteMakeActive, this.CanExecuteMakeActive);
            this.DisconnectCommand = new RelayCommand(this.ExecuteDisconnect, this.CanExecuteDisconnect);
        }

        #region BatteryPercent
        public int BatteryPercent
        {
            get => this.batteryPercent;
            set => this.Set(ref this.batteryPercent, value);
        }

        private int batteryPercent;
        #endregion

        #region ChargeStatus        
        public ChargeStatus ChargeStatus
        {
            get => this.chargeStatus;
            set => this.Set(ref this.chargeStatus, value);
        }

        private ChargeStatus chargeStatus;
        #endregion

        #region ConnectionState        
        public ReaderStates ConnectionState
        {
            get => this.connectionState;
            set
            {
                if (this.Set(ref this.connectionState, value))
                {
                    if (this.ConnectionState ==  ReaderStates.Connected)
                    {
                        this.MakeActiveCommand.RefreshCanExecute();
                        this.OnPropertyChanged("SerialNumber");
                        this.OnPropertyChanged("ConnectionState");

                        this.StartBatteryMonitoring();
                    }
                    else if (this.ConnectionState == ReaderStates.Disconnecting
                        || this.ConnectionState == ReaderStates.Lost)
                    {
                        this.StopBatteryMonitoring();
                        this.ChargeStatus = ChargeStatus.Unknown;
                        this.MakeActiveCommand.RefreshCanExecute();
                    }
                }

                this.DisconnectCommand.RefreshCanExecute();
            }
        }

        private ReaderStates connectionState;
        #endregion

        #region IsActive        
        public bool IsActive
        {
            get => this.isActive;
            set
            {
                this.Set(ref this.isActive, value);
                this.MakeActiveCommand.RefreshCanExecute();
            }
        }

        private bool isActive;
        #endregion

        #region IsSelected
        public bool IsSelected
        {
            get => this.selected;
            set
            {
                this.Set(ref this.selected, value);
                this.MakeActiveCommand.RefreshCanExecute();
                this.DisconnectCommand.RefreshCanExecute();
            }
        }

        private bool selected;
        #endregion

        public ICommand MakeActiveCommand { get; private set; }

        public ICommand DisconnectCommand { get; private set; }

        /// <summary>
        /// Gets the reader represented by this model
        /// </summary>
        public IReader Reader { get; private set; }

        public string SerialNumber => this.Reader.SerialNumber;        

        public async void StartBatteryMonitoring()
        {
            await this.batteryStatus.EnableAsync();
            await this.batteryStatus.StartAsync();
        }

        public async void StopBatteryMonitoring()
        {
            await this.batteryStatus.StopAsync();
            await this.batteryStatus.DisableAsync();
        }

        private bool CanExecuteMakeActive()
        {
            return this.ConnectionState == ReaderStates.Connected && this.IsSelected && !this.IsActive;
        }

        private bool CanExecuteDisconnect()
        {
            return this.ConnectionState == ReaderStates.Connected && this.IsSelected;
        }

        private void ExecuteDisconnect()
        {
            if (this.ConnectionState == ReaderStates.Connected)
            {
                this.ConnectionState = ReaderStates.Disconnecting;
            }
        }

        private void ExecuteMakeActive()
        {
            this.IsActive = true;
        }

        private void BatteryStatus_Update(IReaderOperationBatteryStatus batteryStatus)
        {
            this.BatteryPercent = batteryStatus.BatteryLevel;
            this.ChargeStatus = batteryStatus.Status;
        }
    }
}
