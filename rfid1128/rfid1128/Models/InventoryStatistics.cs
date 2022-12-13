using rfid1128.Infrastructure;

namespace rfid1128.Models
{
    /// <summary>
    /// Container for statistics relating to the inventory passes
    /// </summary>
    public class InventoryStatistics : ObservableObject
    {
        /// <summary>
        /// Backing field for InventoryMode
        /// </summary>
        private string inventoryMode = string.Empty;

        /// <summary>
        /// Backing field for CurrentScanSeenCount
        /// </summary>
        private int currentScanSeenCount;

        /// <summary>
        /// Backing field for CurrentScanUniqueCount
        /// </summary>
        private int currentScanUniqueCount;

        /// <summary>
        /// Backing field for last scan unique count
        /// </summary>
        private int lastScanUniqueCount;

        /// <summary>
        /// Backing field for LastScanSeenCount
        /// </summary>
        private int lastScanSeenCount;

        /// <summary>
        /// Backing field for NumberOfScans
        /// </summary>
        private int numberOfScans;

        /// <summary>
        /// Backing field for TotalSeenCount
        /// </summary>
        private int totalSeenCount;

        /// <summary>
        /// Backing field for TotalUniqueCount
        /// </summary>
        private int totalUniqueCount;

        /// <summary>
        /// Gets or sets the InventoryMode for the inventory
        /// </summary>
        public string InventoryMode
        {
            get
            {
                return this.inventoryMode;
            }

            set
            {
                this.Set(ref this.inventoryMode, value);
            }
        }

        /// <summary>
        /// Gets or sets the total number of transponders seen in this inventory pass
        /// </summary>
        public int CurrentScanSeenCount
        {
            get
            {
                return this.currentScanSeenCount;
            }

            set
            {
                this.Set(ref this.currentScanSeenCount, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of new unique identifiers added in this inventory pass
        /// </summary>
        public int CurrentScanUniqueCount
        {
            get
            {
                return this.currentScanUniqueCount;
            }

            set
            {
                this.Set(ref this.currentScanUniqueCount, value);
            }
        }

        /// <summary>
        /// Gets or sets the total number of transponders since last reset
        /// </summary>
        public int TotalUniqueCount
        {
            get
            {
                return this.totalUniqueCount;
            }

            set
            {
                this.Set(ref this.totalUniqueCount, value);
            }
        }

        /// <summary>
        /// Gets or sets the total number of identifies seen since the statistics were last reset
        /// </summary>
        public int TotalSeenCount
        {
            get
            {
                return this.totalSeenCount;
            }

            set
            {
                this.Set(ref this.totalSeenCount, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of inventory passes completed
        /// </summary>
        public int NumberOfScans
        {
            get
            {
                return this.numberOfScans;
            }

            set
            {
                this.Set(ref this.numberOfScans, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of unique transponders added in the last complete pass
        /// </summary>
        public int LastScanUniqueCount
        {
            get
            {
                return this.lastScanUniqueCount;
            }

            set
            {
                this.Set(ref this.lastScanUniqueCount, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of transponders seen in the last pass
        /// </summary>
        public int LastScanSeenCount
        {
            get
            {
                return this.lastScanSeenCount;
            }

            set
            {
                this.Set(ref this.lastScanSeenCount, value);
            }
        }

        /// <summary>
        /// Clears the current statistics
        /// </summary>
        public void Clear()
        {
            this.CurrentScanSeenCount = 0;
            this.CurrentScanUniqueCount = 0;
            this.InventoryMode = string.Empty;
            this.LastScanSeenCount = 0;
            this.LastScanUniqueCount = 0;
            this.NumberOfScans = 0;
            this.TotalSeenCount = 0;
            this.TotalUniqueCount = 0;
        }

        /// <summary>
        /// Updates the statistics with a partial result
        /// </summary>
        /// <param name="unique">The number of unique transponders seen since the last update</param>
        /// <param name="seen">The total number of transponders seen since the last update</param>
        /// <param name="endPass">True if this completes the inventory; False otherwise. i.e. to reset the Scan counts</param>
        public void Update(int unique, int seen, bool endPass)
        {
            this.CurrentScanSeenCount += seen;
            this.CurrentScanUniqueCount += unique;
            this.TotalSeenCount += seen;
            this.TotalUniqueCount += unique;

            if (endPass)
            {
                this.EndPass();
            }
        }

        /// <summary>
        /// Copies the current scan values to the last scan values.
        /// Resets the current scan values and increments the scan count
        /// </summary>
        public void EndPass()
        {
            this.LastScanSeenCount = this.CurrentScanSeenCount;
            this.LastScanUniqueCount = this.CurrentScanUniqueCount;
            this.CurrentScanSeenCount = 0;
            this.CurrentScanUniqueCount = 0;
            this.NumberOfScans += 1;
        }
    }
}
