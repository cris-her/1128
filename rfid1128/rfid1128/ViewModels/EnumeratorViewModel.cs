using rfid1128.Infrastructure;
using System;
using System.Windows.Input;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;
using Xamarin.Forms;

namespace rfid1128.ViewModels
{
    /// <summary>
    /// A visual representation of a <see cref="IAsciiTransportEnumerator"/>
    /// </summary>
    public class EnumeratorViewModel
        : ViewModelBase
    {
        /// <summary>
        /// The model we're representing
        /// </summary>
        private IAsciiTransportEnumerator model;

        /// <summary>
        /// Used to update the view based on changes to the model
        /// </summary>
        private IProgress<IAsciiTransportEnumerator> modelView;

        /// <summary>
        /// Initializes a new instance of the EnumeratorViewModel class
        /// </summary>
        /// <param name="model">The model to represent</param>
        public EnumeratorViewModel(IAsciiTransportEnumerator model)
        {
            this.model = model ?? throw new ArgumentNullException("model");
            this.modelView = new Progress<IAsciiTransportEnumerator>(this.UpdateFromModel);
            this.model.StateChanged += (sender, e) => { this.modelView.Report(this.model); };

            this.State = this.model.State.ToString();
            this.StartEnumeratorCommand = new Command(() => this.ExecuteStartEnumerator());
            this.StopEnumeratorCommand = new Command(() => this.ExecuteStopEnumerator());
        }

        public ICommand StartEnumeratorCommand { get; private set; }

        public ICommand StopEnumeratorCommand { get; private set; }

        /// <summary>
        /// Gets the current state of the enumerator
        /// </summary>
        public string State
        {
            get => this.state;
            private set => this.Set(ref this.state, value);
        }
        private string state;

        /// <summary>
        /// Gets the transport used by the enumerator
        /// </summary>
        public string Transport => this.model.Physical.ToString();

        /// <summary>
        /// Updates the ViewModel as the model changes
        /// </summary>
        /// <param name="enumerator">The model that changed</param>
        private void UpdateFromModel (IAsciiTransportEnumerator enumerator)
        {
            this.State = enumerator.State.ToString();
        }

        private void ExecuteStartEnumerator()
        {
            try
            {
                this.model.Start();
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private void ExecuteStopEnumerator()
        {
            try
            {
                this.model.Stop();
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }        
    }
}
