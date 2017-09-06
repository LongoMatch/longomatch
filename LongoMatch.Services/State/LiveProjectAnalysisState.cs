//
//  Copyright (C) 2016 Fluendo S.A.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.ViewModel;
using VAS.Services;
using VAS.Core;
using VAS.Services.Controller;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// A state for live analysis of projects.
	/// </summary>
	public class LiveProjectAnalysisState : AnalysisStateBase
	{
		public const string NAME = "LiveProjectAnalysis";

		public override string Name {
			get {
				return NAME;
			}
		}

		protected override Task<bool> LoadProject ()
		{
			try {
				ViewModel.Capturer.Run (ViewModel.CaptureSettings, ViewModel.Project.FileSet.First ().Model);
			} catch (Exception ex) {
				Log.Exception (ex);
				App.Current.Dialogs.ErrorMessage (ex.Message);
				return AsyncHelpers.Return (false);
			}
			return AsyncHelpers.Return (true);
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new LMProjectAnalysisVM ();
			ViewModel.Project.Model = data.Project.Model;
			ViewModel.CaptureSettings = data.CaptureSettings;
			ViewModel.VideoPlayer = new VideoPlayerVM ();
			ViewModel.VideoPlayer.ViewMode = PlayerViewOperationMode.LiveAnalysisReview;
			ViewModel.VideoPlayer.ShowDetachButton = false;
			ViewModel.VideoPlayer.ShowCenterPlayHeadButton = false;
			// FIXME: use this hack until the capturer uses a controller
			ViewModel.Capturer = (ICapturerBin)(Panel.GetType ().GetProperty ("Capturer").GetValue (Panel));
			CreateLimitation ();
		}

		protected override void CreateControllers (dynamic data)
		{
			var playerController = new VideoPlayerController ();
			playerController.SetViewModel (ViewModel.VideoPlayer);
			Controllers.Add (playerController);
			Controllers.Add (new CoreEventsController ());
		}
	}
}
