using System;
using TechnologySolutions.Rfid;

namespace rfid1128.Services
{
    public class MonitorTransponders
        : IMonitorTransponders
    {
        private IReaderManager readerManager;
        private bool isEnabled;

        public MonitorTransponders(IReaderManager readerManager)
        {
            this.readerManager = readerManager ?? throw new ArgumentNullException("readerManager");
            this.readerManager.ActiveReaderChanged += ReaderManager_ActiveReaderChanged;
            this.ReaderManager_ActiveReaderChanged(null, null);
        }

        public event EventHandler<TranspondersEventArgs> TranspondersReceived;

        private async void ReaderManager_ActiveReaderChanged(object sender, ReaderEventArgs e)
        {

            var inventoryOperation = this.readerManager.ActiveReader?.OperationOfType<IReaderOperationInventory>();

            if (this.OperationInventory != inventoryOperation)
            {                
                if (this.OperationInventory != null)
                {
                    // disable disconnect the previous operation
                    this.OperationInventory.TranspondersReceived -= this.Operation_TranspondersReceived;
                    await this.OperationInventory.DisableAsync();
                }

                this.OperationInventory = inventoryOperation;

                if (this.OperationInventory != null)
                {
                    // enable connect the new current operation
                    this.OperationInventory.TranspondersReceived += this.Operation_TranspondersReceived;
                    if (this.IsEnabled)
                    {
                        await this.OperationInventory.EnableAsync();
                    }
                }
            }
        }

        private IReaderOperationInventory OperationInventory { get; set; }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled != value)
                {
                    this.isEnabled = value;
                    this.OnEnabledChanged();                    
                }                
            }
        }

        private async void OnEnabledChanged()
        {
            var operationInventory = this.OperationInventory;

            if (operationInventory != null)
            {
                if (this.IsEnabled)
                {
                    await operationInventory?.EnableAsync();
                }
                else
                {
                    await operationInventory?.DisableAsync();
                }
            }
        }

        private void Operation_TranspondersReceived(object sender, TranspondersEventArgs e)
        {
            this.TranspondersReceived?.Invoke(this, e);
        }
    }
}
