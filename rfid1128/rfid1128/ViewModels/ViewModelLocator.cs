using rfid1128.Infrastructure;
using TechnologySolutions.Rfid.AsciiProtocol.Extensions;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;

namespace rfid1128.ViewModels
{

    /// <summary>
    /// Provides instances of ViewModels for the Views
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// The IOC container
        /// </summary>
        private readonly Locator locator = Locator.Default;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class
        /// </summary>
        /// <remarks>
        /// Registers all the required Types with the locator so it can return instances of the ViewModels
        /// </remarks>
        public ViewModelLocator()
        {
            var locator = this.locator;
            
            locator.Register<IAsciiTransportsManager, AsciiTransportsManager>();

            if (!locator.IsRegistered<IHostBarcodeHandler>())
            {
                locator.Register<IHostBarcodeHandler, HostBarcodeHandler>();
            }

            locator.Register<TransportsViewModel>();

            locator.Register<IReaderManager, TslReaderManager>();

            locator.Register<Models.InventoryStatistics>(); // none
            locator.Register<Services.IMonitorTransponders, Services.MonitorTransponders>(); // IReaderOperationInventory
            locator.Register<Services.ISignalNormalization, Services.SignalNormalization>(); // none
            locator.Register<Models.TransponderInventory>(); // InventoryStatistics, IMonitorTransponders, ISignalNormalization
            locator.Register<Services.IInventoryConfigurator, Services.InventoryConfigurator>(); // IReaderOperationInventory, InventoryConfiguration
            locator.Register<Models.InventoryConfiguration>(); // none
            locator.Register<InventoryViewModel>(); //  TransponderInventory, IInventoryConfigurator, InventoryConfiguration            

            locator.Register<Services.TagReaderWriterOperation>();
            locator.Register<Services.ITagReader>(() => locator.Locate<Services.TagReaderWriterOperation>());
            locator.Register<Services.ITagWriter>(() => locator.Locate<Services.TagReaderWriterOperation>());
            locator.Register<Services.ProximityEpcChangerOperation>();
            locator.Register<Services.ITagEpcChanger>(() => locator.Locate<Services.ProximityEpcChangerOperation>());

            locator.Register<ReadersViewModel>();
            locator.Register<BarcodeViewModel>();
            //locator.Register<FindTagViewModel>();
            //locator.Register<ReadWriteViewModel>(); // ITagReader, ITagWriter
            //locator.Register<ChangeEpcViewModel>(); // ITagEpcChanger
        }

        /// <summary>
        /// Gets the <see cref="BarcodeViewModel"/>
        /// </summary>
        public BarcodeViewModel Barcode => this.locator.Locate<BarcodeViewModel>();

        /// <summary>
        /// Gets the <see cref="ChangeEpcViewModel"/>
        /// </summary>
        //public ChangeEpcViewModel ChangeEpc => this.locator.Locate<ChangeEpcViewModel>();

        /// <summary>
        /// Gets the <see cref="FindTagViewModel"/>
        /// </summary>
        //public FindTagViewModel FindTag => this.locator.Locate<FindTagViewModel>();

        /// <summary>
        /// Gets the <see cref="InventoryViewModel"/>
        /// </summary>
        public InventoryViewModel Inventory => this.locator.Locate<InventoryViewModel>();

        /// <summary>
        /// Gets the <see cref="ReadersViewModel"/>
        /// </summary>
        public ReadersViewModel Readers => this.locator.Locate<ReadersViewModel>();

        /// <summary>
        /// Gets the <see cref="ReaderWriteViewModel"/>
        /// </summary>
        //public ReadWriteViewModel ReadWrite => this.locator.Locate<ReadWriteViewModel>();

        /// <summary>
        /// Gets the <see cref="TransportsViewModel"/>
        /// </summary>
        public TransportsViewModel Transports => this.locator.Locate<TransportsViewModel>();
 
        /// <summary>
        /// Returns the instance of the specified ViewModel
        /// </summary>
        /// <typeparam name="TViewModel">The type of ViewModel required</typeparam>
        /// <returns>The ViewModel instance</returns>
        public TViewModel Locate<TViewModel>()
        {
            return this.locator.Locate<TViewModel>();
        }
    }
}
