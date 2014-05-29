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
using System.Linq;
using System.Collections.Generic;
using LongoMatch.Common;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store.Drawables;
using LongoMatch.Store.Templates;
using LongoMatch.Drawing.CanvasObject;
using LongoMatch.Store;
using LongoMatch.Handlers;

namespace LongoMatch.Drawing.Widgets
{
	public class TeamTagger: SelectionCanvas
	{
	
		public event PlayersPropertiesHandler PlayersSelectionChangedEvent;
		public event PlayersPropertiesHandler ShowMenuEvent;

		const int PLAYER_WIDTH = 60;
		const int PLAYER_HEIGHT = 60;
		const int BENCH_WIDTH = PLAYER_WIDTH * 2;
		TeamTemplate homeTeam, awayTeam;
		Image background;
		double currentWidth, currentHeight, scaleX, scaleY;
		Point offset;
		double backgroundWidth;
		MultiSelectionMode prevMode;
		bool inSubs;

		public TeamTagger (IWidget widget): base (widget)
		{
			Accuracy = 0;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			SubstitutionsMode = false;
			HomeColor = Common.PLAYER_UNSELECTED_COLOR;
			AwayColor = Common.PLAYER_UNSELECTED_COLOR;
		}
		
		public Color HomeColor {
			get;
			set;
		}
		
		public Color AwayColor {
			get;
			set;
		}
		
		public void LoadTeams (TeamTemplate homeTeam, TeamTemplate awayTeam, Image background) {
			this.homeTeam = homeTeam;
			this.awayTeam = awayTeam;
			this.background = background;
			Resize ();
		}
		
		public bool SubstitutionsMode {
			set {
				if (value) {
					prevMode = SelectionMode;
					SelectionMode = MultiSelectionMode.Multiple;
					ClearSelection ();
				} else {
					SelectionMode = prevMode;
				}
				inSubs = value;
			}
			get {
				return inSubs;
			}
		}
		public void Select (Player p) {
			ClearSelection ();
			if (p != null) {
				ICanvasObject co = Objects.LastOrDefault (pl => (pl as PlayerObject).Player == p);
				PlayerObject po = co as PlayerObject;
				if (po != null) {
					UpdateSelection (new Selection (po, SelectionPosition.All));
				}
			}
			widget.ReDraw ();
		}
		
		public void Reload () {
			Objects.Clear();
			if (homeTeam != null) {
				LoadTeam (homeTeam, Team.LOCAL);
			}
			if (awayTeam != null) {
				LoadTeam (awayTeam, Team.VISITOR);
			}
			widget.ReDraw ();
		}
		
		int NTeams {
			get {
				return awayTeam == null ? 1 : 2;
			}
		}
		
		
		void LoadTeam (TeamTemplate template, Team team) {
			int index = 0;
			double width, colWidth, offsetX;
			Color color;

			width = backgroundWidth / NTeams;
			colWidth = width / template.Formation.Length;
			if (team == Team.LOCAL) {
				offsetX = BENCH_WIDTH;
				color = HomeColor;
			} else {
				offsetX = currentWidth - BENCH_WIDTH;
				color = AwayColor;
			}
			
			/* Starting players */
			for (int col=0; col < template.Formation.Length; col ++) {
				double colX, rowHeight;
				
				if (template.Count == index)
					break;

				if (team == Team.LOCAL) {
					colX = offsetX + colWidth * col + colWidth / 2;
				} else {
					colX = offsetX - colWidth * col - colWidth / 2;
				}
				rowHeight = currentHeight / template.Formation[col];

				for (int row=0; row < template.Formation[col]; row ++) {
					Point p = new Point (colX, rowHeight * row + rowHeight / 2);
					PlayerObject po = new PlayerObject (template [index], p);
					po.Width = PLAYER_WIDTH;
					po.Height = PLAYER_HEIGHT;
					po.UnSelectedColor = color;
					Objects.Add (po);
					index ++;
					if (template.Count == index)
						break;
				}
			}
			
			for (int i = index; i < template.Count; i++) {
				PlayerObject po;
				double x, y;
				int reli = i - index;
				
				x = PLAYER_WIDTH * (reli % 2) + PLAYER_WIDTH / 2;
				y = PLAYER_HEIGHT * (reli / 2) + PLAYER_HEIGHT / 2;
				if (team == Team.VISITOR) {
					x += BENCH_WIDTH + backgroundWidth;
				}
				                     
				po = new PlayerObject (template [i], new Point (x, y));
				po.Width = PLAYER_WIDTH;
				po.Height = PLAYER_HEIGHT;
				po.UnSelectedColor = color;
				Objects.Add (po);
			}
		}
		
		void Resize () {
			currentWidth = widget.Width;
			currentHeight = widget.Height;
			backgroundWidth = currentWidth - BENCH_WIDTH * NTeams;
			
			if (background != null) {
				background.ScaleFactor ((int) backgroundWidth, (int) currentHeight,
				                        out scaleX, out scaleY, out offset);
			}
			Reload ();
		}
		
		protected override void SelectionChanged (List<Selection> selections) {
			List<Player> players;
			
			players = selections.Select (s => (s.Drawable as PlayerObject).Player).ToList();

			if (SubstitutionsMode) {
				bool subsDone = false;
				if (homeTeam != null) {
					List<Player> hplayers = players.Where (p => homeTeam.Contains (p)).ToList();
					if (hplayers.Count == 2) {
						homeTeam.Swap (hplayers[0], hplayers[1]);
						subsDone = true;
					}
				}
				if (awayTeam != null) {
					List<Player> aplayers = players.Where (p => awayTeam.Contains (p)).ToList();
					if (aplayers.Count == 2) {
						awayTeam.Swap (aplayers[0], aplayers[1]);
						subsDone = true;
					}
				}
				if (subsDone) {
					ClearSelection ();
					Reload ();
					widget.ReDraw ();
				}
			} else {
				if (PlayersSelectionChangedEvent != null) {
					PlayersSelectionChangedEvent (players);
				}
			}
		}
		
		protected override void StartMove (Selection sel) {
		}
		
		protected override void StopMove () {
		}

		protected override void ShowMenu (Point coords) {
			if (ShowMenuEvent != null && Selections.Count > 0){
				ShowMenuEvent (
					Selections.Select (s => (s.Drawable as PlayerObject).Player).ToList());
			}
		}
		
		protected override void SelectionMoved (Selection sel) {
		}

		protected override void HandleDraw (object context, Area area)
		{
			if (currentWidth != widget.Width || currentHeight != widget.Height) {
				Resize ();
			}
			
			tk.Context = context;
			tk.Begin ();

			/* Background */
			if (background != null) {
				tk.DrawImage (new Point (BENCH_WIDTH, 0), backgroundWidth, currentHeight,
				              background, false);
			}
			
			tk.End ();
			base.HandleDraw (context, area);
		}
	}
}

