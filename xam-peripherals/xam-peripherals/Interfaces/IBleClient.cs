using System;
using System.Threading.Tasks;

namespace xam_peripherals.Interfaces
{
    public enum e_BtMode : byte
    {
        None,
        LowEnergy,
        Socket,
    }

    public interface IBleClient
    {
        /// Status
        bool BleEnabled { get; }
        bool IsConnected { get; }
        Action<bool> Connected { get; set; }

        /// Scan
        bool StartScan(int timeoutMs, bool filtered);
        bool StopScan();
        Action<string, string> DeviceFound { get; set; }

        /// Connect
        Task<bool> StartConnection(string name, string address, int timeoutMs, e_BtMode mode);
        bool CloseConnection();

        /// Transfer data
        int Transfer(byte[] data);
    }
}
