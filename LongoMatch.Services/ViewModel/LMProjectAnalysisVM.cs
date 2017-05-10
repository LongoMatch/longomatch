//
//  Copyright (C) 2016 Fluendo S.A.
using System.ComponentModel;
using System.Threading.Tasks;
using LongoMatch.Core.Events;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Interfaces;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Services.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	public class LMProjectAnalysisVM : ProjectAnalysisVM<LMProjectVM>, ICapturerBinDealer, ILMTeamTaggerDealer
	{
		public LMProjectAnalysisVM ()
		{
			TeamTagger = new LMTeamTaggerVM ();
			TeamTagger.Compact = true;
			TeamTagger.SelectionMode = MultiSelectionMode.Multiple;
			TeamTagger.ShowTeamsButtons = true;
			Project = new LMProjectVM ();
			SaveCommand = new Command (
				() => App.Current.EventsBroker.Publish (new SaveEvent<LMProjectVM> { Object = Project }),
				() => Project.Edited);
			ShowStatsCommand = new Command (
				() => App.Current.EventsBroker.Publish (new ShowProjectStatsEvent { Project = Project.Model }));
			CloseCommand = new Command (Close);
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			Project.PropertyChanged -= HandleProjectPropertyChanged;
		}

		public new LMProjectVM Project {
			get {
				return base.Project;
			}
			set {
				if (base.Project != null) {
					base.Project.PropertyChanged -= HandleProjectPropertyChanged;
				}
				base.Project = value;
				if (base.Project != null) {
					base.Project.PropertyChanged += HandleProjectPropertyChanged;
					TeamTagger.ResetTeamTagger (base.Project);
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

		void HandleProjectPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (Project.NeedsSync (e, nameof (Project.Model))) {
				TeamTagger.ResetTeamTagger (Project);
			}
		}
	}
}
