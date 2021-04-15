using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xam_peripherals.Interfaces;
using xam_peripherals.Models;

namespace xam_peripherals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SocketClientManager : ContentPage
    {
        private static SocketViewModel _SocketViewModel;
        private SocketClient _SocketClient;
        private byte[] _Data;
        private IDevicePh _Device;
        
        public SocketClientManager()
        {
            InitializeComponent();

            _SocketViewModel = new SocketViewModel();
            BindingContext = _SocketViewModel;

            _SocketClient = new SocketClient();
            _Data = SocketUtility.GetRandomData(1000);

            _Device = DependencyService.Get<IDevicePh>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private void SendPacketToServer(object sender, System.EventArgs e)
        {
            _SocketClient.StartClient(SocketUtility.PC_LOCAL_IP, SocketUtility.SERVER_DEF_PORT, _Data);
        }

        private void ConnectToCustonWifi(object sender, System.EventArgs e)
        {
            _Device.ConnectToWifi(WifiSsidEntry.Text, WifiPwEntry.Text);
        }

        private void DisconnectCustonWifi(object sender, System.EventArgs e)
        {
            _Device.DisconnectWifi();
        }

        private void TestCustonWifi(object sender, System.EventArgs e)
        {
            _SocketViewModel.StatusText = _Device.TestConnection();
        }
    }

    #region INotifyPropertyChanged
    public class SocketViewModel : INotifyPropertyChanged
    {
        private string _StatusText;

        public string StatusText
        {
            get => _StatusText;
            set
            {
                if (_StatusText != value)
                {
                    _StatusText = value;
                    OnPropertyChanged("StatusText");
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