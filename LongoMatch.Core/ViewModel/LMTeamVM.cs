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
using VAS.Core.Events;
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
			FieldPlayersList = new RangeObservableCollection<LMPlayerVM> ();
			BenchPlayersList = new RangeObservableCollection<LMPlayerVM> ();
		}

		public LMTeam TypedModel {
			get {
				return (LMTeam)base.Model;
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
				return TypedModel.TeamName;
			}
			set {
				TypedModel.TeamName = value;
			}
		}

		/// <summary>
		/// Gets or sets the formation.
		/// </summary>
		/// <value>The formation.</value>
		public int [] Formation {
			get {
				return TypedModel.Formation;
			}
			set {
				TypedModel.Formation = value;
			}
		}

		/// <summary>
		/// Gets the main color of the team
		/// </summary>
		/// <value>The main color of the team.</value>
		public Color MainColor { 
			get {
				if (TypedModel.Colors.Count () >= 1) {
					return TypedModel.Colors [0];
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the secondary color of the team
		/// </summary>
		/// <value>The secondary color of the team.</value>
		public Color SecondaryColor {
			get { 
				if (TypedModel.Colors.Count () >= 2) {
					return TypedModel.Colors [1];
				}

				return null;
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
		/// Gets the preview of the first file in set or null if the set is empty.
		/// </summary>
		/// <value>The preview.</value>
		public Image Preview {
			get {
				return Model.Preview;
			}
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
		public RangeObservableCollection<LMPlayerVM> FieldPlayersList { get; private set; }

		/// <summary>
		/// Gets the bench players list.
		/// </summary>
		/// <value>The bench players list.</value>
		public RangeObservableCollection<LMPlayerVM> BenchPlayersList { get; private set; }

		/// <summary>
		/// Method to click a LMPlayerVM
		/// </summary>
		/// <param name="player">Player.</param>
		/// <param name="modifier">Modifier.</param>
		public void PlayerClick (LMPlayerVM player, ButtonModifier modifier)
		{
			App.Current.EventsBroker.Publish (new TagPlayerEvent {
				Player = player,
				Team = this,
				Modifier = modifier,
				Sender = player
			});
		}

		protected override void SyncLoadedModel ()
		{
			base.SyncLoadedModel ();
			UpdatePlayerList ();
		}

		void UpdatePlayerList ()
		{
			int count = Math.Min (TypedModel.StartingPlayers, CalledPlayersList.Count ());
			FieldPlayersList.Reset (CalledPlayersList.Take (count));
			BenchPlayersList.Reset (CalledPlayersList.Except (FieldPlayersList));
			foreach (var player in FieldPlayersList) {
				player.Playing = true;
			}
			foreach (var player in BenchPlayersList) {
				player.Playing = false;
			}
		}
	}
}