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

		TeamTemplate homeTeam, awayTeam;
		Image background;
		double currentWidth, currentHeight, scaleX, scaleY;
		double backgroundWidth;
		Point offset;
		MultiSelectionMode prevMode;
		PlayersIconSize iconSize;
		bool inSubs;

		public TeamTagger (IWidget widget): base (widget)
		{
			Accuracy = 0;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			SubstitutionsMode = false;
			HomeColor = Constants.PLAYER_UNSELECTED_COLOR;
			AwayColor = Constants.PLAYER_UNSELECTED_COLOR;
			PlayersPorRowInBench = 2;
			BenchIconSize = PlayersIconSize.Small;
		}
		
		public Color HomeColor {
			get;
			set;
		}
		
		public Color AwayColor {
			get;
			set;
		}
		
		public int PlayersPorRowInBench {
			get;
			set;
		}
		
		public PlayersIconSize BenchIconSize {
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

		public void Select (List<Player> players) {
			ClearSelection ();
			if (players != null) {
				foreach (Player p in players) {
					SelectPlayer (p, false);
				}
			}
			widget.ReDraw ();
		}

		public void Select (Player p) {
			ClearSelection ();
			SelectPlayer (p, false);
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
		
		int BenchWidth {
			get {
				return PlayersPorRowInBench * (int)BenchIconSize;
			}
		}
		
		void SelectPlayer (Player p, bool notify=true)
		{
			if (p != null) {
				ICanvasObject co = Objects.LastOrDefault (pl => (pl as PlayerObject).Player.ID == p.ID);
				PlayerObject po = co as PlayerObject;
				if (po != null) {
					UpdateSelection (new Selection (po, SelectionPosition.All), notify);
				}
			}
		}
		
		PlayersIconSize BestIconSize (int[] formation) {
			double width = backgroundWidth / NTeams;
			double optWidth = width / formation.Count();
			double optHeight = currentHeight / formation.Max();
			double size = Math.Min (optWidth, optHeight);

			if (size < (int) PlayersIconSize.Small) {
				return PlayersIconSize.Smallest;
			} else if (size < (int) PlayersIconSize.Medium) {
				return PlayersIconSize.Small;
			} else if (size < (int) PlayersIconSize.Large) {
				return PlayersIconSize.Medium;
			} else if (size < (int) PlayersIconSize.ExtraLarge) {
				return PlayersIconSize.Large;
			} else {
				return PlayersIconSize.ExtraLarge;
			}
		}

		void LoadTeam (TeamTemplate template, Team team) {
			int index = 0;
			double width, colWidth, offsetX;
			Color color;
			PlayersIconSize size = BestIconSize (template.Formation);

			width = backgroundWidth / NTeams;
			colWidth = width / template.Formation.Length;
			if (team == Team.LOCAL) {
				offsetX = BenchWidth;
				color = HomeColor;
			} else {
				offsetX = currentWidth - BenchWidth;
				color = AwayColor;
			}
			
			/* Starting players */
			for (int col=0; col < template.Formation.Length; col ++) {
				double colX, rowHeight;
				
				if (template.List.Count == index)
					break;

				if (team == Team.LOCAL) {
					colX = offsetX + colWidth * col + colWidth / 2;
				} else {
					colX = offsetX - colWidth * col - colWidth / 2;
				}
				rowHeight = currentHeight / template.Formation[col];

				for (int row=0; row < template.Formation[col]; row ++) {
					Point p = new Point (colX, rowHeight * row + rowHeight / 2);
					PlayerObject po = new PlayerObject (template.List [index], p);
					po.IconSize = size;
					po.UnSelectedColor = color;
					Objects.Add (po);
					index ++;
					if (template.List.Count == index)
						break;
				}
			}
			
			/* Substitution players */
			for (int i = index; i < template.List.Count; i++) {
				PlayerObject po;
				double x, y;
				int reli = i - index;
				int s = (int)BenchIconSize;
				
				x = s * (reli % PlayersPorRowInBench) + s / 2;
				y = s * (reli / PlayersPorRowInBench) + s / 2;
				if (team == Team.VISITOR) {
					x += BenchWidth + backgroundWidth;
				}
				                     
				po = new PlayerObject (template.List [i], new Point (x, y));
				po.IconSize = PlayersIconSize.Small;
				po.UnSelectedColor = color;
				Objects.Add (po);
			}
		}
		
		void Resize () {
			currentWidth = widget.Width;
			currentHeight = widget.Height;
			backgroundWidth = currentWidth - BenchWidth * NTeams;
			
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
					List<Player> hplayers = players.Where (p => homeTeam.List.Contains (p)).ToList();
					if (hplayers.Count == 2) {
						homeTeam.List.Swap (hplayers[0], hplayers[1]);
						subsDone = true;
					}
				}
				if (awayTeam != null) {
					List<Player> aplayers = players.Where (p => awayTeam.List.Contains (p)).ToList();
					if (aplayers.Count == 2) {
						awayTeam.List.Swap (aplayers[0], aplayers[1]);
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
		
		protected override void ShowMenu (Point coords) {
			if (ShowMenuEvent != null && Selections.Count > 0){
				ShowMenuEvent (
					Selections.Select (s => (s.Drawable as PlayerObject).Player).ToList());
			}
		}
		
		public override void Draw (IContext context, Area area)
		{
			if (currentWidth != widget.Width || currentHeight != widget.Height) {
				Resize ();
			}
			
			tk.Context = context;
			tk.Begin ();

			/* Background */
			if (background != null) {
				tk.DrawImage (new Point (BenchWidth, 0), backgroundWidth, currentHeight,
				              background, false);
			}
			
			tk.End ();
			base.Draw (context, area);
		}
	}
}

