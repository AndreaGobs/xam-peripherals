using Android.Bluetooth;
using Android.Runtime;
using System;
using System.Diagnostics;
using xam_peripherals.Interfaces;

namespace xam_peripherals.Droid.Interfaces
{
    public class BleClientCallback : BluetoothGattCallback
    {
        private BluetoothGatt _ServerDevice;
        private BluetoothGattCharacteristic _ServerCh;

        private string _Device;
        private Stopwatch _Timer;

        public BleClientCallback() : base()
        {
            _Timer = new Stopwatch();
            _Timer.Stop();
            _Device = null;
        }

        #region Public interface
        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            try
            {
                if (status == GattStatus.Success)
                {
                    if (newState == ProfileState.Connected && _Device == null)
                    {
                        BLEC("connected to " + gatt.Device.Address + ", status=" + status + ", bond=" + gatt.Device.BondState + ", discovering service...");

                        _ServerDevice = gatt;
                        _Device = gatt.Device.Address;
                        ConnectionCompleted?.Invoke(true);

                        if (!_ServerDevice.DiscoverServices())
                            BLEC_ERR("can't start discovery service");
                    }
                    else if (newState == ProfileState.Connected)
                    {
                        gatt.Close();
                    }
                    else if (newState == ProfileState.Disconnected && _Device != null && gatt.Device.Address == _Device)
                    {
                        _Device = null;
                        ConnectionCompleted?.Invoke(false);
                    }
                }
            }
            catch (Exception ex)
            {
                BLEC_ERR("generic connection state changed", ex);
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            try
            {
                if (status == GattStatus.Success)
                {
                    BLEC("services discovered status=" + status);

                    foreach (var s in gatt.Services)
                        foreach (var c in s.Characteristics)
                        {
                            BLEC("ser=" + s.Uuid.ToString() + ", ch=" + c.Uuid.ToString());

                            if (c.Uuid.ToString().ToUpperInvariant() == BleConstants.BLE_SERVER_CH_UUID.ToUpperInvariant())
                                _ServerCh = c;
                        }
                }
                else
                    BLEC_ERR("services discovered status=" + status);
            }
            catch (Exception ex)
            {
                BLEC_ERR("generic services discovered", ex);
            }
        }

        public override void OnMtuChanged(BluetoothGatt gatt, int mtu, [GeneratedEnum] GattStatus status)
        {
            try
            {
                if (status == GattStatus.Success)
                    BLEC("mtu changed=" + mtu);
                else
                    BLEC_ERR("mtu changed error=" + status);
            }
            catch (Exception ex)
            {
                BLEC_ERR("mtu changed error", ex);
            }
        }

        /// <summary>
        /// Callback returning the result of a characteristic read operation
        /// </summary>
        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            try
            {
                if (status == GattStatus.Success)
                    BLEC("characteristic read=" + status);
                else
                    BLEC_ERR("characteristic read=" + status);
            }
            catch (Exception ex)
            {
                BLEC_ERR("generic characteristic read", ex);
            }
        }

        /// <summary>
        /// Callback indicating the result of a characteristic write operation
        /// </summary>
        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            try
            {
                if (status == GattStatus.Success)
                    BLEC("characteristic write=" + status);
                else
                    BLEC_ERR("characteristic write=" + status);
            }
            catch (Exception ex)
            {
                BLEC_ERR("generic characteristic write", ex);
            }
        }
        #endregion

        public Action<bool> ConnectionCompleted { private get; set; }

        public bool CloseConnection()
        {
            try
            {
                BLEC("close callback");

                if (_ServerDevice != null)
                {
                    _ServerDevice.Disconnect();
                    _ServerDevice.Close();
                    _ServerDevice = null;
                }

                return true;
            }
            catch (Exception ex)
            {
                BLEC_ERR("close callback error", ex);
                return false;
            }
        }

        public int Transfer(byte[] data)
        {
            if (_ServerCh != null && _ServerDevice != null)
            {
                var sv = _ServerCh.SetValue(data);
                if (sv)
                {
                    var wc = _ServerDevice.WriteCharacteristic(_ServerCh);
                    if (wc)
                        return data.Length;
                    else
                        return -2;
                }
                return -1;
            }
            return 0;
        }

        private void BLEC(string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "BLEC")
        {
            Android.Util.Log.Debug("TEST", "XXX|" + caller + " MSG=" + message);
        }
        private void BLEC_ERR(string message, Exception ex = null, [System.Runtime.CompilerServices.CallerMemberName] string caller = "BLEC")
        {
            Android.Util.Log.Error("TEST", "YYY|" + caller + " MSG=" + message + ", EX=" + ex.Message);
        }
    }
}