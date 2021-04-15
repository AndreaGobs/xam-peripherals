using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xam_peripherals.Functions;
using xam_peripherals.Interfaces;

namespace xam_peripherals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BleServerManager : ContentPage
    {
        private static ServerViewModel _ServerViewModel;
        private bool _Check;
        private bool _Check1, _Check2, _Check3;

        public BleServerManager()
        {
            InitializeComponent();
            _ServerViewModel = new ServerViewModel();
            _ServerViewModel.BleServer = DependencyService.Get<IBleServer>();
            _ServerViewModel.BleServer.Advertising = ServerAdvertising;
            _ServerViewModel.BleServer.Connected = ServerConnected;
            BindingContext = _ServerViewModel;

            _Check = false;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_Check1)
            {
                await UIDialog.Instance.RequestPermissionsAsync();
                _Check1 = true;
            }
            if (!_Check2)
            {
                await UIDialog.Instance.CheckBle();
                _Check2 = true;
            }
            if (!_Check3)
            {
                await UIDialog.Instance.CheckGps();
                _Check3 = true;
            }

            if (!_Check)
            {
                _ServerViewModel.BleServer.CloseConnection();
                await _ServerViewModel.BleServer.StartServer(e_BtMode.LowEnergy);
                _Check = true;
            }            
            ServerAdvertising(_ServerViewModel.BleServer.IsAdvertising);
            ServerConnected(_ServerViewModel.BleServer.IsConnected);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _ServerViewModel.BleServer.CloseConnection();
        }

        private void ServerAdvertising(bool advertising)
        {
            _ServerViewModel.ServerAdverStatus = advertising ? "Ble server advertising..." : "Ble server not advertising";
        }

        private void ServerConnected(bool connected)
        {
            _ServerViewModel.ServerConnStatus = connected ? "Ble server connected" : "Ble server disconnected";
        }
    }

    #region INotifyPropertyChanged
    public class ServerViewModel : INotifyPropertyChanged
    {
        public IBleServer BleServer { get; set; }

        private bool _ToggleStatus;
        private string _ToggleText;
        private string _ServerAdverStatus;
        private string _ServerConnStatus;

        public bool ToggleStatus
        {
            get => _ToggleStatus;
            set
            {
                ToggleText = value ? "Bt SOCKET" : "Bt LE";

                if (_ToggleStatus != value)
                {
                    _ToggleStatus = value;
                    BleServer.CloseConnection();
                    BleServer.StartServer(value ? e_BtMode.Socket : e_BtMode.LowEnergy);
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

        public string ServerAdverStatus
        {
            get => _ServerAdverStatus;
            set
            {
                if (_ServerAdverStatus != value)
                {
                    _ServerAdverStatus = value;
                    OnPropertyChanged("ServerAdverStatus");
                }
            }
        }

        public string ServerConnStatus
        {
            get => _ServerConnStatus;
            set
            {
                if (_ServerConnStatus != value)
                {
                    _ServerConnStatus = value;
                    OnPropertyChanged("ServerConnStatus");
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