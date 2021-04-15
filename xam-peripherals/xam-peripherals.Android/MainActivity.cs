using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;

namespace xam_peripherals.Droid
{
    [Activity(Label = "xam-peripherals", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance { get; private set; }
        public static int ActionBarHeight { get; private set; }
        public event Action<int, Result, Intent> ActivityResult;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Instance == null)
                Instance = this;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        protected override void OnStart()
        {
            base.OnStart();

            ActionBarHeight = GetActionBarHeight();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (this.ActivityResult != null)
                ActivityResult(requestCode, resultCode, data);
        }

        #region Private methods
        private int GetActionBarHeight()
        {
            try
            {
                var styledAttributes = Theme.ObtainStyledAttributes(new int[] { Android.Resource.Attribute.ActionBarSize });
                var height = (int)(styledAttributes.GetDimension(0, 0) / Resources.DisplayMetrics.Density);
                styledAttributes.Recycle();
                Console.WriteLine("Navigation bar height: " + height);
                return height;
            }
            catch (Exception)
            {
                Console.WriteLine("Navigation bar height: error");
            }
            return 0;
        }
        #endregion
    }
}