using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace xam_peripherals.Interfaces
{
    public class SignalRClientUtility
    {
        private static SignalRClientUtility _Instance;
        private static HubConnection _Connection;
        private static string _BaseUrl;
        private static string _Hub;

        private SignalRClientUtility()
        {
        }

        public static SignalRClientUtility Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SignalRClientUtility();
                return _Instance;
            }
        }

        public async Task<bool> ConnectToHub(string baseUrl, string hubName, string email = null, string password = null)
        {
            try
            {
                _BaseUrl = baseUrl;
                _Hub = hubName;
                if (_Connection != null)
                {
                    await _Connection.StopAsync();
                    _Connection = null;
                }

                _Connection = new HubConnectionBuilder()
                    .WithUrl($"{_BaseUrl}/{_Hub}")
                    .WithAutomaticReconnect()
                    .Build();

                _Connection.Closed += async (error) =>
                {
                    LOG("connection closed, connectionId=" + _Connection.ConnectionId);
                    await Task.Delay(1000);
                    //await connection.StartAsync();
                };

                _Connection.Reconnecting += async (error) =>
                {
                    LOG("reconnecting, connectionId=" + _Connection.ConnectionId);
                    await Task.Delay(1000);
                };

                _Connection.Reconnected += async (error) =>
                {
                    LOG("connection reopen, connectionId=" + _Connection.ConnectionId);
                    await Task.Delay(1000);
                };

                _Connection.On<string, string>("ReceiveMessageFromServer", (user, message) =>
                {
                    if (string.IsNullOrEmpty(user))
                        user = "---";
                    if (string.IsNullOrEmpty(message))
                        message = "---";
                    LOG("Received message from server. User: " + user + "; Message: " + message);
                });

                LOG("start connecting");
                await _Connection.StartAsync();
                return true;
            }
            catch (Exception ex)
            {
                LOG_ERR("starting connection error", ex);
                return false;
            }
        }

        public async Task<bool> SendToHub(string user, string message)
        {
            try
            {
                LOG("start sending");
                await _Connection.SendAsync("SendMessageToUser", user, message);
                return true;
            }
            catch (Exception ex)
            {
                LOG_ERR("starting connection error", ex);
                return false;
            }
        }

        private static void LOG(string message, string caller = "SIGNALR")
        {
            Console.WriteLine("XXX|" + caller + " " + message);
        }

        private static void LOG_ERR(string message, Exception ex = null, string caller = "SIGNALR_ERR")
        {
            Console.WriteLine("XXX|" + caller + " " + message + ", EX=" + ex.Message);
        }
    }
}