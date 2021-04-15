using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using xam_peripherals.Interfaces;

namespace xam_peripherals.Models
{
    public class SocketServer
    {
        private int _Port;

        private static ManualResetEvent _MRE;

        /// <summary>
        /// Socket listener CSTR
        /// </summary>
        /// <param name="ip">Ip address</param>
        /// <param name="port">Port number</param>
        public SocketServer(int port)
        {
            _Port = port;

            _MRE = new ManualResetEvent(false);
        }

        /// <summary>
        /// Start socket listener
        /// </summary>
        public void StartListening()
        {
            try
            {
                LOG("Starting server...");
                var localIp = SocketUtility.PC_LOCAL_IP;

                // Establish the local endpoint for the socket
                IPAddress ipa = IPAddress.Parse(localIp);
                IPEndPoint ipe = new IPEndPoint(ipa, _Port);

                // Create a TCP/IP socket
                Socket listener = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Bind the socket to the local endpoint and listen for incoming connections
                listener.Bind(ipe);
                listener.Listen(5);

                while (true)
                {
                    // Set the event to nonsignaled state
                    _MRE.Reset();

                    // Start an asynchronous socket to listen for connections
                    LOG("waiting for connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing
                    _MRE.WaitOne();
                }
            }
            catch (Exception ex)
            {
                LOG_ERR("start server error", ex);
            }
        }

        /// <summary>
        /// The accept callback method is called when a new connection request is received on the socket
        /// </summary>
        /// <param name="ar">Represents the status of an asynchronous operation</param>
        private static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                LOG("accept callback");

                // Get the socket that handles the client request
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Signal the main thread to continue
                _MRE.Set();

                // Create the state object and begin to receive data
                SocketState state = new SocketState { Instance = handler };
                handler.BeginReceive(state.Buffer, 0, SocketState.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception ex)
            {
                LOG_ERR("accept callback error", ex);
            }
        }

        /// <summary>
        /// This method reads one or more bytes from the client socket into the data buffer, 
        /// then calls the BeginReceive method again until the data sent by the client is complete
        /// </summary>
        private static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                SocketState state = (SocketState)ar.AsyncState;
                Socket handler = state.Instance;

                // Read data from the client socket
                int readLen = handler.EndReceive(ar);
                LOG("read callback, length: " + readLen);

                if (readLen > 0)
                {
                    // There  might be more data, so store the data received so far
                    state.Sb.Append(SocketUtility.GetEncoding.GetString(state.Buffer, 0, readLen));

                    // Check for end-of-file tag
                    string data = state.Sb.ToString();
                    if (data.IndexOf(SocketUtility.DEF_EOF) > -1)
                    {
                        // All the data has been read from the client, answer to it
                        LOG("received: " + state.Sb.ToString());
                        byte[] answer = SocketUtility.GetEncoding.GetBytes("Hi! Received message length: " + state.Sb.Length);
                        handler.BeginSend(answer, 0, answer.Length, SocketFlags.None, new AsyncCallback(SendCallback), handler);
                    }
                    else
                    {
                        // Not all data received
                        handler.BeginReceive(state.Buffer, 0, SocketState.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception ex)
            {
                LOG_ERR("read callback error", ex);
            }
        }

        /// <summary>
        /// Complete to send data answer 
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device
                int answerLen = handler.EndSend(ar);
                LOG("sent message length: " + answerLen);

                // Close connection
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                LOG_ERR("send callback error", ex);
            }
        }

        private static void LOG(string message, string caller = "SOCKETSER")
        {
            Console.WriteLine("XXX|" + caller + " " + message);
        }

        private static void LOG_ERR(string message, Exception ex = null, string caller = "SOCKETSER_ERR")
        {
            Console.WriteLine("XXX|" + caller + " " + message + ", EX=" + ex.Message);
        }
    }
}
