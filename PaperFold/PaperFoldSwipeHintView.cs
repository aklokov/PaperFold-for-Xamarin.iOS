using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace PaperFold
{
	public class PaperFoldSwipeHintView : UIView
	{
		public UIImageView ImageView {
			get;
			set;
		}

		public PaperFoldSwipeHintViewMode Mode {
			get;
			set;
		}

		public PaperFoldSwipeHintView (PaperFoldSwipeHintViewMode mode)
		{
			this.Mode = mode;

			UIImage image = null;

			if (this.Mode == PaperFoldSwipeHintViewMode.PaperFoldSwipeHintViewModeSwipeLeft) {
				image = UIImage.FromBundle ("swipe_guide_left.png");
			} else if (this.Mode == PaperFoldSwipeHintViewMode.PaperFoldSwipeHintViewModeSwipeRight) {
				image = UIImage.FromBundle ("swipe_guide_right.png");
			}

			this.ImageView = new UIImageView (new RectangleF(0.0f, 0.0f, image.Size.Width, image.Size.Height));
			this.AddSubview (this.ImageView);
			this.ImageView.Image = image;
			this.UserInteractionEnabled = false;

			this.Tag = 2090;
		}

		public void ShowInView(UIView view)
		{
			if (view.ViewWithTag (2090) != null) {
				view.ViewWithTag (2090).RemoveFromSuperview ();
			}

			this.Frame = view.Frame;

			var imageViewFrame = this.ImageView.Frame;
			imageViewFrame.X = (view.Frame.Width - imageViewFrame.Width) / 2;
			imageViewFrame.Y = (view.Frame.Height - imageViewFrame.Height) / 2;
			this.ImageView.Frame = imageViewFrame;

			view.AddSubview (this);

			this.Alpha = 0.0f;

			UIView.Animate (0.2, () => {
				this.Alpha = 1.0f;
			});

		}

		public void Hide()
		{
			this.Alpha = 1.0f;

			UIView.Animate (0.2, () => {
				this.Alpha = 0.0f;
			},() => {
				this.RemoveFromSuperview();
			});

		}

		public static void HidePaperFoldHintViewInView(UIView view)
		{
			var hintView = (PaperFoldSwipeHintView)view.ViewWithTag (2090);
			if (hintView != null) {
				hintView.Hide ();
			}
		}
	}
}

