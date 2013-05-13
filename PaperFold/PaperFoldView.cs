using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using System.Threading;

namespace PaperFold
{
	public class PaperFoldView : UIView
	{
	
		private UIPanGestureRecognizer PanGestureRecognizer {
			get;
			set;
		}

		// indicate if the divider line should be visible
		private bool showDividerLines;

		private Action completionHandler;

		public Action<UIView, bool, PaperFoldState> DidFoldAutomaticallToState {
			get;
			set;
		}

		public Action<UIView, PointF> DidOffset {
			get;
			set;
		}

		// main content view
		public TouchThroughUIView ContentView {
			get;
			set;
		}

		// timer to animate folds after gesture ended
		// manual animation with NSTimer is required to sync the offset of the contentView, with the folding of views
		public Timer AnimationTimer {
			get;
			set;
		}

		private Action animationAction;

		// the fold view on the left and bottom
		public FoldView BottomFoldView {
			get;
			set;
		}

		// the fold view on the left
		public MultiFoldView LeftFoldView {
			get;
			set;
		}


		// the multiple fold view on the right
		public MultiFoldView RightFoldView {
			get;
			set;
		}

		// the multiple fold view on the top
		public MultiFoldView TopFoldView {
			get;
			set;
		}

		// state of the current fold
		public PaperFoldState State {
			get;
			set;
		}

		public PaperFoldState LastState {
			get;
			set;
		}

		// enable and disable dragging

		public bool EnableLeftFoldDragging {
			get;
			set;
		}

		public bool EnableRightFoldDragging {
			get;
			set;
		}

		public bool EnableTopFoldDragging {
			get;
			set;
		}

		public bool EnableBottomFoldDragging {
			get;
			set;
		}

		public bool EnableHorizontalEdgeDragging {
			get;
			set;
		}

		// indicate if the fold was triggered by finger panning, or set state
		public bool IsAutomatedFolding {
			get;
			set;
		}

		// the initial panning direction
		public PaperFoldInitialPanDirection PaperFoldInitialPanDirection {
			get;
			set;
		}

		// optimized screenshot follows the scale of the screen
		// non-optimized is always the non-retina image
		public bool UseOptimizedScreenshot {
			get;
			set;
		}

		// restrict the dragging gesture recogniser to a certain UIRect of this view. Useful to restrict
		// dragging to, say, a navigation bar.
		public RectangleF RestrictedDraggingRect {
			get;
			set;
		}

		// divider lines
		// these are exposed so that it is possible to hide the lines
		// especially when views have rounded corners
		public UIView LeftDividerLine {
			get;
			set;
		}

		public UIView RightDividerLine {
			get;
			set;
		}

		
		public UIView TopDividerLine {
			get;
			set;
		}

		
		public UIView BottomDividerLine {
			get;
			set;
		}

		public PaperFoldView (RectangleF frame)
			:base(frame)
		{
			Initialize();
		}

		public override RectangleF Frame {
			get{
				return base.Frame;
			}
			set {
				base.Frame = value;
				this.UpdateSideFoldViewFrames (value);
			}
		}



		public PaperFoldView(IntPtr handle)
			:base(handle)
		{
			Initialize();
		}

		private void Initialize()
		{
			this.UseOptimizedScreenshot = true;

			this.BackgroundColor = UIColor.DarkGray;
			this.AutosizesSubviews = true;

			this.ContentView = new TouchThroughUIView (new RectangleF(0.0f, 0.0f, this.Frame.Width, this.Frame.Height));
			this.AutoresizingMask = 
				this.ContentView.AutoresizingMask =  
					UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;

			this.AddSubview (this.ContentView);
			this.ContentView.BackgroundColor = UIColor.White;
			this.ContentView.AutosizesSubviews = true;

			this.PanGestureRecognizer = new UIPanGestureRecognizer (this.ContentViewPanned);

			var panDelegate = new PanRecognitionDelegate (this);

			this.PanGestureRecognizer.Delegate = panDelegate;

			this.ContentView.AddGestureRecognizer (this.PanGestureRecognizer);


			this.LastState = this.State = PaperFoldState.PaperFoldStateDefault;

			this.EnableLeftFoldDragging = 
				this.EnableRightFoldDragging = 
					this.EnableTopFoldDragging = 
					this.EnableBottomFoldDragging = false;

			this.RestrictedDraggingRect = RectangleF.Empty;
			this.showDividerLines = false;
			this.LeftFoldView = new MultiFoldView ();
			this.RightFoldView = new MultiFoldView ();
			this.BottomFoldView = new FoldView (new RectangleF(0.0f, 0.0f, 0.0f, 0.0f));
			this.TopFoldView = new MultiFoldView ();
		}


