using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xam_peripherals.Interfaces;
using xam_peripherals.Models;

namespace xam_peripherals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BleClientManager : ContentPage, INotifyPropertyChanged
    {
        private static ClientViewModel _ClientViewModel;
        private static byte[] _Data;
        private bool _Check;

        public BleClientManager(BleDevice device)
        {
            _Data = new byte[BleConstants.BLE_MAX_MTU];
            for (byte i = 0; i < BleConstants.BLE_MAX_MTU; ++i)
                _Data[i] = i;

            InitializeComponent();
            _ClientViewModel = new ClientViewModel();
            _ClientViewModel.BleClient = DependencyService.Get<IBleClient>();
            _ClientViewModel.BleClient.Connected = ClientConnected;
            _ClientViewModel.BleDevice = device;
            BindingContext = _ClientViewModel;

            _Check = false;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!_Check)
            {
                _ClientViewModel.BleClient.CloseConnection();
                _ClientViewModel.BleClient.StartConnection(_ClientViewModel.BleDevice.Name, _ClientViewModel.BleDevice.Address, BleConstants.BLE_TIMEOUT_MS, e_BtMode.LowEnergy);
                _Check = true;
            }
            ClientConnected(_ClientViewModel.BleClient.IsConnected);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _ClientViewModel.BleClient.CloseConnection();
        }

        private void SendPacketToServer(object sender, System.EventArgs e)
        {
            if (_ClientViewModel.BleClient.IsConnected)
                _ClientViewModel.BleClient.Transfer(_Data);
            else
                Console.WriteLine("client not connected");
        }

        private void SendSequenceToServer(object sender, System.EventArgs e)
        {
            DisplayAlert("Warning", "Function not yet implemented", "Ok");
        }

        private void ClientConnected(bool connected)
        {
            _ClientViewModel.ClientStatus = connected ? "Ble client connected" : "Ble client disconnected";
            _ClientViewModel.SendPacketEnabled = connected;
            _ClientViewModel.SendSequenceEnabled = connected;
        }
    }

    #region INotifyPropertyChanged
    public class ClientViewModel : INotifyPropertyChanged
    {
        public IBleClient BleClient { get; set; }
        public BleDevice BleDevice { get; set; }

        private bool _ToggleStatus;
        private string _ToggleText;
        private string _ClientStatus;
        private bool _SendPacketEnabled;
        private bool _SendSequenceEnabled;

        public bool ToggleStatus
        {
            get => _ToggleStatus;
            set
            {
                ToggleText = value ? "Bt SOCKET" : "Bt LE";

                if (_ToggleStatus != value)
                {
                    _ToggleStatus = value;
                    BleClient.CloseConnection();
                    BleClient.StartConnection(BleDevice.Name, BleDevice.Address, BleConstants.BLE_TIMEOUT_MS, value ? e_BtMode.Socket : e_BtMode.LowEnergy);
                    OnPropertyChanged("ToggleStatus");
                }
            }
        }

        public string ToggleText
        {
            get => _ToggleText;
            set
            {
                if (_ToggleText != value)
                {
                    _ToggleText = value;
                    OnPropertyChanged("ToggleText");
                }
            }
        }

        public string ClientStatus
        {
            get => _ClientStatus;
            set
            {
                if (_ClientStatus != value)
                {
                    _ClientStatus = value;
                    OnPropertyChanged("ClientStatus");
                }
            }
        }

        public bool SendPacketEnabled
        {
            get => _SendPacketEnabled;
            set
            {
                if (_SendPacketEnabled != value)
                {
                    _SendPacketEnabled = value;
                    OnPropertyChanged("SendPacketEnabled");
                }
            }
        }

        public bool SendSequenceEnabled
        {
            get => _SendSequenceEnabled;
            set
            {
                if (_SendSequenceEnabled != value)
                {
                    _SendSequenceEnabled = value;
                    OnPropertyChanged("SendSequenceEnabled");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    #endregion
}