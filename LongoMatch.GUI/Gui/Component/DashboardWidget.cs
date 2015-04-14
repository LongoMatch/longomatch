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
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class DashboardWidget : Gtk.Bin
	{
		public event NewEventHandler NewTagEvent;

		DashboardMode mode;
		DashboardCanvas tagger;
		Dashboard template;
		DashboardButton selected;
		Gtk.Image editimage, linksimage;
		ToggleToolButton editbutton, linksbutton, popupbutton;
		RadioToolButton d11button, fillbutton, fitbutton;
		bool internalButtons, edited, ignoreChanges;
		Project project;

		public DashboardWidget ()
		{
			this.Build ();

			addcatbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-category", IconSize.Button);
			addtimerbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-timer", IconSize.Button);
			addcardbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-card", IconSize.Button);
			addscorebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-score", IconSize.Button);
			addtagbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-tag", IconSize.Button);
			applyimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-apply", IconSize.Button);

			tagger = new DashboardCanvas (new WidgetWrapper (drawingarea));
			tagger.ButtonsSelectedEvent += HandleTaggersSelectedEvent;
			tagger.ShowMenuEvent += HandleShowMenuEvent;
			tagger.NewTagEvent += HandleNewTagEvent;
			tagger.EditButtonTagsEvent += EditEventSubcategories;
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
			Mode = DashboardMode.Code;
			// Initialize to a sane default value.
			propertiesnotebook.Page = 1;
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
				propertiesnotebook.Page = 1;
				tagproperties.Dashboard = value;
				popupbutton.Active = value.DisablePopupWindow;
			}
		}

		public DashboardMode Mode {
			set {
				UpdateMode (value);
			}
			get {
				return mode;
			}
		}

		public bool ButtonsVisible {
			set {
				internalButtons = value;
				Mode = mode;
			}
		}

		public void ClickButton (DashboardButton button, Tag tag = null)
		{
			tagger.Click (button, tag);
		}

		public void Refresh (DashboardButton b = null)
		{
			tagger.Refresh (b);
		}

		public void AddButton (string buttontype)
		{
			DashboardButton button = null;

			if (buttontype == "Card") {
				button = new PenaltyCardButton {
					PenaltyCard = new PenaltyCard ("Red", Color.Red, CardShape.Rectangle)
				};
			} else if (buttontype == "Score") {
				button = new ScoreButton {
					Score = new Score ("Score", 1)
				};
			} else if (buttontype == "Timer") {
				button = new TimerButton { Timer = new Timer { Name = "Timer" } };
			} else if (buttontype == "Tag") {
				button = new TagButton { Tag = new Tag ("Tag", "") };
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

		void UpdateMode (DashboardMode mode)
		{
			ignoreChanges = true;
			tagger.Mode = this.mode = mode;
			// Add buttons for cards/tags/etc.. can be handled remotely.
			hbuttonbox2.Visible = mode == DashboardMode.Edit && internalButtons;
			editbutton.Active = rightbox.Visible = linksbutton.Visible = mode == DashboardMode.Edit;

			if (mode == DashboardMode.Edit) {
				editimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-dash-edit_active", 22);
				tagger.ShowLinks = linksbutton.Active;
			} else {
				editimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-dash-edit", 22);
				tagger.ShowLinks = false;
			}

			LongoMatch.Gui.Helpers.Misc.SetFocus (this, mode == DashboardMode.Edit);
			if (project != null) {
				if (mode == DashboardMode.Edit) {
					Edited = false;
				} else {
					if (Edited)
						Config.EventsBroker.EmitDashboardEdited ();
				}

			}
			ignoreChanges = false;
		}

		void RemoveButton (DashboardButton button)
		{
			string msg = Catalog.GetString ("Do you want to delete: ") +
			             button.Name + "?";
			if (Config.GUIToolkit.QuestionMessage (msg, null, this)) {
				template.List.Remove (button);
				Edited = true;
				Refresh ();
			}
		}

		void RemoveLink (ActionLink link)
		{
			string msg = string.Format ("{0} {1} ?",
				             Catalog.GetString ("Do you want to delete: "), link);
			if (Config.GUIToolkit.QuestionMessage (msg, null, this)) {
				link.SourceButton.ActionLinks.Remove (link);
				Edited = true;
				Refresh ();
			}
		}

		void FillToolbar ()
		{
			Toolbar toolbar = new Toolbar ();
			toolbar.Orientation = Orientation.Vertical;
			toolbar.ToolbarStyle = ToolbarStyle.Icons;
			
			editimage = new Gtk.Image (Helpers.Misc.LoadIcon ("longomatch-dash-edit_active", 22));
			editbutton = new ToggleToolButton ();
			editbutton.IconWidget = editimage;
			editbutton.Toggled += HandleEditToggled;
			editbutton.TooltipText = Catalog.GetString ("Edit dashboard");
			toolbar.Add (editbutton);
			toolbar.Add (new SeparatorToolItem ());

			linksimage = new Gtk.Image (Helpers.Misc.LoadIcon ("longomatch-link-disabled", 22));
			linksbutton = new ToggleToolButton ();
			linksbutton.IconWidget = linksimage;
			linksbutton.Toggled += HandleLinksToggled;
			linksbutton.TooltipText = Catalog.GetString ("Edit actions links");
			toolbar.Add (linksbutton);
			toolbar.Add (new SeparatorToolItem ());

			popupbutton = new ToggleToolButton ();
			popupbutton.IconWidget = new Gtk.Image (Helpers.Misc.LoadIcon ("longomatch-popup", 22));
			popupbutton.Active = true;
			popupbutton.Toggled += HandlePopupToggled;
			popupbutton.TooltipText = Catalog.GetString ("Disable popup window");
			toolbar.Add (popupbutton);
			toolbar.Add (new SeparatorToolItem ());
			
			fitbutton = new RadioToolButton ((GLib.SList)null);
			fitbutton.IconWidget = new Gtk.Image (Helpers.Misc.LoadIcon ("longomatch-dash-fit", 22));
			fitbutton.Toggled += HandleFitModeToggled;
			fitbutton.TooltipText = Catalog.GetString ("Fit dashboard");
			toolbar.Add (fitbutton);
			fillbutton = new RadioToolButton (fitbutton);
			fillbutton.IconWidget = new Gtk.Image (Helpers.Misc.LoadIcon ("longomatch-dash-fill", 22));
			fillbutton.Toggled += HandleFitModeToggled;
			fillbutton.TooltipText = Catalog.GetString ("Fill dashboard");
			toolbar.Add (fillbutton);
			d11button = new RadioToolButton (fitbutton);
			d11button.IconWidget = new Gtk.Image (Helpers.Misc.LoadIcon ("longomatch-dash-11", 22));
			d11button.Toggled += HandleFitModeToggled;
			d11button.TooltipText = Catalog.GetString ("1:1 dashboard");
			toolbar.Add (d11button);
			toolbar.ShowAll ();
			hbox2.PackEnd (toolbar, false, false, 0);

			editbutton.Active = true;
			linksbutton.Active = false;
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

		void HandlePopupToggled (object sender, EventArgs e)
		{
			if (ignoreChanges) {
				return;
			}
			template.DisablePopupWindow = popupbutton.Active;
		}

		void HandleEditToggled (object sender, EventArgs e)
		{
			if (ignoreChanges) {
				return;
			}
			if (editbutton.Active) {
				UpdateMode (DashboardMode.Edit);
			} else {
				UpdateMode (DashboardMode.Code);
			}
		}

		void HandleLinksToggled (object sender, EventArgs e)
		{
			if (linksbutton.Active) {
				linksimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-link-active", 22);
			} else {
				linksimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-link-disabled", 22);
			}
			if (ignoreChanges) {
				return;
			}
			tagger.ShowLinks = linksbutton.Active;
		}

		void HandleTaggersSelectedEvent (List<DashboardButton> taggers)
		{
			if (taggers.Count == 1) {
				selected = taggers [0];
				tagproperties.Tagger = taggers [0];
				propertiesnotebook.Page = 0;
			} else {
				selected = null;
				tagproperties.Tagger = null;
				propertiesnotebook.Page = 1;
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

		void HandleShowMenuEvent (List<DashboardButton> buttons, List<ActionLink> links)
		{
			Menu menu;
			MenuItem delbut;
			
			if (Mode != DashboardMode.Edit) {
				return;
			}
			
			menu = new Menu ();
			foreach (DashboardButton button in buttons) {
				delbut = new MenuItem (string.Format ("{0}: {1}",
					Catalog.GetString ("Delete"), button.Name));
				delbut.Activated += (sender, e) => RemoveButton (button);
				menu.Add (delbut);
			}
			foreach (ActionLink link in links) {
				delbut = new MenuItem (string.Format ("{0}: {1}",
					Catalog.GetString ("Delete"), link));
				delbut.Activated += (sender, e) => RemoveLink (link);
				menu.Add (delbut);
			}
			menu.ShowAll ();
			menu.Popup ();
		}

		void HandleAddClicked (object sender, EventArgs e)
		{
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

		void HandleNewTagEvent (EventType evntType, List<Player> players, TeamType team, List<Tag> tags,
		                        Time start, Time stop, Time eventTime, Score score, PenaltyCard card, DashboardButton btn)
		{
			/* Forward event until we have players integrted in the dashboard layout */
			if (NewTagEvent != null) {
				NewTagEvent (evntType, players, team, tags, start, stop, eventTime, score, card, btn);
			}
			//Config.EventsBroker.EmitNewTag (button, players, tags, start, stop);
		}

		void EditEventSubcategories (DashboardButton dashboardButton)
		{
			AnalysisEventButton button = (dashboardButton as AnalysisEventButton); 
			AnalysisEventType evt = button.AnalysisEventType;
			EventTypeTagsEditor dialog = new EventTypeTagsEditor ();
			dialog.EventType = evt;
			dialog.Run ();
			dialog.Destroy ();
			template.RemoveDeadLinks (button);
			Edited = true;
			Refresh ();
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
