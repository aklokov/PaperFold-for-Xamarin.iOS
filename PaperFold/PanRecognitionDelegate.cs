using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace PaperFold
{
	public class PanRecognitionDelegate : UIGestureRecognizerDelegate
	{
		private WeakReference parent;

		public PanRecognitionDelegate (PaperFoldView paperfoldView)
		{
			this.parent = new WeakReference (paperfoldView);
		}

		public override bool ShouldBegin(UIGestureRecognizer recognizer)
		{/*
			if (this.parent.IsAlive) {
				var paperfoldView = (PaperFoldView)this.parent.Target;
				if (paperfoldView.EnableHorizontalEdgeDragging){
					var location = recognizer.LocationInView(paperfoldView.ContentView);
					if (location.X < PaperFoldConstants.EdgeScrollWidth || location.X > (paperfoldView.ContentView.Frame.Width - PaperFoldConstants.EdgeScrollWidth)) {
						return false;
					} else {
						return true;
					}
				} else {
					return false;
				}
			}
			return false;*/return true;
		}


		public override bool ShouldRecognizeSimultaneously (UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
		{
			if (this.parent.IsAlive) {
				var paperfoldView = (PaperFoldView)this.parent.Target;
				if (paperfoldView.RestrictedDraggingRect.IsNull () == false &&
					paperfoldView.RestrictedDraggingRect.Contains (gestureRecognizer.LocationInView(paperfoldView)) == false) {
					return false;
				} else {
					return true;
				}
			}
			return false;
		}
	}
}

