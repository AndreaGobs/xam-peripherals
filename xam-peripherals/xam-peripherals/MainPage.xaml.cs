using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace xam_peripherals
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void ButtonBleServerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BleServerManager());
        }

        private async void ButtonBleClientClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BleClientScanner());
        }

        private async void ButtonSocketServerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SocketServerManager());
        }

        private async void ButtonSocketClientClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SocketClientManager());
        }

        private async void ButtonSignalRClientClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignalRClientManager());
        }
    }
}
