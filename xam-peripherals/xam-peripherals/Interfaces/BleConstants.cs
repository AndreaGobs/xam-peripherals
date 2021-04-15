using System;
using System.Collections.Generic;
using System.Text;

namespace xam_peripherals.Interfaces
{
    public class BleConstants
    {
        public const string BLE_SERVER_SERVICE_UUID = "0a756f73-8a1f-4a62-a2a4-162d03ea720c";
        public const string BLE_SERVER_CH_UUID = "b381a8f8-9314-6e97-1d49-dfc1a166b031";

        public const string BLE_SERVER_SOCKET_UUID = "b444bc82-2ee8-45b1-a4a0-cc0cbba9b2a9";

        public const int BLE_TIMEOUT_MS = 20000;

        public const int BLE_MAX_MTU = 20;
    }
}
