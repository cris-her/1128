using rfid1128.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;

namespace rfid1128.Helpers
{
    public class RfidHelper : IRfidHelper
    {
        public async Task Connect(IAsciiTransport model)
        {
            await model.ConnectAsync();

            //this.model.Connection.Received += this.Connection_Received;
            await Task.Run(async () =>
            {
                model.Connection.WriteLine(".vr");
                for (int i = 0; i < 10; i++)
                {
                    if (model.Connection?.IsLineAvailable ?? false)
                    {
                        break;
                    }

                    await Task.Delay(100);
                }

                while (model.Connection?.IsLineAvailable ?? false)
                {
                    System.Diagnostics.Debug.WriteLine(model.Connection.ReadLine());
                }
            });
        }

        public void Disconnect(IAsciiTransport model)
        {
            model.Disconnect();
        }

        public void GetReader()
        {
            throw new NotImplementedException();
        }

        public bool IsReading()
        {
            throw new NotImplementedException();
        }

        public void ReaderConfig()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected(IAsciiTransport model)
        {
            return model.State.ToString() == "Connected";
        }

        public void PurgeTags(TransponderInventory transponders)
        {
            transponders.Clear();
        }

        public void PurgeTags(ObservableCollection<IdentifiedItem> barcodes, ObservableCollection<string> activity)
        {
            barcodes.Clear();
            activity.Clear();
        }
    }
}
