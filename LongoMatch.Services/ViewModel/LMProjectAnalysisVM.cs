//
//  Copyright (C) 2016 Fluendo S.A.
using System.Threading.Tasks;
using LongoMatch.Core.Events;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Services.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	public class LMProjectAnalysisVM : ProjectAnalysisVM<LMProjectVM>
	{
		public LMProjectAnalysisVM ()
		{
			Project = new LMProjectVM ();
			SaveCommand = new Command (
				() => App.Current.EventsBroker.Publish (new SaveEvent<LMProjectVM> { Object = Project }),
				() => Project.Edited);
			ShowStatsCommand = new Command (
				() => App.Current.EventsBroker.Publish (new ShowProjectStatsEvent { Project = Project.Model }));
			CloseCommand = new Command (Close);
		}

		public ICapturerBin Capturer {
			get;
			set;
		}

		public CaptureSettings CaptureSettings {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the command that displays the view of the statistics
		/// </summary>
		/// <value>The show statistics command.</value>
		public Command ShowStatsCommand {
			get;
			set;
		}

		async Task Close ()
		{
			await App.Current.EventsBroker.Publish (new CloseEvent<LMProjectVM> { Object = Project });
		}
	}
}
