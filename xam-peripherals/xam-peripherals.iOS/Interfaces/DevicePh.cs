using System;
using System.Threading.Tasks;
using Foundation;
using NetworkExtension;
using UIKit;
using Xamarin.Forms;
using xam_peripherals.Interfaces;
using xam_peripherals.iOS.Interfaces;
using xam_peripherals.iOS.Renderer;

[assembly: Dependency(typeof(DevicePh))]
namespace xam_peripherals.iOS.Interfaces
{
    public class DevicePh : IDevicePh
    {
        private const double TOAST_DELAY_S = 2.0;

        public bool GpsIsEnable()
        {
            return true;
        }

        public bool BtIsEnable()
        {
            return true;
        }

        public Task<bool> EnableGps()
        {
            return Task.FromResult(true);
        }

        public Task<bool> EnableBt()
        {
            return Task.FromResult(true);
        }

        public void PromptToast(string message)
        {
            ShowAlert(message, TOAST_DELAY_S);
        }

        public bool ConnectToWifi(string ssid, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("connecting to network " + ssid);
                var wifiConfig = new NEHotspotConfiguration(ssid, password, false);
                wifiConfig.JoinOnce = true;

                var wifiManager = new NEHotspotConfigurationManager();
                wifiManager.ApplyConfiguration(wifiConfig, (error) =>
                {
                    if (error != null)
                    {
                        System.Diagnostics.Debug.WriteLine("error while connecting to network " + ssid + ", error: " + error);
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("connection error, ex=" + ex);
                return false;
            }
        }

        public bool DisconnectWifi()
        {
            return false;
        }

        public string TestConnection()
        {
            return null;
        }

        public int NavigationBarHeight => CustomNavigationRenderer.NavigationBarHeight;

        #region Private methods
        private void ShowAlert(string message, double seconds)
        {
            var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);

            var alertDelay = NSTimer.CreateScheduledTimer(seconds, obj =>
            {
                DismissMessage(alert, obj);
            });

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
        }

        private void DismissMessage(UIAlertController alert, NSTimer alertDelay)
        {
            if (alert != null)
            {
                alert.DismissViewController(true, null);
            }

            if (alertDelay != null)
            {
                alertDelay.Dispose();
            }
        }
        #endregion
    }
}