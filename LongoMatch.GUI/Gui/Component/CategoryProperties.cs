// CategoryProperties.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using Gtk;
using Mono.Unix;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Dialog;
using Point = LongoMatch.Core.Common.Point;

namespace LongoMatch.Gui.Component
{
	public delegate void HotKeyChangeHandler (HotKey prevHotKey,DashboardButton button);
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial  class CategoryProperties : Gtk.Bin
	{

		public event HotKeyChangeHandler HotKeyChanged;
		public event EventHandler EditedEvent;

		SizeGroup sizegroup;
		DashboardButton button;
		TimedDashboardButton timedButton;
		EventButton eventButton;
		AnalysisEventButton catButton;
		PenaltyCardButton cardButton;
		ScoreButton scoreButton;
		TagButton tagButton;
		Time lastLeadTime;
		bool edited, ignore;

		public CategoryProperties ()
		{
			this.Build ();

			nameentry.Changed += HandleNameentryChanged;
			colorbutton1.ColorSet += HandleColorSet;
			textcolorbutton.ColorSet += HandleColorSet;
			tagmodecombobox.Changed += HandleTagModeChanged;
			tagscheckbutton.Toggled += HandleTagsToggled;
			tprbutton.ValueChanged += HandleTagsPerRowValueChanged;
			leadtimebutton.ValueChanged += HandleLeadTimeChanged;
			lagtimebutton.ValueChanged += HandleLagTimeChanged;
			changebuton.Clicked += HandleChangeHotkey;
			sortmethodcombobox.Changed += HandleSortMethodChanged;
			fieldcombobox.Changed += HandlePositionChanged;
			hfieldcombobox.Changed += HandlePositionChanged;
			goalcombobox.Changed += HandlePositionChanged;
			shapecombobox.Changed += HandleShapeChanged;
			pointsbutton.Changed += HandlePointsChanged;

			postable.NoShowAll = true;
			cattable.NoShowAll = true;
			scoretable.NoShowAll = true;

			sizegroup = new SizeGroup (SizeGroupMode.Horizontal);
			sizegroup.IgnoreHidden = false;
			foreach (Widget w in vbox3.Children) {
				foreach (Widget t in (w as Table).Children) {
					if (!(t is Label)) {
						sizegroup.AddWidget (t);
					}
				}
			}

			CanChangeHotkey = true;
			Tagger = null;

			UpdateGui ();
		}

		public bool Edited {
			set {
				edited = value;
				if (!ignore && EditedEvent != null) {
					EditedEvent (this, null);
				}
			}
			get {
				return edited;
			}
		}

		public bool CanChangeHotkey {
			set {
				if (value == true)
					changebuton.Sensitive = true;
			}
		}

		public EventType EventType {
			set {
				EventButton button = new EventButton { EventType = value };
				Tagger = button;
				timetable.Visible = false;
				texttable.Visible = false;
			}
		}

		public DashboardButton Tagger {
			set {
				button = value;
				eventButton = value as EventButton;
				timedButton = value as TimedDashboardButton;
				catButton = value as AnalysisEventButton;
				cardButton = value as PenaltyCardButton;
				scoreButton = value as ScoreButton;
				tagButton = value as TagButton;
				UpdateGui ();
			}
			get {
				return catButton;
			}
		}

		public Project Project {
			set;
			get;
		}

		void SetPositionCombo (ComboBox box, bool tagField, bool asTrayectory)
		{
			if (!tagField) {
				box.Active = 0;
			} else if (!asTrayectory) {
				box.Active = 1;
			} else {
				box.Active = 2;
			}
			Edited = true;
		}

		void ReadPositionCombo (ComboBox box, out bool tagField, out bool asTrayectory)
		{
			if (box.Active == 0) {
				tagField = true;
				asTrayectory = false;
			} else if (box.Active == 1) {
				tagField = true;
				asTrayectory = true;
			} else {
				tagField = false;
				asTrayectory = false;
			}
		}

		private void  UpdateGui ()
		{
			ignore = true;
			
			cattable.Visible = catButton != null;
			timetable.Visible = timedButton != null;
			postable.Visible = eventButton != null;
			scoretable.Visible = scoreButton != null;
			cardtable.Visible = cardButton != null;

			if (button != null) {
				nameentry.Text = button.Name;
				colorbutton1.Color = Helpers.Misc.ToGdkColor (button.BackgroundColor);
				textcolorbutton.Color = Helpers.Misc.ToGdkColor (button.TextColor);
				if (button.HotKey != null && button.HotKey.Defined)
					hotKeyLabel.Text = button.HotKey.ToString ();
				else
					hotKeyLabel.Text = Catalog.GetString ("none");
			} else {
				nameentry.Text = "";
				colorbutton1.Color = new Gdk.Color (0, 0, 0);
				textcolorbutton.Color = new Gdk.Color (0, 0, 0);
				lastLeadTime = new Time ();
				tagmodecombobox.Active = 0;
				leadtimebutton.Value = 0;
				lagtimebutton.Value = 0;
				sortmethodcombobox.Active = 0;
				hotKeyLabel.Text = Catalog.GetString ("none");
			}
			if (timedButton != null) {
				lastLeadTime = timedButton.Start;
				tagmodecombobox.Active = (int)timedButton.TagMode;
				leadtimebutton.Value = timedButton.Start.Seconds;
				lagtimebutton.Value = timedButton.Stop.Seconds;
			}
			if (eventButton != null) {
				SetPositionCombo (fieldcombobox, eventButton.EventType.TagFieldPosition,
				                  eventButton.EventType.FieldPositionIsDistance);
				SetPositionCombo (hfieldcombobox, eventButton.EventType.TagHalfFieldPosition,
				                  eventButton.EventType.HalfFieldPositionIsDistance);
				SetPositionCombo (goalcombobox, eventButton.EventType.TagGoalPosition, false);
			}
			if (catButton != null) {
				sortmethodcombobox.Active = (int)catButton.AnalysisEventType.SortMethod;
				tagscheckbutton.Active = catButton.ShowSubcategories;
				tprbutton.Value = catButton.TagsPerRow;
			}
			if (scoreButton != null) {
				pointsbutton.Value = scoreButton.Score.Points;
			}
			if (cardButton != null) {
				shapecombobox.Active = (int)cardButton.PenaltyCard.Shape;
			}
			ignore = false;
			Edited = false;
		}

		void HandleChangeHotkey (object sender, System.EventArgs e)
		{
			if (ignore)
				return;

			HotKeySelectorDialog dialog = new HotKeySelectorDialog ();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;
			HotKey prevHotKey = button.HotKey;
			if (dialog.Run () == (int)ResponseType.Ok) {
				button.HotKey = dialog.HotKey;
				UpdateGui ();
			}
			dialog.Destroy ();
			if (HotKeyChanged != null)
				HotKeyChanged (prevHotKey, button);
			Edited = true;
		}

		void HandlePositionChanged (object sender, EventArgs e)
		{
			if (ignore == true)
				return;

			bool tag = false, trayectory = false;
			
			ReadPositionCombo (sender as ComboBox, out tag, out trayectory);
			if (sender == fieldcombobox) {
				eventButton.EventType.TagFieldPosition = tag;
				eventButton.EventType.FieldPositionIsDistance = trayectory;
			} else if (sender == hfieldcombobox) {
				eventButton.EventType.TagHalfFieldPosition = tag;
				eventButton.EventType.HalfFieldPositionIsDistance = trayectory;
			} else {
				eventButton.EventType.TagGoalPosition = tag;
			}
			Edited = true;
		}

		void HandleTagsPerRowValueChanged (object sender, EventArgs e)
		{
			if (ignore)
				return;

			catButton.TagsPerRow = tprbutton.ValueAsInt;
			Edited = true;
		}

		void HandleTagsToggled (object sender, EventArgs e)
		{
			if (ignore)
				return;

			catButton.ShowSubcategories = tagscheckbutton.Active;
			Edited = true;
		}

		void HandleTagModeChanged (object sender, EventArgs e)
		{
			if (ignore)
				return;

			timedButton.TagMode = (TagMode)tagmodecombobox.Active;
			if (timedButton.TagMode == TagMode.Predefined) {
				lagtimebutton.Sensitive = true;
				leadtimebutton.Value = lastLeadTime.Seconds;
			} else {
				lagtimebutton.Sensitive = false;
				lastLeadTime = timedButton.Start;
				leadtimebutton.Value = 0;
			}
			Edited = true;
		}

		void HandleColorSet (object sender, EventArgs e)
		{
			if (ignore)
				return;

			LongoMatch.Core.Common.Color c = Helpers.Misc.ToLgmColor ((sender as ColorButton).Color);
			if (sender == colorbutton1) {
				button.BackgroundColor = c;
			} else {
				button.TextColor = c;
			}
			Edited = true;
		}

		void HandleLeadTimeChanged (object sender, System.EventArgs e)
		{
			if (ignore)
				return;

			timedButton.Start = new Time { Seconds=(int)leadtimebutton.Value };
			Edited = true;
		}

		void HandleLagTimeChanged (object sender, System.EventArgs e)
		{
			if (ignore)
				return;

			timedButton.Stop = new Time { Seconds=(int)lagtimebutton.Value };
			Edited = true;
		}

		void HandleNameentryChanged (object sender, System.EventArgs e)
		{
			if (ignore)
				return;

			button.Name = nameentry.Text;
			Edited = true;
		}

		void HandleSortMethodChanged (object sender, System.EventArgs e)
		{
			if (ignore)
				return;

			catButton.EventType.SortMethodString = sortmethodcombobox.ActiveText;
			Edited = true;
		}

		void HandleShapeChanged (object sender, EventArgs e)
		{
			if (ignore)
				return;

			cardButton.PenaltyCard.Shape = (CardShape)shapecombobox.Active;
			Edited = true;
		}

		void HandlePointsChanged (object sender, EventArgs e)
		{
			if (ignore)
				return;

			scoreButton.Score.Points = pointsbutton.ValueAsInt;
			Edited = true;
		}
	}
}
