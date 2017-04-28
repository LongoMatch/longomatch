//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using LongoMatch.Services.Interfaces;
using VAS.Core.Store;
using VAS.Services.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	public class LMDrawingToolVM : DrawingToolVM, ILMTeamTaggerVM
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