		public void SetCenterContentView(UIView view)
		{
			view.AutoresizingMask = this.AutoresizingMask;
			this.ContentView.AddSubview (view);
		}

		public void SetLeftFoldContentView(UIView view, int foldCount, float pullFactor)
		{
			if (this.LeftFoldView != null) {
				this.LeftFoldView.RemoveFromSuperview ();
			}

			this.LeftFoldView = new MultiFoldView (new RectangleF(0.0f, 0.0f, view.Frame.Width, view.Frame.Height), FoldDirection.FoldDirectionHorizontalLeftToRight, foldCount, pullFactor);
			this.LeftFoldView.DisplacementOfMultiFoldView = this.DisplacementOfMultiFoldView;
			this.LeftFoldView.UseOptimizedScreenshot = this.UseOptimizedScreenshot;
			this.LeftFoldView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			this.InsertSubviewBelow (this.LeftFoldView, this.ContentView);
			this.LeftFoldView.SetContent (view);
			this.LeftFoldView.Hidden = true;
			view.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			var line = new UIView (new RectangleF(-1.0f,0.0f,1.0f,this.Frame.Height));
			line.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			this.ContentView.AddSubview (line);
			line.BackgroundColor = UIColor.FromWhiteAlpha (0.9f, 0.5f);
			line.Alpha = 0.0f;
			this.LeftDividerLine = line;

			this.EnableLeftFoldDragging = true;
		}

		public void SetBottomFoldContentView(UIView view)
		{
			if (this.BottomFoldView != null) {
				this.BottomFoldView.RemoveFromSuperview ();
			}

			this.BottomFoldView = new FoldView (new RectangleF(0.0f, this.Frame.Height - view.Frame.Height, view.Frame.Width, view.Frame.Height), FoldDirection.FoldDirectionVertical);
			this.BottomFoldView.UseOptimizedScreenshot = this.UseOptimizedScreenshot;
			this.BottomFoldView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			this.InsertSubviewBelow (this.BottomFoldView, this.ContentView);
			this.BottomFoldView.SetContent (view);
			this.BottomFoldView.Hidden = true;
			view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;

			var line = new UIView (new RectangleF(0.0f, this.Frame.Height,this.Frame.Width,1.0f));
			line.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			this.ContentView.AddSubview (line);
			line.BackgroundColor = UIColor.FromWhiteAlpha (0.9f, 0.5f);
			line.Alpha = 0.0f;
			this.BottomDividerLine = line;

			this.EnableBottomFoldDragging = true;
		}

		public void SetRightFoldContentView(UIView view, int foldCount, float pullFactor)
		{
			if (this.RightFoldView != null) {
				this.RightFoldView.RemoveFromSuperview ();
			}

			this.RightFoldView = new MultiFoldView (new RectangleF(this.Frame.Width, 0.0f, view.Frame.Width, this.Frame.Height), FoldDirection.FoldDirectionHorizontalRightToLeft, foldCount, pullFactor);
			this.RightFoldView.DisplacementOfMultiFoldView = this.DisplacementOfMultiFoldView;
			this.RightFoldView.UseOptimizedScreenshot = this.UseOptimizedScreenshot;
			this.RightFoldView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			this.InsertSubviewBelow (this.RightFoldView, this.ContentView);
			this.RightFoldView.SetContent (view);
			this.RightFoldView.Hidden = true;
			view.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			var line = new UIView (new RectangleF( this.ContentView.Frame.Width, 0.0f,1.0f,this.Frame.Height));
			line.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			this.ContentView.AddSubview (line);
			line.BackgroundColor = UIColor.FromWhiteAlpha (0.9f, 0.5f);
			line.Alpha = 0.0f;
			this.RightDividerLine = line;

			this.EnableRightFoldDragging = true;
		}

