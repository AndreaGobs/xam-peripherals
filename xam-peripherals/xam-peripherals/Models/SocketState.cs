using System.Net.Sockets;
using System.Text;

namespace xam_peripherals.Models
{
    public class SocketState
    {
        /// <summary>
        /// Socket instance
        /// </summary>
        public Socket Instance { get; set; }
        /// <summary>
        /// Data buffer size
        /// </summary>
        public const int BufferSize = 1024;
        /// <summary>
        /// Data buffer
        /// </summary>
        public byte[] Buffer = new byte[BufferSize];
        /// <summary>
        /// Data as string
        /// </summary>
        public StringBuilder Sb = new StringBuilder();
    }
}
