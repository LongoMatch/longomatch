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
using Gtk;

namespace LongoMatch.Gui.Dialog
{
	public partial class PlayEditor : Gtk.Dialog
	{
		const int TAGS_PER_ROW = 5;
		TeamTagger teamtagger;
		TimelineEvent play;

		public PlayEditor ()
		{
			this.Build ();
			teamtagger = new TeamTagger (new WidgetWrapper (drawingarea3));
			teamtagger.Compact = true;
			teamtagger.ShowSubstitutionButtons = false;
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
			tagstable.Visible = editTags;

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
		
			if (editTags) {
				FillTags (project, play);
			}
		}

		void FillTags (Project project, TimelineEvent evt)
		{
			List<Tag> tags;
			
			if (evt.EventType is AnalysisEventType) {
				tags = (evt.EventType as AnalysisEventType).Tags.ToList ();
			} else {
				tags = new List<Tag> ();
			}
			tags.AddRange (project.Dashboard.List.OfType<TagButton> ().Select (t => t.Tag).ToList ());
			tags = tags.Union (evt.Tags).ToList ();
			
			tagstable.NRows = (uint)(tags.Count / TAGS_PER_ROW);
			for (int i=0; i < tags.Count; i++) {
				uint row_top, row_bottom, col_left, col_right;
				Tag t = tags [i];
				ToggleButton tb = new ToggleButton (t.Value);
				tb.Active = evt.Tags.Contains (t);
				tb.Toggled += (sender, e) => {
					if (tb.Active) {
						evt.Tags.Add (t);
					} else {
						evt.Tags.Remove (t);
					}
				};
				row_top = (uint)(i / tagstable.NColumns);
				row_bottom = (uint)row_top + 1;
				col_left = (uint)i % tagstable.NColumns;
				col_right = (uint)col_left + 1;
				tagstable.Attach (tb, col_left, col_right, row_top, row_bottom);
				tb.Show ();
			}
			
		}

		void HandlePlayersSelectionChangedEvent (List<Player> players)
		{
			play.Players = players.ToList (); 
		}
	}
}
