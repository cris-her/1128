using System;
using System.Threading.Tasks;
using rfid1128.Models;
using TechnologySolutions.Rfid;

namespace rfid1128.Services
{
    public class InventoryConfigurator : IInventoryConfigurator
    {
        private IReaderManager readerManager;

        public InventoryConfigurator(IReaderManager readerManager, InventoryConfiguration configuration)
        {
            this.Configuration = configuration ?? throw new ArgumentNullException("configuration");
            this.readerManager = readerManager ?? throw new ArgumentNullException("readerManager");

            this.readerManager.ActiveReaderChanged += this.ReaderManager_ActiveReaderChanged;

            this.ReaderManager_ActiveReaderChanged(null, null);
        }

        private void ReaderManager_ActiveReaderChanged(object sender, ReaderEventArgs e)
        {
            var inventoryOperation = this.readerManager.ActiveReader?.OperationOfType<IReaderOperationInventory>();
            
            if (this.OperationInventory != inventoryOperation )
            {
                this.OperationInventory = inventoryOperation;
                this.CanConfigure = this.OperationInventory != null;
                if (this.CanConfigure)
                {
                    this.Configuration.OutputPower = this.Configuration.MaximumPower = this.OperationInventory.MaximumOutputPower;
                    this.Configuration.MinimumPower = this.OperationInventory.MinimumOutputPower;
                }

                this.Changed?.Invoke(this, EventArgs.Empty);
            }            
        }

        event EventHandler Changed;

        /// <summary>
        /// Gets a value indicating whether
        /// </summary>
        public bool CanConfigure { get; private set; }

        public IReaderOperationInventory OperationInventory { get; private set; }

        public InventoryConfiguration Configuration { get; }

        public Task ConfigureAsync()
        {
            if (this.OperationInventory != null)
            {
                this.OperationInventory.Filter = ConfigurationToFilter(this.Configuration);
                this.Configuration.UpdateAll();
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private TagFilterFields ConfigurationToFilter(InventoryConfiguration configuration)
        {
            TagFields fields = TagFields.Epc;

            fields |= configuration.IncludeChannelFrequency ? TagFields.Channel : TagFields.None;
            fields |= configuration.IncludeChecksum ? TagFields.Crc : TagFields.None;
            // TODO: not supported? fields |= configuration.IncludeDateTime ? TagFields. : TagFields.None;
            fields |= configuration.IncludeEpc ? TagFields.Epc : TagFields.None;
            fields |= configuration.IncludeIndex ? TagFields.Index : TagFields.None;
            fields |= configuration.IncludePC ? TagFields.PC : TagFields.None;
            fields |= configuration.IncludePhase ? TagFields.Phase : TagFields.None;
            fields |= configuration.IncludeRssi ? TagFields.Rssi : TagFields.None;
            // TODO: not supported? fields |= configuration. ? TagFields.Tid : TagFields.None;

            var filter = TagFilter.All().AtPower(configuration.OutputPower).Report(fields);
            
            return filter;
        }
    }
}
