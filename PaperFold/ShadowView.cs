using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.CoreAnimation;


namespace PaperFold
{
	public class ShadowView : UIView
	{
		public List<UIColor> Colors {
			get;
			set;
		} 

		public CAGradientLayer Gradient {
			get;
			set;
		}


		public ShadowView (RectangleF frame, FoldDirection foldDirection)
			:base(frame)
		{
			this.Gradient = new CAGradientLayer ();
			this.Gradient.Frame = new RectangleF (0.0f, 0.0f, frame.Width, frame.Height);

			if (foldDirection == FoldDirection.FoldDirectionVertical) {
				this.Gradient.StartPoint = new PointF(0.0f, 1.0f);
				this.Gradient.EndPoint = new PointF(0.0f, 0.0f);
			} else {
				this.Gradient.StartPoint = new PointF(0.0f, 0.0f);
				this.Gradient.EndPoint = new PointF(0.0f, 1.0f);
			}
			this.Layer.InsertSublayer(this.Gradient, 0);
			this.BackgroundColor = UIColor.Clear;
		}
	}
}

