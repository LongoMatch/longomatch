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
using Gtk;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Gui;
using Helpers = LongoMatch.Gui.Helpers;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;
using Mono.Unix;
using Image = LongoMatch.Common.Image;
using LongoMatch.Gui.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DashboardWidget : Gtk.Bin
	{
		public event NewTagHandler NewTagEvent;

		TagMode tagMode;
		Dashboard tagger;
		Categories template;
		TaggerButton selected;
		Gtk.Image editimage;
		ToggleToolButton editbutton;
		RadioToolButton d11button, fillbutton, fitbutton;
		bool internalButtons, edited, ignoreChanges;

		public DashboardWidget()
		{
			this.Build();
			tagger = new Dashboard (new WidgetWrapper (drawingarea));
			tagger.TaggersSelectedEvent += HandleTaggersSelectedEvent;
			tagger.ShowMenuEvent += HandleShowMenuEvent;
			tagger.NewTagEvent += HandleNewTagEvent;
			tagger.AddNewTagEvent += HandleAddNewTagEvent;
			drawingarea.CanFocus = true;
			drawingarea.KeyPressEvent += HandleKeyPressEvent;
			fieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			hfieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			goaleventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			resetfieldbutton.Clicked += HandleResetField;
			resethfieldbutton.Clicked += HandleResetField;
			resetgoalbutton.Clicked += HandleResetField;
			tagproperties.EditedEvent += (sender, e) => {drawingarea.QueueDraw();};
			addcatbutton.Clicked += HandleAddClicked;
			addtimerbutton.Clicked += HandleAddClicked;
			addscorebutton.Clicked += HandleAddClicked;
			addtagbutton.Clicked += HandleAddClicked;
			addcardbutton.Clicked += HandleAddClicked;

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
				tagger.Refresh ();
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

		public Categories Template {
			set {
				template = value;
				tagger.Template = value;
				fieldimage.Pixbuf = value.FieldBackground.Scale (50, 50).Value;
				hfieldimage.Pixbuf = value.HalfFieldBackground.Scale (50, 50).Value;
				goalimage.Pixbuf = value.GoalBackground.Scale (50, 50).Value;
				Edited = false;
				// Start with disabled widget until something get selected
				tagproperties.Tagger = null;
				tagproperties.Sensitive = false;
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
		
		public void Refresh (TaggerButton b = null) {
			tagger.Refresh (b);
		}

		public void AddButton (string buttontype) {
			TaggerButton tagger = null;

			if (buttontype == "Card") {
				tagger = new PenaltyCard ("Red", Color.Red, CardShape.Rectangle);
			} else if (buttontype == "Score") {
				tagger = new Score ("Score", 1);
			} else if (buttontype == "Timer") {
				tagger = new Timer {Name = "Timer"};
			} else if (buttontype == "Tag") {
				tagger = new TagButton {Name = "Tag"};
			} else if (buttontype == "Category") {
				tagger = template.AddDefaultItem (template.List.Count);
			} else {
				return;
			}

			if (!(tagger is Category)) {
				template.List.Add (tagger);
			}
			tagger.Position = new Point (template.CanvasWidth, 0);
			Refresh (tagger);
		}
		
		void RemoveButton (TaggerButton button) {
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

		void HandleTaggersSelectedEvent (List<TaggerButton> taggers)
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
			LongoMatch.Common.Image background;
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
			drawingarea.QueueDraw ();
		}
		
		void HandleKeyPressEvent (object o, KeyPressEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.Delete && selected != null) {
				RemoveButton (selected);
			}
		}
		
		void HandleShowMenuEvent (TaggerButton taggerbutton, Tag tag)
		{
			Menu menu;
			MenuItem delbut, deltag;
			
			menu = new Menu ();
			delbut = new MenuItem (Catalog.GetString ("Delete"));
			delbut.Activated += (sender, e) => {RemoveButton (taggerbutton);};
			menu.Add (delbut);
			
			if (tag != null) {
				deltag = new MenuItem (String.Format ("{0} \"{1}\"",
				                                      Catalog.GetString ("Delete tag:"),
				                                      tag.Value));
				deltag.Activated += (sender, e) => {
					(taggerbutton as Category).Tags.Remove (tag);
					Edited = true;
					tagger.Refresh (taggerbutton);
				};
				menu.Add (deltag);
			}

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
		
		void HandleNewTagEvent (TaggerButton button, List<Player> players,
		                      List<Tag> tags, Time start, Time stop)
		{
			if (button is TagButton || button is Timer) {
				return;
			}
			
			/* Forward event until we have players integrted in the dashboard layout */
			if (NewTagEvent != null) {
				NewTagEvent (button , players, tags, start, stop);
			}
			//Config.EventsBroker.EmitNewTag (button, players, tags, start, stop);
		}

		void HandleAddNewTagEvent (TaggerButton taggerbutton)
		{
			string res = MessagesHelpers.QueryMessage (this, Catalog.GetString ("Name"),
			                                           Catalog.GetString ("New tag"));
			if (res != null && res != "") {
				(taggerbutton as Category).Tags.Add (new Tag (res));
				tagger.Refresh (null);
			}
		}

		void HandleClicked (object sender, EventArgs e)
		{
			if (Mode == TagMode.Edit) {
				Mode = TagMode.Predefined;
			} else {
				Mode = TagMode.Edit;
			}
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
	}
}
