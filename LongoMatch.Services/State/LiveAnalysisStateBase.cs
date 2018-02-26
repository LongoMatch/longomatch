//
//  Copyright (C) 2018 Fluendo S.A.
using System;
using VAS.Core.Interfaces.Services;

namespace LongoMatch.Services.State
{
	public abstract class LiveAnalysisStateBase : AnalysisStateBase
	{

		protected override void SetCommands ()
		{
			base.SetCommands ();
			GetService<IVideoRecorderService> ().SetDefaultCallbacks (ViewModel.VideoRecorder);
		}
	}
}
