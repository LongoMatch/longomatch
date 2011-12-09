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
using Gdk;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Gui;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ButtonsWidget : Gtk.Bin
	{

		private Categories categories;
		private TagMode tagMode;
		private Dictionary<Widget, Category> buttonsDic;

		public event NewTagHandler NewMarkEvent;
		public event NewTagStartHandler NewMarkStartEvent;
		public event NewTagStopHandler NewMarkStopEvent;


		public ButtonsWidget()
		{
			this.Build();
			Mode = TagMode.Predifined;
			buttonsDic = new Dictionary<Widget, Category>();
		}

		public TagMode Mode {
			set {
				bool isPredef = (value == TagMode.Predifined);
				table1.Visible = isPredef;
				starttagbutton.Visible = !isPredef;
				cancelbutton.Visible = false;
				tagMode = value;
			}
		}

		public Categories Categories {
			set {
				foreach(Widget w in table1.AllChildren) {
					table1.Remove(w);
					w.Destroy();
				}
				categories = value;
				if(value == null)
					return;

				buttonsDic.Clear();
				int sectionsCount = value.Count;

				table1.NColumns =(uint) 10;
				table1.NRows =(uint)(sectionsCount/10);

				for(int i=0; i<sectionsCount; i++) {
					Button b = new Button();
					Label l = new Label();
					Category cat = value[i];

					uint row_top =(uint)(i/table1.NColumns);
					uint row_bottom = (uint) row_top+1 ;
					uint col_left = (uint) i%table1.NColumns;
					uint col_right = (uint) col_left+1 ;

					l.Markup = cat.Name;
					l.Justify = Justification.Center;
					l.Ellipsize = Pango.EllipsizeMode.Middle;
					l.CanFocus = false;
					
					var c = new Color();
					Color.Parse("black", ref c);
					l.ModifyFg(StateType.Normal, c);
					l.ModifyFg(StateType.Prelight, Helpers.ToGdkColor(cat.Color));
                    l.Markup = cat.Name;

					b.Add(l);
					b.Name = i.ToString();
					b.Clicked += new EventHandler(OnButtonClicked);
					b.CanFocus = false;
					b.ModifyBg(StateType.Normal, Helpers.ToGdkColor(cat.Color));

					l.Show();
					b.Show();

					table1.Attach(b,col_left,col_right,row_top,row_bottom);

					buttonsDic.Add(b, cat);
				}
			}
		}

		protected virtual void OnButtonClicked(object sender,  System.EventArgs e)
		{
			if(categories == null)
				return;
			Widget w = (Button)sender;
			if(tagMode == TagMode.Predifined) {
				if(NewMarkEvent != null)
					NewMarkEvent(buttonsDic[w]);
			} else {
				starttagbutton.Visible = true;
				table1.Visible = false;
				cancelbutton.Visible = false;
				if(NewMarkStopEvent != null)
					NewMarkStopEvent(buttonsDic[w]);
			}
		}

		protected virtual void OnStartTagClicked(object sender, System.EventArgs e)
		{
			if(categories == null)
				return;

			starttagbutton.Visible = false;
			table1.Visible = true;
			cancelbutton.Visible = true;

			if(NewMarkStartEvent != null)
				NewMarkStartEvent();
		}

		protected virtual void OnCancelbuttonClicked(object sender, System.EventArgs e)
		{
			starttagbutton.Visible = true;
			table1.Visible = false;
			cancelbutton.Visible = false;
		}
	}
}
