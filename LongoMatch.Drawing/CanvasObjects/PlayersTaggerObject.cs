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
using LongoMatch.Core.Store.Templates;
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Store;
using LongoMatch.Core.Handlers;
using System.IO;

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
		const int SUBSTITUTION_BUTTONS_HEIGHT = 40;
		const int SUBSTITUTION_BUTTONS_WIDTH = 60;
		ButtonObject subPlayers, subInjury;
		/* Cached surfaces reused by player objects */
		ISurface backgroundSurface, homeNumberSurface, awayNumberSurface, photoSurface;
		ISurface homeInSurface, homeOutSurface, awayInSurface, awayOutSurface;
		TeamTemplate homeTeam, awayTeam;
		Image background;
		Dictionary<Player, PlayerObject> playerToPlayerObject;
		List<PlayerObject> homePlayingPlayers, awayPlayingPlayers;
		List<PlayerObject> homeBenchPlayers, awayBenchPlayers;
		List <PlayerObject> homePlayers, awayPlayers;
		BenchObject homeBench, awayBench;
		PlayerObject clickedPlayer, substitutionPlayer;
		ButtonObject clickedButton;
		FieldObject field;
		int NTeams;
		Point offset;
		bool substitutionMode;
		double scaleX, scaleY;
		Time lastTime, currentTime;

		public PlayersTaggerObject ()
		{
			Position = new Point (0, 0);
			homeBench = new BenchObject ();
			awayBench = new BenchObject ();
			playerToPlayerObject = new Dictionary<Player, PlayerObject>();
			field = new FieldObject ();
			SelectedPlayers = new List<Player> ();
			lastTime = null;
			LoadSurfaces ();
			LoadSubsButtons ();
		}

		protected override void Dispose (bool disposing)
		{
			ClearPlayers ();
			homeBench.Dispose ();
			awayBench.Dispose ();
			field.Dispose ();
			photoSurface.Dispose ();
			backgroundSurface.Dispose ();
			homeNumberSurface.Dispose ();
			awayNumberSurface.Dispose ();
			homeOutSurface.Dispose ();
			awayOutSurface.Dispose ();
			homeInSurface.Dispose ();
			awayInSurface.Dispose ();
			subPlayers.Dispose ();
			subInjury.Dispose ();
			base.Dispose (disposing);
		}

		public Time CurrentTime {
			get {
				return currentTime;
			}
			set {
				currentTime = value;
				if (lastTime == null) {
					UpdateLineup ();
				} else if (currentTime != lastTime && Project != null) {
					Time start, stop;
					if (lastTime < currentTime) {
						start = lastTime;
						stop = currentTime;
					} else {
						start = currentTime;
						stop = lastTime;
					}
					if (Project.LineupChanged (start, stop)) {
						UpdateLineup ();
					}
				}
				lastTime = currentTime;
			}
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

		public MultiSelectionMode SelectionMode {
			set;
			get;
		}
		
		public bool Compact {
			get;
			set;
		}

		public Project Project {
			get;
			set;
		}

		public bool SubstitutionMode {
			get {
				return substitutionMode;
			}
			set {
				substitutionMode = value;
				homeBench.SubstitutionMode = awayBench.SubstitutionMode = field.SubstitutionMode = value;
			}
		}
		
		public bool ShowSubsitutionButtons {
			get;
			set;
		}
		
		public bool ShowInjurySubsitutionButton {
			get;
			set;
		}

		public List<Player> SelectedPlayers {
			get;
			set;
		}

		public void Reload ()
		{
			LoadTeams (homeTeam, awayTeam, background);
		}

		public void Update ()
		{
			homeBench.Update ();
			awayBench.Update ();
			field.Update ();
		}
		
		public void Select (List<Player> players)
		{
			ResetSelection ();
			foreach (Player p in players) {
				Select (p, true);
			}
			if (PlayersSelectionChangedEvent != null) {
				PlayersSelectionChangedEvent (SelectedPlayers);
			}
		}

		public void Select (Player player, bool silent=false)
		{
			PlayerObject po;

			po = homePlayers.FirstOrDefault (p => p.Player == player);
			if (po == null) {
				po = awayPlayers.FirstOrDefault (p => p.Player == player);
			}
			if (po != null) {
				if (!silent) {
					ResetSelection ();
				}
				SelectedPlayers.Add (player);
				po.Active = true;
				if (!silent && PlayersSelectionChangedEvent != null) {
					PlayersSelectionChangedEvent (SelectedPlayers);
				}
			}
		}

		public void ResetSelection ()
		{
			SelectedPlayers.Clear ();
			substitutionPlayer = null;
			if (homePlayers != null) {
				foreach (PlayerObject player in homePlayers) {
					player.Active = false;
				}
			}
			if (awayPlayers != null) {
				foreach (PlayerObject player in awayPlayers) {
					player.Active = false;
				}
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
			ResetSelection ();
			ClearPlayers ();
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
			playerSize = colSize * 90 / 100;

			BenchWidth (colSize, field.Height, playerSize);
			field.LoadTeams (background, homeF, awayF, homePlayingPlayers,
			                 awayPlayingPlayers, playerSize, NTeams);
			homeBench.BenchPlayers = homeBenchPlayers;
			awayBench.BenchPlayers = awayBenchPlayers;
			homeBench.Height = awayBench.Height = field.Height;
			
			border = Config.Style.TeamTaggerBenchBorder;
			homeBench.Position = new Point (border, 0);
			field.Position = new Point (awayBench.Width + 2 * border, 0);
			awayBench.Position = new Point (awayBench.Width + field.Width + 3 * border, 0);

			Update ();
		}

		void UpdateLineup ()
		{
			List<Player> homeFieldL, awayFieldL, homeBenchL, awayBenchL;
			Project.CurrentLineup (currentTime, out homeFieldL, out homeBenchL,
			                       out awayFieldL, out awayBenchL);
			homePlayingPlayers = homeFieldL.Select (p => playerToPlayerObject [p]).ToList ();
			homeBenchPlayers = homeBenchL.Select (p => playerToPlayerObject [p]).ToList ();
			awayPlayingPlayers = awayFieldL.Select (p => playerToPlayerObject [p]).ToList ();
			awayBenchPlayers = awayBenchL.Select (p => playerToPlayerObject [p]).ToList ();
			homeBench.BenchPlayers = homeBenchPlayers;
			awayBench.BenchPlayers = awayBenchPlayers;
			field.HomePlayingPlayers = homePlayingPlayers;
			field.AwayPlayingPlayers = awayPlayingPlayers;
			Update ();
			EmitRedrawEvent (this, new Area (Position, Width, Height));
		}

		void BenchWidth (int colSize, int height, int playerSize)
		{
			int maxPlayers, playersPerColumn, playersPerRow;
			double ncolSize;
			
			ncolSize = colSize;
			
			maxPlayers = Math.Max (
				homeBenchPlayers != null ? homeBenchPlayers.Count : 0,
				awayBenchPlayers != null ? awayBenchPlayers.Count : 0);
			playersPerColumn = height / colSize;
			if (Compact) {
				/* Try with 4/4, 3/4 and 2/4 of the original column size
				 * to fit all players in a single column */ 
				for (int i=4; i>1; i--) {
					ncolSize = (double)colSize * i / 4;
					playersPerColumn = (int)(height / ncolSize);
					playersPerRow = (int)Math.Ceiling ((double)maxPlayers / playersPerColumn);
					if (playersPerRow == 1) {
						break;
					}
				}
			}

			homeBench.PlayersSize = awayBench.PlayersSize = (int)(ncolSize * 90 / 100);
			homeBench.PlayersPerRow = awayBench.PlayersPerRow =
				(int)Math.Ceiling ((double)maxPlayers / playersPerColumn);
			homeBench.Width = awayBench.Width = (int)ncolSize * homeBench.PlayersPerRow;
		}

		void ClearPlayers ()
		{
			if (homePlayers != null) {
				foreach (PlayerObject po in homePlayers) {
					po.Dispose ();
					homePlayers = null;
				}
			}
			if (awayPlayers != null) {
				foreach (PlayerObject po in awayPlayers) {
					po.Dispose ();
					awayPlayers = null;
				}
			}
			playerToPlayerObject.Clear ();
		}

		ISurface CreateSurface (string name)
		{
			return Config.DrawingToolkit.CreateSurface (Path.Combine (Config.ImagesDir, name));
		}

		void LoadSurfaces ()
		{
			photoSurface = CreateSurface (StyleConf.PlayerPhoto);
			backgroundSurface = CreateSurface (StyleConf.PlayerBackground);
			homeNumberSurface = CreateSurface (StyleConf.PlayerHomeNumber);
			awayNumberSurface = CreateSurface (StyleConf.PlayerAwayNumber);
			homeOutSurface = CreateSurface (StyleConf.PlayerHomeOut);
			awayOutSurface = CreateSurface (StyleConf.PlayerAwayOut);
			homeInSurface = CreateSurface (StyleConf.PlayerHomeIn);
			awayInSurface = CreateSurface (StyleConf.PlayerAwayIn);
		}

		void LoadSubsButtons () {
			subPlayers = new ButtonObject ();
			string  path = Path.Combine (Config.IconsDir, StyleConf.SubsUnlock);
			subPlayers.BackgroundImageActive = Image.LoadFromFile (path);
			path = Path.Combine (Config.IconsDir, StyleConf.SubsLock);
			subPlayers.BackgroundImage = Image.LoadFromFile (path);
			subPlayers.Toggle = true;
			subPlayers.ClickedEvent += HandleSubsClicked;
			subInjury = new ButtonObject ();
			subInjury.Toggle = true;
			subInjury.ClickedEvent += HandleSubsClicked;
			subInjury.Visible = false;
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
			ISurface number, sin, sout;

			if (team == Team.LOCAL) {
				color = Config.Style.HomeTeamColor;
				number = homeNumberSurface;
				sin = homeInSurface;
				sout = homeOutSurface;
			} else {
				color = Config.Style.AwayTeamColor;
				number = awayNumberSurface;
				sin = awayInSurface;
				sout = awayOutSurface;
			}

			playerObjects = new List<PlayerObject> ();
			foreach (Player p in players) {
				PlayerObject po = new PlayerObject { Player = p, Color = color,
					Team = team, Background = backgroundSurface,
					Number =  number, In = sin, Out = sout,
					SubstitutionMode =  SubstitutionMode,
					Photo = photoSurface
				};
				po.ClickedEvent += HandlePlayerClickedEvent;
				playerObjects.Add (po);
				playerToPlayerObject.Add (p, po);
			}
			return playerObjects;
		}

		void EmitSubsitutionEvent (PlayerObject player1, PlayerObject player2)
		{
			TeamTemplate team;
			List<PlayerObject> bench, field;

			if (substitutionPlayer.Team == Team.LOCAL) {
				team = homeTeam;
				bench = homeBenchPlayers;
			} else {
				team = awayTeam;
				bench = awayBenchPlayers;
			}
			if (PlayersSubstitutionEvent != null) {
				if (bench.Contains (player1) && bench.Contains (player2)) {
					PlayersSubstitutionEvent (team, player1.Player, player2.Player,
					                          SubstitutionReason.BenchPositionChange, CurrentTime);
				} else if (!bench.Contains (player1) && !bench.Contains (player2)) {
					PlayersSubstitutionEvent (team, player1.Player, player2.Player,
					                          SubstitutionReason.PositionChange, CurrentTime);
				} else if (bench.Contains (player1)) {
					PlayersSubstitutionEvent (team, player1.Player, player2.Player,
					                          SubstitutionReason.PlayersSubstitution, CurrentTime);
				} else {
					PlayersSubstitutionEvent (team, player2.Player, player1.Player,
					                          SubstitutionReason.PlayersSubstitution, CurrentTime);
				}
			}
			ResetSelection ();
		}

		void HandlePlayerClickedEvent (ICanvasObject co)
		{
			PlayerObject player = co as PlayerObject;

			if (SubstitutionMode) {
				if (substitutionPlayer == null) {
					substitutionPlayer = player;
				} else {
					if (substitutionPlayer.Team == player.Team) {
						EmitSubsitutionEvent (player, substitutionPlayer);
					} else {
						player.Active = false;
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

		bool ButtonClickPressed (Point point, ButtonModifier modif, params ButtonObject[] buttons)
		{
			Selection sel;
			
			if (!ShowSubsitutionButtons) {
				return false;
			}

			foreach (ButtonObject button in buttons) {
				if (!button.Visible)
					continue;
				sel = button.GetSelection (point, 0);
				if (sel != null) {
					clickedButton = sel.Drawable as ButtonObject;
					(sel.Drawable as ICanvasObject).ClickPressed (point, modif);
					return true;
				}
			}
			return false;
		}

		void HandleSubsClicked (ICanvasObject co)
		{
			ResetSelection ();
			if (PlayersSelectionChangedEvent != null) {
				PlayersSelectionChangedEvent (SelectedPlayers);
			}
			SubstitutionMode = !SubstitutionMode;
		}

		public override void ClickPressed (Point point, ButtonModifier modif)
		{
			Selection selection = null;

			if (ButtonClickPressed (point, modif, subPlayers, subInjury)) {
				return;
			}
			
			if (!SubstitutionMode && SelectionMode != MultiSelectionMode.Multiple) {
				if (SelectionMode == MultiSelectionMode.Single || modif == ButtonModifier.None) {
					ResetSelection ();
				}
			}
			
			point = Utils.ToUserCoords (point, offset, scaleX, scaleY);
			selection = homeBench.GetSelection (point, 0, false);
			if (selection == null) {
				selection = awayBench.GetSelection (point, 0, false);
				if (selection == null) {
					selection = field.GetSelection (point, 0, false);
				}
			}
			if (selection != null) {
				clickedPlayer = selection.Drawable as PlayerObject;
				if (SubstitutionMode && substitutionPlayer != null &&
					clickedPlayer.Team != substitutionPlayer.Team) {
					clickedPlayer = null;
				} else {
					(selection.Drawable as ICanvasObject).ClickPressed (point, modif);
				}
			} else {
				clickedPlayer = null;
			}
		}

		public override void ClickReleased ()
		{
			if (clickedButton != null) {
				clickedButton.ClickReleased ();
				clickedButton = null;
			} else if (clickedPlayer != null) {
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
			Image.ScaleFactor ((int)width, (int)height, (int)Width,
			                   (int)Height - SUBSTITUTION_BUTTONS_HEIGHT,
			                   out scaleX, out scaleY, out offset);
			offset.Y += SUBSTITUTION_BUTTONS_HEIGHT;
			tk.Begin ();
			tk.Clear (Config.Style.PaletteBackground);

			/* Draw substitution buttons */
			if (ShowSubsitutionButtons) {
				subPlayers.Position = new Point (Width / 2 - SUBSTITUTION_BUTTONS_WIDTH / 2,
				                                 offset.Y - SUBSTITUTION_BUTTONS_HEIGHT);
				subPlayers.Width = SUBSTITUTION_BUTTONS_WIDTH;
				subPlayers.Height = SUBSTITUTION_BUTTONS_HEIGHT;
				subPlayers.Draw (tk, area);
				
				//subInjury.Position = new Point (100, 0);
				//subInjury.Width = 100;
				//subInjury.Height = SUBSTITUTION_BUTTONS_HEIGHT;
				//subInjury.Draw (tk, area);
			}

			
			tk.TranslateAndScale (Position + offset, new Point (scaleX, scaleY));
			homeBench.Draw (tk, area);
			awayBench.Draw (tk, area);
			field.Draw (tk, area);
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion=false)
		{
			Selection sel = null;

			if (!inMotion) {
				if (point.X < Position.X || point.X > Position.X + Width ||
					point.Y < Position.Y || point.Y > Position.Y + Height) {
					sel = null;
				} else {
					sel = new Selection (this, SelectionPosition.All, 0);
				}
			} else {
				point = Utils.ToUserCoords (point, offset, scaleX, scaleY);
				sel = homeBench.GetSelection (point, 0, false);
				if (sel == null) {
					sel = awayBench.GetSelection (point, 0, false);
					if (sel == null) {
						sel = field.GetSelection (point, 0, false);
					}
				}
			}
			return sel;
		}

		public void Move (Selection s, Point p, Point start)
		{
			throw new NotImplementedException ("Unsupported move for PlayersTaggerObject:  " + s.Position);
		}
	}
}

