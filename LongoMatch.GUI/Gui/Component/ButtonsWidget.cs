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
	public partial class ButtonsWidget : Gtk.Bin
	{
		public event NewTagHandler NewTagEvent;

		TagMode tagMode;
		PlaysTagger tagger;
		Categories template;
		TaggerButton selected;
		bool edited;

		public ButtonsWidget()
		{
			this.Build();
			tagger = new PlaysTagger (new WidgetWrapper (drawingarea1));
			tagger.FitMode = FitMode.Original;
			tagger.TaggersSelectedEvent += HandleTaggersSelectedEvent;
			tagger.ShowMenuEvent += HandleShowMenuEvent;
			tagger.NewTagEvent += HandleNewTagEvent;
			tagger.AddNewTagEvent += HandleAddNewTagEvent;
			drawingarea1.CanFocus = true;
			drawingarea1.KeyPressEvent += HandleKeyPressEvent;
			Mode = TagMode.Predefined;
			fieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			hfieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			goaleventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			categoryproperties1.EditedEvent += (sender, e) => {drawingarea1.QueueDraw();};
			addcatbutton.Clicked += HandleAddClicked;
			addtimerbutton.Clicked += HandleAddClicked;
			addscorebutton.Clicked += HandleAddClicked;
			addtagbutton.Clicked += HandleAddClicked;
			addcardbutton.Clicked += HandleAddClicked;
			Config.EventsBroker.Tick += HandleTick;
			Edited = false;
		}

		public override void Destroy ()
		{
			Config.EventsBroker.Tick -= HandleTick;
			tagger.Dispose ();
			base.Destroy ();
		}
		
		public bool Edited {
			get {
				return edited || tagger.Edited || categoryproperties1.Edited;
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
				hbuttonbox2.Visible = rightbox.Visible = tagMode == TagMode.Edit;
			}
		}
		
		public void Refresh (TaggerButton b = null) {
			tagger.Refresh (b);
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
				categoryproperties1.Tagger = taggers[0];
				propsframe.Sensitive = true;
			} else {
				selected = null;
				propsframe.Sensitive = false;
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
			drawingarea1.QueueDraw ();
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
			TaggerButton tagger = null;
			
			if (sender == addcardbutton) {
				tagger = new PenaltyCard ("Red", Color.Red, CardShape.Rectangle);
			} else if (sender == addscorebutton) {
				tagger = new Score ("Score", 1);
			} else if (sender == addtimerbutton) {
				tagger = new Timer {Name = "Timer"};
			} else if (sender == addtagbutton) {
				tagger = new TagButton {Name = "Tag"};
			} else if (sender == addcatbutton) {
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
		
		void HandleNewTagEvent (TaggerButton button, List<Player> players,
		                        List<Tag> tags, Time start, Time stop)
		{
			if (button is TagButton || button is Timer) {
				return;
			}
			
			Config.EventsBroker.EmitNewTag (button, players, tags, start, stop);
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
