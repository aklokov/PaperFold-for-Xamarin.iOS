using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;

namespace PaperFold
{
	public class FoldView : UIView
	{
		public FacingView RightView {
			get;
			set;
		}

		public FacingView LeftView {
			get;
			set;
		}

		public FacingView BottomView {
			get;
			set;
		}

		public FacingView TopView {
			get;
			set;
		}

		public FoldState State {
			get;
			set;
		}

		public UIView ContentView {
			get;
			set;
		}

		public FoldDirection FoldDirection {
			get;
			set;
		}

		public bool UseOptimizedScreenshot {
			get;
			set;
		}

		public FoldView (RectangleF frame)
			:this(frame, FoldDirection.FoldDirectionHorizontalRightToLeft)
		{
			
		}

		public FoldView (RectangleF frame, FoldDirection foldDirection)
			:base(frame)
		{
			this.UseOptimizedScreenshot = true;
			this.FoldDirection = foldDirection;

			this.ContentView = new UIView (new RectangleF(0.0f, 0.0f, this.Frame.Width, this.Frame.Height));
			this.ContentView.BackgroundColor = UIColor.Clear;

			this.AddSubview (this.ContentView);

			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight || this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft) {

				this.LeftView = new FacingView (new RectangleF(-1 * frame.Width / 4, 0.0f, frame.Width / 2, frame.Height));
				this.LeftView.BackgroundColor = UIColor.FromWhiteAlpha (0.99f, 1.0f);
				this.LeftView.Layer.AnchorPoint = new PointF (0.0f, 0.5f);
				this.AddSubview (this.LeftView);
				this.LeftView.ShadowView.Colors = new List<UIColor>() {UIColor.FromWhiteAlpha(0.0f, 0.05f), UIColor.FromWhiteAlpha(0.0f, 0.06f) };

				this.RightView = new FacingView (new RectangleF(-1 * frame.Width / 4,0, frame.Width / 2,frame.Height));
				this.RightView.BackgroundColor = UIColor.FromWhiteAlpha (0.99f, 1.0f);
				this.RightView.Layer.AnchorPoint = new PointF (1.0f, 0.5f);
				this.AddSubview (this.RightView);
				this.RightView.ShadowView.Colors = new List<UIColor>() {UIColor.FromWhiteAlpha(0.0f, 0.9f), UIColor.FromWhiteAlpha(0.0f, 0.55f) };

				var transform = CATransform3D.Identity;
				transform.m34 = -1/500.0f;
				this.Layer.SublayerTransform = transform;

				this.LeftView.Layer.Transform = CATransform3D.MakeRotation((float)(Math.PI / 2.0f), 0.0f, 1.0f, 0.0f);
				this.RightView.Layer.Transform = CATransform3D.MakeRotation((float)(Math.PI / 2.0f), 0.0f, 1.0f, 0.0f);
			} else if (this.FoldDirection == FoldDirection.FoldDirectionVertical) {

				this.BottomView = new FacingView (new RectangleF(0.0f, 3 * frame.Height / 4, frame.Width, frame.Height / 2), FoldDirection.FoldDirectionVertical);
				this.BottomView.BackgroundColor = UIColor.FromWhiteAlpha (0.99f, 1.0f);
				this.BottomView.Layer.AnchorPoint = new PointF (0.5f, 1.0f);
				this.AddSubview (this.BottomView);
				this.BottomView.ShadowView.Colors = new List<UIColor>() {UIColor.FromWhiteAlpha(0.0f, 0.5f), UIColor.FromWhiteAlpha(0.0f, 0.6f) };

				this.TopView = new FacingView (new RectangleF(0.0f, 3 * frame.Height / 4, frame.Width, frame.Height / 2), FoldDirection.FoldDirectionVertical);
				this.TopView.BackgroundColor = UIColor.FromWhiteAlpha (0.99f, 1.0f);
				this.TopView.Layer.AnchorPoint = new PointF (0.5f, 0.0f);
				this.AddSubview (this.TopView);
				this.TopView.ShadowView.Colors = new List<UIColor>() {UIColor.FromWhiteAlpha(0.0f, 0.9f), UIColor.FromWhiteAlpha(0.0f, 0.55f) };

				var transform = CATransform3D.Identity;
				transform.m34 = -1/500.0f;
				this.Layer.SublayerTransform = transform;

				this.BottomView.Layer.Transform = CATransform3D.MakeRotation((float)(Math.PI / 2.0f), 1.0f, 0.0f, 0.0f);
				this.TopView.Layer.Transform = CATransform3D.MakeRotation((float)(Math.PI / 2.0f), 1.0f, 0.0f, 0.0f);
			}

			this.AutosizesSubviews = true;

			this.ContentView.AutosizesSubviews = true;
			this.ContentView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

		}

