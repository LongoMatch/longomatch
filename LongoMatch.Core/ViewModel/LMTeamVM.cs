//
//  Copyright (C) 2016 Fluendo S.A.
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
using System;
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Common;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{

	/// <summary>
	/// ViewModel for a sports team.
	/// </summary>
	public class LMTeamVM : TeamVM
	{
		public LMTeamVM ()
		{
			SubViewModel = new LMPlayersCollectionVM ();
		}

		public new LMTeam Model {
			get {
				return base.Model as LMTeam;
			}
			set {
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets or sets the icon of the team.
		/// </summary>
		/// <value>The icon.</value>
		public override Image Icon {
			get {
				return Model.Shield;
			}
			set {
				Model.Shield = value;
			}
		}

		/// <summary>
		/// Gets or sets the display name used for a team
		/// </summary>
		/// <value>the display name</value>
		public string TeamName {
			get {
				return Model.TeamName;
			}
			set {
				Model.TeamName = value;
			}
		}

		/// <summary>
		/// Gets or sets the formation.
		/// </summary>
		/// <value>The formation.</value>
		public int [] Formation {
			get {
				return Model.Formation;
			}
			set {
				Model.Formation = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Core.ViewModel.LMTeamVM"/> template is in editor mode.
		/// </summary>
		/// <value><c>true</c> if template editor mode; otherwise, <c>false</c>.</value>
		public bool TemplateEditorMode {
			set;
			get;
		}

		/// <summary>
		/// Gets the playing players list.
		/// </summary>
		/// <value>The playing players list.</value>
		public IEnumerable<LMPlayerVM> CalledPlayersList {
			get {
				if (TemplateEditorMode) {
					return ViewModels.OfType<LMPlayerVM> ();
				}
				return ViewModels.OfType<LMPlayerVM> ().Where (p => p.Called);
			}
		}

		/// <summary>
		/// Gets the starting players list.
		/// </summary>
		/// <value>The starting players list.</value>
		public IEnumerable<LMPlayerVM> FieldPlayersList {
			get;
			set;
		}

		/// <summary>
		/// Gets the bench players list.
		/// </summary>
		/// <value>The bench players list.</value>
		public IEnumerable<LMPlayerVM> BenchPlayersList {
			get;
			set;
		}

		protected override void SyncLoadedModel ()
		{
			base.SyncLoadedModel ();
			UpdatePlayerList ();
		}

		void UpdatePlayerList ()
		{
			int count = Math.Min (Model.StartingPlayers, CalledPlayersList.Count ());
			FieldPlayersList = CalledPlayersList.Take (count);
			BenchPlayersList = CalledPlayersList.Except (FieldPlayersList);
			foreach (var player in FieldPlayersList) {
				player.Playing = true;
			}
			foreach (var player in BenchPlayersList) {
				player.Playing = false;
			}
		}
	}
}