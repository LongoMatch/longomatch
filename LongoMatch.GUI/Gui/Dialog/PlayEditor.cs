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
using System.Collections.ObjectModel;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Core;

namespace LongoMatch.Gui.Dialog
{
	public partial class PlayEditor : Gtk.Dialog
	{
		const int TAGS_PER_ROW = 5;
		TeamTagger teamtagger;
		TimelineEvent play;

		public PlayEditor (Window parent)
		{
			TransientFor = parent;
			this.Build ();
			teamtagger = new TeamTagger (new WidgetWrapper (drawingarea3));
			teamtagger.Compact = true;
			teamtagger.ShowSubstitutionButtons = false;
			teamtagger.SelectionMode = MultiSelectionMode.Multiple;
			teamtagger.PlayersSelectionChangedEvent += HandlePlayersSelectionChangedEvent;
			teamtagger.TeamSelectionChangedEvent += HandleTeamSelectionChangedEvent;
			teamtagger.ShowTeamsButtons = true;
			nameentry.Changed += HandleChanged;
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
			tagsvbox.Visible = editTags;

			nameentry.Text = play.Name;
			nameentry.GrabFocus ();

			if (editPos) {
				tagger.LoadBackgrounds (project);
				tagger.LoadPlay (play);
			}
			
			if (editNotes) {
				notes.Play = play;
			}
			if (editPlayers) {
				teamtagger.Project = project;
				teamtagger.LoadTeams (project.LocalTeamTemplate, project.VisitorTeamTemplate,
					project.Dashboard.FieldBackground);
				/* Force lineup update */
				teamtagger.CurrentTime = play.EventTime;
				teamtagger.Select (play.Players, play.Teams);
			}
		
			if (editTags) {
				FillTags (project, play);
			}
		}

		void AddTagsGroup (TimelineEvent evt, string grp, List<Tag> tags, SizeGroup sgroup)
		{
			HBox box = new HBox ();
			Label label = new Label (String.IsNullOrEmpty (grp) ? Catalog.GetString ("Common tags") : grp);
			Table tagstable = new Table ((uint)(tags.Count / TAGS_PER_ROW), TAGS_PER_ROW, true);
			RadioButton first = null;
			Tag noneTag = new Tag (Catalog.GetString ("None"));
			label.WidthRequest = 200;
			if (!String.IsNullOrEmpty (grp)) {
				tags.Insert (0, noneTag);
			}
			for (int i = 0; i < tags.Count; i++) {
				uint row_top, row_bottom, col_left, col_right;
				Tag t = tags [i];
				CheckButton tb;
				if (String.IsNullOrEmpty (grp)) {
					tb = new CheckButton (t.Value);
				} else {
					if (first == null) {
						tb = first = new RadioButton (t.Value);
					} else {
						tb = new RadioButton (first, t.Value);
					}
				}
				tb.Active = evt.Tags.Contains (t);
				tb.Toggled += (sender, e) => {
					if (tb.Active && t != noneTag) {
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
			}
			sgroup.AddWidget (label);
			box.PackStart (label, false, true, 0);
			box.PackEnd (tagstable, true, true, 0);
			box.Spacing = 5;
			tagsvbox.PackStart (box, false, true, 0);
			tagsvbox.PackStart (new HSeparator ());
		}

		void FillTags (Project project, TimelineEvent evt)
		{
			Dictionary<string, List<Tag>> tagsByGroup;
			SizeGroup sgroup = new SizeGroup (SizeGroupMode.Horizontal);
			
			if (evt.EventType is AnalysisEventType) {
				tagsByGroup = (evt.EventType as AnalysisEventType).TagsByGroup; 
			} else {
				tagsByGroup = new Dictionary<string, List<Tag>> ();
			}
			
			tagsvbox.PackStart (new HSeparator ());
			foreach (var kv in project.Dashboard.CommonTagsByGroup) {
				AddTagsGroup (evt, kv.Key, kv.Value, sgroup);
			}
			foreach (var kv in tagsByGroup) {
				AddTagsGroup (evt, kv.Key, kv.Value, sgroup);
			}
			tagsvbox.ShowAll ();
		}

		void HandleChanged (object sender, EventArgs e)
		{
			if (play != null) {
				play.Name = nameentry.Text;
			}
		}

		void HandlePlayersSelectionChangedEvent (List<Player> players)
		{
			play.Players = new ObservableCollection<Player> (players);
		}

		void HandleTeamSelectionChangedEvent (ObservableCollection<Team> teams)
		{
			play.Teams = teams;
		}
	}
}
