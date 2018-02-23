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
using VAS.Services.Service;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// Analysis state that does not contain a video player
	/// At this moment used only by the mobile application
	/// </summary>
	public class LightLiveProjectState : ScreenState<LMProjectAnalysisVM>
	{
		public const string NAME = "LightLiveProject";
		VideoRecoderService service;

		/// <summary>
		/// Gets the name of the state
		/// </summary>
		/// <value>The name.</value>
		public override string Name {
			get {
				return NAME;
			}
		}

		public override Task<bool> UnloadState ()
		{
			service.Close ();
			return base.UnloadState ();
		}

		public override async Task<bool> LoadState (dynamic data)
		{
			if (!await Initialize (data)) {
				return false;
			}

			try {
				service.Run ();
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
			service = new VideoRecoderService ();
			// Fixme; these props should probably be passed view constructor as they are impepinable
			ViewModel.VideoRecorder = new VideoRecorderVM (data.CaptureSettings, ViewModel.Project.FileSet.First (),
														   App.Current.MultimediaToolkit.GetCapturer (), service);
			ViewModel.PeriodsNames = viewModel.Project.Model.Dashboard.GamePeriods.ToList ();
			ViewModel.Periods = viewModel.Project.Model.Periods.ToList ();
		}
	}
}
