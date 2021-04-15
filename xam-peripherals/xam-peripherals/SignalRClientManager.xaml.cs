using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xam_peripherals.Interfaces;

namespace xam_peripherals
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SignalRClientManager : ContentPage
    {
        public SignalRClientManager()
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

        private void ConnectToHub(object sender, System.EventArgs e)
        {
            Task.Run(async () =>
            {
                await SignalRClientUtility.Instance.ConnectToHub(BaseUrl.Text, HubName.Text, Email.Text, Password.Text);
            });
        }

        private void SendToHub(object sender, System.EventArgs e)
        {
            Task.Run(async () =>
            {
                await SignalRClientUtility.Instance.SendToHub(MessagePart1.Text, MessagePart2.Text);
            });
        }
    }
}