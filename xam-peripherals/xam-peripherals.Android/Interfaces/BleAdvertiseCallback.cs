using System;
using Android.Bluetooth.LE;
using Android.Runtime;

namespace xam_peripherals.Droid.Interfaces
{
    public class BleAdvertiseCallback : AdvertiseCallback
    {
        public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
        {
            base.OnStartSuccess(settingsInEffect);

            AdvertiseStarted?.Invoke(true);

            Android.Util.Log.Debug("TEST", "AAA| start success");
        }

        public override void OnStartFailure([GeneratedEnum] AdvertiseFailure errorCode)
        {
            base.OnStartFailure(errorCode);

            AdvertiseStarted?.Invoke(false);

            Android.Util.Log.Debug("TEST", "AAA| start failure err=" + errorCode.ToString());
        }

        public Action<bool> AdvertiseStarted { private get; set; }
    }
}