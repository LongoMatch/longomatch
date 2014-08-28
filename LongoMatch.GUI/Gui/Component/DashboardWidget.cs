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
		bool internalButtons;
		bool edited;

		public DashboardWidget()
		{
			this.Build();
			tagger = new Dashboard (new WidgetWrapper (drawingarea));
			tagger.FitMode = FitMode.Original;
			tagger.TaggersSelectedEvent += HandleTaggersSelectedEvent;
			tagger.ShowMenuEvent += HandleShowMenuEvent;
			tagger.NewTagEvent += HandleNewTagEvent;
			tagger.AddNewTagEvent += HandleAddNewTagEvent;
			drawingarea.CanFocus = true;
			drawingarea.KeyPressEvent += HandleKeyPressEvent;
			Mode = TagMode.Predefined;
			fieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			hfieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			goaleventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			tagproperties.EditedEvent += (sender, e) => {drawingarea.QueueDraw();};
			addcatbutton.Clicked += HandleAddClicked;
			addtimerbutton.Clicked += HandleAddClicked;
			addscorebutton.Clicked += HandleAddClicked;
			addtagbutton.Clicked += HandleAddClicked;
			addcardbutton.Clicked += HandleAddClicked;
			Edited = false;
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
				tagger.FitMode = value;
			}
		}

		public bool Edited {
			get {
				return edited || tagger.Edited || tagproperties.Edited;
			}
			set {
				edited = value;
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
			}
		}

		public TagMode Mode {
			set {
				tagMode = value;
				tagger.TagMode = value;
				// Properties only visible in edit mode
				rightbox.Visible = tagMode == TagMode.Edit;
				// Add buttons for cards/tags/etc.. can be handled remotely.
				hbuttonbox2.Visible = tagMode == TagMode.Edit && internalButtons;
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
		
		void HandleTaggersSelectedEvent (List<TaggerButton> taggers)
		{
			if (taggers.Count == 1) {
				selected = taggers[0];
				tagproperties.Tagger = taggers[0];
				tagproperties.Sensitive = true;
			} else {
				selected = null;
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
				template.FieldBackground = background;
				fieldimage.Pixbuf = background.Scale (50, 50).Value;
			} else if (o == hfieldeventbox) {
				template.HalfFieldBackground = background;
				hfieldimage.Pixbuf = background.Scale (50, 50).Value;
			} else if (o == goaleventbox) {
				template.GoalBackground = background;
				goalimage.Pixbuf = background.Scale (50, 50).Value;
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
	}
}
