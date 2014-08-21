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
using System.Collections.Generic;
using Gdk;
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Popup;
using LongoMatch.Gui.Helpers;
using Point = LongoMatch.Common.Point;

namespace LongoMatch.Gui.Component
{

	public delegate void HotKeyChangeHandler(HotKey prevHotKey, Category newSection);

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial  class CategoryProperties : Gtk.Bin
	{

		public event HotKeyChangeHandler HotKeyChanged;
		public event EventHandler EditedEvent;
		SizeGroup sizegroup;

		TaggerButton tagger;
		Category cat;
		PenaltyCard card;
		Score score;
		Time lastLeadTime;
		bool edited, ignore;

		public CategoryProperties()
		{
			this.Build();
			nameentry.Changed += HandleNameentryChanged;
			colorbutton1.ColorSet += HandleColorSet;
			colorbutton2.ColorSet += HandleColorSet;
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

		public TaggerButton Tagger {
			set {
				tagger = value;
				cat = value as Category;
				card = value as PenaltyCard;
				score = value as Score;
				UpdateGui();
			}
			get {
				return cat;
			}
		}
		
		public Project Project {
			set;
			get;
		}
		
		void SetPositionCombo (ComboBox box, bool tagField, bool asTrayectory) {
			if (!tagField) {
				box.Active = 0;
			} else if (!asTrayectory) {
				box.Active = 1;
			} else {
				box.Active = 2;
			}
			Edited = true;
		}
		
		void ReadPositionCombo (ComboBox box, out bool tagField, out bool asTrayectory) {
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
		
		private void  UpdateGui() {
			ignore = true;
			
			cattable.Visible = cat != null;
			scoretable.Visible = score != null;
			cardtable.Visible = card != null;

			if (tagger != null) {
				nameentry.Text = tagger.Name;
				colorbutton1.Color = Helpers.Misc.ToGdkColor(tagger.Color);
				colorbutton2.Color = Helpers.Misc.ToGdkColor(tagger.TextColor);
				lastLeadTime = tagger.Start;
				tagmodecombobox.Active = (int)tagger.TagMode;
				leadtimebutton.Value = tagger.Start.Seconds;
				lagtimebutton.Value = tagger.Stop.Seconds;
			}
			if(cat != null) {
				tagscheckbutton.Active = cat.ShowSubcategories;
				tprbutton.Value = cat.TagsPerRow;
				sortmethodcombobox.Active = (int)cat.SortMethod;
				SetPositionCombo (fieldcombobox, cat.TagFieldPosition, cat.FieldPositionIsDistance);
				SetPositionCombo (hfieldcombobox, cat.TagHalfFieldPosition, cat.HalfFieldPositionIsDistance);
				SetPositionCombo (goalcombobox, cat.TagGoalPosition, false);
				if(cat.HotKey.Defined)
					hotKeyLabel.Text = cat.HotKey.ToString();
				else
					hotKeyLabel.Text = Catalog.GetString("none");
			}
			if (score != null) {
				pointsbutton.Value = score.Points;
			}
			if (card != null) {
				shapecombobox.Active = (int) card.Shape;
			}
			ignore = false;
			Edited = false;
		}
		
		void HandleChangeHotkey(object sender, System.EventArgs e)
		{
			HotKeySelectorDialog dialog = new HotKeySelectorDialog();
			dialog.TransientFor=(Gtk.Window)this.Toplevel;
			HotKey prevHotKey =  cat.HotKey;
			if(dialog.Run() == (int)ResponseType.Ok) {
				cat.HotKey=dialog.HotKey;
				UpdateGui();
			}
			dialog.Destroy();
			if(HotKeyChanged != null)
				HotKeyChanged(prevHotKey,cat);
			Edited = true;
		}

		void HandlePositionChanged (object sender, EventArgs e)
		{
			bool tag = false, trayectory = false;
			
			ReadPositionCombo (sender as ComboBox, out tag, out trayectory);
			if (sender == fieldcombobox) {
				cat.TagFieldPosition = tag;
				cat.FieldPositionIsDistance = trayectory;
			} else if (sender == hfieldcombobox) {
				cat.TagHalfFieldPosition = tag;
				cat.HalfFieldPositionIsDistance = trayectory;
			} else {
				cat.TagGoalPosition = tag;
			}
			Edited = true;
		}

		void HandleTagsPerRowValueChanged (object sender, EventArgs e)
		{
			cat.TagsPerRow = tprbutton.ValueAsInt;
			Edited = true;
		}
		
		void HandleTagsToggled (object sender, EventArgs e)
		{
			cat.ShowSubcategories = tagscheckbutton.Active;
			Edited = true;
		}

		void HandleTagModeChanged (object sender, EventArgs e)
		{
			tagger.TagMode = (TagMode) tagmodecombobox.Active;
			if (tagger.TagMode == TagMode.Predefined) {
				lagtimebutton.Sensitive = true;
				leadtimebutton.Value = lastLeadTime.Seconds;
			} else {
				lagtimebutton.Sensitive = false;
				lastLeadTime = tagger.Start;
				leadtimebutton.Value = 0;
			}
			Edited = true;
		}
		
		void HandleColorSet (object sender, EventArgs e)
		{
			LongoMatch.Common.Color c = Helpers.Misc.ToLgmColor((sender as ColorButton).Color);
			if (sender == colorbutton1) {
				tagger.Color = c;
			} else {
				tagger.TextColor = c;
			}
			Edited = true;
		}
		
		void HandleLeadTimeChanged(object sender, System.EventArgs e)
		{
			tagger.Start = new Time{Seconds=(int)leadtimebutton.Value};
			Edited = true;
		}

		void HandleLagTimeChanged(object sender, System.EventArgs e)
		{
			tagger.Stop = new Time{Seconds=(int)lagtimebutton.Value};
			Edited = true;
		}

		void HandleNameentryChanged(object sender, System.EventArgs e)
		{
			tagger.Name = nameentry.Text;
			Edited = true;
		}

		void HandleSortMethodChanged(object sender, System.EventArgs e)
		{
			cat.SortMethodString = sortmethodcombobox.ActiveText;
			Edited = true;
		}
		
		void HandleShapeChanged (object sender, EventArgs e)
		{
			card.Shape = (CardShape) shapecombobox.Active;
			Edited = true;
		}
		
		void HandlePointsChanged (object sender, EventArgs e)
		{
			score.Points = pointsbutton.ValueAsInt;
			Edited = true;
		}

	}
}
