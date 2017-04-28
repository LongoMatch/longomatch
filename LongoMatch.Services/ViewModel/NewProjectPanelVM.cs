//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Interfaces;

namespace LongoMatch.Services.ViewModel
{
	public class NewProjectPanelVM : LMProjectVM, ILMTeamTaggerVM
	{
		public NewProjectPanelVM ()
		{
			TeamTagger = new LMTeamTaggerVM ();
			TeamTagger.ShowSubstitutionButtons = false;
			TeamTagger.SubstitutionMode = true;
		}

		/// <summary>
		/// Gets the team tagger.
		/// </summary>
		/// <value>The team tagger.</value>
		public LMTeamTaggerVM TeamTagger {
			get;
		}

		protected override void SyncLoadedModel ()
		{
			base.SyncLoadedModel ();
			ResetTeamTagger ();
		}

		void ResetTeamTagger ()
		{
			TeamTagger.AwayTeam.Model = Model.VisitorTeamTemplate;
			TeamTagger.HomeTeam.Model = Model.LocalTeamTemplate;
			TeamTagger.Background = Model.Dashboard?.FieldBackground;
		}
	}
}
