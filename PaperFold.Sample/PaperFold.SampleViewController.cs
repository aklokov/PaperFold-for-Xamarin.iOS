using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace PaperFold.Sample
{
	public partial class PaperFold_SampleViewController : UIViewController
	{

		private PaperFoldView paperFoldView;
		private UIView redView;
		private UIView content;

		public PaperFold_SampleViewController (IntPtr handle) : base (handle)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		#region View lifecycle
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			paperFoldView = new PaperFoldView (new RectangleF(0.0f, 0.0f, 100.0f, this.View.Bounds.Height));

			this.View.AddSubview (paperFoldView);

			redView = new UIView (new RectangleF(0.0f, 0.0f, 100.0f, this.View.Bounds.Height));
			redView.BackgroundColor = UIColor.Red;

			var bt = UIButton.FromType (UIButtonType.RoundedRect);
			bt.Frame = new RectangleF(0.0f, 0.0f, 100.0f, 100.0f);
			bt.TouchUpInside += (object sender, EventArgs e) => {

				paperFoldView.RestoreToCenter();
			};
			redView.Add (bt);

			paperFoldView.SetLeftFoldContentView (redView, 3, 0.9f);

			content = new UIView (new RectangleF(0.0f, 0.0f, this.View.Bounds.Width, this.View.Bounds.Height));
			content.BackgroundColor = UIColor.Gray;
			var button = UIButton.FromType (UIButtonType.RoundedRect);
			button.Frame = new RectangleF(0.0f, 0.0f, 100.0f, 100.0f);

			button.TouchUpInside += (object sender, EventArgs e) => {

				paperFoldView.UnfoldLeftView();
			};
			content.AddSubview(button);

			paperFoldView.SetCenterContentView (content);

			paperFoldView.EnableHorizontalEdgeDragging = true;
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}
		#endregion
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}
	}
}