		public void SetImage(UIImage image)
		{

			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight || this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft) {

				var imageRef = image.CGImage.WithImageInRect (new RectangleF(0.0f, 0.0f, image.Size.Width * image.CurrentScale / 2, image.Size.Height * image.CurrentScale));
				this.LeftView.Layer.Contents = imageRef;

				var imageRef2 = image.CGImage.WithImageInRect (new RectangleF(image.Size.Width*image.CurrentScale/2, 0, image.Size.Width*image.CurrentScale/2, image.Size.Height*image.CurrentScale));

				this.RightView.Layer.Contents = imageRef2;

			} else if (this.FoldDirection == FoldDirection.FoldDirectionVertical) {
				var imageRef = image.CGImage.WithImageInRect (new RectangleF(0.0f, image.Size.Height*image.CurrentScale/2, image.Size.Width*image.CurrentScale, image.Size.Height*image.CurrentScale/2));
				this.BottomView.Layer.Contents = imageRef;

				var imageRef2 = image.CGImage.WithImageInRect (new RectangleF(0.0f, 0.0f, image.Size.Width*image.CurrentScale, image.Size.Height*image.CurrentScale/2));
				this.TopView.Layer.Contents = imageRef2;
			}
		}

		public void UnfoldView(float fraction)
		{
			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft || this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight) {
				var delta = (float)Math.Asin ((double)fraction);

				this.LeftView.Layer.Transform = CATransform3D.MakeRotation ((float)(Math.PI / 2.0f) - delta, 0.0f, 1.0f, 0.0f);

				var transform1 = CATransform3D.MakeTranslation (2  * this.LeftView.Frame.Width, 0.0f, 0.0f);
				var transform2 = CATransform3D.MakeRotation ((float)(Math.PI / 2.0f) - delta, 0.0f, -1.0f, 0.0f);
				var transform = transform1.Concat (transform2);
				this.RightView.Layer.Transform = transform;

				this.LeftView.ShadowView.Alpha = 1 - fraction;
				this.RightView.ShadowView.Alpha = 1 - fraction;

			} else if (this.FoldDirection == FoldDirection.FoldDirectionVertical) {
				var delta = (float)Math.Asin ((double)fraction);

				this.BottomView.Layer.Transform = CATransform3D.MakeRotation ((float)(Math.PI / 2.0f) - delta, 1.0f, 0.0f, 0.0f);

				var transform1 = CATransform3D.MakeTranslation (0.0f, -2  * this.BottomView.Frame.Height, 0.0f);
				var transform2 = CATransform3D.MakeRotation ((float)(Math.PI / 2.0f) - delta, -1.0f, -0.0f, 0.0f);
				var transform = transform2.Concat (transform1);
				this.TopView.Layer.Transform = transform;

				this.BottomView.ShadowView.Alpha = 1 - fraction;
				this.TopView.ShadowView.Alpha = 1 - fraction;
			}
		}

		public void CalculateFoldState(float offset)
		{

			var fraction = 0.0f;

			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight || this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft) {
				fraction = offset / this.Frame.Width;
				if (fraction < 0) {
					fraction = 0;
				}
				if (fraction > 1) {
					fraction = 1;
				}
			} else if (this.FoldDirection == FoldDirection.FoldDirectionVertical) {
				fraction = offset / this.Frame.Height;
				if (fraction < 0) {
					fraction = -1 * fraction;
				}
				if (fraction > 1) {
					fraction = 1;
				}
			}

			if (this.State == FoldState.FoldStateClosed && fraction > 0) {
				this.State = FoldState.FoldStateTransition;
				this.FoldWillOpen();
			} else if (this.State == FoldState.FoldStateOpened && fraction < 1) {
				this.State = FoldState.FoldStateTransition;
				this.FoldWillClose();
			} else if (this.State == FoldState.FoldStateTransition) {
				if (fraction==0)
				{
					this.State = FoldState.FoldStateClosed;
					this.FoldDidClose();
				}
				else if (fraction==1)
				{
					this.State = FoldState.FoldStateOpened;
					this.FoldDidOpen();
				}
			}
		}

		public void Unfold(float parentOffset)
		{
			this.CalculateFoldState (parentOffset);

			var fraction = 0.0f;

			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight || this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft) {
				fraction = parentOffset / this.Frame.Width;
				if (fraction < 0) {
					fraction = 0;
				}
				if (fraction > 1) {
					fraction = 1;
				}

				this.UnfoldView(fraction);
			} else if (this.FoldDirection == FoldDirection.FoldDirectionVertical) {
				fraction = parentOffset / this.Frame.Height;
				if (fraction < 0) {
					fraction = -1 * fraction;
				}
				if (fraction > 1) {
					fraction = 1;
				}
				this.UnfoldView(fraction);
			}
		}

		public void SetContent(UIView contentView)
		{

			contentView.Frame = new RectangleF (0.0f, 0.0f, contentView.Frame.Width, contentView.Frame.Height);
			this.ContentView.AddSubview (contentView);
			this.DrawScreenshotOnFolds ();
		}

		private void DrawScreenshotOnFolds()
		{
			var image = this.ContentView.Screenshot (this.UseOptimizedScreenshot);
			this.SetImage (image);
		}

		private void ShowFolds(bool show)
		{
			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight || this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft) {
				this.LeftView.Hidden = !show;
				this.RightView.Hidden = !show;
			} else if (this.FoldDirection == FoldDirection.FoldDirectionVertical) {
				this.TopView.Hidden = !show;
				this.BottomView.Hidden = !show;
			}
		}

		private void FoldDidOpen()
		{
			this.ContentView.Hidden = false;
			this.ShowFolds (false);
		}

		private void FoldDidClose()
		{
			this.ContentView.Hidden = false;
			this.ShowFolds (true);
		}

		private void FoldWillOpen()
		{
			this.ContentView.Hidden = true;
			this.ShowFolds (true);
		}

		private void FoldWillClose()
		{
			this.DrawScreenshotOnFolds ();
			this.ContentView.Hidden = true;
			this.ShowFolds (true);
		}

	}
}

