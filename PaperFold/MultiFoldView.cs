using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace PaperFold
{
	public class MultiFoldView : UIView
	{

		public Func<UIView, float> DisplacementOfMultiFoldView {
			get;
			set;
		}

		// number of folds
		public int NumberOfFolds {
			get;
			set;
		}

		// fraction of the view on the right to its immediate left
		// determines when the next fold on the right should open
		public float PullFactor {
			get;
			set;
		}

		// indicate whether the fold is open or closed
		public FoldState State {
			get;
			set;
		}

		// fold direction
		public FoldDirection FoldDirection {
			get;
			set;
		}

		// optimized screenshot follows the scale of the screen
		// non-optimized is always the non-retina image
		public bool UseOptimizedScreenshot {
			get;
			set;
		}

		// take screenshot just before unfolding
		// this is only necessary for mapview, not for the rest of the views
		public bool ShoulTakeScreenshotBeforeUnfolding {
			get;
			set;
		}

		public UIView ContentViewHolder {
			get;
			set;
		}

		public override RectangleF Frame {
			get{
				return base.Frame;
			}
			set{
				base.Frame = value;

				if (this.ContentViewHolder != null) {
					var contentViewHolderFrame = this.ContentViewHolder.Frame;
					contentViewHolderFrame.Height = value.Height;
					this.ContentViewHolder.Frame = contentViewHolderFrame;
				}
				foreach (var subView in this.Subviews) {
					if (subView.GetType () == typeof(FoldView)) {
						subView.RemoveFromSuperview ();
					}
				}

				this.CreateFoldViews (value, this.FoldDirection);
			}
		}

		public MultiFoldView ()
			:this(new RectangleF(0.0f, 0.0f, 0.0f, 0.0f), 0, 0.0f)
		{
			
		}

		public MultiFoldView (RectangleF frame, int folds, float pullFactor)
			:this(frame, FoldDirection.FoldDirectionHorizontalLeftToRight, folds, pullFactor)
		{

		}

		public MultiFoldView (RectangleF frame, FoldDirection foldDirection, int folds, float pullFactor)
			: base(frame)
		{
			this.UseOptimizedScreenshot = true;
			this.FoldDirection = foldDirection;
			this.NumberOfFolds = folds;

			if (this.NumberOfFolds == 1) {
				this.PullFactor = 0;
			} else {
				this.PullFactor = pullFactor;
			}

			this.CreateFoldViews (frame, foldDirection);

			this.AutosizesSubviews = true;
		}

		public void SetContent(UIView contentView)
		{
			if (contentView.GetType ().Name == "MKMapView") {
				this.ShoulTakeScreenshotBeforeUnfolding = true;
			}

			this.ContentViewHolder = new UIView (new RectangleF(0.0f, 0.0f, contentView.Frame.Width, contentView.Frame.Height));
			this.ContentViewHolder.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			this.InsertSubview (this.ContentViewHolder, 0);
			this.ContentViewHolder.AddSubview (contentView);

			this.DrawScreenshotOnFolds ();

			this.ContentViewHolder.Hidden = true;

		}

		private void DrawScreenshotOnFolds()
		{
			var image = this.ContentViewHolder.Screenshot (this.UseOptimizedScreenshot);
			this.SetScreenshotImage (image);
		}

		private void SetScreenshotImage(UIImage image)
		{
			if(this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight || this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft)
			{
				var foldWidth = image.Size.Width / this.NumberOfFolds;

				for (int i = 0; i < this.NumberOfFolds; i++) {
					var imageRef = image.CGImage.WithImageInRect (new RectangleF(foldWidth*i*image.CurrentScale, 0.0f, foldWidth * image.CurrentScale, image.Size.Height * image.CurrentScale));
					if (imageRef != null) {
						var croppedImage = UIImage.FromImage (imageRef);

						FoldView foldView = null;

						if(this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight) {
							foldView = (FoldView)this.ViewWithTag (PaperFoldConstants.FoldViewTag + (this.NumberOfFolds - 1) - i);
						} else {
							foldView = (FoldView)this.ViewWithTag(PaperFoldConstants.FoldViewTag + i);
						}
						foldView.SetImage(croppedImage);
					}
				}
			} else if (this.FoldDirection == FoldDirection.FoldDirectionVertical){
				var foldHeight = image.Size.Height / this.NumberOfFolds;
				for (int i = 0; i < this.NumberOfFolds; i++) {
					var imageRef = image.CGImage.WithImageInRect (new RectangleF(0.0f, foldHeight*(this.NumberOfFolds-i-1)*image.CurrentScale, image.Size.Width * image.CurrentScale, foldHeight * image.CurrentScale));
					if(imageRef != null){
						var croppedImage = UIImage.FromImage (imageRef);
						var foldView = (FoldView)this.ViewWithTag(PaperFoldConstants.FoldViewTag+i);
						foldView.SetImage(croppedImage);
					}

				}
			}
		}

		private void CalculateFoldState(float offset)
		{
			var fraction = 0.0f;
			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft || this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight)
			{
				if (offset < 0)
					fraction = -1*offset/this.Frame.Width;
				else
					fraction = offset/this.Frame.Width;
			}
			else if (this.FoldDirection==FoldDirection.FoldDirectionVertical)
			{
				fraction = offset/this.Frame.Height;
			}

			if (this.State == FoldState.FoldStateClosed && fraction>0)
			{
				this.State = FoldState.FoldStateTransition;
				this.FoldWillOpen();
			}
			else if (this.State == FoldState.FoldStateOpened && fraction<1)
			{
				this.State = FoldState.FoldStateTransition;
				this.FoldWillClose();
			}
			else if (this.State == FoldState.FoldStateTransition)
			{
				if (fraction==0)
				{
					this.State = FoldState.FoldStateClosed;
					this.FoldDidClose();
				}
				else if (fraction>=1)
				{
					this.State = FoldState.FoldStateOpened;
					this.FoldDidOpen();
				}
			}

		}

		public void Unfold(float parentOffset)
		{
			this.CalculateFoldState (parentOffset);

			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight || this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft) {
				var foldWidth = this.Frame.Width / this.NumberOfFolds;

				float fraction;
				if (parentOffset < 0) {
					if (parentOffset<-1*(foldWidth+this.PullFactor*foldWidth))
					{
						parentOffset = -1*(foldWidth+this.PullFactor*foldWidth);
					}
					fraction = parentOffset /(-1*(foldWidth+this.PullFactor*foldWidth));
				} else {
					if (parentOffset > (foldWidth+this.PullFactor*foldWidth))
					{
						parentOffset = (foldWidth+this.PullFactor*foldWidth);
					}
					fraction = parentOffset /(foldWidth+this.PullFactor*foldWidth);
				}
				//fraction = offset /(-1*(foldWidth+self.pullFactor*foldWidth));

				if (fraction < 0) fraction  = -1*fraction;//0;
				if (fraction > 1) fraction = 1;
				this.UnfoldView (fraction);

			} else if(this.FoldDirection == FoldDirection.FoldDirectionVertical){ 
				var foldHeight = this.Frame.Height / this.NumberOfFolds;

				if (parentOffset>(foldHeight+this.PullFactor*foldHeight))
				{
					parentOffset = (foldHeight+this.PullFactor*foldHeight);
				}

				var fraction = parentOffset /(foldHeight+this.PullFactor*foldHeight);
				if (fraction < 0) fraction = -1*fraction;//0;
				if (fraction > 1) fraction = 1;

				this.UnfoldView(fraction);
			}
		}

		public void UnfoldView(float fraction)
		{
			// start the cascading effect of unfolding
			// with the first foldView with index FOLDVIEW_TAG+0

			var firstFoldView = (FoldView)this.ViewWithTag (PaperFoldConstants.FoldViewTag);

			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight) {
				var offset = 0.0f;
				if (this.DisplacementOfMultiFoldView != null) {
					offset = this.DisplacementOfMultiFoldView (this);
				} else {
					offset = this.Superview.Frame.X;
				}

				if(offset < 0){
					offset = -1 * offset;
				}

				this.UnfoldView(firstFoldView, fraction, offset);
			} else {
				this.UnfoldView(firstFoldView, fraction, 0.0f);
			}
		}

		public void UnfoldView(FoldView foldView, float fraction, float offset)
		{
			foldView.UnfoldView (fraction);

			if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight || this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft) {
				if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight) {
					foldView.Frame = new RectangleF (offset - 2 * foldView.RightView.Frame.Width, 0.0f, foldView.Frame.Width, foldView.Frame.Height);
				}

				var index = foldView.Tag - PaperFoldConstants.FoldViewTag;

				if (index < this.NumberOfFolds - 1) {
					var nextFoldView = (FoldView)this.ViewWithTag (PaperFoldConstants.FoldViewTag+index+1);

					if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight) {
						nextFoldView.Frame = new RectangleF (foldView.Frame.X - 2*nextFoldView.RightView.Frame.Width, 0.0f, nextFoldView.Frame.Width, nextFoldView.Frame.Height);
					} else {
						nextFoldView.Frame = new RectangleF (foldView.Frame.X + 2*foldView.LeftView.Frame.Width, 0.0f, nextFoldView.Frame.Width, nextFoldView.Frame.Height);
					}

					var foldWidth = this.Frame.Width / this.NumberOfFolds;

					var displacement = 0.0f;

					if (this.DisplacementOfMultiFoldView != null) {
						displacement = this.DisplacementOfMultiFoldView (this);
					} else {
						displacement = this.Superview.Frame.X;
					}

					float x;
					if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight) {
						x = (foldView.Frame.X + (fraction * foldView.Frame.Width)) - 2 * foldView.RightView.Frame.Width;
					} else {
						x = displacement + foldView.Frame.X + 2 * foldView.LeftView.Frame.Width;
					}

					float adjustedFraction = 0.0f;
					if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight) {
						x = -x;
					}

					if (index + 1 == this.NumberOfFolds - 1) {
						adjustedFraction = (-1 * x) / foldWidth;
					} else {
						adjustedFraction = (-1 * x) / (foldWidth + this.PullFactor * foldWidth);
					}

					if (adjustedFraction < 0) {
						adjustedFraction = 0;
					}

					if (adjustedFraction > 1) {
						adjustedFraction = 1;
					}

					this.UnfoldView (nextFoldView, adjustedFraction, foldView.Frame.X);
				}
			} else if (this.FoldDirection == FoldDirection.FoldDirectionVertical) {

				var index = foldView.Tag - PaperFoldConstants.FoldViewTag;

				if (index < this.NumberOfFolds - 1) {
					var nextFoldView = (FoldView)this.ViewWithTag (PaperFoldConstants.FoldViewTag + index + 1);

					nextFoldView.Frame = new RectangleF (0.0f, foldView.Frame.Y - 2 * foldView.BottomView.Frame.Height, nextFoldView.Frame.Width, nextFoldView.Frame.Height);

					var foldHeight = this.Frame.Height / this.NumberOfFolds;

					var displacement = 0.0f;

					if (this.DisplacementOfMultiFoldView != null) {
						displacement = this.DisplacementOfMultiFoldView (this);
					} else {
						if (this.Superview.GetType () == typeof(UIScrollView)) {
							displacement = -1 * ((UIScrollView)this.Superview).ContentOffset.Y;
						} else {
							displacement = this.Superview.Frame.Y;
						}
					}
					float y = displacement - 2 * foldView.BottomView.Frame.Height;

					var _index = index - 1;

					while (_index>=0) {
						var prevFoldView = (FoldView)this.ViewWithTag (PaperFoldConstants.FoldViewTag+_index);
						y -= 2 * prevFoldView.BottomView.Frame.Height;
						_index -= 1;
					}

					var adjustedFraction = 0.0f;

					if (index + 1 == this.NumberOfFolds - 1) {
						adjustedFraction = y / foldHeight;
					} else {
						adjustedFraction = y / (foldHeight + this.PullFactor * foldHeight);
					}

					if (adjustedFraction < 0) {
						adjustedFraction = 0;
					}
					if (adjustedFraction > 1) {
						adjustedFraction = 1;
					}

					this.UnfoldView (nextFoldView, adjustedFraction, 0.0f);
				}
			}
		}

		public void UnfoldWithoutAnimation()
		{
			this.Unfold (this.Frame.Width);
			this.FoldDidOpen ();
		}

		private void ShowFolds(bool show)
		{
			for (int i = 0; i < this.NumberOfFolds; i++) {
				var foldView = (FoldView)this.ViewWithTag (PaperFoldConstants.FoldViewTag + i);
				foldView.Hidden  = !show;
			}
		}


		private void FoldDidOpen()
		{
			this.ContentViewHolder.Hidden = false;
			this.ShowFolds (false);
		}

		private void FoldDidClose()
		{
			this.ContentViewHolder.Hidden = true;
			this.ShowFolds (true);
		}

		private void FoldWillOpen()
		{
			if (this.ShoulTakeScreenshotBeforeUnfolding) {
				this.ContentViewHolder.Hidden = false;
				this.DrawScreenshotOnFolds ();
			}
			this.ContentViewHolder.Hidden = true;
			this.ShowFolds (true);
		}

		private void FoldWillClose()
		{
			this.DrawScreenshotOnFolds ();
			this.ContentViewHolder.Hidden = true;
			this.ShowFolds (true);
		}

		private void CreateFoldViews (RectangleF frame, FoldDirection foldDirection)
		{
			// create multiple FoldView next to each other
			for (int i = 0; i < this.NumberOfFolds; i++) {
				if (this.FoldDirection == FoldDirection.FoldDirectionHorizontalLeftToRight || this.FoldDirection == FoldDirection.FoldDirectionHorizontalRightToLeft) {
					var foldWidth = frame.Width / this.NumberOfFolds;
					var foldView = new FoldView (new RectangleF (foldWidth * i, 0.0f, foldWidth, frame.Height), foldDirection);
					foldView.Tag = PaperFoldConstants.FoldViewTag + i;
					this.AddSubview (foldView);
				}
				else
					if (this.FoldDirection == FoldDirection.FoldDirectionVertical) {
						var foldHeight = frame.Height / this.NumberOfFolds;
						var foldView = new FoldView (new RectangleF (0.0f, foldHeight * i, foldHeight, frame.Height), foldDirection);
						foldView.Tag = PaperFoldConstants.FoldViewTag + i;
						this.AddSubview (foldView);
					}
			}
		}
	}
}

