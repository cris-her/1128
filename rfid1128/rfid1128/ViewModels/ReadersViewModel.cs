using rfid1128.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TechnologySolutions.Rfid.AsciiOperations;

namespace rfid1128.ViewModels
{
    /// <summary>
    /// View model to display the list of available readers
    /// </summary>
    public class ReadersViewModel
        : ViewModelBase
    {
        private readonly IReaderManager readerManager;
        private readonly IProgress<ReaderEventArgs> readerChanged;
        private readonly IProgress<ReaderEventArgs> activeReaderChanged;


        /// <summary>
        /// Initializes a new instance of the ReadersViewModel class
        /// </summary>
        /// <param name="readerManager">The reader manager that reports changes to readers</param>
        public ReadersViewModel (IReaderManager readerManager)
        {
            this.readerManager = readerManager ?? throw new ArgumentNullException("readerManager");

            // Report ReaderChanged events on UI thread to ReaderManager_ReaderChanged
            this.readerChanged = new Progress<ReaderEventArgs>(this.ReaderManager_ReaderChanged);                       
            this.readerManager.ReaderChanged += (sender, e) => { this.readerChanged.Report(e); };

            // Report ActiveReaderChanged events on UI thread to ReaderManager_ActiveReaderChanged
            this.activeReaderChanged = new Progress<ReaderEventArgs>(this.ReaderManager_ActiveReaderChanged);
            this.readerManager.ActiveReaderChanged += (sender, e) => { this.activeReaderChanged.Report(e); };

            this.RefreshReadersCommand = new RelayCommand(this.ExecuteRefreshReaders);
        }

        /// <summary>
        /// Gets the list of available readers
        /// </summary>
        public ObservableCollection<ReaderViewModel> Readers { get; private set; } = new ObservableCollection<ReaderViewModel>();

        #region Selected Reader
        /// <summary>
        /// Gets or sets the selected reader
        /// </summary>
        public ReaderViewModel SelectedReader
        {
            get => this.selectedReader;
            set
            {
                if (this.selectedReader != null)
                {
                    this.selectedReader.IsSelected = false;
                    this.SelectedReader.PropertyChanged -= this.SelectedReader_PropertyChanged;
                }

                this.Set(ref this.selectedReader, value);

                if (this.selectedReader != null)
                {
                    this.selectedReader.IsSelected = true;
                    this.SelectedReader.PropertyChanged += this.SelectedReader_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="SelectedReader"/>
        /// </summary>
        private ReaderViewModel selectedReader;
        #endregion

        public ICommand RefreshReadersCommand { get; private set; }

        private void ReaderManager_ActiveReaderChanged(ReaderEventArgs e)
        {
            if (e.State == ReaderStates.Connecting || e.State == ReaderStates.Connected)
            {
                this.Readers.Where(r => r.Reader == e.Reader).Single().IsActive = true;
                foreach (var reader in this.Readers.Where(r => r.Reader != e.Reader && r.IsActive))
                {
                    reader.IsActive = false;
                }
            }
            else if (e.State == ReaderStates.Disconnecting || e.State == ReaderStates.Disconnected)
            {
                foreach (var reader in this.Readers.Where (r => r.Reader == e.Reader && r.IsActive))
                {
                    reader.IsActive = false;
                }
            }
        }

        private void ReaderManager_ReaderChanged(ReaderEventArgs e)
        {
            var readerModel = this.Readers.Where(r => r.SerialNumber == e.Reader.SerialNumber).FirstOrDefault();
            if (readerModel == null)
            {
                readerModel = new ReaderViewModel(e.Reader);
                this.Readers.Add(readerModel);
            }

            readerModel.ConnectionState = e.State;

            //if (e.State == ReaderStates.Connected)
            //{
            //    readerModel.StartBatteryMonitoring();
            //}
            //else if (e.State == ReaderStates.Disconnecting )
            //{
            //    readerModel.StopBatteryMonitoring();
            //}
        }

        private async void SelectedReader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                if (this.SelectedReader.IsActive )
                {
                    this.readerManager.ActiveReader = this.SelectedReader.Reader;
                }
            }
            else if (e.PropertyName == "ConnectionState")
            {
                if (this.SelectedReader.ConnectionState == ReaderStates.Disconnecting)
                {
                    await this.readerManager.DisconnectReaderAsync(this.SelectedReader.Reader);
                }
            }
        }

        //private bool CanExecuteMakeActive()
        //{
        //    return this.SelectedReader != null &&
        //        this.SelectedReader.ConnectionState == ReaderStates.Connected &&
        //        !this.SelectedReader.IsActive;
        //}

        //private bool CanExecuteDisconnect()
        //{
        //    return this.SelectedReader != null && this.SelectedReader.ConnectionState == ReaderStates.Connected;
        //}

        //private async void ExecuteDisconnect()
        //{
        //    //await (this.SelectedReader.Reader as AsciiReader ).DisconnectAsync();
        //    await this.readerManager.DisconnectReaderAsync(this.SelectedReader.Reader);
        //}

        //private void ExecuteMakeActive()
        //{
        //    this.readerManager.ActiveReader = this.SelectedReader?.Reader?? this.readerManager.ActiveReader;
        //}

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

        private void ExecuteRefreshReaders()
        {
            this.IsRefreshing = true;
            this.Readers.Add(new ReaderViewModel(new AsciiReader()));
            this.IsRefreshing = false;
        }
    }
}