		public void SetTopFoldContentView(UIView view,  int foldCount, float pullFactor)
		{
			if (this.TopFoldView != null) {
				this.TopFoldView.RemoveFromSuperview ();
			}

			this.TopFoldView = new MultiFoldView (new RectangleF(0.0f, -1 * view.Frame.Height, view.Frame.Width, view.Frame.Height), FoldDirection.FoldDirectionVertical, foldCount, pullFactor);
			this.TopFoldView.UseOptimizedScreenshot = this.UseOptimizedScreenshot;
			this.TopFoldView.DisplacementOfMultiFoldView = this.DisplacementOfMultiFoldView;
			this.TopFoldView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			this.InsertSubviewBelow (this.TopFoldView, this.ContentView);
			this.TopFoldView.SetContent (view);
			this.TopFoldView.Hidden = true;
			view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;

			var line = new UIView (new RectangleF(0.0f,-1.0f, this.Frame.Width, 1.0f));
			line.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			this.ContentView.AddSubview (line);
			line.BackgroundColor = UIColor.FromWhiteAlpha (0.9f, 0.5f);
			line.Alpha = 0.0f;
			this.TopDividerLine = line;

			this.EnableTopFoldDragging = true;
		}

		private void UpdateSideFoldViewFrames(RectangleF frame)
		{
			if(this.LeftFoldView != null){
				var leftFoldViewFrame = this.LeftFoldView.Frame;
				leftFoldViewFrame.Height = frame.Height;
				this.LeftFoldView.Frame = leftFoldViewFrame;
			}

			if(this.RightFoldView != null){
				var rightFoldViewFrame = this.RightFoldView.Frame;
				rightFoldViewFrame.Height = frame.Height;
				this.RightFoldView.Frame = rightFoldViewFrame;
			}
		}

		private float DisplacementOfMultiFoldView(UIView multiFoldView)
		{
			if(multiFoldView == this.RightFoldView){
				return this.ContentView.Frame.X;
			} else if (multiFoldView == this.LeftFoldView) {
				return -1 * this.ContentView.Frame.X;
			} else if (multiFoldView == this.TopFoldView) {
				if(this.ContentView.GetType() == typeof(UIScrollView)){
					var scroll = (UIView)this.ContentView;
					return -1 * ((UIScrollView)scroll).ContentOffset.Y;
				} else {
					return this.ContentView.Frame.Y;
				}
			} else if (multiFoldView == this.BottomFoldView) {
				if(this.ContentView.GetType() == typeof(UIScrollView)){
					var scroll = (UIView)this.ContentView;
					return -1 * ((UIScrollView)scroll).ContentOffset.Y;
				} else {
					return this.ContentView.Frame.Y;
				}
			}

			return 0.0f;
		}

		void CreateTimer (Action unfoldAction)
		{
			this.animationAction = unfoldAction;
			AutoResetEvent autoEvent = new AutoResetEvent(false);
			this.AnimationTimer = new Timer (new TimerCallback(this.TimerFire), autoEvent, TimeSpan.FromSeconds(0.01), TimeSpan.FromSeconds(0.01));
		}

		// This method is called by the timer. 
		public void TimerFire(Object stateInfo)
		{
			this.InvokeOnMainThread (() => {
				this.animationAction ();
				Console.WriteLine("Timer Hit");
			});
		}

