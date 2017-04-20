//
//  Copyright (C) 2016 Fluendo S.A.
using System.Threading.Tasks;
using LongoMatch.Core.Events;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Services.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	public class LMProjectAnalysisVM : ProjectAnalysisVM<LMProjectVM>, ICapturerBinDealer
	{
		public LMProjectAnalysisVM ()
		{
			TeamTagger = new LMTeamTaggerVM ();
			Project = new LMProjectVM ();
			SaveCommand = new Command (
				() => App.Current.EventsBroker.Publish (new SaveEvent<LMProjectVM> { Object = Project }),
				() => Project.Edited);
			ShowStatsCommand = new Command (
				() => App.Current.EventsBroker.Publish (new ShowProjectStatsEvent { Project = Project.Model }));
			CloseCommand = new Command (Close);
		}

		public new LMProjectVM Project {
			get {
				return base.Project;
			}
			set {
				base.Project = value;
				if (value != null) {
					ResetTeamTagger (base.Project);
				}
			}
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
		/// Gets the team tagger.
		/// </summary>
		/// <value>The team tagger.</value>
		public LMTeamTaggerVM TeamTagger {
			get;
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

		void ResetTeamTagger (LMProjectVM project)
		{
			TeamTagger.AwayTeam = project.AwayTeam;
			TeamTagger.HomeTeam = project.HomeTeam;
			TeamTagger.Background = project.Dashboard.Model != null ? project.Dashboard.FieldBackground : null;
		}
	}
}
