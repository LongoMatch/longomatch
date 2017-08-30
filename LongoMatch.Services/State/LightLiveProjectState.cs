//
//  Copyright (C) 2017 FLUENDO
//
//
using LongoMatch.Services.ViewModel;
using VAS.Core.Interfaces.GUI;
using VAS.Core.ViewModel;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// Analysis state that does not contain a video player
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

		/// <summary>
		/// Creates the view model.
		/// </summary>
		/// <param name="data">Data.</param>
		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new LMProjectAnalysisVM ();
			ViewModel.Project.Model = data.Project.Model;
			ViewModel.CaptureSettings = data.CaptureSettings;
			ViewModel.VideoPlayer = new VideoPlayerVM ();
			// FIXME: use this hack until the capturer uses a controller
			ViewModel.Capturer = (ICapturerBin)(Panel.GetType ().GetProperty ("Capturer").GetValue (Panel));
		}
	}
}
