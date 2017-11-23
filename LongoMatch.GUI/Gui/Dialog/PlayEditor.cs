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
using Gtk;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Drawing.Cairo;

namespace LongoMatch.Gui.Dialog
{
	// Fixme: Change the view to not use the model, use the VM provided
	[ViewAttribute (PlayEditorState.NAME)]
	public partial class PlayEditor : Gtk.Dialog, IPanel<PlayEditorVM>
	{
		const int TAGS_PER_ROW = 5;
		LMTeamTaggerView teamtagger;
		TimelineEventLocationTaggerView field, hfield, goal;
		PlayEditorVM editorVM;

		public PlayEditor ()
		{
			this.Build ();
			field = new TimelineEventLocationTaggerView (new WidgetWrapper (fieldDrawingarea)) {
				FieldPosition = FieldPositionType.Field
			};
			hfield = new TimelineEventLocationTaggerView (new WidgetWrapper (hfieldDrawingarea)) {
				FieldPosition = FieldPositionType.HalfField
			};
			goal = new TimelineEventLocationTaggerView (new WidgetWrapper (goalDrawingarea)) {
				FieldPosition = FieldPositionType.Goal
			};

			teamtagger = new LMTeamTaggerView (new WidgetWrapper (drawingarea3));
			nameentry.Changed += HandleChanged;
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		public PlayEditorVM ViewModel {
			get {
				return editorVM;
			}
			set {
				editorVM = value;
			}
		}

		protected bool Disposed { get; private set; } = false;

		public void OnLoad ()
		{
			// FIXME: change to bindings
			notesframe.Visible = editorVM.EditionSettings.EditNotes;
			locationsBox.Visible = editorVM.EditionSettings.EditPositions &&
				(editorVM.Play.EventType.TagFieldPosition ||
				editorVM.Play.EventType.TagHalfFieldPosition ||
				editorVM.Play.EventType.TagGoalPosition);
			drawingarea3.Visible = editorVM.EditionSettings.EditPlayers;
			nameframe.Visible = editorVM.EditionSettings.EditTags;
			tagsvbox.Visible = editorVM.EditionSettings.EditTags;

			nameentry.Text = editorVM.Play.Name;
			nameentry.GrabFocus ();

			if (editorVM.EditionSettings.EditPositions) {
				LoadBackgrounds (editorVM.Project.Model);
				LoadTimelineEvent (editorVM.Play);
			}

			if (editorVM.EditionSettings.EditNotes) {
				notes.Play = editorVM.Play;
			}
			if (editorVM.EditionSettings.EditPlayers) {
				teamtagger.ViewModel = editorVM.TeamTagger;
			}

			if (editorVM.EditionSettings.EditTags) {
				FillTags (editorVM.Project.Model, editorVM.Play);
			}
		}

		public void OnUnload ()
		{
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = ((PlayEditorVM)viewModel as dynamic);
		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		protected override void OnResponse (ResponseType response_id)
		{
			base.OnResponse (response_id);
			App.Current.StateController.MoveBack ();
		}

		protected override void OnDestroyed ()
		{
			OnUnload ();
			teamtagger.Dispose ();
			field.Dispose ();
			hfield.Dispose ();
			goal.Dispose ();
			base.OnDestroyed ();
		}

		void LoadBackgrounds (Project project)
		{
			field.Background = project.GetBackground (FieldPositionType.Field);
			hfield.Background = project.GetBackground (FieldPositionType.HalfField);
			goal.Background = project.GetBackground (FieldPositionType.Goal);
		}

		void LoadTimelineEvent (LMTimelineEventVM evt)
		{
			fieldDrawingarea.Visible = evt.EventType.TagFieldPosition;
			hfieldDrawingarea.Visible = evt.EventType.TagHalfFieldPosition;
			goalDrawingarea.Visible = evt.EventType.TagGoalPosition;
			field.SetViewModel (evt);
			hfield.SetViewModel (evt);
			goal.SetViewModel (evt);
		}

		void AddTagsGroup (LMTimelineEventVM evt, string grp, List<Tag> tags, SizeGroup sgroup)
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

		void FillTags (LMProject project, LMTimelineEventVM eventVM)
		{
			Dictionary<string, List<Tag>> tagsByGroup;
			SizeGroup sgroup = new SizeGroup (SizeGroupMode.Horizontal);

			if (eventVM.EventType is AnalysisEventType) {
				tagsByGroup = (eventVM.EventType as AnalysisEventType).TagsByGroup;
			} else {
				tagsByGroup = new Dictionary<string, List<Tag>> ();
			}

			tagsvbox.PackStart (new HSeparator ());
			foreach (var kv in project.Dashboard.CommonTagsByGroup) {
				AddTagsGroup (eventVM, kv.Key, kv.Value, sgroup);
			}
			foreach (var kv in tagsByGroup) {
				AddTagsGroup (eventVM, kv.Key, kv.Value, sgroup);
			}
			tagsvbox.ShowAll ();
		}

		void HandleChanged (object sender, EventArgs e)
		{
			if (editorVM.Play != null) {
				editorVM.Play.Name = nameentry.Text;
			}
		}
	}
}
