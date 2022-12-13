using System;
using TechnologySolutions.Rfid;

namespace rfid1128.Services
{
    /// <summary>
    /// Notifies the progress of a command that returns transponders
    /// </summary>
    public interface IMonitorTransponders
    {
        /// <summary>
        /// Raised for each transponder in the response
        /// </summary>
        event EventHandler<TranspondersEventArgs> TranspondersReceived;

        /// <summary>
        /// Gets or sets a value indicating whether transponders should be reported
        /// </summary>
        bool IsEnabled { get; set; }
    }
}
