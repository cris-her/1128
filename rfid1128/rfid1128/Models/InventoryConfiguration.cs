using rfid1128.Infrastructure;

namespace rfid1128.Models
{
    /// <summary>
    /// A representation of the required reader Inventory parameter configuration
    /// </summary>
    public class InventoryConfiguration : ObservableObject
    {
        /// <summary>
        /// Backing field for <see cref="OutputPower"/>
        /// </summary>
        private Tracked<int> outputPower;
        private Tracked<bool> includeChannelFrequency;
        private Tracked<bool> includeChecksum;
        private Tracked<bool> includeDateTime;
        private Tracked<bool> includeEpc;
        private Tracked<bool> includeIndex;
        private Tracked<bool> includePc;
        private Tracked<bool> includePhase;
        private Tracked<bool> includeRssi;

        /// <summary>
        /// Backing field for <see cref="MaximumPower"/>
        /// </summary>
        private int maximumPower;

        /// <summary>
        /// Backing field for <see cref="MinimumPower"/>
        /// </summary>
        private int minimumPower;

        public InventoryConfiguration()
        {
            this.outputPower = new Tracked<int>();
            this.includeChannelFrequency = new Tracked<bool>();
            this.includeChecksum = new Tracked<bool>();
            this.includeDateTime = new Tracked<bool>();
            this.includeEpc = new Tracked<bool>();
            this.includeIndex = new Tracked<bool>();
            this.includePc = new Tracked<bool>();
            this.includePhase = new Tracked<bool>();
            this.includeRssi = new Tracked<bool>();

            // EPC is required for unique transponder count to work
            this.IncludeEpc = true;

            this.MinimumPower = 10;
            this.OutputPower = this.MaximumPower = 29;
        }        

        /// <summary>
        /// Gets or sets a value indicating whether channel frequency at which the transponder was inventoried should be reported
        /// </summary>
        public bool IncludeChannelFrequency
        {
            get
            {
                return this.includeChannelFrequency.value;
            }

            set
            {
                this.Set(ref this.includeChannelFrequency.value, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the checksum of the transponder should be reported
        /// </summary>
        public bool IncludeChecksum
        {
            get
            {
                return this.includeChecksum.value;
            }

            set
            {
                this.Set(ref this.includeChecksum.value, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the timestamp of the inventory should be reported
        /// </summary>
        public bool IncludeDateTime
        {
            get
            {
                return this.includeDateTime.value;
            }

            set
            {
                this.Set(ref this.includeDateTime.value, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the EPC of the transponder should be reported
        /// </summary>
        public bool IncludeEpc
        {
            get
            {
                return this.includeEpc.value;
            }

            set
            {
                this.Set(ref this.includeEpc.value, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the index of the transponder (per inventory) should be reported
        /// </summary>
        public bool IncludeIndex
        {
            get
            {
                return this.includeIndex.value;
            }

            set
            {
                this.Set(ref this.includeIndex.value, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the PC word of the transponder should be reported
        /// </summary>
        public bool IncludePC
        {
            get
            {
                return this.includePc.value;
            }

            set
            {
                this.Set(ref this.includePc.value, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the phase of the transponder should be reported
        /// </summary>
        public bool IncludePhase
        {
            get
            {
                return this.includePhase.value;
            }

            set
            {
                this.Set(ref this.includePhase.value, value);
            }
        }

        /// <summary>        
        /// Gets or sets a value indicating whether the RSSI should be reported
        /// </summary>
        public bool IncludeRssi
        {
            get
            {
                return this.includeRssi.value;
            }

            set
            {
                this.Set(ref this.includeRssi.value, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum output power of the connected reader
        /// </summary>
        public int MaximumPower
        {
            get
            {
                return this.maximumPower;
            }

            set
            {
                this.Set(ref this.maximumPower, (int)value);
            }
        }

        /// <summary>
        /// Gets or sets the minimum output power of the connected reader
        /// </summary>
        public int MinimumPower
        {
            get
            {
                return this.minimumPower;
            }

            set
            {
                this.Set(ref this.minimumPower, (int)value);
            }
        }

        /// <summary>
        /// Gets or sets the antenna output power
        /// </summary>
        public int OutputPower
        {
            get
            {
                return this.outputPower.value;
            }

            set
            {
                this.Set(ref this.outputPower.value, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the configuration values are different since the last update
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return this.outputPower.IsChanged |
                    this.includeChannelFrequency.IsChanged |
                    this.includeChecksum.IsChanged |
                    this.includeDateTime.IsChanged |
                    this.includeEpc.IsChanged |
                    this.includeIndex.IsChanged |
                    this.includePc.IsChanged |
                    this.includePhase.IsChanged |
                    this.includeRssi.IsChanged;
            }
        }

        /// <summary>
        /// Clears <see cref="IsChanged"/> by updating the original values to match the current values
        /// </summary>
        public void UpdateAll()
        {
            this.outputPower.Update();
            this.includeChannelFrequency.Update();
            this.includeChecksum.Update();
            this.includeDateTime.Update();
            this.includeEpc.Update();
            this.includeIndex.Update();
            this.includePc.Update();
            this.includePhase.Update();
            this.includeRssi.Update();
        }

        /// <summary>
        /// Simple class to track the change to a backing field
        /// </summary>
        /// <typeparam name="TValue">The type of the backing field</typeparam>
        private struct Tracked<TValue>
        {
            /// <summary>
            /// Holds the original (or <see cref="Update"/>d) value of the field
            /// </summary>
            private TValue original;

            /// <summary>
            /// Exposes the current value for use with the normal ViewModel Set pattern
            /// </summary>
            public TValue value;

            /// <summary>
            /// Updates the original value to match the current value so the field is no longer changed
            /// </summary>
            public void Update()
            {
                this.original = this.value;
            }

            /// <summary>
            /// Gets a value indicating whether the field value has changed since the last update
            /// </summary>
            public bool IsChanged
            {
                get
                {
                    return !object.Equals(this.original, this.value);
                }
            }
        }
    }
}
