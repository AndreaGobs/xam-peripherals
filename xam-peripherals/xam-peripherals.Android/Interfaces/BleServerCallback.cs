using System;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Runtime;

namespace xam_peripherals.Droid.Interfaces
{
    public class BleServerCallback : BluetoothGattServerCallback
    {
        public override void OnConnectionStateChange(BluetoothDevice device, [GeneratedEnum] ProfileState status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(device, status, newState);

            bool connected = newState == ProfileState.Connected;
            ConnectionCompleted?.Invoke(connected);

            SERVERC("OnConnection device=" + device.Address + " status=" + status.ToString() + " state=" + newState.ToString());
        }

        public override void OnServiceAdded([GeneratedEnum] GattStatus status, BluetoothGattService service)
        {
            base.OnServiceAdded(status, service);

            SERVERC("OnServiceAdded service=" + service.Uuid.ToString() + " status=" + status.ToString());
        }

        public override void OnPhyRead(BluetoothDevice device, [GeneratedEnum] ScanSettingsPhy txPhy, [GeneratedEnum] ScanSettingsPhy rxPhy, [GeneratedEnum] GattStatus status)
        {
            base.OnPhyRead(device, txPhy, rxPhy, status);

            SERVERC("OnPhyRead device=" + device.Address + " status=" + status.ToString());
        }

        public override void OnPhyUpdate(BluetoothDevice device, [GeneratedEnum] ScanSettingsPhy txPhy, [GeneratedEnum] ScanSettingsPhy rxPhy, [GeneratedEnum] GattStatus status)
        {
            base.OnPhyUpdate(device, txPhy, rxPhy, status);

            SERVERC("OnPhyUpdate device=" + device.Address + " status=" + status.ToString());
        }

        public override void OnNotificationSent(BluetoothDevice device, [GeneratedEnum] GattStatus status)
        {
            base.OnNotificationSent(device, status);

            SERVERC("OnNotificationSent device=" + device.Address + " status=" + status.ToString());
        }

        public override void OnDescriptorReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor)
        {
            base.OnDescriptorReadRequest(device, requestId, offset, descriptor);

            SERVERC("OnDescrpRead reqID=" + requestId + " from=" + descriptor.Uuid.ToString());
        }

        public override void OnDescriptorWriteRequest(BluetoothDevice device, int requestId, BluetoothGattDescriptor descriptor, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            base.OnDescriptorWriteRequest(device, requestId, descriptor, preparedWrite, responseNeeded, offset, value);

            SERVERC("OnDescrpWrite reqID=" + requestId + " from=" + descriptor.Uuid.ToString());
        }

        public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicReadRequest(device, requestId, offset, characteristic);

            SERVERC("OnRead reqID=" + requestId + " from=" + characteristic.Uuid.ToString());
        }

        public override void OnCharacteristicWriteRequest(BluetoothDevice device, int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
        {
            base.OnCharacteristicWriteRequest(device, requestId, characteristic, preparedWrite, responseNeeded, offset, value);

            SERVERC("OnWrite reqID=" + requestId + " from=" + characteristic.Uuid.ToString());
        }

        public Action<bool> ConnectionCompleted { private get; set; }

        private void SERVERC(string message, [System.Runtime.CompilerServices.CallerMemberName] string caller = "SERVERC")
        {
            Android.Util.Log.Debug("TEST", "WWW|" + caller + " MSG=" + message);
        }
        private void SERVERC_ERR(string message, Exception ex = null, [System.Runtime.CompilerServices.CallerMemberName] string caller = "SERVERC_ERR")
        {
            Android.Util.Log.Error("TEST", "WWW|" + caller + " MSG=" + message + ", EX=" + ex.Message);
        }
    }
}