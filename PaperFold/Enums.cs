using System;

namespace PaperFold
{
	/*
	ifndef PaperFold_PaperFoldConstants_h
	define PaperFold_PaperFoldConstants_h

	define FOLDVIEW_TAG 1000
	define kLeftViewUnfoldThreshold 0.3
	define kRightViewUnfoldThreshold 0.3
	define kTopViewUnfoldThreshold 0.3
	define kBottomViewUnfoldThreshold 0.3
	define kEdgeScrollWidth 40.0
*/

	public static class PaperFoldConstants{

		public const int FoldViewTag = 1000;
		public const float LeftViewUnfoldThreshold = 0.3f;
		public const float RightViewUnfoldThreshold = 0.3f;
		public const float TopViewUnfoldThreshold = 0.3f;
		public const float BottomViewUnfoldThreshold = 0.3f;
		public const float EdgeScrollWidth = 40.0f;

	}

	public enum FoldState{
		FoldStateClosed = 0,
		FoldStateOpened = 1,
		FoldStateTransition = 2
	}

	public enum FoldDirection{
		FoldDirectionHorizontalRightToLeft = 0,
		FoldDirectionHorizontalLeftToRight = 1,
		FoldDirectionVertical = 2
	}

	public enum PaperFoldState{
		PaperFoldStateDefault = 0,
		PaperFoldStateLeftUnfolded = 1,
		PaperFoldStateRightUnfolded = 2,
		PaperFoldStateTopUnfolded = 3,
		PaperFoldStateBottomUnfolded = 4,
		PaperFoldStateTransition = 5
	}

	public enum PaperFoldInitialPanDirection{
		PaperFoldInitialPanDirectionHorizontal = 0,
		PaperFoldInitialPanDirectionVertical = 1
	}

}