		private void ContentViewPanned(UIPanGestureRecognizer panGestureRecognizer)
		{
			if (this.AnimationTimer!= null /*&& this.AnimationTimer.*/) {
				return;
			}

			if (panGestureRecognizer.State == UIGestureRecognizerState.Began) {
				this.ShowDividerLines (true, true);

				var velocity = panGestureRecognizer.VelocityInView (this);

				if (Math.Abs (velocity.X) > Math.Abs (velocity.Y)) {
					if (this.State == PaperFoldState.PaperFoldStateDefault) {
						this.PaperFoldInitialPanDirection = PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionHorizontal;
					} else {
						if (this.EnableHorizontalEdgeDragging) {
							PointF location = panGestureRecognizer.LocationInView (this.ContentView);

							if (location.X < PaperFoldConstants.EdgeScrollWidth || location.Y > (this.ContentView.Frame.Width - PaperFoldConstants.EdgeScrollWidth)) {
								this.PaperFoldInitialPanDirection = PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionHorizontal;
							} else {
								this.PaperFoldInitialPanDirection = PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionVertical;

							}
						} else {
							this.PaperFoldInitialPanDirection = PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionHorizontal;
						}
					}
				} else {
					if(this.State == PaperFoldState.PaperFoldStateDefault){
						this.PaperFoldInitialPanDirection = PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionVertical;
					}
				}

			} else {
				if (this.PaperFoldInitialPanDirection == PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionHorizontal) {
					this.ContentPannedHorizontally (panGestureRecognizer);
				} else {
					this.ContentPannedVertically (panGestureRecognizer);
				}

				if (panGestureRecognizer.State == UIGestureRecognizerState.Changed) {
					this.ShowDividerLines(false, true);
				}
			}
		}

		private void ContentPannedHorizontally(UIPanGestureRecognizer panGestureRecognizer)
		{
			this.RightFoldView.Hidden = false;	
			this.LeftFoldView.Hidden = false;

			this.TopFoldView.Hidden = true;
			this.BottomFoldView.Hidden = true;

			var point = panGestureRecognizer.TranslationInView (this);

			if (panGestureRecognizer.State == UIGestureRecognizerState.Changed) {

				if (this.State == PaperFoldState.PaperFoldStateDefault) {
					this.Animate (point, true); 
				} else if (this.State == PaperFoldState.PaperFoldStateLeftUnfolded) {
					var adjustedPoint = new PointF(point.X + this.LeftFoldView.Frame.Width, point.Y);
					this.Animate (adjustedPoint, true);
				} else if (this.State == PaperFoldState.PaperFoldStateRightUnfolded) {
					var adjustedPoint = new PointF(point.X + this.RightFoldView.Frame.Width, point.Y);
					this.Animate (adjustedPoint, true);
				}
			} else if (panGestureRecognizer.State == UIGestureRecognizerState.Ended || panGestureRecognizer.State == UIGestureRecognizerState.Cancelled) {
				var x = point.X;
				if(x >= 0.0){
					if((x >= PaperFoldConstants.LeftViewUnfoldThreshold * this.LeftFoldView.Frame.Width && this.State == PaperFoldState.PaperFoldStateDefault) ||
					   this.ContentView.Frame.X == this.LeftFoldView.Frame.Width){
						if (this.EnableLeftFoldDragging) {
							CreateTimer (this.InternalUnFoldLeftView);
							return;
						}
					}
				} else if (x < 0.0) {
					if((x <=-PaperFoldConstants.RightViewUnfoldThreshold * this.RightFoldView.Frame.Width && this.State == PaperFoldState.PaperFoldStateDefault) ||
					   this.ContentView.Frame.X ==-this.RightFoldView.Frame.Width){
						if (this.EnableRightFoldDragging) {
							CreateTimer (this.InternalUnFoldRightView);
							return;
						}
					}
				}

				CreateTimer (this.InternalRestoreView);
			}
		}

