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
using LongoMatch.Core.Interfaces.Drawing;
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class FieldObject: CanvasObject, ICanvasSelectableObject
	{
		int[] homeFormation;
		int[] awayFormation;
		List<PlayerObject> homePlayingPlayers;
		List<PlayerObject> awayPlayingPlayers;
		int playerSize;
		Image background;

		public FieldObject ()
		{
			Position = new Point (0, 0);
		}

		public int Width {
			get;
			set;
		}

		public int Height {
			get;
			set;
		}

		public Point Position {
			get;
			set;
		}

		public bool SubstitutionMode {
			get;
			set;
		}

		public void LoadTeams (Image backgroundImg, int[] homeF, int[] awayF,
		                       List<PlayerObject> homeT, List<PlayerObject> awayT,
		                       int size, int nteams)
		{
			background = backgroundImg;
			homeFormation = homeF;
			awayFormation = awayF;
			homePlayingPlayers = homeT;
			awayPlayingPlayers = awayT;
			playerSize = size;
			NTeams = nteams;
			Update ();
		}

		public void Update ()
		{
			if (homeFormation != null) {
				UpdateTeam (homePlayingPlayers, homeFormation, Team.LOCAL);
			}
			if (awayFormation != null) {
				UpdateTeam (awayPlayingPlayers, awayFormation, Team.VISITOR);
			}
		}

		public int NTeams {
			get;
			set;
		}

		void UpdateTeam (List<PlayerObject> players, int[] formation, Team team)
		{
			int index = 0, offsetX;
			int width, colWidth;
			Color color;

			width = Width / NTeams;
			colWidth = width / formation.Length;
			if (team == Team.LOCAL) {
				color = Config.Style.HomeTeamColor;
				offsetX = 0;
			} else {
				color = Config.Style.AwayTeamColor; 
				offsetX = Width;
			}

			/* Columns */
			for (int col=0; col < formation.Length; col ++) {
				double colX, rowHeight;
				
				if (players.Count == index)
					break;

				if (team == Team.LOCAL) {
					colX = offsetX + colWidth * col + colWidth / 2;
				} else {
					colX = offsetX - colWidth * col - colWidth / 2;
				}
				rowHeight = Height / formation [col];

				for (int row=0; row < formation[col]; row ++) {
					PlayerObject po = players [index];
					po.Position = new Point (colX, rowHeight * row + rowHeight / 2); 
					po.Size = playerSize;
					index ++;
					if (players.Count == index)
						break;
				}
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			tk.Begin ();
			tk.TranslateAndScale (Position, new Point (1, 1));
			if (background != null) {
				tk.DrawImage (background);
			}
			if (homePlayingPlayers != null) {
				foreach (PlayerObject po in homePlayingPlayers) {
					po.Playing = true;
					po.Draw (tk, area);
				}
			}
			if (awayPlayingPlayers != null) {
				foreach (PlayerObject po in awayPlayingPlayers) {
					po.Playing = true;
					po.Draw (tk, area);
				}
			}
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion)
		{
			Selection selection = null;

			point = Utils.ToUserCoords (point, Position, 1, 1);

			if (homePlayingPlayers != null) {
				foreach (PlayerObject po in homePlayingPlayers) {
					selection = po.GetSelection (point, precision);
					if (selection != null)
						break;
				}
			}
			if (selection == null && awayPlayingPlayers != null) {
				foreach (PlayerObject po in awayPlayingPlayers) {
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
	}
}

