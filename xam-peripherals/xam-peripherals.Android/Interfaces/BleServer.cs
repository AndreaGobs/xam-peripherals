using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using xam_peripherals.Droid.Interfaces;
using xam_peripherals.Interfaces;

[assembly: Dependency(typeof(BleServer))]
namespace xam_peripherals.Droid.Interfaces
{
    public class BleServer : IBleServer
    {
        private BluetoothManager _CentralManager;
        private BluetoothServerSocket _ServerSocket;
        private BluetoothSocket _BluetoothSocket;
        private BluetoothGattServer _GattServer;
        private BleAdvertiseCallback _AdvertiseCallback;
        private BleServerCallback _GattServerCallback;
        private bool _IsAdvertising, _IsConnected;
        private e_BtMode _Mode;

        private bool Init()
        {
            try
            {
                if (_CentralManager == null)
                {
                    _CentralManager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Context.BluetoothService);
                    _AdvertiseCallback = new BleAdvertiseCallback { AdvertiseStarted = ServerAdvertising };
                    _GattServerCallback = new BleServerCallback { ConnectionCompleted = ServerConnected };
                }
                _IsAdvertising = false;
                _IsConnected = false;

                SERVER("init OK");
                return true;
            }
            catch (Exception ex)
            {
                SERVER_ERR("init", ex);
                return false;
            }
        }

        private void ServerAdvertising(bool advertising)
        {
            _IsAdvertising = advertising;
            Advertising?.Invoke(_IsAdvertising);
        }

        private void ServerConnected(bool connected)
        {
            _IsConnected = connected;
            Connected?.Invoke(_IsConnected);
        }

        public bool IsAdvertising => _IsAdvertising;

        public bool IsConnected => _IsConnected;

        public Action<bool> Advertising { get; set; }

        public Action<bool> Connected { get; set; }

        public async Task<bool> StartServer(e_BtMode mode)
        {
            try
            {
                if (!Init())
                    return false;

                if (mode == e_BtMode.LowEnergy)
                {
                    var uuid_ser = Java.Util.UUID.FromString(BleConstants.BLE_SERVER_SERVICE_UUID);
                    var uuid_ch = Java.Util.UUID.FromString(BleConstants.BLE_SERVER_CH_UUID);

                    /// Set advertising
                    AdvertiseSettings settings = new AdvertiseSettings.Builder()
                    .SetConnectable(true)
                    .SetAdvertiseMode(AdvertiseMode.LowLatency)
                    .SetTxPowerLevel(AdvertiseTx.PowerMedium)
                    .Build();

                    AdvertiseData advertiseData = new AdvertiseData.Builder()
                    .SetIncludeDeviceName(true)
                    .SetIncludeTxPowerLevel(true)
                    .Build();

                    AdvertiseData scanResponseData = new AdvertiseData.Builder()
                    //.AddServiceUuid(new ParcelUuid(uuid_ser)) //otherwise AdvertiseFailure for DataTooLarge
                    //.AddServiceData(new ParcelUuid(uuid_ser), new byte[] { 0x01, 0x02 })
                    .SetIncludeDeviceName(true)
                    .SetIncludeTxPowerLevel(true)
                    .Build();

                    _CentralManager.Adapter.BluetoothLeAdvertiser.StartAdvertising(settings, advertiseData, scanResponseData, _AdvertiseCallback);

                    /// Set ble server
                    _GattServer = _CentralManager.OpenGattServer(Android.App.Application.Context, _GattServerCallback);
                    _Mode = mode;

                    var write_ch = new BluetoothGattCharacteristic(uuid_ch, GattProperty.WriteNoResponse, GattPermission.Write);
                    var service = new BluetoothGattService(uuid_ser, GattServiceType.Primary);
                    service.AddCharacteristic(write_ch);
                    _GattServer.AddService(service);
                }
                else if (mode == e_BtMode.Socket)
                {
                    _Mode = mode;
                    _ServerSocket = _CentralManager.Adapter.ListenUsingRfcommWithServiceRecord("server_socket", Java.Util.UUID.FromString(BleConstants.BLE_SERVER_SOCKET_UUID));
                    _BluetoothSocket = await _ServerSocket.AcceptAsync();
                    await Task.Run(ServerSocketManager);
                }
                else
                {
                    SERVER_ERR("start server failed");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                SERVER_ERR("start server error", ex);
                return false;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                if (_Mode == e_BtMode.LowEnergy)
                {
                    SERVER("close server ble");

                    _GattServer?.Close();
                }
                else if (_Mode == e_BtMode.Socket)
                {
                    SERVER("close server socket");

                    _ServerSocket?.Close();
                    _BluetoothSocket?.Close();
                }
                else
                    SERVER("no mode selected");

                return true;
            }
            catch (Exception ex)
            {
                SERVER_ERR("close server error", ex);
                return false;
            }
        }

        private void ServerSocketManager()
        {
            try
            {
                SERVER("start server manager");
                while (_BluetoothSocket != null && _BluetoothSocket.IsConnected && _BluetoothSocket.ConnectionType == BluetoothConnectionType.Rfcomm)
                {
                    try
                    {
                        var inStream = _BluetoothSocket.InputStream;
                        if (inStream.CanRead)
                        {
                            var readArray = new byte[_BluetoothSocket.MaxReceivePacketSize];
                            var data = new Span<byte>(readArray, 0, _BluetoothSocket.MaxReceivePacketSize);
                            var counter = inStream.Read(data);
                            if (counter > 0)
                            {
                                readArray = data.ToArray();

                                string str = "";
                                for (int i = 0; i < readArray.Length; i++)
                                    str += readArray[i] + " ";
                                SERVER("read data[]=" + str);
                            }
                            else
                                SERVER("no data read");
                        }
                        else
                            SERVER("server can't read");
                    }
                    catch (Exception ex)
                    {
                        SERVER_ERR("server read failed", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                SERVER_ERR("server manager error", ex);
            }
        }

        private void SERVER(string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "SERVER|")
        {
            Android.Util.Log.Debug("TEST", "ZZZ|" + caller + " MSG=" + message);
        }
        private void SERVER_ERR(string message, Exception ex = null, [System.Runtime.CompilerServices.CallerMemberName] string caller = "SERVER_ERR|")
        {
            Android.Util.Log.Error("TEST", "ZZZ|" + caller + " MSG=" + message + ", EX=" + ex.Message);
        }
    }
}