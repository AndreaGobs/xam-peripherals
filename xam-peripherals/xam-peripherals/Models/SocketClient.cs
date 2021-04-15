using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using xam_peripherals.Interfaces;

namespace xam_peripherals.Models
{
    public class SocketClient
    {
        private static int LOOP_ITERS = 1000; // bit rate = (LOOP_ITERS * data.Length / 8) / t [bit/s]
        private Stopwatch _Timer;

        private ManualResetEvent _ConnectDone;
        private ManualResetEvent _SendDone;
        private ManualResetEvent _ReceiveDone;

        public void StartClient(string serverIp, int serverPort, byte[] data)
        {
            try
            {
                _Timer = new Stopwatch();
                _Timer.Stop();

                _ConnectDone = new ManualResetEvent(false);
                _SendDone = new ManualResetEvent(false);
                _ReceiveDone = new ManualResetEvent(false);

                // Get client
                IPAddress ipa = IPAddress.Parse(SocketUtility.GetLocalIpAddress());
                Socket client = new Socket(ipa.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Get server
                IPAddress serverIpa = IPAddress.Parse(serverIp);
                IPEndPoint serverIpe = new IPEndPoint(serverIpa, serverPort);

                LOG("Starting connection to " + serverIpa.ToString() + ":" + serverPort);

                // Begin connection
                client.BeginConnect(serverIpe, new AsyncCallback(ConnectionCallback), client);
                _ConnectDone.WaitOne();

                LOG("Start sending " + data.Length + " bytes for " + LOOP_ITERS + " loops");
                _Timer.Restart();

                // Sending data
                int i = LOOP_ITERS;
                while (i > 0)
                {
                    client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
                    _SendDone.WaitOne();
                    i--;
                }
                var eof = SocketUtility.GetEncoding.GetBytes(SocketUtility.DEF_EOF);
                client.BeginSend(eof, 0, eof.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
                _SendDone.WaitOne();

                LOG("End sending data, t=" + _Timer.ElapsedMilliseconds + "[ms]");
                LOG("Begin receiving the answer");
                _Timer.Stop();

                // Begin receive data
                SocketState state = new SocketState { Instance = client };
                client.BeginReceive(state.Buffer, 0, SocketState.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                _ReceiveDone.WaitOne();

                // Close client socket
                client.Shutdown(SocketShutdown.Both);
                client.Close();

                LOG("Closing the client");
            }
            catch (Exception ex)
            {
                LOG_ERR("start client error", ex);
            }
        }

        private void ConnectionCallback(IAsyncResult ar)
        {
            try
            {
                LOG("connection callback");

                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                // Signal that the connection has been made
                _ConnectDone.Set();
            }
            catch (Exception ex)
            {
                LOG_ERR("connection callback error", ex);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int dataLen = client.EndSend(ar);
                //LOG("send callback, len=" + dataLen);

                // Signal that the message has been sent
                _SendDone.Set();
            }
            catch (Exception ex)
            {
                LOG_ERR("send callback error", ex);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                SocketState state = (SocketState)ar.AsyncState;
                Socket client = state.Instance;
                int readLen = client.EndReceive(ar);
                LOG("receive callback, len=" + readLen);

                if (readLen > 0)
                {
                    state.Sb.Append(SocketUtility.GetEncoding.GetString(state.Buffer, 0, readLen));
                    client.BeginReceive(state.Buffer, 0, SocketState.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived
                    if (state.Sb.Length > 1)
                        LOG("receive data: " + state.Sb.ToString());

                    // Signal that all bytes have been received
                    _ReceiveDone.Set();
                }
            }
            catch (Exception ex)
            {
                LOG_ERR("receive callback error", ex);
            }
        }

        private static void LOG(string message, string caller = "SOCKETCLI")
        {
            Console.WriteLine("XXX|" + caller + " " + message);
        }

        private static void LOG_ERR(string message, Exception ex = null, string caller = "SOCKETCLI_ERR")
        {
            Console.WriteLine("XXX|" + caller + " " + message + ", EX=" + ex.Message);
        }
    }
}
