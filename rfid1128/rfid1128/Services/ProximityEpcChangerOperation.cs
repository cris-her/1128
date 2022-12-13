using System;
using System.Linq;
using System.Threading.Tasks;
using TechnologySolutions.Rfid;
using TechnologySolutions.Rfid.AsciiProtocol;

namespace rfid1128.Services
{
    /// <summary>
    /// A class to change the EPC of a single tag located by proximity
    /// This class demonstrates the use of the following ReaderOperations:
    ///     <see cref="IReaderOperationTranspondersAccess"/>
    /// </summary>
    public class ProximityEpcChangerOperation : ITagEpcChanger
    {
        /// <summary>
        /// The single target tag must return a signal stronger than this.
        /// This will increase the probability that the detected tag is the closest to the reader
        /// </summary>
        private const int MinimumAcceptableRssi = -50;

        /// <summary>
        /// Used to obtain the reader operation to use
        /// </summary>
        private IReaderManager readerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProximityEpcChanger"/> class
        /// </summary>
        /// <param name="commander">the AsciiCommander to use to command the reader </param>
        public ProximityEpcChangerOperation(IReaderManager readerManager)
        {
            this.readerManager = readerManager;
        }

        /// <summary>
        /// Raised as the Change EPC operation progresses to provide informational messages
        /// </summary>
        public event EventHandler<MessageEventArgs> ProgressUpdate;

        /// <summary>
        /// Gets the identifier of the tag that is to be, or has been, modified
        /// </summary>
        public string TargetTagHexIdentifier { get; set; }

