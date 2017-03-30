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
using System.Threading.Tasks;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Core.Interfaces.GUI;
using VAS.Core.ViewModel;
using VAS.Services;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// A state for fake live analysis of projects.
	/// </summary>
	public class FakeLiveProjectAnalysisState : AnalysisStateBase
	{
		public const string NAME = "FakeLiveProjectAnalysis";

		public override string Name {
			get {
				return NAME;
			}
		}

		public override async Task<bool> LoadState (dynamic data)
		{
			LMProjectVM projectVM = data.Project;

			if (!InternalLoad (projectVM)) {
				return false;
			}

			projectVM.Model.UpdateEventTypesAndTimers ();
			return await Initialize (data);
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new LMProjectAnalysisVM ();
			ViewModel.Project = data.Project;
			ViewModel.CaptureSettings = data.CaptureSettings;
			ViewModel.VideoPlayer = new VideoPlayerVM ();
			// FIXME: use this hack until the capturer uses a controller
			ViewModel.Capturer = (ICapturerBin)(Panel.GetType ().GetProperty ("Capturer").GetValue (Panel));
		}

		protected override void CreateControllers (dynamic data)
		{
			var playerController = new VideoPlayerController ();
			playerController.SetViewModel (ViewModel.VideoPlayer);
			Controllers.Add (playerController);
		}
	}
}
