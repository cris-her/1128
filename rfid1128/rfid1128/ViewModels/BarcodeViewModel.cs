using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using rfid1128.Helpers;
using rfid1128.Infrastructure;
using rfid1128.Models;
using rfid1128.Services;
using TechnologySolutions.Rfid;
using TechnologySolutions.Rfid.AsciiProtocol.Extensions;

namespace rfid1128.ViewModels
{
    /// <summary>
    /// ViewModel for the inventory view
    /// </summary>
    public class BarcodeViewModel
        : ViewModelBase, ILifecycle
    {
        private readonly IProgress<string> reportState;
        private readonly IProgress<IdentifiedItem> reportBarcode;
        private readonly IReaderManager readerManager;
        private readonly IHostBarcodeHandler hostBarcodeHandler;
        private readonly HostBarcodeMock mockBarcodeHandler;

        private IReaderOperationBarcode operationReaderBarcode;
        private IReaderOperationHostBarcode operationHostBarcode;

        /// <summary>
        /// Initializes a new instance of the InventoryViewModel class
        /// </summary>
        /// <param name="transponders">The unique transponders list</param>
        /// <param name="configurator">Used to update the reader configuration</param>
        /// <param name="configuration">The inventory configuration to apply</param>
        public BarcodeViewModel(IReaderManager readerManager, IHostBarcodeHandler hostBarcodeHandler)
        {
            this.reportBarcode = new Progress<IdentifiedItem>((item) =>
            {
                var identifiedItem = this.Barcodes.Where(t => t.Identifier == item.Identifier && t.Source == item.Source).FirstOrDefault();
                if (identifiedItem == null)
                {
                    identifiedItem = item;
                    this.Barcodes.Add(item);
                }

                identifiedItem.Seen(DateTime.Now);
                this.Activity.Add(String.Format("{0}: {1}", item.Source, item.Identifier));
            });

            this.reportState = new Progress<string>((state) => this.Activity.Add(state));

            this.readerManager = readerManager;
            this.readerManager.ActiveReaderChanged += ReaderManager_ActiveReaderChanged;

            this.hostBarcodeHandler = hostBarcodeHandler;
            this.mockBarcodeHandler = hostBarcodeHandler as HostBarcodeMock;
            if (this.hostBarcodeHandler != null)
            {
                this.hostBarcodeHandler.BarcodeScanned += this.Operation_BarcodeScanned;
            }

            this.ClearCommand = new RelayCommand(() =>
            {
                new RfidHelper().PurgeTags(this.Barcodes, this.Activity);
            });

            //this.Activity.Add("Activity reports:");
        }

        /// <summary>
        /// Gets a list of activity
        /// </summary>
        public ObservableCollection<string> Activity { get; private set; } = new ObservableCollection<string>();

        public bool IsHostBarcodeAvailable
        {
            get => this.isHostBarcodeAvailable;
            set
            {
                if (this.Set(ref this.isHostBarcodeAvailable, value))
                {
                }
            }
        }
        private bool isHostBarcodeAvailable;



        public bool IsReaderBarcodeAvailable
        {
            get => this.isReaderBarcodeAvailable;
            set
            {
                if (this.Set(ref this.isReaderBarcodeAvailable, value))
                {
                }
            }
        }
        private bool isReaderBarcodeAvailable;

        /// <summary>
        /// Gets a value indicating whether the host barcode handler is mock instance
        /// </summary>
        public bool IsHostBarcodeMock => this.mockBarcodeHandler != null;

        #region IsHostBarcodeEnabled
        public bool IsHostBarcodeEnabled
        {
            get => this.isHostBarcodeEnabled;
            set
            {
                if (this.Set (ref this.isHostBarcodeEnabled ,value))
                {
                    this.OnHostBarcodeEnabledChanged();
                }
            }
        }

        private bool isHostBarcodeEnabled;
        #endregion

        #region IsHostBarcodeSecondary
        public bool IsHostBarcodeSecondary
        {
            get => this.isHostBarodeSecondary;
            set => this.Set(ref this.isHostBarodeSecondary, value);
        }

        private bool isHostBarodeSecondary;
        #endregion

        #region IsReaderBarcodeEnabled
        public bool IsReaderBarcodeEnabled
        {
            get => this.isReaderBarcodeEnabled;

            set
            {
                if(this.Set (ref this.isReaderBarcodeEnabled,value))
                {
                    this.OnReaderBarcodeEnabledChanged();
                }
            }
        }

        private bool isReaderBarcodeEnabled;
        #endregion

        #region IsReaderBarcodeSecondary
        public bool IsReaderBarcodeSecondary
        {
            get => this.isReaderBarcodeSecondary;
            set => this.Set(ref this.isReaderBarcodeSecondary, value);
        }

        private bool isReaderBarcodeSecondary;
        #endregion

        #region NextBarcode
        /// <summary>
        /// Get or sets the Next barcode to report if we have a mock host barcode (UWP)
        /// </summary>
        public string NextBarcode
        {
            get => this.nextBarcode;
            set
            {
                if (this.Set(ref this.nextBarcode, value) && this.IsHostBarcodeMock)
                {
                    this.mockBarcodeHandler.NextBarcode = value;
                }
            }
        }

        /// <summary>
        /// Backing field for NextBarcode
        /// </summary>
        private string nextBarcode;
        #endregion


        private async Task UpdateOperationBarcodeAsync(IReaderOperationBarcode value)
        {
            if (this.operationReaderBarcode != value)
            {
                if (this.operationReaderBarcode != null)
                {
                    this.operationReaderBarcode.BarcodeScanned -= this.Operation_BarcodeScanned;
                    this.operationReaderBarcode.StateChanged -= this.Operation_StateChanged;
                    await this.operationReaderBarcode.DisableAsync();
                }

                this.operationReaderBarcode = value;

                if (this.operationReaderBarcode != null)
                {
                    this.operationReaderBarcode.BarcodeScanned += this.Operation_BarcodeScanned;
                    this.operationReaderBarcode.StateChanged += this.Operation_StateChanged;
                    this.OnReaderBarcodeEnabledChanged();
                }
            }
        }

        private async Task UpdateOperationHostBarcodeAsync(IReaderOperationHostBarcode value)
        {
            if (this.operationHostBarcode != value)
            {
                bool enableScanning = true;
                if (this.operationHostBarcode != null)
                {
                    // save the enabled state of the existing host scanner
                    enableScanning = this.operationHostBarcode.HostBarcodeHandler.IsBarcodeScanningEnabled;

                    this.operationHostBarcode.StateChanged -= this.Operation_StateChanged;
                    await this.operationHostBarcode.DisableAsync();
                    this.operationHostBarcode.HostBarcodeHandler = null;
                }

                this.operationHostBarcode = value;

                if (this.operationHostBarcode != null)
                {
                    this.operationHostBarcode.HostBarcodeHandler = this.hostBarcodeHandler;
                    this.operationHostBarcode.StateChanged += this.Operation_StateChanged;
                    // Notify the change of scanner but use the enabled state from the old reader
                    this.OnHostBarcodeEnabledChanged(enableScanning);
                }
            }
        }

        private async void ReaderManager_ActiveReaderChanged(object sender, ReaderEventArgs e)
        {
            if (e.State == ReaderStates.Connected)
            {
                var hostBarcode = this.readerManager.ActiveReader.OperationOfType<IReaderOperationHostBarcode>();
                var barcode = this.readerManager.ActiveReader.OperationOfType<IReaderOperationBarcode>();

                await this.UpdateOperationHostBarcodeAsync(hostBarcode);
                await this.UpdateOperationBarcodeAsync(barcode);

                this.IsHostBarcodeAvailable = this.operationHostBarcode != null;
                this.IsReaderBarcodeAvailable = this.operationReaderBarcode != null;
            }
            else if(e.State == ReaderStates.Disconnecting)
            {
                this.IsHostBarcodeAvailable = false;
                this.IsReaderBarcodeAvailable = false;
            }
        }

        /// <summary>
        /// Gets the barcodes scanned
        /// </summary>
        public ObservableCollection<IdentifiedItem> Barcodes { get; } = new ObservableCollection<IdentifiedItem>();

        /// <summary>
        /// Gets the command to clear the barcodes
        /// </summary>
        public ICommand ClearCommand { get; private set; }


        public void Shown()
        {
            this.hostBarcodeHandler?.EnableBarcodeScanning();
            OnReaderBarcodeEnabledChanged();
        }

        public void Hidden()
        {
            this.operationReaderBarcode?.DisableAsync();
            this.hostBarcodeHandler?.DisableBarcodeScanning();
        }

        private void Operation_BarcodeScanned(object sender, TechnologySolutions.Rfid.AsciiProtocol.BarcodeEventArgs e)
        {
            string source;
            if (sender == this.hostBarcodeHandler)
            {
                source = "Host";
            }
            else if (sender == this.operationReaderBarcode)
            {
                source = "Reader";
            }
            else
            {
                source = "Unknown";
            }

            this.reportBarcode.Report(new IdentifiedItem(e.Barcode, source));
        }

        private void Operation_StateChanged(object sender, ReaderOperationEventArgs e)
        {
            string state;
            if (sender == this.operationHostBarcode)
            {
                state = "Host: " + e.State.ToString();
            }
            else if(sender == this.operationReaderBarcode)
            {
                state = "Reader: " + e.State.ToString();
            }
            else
            {
                state = e.State.ToString();
            }

            this.reportState.Report(state);
        }

        private async void OnHostBarcodeEnabledChanged(bool enableScanning = true)
        {
            IReaderOperation operation = this.operationHostBarcode;
            bool secondary = this.IsHostBarcodeSecondary;

            if (this.operationHostBarcode != null)
            {
                if (this.IsHostBarcodeEnabled)
                {
                    operation.TriggerIndex = secondary ? 2 : 1;
                    if (enableScanning)
                    {
                        this.hostBarcodeHandler?.EnableBarcodeScanning();
                    }
                    else
                    {
                        this.hostBarcodeHandler?.DisableBarcodeScanning();
                    }
                    await operation.EnableAsync();
                }
                else
                {
                    this.hostBarcodeHandler?.DisableBarcodeScanning();
                    await operation.DisableAsync();
                }
            }
        }

        private async void OnReaderBarcodeEnabledChanged()
        {
            IReaderOperation operation = this.operationReaderBarcode;
            bool secondary = this.IsReaderBarcodeSecondary;

            if (operation != null)
            {
                if (this.IsReaderBarcodeEnabled)
                {
                    await operation.DisableAsync();
                    operation.TriggerIndex = secondary ? 2 : 1;
                    await operation.EnableAsync();
                }
                else
                {
                    await operation.DisableAsync();
                }
            }
        }
    }
}
