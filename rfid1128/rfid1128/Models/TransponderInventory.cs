using rfid1128.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TechnologySolutions.Rfid;

namespace rfid1128.Models
{ 

    /// <summary>
    /// View model for the inventory of transponders and barcodes
    /// </summary>
    public class TransponderInventory
    {
        /// <summary>
        /// Reports transponders seen to the model
        /// </summary>
        private IMonitorTransponders monitorTransponders;

        /// <summary>
        /// Cache used to identify unique transponders
        /// </summary>
        private readonly IDictionary<string, IdentifiedItem> transponders;

        /// <summary>
        /// Used to invoke actions on the user interface thread
        /// </summary>
        private IProgress<TranspondersEventArgs> dispatcher;

        /// <summary>
        /// Used to normalize the RSSI signal
        /// </summary>
        private ISignalNormalization signal;

        /// <summary>
        /// Initializes a new instance of the TransponderInventory class
        /// </summary>
        /// <param name="statistics">The model to update with inventory statistics</param>
        /// <param name="monitorTransponders">Reports transponders inventory</param>
        /// <param name="signal">Used to normalize RSSI values</param>
        public TransponderInventory(InventoryStatistics statistics, IMonitorTransponders monitorTransponders, ISignalNormalization signal)
        {
            this.monitorTransponders = monitorTransponders;

            this.dispatcher = new Progress<TranspondersEventArgs>(this.AddTransponders);
            this.signal = signal;
            this.Statistics = statistics;
            this.Identifiers = new ObservableCollection<IdentifiedItem>();
            this.transponders = new Dictionary<string, IdentifiedItem>();
            //this.IsEnabled = true;

            monitorTransponders.TranspondersReceived += (sender, e) =>
            {
                if (this.IsEnabled)
                {
                    this.dispatcher.Report(e);
                }
            };
        }

        /// <summary>
        /// Gets or sets a value indicating whether to append received transponders to the Identifiers collection
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return this.monitorTransponders.IsEnabled;
            }

            set
            {
                this.monitorTransponders.IsEnabled = value;
            }
        }

        /// <summary>
        /// Gets the statistics for the transponders scanned
        /// </summary>
        public InventoryStatistics Statistics { get; private set; }

        /// <summary>
        /// Gets the transponders scanned
        /// </summary>
        public ObservableCollection<IdentifiedItem> Identifiers { get; private set; }

        /// <summary>
        /// Adds the transponders to the displayed list updating the statistics
        /// </summary>
        /// <param name="transponders">The transponders to add</param>
        /// <param name="endPass">True if this update is the last for this inventory pass</param>
        //public void AddTransponders(IEnumerable<TechnologySolutions.Rfid.AsciiProtocol.TransponderData> transponders, bool endPass)
        public void AddTransponders(TranspondersEventArgs transpondersChunk)
        {
            int unique;
            int seen;

            seen = 0;
            unique = 0;

            foreach (var transponder in transpondersChunk.Transponders)
            {
                var rssi = transponder.Rssi.HasValue ? this.signal.Normalize(transponder.Rssi.Value) : IdentifiedItem.NoSignal;
                var channel = transponder.ChannelFrequency ?? 0;
                var phase = transponder.Phase ?? 0;

                seen += 1;

                IdentifiedItem item = null;
                if (!this.transponders.ContainsKey(transponder.Epc))
                {
                    item = new IdentifiedItem(transponder.Epc);
                    unique += 1;
                    this.transponders.Add(transponder.Epc, item);
                    this.Identifiers.Add(item);
                }
                else
                {
                    item = this.transponders[transponder.Epc];
                }

                item.Seen(transponder.Timestamp);
                item.NormalizedSignal = rssi;
                item.ChannelFrequency = channel;
                item.Phase = phase;
            }

            this.Statistics.Update(unique, seen, transpondersChunk.EndOfPass);
        }

        /// <summary>
        /// Reset all of the statistics
        /// </summary>
        public void Clear()
        {
            this.Identifiers.Clear();
            this.Statistics.Clear();
            this.transponders.Clear();
            this.signal.Reset();
        }
    }
}
