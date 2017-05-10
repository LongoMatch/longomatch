//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Interfaces;
using VAS.Core.MVVMC;

namespace LongoMatch.Services.ViewModel
{
	/// <summary>
	/// ViewModel used in NewProjectPanel View, it has a LMTeamTaggerVM that auto initializes
	/// </summary>
	public class NewProjectVM : ViewModelBase, ILMTeamTaggerDealer
	{
		LMProjectVM project;

		public NewProjectVM ()
		{
			TeamTagger = new LMTeamTaggerVM ();
			TeamTagger.ShowSubstitutionButtons = false;
			TeamTagger.SubstitutionMode = true;
			Project = new LMProjectVM ();
		}

		/// <summary>
		/// Gets or sets the project.
		/// </summary>
		/// <value>The project.</value>
		public LMProjectVM Project {
			get {
				return project;
			}
			set {
				project = value;
				if (project != null) {
					TeamTagger.ResetTeamTagger (value);
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
	}
}
