using System;
using System.Threading.Tasks;
using TechnologySolutions.Rfid.AsciiProtocol;
using TechnologySolutions.Rfid.AsciiProtocol.Extensions;

namespace rfid1128.Services
{
    public class HostBarcodeMock : IHostBarcodeHandler
    {        
        /// <summary>
        /// Raised when a barcode is scanned
        /// </summary>
        public event EventHandler<BarcodeEventArgs> BarcodeScanned;

        /// <summary>
        /// Gets a value indicating whether barcode scanning is available using the host terminal. True as we're pretending it is
        /// </summary>
        public bool IsAvailable => true;

        /// <summary>
        /// Gets a value indicating whether barcode scanning is enabled
        /// </summary>
        public bool IsBarcodeScanningEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="BarcodeScanned"/> event is expected
        /// </summary>
        /// <remarks>
        /// This returns true once <see cref="InitiateScan"/> has been called and a neither of the <see cref="BarcodeScanned"/> event has been raised
        /// or the scan cancelled with <see cref="CancelScan"/>/>
        /// </remarks>
        public bool IsScanPending { get; private set; }        

        /// <summary>
        /// Gets or sets the barcode to report on the next call to <see cref="InitiateScan"/>. If this is string.IsNullOrEmpty no barcode event is raised
        /// </summary>
        public string NextBarcode { get; set; }

        /// <summary>
        /// Gets or sets the period to wait after <see cref="InitiateScan"/> before <see cref="BarcodeScanned"/> is raised
        /// </summary>
        public TimeSpan BarcodeScanDelay { get; set; } = TimeSpan.FromMilliseconds(750);

        /// <summary>
        /// Cancels a pending scan if <see cref="IsScanPending"/>
        /// </summary>
        /// <returns>True if <see cref="IsScanPending"/> and it was cancelled</returns>
        public bool CancelScan()
        {
            if (this.IsScanPending)
            {
                this.IsScanPending = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Disables barcode scanning from this source
        /// </summary>
        public void DisableBarcodeScanning()
        {
            this.CancelScan();
            this.IsBarcodeScanningEnabled = false;
        }

        /// <summary>
        /// Enables barcode scanning from this source
        /// </summary>
        public void EnableBarcodeScanning()
        {
            this.IsBarcodeScanningEnabled = true;
        }

        /// <summary>
        /// Initiates a barcode scan
        /// </summary>
        /// <returns>True if the barcode scan is initiated</returns>
        public bool InitiateScan()
        {
            if (this.IsBarcodeScanningEnabled && !this.IsScanPending)
            {
                if (!string.IsNullOrEmpty(this.NextBarcode))
                {
                    Task.Run(async () => await this.ScanBarcodeAsync(this.NextBarcode));
                    return true;
                }
            }

            return false;
        }

        private async Task ScanBarcodeAsync(string barcode)
        {
            // TODO: cancel task
            await Task.Delay(this.BarcodeScanDelay);
            this.BarcodeScanned.Invoke(this, new BarcodeEventArgs(barcode, DateTime.Now));
        }
    }
}
