using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using xam_peripherals.Interfaces;

namespace xam_peripherals.Functions
{
    public class UIDialog
    {
        private static UIDialog _Instance;
        private UIDialog()
        {
        }

        public static UIDialog Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new UIDialog();
                return _Instance;
            }
        }

        public bool CheckNetworkConnection()
        {
            try
            {
                return Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet;
            }
            catch { }

            return false;
        }

        public async Task<bool> CheckPermissionsStatus()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                return status == Xamarin.Essentials.PermissionStatus.Granted;
            }
            catch { }

            return false;
        }

        public async Task<bool> RequestPermissionsAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != Xamarin.Essentials.PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                return status == Xamarin.Essentials.PermissionStatus.Granted;
            }
            catch { }

            return false;
        }

        public bool RequestBleAndLocation()
        {
            try
            {
                Task.Run(async () =>
                {
                    await CheckStatus();
                    await RequestPermissionsAsync();
                });
                return true;
            }
            catch { }

            return false;
        }

        #region Private methods
        private async Task CheckStatus()
        {
            try
            {
                await CheckBle();
                await CheckGps();
            }
            catch { }
        }

        public async Task CheckBle()
        {
            if (!DependencyService.Get<IDevicePh>().BtIsEnable())
            {
                await Xamarin.Forms.Application.Current?.MainPage?.DisplayAlert("Warning", "Bluetooth required", "OK");
                await DependencyService.Get<IDevicePh>().EnableBt();
            }
        }

        public async Task CheckGps()
        {
            if (!DependencyService.Get<IDevicePh>().GpsIsEnable())
            {
                await Xamarin.Forms.Application.Current?.MainPage?.DisplayAlert("Warning", "Gps required", "OK");
                await DependencyService.Get<IDevicePh>().EnableGps();
            }
        }
        #endregion
    }
}
