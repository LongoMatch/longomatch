//
//  Copyright (C) 2017 FLUENDO
//
//
using System;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.ViewModel;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// Analysis state that does not contain a video player
	/// At this moment used only by the mobile application
	/// </summary>
	public class LightLiveProjectState : ScreenState<LMProjectAnalysisVM>
	{
		 public const string NAME = "LightLiveProject";

		/// <summary>
		/// Gets the name of the state
		/// </summary>
		/// <value>The name.</value>
		public override string Name {
			get {
				return NAME;
			}
		}

		public override async Task<bool> LoadState (dynamic data)
		{
			if (!await Initialize (data)) {
				return false;
			}

			try {
				ViewModel.Capturer.Run (ViewModel.CaptureSettings, ViewModel.Project.FileSet.First ().Model);
				return true;
			} catch {
				return false;
			}
		}

		/// <summary>
		/// Creates the view model.
		/// </summary>
		/// <param name="data">Data.</param>
		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new LMProjectAnalysisVM ();
			ViewModel.Project.Model = data.Project.Model;
			ViewModel.CaptureSettings = data.CaptureSettings;
			// FIXME: use this hack until the capturer uses a controller
			ViewModel.Capturer = (ICapturerBin)(Panel.GetType ().GetProperty ("Capturer").GetValue (Panel));
		}
	}
}
