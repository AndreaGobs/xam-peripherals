using System.Threading.Tasks;

namespace xam_peripherals.Interfaces
{
    public interface IDevicePh
    {
        /// <summary>
        /// Return true if Localization enable
        /// </summary>
        bool GpsIsEnable();
        /// <summary>
        /// Return true if Bluetooth enable
        /// </summary>
        bool BtIsEnable();
        /// <summary>
        /// If disable, turn on Localization and return the new status (not necessary in iOS)
        /// </summary>
        Task<bool> EnableGps();
        /// <summary>
        /// If disable, turn on Bluetooth and return the new status (not necessary in iOS)
        /// </summary>
        Task<bool> EnableBt();
        /// <summary>
        /// Call native toast on Android; implement our toast on iOS
        /// </summary>
        void PromptToast(string message);
        /// <summary>
        /// Return the navigation bar height
        /// </summary>
        int NavigationBarHeight { get; }

        bool ConnectToWifi(string ssid, string password);
        bool DisconnectWifi();
        string TestConnection();
    }
}