        /// <summary>
        /// Performs a sequence of operations attempting to change the EPC of the single, nearest
        /// tag to the given new identifier
        /// The length of the tag EPC will be adjusted to match the given identifier's length
        /// </summary>
        /// <param name="hexIdentifier">the new EPC identifier as lower-case hex - must be multiple of 4  digits long</param>
        /// <returns>true if the operation succeeded</returns>
        public async Task<bool> ChangeEpcAsync(string hexIdentifier)
        {
            TagMemory newEpc = null;
            this.TargetTagHexIdentifier = null;

            try
            {
                newEpc = TagMemory.Parse(hexIdentifier);
            }
            catch (FormatException fe)
            {
                throw new ArgumentException("Please enter a valid EPC: " + fe.Message, fe);
            }

            if (newEpc.LengthWords == 0)
            {
                // Although writing a zero length EPC is possible we'll stop it for this app
                // to prevent user confusion in Inventory etc..
                throw new ArgumentException("Writing a zero length EPC is not supported in this app");
            }

            if (newEpc.LengthBytes % 2!= 0)
            {
                throw new ArgumentException("Tag memory is 16 bit words an even number of bytes is required");
            }

            this.OnProgressUpdated("Scanning for tag to modify...");

            var operation = this.readerManager.ActiveReader?.OperationOfType<IReaderOperationTranspondersAccess>();
            var outputPower = Math.Min(22, operation.MaximumOutputPower);

            //
            // Scan for tags in range
            //

            // Ensure the reader has default values for any parameter that is not explicitly set below
            // Use lower power to ignore transponders that are distant from the reader
            operation.Filter = TagFilter.All().AtPower(outputPower);

            // Get the tag RSSI to estimate proximity to the reader
            // Get the PC word so that we can alter the EPC identifier length
            operation.Report = TagFields.Default | TagFields.Rssi | TagFields.PC;
            operation.Access = null; // Null access == Inventory

            try
            {
                // TODO: operation.Extensions = "-al off"; // Do the operation silently
                await operation.EnableAsync();
                await operation.StartAsync();
                await operation.StopAsync();
                await operation.DisableAsync();
            }
            catch (Exception ex)
            {
                this.OnProgressUpdated("Failed to find transponders in range of reader");
                this.OnProgressUpdated(ex.Message);
                return false;
            }

            // Ensure that only one tag is in range
            int transpondersCount = operation.Transponders.Count();
            if (transpondersCount != 1)
            {
                if (transpondersCount == 0)
                {
                    this.OnProgressUpdated("No tag found!");
                }
                else
                {
                    this.OnProgressUpdated(string.Format("Multiple tags ({0}) present!", transpondersCount));
                }

                return false;
            }

            // One tag was detected check that the PC word and RSSI are valid
            // and try to ensure that it is close to the reader 
            var targetTag = operation.Transponders.Single();

            this.OnProgressUpdated(string.Format("Tag found: {0}", targetTag.Epc));

            if (!targetTag.Pc.HasValue)
            {
                this.OnProgressUpdated("Unable to read Tag PC word!");
                return false;
            }

            if (!targetTag.Rssi.HasValue)
            {
                this.OnProgressUpdated("Cannot verify that tag is close to the reader!");
                return false;
            }

            if (targetTag.Rssi.Value < MinimumAcceptableRssi)
            {
                this.OnProgressUpdated("Tag is too far from reader!");
                return false;
            }

            // Update the target tag property
            this.TargetTagHexIdentifier = targetTag.Epc;

            //
            // Write the new EPC to the tag's EPC memory bank
            //
            // Set the select parameters
            operation.Filter = TagFilter.ForEpc(targetTag.Epc);

            // Set the data to be written
            // A single tag is being selected so full power can be used
            //if (EpcLengthFromPc(targetTag.Pc.Value) == hexIdentifier.Length / 4)
            if (targetTag.Epc.Length == hexIdentifier.Length)
            {
                // New EPC is same length as the old - just write the new EPC value
                operation.Access = TagAccess.Write(MemoryBank.Epc, TagMemoryOffset.Epc.Words, TagMemory.Parse(hexIdentifier));
            }
            else
            {
                // The PC word will need to be changed as the new length is different
                int pcWord = UpdatePcForLength(targetTag.Pc.Value, hexIdentifier.Length / 4);
                string pcAndIdentifier = string.Format("{0:x4}", pcWord) + hexIdentifier.ToLower();

                // Writing PC and EPC value 
                operation.Access = TagAccess.Write(MemoryBank.Epc, TagMemoryOffset.PC.Words, TagMemory.Parse(pcAndIdentifier));
            }

            this.OnProgressUpdated(string.Empty);
            this.OnProgressUpdated("Changing EPC of tag...");

            // Execute the command
            try
            {
                // TODO: operation.Extensions = "-al off"; // Do the operation silently
                operation.TranspondersReceived += this.Operation_TranspondersReceived;
                await operation.EnableAsync();
                await operation.StartAsync();
                await operation.StopAsync();
                await operation.DisableAsync();
            }
            catch (Exception ex)
            {
                this.OnProgressUpdated("Failed to write new EPC");
                this.OnProgressUpdated(ex.Message);
                return false;
            }
            finally
            {
                operation.TranspondersReceived -= this.Operation_TranspondersReceived;
            }

            if (operation.Transponders.Count() == 1 && operation.Transponders.Single().WordsWritten == operation.Access.Data.LengthWords)
            {
                // Write succeeded
                this.OnProgressUpdated(string.Empty);
                this.OnProgressUpdated("EPC changed - verifying with TID read...");

                // Verify by reading back data from the tag using the new EPC value
                // Read 2 words of data from the start of the TID bank
                // (this includes the Designer Id and the Model Id)
                operation.Filter = TagFilter.ForEpc(hexIdentifier); // target the transponder with the new EPC
                operation.Report = TagFields.Default;
                operation.Access = TagAccess.Read(MemoryBank.Tid, 0, 2);

                this.OnProgressUpdated(string.Empty);
                this.OnProgressUpdated(string.Format("Reading TID from: {0}", hexIdentifier));
                this.OnProgressUpdated(string.Empty);

                try
                {
                    // Process each response as it comes in
                    operation.TranspondersReceived += this.Operation_TranspondersReceived;

                    // Execute the command
                    await operation.EnableAsync();
                    await operation.StartAsync();
                    await operation.StopAsync();
                    await operation.DisableAsync();
                }
                catch(Exception ex)
                {
                    this.OnProgressUpdated("Failed to read the transponder by new EPC");
                    this.OnProgressUpdated(ex.Message);
                }
                finally
                {
                    operation.TranspondersReceived -= this.Operation_TranspondersReceived;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Notify changes to Progress
        /// </summary>
        /// <param name="message">the message included with the notification</param>
        protected virtual void OnProgressUpdated(string message)
        {
            this.ProgressUpdate?.Invoke(this, new MessageEventArgs(message));
        }

        /// <summary>
        /// Report details about each transponder received from the Read command
        /// </summary>
        /// <param name="sender">the source of the event</param>
        /// <param name="e">the transponder data</param>
        private void Operation_TranspondersReceived(object sender, TranspondersEventArgs e)
        {
            foreach (var transponder in e.Transponders)
            {
                this.OnProgressUpdated(string.Format("EPC: {0}", transponder.Epc));

                if (!string.IsNullOrEmpty(transponder.ReadData))
                {
                    if (transponder.ReadData.Length >= 8)
                    {
                        // Always show the full TID data that was read
                        this.OnProgressUpdated(string.Format("TID: {0}", transponder.ReadData));

                        // Extract the Designer & Model Ids when available
                        if (transponder.ReadData.Substring(0, 2).Equals("E2"))
                        {
                            string designerId = transponder.ReadData.Substring(2, 3);
                            string modelId = transponder.ReadData.Substring(5, 3);
                            this.OnProgressUpdated(string.Format("Designer Id: {0}", designerId));
                            this.OnProgressUpdated(string.Format("Model Id: {0}", modelId));
                        }
                    }
                    else
                    {
                        this.OnProgressUpdated(string.Format("ERROR Partial Read: {0}", transponder.ReadData));
                    }
                }
                else if (transponder.WordsWritten != null)
                {
                    var operation = sender as IReaderOperationTranspondersAccess;
                    this.OnProgressUpdated(string.Format("Written: {0} words", transponder.WordsWritten));

                    if (operation.Access.Data.LengthWords != transponder.WordsWritten)
                    {
                        if (transponder.WordsWritten == 0)
                        {
                            this.OnProgressUpdated("Unable to write new EPC!");
                        }
                        else
                        {
                            this.OnProgressUpdated(string.Format("Incomplete EPC written ({0})", transponder.WordsWritten));
                        }
                    }
                }

                if (transponder.TransponderBackscatterErrorCode != null)
                {
                    this.OnProgressUpdated(
                        string.Format(
                            "TAG ERROR: {0} {1}",
                            transponder.TransponderBackscatterErrorCode.Parameter(),
                            transponder.TransponderBackscatterErrorCode.Description()));
                }
                else if (transponder.TransponderAccessErrorCode != null)
                {
                    this.OnProgressUpdated(
                        string.Format(
                            "READER ERROR: {0} {1}",
                            transponder.TransponderAccessErrorCode.Parameter(),
                            transponder.TransponderAccessErrorCode.Description()));
                }

                this.OnProgressUpdated(string.Empty);
            }
        }

        /// <summary>
        /// Gets the new PC word value for the given length
        /// </summary>
        /// <param name="pc">the existing PC word</param>
        /// <param name="length">the new length</param>
        /// <returns>the modified PC word</returns>
        private static int UpdatePcForLength(int pc, int length)
        {
            return (pc & 0x7ff) | length << 11;
        }

        /// <summary>
        /// Gets the EPC identifier length from the given PC word length 
        /// </summary>
        /// <param name="pc">the PC word</param>
        /// <returns>the EPC identifier length </returns>
        private static int EpcLengthFromPc(int pc)
        {
            return (pc & 0xf800) >> 11;
        }
    }
}
