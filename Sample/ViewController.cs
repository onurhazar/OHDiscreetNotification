using System;
using CoreGraphics;
using OHDiscreetNotification;
using UIKit;

namespace Sample
{
    public partial class ViewController : UIViewController
    {
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.View.BackgroundColor = UIColor.Orange;

            var discreetNotification = new OHDiscreetNotificationView("Synchronizing..", false, OHDNPresentationMode.Top, this.View);
            discreetNotification.Show(true);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}
