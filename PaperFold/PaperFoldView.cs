using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace PaperFold
{
	public class PaperFoldView : UIView
	{

		private UIPanGestureRecognizer panGestureRecognizer;

		// indicate if the divider line should be visible
		private bool showDividerLines;

		// main content view
		public TouchThroughUIView ContentView {
			get;
			set;
		}

		// timer to animate folds after gesture ended
		// manual animation with NSTimer is required to sync the offset of the contentView, with the folding of views
		public NSTimer AnimationTimer {
			get;
			set;
		}

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

			var panGestureRecognizer = new UIPanGestureRecognizer (this.ContentViewPanned);
			this.ContentView.AddGestureRecognizer (panGestureRecognizer);

			this.LastState = this.State = PaperFoldState.PaperFoldStateDefault;

			this.EnableLeftFoldDragging = 
				this.EnableRightFoldDragging = 
					this.EnableTopFoldDragging = 
					this.EnableBottomFoldDragging = false;

			this.RestrictedDraggingRect = RectangleF.Empty;
			this.showDividerLines = false;
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
			var leftFoldViewFrame = this.LeftFoldView.Frame;
			leftFoldViewFrame.Height = frame.Height;
			this.LeftFoldView.Frame = leftFoldViewFrame;

			var rightFoldViewFrame = this.RightFoldView.Frame;
			rightFoldViewFrame.Height = frame.Height;
			this.RightFoldView.Frame = rightFoldViewFrame;
		}

		private float DisplacementOfMultiFoldView(UIView multiFoldView)
		{
			return 0.0f;
		}



		private void ContentViewPanned(UIPanGestureRecognizer panGestureRecognizer)
		{
			if (this.AnimationTimer.IsValid) {
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
							this.AnimationTimer = NSTimer.CreateTimer (TimeSpan.FromSeconds(0.01), this.UnFoldLeftView);
							return;
						}
					}
				} else if (x < 0.0) {
					if((x <=-PaperFoldConstants.RightViewUnfoldThreshold * this.RightFoldView.Frame.Width && this.State == PaperFoldState.PaperFoldStateDefault) ||
					   this.ContentView.Frame.X ==-this.RightFoldView.Frame.Width){
						if (this.EnableRightFoldDragging) {
							this.AnimationTimer = NSTimer.CreateTimer (TimeSpan.FromSeconds(0.01), this.UnFoldRightView);
							return;
						}
					}
				}

				this.AnimationTimer = NSTimer.CreateTimer (TimeSpan.FromSeconds(0.01), this.RestoreView);
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
							this.AnimationTimer = NSTimer.CreateTimer (TimeSpan.FromSeconds(0.01), this.UnFoldBottomView);
							return;
						}
					}
				} else if (y > 0.0) {
					if((y >= PaperFoldConstants.TopViewUnfoldThreshold * this.TopFoldView.Frame.Height && this.State == PaperFoldState.PaperFoldStateDefault) ||
					   this.ContentView.Frame.Y == this.TopFoldView.Frame.Height){
						if (this.EnableTopFoldDragging) {
							this.AnimationTimer = NSTimer.CreateTimer (TimeSpan.FromSeconds(0.01), this.UnFoldTopView);
							return;
						}
					}
				}

				this.AnimationTimer = NSTimer.CreateTimer (TimeSpan.FromSeconds(0.01), this.RestoreView);
			}
		}
		
		private void UnFoldLeftView()
		{
		}

		private void UnFoldRightView()
		{
		}

		private void UnFoldBottomView()
		{
		}

		private void UnFoldTopView()
		{
		}

		private void RestoreView()
		{
		}

		private void Animate(PointF contentOffset, bool panned)
		{
			/*
			 if (self.paperFoldInitialPanDirection==PaperFoldInitialPanDirectionHorizontal)
    {
        float x = point.x;
        // if offset to the right, show the left view
        // if offset to the left, show the right multi-fold view
        
        if (self.state!=self.lastState) self.lastState = self.state;
        
        if (x>0.0)
        {
            if (self.enableLeftFoldDragging || !panned)
            {
                // set the limit of the right offset
                if (x>=self.leftFoldView.frame.size.width)
                {
                    if (self.lastState != PaperFoldStateLeftUnfolded) {
						[self finishForState:PaperFoldStateLeftUnfolded];
					}
                    self.lastState = self.state;
                    self.state = PaperFoldStateLeftUnfolded;
                    x = self.leftFoldView.frame.size.width;
                }
                [self.contentView setTransform:CGAffineTransformMakeTranslation(x, 0)];
                //[self.leftFoldView unfoldWithParentOffset:-1*x];
                [self.leftFoldView unfoldWithParentOffset:x];
                
                if ([self.delegate respondsToSelector:@selector(paperFoldView:viewDidOffset:)])
                {
                    [self.delegate paperFoldView:self viewDidOffset:CGPointMake(x,0)];
                }
            }
        }
        else if (x<0.0)
        {
            if (self.enableRightFoldDragging || !panned)
            {
                // set the limit of the left offset
                // original x value not changed, to be sent to multi-fold view
                float x1 = x;
                if (x1<=-self.rightFoldView.frame.size.width)
                {
					if (self.lastState != PaperFoldStateRightUnfolded) {
						[self finishForState:PaperFoldStateRightUnfolded];
					}
                    self.lastState = self.state;
                    self.state = PaperFoldStateRightUnfolded;
                    x1 = -self.rightFoldView.frame.size.width;
                }
                [self.contentView setTransform:CGAffineTransformMakeTranslation(x1, 0)];
                [self.rightFoldView unfoldWithParentOffset:x];
                
                if ([self.delegate respondsToSelector:@selector(paperFoldView:viewDidOffset:)])
                {
                    [self.delegate paperFoldView:self viewDidOffset:CGPointMake(x,0)];
                }
            }
        }
        else
        {
            [self.contentView setTransform:CGAffineTransformMakeTranslation(0, 0)];
            [self.leftFoldView unfoldWithParentOffset:-1*x];
            [self.rightFoldView unfoldWithParentOffset:x];
            self.state = PaperFoldStateDefault;
            
            if ([self.delegate respondsToSelector:@selector(paperFoldView:viewDidOffset:)])
            {
                [self.delegate paperFoldView:self viewDidOffset:CGPointMake(x,0)];
            }
        }
    }
    else if (self.paperFoldInitialPanDirection==PaperFoldInitialPanDirectionVertical)
    {
        float y = point.y;
        // if offset to the top, show the bottom view
        // if offset to the bottom, show the top multi-fold view
        
        if (self.state!=self.lastState) self.lastState = self.state;
        
        if (y<0.0)
        {
            if (self.enableBottomFoldDragging || !panned)
            {
                // set the limit of the top offset
                if (-y>=self.bottomFoldView.frame.size.height)
                {
                    self.lastState = self.state;
                    self.state = PaperFoldStateBottomUnfolded;
                    y = -self.bottomFoldView.frame.size.height;
                }
                [self.contentView setTransform:CGAffineTransformMakeTranslation(0, y)];
                [self.bottomFoldView unfoldWithParentOffset:y];
                
                if ([self.delegate respondsToSelector:@selector(paperFoldView:viewDidOffset:)])
                {
                    [self.delegate paperFoldView:self viewDidOffset:CGPointMake(0,y)];
                }
            }
        }
        else if (y>0.0)
        {
            if (self.enableTopFoldDragging || !panned)
            {
                // set the limit of the bottom offset
                // original y value not changed, to be sent to multi-fold view
                float y1 = y;
                if (y1>=self.topFoldView.frame.size.height)
                {
                    self.lastState = self.state;
                    self.state = PaperFoldStateTopUnfolded;
                    y1 = self.topFoldView.frame.size.height;
                }
                [self.contentView setTransform:CGAffineTransformMakeTranslation(0, y1)];
                [self.topFoldView unfoldWithParentOffset:y];
                
                if ([self.delegate respondsToSelector:@selector(paperFoldView:viewDidOffset:)])
                {
                    [self.delegate paperFoldView:self viewDidOffset:CGPointMake(0,y)];
                }
            }
        }
        else
        {
            
            [self.contentView setTransform:CGAffineTransformMakeTranslation(0, 0)];
            [self.bottomFoldView unfoldWithParentOffset:y];
            [self.topFoldView unfoldWithParentOffset:y];
            self.state = PaperFoldStateDefault;
            
            if ([self.delegate respondsToSelector:@selector(paperFoldView:viewDidOffset:)])
            {
                [self.delegate paperFoldView:self viewDidOffset:CGPointMake(0,y)];
            }
        }
    }*/
		}

		private void ShowDividerLines(bool show)
		{
			this.ShowDividerLines (show, false);
		}

		private void ShowDividerLines(bool show, bool animated)
		{

		}
	}
}

