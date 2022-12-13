using rfid1128.Infrastructure;
using System;

namespace rfid1128.Models
{
    /// <summary>
    /// Represents a unique item that can be scanned multiple times
    /// </summary>
    public class IdentifiedItem : ViewModelBase
    {
        /// <summary>
        /// The value used when the signal is not specified
        /// </summary>
        public const double NoSignal = -0.0001;

        /// <summary>
        /// Backing field for <see cref="Identifier"/>
        /// </summary>
        private string identifier = string.Empty;

        /// <summary>
        /// Backing field for <see cref="SeenCount"/>
        /// </summary>
        private int seenCount;

        /// <summary>
        /// Backing field for <see cref="FirstSeen"/>
        /// </summary>
        private DateTime firstSeen;

        /// <summary>
        /// Backing field for LastSeen
        /// </summary>
        private DateTime lastSeen;

        /// <summary>
        /// Backing field for Signal
        /// </summary>
        private double normalizedSignal;

        private int channelFrequency;

        private int phase;

        /// <summary>
        /// Backing field for the Source property
        /// </summary>
        private string source;

        /// <summary>
        /// Initializes a new instance of the IdentifiedItem class
        /// </summary>
        /// <param name="identifier">The unique identifier of the item</param>
        /// <remarks><see cref="Seen(DateTime)"/> should be called each time the item is seen, including the first time</remarks>
        public IdentifiedItem(string identifier)
        {
            this.identifier = identifier;
        }

        /// <summary>
        /// Initializes a new instance of the IdentifiedItem class
        /// </summary>
        /// <param name="identifier">The unique identifier of the item</param>
        /// <param name="source"></param>
        /// <remarks><see cref="Seen(DateTime)"/> should be called each time the item is seen, including the first time</remarks>
        public IdentifiedItem(string identifier, string source)
        {
            this.identifier = identifier;
            this.source = source;
        }

        /// <summary>
        /// Gets or sets the identifier for the item
        /// </summary>
        public string Identifier
        {
            get
            {
                return this.identifier;
            }

            set
            {
                this.Set(ref this.identifier, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of times this item has been seen
        /// </summary>
        public int SeenCount
        {
            get
            {
                return this.seenCount;
            }

            set
            {
                this.Set(ref this.seenCount, value);
            }
        }

        /// <summary>
        /// Gets or sets the timestamp when the item was first seen
        /// </summary>
        public DateTime FirstSeen
        {
            get
            {
                return this.firstSeen;
            }

            set
            {
                this.Set(ref this.firstSeen, value);
            }
        }

        /// <summary>
        /// Gets or sets the timestamp when the item was last seen
        /// </summary>
        public DateTime LastSeen
        {
            get
            {
                return this.lastSeen;
            }

            set
            {
                this.Set(ref this.lastSeen, value);
            }
        }

        /// <summary>
        /// Gets or sets the received signal strength indication last time the transponder was seen
        /// </summary>
        public double NormalizedSignal
        {
            get
            {
                return this.normalizedSignal;
            }

            set
            {
                if (value == NoSignal)
                {
                    value = NoSignal;
                }
                else if (value < 0.0)
                {
                    value = 0.0;
                }
                else if (value > 1.0)
                {
                    value = 1.0;
                }

                this.Set(ref this.normalizedSignal, value);
            }
        }

        public int ChannelFrequency
        {
            get
            {
                return this.channelFrequency;
            }

            set
            {
                this.Set(ref this.channelFrequency, value);
            }
        }

        public int Phase
        {
            get
            {
                return this.phase;
            }

            set
            {
                this.Set(ref this.phase, value);
            }
        }

        /// <summary>
        /// Gets or sets a text desription for the source of this item
        /// </summary>
        public string Source
        {
            get
            {
                return this.source;
            }

            set
            {
                this.Set(ref this.source, value);
            }
        }


        ///// <summary>
        ///// Marks the item as seen
        ///// </summary>
        ///// <param name="signal">The RSSI signal seen</param>
        ///// <param name="lastSeen">When the item was seen</param>
        //public void Seen(double signal, DateTime lastSeen)
        //{
        //    this.SeenCount += 1;
        //    this.NormalizedSignal = signal;
        //    this.LastSeen = lastSeen == DateTime.MinValue ? DateTime.Now : lastSeen;
        //}

        /// <summary>
        /// Marks the item as seen
        /// </summary>
        /// <param name="lastSeen">When the item was seen</param>
        public void Seen(DateTime lastSeen)
        {
            this.SeenCount += 1;
            this.LastSeen = lastSeen == DateTime.MinValue ? DateTime.Now : lastSeen;
            if (this.FirstSeen == default(DateTime))
            {
                this.FirstSeen = this.LastSeen;
            }
        }

        /// <summary>
        /// Returns a string representation of this instance
        /// </summary>
        /// <returns>A string representation of the IdentifiedItem</returns>
        public override string ToString()
        {
            if (this.normalizedSignal == NoSignal)
            {
                return string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    "{0} {1}",
                    this.Identifier,
                    this.SeenCount);
            }
            else
            {
                return string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    "{0} {1} {2}%",
                    this.Identifier,
                    this.SeenCount,
                    this.NormalizedSignal * 100);
            }
        }
    }
}
