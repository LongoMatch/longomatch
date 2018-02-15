//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using LongoMatch.Core.Interfaces;
using VAS.Core.Store;
using VAS.Services.ViewModel;
using LongoMatch.Core.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	public class LMDrawingToolVM : DrawingToolVM, ILMTeamTaggerDealer
	{
		public LMDrawingToolVM ()
		{
			TeamTagger = new LMTeamTaggerVM ();
			TeamTagger.ShowSubstitutionButtons = false;
			ResetTeamTagger ();
		}

		public override Project Project {
			get {
				return base.Project;
			}
			set {
				base.Project = value;
				//FIXME: this should use the TeamTagger.ResetTeamTagger(LMProjectVM)
				// when having a LMProjectVM instead of the Model
				ResetTeamTagger ();
			}
		}

		/// <summary>
		/// Gets or sets the team tagger.
		/// </summary>
		/// <value>The team tagger.</value>
		public LMTeamTaggerVM TeamTagger {
			get;
			protected set;
		}

		void ResetTeamTagger ()
		{
			var project = Project as LMProject;
			if (project != null) {
				TeamTagger.AwayTeam.Model = project.VisitorTeamTemplate;
				TeamTagger.HomeTeam.Model = project.LocalTeamTemplate;
				TeamTagger.Background = project.Dashboard?.FieldBackground;
			}
		}
	}
}
