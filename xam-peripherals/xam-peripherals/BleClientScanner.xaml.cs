using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xam_peripherals.Functions;
using xam_peripherals.Interfaces;
using xam_peripherals.Models;

namespace xam_peripherals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BleClientScanner : ContentPage
    {
        private static IBleClient _BleClient;
        private static ObservableCollection<BleDevice> _DeviceSource;
        private bool _Check1, _Check2, _Check3;

        public BleClientScanner()
        {
            InitializeComponent();

            _DeviceSource = new ObservableCollection<BleDevice>();
            DeviceList.ItemsSource = _DeviceSource;

            _BleClient = DependencyService.Get<IBleClient>();
            _BleClient.DeviceFound = DeviceFound;
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

            _DeviceSource.Clear();
            if (_BleClient.BleEnabled)
                _BleClient.StartScan(BleConstants.BLE_TIMEOUT_MS, false);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (_BleClient.BleEnabled)
                _BleClient.StopScan();
        }

        private async void DeviceSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is BleDevice device)
                await Navigation.PushAsync(new BleClientManager(device));
        }

        private void DeviceFound(string name, string address)
        {
            var device = new BleDevice { Name = name, Address = address };
            _DeviceSource.Add(device);
            Console.WriteLine("new device=" + name + ", mac=" + address);
        }
    }
}