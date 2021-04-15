using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Locations;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Widget;
using Xamarin.Forms;
using xam_peripherals.Droid.Interfaces;
using xam_peripherals.Interfaces;

[assembly: Dependency(typeof(DevicePh))]
namespace xam_peripherals.Droid.Interfaces
{
    public class DevicePh : IDevicePh
    {
        private static readonly int REQUEST_ENABLE = 2;
        private static WifiManager _WifiManager;
        private static ConnectivityManager _ConnectivityManager;
        private static WifiNetworkCallback _NetworkCallback;
        public static string _Status;

        public bool GpsIsEnable()
        {
            try
            {
                var locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
                return locationManager.IsProviderEnabled(LocationManager.GpsProvider);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool BtIsEnable()
        {
            try
            {
                return ((BluetoothManager)Android.App.Application.Context.GetSystemService(Context.BluetoothService)).Adapter.IsEnabled;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<bool> EnableGps()
        {
            try
            {
                var locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
                if (!GpsIsEnable())
                {
                    var listener = new ActivityResultListener();
                    MainActivity.Instance.StartActivityForResult(new Intent(Android.Provider.Settings.ActionLocationSourceSettings), REQUEST_ENABLE);
                    return listener.Task;
                }
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                Android.Util.Log.Error("TEST", "Enable GPS error");
            }

            return Task.FromResult(false);
        }

        public Task<bool> EnableBt()
        {
            try
            {
                if (!BtIsEnable())
                {
                    var listener = new ActivityResultListener();
                    MainActivity.Instance.StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), REQUEST_ENABLE);
                    return listener.Task;
                }
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("TEST", "Enable BT error, ex=" + ex.Message);
            }
            return Task.FromResult(false);
        }

        public void PromptToast(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
        }

        public int NavigationBarHeight => MainActivity.ActionBarHeight;

        public bool ConnectToWifi(string ssid, string password)
        {
            try
            {
                if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Q)
                {
                    _WifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
                    string oldSsid = "empty";
                    string oldBsid = "empty";
                    try
                    {
                        oldSsid = _WifiManager.ConnectionInfo.SSID.Replace("\"", "");
                        oldBsid = _WifiManager.ConnectionInfo.BSSID.Replace("\"", "");
                    }
                    catch { }
                    
                    var formattedSsid = $"\"{ssid}\"";
                    var formattedPassword = $"\"{password}\"";
                    var wifiConfig = new WifiConfiguration
                    {
                        Ssid = formattedSsid,
                        PreSharedKey = formattedPassword
                    };
                    _WifiManager.AddNetwork(wifiConfig);

                    var dis =_WifiManager.Disconnect();
                    var network = _WifiManager.ConfiguredNetworks.FirstOrDefault(n => n.Ssid.Contains(formattedSsid));
                    var ena = _WifiManager.EnableNetwork(network.NetworkId, true);
                    var req = _WifiManager.Reconnect();

                    _Status = "api<29, connecting to " + formattedSsid + " from " + oldSsid + ", dis = " + dis + " ena = " + ena + " req = " + req;
                    Android.Util.Log.Debug("TEST", _Status);
                }
                else
                {

                    NetworkSpecifier specifier = new WifiNetworkSpecifier.Builder()
                        .SetSsidPattern(new PatternMatcher(ssid, Pattern.Prefix))
                        .SetBssidPattern(MacAddress.FromString(password), MacAddress.FromString("ff:ff:ff:ff:ff:ff"))
                        .Build();

                    NetworkRequest request = new NetworkRequest.Builder()
                        .AddTransportType(TransportType.Wifi)
                        .RemoveCapability(NetCapability.Internet)
                        .SetNetworkSpecifier(specifier)
                        .Build();

                    _ConnectivityManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Context.ConnectivityService);
                    _NetworkCallback = new WifiNetworkCallback();
                    _Status = "api>=29, connecting to " + ssid + " mac " + password;
                    Android.Util.Log.Debug("TEST", _Status);
                    _ConnectivityManager.RequestNetwork(request, _NetworkCallback);
                }

                return true;
            }
            catch (Exception ex)
            {
                _Status = "Connect to WiFi error, ex=" + ex.Message;
                Android.Util.Log.Error("TEST", _Status);
                return false;
            }
        }

        public bool DisconnectWifi()
        {
            try
            {
                if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Q)
                {
                    var req = _WifiManager.Reconnect();
                    Android.Util.Log.Debug("TEST", "connection req=" + req);
                }
                else
                {
                    _ConnectivityManager.UnregisterNetworkCallback(_NetworkCallback);
                }
                return true;
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("TEST", "Disconnect WiFi error, ex=" + ex.Message);
                return false;
            }
        }

        public string TestConnection()
        {
            try
            {
                if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Q)
                {
                    var ssid = _WifiManager.ConnectionInfo.SSID.Replace("\"", "");
                    Android.Util.Log.Debug("TEST", "connected to " + ssid);
                }
                else
                {

                }
                return _Status;
            }
            catch (Exception ex)
            {
                _Status = "Test WiFi error, ex=" + ex.Message;
                Android.Util.Log.Error("TEST", _Status);
                return _Status;
            }
        }

        #region Private
        private class ActivityResultListener
        {
            private TaskCompletionSource<bool> Complete = new TaskCompletionSource<bool>();
            public Task<bool> Task { get { return this.Complete.Task; } }

            public ActivityResultListener()
            {
                // subscribe to activity results
                MainActivity.Instance.ActivityResult += OnActivityResult;
            }

            private void OnActivityResult(int requestCode, Result resultCode, Intent data)
            {
                // unsubscribe from activity results
                MainActivity.Instance.ActivityResult -= OnActivityResult;

                // process result
                if (requestCode == REQUEST_ENABLE)
                    this.Complete.TrySetResult(true);
                else
                    this.Complete.TrySetResult(false);
            }
        }
        #endregion Private
    }

    public class WifiNetworkCallback : ConnectivityManager.NetworkCallback
    {
        /// <summary>
        /// Called when the framework connects and has declared a new network ready for use
        /// </summary>
        public override void OnAvailable(Network network)
        {
            base.OnAvailable(network);

            if (network == null)
            {
                DevicePh._Status = "callback onavailable error";
                Android.Util.Log.Error("TEST", DevicePh._Status);
            }
            else
            {
                DevicePh._Status = "callback OnAvailable: " + network.ToString();
                Android.Util.Log.Debug("TEST", DevicePh._Status);
            }
        }
        /// <summary>
        /// Called if no network is found within the timeout time specified or if the requested network request cannot be fulfilled (whether or not a timeout was specified)
        /// </summary>
        public override void OnUnavailable()
        {
            base.OnUnavailable();

            DevicePh._Status = "callback OnUnavailable";
            Android.Util.Log.Debug("TEST", DevicePh._Status);
        }
    }
}