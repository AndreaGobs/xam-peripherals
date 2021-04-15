using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using xam_peripherals.iOS.Renderer;

[assembly: ExportRenderer(typeof(NavigationPage), typeof(CustomNavigationRenderer))]
namespace xam_peripherals.iOS.Renderer
{
    class CustomNavigationRenderer : NavigationRenderer
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            try
            {
                NavigationBarHeight = (int)NavigationBar.GetFrame().Height + (int)UIApplication.SharedApplication.StatusBarFrame.Height;

                System.Diagnostics.Debug.WriteLine("navigation bar height: " + NavigationBarHeight);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERR| navigation bar height ex: ", ex);
            }
        }

        #region Properties
        public static int NavigationBarHeight { get; private set; }
        #endregion 
    }
}