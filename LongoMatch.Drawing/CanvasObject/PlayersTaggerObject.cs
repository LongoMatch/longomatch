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
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store.Templates;
using System.Collections.Generic;
using LongoMatch.Common;
using LongoMatch.Store.Drawables;
using LongoMatch.Store;

namespace LongoMatch.Drawing.CanvasObject
{
	public class PlayersTaggerObject: CanvasObject, ICanvasSelectableObject
	{

		TeamTemplate homeTeam, awayTeam;
		List<PlayerObject> homePlayingPlayers, awayPlayingPlayers;
		List<PlayerObject> homeBenchPlayers, awayBenchPlayers;
		BenchObject homeBench, awayBench;
		Image background;
		FieldObject field;
		int NTeams;
		Point offset;
		double scaleX, scaleY;

		public PlayersTaggerObject ()
		{
			Position = new Point (0, 0);
			homeBench = new BenchObject ();
			awayBench = new BenchObject ();
			field = new FieldObject ();
		}
		
		public Point Position {
			get;
			set;
		}
		
		public double Width {
			get;
			set;
		}
		
		public double Height {
			get;
			set;
		}
		
		public void Update ()
		{
			homeBench.Update ();
			awayBench.Update ();
			field.Update ();
		}

		int ColumnSize {
			get {
				int width, optWidth, optHeight, count = 0, max = 0;

				width = field.Width / NTeams;
				if (homeTeam != null && awayTeam != null) {
					count = Math.Max (homeTeam.Formation.Count (),
					                  awayTeam.Formation.Count ());
					max = Math.Max (homeTeam.Formation.Max (),
					                awayTeam.Formation.Max ());
				} else if (homeTeam != null) {
					count = homeTeam.Formation.Count ();
					max = homeTeam.Formation.Max ();
				} else if (awayTeam != null) {
					count = awayTeam.Formation.Count ();
					max = awayTeam.Formation.Max ();
				}
				optWidth = width / count;
				optHeight = field.Height / max;
				return Math.Min (optWidth, optHeight);
			}
		}

		List<PlayerObject> GetPlayers (List<Player> players, Team team)
		{
			Color color = null;

			if (team == Team.LOCAL) {
				color = Config.Style.HomeTeamColor;
			} else {
				color = Config.Style.AwayTeamColor;
			}

			return players.Select (p => new PlayerObject {Player = p, Color = color}).ToList();
		}

		public void LoadTeams (TeamTemplate homeTeam, TeamTemplate awayTeam, Image background)
		{
			int[] homeF = null, awayF = null;
			int playerSize, colSize, widgetSize, border;
			double width, height;

			this.homeTeam = homeTeam;
			this.awayTeam = awayTeam;
			this.background = background;
			NTeams = 0;

			if (background != null) {
				field.Height = background.Height;
				field.Width = background.Width;
			} else {
				field.Width = 300;
				field.Height = 250;
			}
			homePlayingPlayers = awayPlayingPlayers = null;

			if (homeTeam != null) {
				homePlayingPlayers = GetPlayers (homeTeam.StartingPlayersList, Team.LOCAL);
				homeBenchPlayers = GetPlayers (homeTeam.BenchPlayersList, Team.LOCAL);
				homeF = homeTeam.Formation;
				NTeams ++;
			}
			if (awayTeam != null) {
				awayPlayingPlayers = GetPlayers (awayTeam.StartingPlayersList, Team.VISITOR);
				awayBenchPlayers = GetPlayers (awayTeam.BenchPlayersList, Team.VISITOR);
				awayF = awayTeam.Formation;
				NTeams ++;
			}

			colSize = ColumnSize;
			playerSize = colSize * 80 / 100;

			field.LoadTeams (background, homeF, awayF, homePlayingPlayers,
			                 awayPlayingPlayers, playerSize, NTeams);
			homeBench.BenchPlayers = homeBenchPlayers;
			awayBench.BenchPlayers = awayBenchPlayers;
			homeBench.PlayersSize = awayBench.PlayersSize = playerSize;
			homeBench.PlayersPerRow = awayBench.PlayersPerRow = 2;
			homeBench.Width = awayBench.Width = colSize * 2;
			homeBench.Height = awayBench.Height = field.Height;
			
			border = Config.Style.TeamTaggerBenchBorder;
			homeBench.Position = new Point (border, 0);
			field.Position = new Point (awayBench.Width + 2 * border, 0);
			awayBench.Position = new Point (awayBench.Width + field.Width + 3 * border, 0);

			Update ();
		}
		
		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double width, height;
			
			/* Compute how we should scale and translate to fit the widget
			 * in the designated area */
			width = homeBench.Width * NTeams + field.Width +
				2 * NTeams * Config.Style.TeamTaggerBenchBorder; 
			height = field.Height;
			Image.ScaleFactor ((int)width, (int)height, (int)Width, (int)Height,
			                   out scaleX, out scaleY, out offset);
			tk.Begin ();
			tk.TranslateAndScale (Position + offset, new Point (scaleX, scaleY));
			homeBench.Draw (tk, area);
			awayBench.Draw (tk, area);
			field.Draw (tk, area);
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision)
		{
			Selection selection = null;

			point = Utils.ToUserCoords (point, offset, scaleX, scaleY);

			selection = homeBench.GetSelection (point, precision);
			if (selection == null) {
				selection = awayBench.GetSelection (point, precision);
				if (selection == null) {
					selection = field.GetSelection (point, precision);
				}
			}
			return selection;
		}

		public void Move (Selection s, Point p, Point start)
		{
		}
		
	}
}