		private void ContentPannedVertically(UIPanGestureRecognizer panGestureRecognizer)
		{
			this.RightFoldView.Hidden = true;	
			this.LeftFoldView.Hidden = true;

			this.TopFoldView.Hidden = false;
			this.BottomFoldView.Hidden = false;

			var point = panGestureRecognizer.TranslationInView (this);

			if (panGestureRecognizer.State == UIGestureRecognizerState.Changed) {

				if (this.State == PaperFoldState.PaperFoldStateDefault) {
					this.Animate (point, true); 
				} else if (this.State == PaperFoldState.PaperFoldStateBottomUnfolded) {
					var adjustedPoint = new PointF(point.X, point.Y - this.BottomFoldView.Frame.Height);
					this.Animate (adjustedPoint, true);
				} else if (this.State == PaperFoldState.PaperFoldStateTopUnfolded) {
					var adjustedPoint = new PointF(point.X, point.Y + this.TopFoldView.Frame.Height);
					this.Animate (adjustedPoint, true);
				}
			} else if (panGestureRecognizer.State == UIGestureRecognizerState.Ended || panGestureRecognizer.State == UIGestureRecognizerState.Cancelled) {
				var y = point.Y;
				if(y <= 0.0){
					if((-y >= PaperFoldConstants.BottomViewUnfoldThreshold * this.BottomFoldView.Frame.Height && this.State == PaperFoldState.PaperFoldStateDefault) ||
					   -1*this.ContentView.Frame.Y == this.BottomFoldView.Frame.Height){
						if (this.EnableBottomFoldDragging) {
							CreateTimer (this.UnfoldBottomView);
							return;
						}
					}
				} else if (y > 0.0) {
					if((y >= PaperFoldConstants.TopViewUnfoldThreshold * this.TopFoldView.Frame.Height && this.State == PaperFoldState.PaperFoldStateDefault) ||
					   this.ContentView.Frame.Y == this.TopFoldView.Frame.Height){
						if (this.EnableTopFoldDragging) {
							CreateTimer (this.InternalUnFoldTopView);
							return;
						}
					}
				}

				this.CreateTimer (this.InternalRestoreView);
			}
		}

		private void InternalUnFoldBottomView()
		{
			this.TopFoldView.Hidden = false;
			this.BottomFoldView.Hidden = false;
			this.LeftFoldView.Hidden = true;
			this.RightFoldView.Hidden = true;

			this.PaperFoldInitialPanDirection = PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionVertical;

			var transform = this.ContentView.Transform;

			var y = transform.y0 - (this.BottomFoldView.Frame.Height + transform.y0)/4;

			transform = CGAffineTransform.MakeTranslation (0.0f, y);
			this.ContentView.Transform = transform;

			if(-y >= this.BottomFoldView.Frame.Height-2){
				this.AnimationTimer.Dispose ();
				transform = CGAffineTransform.MakeTranslation (0.0f, -1 * this.BottomFoldView.Frame.Height);
				this.ContentView.Transform = transform;

				if (this.LastState != PaperFoldState.PaperFoldStateBottomUnfolded) {
					this.Finish (PaperFoldState.PaperFoldStateBottomUnfolded);
				}
			}

			this.Animate (new PointF(0.0f, this.ContentView.Frame.Y), false);
		}

		private void InternalUnFoldLeftView()
		{
			this.TopFoldView.Hidden = true;
			this.BottomFoldView.Hidden = true;
			this.LeftFoldView.Hidden = false;
			this.RightFoldView.Hidden = false;

			var transform = this.ContentView.Transform;
			var x = transform.x0 + (this.LeftFoldView.Frame.Width - transform.x0) / 4;

			transform = CGAffineTransform.MakeTranslation (x, 0.0f);
			this.ContentView.Transform = transform;

			if (x >= this.LeftFoldView.Frame.Width - 2) {
				this.AnimationTimer.Dispose();
				transform = CGAffineTransform.MakeTranslation (this.LeftFoldView.Frame.Width, 0.0f);
				this.ContentView.Transform = transform;

				if (this.LastState != PaperFoldState.PaperFoldStateLeftUnfolded) {
					this.Finish (PaperFoldState.PaperFoldStateLeftUnfolded);
				}
			}
			this.Animate (new PointF(this.ContentView.Frame.X, 0.0f), false);
		}

		private void InternalUnFoldTopView()
		{
			this.TopFoldView.Hidden = false;
			this.BottomFoldView.Hidden = false;
			this.LeftFoldView.Hidden = true;
			this.RightFoldView.Hidden = true;

			this.PaperFoldInitialPanDirection = PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionVertical;

			var transform = this.ContentView.Transform;

			var y = transform.y0 + (this.TopFoldView.Frame.Height - transform.y0)/8;

			transform = CGAffineTransform.MakeTranslation (0.0f, y);
			this.ContentView.Transform = transform;

			if(y >= this.TopFoldView.Frame.Height-5){
				this.AnimationTimer.Dispose ();
				transform = CGAffineTransform.MakeTranslation (0.0f, this.TopFoldView.Frame.Height);
				this.ContentView.Transform = transform;

				if (this.LastState != PaperFoldState.PaperFoldStateTopUnfolded) {
					this.Finish (PaperFoldState.PaperFoldStateTopUnfolded);
				}
			}

			this.Animate (new PointF(0.0f, this.ContentView.Frame.Y), false);

		}

