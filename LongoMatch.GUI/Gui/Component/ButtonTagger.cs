//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using Gtk;
using Gdk;

using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;
using Color = Gdk.Color;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ButtonTagger : Gtk.Bin
	{
		public event NewTagHandler NewTag;
		public event NewTagStartHandler NewTagStart;
		public event NewTagStopHandler NewTagStop;
		public event NewTagCancelHandler NewTagCancel;
		
		Category category;
		Button tagButton;
		Label label;
		Time start, current;
		TagMode mode;
		Color black, grey;
		
		public ButtonTagger (Category category)
		{
			this.Build ();
			black = new Color();
			Color.Parse ("black", ref black);
			Color.Parse ("grey", ref grey);
			this.category = category;
			CreateButton ();
			cancelbutton.Clicked += OnButtonClicked;
			CurrentTime = new Time {MSeconds = 0};
			mode = TagMode.Predifined;
		}

		public TagMode  Mode {
			set {
				mode = value;
				if (mode == TagMode.Predifined) {
					cancelbutton.Visible = false;
				} else {
					if (start == null) {
						cancelbutton.Visible = start != null;
					}
				}
			}
		}
		
		public Time CurrentTime {
			set {
				current = value;
				if (mode == TagMode.Free && start != null) {
					Time ellapsed = value - start;
					label.Markup = String.Format ("{0} {1}", 
					                              GLib.Markup.EscapeText (category.Name),
					                              ellapsed.ToSecondsString());
				} else {
					label.Markup =  GLib.Markup.EscapeText (category.Name);
				}
			}
		}

		void ChangeButton (bool started) {
			if (started) {
				label.ModifyFg(StateType.Normal, Helpers.Misc.ToGdkColor(category.Color));
				label.ModifyFg(StateType.Prelight, black);
				tagButton.ModifyBg(StateType.Normal, grey);
				tagButton.ModifyBg(StateType.Prelight, Helpers.Misc.ToGdkColor(category.Color));
			} else {
				label.ModifyFg(StateType.Normal, black);
				label.ModifyFg(StateType.Prelight, Helpers.Misc.ToGdkColor(category.Color));
				tagButton.ModifyBg(StateType.Normal, Helpers.Misc.ToGdkColor(category.Color));
				tagButton.ModifyBg(StateType.Prelight, grey);
			}
		}
				
		void EmitStartTag () {
			if (NewTagStart != null)
				NewTagStart (category);
			cancelbutton.Visible = true;
			ChangeButton (true);
		}

		void EmitStopTag () {
			if (NewTagStop != null)
				NewTagStop (category);
			cancelbutton.Visible = false;
			ChangeButton (false);
		}
		
		void EmitCancelTag () {
			cancelbutton.Visible = false;
			start = null;
			ChangeButton (false);
			if (NewTagCancel != null)
				NewTagCancel (category);
		}
		
		void EmitNewTag () {
			if (NewTag != null)
				NewTag (category, null);
		}
		
		void OnButtonClicked (object sender, EventArgs args)
		{
			if (sender == tagButton) {
				if (mode == TagMode.Predifined) {
					EmitNewTag ();
				} else {
					if (start == null) {
						start = current;
						EmitStartTag ();
					} else {
						EmitStopTag ();
						start = null;
					}
				}
			} else {
				EmitCancelTag ();
			}
		}
		
		void CreateButton () {
			label = new Label();
			label.Markup = GLib.Markup.EscapeText (category.Name);
			label.Justify = Justification.Center;
			label.Ellipsize = Pango.EllipsizeMode.Middle;
			label.CanFocus = false;
            
			tagButton = new Button();
			tagButton.Add(label);
			tagButton.Clicked += new EventHandler(OnButtonClicked);
			tagButton.CanFocus = false;
			
			label.Show();
			tagButton.Show();
			buttonbox.PackStart (tagButton);
			ChangeButton (false);
		}
	}
}

