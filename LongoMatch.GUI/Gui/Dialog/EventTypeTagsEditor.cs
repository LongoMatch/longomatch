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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Store;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui.Dialog
{
	public partial class EventTypeTagsEditor : Gtk.Dialog
	{
		const int COLS_PER_ROW = 4;
		AnalysisEventType eventType;
		Entry focusEntry;

		public EventTypeTagsEditor (Window parent)
		{
			TransientFor = parent;
			this.Build ();
		}

		public AnalysisEventType EventType {
			set {
				eventType = value;
				LoadEventType (value);
			}
			get {
				return eventType;
			}
		}

		void LoadEventType (AnalysisEventType eventType)
		{
			Button newGroupButton;

			var tagsByGroup = eventType.Tags.GroupBy (t => t.Group);
			
			foreach (var tagsGroup in tagsByGroup) {
				AddNewGroup (tagsGroup.Key, tagsGroup.ToList ());
			}
			newGroupButton = CreateAddGroupButton ();
			mainvbox.PackEnd (newGroupButton, false, true, 0);
			mainvbox.ShowAll ();
		}

		Widget CreateTagsTable (TagsGroup g)
		{
			int i = 0;
			uint rows, count;
			Alignment alignment;
			Table t;

			count = (uint)g.tags.Count + 1;
			rows = count / COLS_PER_ROW + 1;

			alignment = new Alignment (0.5F, 0.5F, 1, 1);
			alignment.LeftPadding = 20;
			t = new Table (rows, COLS_PER_ROW, false);
			t.RowSpacing = 2;
			t.ColumnSpacing = 5;
			foreach (Tag tag in g.tags) {
				CreateTagBox (t, tag, i, g);
				i++;
			}
			var addb = CreateAddTagButton (g);
			InsertInTable (t, addb, i);
			alignment.Add (t);
			g.table = alignment;
			return alignment;
		}

		void AddNewGroup (string name, List<Tag> tags)
		{
			TagsGroup g;
			Widget t;
			VBox vbox = new VBox (false, 5);
			HBox hbox = new HBox (false, 5);

			g = new TagsGroup (vbox, tags);
			hbox.PackStart (GroupBox (name, g), false, true, 0);
			t = CreateTagsTable (g);
			vbox.PackStart (hbox, true, true, 0);
			vbox.PackStart (t, true, true, 0);
			vbox.PackEnd (new HSeparator (), true, true, 0);
			vbox.ShowAll ();
			mainvbox.PackStart (vbox, true, true, 0);
		}

		void RemoveGroup (TagsGroup g)
		{
			string msg = Catalog.GetString ("Do you want to remove this subcategory and all its tags?");
			if (App.Current.Dialogs.QuestionMessage (msg, null, this).Result) {
				EventType.Tags.RemoveAll (g.tags.Contains);
				mainvbox.Remove (g.container);
			}
		}

		void RemoveTag (Tag tag, TagsGroup g)
		{
			string msg = Catalog.GetString ("Do you want to remove this tag?");
			if (App.Current.Dialogs.QuestionMessage (msg, null, this).Result) {
				EventType.Tags.Remove (tag);
				g.tags.Remove (tag);
				g.container.Remove (g.table);
				g.container.PackStart (CreateTagsTable (g), true, true, 0);
				g.container.ShowAll ();
			}
		}

		void AddTag (TagsGroup g)
		{
			Tag t = new Tag (Catalog.GetString ("New tag"), g.nameEntry.Text);
			EventType.Tags.Add (t);
			g.tags.Add (t);
			g.container.Remove (g.table);
			g.container.PackStart (CreateTagsTable (g), true, true, 0);
			g.container.ShowAll ();
			if (focusEntry != null) {
				focusEntry.GrabFocus ();
			}
		}

		Box GroupBox (string name, TagsGroup g)
		{
			HBox box = new HBox (false, 5);
			Label l = new Label ();
			Entry entry = new Entry (name);
			Button b = Button ("gtk-remove");
			
			l.Markup = Catalog.GetString ("<b>Subcategory name:</b>");
			g.nameEntry = entry;
			entry.Changed += (sender, e) => {
				foreach (Tag t in g.tags) {
					t.Group = entry.Text;
				}
			};
			b.Clicked += (sender, e) => RemoveGroup (g);
			box.PackStart (l, false, false, 0);
			box.PackStart (entry, false, true, 0);
			box.PackStart (b, false, false, 0);
			return box;
		}

		void CreateTagBox (Table t, Tag tag, int i, TagsGroup g)
		{
			HBox box = new HBox (false, 2);
			Entry tagEntry = new Entry (tag.Value);
			Label hotkeyLabel = new Label (tag.HotKey.ToString ());
			Button editHK = Button ("gtk-edit");
			Button b = Button ("gtk-remove");

			b.Clicked += (sender, e) => RemoveTag (tag, g);

			editHK.Clicked += (sender, e) => {
				HotKey hotkey = App.Current.GUIToolkit.SelectHotkey (tag.HotKey);
				if (hotkey != null) {
					try {
						if (EventType.Tags.Select (tt => tt.HotKey).Contains (hotkey)) {
							throw new HotkeyAlreadyInUse (hotkey);
						}
						tag.HotKey = hotkey;
						hotkeyLabel.Text = hotkey.ToString ();
					} catch (HotkeyAlreadyInUse ex) {
						App.Current.Dialogs.ErrorMessage (ex.Message, this);
					}
				}
			};

			tagEntry.Changed += (o, e) => { 
				tag.Value = tagEntry.Text;
			};
			focusEntry = tagEntry;

			box.PackStart (tagEntry, false, false, 0);
			box.PackStart (hotkeyLabel, false, false, 0);
			box.PackStart (editHK, false, false, 0);
			box.PackStart (b, false, false, 0);
			InsertInTable (t, box, i);
		}

		Button Button (string name)
		{
			Button b = new Button ();
			Alignment a = new Alignment (0.5F, 0.5F, 0F, 0F);
			Gtk.Image i = new Gtk.Image (Misc.LoadStockIcon (this, name, IconSize.Button));
			a.Add (i);
			b.Add (a);
			return b;
		}

		Button CreateButton (string s, IconSize size)
		{
			Button b = new Button ();
			Gtk.Image i = new Gtk.Image (Misc.LoadStockIcon (this, "gtk-add", size));
			Label l = new Label (s);
			HBox box = new HBox ();
			box.PackStart (i, false, false, 5);
			box.PackStart (l, false, false, 5);
			b.Add (box);
			return b;
		}

		Button CreateAddGroupButton ()
		{
			Button b = CreateButton (Catalog.GetString ("Add new subcategory"),
				           IconSize.LargeToolbar);
			b.Clicked += (sender, e) =>
				AddNewGroup (Catalog.GetString ("New subcategory"), new List<Tag> ());
			return b;
		}

		Button CreateAddTagButton (TagsGroup g)
		{
			Button b = CreateButton (Catalog.GetString ("Add new tag"), IconSize.Button);
			b.Clicked += (sender, e) => AddTag (g);
			return b;
		}

		void InsertInTable (Table table, Widget widget, int position)
		{
			uint row_top, row_bottom, col_left, col_right;
			row_top = (uint)(position / table.NColumns);
			row_bottom = (uint)row_top + 1;
			col_left = (uint)position % table.NColumns;
			col_right = (uint)col_left + 1;
			table.Attach (widget, col_left, col_right, row_top, row_bottom);
		}

		protected class TagsGroup
		{
			public List<Tag> tags;
			public Box container;
			public Entry nameEntry;
			public Widget table;

			public TagsGroup (Box container, List<Tag> tags)
			{
				this.container = container;
				this.tags = tags;
			}
		}
	}
}
