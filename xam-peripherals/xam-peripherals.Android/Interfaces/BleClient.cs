using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Locations;
using Android.OS;
using Xamarin.Forms;
using xam_peripherals.Droid.Interfaces;
using xam_peripherals.Interfaces;

[assembly: Dependency(typeof(BleClient))]
namespace xam_peripherals.Droid.Interfaces
{
    public class BleClient : ScanCallback, IBleClient
    {
        private BluetoothManager _CentralManager;
        private BleClientCallback _ConnectCallback;
        private BluetoothSocket _BluetoothSocket;
        private List<BluetoothDevice> _Devices;
        private e_BtMode _Mode;

        private Timer _Timeout;
        private bool _IsConnected;
        private SemaphoreSlim _Semaphore;

        private bool Init()
        {
            try
            {
                if (_CentralManager == null)
                {
                    _CentralManager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Context.BluetoothService);
                    _ConnectCallback = new BleClientCallback { ConnectionCompleted = ConnectionCompleted };
                    _Devices = new List<BluetoothDevice>();
                }
                _Timeout?.Change(Timeout.Infinite, Timeout.Infinite);
                _IsConnected = false;
                _Semaphore?.Dispose();
                
                BLE("init OK");
                return true;
            }
            catch (Exception ex)
            {
                BLE_ERR("init", ex);
                return false;
            }
        }

        private void ConnectionCompleted(bool connected)
        {
            _IsConnected = connected;
            Connected?.Invoke(_IsConnected);
            _Semaphore?.Release();
        }

        private void BLE(string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "BLE|")
        {
            Android.Util.Log.Debug("TEST", "XXX|" + caller + " MSG=" + message);
        }
        private void BLE_ERR(string message, Exception ex = null, [System.Runtime.CompilerServices.CallerMemberName] string caller = "BLE_ERR|")
        {
            Android.Util.Log.Error("TEST", "XXX|" + caller + " MSG=" + message + ", EX=" + ex.Message);
        }

        public bool BleEnabled
        {
            get
            {
                try
                {
                    var locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
                    return locationManager.IsProviderEnabled(LocationManager.GpsProvider) &&
                           ((BluetoothManager)Android.App.Application.Context.GetSystemService(Context.BluetoothService)).Adapter.IsEnabled;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool IsConnected => _IsConnected;

        public Action<bool> Connected { get; set; }

        public bool StartScan(int timeoutMs, bool filtered)
        {
            try
            {
                if (!BleEnabled)
                {
                    BLE_ERR("can't start scan, ble not enabled");
                    return false;
                }

                if (!Init())
                    return false;

                _Timeout = new Timer((obj) =>
                {
                    StopScan();
                }, null, timeoutMs, Timeout.Infinite);

                BLE("start scan");

                _Devices.Clear();
                if (filtered)
                {
                    var filter = new ScanFilter.Builder().SetServiceUuid(ParcelUuid.FromString(BleConstants.BLE_SERVER_SERVICE_UUID)).Build();
                    var settings = new ScanSettings.Builder().SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency).Build();
                    _CentralManager.Adapter.BluetoothLeScanner.StartScan(new List<ScanFilter> { filter }, settings, this);
                }
                else
                    _CentralManager.Adapter.BluetoothLeScanner.StartScan(this);

                return true;
            }
            catch (Exception ex)
            {
                BLE_ERR("start scan error", ex);
                return false;
            }
        }

        public bool StopScan()
        {
            try
            {
                BLE("stop scan");

                _CentralManager.Adapter.BluetoothLeScanner.StopScan(this);
                _Timeout?.Change(Timeout.Infinite, Timeout.Infinite);

                return true;
            }
            catch (Exception ex)
            {
                BLE_ERR("stop scan error", ex);
                return false;
            }
        }

        public Action<string, string> DeviceFound { get; set; }

        public async Task<bool> StartConnection(string name, string address, int timeoutMs, e_BtMode mode)
        {
            try
            {
                if (!BleEnabled)
                {
                    BLE_ERR("can't start connection, ble not enabled");
                    return false;
                }

                if (!Init())
                    return false;

                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(address))
                {
                    BLE_ERR("can't start connection, ble device not valid");
                    return false;
                }

                BluetoothDevice device = null;
                if (string.IsNullOrEmpty(address))
                    device = _Devices.Find(x => x.Name == name);
                else
                    device = _Devices.Find(x => x.Address == address);
                if (device == null)
                {
                    BLE_ERR("can't start connection, device not found");
                    return false;
                }

                bool completed = false;
                if (mode == e_BtMode.LowEnergy)
                {
                    BLE("connecting to " + name + " via ble");
                    _Mode = mode;
                    device.ConnectGatt(Android.App.Application.Context, false, _ConnectCallback, BluetoothTransports.Le);

                    _Semaphore = new SemaphoreSlim(0, 1);
                    completed = await _Semaphore.WaitAsync(timeoutMs);
                    _Semaphore = null;
                }
                else if (mode == e_BtMode.Socket)
                {
                    BLE("connecting to " + name + " via socket");
                    _Mode = mode;
                    _BluetoothSocket = device.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString(BleConstants.BLE_SERVER_SOCKET_UUID));
                    await _BluetoothSocket.ConnectAsync();

                    completed = true;
                    _IsConnected = _BluetoothSocket.IsConnected;
                }

                if (completed && _IsConnected)
                    return true;

                BLE_ERR("connection failed");
                return false;
            }
            catch (Exception ex)
            {
                BLE_ERR("start connection error", ex);
                return false;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                if (_Mode == e_BtMode.LowEnergy)
                {
                    BLE("stop connection via ble");

                    _ConnectCallback.CloseConnection();
                    _IsConnected = false;
                    _Semaphore?.Release();
                    _Semaphore?.Dispose();
                }
                else if (_Mode == e_BtMode.Socket)
                {
                    BLE("stop connection via socket");

                    _BluetoothSocket?.Close();
                }
                else
                    BLE("no mode selected");

                return true;
            }
            catch (Exception ex)
            {
                BLE_ERR("stop connection error", ex);
                return false;
            }
        }

        public int Transfer(byte[] data)
        {
            try
            {
                if (_Mode == e_BtMode.LowEnergy)
                {
                    BLE("start transfer via ble");
                    return _ConnectCallback.Transfer(data);
                }
                else if (_Mode == e_BtMode.Socket)
                {
                    if (_BluetoothSocket != null && _BluetoothSocket.IsConnected && _BluetoothSocket.ConnectionType == BluetoothConnectionType.Rfcomm)
                    {
                        var outStream = _BluetoothSocket.OutputStream;
                        if (outStream.CanWrite)
                        {
                            BLE("start transfer via socket, MTPS=" + _BluetoothSocket.MaxTransmitPacketSize);
                            outStream.Write(data, 0, data.Length);
                        }
                        else
                            BLE("can't start transfer");
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                BLE_ERR("start transfer error", ex);
                return 0;
            }
        }

        #region Callback override
        public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            try
            {
                if (!_Devices.Any(x => x.Address.Equals(result.Device.Address)))
                {
                    _Devices.Add(result.Device);
                    DeviceFound?.Invoke(result.Device.Name, result.Device.Address);
                }
            }
            catch (Exception ex)
            {
                BLE_ERR("generic scan result", ex);
            }
        }
        #endregion
    }
}