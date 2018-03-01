//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Drawing;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.CanvasObjects.Teams
{
	public class FieldObject : FixedSizeCanvasObject, ICanvasSelectableObject, ICanvasObjectView<LMTeamTaggerVM>
	{
		const int MIN_PLAYERS_ROWS = 5;

		int [] homeFormation;
		int [] awayFormation;
		LMTeamTaggerVM viewModel;

		public FieldObject ()
		{
			Position = new Point (0, 0);
			HomePlayingPlayers = new List<LMPlayerView> ();
			AwayPlayingPlayers = new List<LMPlayerView> ();
		}

		protected override void DisposeManagedResources ()
		{
			ClearPlayers ();
		}

		public bool SubstitutionMode {
			get;
			set;
		}

		List<LMPlayerView> HomePlayingPlayers { get; set; }

		List<LMPlayerView> AwayPlayingPlayers { get; set; }

		public Image Background { get; set; }

		public int PlayerSize { get; set; }

		public bool FieldResizable { get; set; }

		public int ColumnSize {
			get {
				int width, optWidth, optHeight, count = 0, max = 0;

				width = (int)(Width / NTeams);
				if (ViewModel == null) {
					return 0;
				}else if (ViewModel.HomeTeam != null && ViewModel.AwayTeam != null) {
					count = Math.Max (ViewModel.HomeTeam.Formation.Count (), ViewModel.AwayTeam.Formation.Count ());
					max = Math.Max (ViewModel.HomeTeam.Formation.Max (), ViewModel.AwayTeam.Formation.Max ());
				} else if (ViewModel.HomeTeam != null) {
					count = ViewModel.HomeTeam.Formation.Count ();
					max = ViewModel.HomeTeam.Formation.Max ();
				} else if (ViewModel.AwayTeam != null) {
					count = ViewModel.AwayTeam.Formation.Count ();
					max = ViewModel.AwayTeam.Formation.Max ();
				}
				optWidth = width / count;
				optHeight = (int)(Height / Math.Max (MIN_PLAYERS_ROWS, max));
				return Math.Min (optWidth, optHeight);
			}
		}

		public void LoadTeams ()
		{
			Background = ViewModel.Background;
			LoadField ();
			NTeams = GetNumTeams ();
			Update ();
		}

		public void Update ()
		{
			if (FieldResizable) {
				PlayerSize = ColumnSize * 90 / 100;
			}

			if (homeFormation != null) {
				UpdateTeam (HomePlayingPlayers, homeFormation, TeamType.LOCAL);
			}
			if (awayFormation != null) {
				UpdateTeam (AwayPlayingPlayers, awayFormation, TeamType.VISITOR);
			}

			ReDraw ();
		}

		public int NTeams { get; set; }

		public LMTeamTaggerVM ViewModel { 
			get { return viewModel; }
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					LoadTeams ();
					viewModel.PropertyChanged += HandlePropertyChanged;
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMTeamTaggerVM)viewModel;
		}

		void UpdateTeam (List<LMPlayerView> players, int [] formation, TeamType team)
		{
			int index = 0;
			double width, colWidth, offsetX;
			Color color;

			width = (Width / NTeams);
			colWidth = width / formation.Length;
			if (team == TeamType.LOCAL) {
				color = App.Current.Style.HomeTeamColor;
				offsetX = 0;
			} else {
				color = App.Current.Style.AwayTeamColor;
				offsetX = Width;
			}

			/* Columns */
			for (int col = 0; col < formation.Length; col++) {
				double colX, rowHeight;

				if (players.Count == index)
					break;

				if (team == TeamType.LOCAL) {
					colX = offsetX + colWidth * col + colWidth / 2;
				} else {
					colX = offsetX - colWidth * col - colWidth / 2;
				}
				rowHeight = Height / formation [col];

				for (int row = 0; row < formation [col]; row++) {
					double rowY;
					LMPlayerView po = players [index];

					if (team == TeamType.LOCAL) {
						rowY = rowHeight * row + rowHeight / 2;
					} else {
						rowY = Height - (rowHeight * row + rowHeight / 2);
					}

					po.Size = PlayerSize;
					po.Center = new Point (colX, rowY);
					index++;
					if (players.Count == index)
						break;
				}
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			tk.Begin ();
			tk.TranslateAndScale (Position, new Point (1, 1));
			if (Background != null) {
				tk.DrawImage (new Point(0, 0), Width, Height, Background, ScaleMode.AspectFit);
			}

			if (HomePlayingPlayers != null) {
				foreach (LMPlayerView po in HomePlayingPlayers) {
					po.SubstitutionMode = SubstitutionMode;
					po.Size = PlayerSize;
					po.Draw (tk, area);
				}
			}
			if (AwayPlayingPlayers != null) {
				foreach (LMPlayerView po in AwayPlayingPlayers) {
					po.SubstitutionMode = SubstitutionMode;
					po.Size = PlayerSize;
					po.Draw (tk, area);
				}
			}
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion)
		{
			Selection selection = null;

			point = VASDrawing.Utils.ToUserCoords (point, Position, 1, 1);

			if (HomePlayingPlayers != null) {
				foreach (LMPlayerView po in HomePlayingPlayers) {
					selection = po.GetSelection (point, precision);
					if (selection != null)
						break;
				}
			}
			if (selection == null && AwayPlayingPlayers != null) {
				foreach (LMPlayerView po in AwayPlayingPlayers) {
					selection = po.GetSelection (point, precision);
					if (selection != null)
						break;
				}
			}
			return selection;
		}

		public void Move (Selection s, Point p, Point start)
		{
		}

		void LoadField () {
			homeFormation = ViewModel.HomeTeam?.Formation;
			awayFormation = ViewModel.AwayTeam?.Formation;
			ViewModel.HomeTeam?.TypedModel.UpdateColors ();
			ViewModel.AwayTeam?.TypedModel.UpdateColors ();
			ClearPlayers ();
			AddPlayers ();
			Update ();
		}

		void HandlePlayerClickedEvent (ICanvasObject co)
		{
			LMPlayerView player = co as LMPlayerView;
			ViewModel.PlayerClick (player.ViewModel, ButtonModifier.None); //)modifier); Store the modifier in the canvas object
			ReDraw ();
		}

		void AddPlayers () {
			if (ViewModel.HomeTeam != null) {
				foreach (var player in ViewModel.HomeTeam.FieldPlayersList) {
					var playerView = new LMPlayerView { Team = TeamType.LOCAL, ViewModel = player };
					HomePlayingPlayers.Add (playerView);
					playerView.ClickedEvent += HandlePlayerClickedEvent;
				}
			}

			if ((ViewModel.AwayTeam != null)) {
				foreach (var player in ViewModel.AwayTeam.FieldPlayersList) {
					var playerView = new LMPlayerView { Team = TeamType.VISITOR, ViewModel = player };
					AwayPlayingPlayers.Add (playerView);
					playerView.ClickedEvent += HandlePlayerClickedEvent;
				}
			}
		}

		void ClearPlayers ()
		{
			foreach (var player in HomePlayingPlayers) {
				player.ClickedEvent -= HandlePlayerClickedEvent;
				player.Dispose ();
			}

			foreach (var player in AwayPlayingPlayers) {
				player.ClickedEvent -= HandlePlayerClickedEvent;
				player.Dispose ();
			}

			HomePlayingPlayers.Clear ();
			AwayPlayingPlayers.Clear ();
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.HomeTeam != null) {
				if (ViewModel.NeedsSync (e.PropertyName, $"Collection_{nameof (ViewModel.HomeTeam.FieldPlayersList)}", sender, ViewModel.HomeTeam)) {
					LoadField ();
				}
			}

			if (ViewModel.AwayTeam != null) {
				if (ViewModel.NeedsSync (e.PropertyName, $"Collection_{nameof (ViewModel.AwayTeam.FieldPlayersList)}", sender, ViewModel.AwayTeam)) {
					LoadField ();
				}
			}
		}

		int GetNumTeams () {
			int nTeams = 0;
			if (ViewModel.HomeTeam != null) {
				nTeams++;
			}
			if (ViewModel.AwayTeam != null) {
				nTeams++;
			}

			return nTeams;
		}
	}
}

