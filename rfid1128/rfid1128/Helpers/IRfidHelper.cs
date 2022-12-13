using rfid1128.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TechnologySolutions.Rfid.AsciiProtocol.Transports;

namespace rfid1128.Helpers
{
    public interface IRfidHelper
    {

        #region Métodos

        /// <summary>
        /// Obtiene el lector de rfid
        /// </summar
        void GetReader();

        /// <summary>
        /// Se configura el lector de rfid
        /// </summar
        void ReaderConfig();

        /// <summary>
        /// Indica si el lector de rfid esta conectado
        /// </summary>
        bool IsConnected(IAsciiTransport model);

        /// <summary>
        /// Indica si el lector de rfid esta leyendo
        /// </summary>
        bool IsReading();

        /// <summary>
        /// Conecta el lector de rfid por default
        /// </summary>
        Task Connect(IAsciiTransport model);

        /// <summary>
        /// Desconecta el lector de rfid
        /// </summary>
        void Disconnect(IAsciiTransport model);

        /// <summary>
        /// Inicia lecturas con el lector de rfid
        /// </summary>
        void Start();

        /// <summary>
        /// Pone en pausa el lector de rfid
        /// </summary>
        void Stop();

        /// <summary>
        /// Se limpiaron etiquetas
        /// </summary>
        void PurgeTags(TransponderInventory transponders);

        void PurgeTags(ObservableCollection<IdentifiedItem> barcodes, ObservableCollection<string> activity);

        #endregion

    }//IRfidHelper
}
