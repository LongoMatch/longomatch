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
using LongoMatch.Handlers;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class PlayersTaggerObject: CanvasObject, ICanvasSelectableObject
	{

		/* This object can be used like single object filling a canvas or embedded
		 * in a canvas with more objects, like in the analysis template.
		 * For this reason we can't use the canvas selection logic and we have
		 * to handle it internally
		 */

		public event PlayersSubstitutionHandler PlayersSubstitutionEvent;
		public event PlayersSelectionChangedHandler PlayersSelectionChangedEvent;

		TeamTemplate homeTeam, awayTeam;
		Image background;
		List<PlayerObject> homePlayingPlayers, awayPlayingPlayers;
		List<PlayerObject> homeBenchPlayers, awayBenchPlayers;
		List <PlayerObject> homePlayers, awayPlayers;
		BenchObject homeBench, awayBench;
		PlayerObject clickedPlayer, substitutionPlayer;
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
			SelectedPlayers = new List<Player> ();
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

		public bool SubstitutionMode {
			get;
			set;
		}
		
		public List<Player> SelectedPlayers {
			get;
			set;
		}

		public void Reload () {
			LoadTeams (homeTeam, awayTeam, background);
		}

		public void Update ()
		{
			homeBench.Update ();
			awayBench.Update ();
			field.Update ();
		}

		public void ResetSelection ()
		{
			SelectedPlayers.Clear ();
			substitutionPlayer = null;
			foreach (PlayerObject player in homePlayers) {
				player.Active = false;
			}
			foreach (PlayerObject player in awayPlayers) {
				player.Active = false;
			}
		}
		
		public void Substitute (Player p1, Player p2, TeamTemplate team)
		{
			if (team == homeTeam) {
				Substitute (homePlayers.FirstOrDefault (p => p.Player == p1),
				            homePlayers.FirstOrDefault (p => p.Player == p2),
				            homePlayingPlayers, homeBenchPlayers);
			} else {
				Substitute (awayPlayers.FirstOrDefault (p => p.Player == p1),
				            awayPlayers.FirstOrDefault (p => p.Player == p2),
				            awayPlayingPlayers, awayBenchPlayers);
			}
		}

		public void LoadTeams (TeamTemplate homeTeam, TeamTemplate awayTeam, Image background)
		{
			int[] homeF = null, awayF = null;
			int playerSize, colSize, border;

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

			homePlayers = new List<PlayerObject> ();
			awayPlayers = new List<PlayerObject> ();

			if (homeTeam != null) {
				homePlayingPlayers = GetPlayers (homeTeam.StartingPlayersList, Team.LOCAL);
				homeBenchPlayers = GetPlayers (homeTeam.BenchPlayersList, Team.LOCAL);
				homePlayers.AddRange (homePlayingPlayers);
				homePlayers.AddRange (homeBenchPlayers);
				homeF = homeTeam.Formation;
				NTeams ++;
			}
			if (awayTeam != null) {
				awayPlayingPlayers = GetPlayers (awayTeam.StartingPlayersList, Team.VISITOR);
				awayBenchPlayers = GetPlayers (awayTeam.BenchPlayersList, Team.VISITOR);
				awayPlayers.AddRange (awayPlayingPlayers);
				awayPlayers.AddRange (awayBenchPlayers);
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


		void Substitute (PlayerObject p1, PlayerObject p2,
		                 List<PlayerObject> playingPlayers,
		                 List<PlayerObject> benchPlayers)
		{
			Point tmpPos;
			List<PlayerObject> p1List, p2List;

			if (playingPlayers.Contains (p1)) {
				p1List = playingPlayers;
			} else {
				p1List = benchPlayers;
			}
			if (playingPlayers.Contains (p2)) {
				p2List = playingPlayers;
			} else {
				p2List = benchPlayers;
			}
			
			if (p1List == p2List) {
				p1List.Swap (p1, p2);
			} else {
				int p1Index, p2Index;

				p1Index = p1List.IndexOf (p1);
				p2Index = p2List.IndexOf (p2);
				p1List.Remove (p1);
				p2List.Remove (p2);
				p1List.Insert (p1Index, p2);
				p2List.Insert (p2Index, p1);
			}
			tmpPos = p2.Position;
			p2.Position = p1.Position;
			p1.Position = tmpPos;
			ResetSelection ();
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
			List<PlayerObject> playerObjects;
			Color color = null;

			if (team == Team.LOCAL) {
				color = Config.Style.HomeTeamColor;
			} else {
				color = Config.Style.AwayTeamColor;
			}

			playerObjects = new List<PlayerObject> ();
			foreach (Player p in players) {
				PlayerObject po = new PlayerObject { Player = p, Color = color, Team = team };
				po.ClickedEvent += HandleClickedEvent;
				playerObjects.Add (po);
			}
			return playerObjects;
		}

		void HandleClickedEvent (CanvasObject co)
		{
			PlayerObject player = co as PlayerObject;

			if (SubstitutionMode) {
				if (substitutionPlayer == null) {
					substitutionPlayer = player;
				} else {
					if (substitutionPlayer.Team == player.Team) {
						TeamTemplate team;
						if (substitutionPlayer.Team == Team.LOCAL) {
							team = homeTeam;
						} else {
							team = awayTeam;
						}
						if (PlayersSubstitutionEvent != null) {
							PlayersSubstitutionEvent (substitutionPlayer.Player,
							                          player.Player, team);
						}
					}
				}
			} else {
				if (player.Active) {
					SelectedPlayers.Add (player.Player);
				} else {
					SelectedPlayers.Remove (player.Player);
				}
				if (PlayersSelectionChangedEvent != null) {
					PlayersSelectionChangedEvent (SelectedPlayers);
				}
			}
		}

		public override void ClickPressed (Point point, ButtonModifier modif)
		{
			Selection selection = null;
			
			if (modif == ButtonModifier.None && !SubstitutionMode) {
				ResetSelection ();
			}
			
			point = Utils.ToUserCoords (point, offset, scaleX, scaleY);
			selection = homeBench.GetSelection (point, 0);
			if (selection == null) {
				selection = awayBench.GetSelection (point, 0);
				if (selection == null) {
					selection = field.GetSelection (point, 0);
				}
			}
			if (selection != null) {
				clickedPlayer = selection.Drawable as PlayerObject;
				if (SubstitutionMode && substitutionPlayer != null &&
					clickedPlayer.Team != substitutionPlayer.Team) {
					clickedPlayer= null;
				} else {
					(selection.Drawable as ICanvasObject).ClickPressed (point, modif);
				}
			} else {
				clickedPlayer = null;
			}
		}

		public override void ClickReleased ()
		{
			if (clickedPlayer != null) {
				clickedPlayer.ClickReleased ();
			} else {
				ResetSelection ();
				if (PlayersSelectionChangedEvent != null) {
					PlayersSelectionChangedEvent (SelectedPlayers);
				}
			}
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
			if (point.X < Position.X || point.X > Position.X + Width ||
				point.Y < Position.Y || point.Y > Position.Y + Height) {
				return null;
			}
			return new Selection (this, SelectionPosition.All, 0);
		}

		public void Move (Selection s, Point p, Point start)
		{
			throw new NotImplementedException ("Unsupported move for PlayersTaggerObject:  " + s.Position);
		}
	}
}

