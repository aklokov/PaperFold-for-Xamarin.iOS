using System;

namespace MonoTouch.UIKit
{
	public static class UIViewScreenshot
	{
		public static UIImage Screenshot(this UIView view, bool optimized = true)
		{
			if (optimized)
			{
				// take screenshot of the view
				if (view.GetType ().Name == "MKMapView") {
					if (float.Parse(UIDevice.CurrentDevice.SystemVersion) >= 6.0) {

						// in iOS6, there is no problem using a non-retina screenshot in a retina display screen
						UIGraphics.BeginImageContextWithOptions (view.Frame.Size, false, 1.0f);

					} else {

						// if the view is a mapview in iOS5.0 and below, screenshot has to take the screen scale into consideration
						// else, the screen shot in retina display devices will be of a less detail map (note, it is not the size of the screenshot, but it is the level of detail of the screenshot
						UIGraphics.BeginImageContextWithOptions (view.Frame.Size, false, 1.0f);
					}
				} else {
					// for performance consideration, everything else other than mapview will use a lower quality screenshot
					UIGraphics.BeginImageContext (view.Frame.Size);	
				}

			} else
			{
				UIGraphics.BeginImageContextWithOptions (view.Frame.Size, false, 0.0f);
			}

	
			var context = UIGraphics.GetCurrentContext ();

			if (context == null)
			{
				Console.WriteLine("UIGraphicsGetCurrentContext() is nil. You may have a UIView with CGRectZero");
				return null;
			}
			else
			{
				view.Layer.RenderInContext (context);
				
				var screenshot = UIGraphics.GetImageFromCurrentImageContext ();
				UIGraphics.EndImageContext();

				return screenshot;
			}
		}
	}
}

