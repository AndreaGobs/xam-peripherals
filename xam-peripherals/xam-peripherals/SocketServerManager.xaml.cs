using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xam_peripherals.Interfaces;
using xam_peripherals.Models;

namespace xam_peripherals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SocketServerManager : ContentPage
    {
        private SocketServer _SocketServer;
        public SocketServerManager()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private void StartServer(object sender, System.EventArgs e)
        {
            _SocketServer = new SocketServer(SocketUtility.SERVER_DEF_PORT);
            _SocketServer.StartListening();
        }
    }
}