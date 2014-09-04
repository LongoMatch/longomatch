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
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Drawing.Widgets;
using System.Collections.Generic;
using LongoMatch.Drawing.Cairo;

namespace LongoMatch.Gui.Dialog
{
	public partial class PlayEditor : Gtk.Dialog
	{
		TeamTagger teamtagger;
		TimelineEvent play;

		public PlayEditor ()
		{
			this.Build ();
			teamtagger = new TeamTagger (new WidgetWrapper (drawingarea3));
			teamtagger.Compact = true;
			teamtagger.SelectionMode = MultiSelectionMode.Multiple;
			teamtagger.PlayersSelectionChangedEvent += HandlePlayersSelectionChangedEvent;
		}

		protected override void OnDestroyed ()
		{
			teamtagger.Dispose ();
			tagger.Destroy ();
			base.OnDestroyed ();
		}

		public void LoadPlay (TimelineEvent play, Project project, bool editTags, bool editPos, bool editPlayers, bool editNotes)
		{
			this.play = play;
			notesframe.Visible = editNotes;
			tagger.Visible = editPos && (play.EventType.TagFieldPosition ||
			                             play.EventType.TagHalfFieldPosition ||
			                             play.EventType.TagGoalPosition);
			drawingarea3.Visible = editPlayers;
			nameframe.Visible = editTags;

			nameentry.Text = play.Name;
			if (editPos) {
				tagger.LoadBackgrounds (project);
				tagger.LoadPlay (play);
			}
			
			if (editNotes) {
				notes.Play = play;
			}
			if (editPlayers) {
				teamtagger.LoadTeams (project.LocalTeamTemplate, project.VisitorTeamTemplate,
				                      project.Dashboard.FieldBackground);
				teamtagger.Select (play.Players);
			}
		}
		
		void HandlePlayersSelectionChangedEvent (List<Player> players)
		{
			play.Players = players.ToList(); 
		}
		
	}
}
