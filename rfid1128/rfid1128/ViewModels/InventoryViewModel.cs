using System.Collections.ObjectModel;
using System.Windows.Input;
using rfid1128.Helpers;
using rfid1128.Infrastructure;
using rfid1128.Models;
using rfid1128.Services;

namespace rfid1128.ViewModels
{
    /// <summary>
    /// ViewModel for the inventory view
    /// </summary>
    public class InventoryViewModel
        : ViewModelBase, ILifecycle
    {
        /// <summary>
        /// Maintains the list of unique transponders and statistics
        /// </summary>
        private TransponderInventory transponders;

        /// <summary>
        /// Backing field for <see cref="IsReaderConfiguring"/>
        /// </summary>
        private bool isReaderConfiguring;

        /// <summary>
        /// Used to update the inventory configuration
        /// </summary>
        private IInventoryConfigurator configurator;

        /// <summary>
        /// Initializes a new instance of the InventoryViewModel class
        /// </summary>
        /// <param name="transponders">The unique transponders list</param>
        /// <param name="configurator">Used to update the reader configuration</param>
        /// <param name="configuration">The inventory configuration to apply</param>
        public InventoryViewModel(
            TransponderInventory transponders,
            IInventoryConfigurator configurator,
            InventoryConfiguration configuration)
        {
            this.transponders = transponders;
            this.Statistics = this.transponders.Statistics;
            this.Transponders = this.transponders.Identifiers;

            this.ClearCommand = new RelayCommand(() => { new RfidHelper().PurgeTags(this.transponders); });
            this.UpdateCommand = new RelayCommand(this.ExecuteUpdate, this.CanExecuteUpdate);
            //this.FindCommand = new RelayCommand(this.ExecuteFind, this.CanExecuteFind);
            //this.ReadWriteCommand = new RelayCommand (this.ExecuteReadWrite, this.CanExecuteRead)

            this.configurator = configurator;

            this.Configuration = configuration;
            this.Configuration.PropertyChanged += (sender, e) =>
            {
                object propertyValue = sender.GetType().GetProperty(e.PropertyName).GetValue(sender);
                System.Diagnostics.Debug.WriteLine(string.Format("Inventory Configuration Changed {0} = {1}", e.PropertyName, propertyValue));
                // update CanExecute of Update command depending on whether configuration is dirty
                this.UpdateCommand.RefreshCanExecute();
            };

            this.IsReaderConfiguring = false;
        }

        /// <summary>
        /// Gets the configuration for the inventory
        /// </summary>
        public InventoryConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the statistics for the RFID inventories performed
        /// </summary>
        public InventoryStatistics Statistics { get; private set; }

        /// <summary>
        /// Gets the unique transponders scanned
        /// </summary>
        public ObservableCollection<IdentifiedItem> Transponders { get; private set; }

        /// <summary>
        /// Gets the command to clear the barcodes, transponders and statistics
        /// </summary>
        public ICommand ClearCommand { get; private set; }

        /// <summary>
        /// Gets the command to apply the inventory configuration to the connected reader
        /// </summary>
        public ICommand UpdateCommand { get; private set; }      

        ///// <summary>
        ///// Gets the command to find the selected transponder
        ///// </summary>
        //public ICommand FindCommand { get; private set; }

        ///// <summary>
        ///// Gets the command to read write the selected transponder
        ///// </summary>
        //public ICommand ReadWriteCommand { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reader is currently being updated
        /// </summary>
        private bool IsReaderConfiguring
        {
            get
            {
                return this.isReaderConfiguring;
            }

            set
            {
                this.Set(ref this.isReaderConfiguring, value);
                this.UpdateCommand.RefreshCanExecute();
            }
        }

        /// <summary>
        /// Returns a value indicating whether the <see cref="UpdateCommand"/> can execute
        /// </summary>
        /// <returns>True if the configuration is changed and not already applying the change</returns>
        private bool CanExecuteUpdate()
        {
            return !this.IsReaderConfiguring && this.Configuration.IsChanged;
        }

        /// <summary>
        /// Performs the <see cref="ClearCommand"/>
        /// </summary>
        private async void ExecuteUpdate()
        {
            this.IsReaderConfiguring = true;

            await this.configurator.ConfigureAsync();
            this.Configuration.UpdateAll();

            this.IsReaderConfiguring = false;
        }

        public void Shown()
        {
            this.transponders.IsEnabled = true;
            if (firstShow)
            {
                this.Configuration.UpdateAll();
                this.UpdateCommand.RefreshCanExecute();
                firstShow = false;
            }
        }
        private bool firstShow = true;

        public void Hidden()
        {
            this.transponders.IsEnabled = false;
        }
    }
}
