using System;
using System.Threading.Tasks;

namespace xam_peripherals.Interfaces
{
    public interface IBleServer
    {
        bool IsAdvertising { get; }
        bool IsConnected { get; }

        Action<bool> Advertising { get; set; }
        Action<bool> Connected { get; set; }

        Task<bool> StartServer(e_BtMode mode);
        bool CloseConnection();
    }
}
