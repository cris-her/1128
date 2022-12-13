
namespace rfid1128.Services
{
    /// <summary>
    /// An implementation of <see cref="ISignalNormalization"/>
    /// </summary>
    public class SignalNormalization
        : ISignalNormalization
    {
        /// <summary>
        /// An expected maximum RSSI signal
        /// </summary>
        private const int DefaultMaximum = -30;

        /// <summary>
        /// An expected minimum RSSI signal
        /// </summary>
        private const int DefaultMinimum = -100;

        /// <summary>
        /// The largest RSSI seen since reset
        /// </summary>
        private int maximumSeen;

        /// <summary>
        /// The smallest RSSI seen since reset
        /// </summary>
        private int minimumSeen;

        /// <summary>
        /// Initializes a new instance of the SignalNormalization class
        /// </summary>
        public SignalNormalization()
        {
            this.Reset();
        }

        /// <summary>
        /// Returns a normalized value based on the range of values currently seen
        /// </summary>
        /// <param name="rssi">The value to normalize</param>
        /// <returns>The normalized signal</returns>
        public double Normalize(int? rssi)
        {
            if (!rssi.HasValue)
            {
                return 0.0;
            }

            if (rssi > this.maximumSeen)
            {
                this.maximumSeen = rssi.Value;
            }
            else if (rssi < this.minimumSeen)
            {
                this.minimumSeen = rssi.Value;
            }

            return (rssi.Value - this.minimumSeen) / (double)(this.maximumSeen - this.minimumSeen);
        }

        /// <summary>
        /// Resets the normalization
        /// </summary>
        public void Reset()
        {
            this.maximumSeen = DefaultMaximum;
            this.minimumSeen = DefaultMinimum;
        }
    }
}
