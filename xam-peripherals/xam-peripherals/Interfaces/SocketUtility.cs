using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace xam_peripherals.Interfaces
{
    public class SocketUtility
    {
        public const int SERVER_DEF_PORT = 13000;
        public const string PC_LOCAL_IP = "192.168.1.5";
        public const string DEF_EOF = "<EOF>";

        /// <summary>
        /// Get local ip address
        /// </summary>
        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("get local ip error");
        }

        public static byte[] GetPacketData(string data)
        {
            return GetEncoding.GetBytes(data + DEF_EOF);
        }

        public static byte[] GetPacketData(byte[] data)
        {
            var eof = GetEncoding.GetBytes(DEF_EOF);
            var packet = new byte[data.Length + eof.Length];
            Array.Copy(data, 0, packet, 0, data.Length);
            Array.Copy(eof, 0, packet, data.Length, eof.Length);
            return packet;
        }

        public static byte[] GetRandomData(int packetLen)
        {
            byte[] data = new byte[packetLen];
            Random rd = new Random();
            for (int i = 0; i < packetLen; ++i)
                data[i] = (byte)rd.Next(30, 120);
            return data;
        }

        public static Encoding GetEncoding => Encoding.UTF8;
    }
}
