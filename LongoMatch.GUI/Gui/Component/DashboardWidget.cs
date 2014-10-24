// ButtonsWidget.cs
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
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Drawing.Widgets;
using Mono.Unix;
using LongoMatch.Gui.Dialog;
using Helpers = LongoMatch.Gui.Helpers;
using Image = LongoMatch.Core.Common.Image;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DashboardWidget : Gtk.Bin
	{
		public event NewEventHandler NewTagEvent;

		TagMode tagMode;
		DashboardCanvas tagger;
		Dashboard template;
		DashboardButton selected;
		Gtk.Image editimage;
		ToggleToolButton editbutton;
		RadioToolButton d11button, fillbutton, fitbutton;
		bool internalButtons, edited, ignoreChanges;
		Project project;

		public DashboardWidget()
		{
			this.Build();

			addcatbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-category", IconSize.Button);
			addtimerbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-timer", IconSize.Button);
			addcardbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-card", IconSize.Button);
			addscorebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-score", IconSize.Button);
			addtagbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-tag", IconSize.Button);
			applyimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-apply", IconSize.Button);

			tagger = new DashboardCanvas (new WidgetWrapper (drawingarea));
			tagger.TaggersSelectedEvent += HandleTaggersSelectedEvent;
			tagger.ShowMenuEvent += HandleShowMenuEvent;
			tagger.NewTagEvent += HandleNewTagEvent;
			tagger.EditButtonTagsEvent += HandleAddNewTagEvent;
			drawingarea.CanFocus = true;
			drawingarea.KeyPressEvent += HandleKeyPressEvent;
			fieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			hfieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			goaleventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			resetfieldbutton.Clicked += HandleResetField;
			resethfieldbutton.Clicked += HandleResetField;
			resetgoalbutton.Clicked += HandleResetField;
			tagproperties.EditedEvent += HandlePropertiedEditedEvent;
			addcatbutton.Clicked += HandleAddClicked;
			addtimerbutton.Clicked += HandleAddClicked;
			addscorebutton.Clicked += HandleAddClicked;
			addtagbutton.Clicked += HandleAddClicked;
			addcardbutton.Clicked += HandleAddClicked;
			applybutton.Clicked += HandleApplyClicked;

			FillToolbar ();
			FitMode = FitMode.Original;
			Edited = false;
			Mode = TagMode.Predefined;
			// Initialize to a sane default value.
			tagproperties.Sensitive = false;
		}

		protected override void OnDestroyed ()
		{
			tagger.Dispose ();
			base.OnDestroyed ();
		}

		public Time CurrentTime {
			set {
				tagger.CurrentTime = value;
			}
		}
		
		public FitMode FitMode {
			set {
				ignoreChanges = true;
				if (value == FitMode.Original) {
					d11button.Active = true;
					dashscrolledwindow.HscrollbarPolicy = PolicyType.Automatic;
					dashscrolledwindow.VscrollbarPolicy = PolicyType.Automatic;
				} else {
					if (value == FitMode.Fill) {
						fillbutton.Active = true;
					} else if (value == FitMode.Fit) {
						fitbutton.Active = true;
					}
					drawingarea.WidthRequest = -1;
					drawingarea.HeightRequest = -1;
					dashscrolledwindow.HscrollbarPolicy = PolicyType.Never;
					dashscrolledwindow.VscrollbarPolicy = PolicyType.Never;
				}
				tagger.FitMode = value;
				ignoreChanges = false;
			}
		}

		public bool Edited {
			get {
				return edited || tagger.Edited || tagproperties.Edited;
			}
			set {
				edited = tagger.Edited = tagproperties.Edited = value;
			}
		}

		public Project Project {
			set {
				project = value;
				tagger.Project = project;
				Template = project.Dashboard;
				positionsbox.Visible = false;
				periodsbox.Visible = false;
			}
		}

		public Dashboard Template {
			set {
				template = value;
				tagger.Template = value;
				try {
					fieldimage.Pixbuf = value.FieldBackground.Scale (50, 50).Value;
					hfieldimage.Pixbuf = value.HalfFieldBackground.Scale (50, 50).Value;
					goalimage.Pixbuf = value.GoalBackground.Scale (50, 50).Value;
				} catch {
				}
				periodsentry.Text = String.Join ("-", template.GamePeriods);
				Edited = false;
				// Start with disabled widget until something get selected
				tagproperties.Tagger = null;
				tagproperties.Sensitive = false;
				tagproperties.Dashboard = value;
			}
		}

		public TagMode Mode {
			set {
				ignoreChanges = true;
				tagMode = value;
				tagger.TagMode = value;
				// Properties only visible in edit mode
				rightbox.Visible = tagMode == TagMode.Edit;
				// Add buttons for cards/tags/etc.. can be handled remotely.
				hbuttonbox2.Visible = tagMode == TagMode.Edit && internalButtons;
				editbutton.Active = value == TagMode.Edit;
				if (value == TagMode.Edit) {
					editimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-dash-edit_active",
					                                          22, IconLookupFlags.ForceSvg);
				} else {
					editimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-dash-edit",
					                                          22, IconLookupFlags.ForceSvg);
				}
				LongoMatch.Gui.Helpers.Misc.SetFocus (this, value == TagMode.Edit);
				if (project != null) {
					if (value == TagMode.Edit) {
						Edited = false;
					} else {
						if (Edited)
							Config.EventsBroker.EmitDashboardEdited ();
					}
					
				}
				ignoreChanges = false;
			}
			get {
				return tagMode;
			}
		}

		public bool ButtonsVisible {
			set {
				internalButtons = value;
				Mode = tagMode;
			}
		}

		public void ClickButton (DashboardButton button)
		{
			tagger.Click (button);
		}
		
		public void Refresh (DashboardButton b = null) {
			tagger.Refresh (b);
		}

		public void AddButton (string buttontype) {
			DashboardButton button = null;

			if (buttontype == "Card") {
				button = new PenaltyCardButton {
					PenaltyCard = new PenaltyCard ("Red", Color.Red, CardShape.Rectangle)};
			} else if (buttontype == "Score") {
				button = new ScoreButton {
					Score = new Score ("Score", 1)};
			} else if (buttontype == "Timer") {
				button = new TimerButton {Timer = new Timer {Name = "Timer"}};
			} else if (buttontype == "Tag") {
				button = new TagButton {Tag = new Tag ("Tag", "")};
			} else if (buttontype == "Category") {
				button = template.AddDefaultItem (template.List.Count);
			} else {
				return;
			}

			if (buttontype != "Category") {
				template.List.Add (button);
			}
			button.Position = new Point (template.CanvasWidth, 0);
			edited = true;
			Refresh (button);
		}
		
		void RemoveButton (DashboardButton button) {
			string msg = Catalog.GetString ("Do you want to delete: ") +
				button.Name + "?";
			if (Config.GUIToolkit.QuestionMessage (msg, null, this)) {
				template.List.Remove (button);
				Refresh ();
			}
		}
		
		void FillToolbar () {
			Toolbar toolbar = new Toolbar ();
			toolbar.Orientation = Orientation.Vertical;
			toolbar.ToolbarStyle = ToolbarStyle.Icons;
			
			editimage = new Gtk.Image (Helpers.Misc.LoadIcon ("longomatch-dash-edit_active",
			                                          22, IconLookupFlags.ForceSvg));
			editbutton = new ToggleToolButton ();
			editbutton.IconWidget = editimage;
			editbutton.Active = true;
			editbutton.Toggled += HandleEditToggled;
			toolbar.Add (editbutton);
			toolbar.Add (new SeparatorToolItem ());
			
			fitbutton = new RadioToolButton ((GLib.SList) null);
			fitbutton.IconName = "longomatch-dash-fit";
			fitbutton.Toggled += HandleFitModeToggled;
			toolbar.Add (fitbutton);
			fillbutton = new RadioToolButton (fitbutton);
			fillbutton.IconName = "longomatch-dash-fill";
			fillbutton.Toggled += HandleFitModeToggled;
			toolbar.Add (fillbutton);
			d11button = new RadioToolButton (fitbutton);
			d11button.IconName = "longomatch-dash-11";
			d11button.Toggled += HandleFitModeToggled;
			toolbar.Add (d11button);
			toolbar.ShowAll ();
			hbox2.PackEnd (toolbar, false, false, 0);
		}

		void UpdateBackground (Image background, int index)
		{
			if (index == 0) {
				template.FieldBackground = background;
				fieldimage.Pixbuf = background.Scale (50, 50).Value;
			} else if (index == 1) {
				template.HalfFieldBackground = background;
				hfieldimage.Pixbuf = background.Scale (50, 50).Value;
			} else if (index == 2) {
				template.GoalBackground = background;
				goalimage.Pixbuf = background.Scale (50, 50).Value;
			}
			Edited = true;
		}
		
		void HandleEditToggled (object sender, EventArgs e)
		{
			if (ignoreChanges) {
				return;
			}
			if (editbutton.Active) {
				Mode = TagMode.Edit;
			} else {
				Mode = TagMode.Predefined;
			}
		}

		void HandleTaggersSelectedEvent (List<DashboardButton> taggers)
		{
			if (taggers.Count == 1) {
				selected = taggers[0];
				tagproperties.Tagger = taggers[0];
				tagproperties.Sensitive = true;
			} else {
				selected = null;
				tagproperties.Tagger = null;
				tagproperties.Sensitive = false;
			}
		}
		
		void HandleFieldButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			LongoMatch.Core.Common.Image background;
			Gdk.Pixbuf pix = Helpers.Misc.OpenImage (this);
			
			if (pix == null) {
				return;
			}
			
			background = new Image (pix);
			background.ScaleInplace (Constants.MAX_BACKGROUND_WIDTH,
			                         Constants.MAX_BACKGROUND_HEIGHT);
			if (o == fieldeventbox) {
				UpdateBackground (background, 0);
			} else if (o == hfieldeventbox) {
				UpdateBackground (background, 1);
			} else if (o == goaleventbox) {
				UpdateBackground (background, 2);
			}
			Edited = true;
		}

		void HandleTick (Time currentTime)
		{
			tagger.CurrentTime = currentTime;
		}
		
		void HandleKeyPressEvent (object o, KeyPressEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.Delete && selected != null) {
				RemoveButton (selected);
			}
		}
		
		void HandleShowMenuEvent (DashboardButton taggerbutton, Tag tag)
		{
			Menu menu;
			MenuItem delbut, deltag;
			
			if (Mode != TagMode.Edit) {
				return;
			}
			
			menu = new Menu ();
			delbut = new MenuItem (Catalog.GetString ("Delete"));
			delbut.Activated += (sender, e) => {RemoveButton (taggerbutton);};
			menu.Add (delbut);
			menu.ShowAll ();
			menu.Popup ();
		}

		void HandleAddClicked (object sender, EventArgs e) {
			if (sender == addcardbutton) {
				AddButton ("Card");
			} else if (sender == addscorebutton) {
				AddButton ("Score");
			} else if (sender == addtimerbutton) {
				AddButton ("Timer");
			} else if (sender == addtagbutton) {
				AddButton ("Tag");
			} else if (sender == addcatbutton) {
				AddButton ("Category");
			} else {
				return;
			}
		}
		
		void HandleNewTagEvent (EventType evntType, List<Player> players, Team team, List<Tag> tags,
		                        Time start, Time stop, Time eventTime, Score score, PenaltyCard card)
		{
			/* Forward event until we have players integrted in the dashboard layout */
			if (NewTagEvent != null) {
				NewTagEvent (evntType , players, team, tags, start, stop, eventTime, score, card);
			}
			//Config.EventsBroker.EmitNewTag (button, players, tags, start, stop);
		}

		void HandleAddNewTagEvent (DashboardButton taggerbutton)
		{
			AnalysisEventType evt = (taggerbutton as AnalysisEventButton).AnalysisEventType;
			EventTypeTagsEditor dialog = new EventTypeTagsEditor ();
			dialog.EventType = evt;
			dialog.Run ();
			dialog.Destroy ();
			Edited = true;
		}

		void HandleResetField (object sender, EventArgs e)
		{
			if (sender == resetfieldbutton) {
				UpdateBackground (Config.FieldBackground, 0);
			} else if (sender == resethfieldbutton) {
				UpdateBackground (Config.HalfFieldBackground, 1);
			} else if (sender == resetgoalbutton) {
				UpdateBackground (Config.GoalBackground, 2);
			}
		}

		void HandleFitModeToggled (object sender, EventArgs e)
		{
			if (ignoreChanges || !(sender as RadioToolButton).Active) {
				return;
			}
			
			if (sender == fitbutton) {
				FitMode = FitMode.Fit;
			} else if (sender == fillbutton) {
				FitMode = FitMode.Fill;
			} else if (sender == d11button) {
				FitMode = FitMode.Original;
			}
			
		}
		
		void HandlePropertiedEditedEvent (object sender, EventArgs e)
		{
			if (selected != null) {
				tagger.RedrawButton (selected);
			}
		}
		
		void HandleApplyClicked (object sender, EventArgs e)
		{
			try {
				template.GamePeriods = periodsentry.Text.Split ('-').ToList ();
				Edited = true;
			} catch {
				Config.GUIToolkit.ErrorMessage (Catalog.GetString ("Could not parse game periods."));
			}
		}
	}
}