		private void InternalUnFoldRightView()
		{
			this.TopFoldView.Hidden = true;
			this.BottomFoldView.Hidden = true;
			this.LeftFoldView.Hidden = false;
			this.RightFoldView.Hidden = false;

			var transform = this.ContentView.Transform;
			var x = transform.x0 + (this.RightFoldView.Frame.Width + transform.x0) / 8;

			transform = CGAffineTransform.MakeTranslation (x, 0.0f);
			this.ContentView.Transform = transform;

			if (x <= -this.RightFoldView.Frame.Width + 5) {
				this.AnimationTimer.Dispose ();
				transform = CGAffineTransform.MakeTranslation (this.LeftFoldView.Frame.Width, 0.0f);
				this.ContentView.Transform = transform;

				if (this.LastState != PaperFoldState.PaperFoldStateRightUnfolded) {
					this.Finish (PaperFoldState.PaperFoldStateRightUnfolded);
				}
			}
			this.Animate (new PointF(this.ContentView.Frame.X, 0.0f), false);
		}


		private void InternalRestoreView()
		{
			if (this.PaperFoldInitialPanDirection == PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionHorizontal) {

				var transform = this.ContentView.Transform;
				var x = transform.x0 / 4 * 3;
				transform = CGAffineTransform.MakeTranslation(x, 0.0f);
				this.ContentView.Transform = transform;

				if ((x>- 0 && x < 5) || (x <=0 && x>-5)) {
					this.AnimationTimer.Dispose ();
					transform = CGAffineTransform.MakeTranslation(0.0f, 0.0f);
					this.ContentView.Transform = transform;
					this.Animate(new PointF(0.0f, 0.0f), false);

					if(this.LastState != PaperFoldState.PaperFoldStateDefault){
						this.Finish(PaperFoldState.PaperFoldStateDefault);
					}

					this.State = PaperFoldState.PaperFoldStateDefault;
				} else {
					this.Animate(new PointF(this.ContentView.Frame.X, 0.0f), false);
				}
			} else if (this.PaperFoldInitialPanDirection == PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionVertical) {

				var transform = this.ContentView.Transform;
				var y = transform.y0 / 4 * 3;
				transform = CGAffineTransform.MakeTranslation(0.0f, y);
				this.ContentView.Transform = transform;

				if ((y >= 0 && y < 5) || (y <=0 && y>-5)) {
					this.AnimationTimer.Dispose();
					transform = CGAffineTransform.MakeTranslation(0.0f, 0.0f);
					this.ContentView.Transform = transform;
					this.Animate(new PointF(0.0f, 0.0f), false);

					if(this.LastState != PaperFoldState.PaperFoldStateDefault){
						this.Finish(PaperFoldState.PaperFoldStateDefault);
					}

					this.State = PaperFoldState.PaperFoldStateDefault;
				} else {
					this.Animate(new PointF(0.0f, this.ContentView.Frame.Y), false);
				}
			}			
		}

		public void UnfoldRightView()
		{
			this.SetPaperFoldState (PaperFoldState.PaperFoldStateRightUnfolded);
		}

		public void UnfoldLeftView()
		{
			this.SetPaperFoldState (PaperFoldState.PaperFoldStateLeftUnfolded);
		}

		public void UnfoldTopView()
		{
			this.SetPaperFoldState (PaperFoldState.PaperFoldStateTopUnfolded);
		}

		public void UnfoldBottomView()
		{
			this.SetPaperFoldState (PaperFoldState.PaperFoldStateBottomUnfolded);
		}

		public void RestoreToCenter()
		{
			this.SetPaperFoldState (PaperFoldState.PaperFoldStateDefault);
		}

