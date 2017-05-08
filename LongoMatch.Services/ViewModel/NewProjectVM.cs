//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Interfaces;

namespace LongoMatch.Services.ViewModel
{
	/// <summary>
	/// ViewModel used in NewProjectPanel View, it has a LMTeamTaggerVM that auto initializes
	/// </summary>
	public class NewProjectVM : LMProjectVM, ILMTeamTaggerVM
	{
		public NewProjectVM ()
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
			TeamTagger.ResetTeamTagger (this);
		}
	}
}
