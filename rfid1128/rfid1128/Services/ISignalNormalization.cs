
namespace rfid1128.Services
{
    /// <summary>
    /// Provides methods to normalize a signal
    /// </summary>
    public interface ISignalNormalization
    {
        /// <summary>
        /// Clear history of previous readings
        /// </summary>
        void Reset();

        /// <summary>
        /// Returns a normalized version of the signal
        /// </summary>
        /// <param name="rssi">The received signal strength</param>
        /// <returns>The normalized signal</returns>
        double Normalize(int? rssi);
    }
}
