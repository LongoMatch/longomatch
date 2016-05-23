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
using System.Collections.Generic;
using LongoMatch.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Core.Common;

namespace LongoMatch.Drawing.CanvasObjects.Teams
{
	public class FieldObject: CanvasObject, ICanvasSelectableObject
	{
		int[] homeFormation;
		int[] awayFormation;
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

		public List<PlayerObject> HomePlayingPlayers {
			get;
			set;
		}

		public List<PlayerObject> AwayPlayingPlayers {
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
			HomePlayingPlayers = homeT;
			AwayPlayingPlayers = awayT;
			playerSize = size;
			NTeams = nteams;
			Update ();
		}

		public void Update ()
		{
			if (homeFormation != null) {
				UpdateTeam (HomePlayingPlayers, homeFormation, TeamType.LOCAL);
			}
			if (awayFormation != null) {
				UpdateTeam (AwayPlayingPlayers, awayFormation, TeamType.VISITOR);
			}
		}

		public int NTeams {
			get;
			set;
		}

		void UpdateTeam (List<PlayerObject> players, int[] formation, TeamType team)
		{
			int index = 0, offsetX;
			int width, colWidth;
			Color color;

			width = Width / NTeams;
			colWidth = width / formation.Length;
			if (team == TeamType.LOCAL) {
				color = Config.Style.HomeTeamColor;
				offsetX = 0;
			} else {
				color = Config.Style.AwayTeamColor; 
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
					PlayerObject po = players [index];

					if (team == TeamType.LOCAL) {
						rowY = rowHeight * row + rowHeight / 2;
					} else {
						rowY = Height - (rowHeight * row + rowHeight / 2);
					}

					po.Position = new Point (colX, rowY);
					po.Size = playerSize;
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
			if (background != null) {
				tk.DrawImage (background);
			}
			if (HomePlayingPlayers != null) {
				foreach (PlayerObject po in HomePlayingPlayers) {
					po.Playing = true;
					po.SubstitutionMode = SubstitutionMode;
					po.Size = playerSize;
					po.Draw (tk, area);
				}
			}
			if (AwayPlayingPlayers != null) {
				foreach (PlayerObject po in AwayPlayingPlayers) {
					po.Playing = true;
					po.SubstitutionMode = SubstitutionMode;
					po.Size = playerSize;
					po.Draw (tk, area);
				}
			}
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion)
		{
			Selection selection = null;

			point = Utils.ToUserCoords (point, Position, 1, 1);

			if (HomePlayingPlayers != null) {
				foreach (PlayerObject po in HomePlayingPlayers) {
					selection = po.GetSelection (point, precision);
					if (selection != null)
						break;
				}
			}
			if (selection == null && AwayPlayingPlayers != null) {
				foreach (PlayerObject po in AwayPlayingPlayers) {
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