		private void Animate(PointF contentOffset, bool panned)
		{
			if (this.PaperFoldInitialPanDirection == PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionHorizontal) {
				var x = contentOffset.X;

				if (this.State != this.LastState) {
					this.LastState = this.State;
				}

				if (x >= 0.0f) {
					if (this.EnableLeftFoldDragging || !panned) {
						if (x>= this.LeftFoldView.Frame.Width) {
							if (this.LastState != PaperFoldState.PaperFoldStateLeftUnfolded) {
								this.Finish (PaperFoldState.PaperFoldStateLeftUnfolded);
							}
							this.LastState = this.State;
							this.State = PaperFoldState.PaperFoldStateLeftUnfolded;
							x = this.LeftFoldView.Frame.Width;
						}
						this.ContentView.Transform = CGAffineTransform.MakeTranslation (x, 0.0f);
						this.LeftFoldView.Unfold (x);

						if (this.DidOffset != null) {
							this.DidOffset (this, new PointF (x, 0.0f));
						}
					}
				} else if (x < 0.0f) {
					if(this.EnableRightFoldDragging || !panned) {
						var x1 = x;

						if (x1 <= this.RightFoldView.Frame.Width) {
							if(this.LastState != PaperFoldState.PaperFoldStateRightUnfolded){
								this.Finish (PaperFoldState.PaperFoldStateRightUnfolded);
							}
							this.LastState = this.State;
							this.State = PaperFoldState.PaperFoldStateRightUnfolded;
							x1 = -this.RightFoldView.Frame.Width;
						}

						this.ContentView.Transform = CGAffineTransform.MakeTranslation (x1, 0.0f);
						this.RightFoldView.Unfold (x);

						if (this.DidOffset != null) {
							this.DidOffset(this, new PointF(x, 0.0f));
						}
					}
				} else {
					this.ContentView.Transform = CGAffineTransform.MakeTranslation(0.0f, 0.0f);
					this.LeftFoldView.Unfold (-1*x);
					this.RightFoldView.Unfold (x);
					this.State = PaperFoldState.PaperFoldStateDefault;
					if (this.DidOffset != null) {
						this.DidOffset (this, new PointF (x, 0.0f));
					}
				}
			} else if (this.PaperFoldInitialPanDirection == PaperFoldInitialPanDirection.PaperFoldInitialPanDirectionVertical) {

				var y = contentOffset.Y;

				if (this.State != this.LastState) {
					this.LastState = this.State;
				}

				if (y < 0.0f) {
					if (this.EnableBottomFoldDragging || !panned) {

						if (-y >= this.BottomFoldView.Frame.Height) {
							this.LastState = this.State;
							this.State = PaperFoldState.PaperFoldStateBottomUnfolded;
							y = -this.BottomFoldView.Frame.Height;
						}

						this.ContentView.Transform = CGAffineTransform.MakeTranslation(0.0f, y);
						this.BottomFoldView.Unfold(y);

						if(this.DidOffset != null){
							this.DidOffset(this, new PointF(0.0f, y));
						}
					}
				} else if (y > 0.0f) {
					if (this.EnableTopFoldDragging || !panned) {
						var y1 = y;
						if(y1 >= this.TopFoldView.Frame.Height){
							this.LastState = this.State;
							this.State = PaperFoldState.PaperFoldStateTopUnfolded;
							y1 = this.TopFoldView.Frame.Height;
						}

						
						this.ContentView.Transform = CGAffineTransform.MakeTranslation(0.0f, y1);
						this.TopFoldView.Unfold(y);

						if(this.DidOffset != null){
							this.DidOffset(this, new PointF(0.0f, y));
						}

					}
				} else {
					this.ContentView.Transform = CGAffineTransform.MakeTranslation (0.0f, 0.0f);
					this.BottomFoldView.Unfold (y);
					this.TopFoldView.Unfold (y);
					this.State = PaperFoldState.PaperFoldStateDefault;

					if(this.DidOffset != null){
						this.DidOffset(this, new PointF(0.0f, y));
					}
				}

			}
		}

		private void SetPaperFoldState(PaperFoldState paperFoldState, bool animated, Action completionHandler)
		{
			this.completionHandler = completionHandler;
			this.SetPaperFoldState (paperFoldState, animated);
		}

