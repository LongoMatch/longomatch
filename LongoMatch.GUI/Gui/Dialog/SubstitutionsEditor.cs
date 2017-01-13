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
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.CanvasObjects.Teams;
using LongoMatch.Drawing.Widgets;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Drawing;
using VAS.Drawing.Cairo;

namespace LongoMatch.Gui.Dialog
{
	public partial class SubstitutionsEditor : Gtk.Dialog
	{
		TeamTagger tagger;
		SelectionCanvas incanvas, outcanvas;
		SportsPlayerObject inpo, outpo;
		LMPlayer inPlayer, outPlayer, selectedPlayer;
		LMTeam homeTeam, awayTeam;
		LineupEvent lineup;
		SubstitutionEvent substitution;
		const int PLAYER_SIZE = 100;

		public SubstitutionsEditor (Window parent)
		{
			TransientFor = parent;
			this.Build ();
			tagger = new TeamTagger (new WidgetWrapper (drawingarea));
			tagger.PlayersSelectionChangedEvent += HandlePlayersSelectionChangedEvent;
			tagger.PlayersSubstitutionEvent += HandlePlayersSubstitutionEvent;
			incanvas = new SelectionCanvas (new WidgetWrapper (drawingarea2));
			outcanvas = new SelectionCanvas (new WidgetWrapper (drawingarea3));
			inpo = new SportsPlayerObject ();
			outpo = new SportsPlayerObject ();
			inpo.ClickedEvent += HandleClickedEvent;
			outpo.ClickedEvent += HandleClickedEvent;
			inpo.Size = PLAYER_SIZE;
			outpo.Size = PLAYER_SIZE;
			inpo.Center = new Point (PLAYER_SIZE / 2, PLAYER_SIZE / 2);
			outpo.Center = new Point (PLAYER_SIZE / 2, PLAYER_SIZE / 2);
			incanvas.AddObject (inpo);
			outcanvas.AddObject (outpo);
			drawingarea2.WidthRequest = drawingarea2.HeightRequest = PLAYER_SIZE;
			drawingarea3.WidthRequest = drawingarea3.HeightRequest = PLAYER_SIZE;
		}

		public void SaveChanges ()
		{
			if (lineup != null) {
				lineup.HomeStartingPlayers = homeTeam.StartingPlayersList;
				lineup.HomeBenchPlayers = homeTeam.BenchPlayersList;
				lineup.AwayStartingPlayers = awayTeam.StartingPlayersList;
				lineup.AwayBenchPlayers = awayTeam.BenchPlayersList;
			} else {
				substitution.In = inPlayer;
				substitution.Out = outPlayer;
			}
		}

		public void Load (LMProject project, StatEvent evt)
		{
			if (evt is LineupEvent) {
				LoadLineup (project, evt as LineupEvent);
			} else {
				LoadSubstitution (project, evt as SubstitutionEvent);
			}
		}

		public void LoadLineup (LMProject project, LineupEvent lineup)
		{
			this.lineup = lineup;
			playershbox.Visible = false;
			tagger.SubstitutionMode = true;
			tagger.ShowSubstitutionButtons = false;
			LoadTeams (project, lineup.HomeStartingPlayers, lineup.HomeBenchPlayers,
				lineup.AwayStartingPlayers, lineup.AwayBenchPlayers);
		}

		public void LoadSubstitution (LMProject project, SubstitutionEvent substitution)
		{
			List<LMPlayer> hfp, hbp, afp, abp;

			this.substitution = substitution;
			project.CurrentLineup (substitution.EventTime, out hfp, out hbp, out afp, out abp);
			playershbox.Visible = true;
			tagger.SubstitutionMode = false;
			tagger.ShowSubstitutionButtons = false;
			tagger.SelectionMode = MultiSelectionMode.Single;
			if (substitution.Teams.Contains (project.LocalTeamTemplate)) {
				LoadTeams (project, hfp, hbp, null, null);
			} else {
				LoadTeams (project, null, null, afp, abp);
			}
			SwitchPlayer (substitution.In, substitution.Out);
		}

		void LoadTeams (LMProject project, List<LMPlayer> homeFieldPlayers, List<LMPlayer> homeBenchPlayers,
						List<LMPlayer> awayFieldPlayers, List<LMPlayer> awayBenchPlayers)
		{
			List<LMPlayer> homeTeamPlayers, awayTeamPlayers;

			if (homeFieldPlayers != null) {
				homeTeamPlayers = homeFieldPlayers.Concat (homeBenchPlayers).ToList ();
				homeTeam = new LMTeam {
					Colors = project.LocalTeamTemplate.Colors,
					ActiveColor = project.LocalTeamTemplate.ActiveColor,
					ID = project.LocalTeamTemplate.ID,
					Formation = project.LocalTeamTemplate.Formation,
				};
				homeTeam.List.Replace (homeTeamPlayers);
			}

			if (awayFieldPlayers != null) {
				awayTeamPlayers = awayFieldPlayers.Concat (awayBenchPlayers).ToList ();
				awayTeam = new LMTeam {
					Colors = project.VisitorTeamTemplate.Colors,
					ActiveColor = project.VisitorTeamTemplate.ActiveColor,
					ID = project.VisitorTeamTemplate.ID,
					Formation = project.VisitorTeamTemplate.Formation,
				};
				awayTeam.List.Replace (awayTeamPlayers);
			}

			tagger.LoadTeams (homeTeam, awayTeam, project.Dashboard.FieldBackground);
		}

		void SwitchPlayer (LMPlayer inPlayer, LMPlayer outPlayer)
		{
			if (inPlayer != null) {
				this.inPlayer = inPlayer;
				inpo.Player = inPlayer;
				inpo.Active = false;
				drawingarea2.QueueDraw ();
				tagger.ResetSelection ();
			} else {
				inframe.Visible = false;
			}
			if (outPlayer != null) {
				this.outPlayer = outPlayer;
				outpo.Player = outPlayer;
				outpo.Active = false;
				drawingarea3.QueueDraw ();
				tagger.ResetSelection ();
			} else {
				outframe.Visible = false;
			}
			selectedPlayer = null;
		}

		void HandlePlayersSelectionChangedEvent (List<LMPlayer> players)
		{
			if (players.Count == 1) {
				selectedPlayer = players [0];
				if (inpo.Active) {
					SwitchPlayer (selectedPlayer, outPlayer);
				} else if (outpo.Active) {
					SwitchPlayer (inPlayer, selectedPlayer);
				}
			} else {
				selectedPlayer = null;
			}
		}

		void HandlePlayersSubstitutionEvent (LMTeam team, LMPlayer p1, LMPlayer p2, SubstitutionReason reason, Time time)
		{
			tagger.Substitute (p1, p2, team);
			if (team.ID == homeTeam.ID) {
				homeTeam.List.Swap (p1, p2);
			} else {
				awayTeam.List.Swap (p1, p2);
			}
		}

		void HandleClickedEvent (ICanvasObject co)
		{
			SportsPlayerObject po = co as SportsPlayerObject;

			if (po == inpo) {
				if (outpo.Active) {
					outpo.Active = false;
					drawingarea3.QueueDraw ();
				}
				if (selectedPlayer != null) {
					SwitchPlayer (selectedPlayer, outPlayer);
				}
			} else {
				if (inpo.Active) {
					inpo.Active = false;
					drawingarea2.QueueDraw ();
				}
				if (selectedPlayer != null) {
					SwitchPlayer (inPlayer, selectedPlayer);
				}
			}
		}

	}
}
