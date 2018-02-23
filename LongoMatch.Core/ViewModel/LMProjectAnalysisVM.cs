//
//  Copyright (C) 2016 Fluendo S.A.
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Events;
using LongoMatch.Core.Interfaces;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{
	public class LMProjectAnalysisVM : ProjectAnalysisVM<LMProjectVM>, IVideoRecorderDealer, ILMTeamTaggerDealer
	{
		public LMProjectAnalysisVM ()
		{
			TeamTagger = new LMTeamTaggerVM ();
			TeamTagger.Compact = true;
			TeamTagger.SelectionMode = MultiSelectionMode.Multiple;
			TeamTagger.ShowTeamsButtons = true;
			Project = new LMProjectVM ();
			ShowStatsCommand = new Command ();
			SaveCommand = new Command ();
			CloseCommand = new AsyncCommand ();
			ShowWarningLimitation = new LimitationCommand (VASFeature.OpenMultiCamera.ToString ());
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			Project.PropertyChanged -= HandleProjectPropertyChanged;
		}

		public override LMProjectVM Project {
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

		public VideoRecorderVM VideoRecorder {
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

		/// <summary>
		/// Gets or sets the show warning limitation command.
		/// </summary>
		/// <value>The show warning limitation command.</value>
		public Command ShowWarningLimitation { get; set; }

		void HandleProjectPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (Project.NeedsSync (e, nameof (Project.Model))) {
				TeamTagger.ResetTeamTagger (Project);
			}
		}
	}
}
