using rfid1128.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using TechnologySolutions.Rfid.AsciiProtocol;
using TechnologySolutions.Rfid.AsciiProtocol.Extensions;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;

namespace rfid1128.ViewModels
{
    public class TransportsViewModel : ViewModelBase, ILifecycle
    {
        private readonly IAsciiTransportsManager transportsManager;
        private IProgress<IAsciiTransport> transportsManagerDispatcher;

        private readonly IHostBarcodeHandler hostBarcode;
        private IProgress <BarcodeEventArgs> hostBarcodeDispatcher;

        private readonly System.Text.RegularExpressions.Regex macMatcher;
        private IAsciiTransportEnumerator addNewEnumerator;

        private bool wasHostBarcodeEnabled;

        public TransportsViewModel(IAsciiTransportsManager transportsManager, IHostBarcodeHandler hostBarcode)
        {
            this.transportsManager = transportsManager ?? throw new ArgumentNullException("transportsManager");
            this.hostBarcode = hostBarcode;

            this.Enumerators = this.transportsManager.Enumerators.Select(e => new EnumeratorViewModel(e)).ToList();

            // Progress relays the progress.Report call to the UI thread. i.e. transportsManagerDispatcher.Report() calls TransportUpdated with e.Transport but on the UI thread
            this.transportsManagerDispatcher = new Progress<IAsciiTransport>(this.TransportUpdated);            
            this.transportsManager.TransportChanged += (sender, e) => this.transportsManagerDispatcher.Report(e.Transport);
            
            // If the host we are running the app on has a barcode scanner we can use it to connect to a TSL reader
            // scanning the 2D barcode on the TSL reader should contain a MAC address, we can use the transportManager.BluetoothSecurity to create a transport to the reader
            // with the specified address
            if (this.hostBarcode != null)
            {
                // progress relays the progress.Report call to the UI thread. i.e. hostBarcodeDispatcher.Report() calls BarcodeScanned with the BarcodeEventArgs but on the UI thread
                this.hostBarcodeDispatcher = new Progress<BarcodeEventArgs>(this.UpdateBarcode);
                this.hostBarcode.BarcodeScanned += (sender, e) => 
                {
                    this.hostBarcodeDispatcher.Report(e);
                };
            }

            this.macMatcher = new System.Text.RegularExpressions.Regex(@"([0-9A-Fa-f]{2})(:[0-9A-Fa-f]{2}){5}");

            // Add New Command
            // The transports manager has a number of enumerators, each one enumerating a particular transport type.
            // Typically we expect only one of the enumerators to support AddNew (where a UI can be show to add a new reader to the list)
            // In any case we'll only support showing the UI for the first enumerator that does
            // Pick the first enumerator that supports AddNew (CanShowAddNew == true) and if we have one wire it up to the AddNewCommand (plus top right on UI, assuming default view)
            this.addNewEnumerator = this.transportsManager.Enumerators.Where(enumerator => enumerator.CanShowAddNew).FirstOrDefault();
            this.AddNewCommand = new RelayCommand(() => { this.addNewEnumerator?.ShowAddNew(); }, () => { return this.addNewEnumerator != null; });

            // Refresh transports command
            // Force all the transport enumerators to revisit and return the list of available transports
            this.RefreshTransportsCommand = new RelayCommand(async () => await this.ExecuteRefreshTransportsAsync());

            // Bluetooth Security commands
            this.PairAndConnectCommand = new RelayCommand(async () => await this.ExecutePairAndConnectAsync(), this.CanExecutePairAndConnect);
            this.UnpairAndDisconnectCommand = new RelayCommand(async () => await this.ExecuteUnpairAndDisconnectAsync(), this.CanExecuteUnpairAndDisconnect);

            this.BluetoothAddressText = "88:6B:0F:31:90:83"; // 2128 - 000215
        }

        /// <summary>
        /// Gets the list of enumerators that will enumerate the available <see cref="IAsciiTransport"/>s per <see cref="TransportType"/>
        /// </summary>
        public IEnumerable<EnumeratorViewModel> Enumerators { get; private set; }

        /// <summary>
        /// Gets the list of available <see cref="IAsciiTransport"/>s reported by all the <see cref="IAsciiTransportEnumerator"/>s
        /// </summary>
        public ObservableCollection<TransportViewModel> Transports { get; private set; } = new ObservableCollection<TransportViewModel>();

        /// <summary>
        /// Gets the command that will show a UI to add a new transport
        /// </summary>
        public ICommand AddNewCommand { get; private set; }

        public ICommand RefreshTransportsCommand { get; private set; }

        /// <summary>
        /// Gets or sets the Bluetooth address to pair to
        /// </summary>
        public string BluetoothAddressText
        {
            get => this.bluetoothAddressText;
            set => this.Set(ref this.bluetoothAddressText, value);
        }

