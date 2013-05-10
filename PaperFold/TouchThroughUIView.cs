using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace PaperFold
{
	public class TouchThroughUIView : UIView
	{
		public TouchThroughUIView (RectangleF frame)
			: base(frame)
		{
		}

		public override bool PointInside (System.Drawing.PointF point, UIEvent uievent)
		{
			// set any point within the bound, and on the right side of the bound, as touch area
			// it is required to set the right side of the bound as touch area because the right fold, is a subview of this view
			// the left fold is not required because it is on the same hierachy as this view, as a subview of this view's superview
			if (point.X < 0.0f) {
				return false;
			} else {
				return true;
			}
		}
	}
}

