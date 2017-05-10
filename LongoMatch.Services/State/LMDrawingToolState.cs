//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Services.ViewModel;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	public class LMDrawingToolState : DrawingToolState
	{
		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new LMDrawingToolVM ();
			ViewModel.Project = data.project;
			ViewModel.TimelineEvent = data.timelineEvent;
			ViewModel.Frame = data.frame;
			ViewModel.Drawing = data.drawing;
			ViewModel.CameraConfig = data.cameraconfig;
		}
	}
}