        /// <summary>
        /// Backing field for <see cref="BluetoothAddressText"/>
        /// </summary>
        private string bluetoothAddressText;

        public ICommand PairAndConnectCommand { get; private set; }

        public ICommand UnpairAndDisconnectCommand { get; private set; }

        public void Shown()
        {
            this.wasHostBarcodeEnabled = this.hostBarcode?.IsBarcodeScanningEnabled ?? false;
            if (!this.wasHostBarcodeEnabled)
            {
                this.hostBarcode?.EnableBarcodeScanning();
            }

            // ensure all the enumerators are started
            foreach(var enumerator in this.transportsManager.Enumerators )
            {
                if (enumerator.State == EnumerationState.Created)
                {
                    enumerator.Start();
                }
            }
        }

        public void Hidden()
        {
            if (!this.wasHostBarcodeEnabled)
            {
                this.hostBarcode?.DisableBarcodeScanning();
            }
        }

        private void TransportUpdated(IAsciiTransport transport)
        {
            if (transport.State == ConnectionState.Available)
            {
                var viewModel = this.Transports.Where(vm => vm.Id == transport.Id).FirstOrDefault();
                if (viewModel == null)
                {
                    viewModel = new TransportViewModel(this.transportsManager, transport);
                    this.Transports.Add(viewModel);
                }
            }
        }

        private void UpdateBarcode(BarcodeEventArgs e)
        {
            var mac = this.macMatcher.Match(e.Barcode ?? string.Empty);
            if (mac.Success)
            {
                Task.Run(async () => await this.PairBluetoothAsync(mac.Value));
            }
        }

        private async Task PairBluetoothAsync(string mac)
        {
            try
            {
                if (!this.transportsManager.BluetoothSecurity.CanPair )
                {
                    throw new ApplicationException("The Transports Manager does not support Bluetooth pairing");
                }

                await this.transportsManager.BluetoothSecurity.PairAsync(BluetoothAddress.Parse(mac));
            }
            catch (Exception ex)
            {
                this.ReportError(ex);
            }
        }

        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                RaisePropertyChanged(nameof(IsRefreshing));
            }
        }

        private async Task ExecuteRefreshTransportsAsync()
        {
            this.IsRefreshing = true;
            try
            {
                foreach (var enumerator in this.transportsManager.Enumerators)
                {
                    await enumerator.ListAsciiTransportsAsync();
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
            this.IsRefreshing = false;
        }

        private bool CanExecutePairAndConnect()
        {
            // TODO consider validate changes to BluetoothAddressText and enable when valid
            return this.transportsManager.BluetoothSecurity.CanPair;
        }

        private async Task ExecutePairAndConnectAsync()
        {
            try
            {
                BluetoothAddress address = BluetoothAddress.Parse(this.BluetoothAddressText);

                var result = await this.transportsManager.BluetoothSecurity.PairAsync(address);
                if (!result )
                {
                    throw new ApplicationException(string.Format("Failed to pair to {0}", this.BluetoothAddressText));
                }

                var transports = await this.transportsManager.Enumerators
                    .Where(t => t.Physical == PhysicalTransport.Bluetooth).FirstOrDefault()
                    ?.ListAsciiTransportsAsync() ?? new List<IAsciiTransport>();

                var transport = transports.Where(t => t.DisplayInfoLine.Contains(address.ToString())).FirstOrDefault();

                if (transport != null)
                {
                    await transport.ConnectAsync();
                }

            }
            catch(FormatException fe)
            {
                this.ReportError(fe, "ConnectionsView");
            }
            catch (Exception ex)
            {
                this.ReportError(ex, "ConnectionsView");
            }
        }

        private bool CanExecuteUnpairAndDisconnect()
        {
            // TODO consider validate changes to BluetoothAddressText and enable when valid
            return this.transportsManager.BluetoothSecurity.CanUnpair;
        }

        private async Task ExecuteUnpairAndDisconnectAsync()
        {
            try
            {
                BluetoothAddress address = BluetoothAddress.Parse(this.BluetoothAddressText);

                //var transports = await this.model.Enumerators
                //    .Where(t => t.Transport == TransportType.Bluetooth).FirstOrDefault()
                //    ?.ListAsciiTransportsAsync() ?? new List<IAsciiTransport>();

                //var transport = transports.Where(t => t.DisplayInfoLine.Contains(address.ToString())).FirstOrDefault();

                //if (transport != null)
                //{
                //    transport.Disconnect();
                //}

                var result = await this.transportsManager.BluetoothSecurity.UnpairAsync(address);
                if (!result)
                {
                    throw new ApplicationException(string.Format("Failed to unpair to {0}", this.BluetoothAddressText));
                }                
            }
            catch (FormatException fe)
            {
                this.ReportError(fe, "ConnectionsView");
            }
            catch (Exception ex)
            {
                this.ReportError(ex, "ConnectionsView");
            }
        }
    }
}
