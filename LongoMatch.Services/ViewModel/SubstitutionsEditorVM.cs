//
//  Copyright (C) 2017 FLUENDO S.A.
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using LongoMatch.Core.Store;
using LongoMatch.Services.Interfaces;
using VAS.Core.MVVMC;

namespace LongoMatch.Services.ViewModel
{
	public class SubstitutionsEditorVM : ViewModelBase<LMProject>, ILMTeamTaggerVM
	{
		public SubstitutionsEditorVM ()
		{
			TeamTagger = new LMTeamTaggerVM ();
			TeamTagger.ShowSubstitutionButtons = false;
		}

		public override LMProject Model {
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
		/// Gets or sets the timeline event.
		/// </summary>
		/// <value>The timeline event.</value>
		public LMTimelineEvent Play { get; set; }

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
