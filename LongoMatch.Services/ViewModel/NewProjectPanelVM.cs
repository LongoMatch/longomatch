//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	public class NewProjectPanelVM : LMProjectVM
	{
		public NewProjectPanelVM ()
		{
			TeamTagger = new LMTeamTaggerVM ();
		}

		public new LMProject Model {
			get {
				return base.Model;
			}
			set {
				base.Model = value;
				if (value != null) {
					ResetTeamTagger (value);
				}
			}
		}

		/// <summary>
		/// Gets the team tagger.
		/// </summary>
		/// <value>The team tagger.</value>
		public LMTeamTaggerVM TeamTagger {
			get;
		}

		void ResetTeamTagger (LMProject project)
		{
			TeamTagger.AwayTeam.Model = project.VisitorTeamTemplate;
			TeamTagger.HomeTeam.Model = project.LocalTeamTemplate;
			TeamTagger.Background = project.Dashboard?.FieldBackground;
		}
	}
}
