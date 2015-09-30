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
using System.Linq;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Drawing.CanvasObjects.Teams
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
		public event TeamSelectionChangedHandler TeamSelectionChangedEvent;

		const int BUTTONS_HEIGHT = 40;
		const int BUTTONS_WIDTH = 60;
		ButtonObject subPlayers, subInjury, homeButton, awayButton;
		Team homeTeam, awayTeam;
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
		bool substitutionMode, showSubsitutionButtons, showTeamsButtons;
		double scaleX, scaleY;
		Time lastTime, currentTime;

		public PlayersTaggerObject ()
		{
			Position = new Point (0, 0);
			homeBench = new BenchObject ();
			awayBench = new BenchObject ();
			offset = new Point (0, 0);
			scaleX = scaleY = 1;
			playerToPlayerObject = new Dictionary<Player, PlayerObject> ();
			field = new FieldObject ();
			SelectedPlayers = new List<Player> ();
			lastTime = null;
			LoadSubsButtons ();
			LoadTeamsButtons ();
			ShowSubsitutionButtons = false;
			ShowTeamsButtons = false;
		}

		protected override void Dispose (bool disposing)
		{
			ClearPlayers ();
			homeBench.Dispose ();
			awayBench.Dispose ();
			field.Dispose ();
			subPlayers.Dispose ();
			subInjury.Dispose ();
			homeButton.Dispose ();
			awayButton.Dispose ();
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
			get {
				return showSubsitutionButtons;
			}
			set {
				subPlayers.Visible = value;
				/* FIXME: Not displayed for now */
				subInjury.Visible = false;
				showSubsitutionButtons = value;
			}
		}

		public bool ShowTeamsButtons {
			get {
				return showTeamsButtons;
			}
			set {
				showTeamsButtons = value;
				homeButton.Visible = value;
				awayButton.Visible = value;
			}
		}

		public List<Player> SelectedPlayers {
			get;
			set;
		}

		public TeamType SelectedTeam {
			get {
				if (homeButton.Active && awayButton.Active) {
					return TeamType.BOTH;
				} else if (homeButton.Active) {
					return TeamType.LOCAL;
				} else if (awayButton.Active) {
					return TeamType.VISITOR;
				} else {
					return TeamType.NONE;
				}
			}
		}

		public void Reload ()
		{
			LoadTeams (homeTeam, awayTeam, background);
			UpdateLineup ();
		}

		public void Update ()
		{
			homeBench.Update ();
			awayBench.Update ();
			field.Update ();
		}

		public void Select (IList<Player> players, TeamType team)
		{
			ResetSelection ();
			foreach (Player p in players) {
				Select (p, true, false);
			}
			homeButton.Active = team == TeamType.BOTH || team == TeamType.LOCAL;
			awayButton.Active = team == TeamType.BOTH || team == TeamType.VISITOR;
			if (PlayersSelectionChangedEvent != null) {
				PlayersSelectionChangedEvent (SelectedPlayers);
			}
		}

		public void Select (TeamType team)
		{
			if (team == TeamType.LOCAL) {
				homeButton.Active = true;
				awayButton.Active = false;
			} else {
				awayButton.Active = true;
				homeButton.Active = false;
			}
		}

		public void Select (Player player, bool silent = false, bool reset = false)
		{
			PlayerObject po;

			po = homePlayers.FirstOrDefault (p => p.Player == player);
			if (po == null) {
				po = awayPlayers.FirstOrDefault (p => p.Player == player);
			}
			if (po != null) {
				if (reset) {
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
			homeButton.Active = false;
			awayButton.Active = false;
		}

		public void Substitute (Player p1, Player p2, Team team)
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

		public void LoadTeams (Team homeTeam, Team awayTeam, Image background)
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
			lastTime = null;

			homePlayers = new List<PlayerObject> ();
			awayPlayers = new List<PlayerObject> ();

			if (homeTeam != null) {
				homeTeam.UpdateColors ();
				homePlayingPlayers = GetPlayers (homeTeam.StartingPlayersList, TeamType.LOCAL);
				homeBenchPlayers = GetPlayers (homeTeam.BenchPlayersList, TeamType.LOCAL);
				homePlayers.AddRange (homePlayingPlayers);
				homePlayers.AddRange (homeBenchPlayers);
				homeF = homeTeam.Formation;
				if (homeTeam.Shield == null) {
					homeButton.BackgroundImage = Resources.LoadImage (StyleConf.DefaultShield);
				} else {
					homeButton.BackgroundImage = homeTeam.Shield;
				}
				NTeams++;
			}
			if (awayTeam != null) {
				awayTeam.UpdateColors ();
				awayPlayingPlayers = GetPlayers (awayTeam.StartingPlayersList, TeamType.VISITOR);
				awayBenchPlayers = GetPlayers (awayTeam.BenchPlayersList, TeamType.VISITOR);
				awayPlayers.AddRange (awayPlayingPlayers);
				awayPlayers.AddRange (awayBenchPlayers);
				awayF = awayTeam.Formation;
				if (awayTeam.Shield == null) {
					awayButton.BackgroundImage = Resources.LoadImage (StyleConf.DefaultShield);
				} else {
					awayButton.BackgroundImage = awayTeam.Shield;
				}
				NTeams++;
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
			if (homeTeam == null || awayTeam == null) {
				if (homeTeam != null) {
					homeBench.Position = new Point (border, 0);
					field.Position = new Point (border + homeBench.Width + border, 0);
				} else {
					field.Position = new Point (border, 0);
					awayBench.Position = new Point (border + field.Width + border, 0);
				}
			} else {
				homeBench.Position = new Point (border, 0);
				field.Position = new Point (homeBench.Width + 2 * border, 0);
				awayBench.Position = new Point (awayBench.Width + field.Width + 3 * border, 0);
			}

			Update ();
		}

		public override void ResetDrawArea ()
		{
			base.ResetDrawArea ();
			if (homePlayers != null) {
				foreach (CanvasObject co in homePlayers) {
					co.ResetDrawArea ();
				}
			}
			if (awayPlayers != null) {
				foreach (CanvasObject co in awayPlayers) {
					co.ResetDrawArea ();
				}
			}
			subPlayers.ResetDrawArea ();
			subInjury.ResetDrawArea ();
			homeButton.ResetDrawArea ();
			awayButton.ResetDrawArea ();
		}

		void UpdateLineup ()
		{
			List<Player> homeFieldL, awayFieldL, homeBenchL, awayBenchL;
			
			if (Project == null) {
				return;
			}
			
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
				for (int i = 4; i > 1; i--) {
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

		void LoadSubsButtons ()
		{
			subPlayers = new ButtonObject ();
			subPlayers.BackgroundImageActive = Resources.LoadImage (StyleConf.SubsUnlock);
			subPlayers.BackgroundColorActive = Config.Style.PaletteBackground;
			subPlayers.BackgroundImage = Resources.LoadImage (StyleConf.SubsLock);
			subPlayers.Toggle = true;
			subPlayers.ClickedEvent += HandleSubsClicked;
			subInjury = new ButtonObject ();
			subInjury.BackgroundColorActive = Config.Style.PaletteBackground;
			subInjury.Toggle = true;
			subInjury.ClickedEvent += HandleSubsClicked;
			subInjury.Visible = false;
		}

		void LoadTeamsButtons ()
		{
			homeButton = new ButtonObject ();
			homeButton.Toggle = true;
			homeButton.ClickedEvent += HandleTeamClickedEvent;
			homeButton.Width = BUTTONS_WIDTH;
			homeButton.Height = BUTTONS_HEIGHT;
			homeButton.RedrawEvent += (co, area) => {
				EmitRedrawEvent (homeButton, area);
			};
			awayButton = new ButtonObject ();
			awayButton.Toggle = true;
			awayButton.Width = BUTTONS_WIDTH;
			awayButton.Height = BUTTONS_HEIGHT;
			awayButton.ClickedEvent += HandleTeamClickedEvent;
			awayButton.RedrawEvent += (co, area) => {
				EmitRedrawEvent (awayButton, area);
			};
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
			EmitRedrawEvent (this, null);
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

		List<PlayerObject> GetPlayers (List<Player> players, TeamType team)
		{
			List<PlayerObject> playerObjects;
			Color color = null;

			if (team == TeamType.LOCAL) {
				color = Config.Style.HomeTeamColor;
			} else {
				color = Config.Style.AwayTeamColor;
			}

			playerObjects = new List<PlayerObject> ();
			foreach (Player p in players) {
				PlayerObject po = new PlayerObject { Player = p, Team = team };
				po.ClickedEvent += HandlePlayerClickedEvent;
				po.RedrawEvent += (co, area) => {
					EmitRedrawEvent (po, area);
				};
				playerObjects.Add (po);
				playerToPlayerObject.Add (p, po);
			}
			return playerObjects;
		}

		void EmitSubsitutionEvent (PlayerObject player1, PlayerObject player2)
		{
			Team team;
			List<PlayerObject> bench;

			if (substitutionPlayer.Team == TeamType.LOCAL) {
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
			UpdateLineup ();
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
			
			if (!ShowSubsitutionButtons && !ShowTeamsButtons) {
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

		void HandleTeamClickedEvent (ICanvasObject co)
		{
			if (TeamSelectionChangedEvent != null)
				TeamSelectionChangedEvent (SelectedTeam);
		}

		public override void ClickPressed (Point point, ButtonModifier modif)
		{
			Selection selection = null;

			if (ButtonClickPressed (point, modif, subPlayers, subInjury,
				    homeButton, awayButton)) {
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
				(int)Height - BUTTONS_HEIGHT,
				out scaleX, out scaleY, out offset);
			offset.Y += BUTTONS_HEIGHT;
			tk.Begin ();
			tk.Clear (Config.Style.PaletteBackground);

			/* Draw substitution buttons */
			if (subPlayers.Visible) {
				subPlayers.Position = new Point (Width / 2 - BUTTONS_WIDTH / 2,
					offset.Y - BUTTONS_HEIGHT);
				subPlayers.Width = BUTTONS_WIDTH;
				subPlayers.Height = BUTTONS_HEIGHT;
				subPlayers.Draw (tk, area);
			}
			if (homeButton.Visible) {
				/* Draw local team button */
				double x = Position.X + Config.Style.TeamTaggerBenchBorder * scaleX + offset.X; 
				homeButton.Position = new Point (x, offset.Y - homeButton.Height);
				homeButton.Draw (tk, area);
			}
			if (awayButton.Visible) {
				double x = (Position.X + Width - offset.X - Config.Style.TeamTaggerBenchBorder * scaleX) - awayButton.Width; 
				awayButton.Position = new Point (x, offset.Y - awayButton.Height);
				awayButton.Draw (tk, area);
			}

			tk.TranslateAndScale (Position + offset, new Point (scaleX, scaleY));
			homeBench.Draw (tk, area);
			awayBench.Draw (tk, area);
			field.Draw (tk, area);
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
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

