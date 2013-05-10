using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace PaperFold
{
	public class FacingView : UIView
	{

		public ShadowView ShadowView {
			get;
			set;
		}

		public FacingView (RectangleF frame)
			:this(frame, FoldDirection.FoldDirectionHorizontalRightToLeft)
		{

		}

		public FacingView (RectangleF frame, FoldDirection foldDirection)
			:base(frame)
		{
			this.ShadowView = new ShadowView (new RectangleF(0.0f, 0.0f, frame.Width, frame.Height), foldDirection);
			this.AddSubview (this.ShadowView);
			this.ShadowView.BackgroundColor = UIColor.Clear;
		}
	}
}