		private void SetPaperFoldState(PaperFoldState paperFoldState, bool animated)
		{
			if (animated) {

				this.SetPaperFoldState (paperFoldState);

			} else {

				this.TopFoldView.Hidden = true;
				this.BottomFoldView.Hidden = true;
				this.LeftFoldView.Hidden = true;
				this.RightFoldView.Hidden = true;

				if(paperFoldState == PaperFoldState.PaperFoldStateDefault){
					var transform = CGAffineTransform.MakeTranslation (0.0f, 0.0f);
					this.ContentView.Transform = transform;

					if(this.LastState != PaperFoldState.PaperFoldStateDefault){
						this.Finish (PaperFoldState.PaperFoldStateDefault);
					}
				} else if (paperFoldState == PaperFoldState.PaperFoldStateLeftUnfolded) {
					this.LeftFoldView.Hidden = false;

					var transform = CGAffineTransform.MakeTranslation (this.LeftFoldView.Frame.Width, 0.0f);
					this.ContentView.Transform = transform;
					this.LeftFoldView.UnfoldWithoutAnimation ();

					if (this.LastState != PaperFoldState.PaperFoldStateLeftUnfolded) {
						this.Finish (PaperFoldState.PaperFoldStateLeftUnfolded);
					}

				} else if (paperFoldState == PaperFoldState.PaperFoldStateRightUnfolded) {
					this.RightFoldView.Hidden = false;

					var transform = CGAffineTransform.MakeTranslation (-this.RightFoldView.Frame.Width, 0.0f);
					this.ContentView.Transform = transform;
					this.RightFoldView.UnfoldWithoutAnimation ();

					if (this.LastState != PaperFoldState.PaperFoldStateRightUnfolded) {
						this.Finish (PaperFoldState.PaperFoldStateRightUnfolded);
					}

				}
				this.State = paperFoldState;
			}
		}

		private void SetPaperFoldState(PaperFoldState paperFoldState)
		{
			this.IsAutomatedFolding = true;

			Action viewAction;
			if (paperFoldState == PaperFoldState.PaperFoldStateDefault) {
				viewAction = this.InternalRestoreView;
			} else if (paperFoldState == PaperFoldState.PaperFoldStateLeftUnfolded) {
				viewAction = this.InternalUnFoldLeftView;
			} else if (paperFoldState == PaperFoldState.PaperFoldStateRightUnfolded) {
				viewAction = this.InternalUnFoldRightView;
			} else if (paperFoldState == PaperFoldState.PaperFoldStateTopUnfolded) {
				viewAction = this.InternalUnFoldTopView;
			} else if (paperFoldState == PaperFoldState.PaperFoldStateBottomUnfolded) {
				viewAction = this.InternalUnFoldBottomView;
			} else {
				viewAction = () => { Console.WriteLine("Something went badly..."); };
			}

			this.CreateTimer(viewAction);
		}

		private void Finish(PaperFoldState paperFoldState)
		{
			this.ShowDividerLines (false, true);

			if (this.completionHandler != null) {
				this.completionHandler ();
				this.completionHandler = null;
			} else if (this.DidFoldAutomaticallToState != null) {
				this.DidFoldAutomaticallToState (this, this.IsAutomatedFolding, paperFoldState);
			}
			this.IsAutomatedFolding = false;

			Console.WriteLine ("Finish Called = {0}", paperFoldState);
		}

		private void ShowDividerLines(bool show)
		{
			this.ShowDividerLines (show, false);
		}

		private void ShowDividerLines(bool show, bool animated)
		{
			if (this.showDividerLines == show) {
				return;
			}

			this.showDividerLines = show;

			var alpha = show ? 1 : 0;

			UIView.Animate (animated ? 0.25 : 0.0, () => {
				if(this.LeftDividerLine != null){
					this.LeftDividerLine.Alpha = alpha;
				}
				if(this.RightDividerLine != null){
					this.RightDividerLine.Alpha = alpha;
				}
				if(this.TopDividerLine != null){
					this.TopDividerLine.Alpha = alpha;
				}
				if(this.BottomDividerLine != null){
					this.BottomDividerLine.Alpha = alpha;
				}
			});
		}
	}
}

