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
using Gtk;
using LongoMatch.DB;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;
using System.Collections.Generic;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ButtonsWidget : Gtk.Bin
	{

		private Sections sections;		
		private TagMode tagMode;

		public event NewMarkEventHandler NewMarkEvent;
		public event NewMarkStartHandler NewMarkStartEvent;
		public event NewMarkStopHandler NewMarkStopEvent;


		public ButtonsWidget()
		{
			this.Build();
			Mode = TagMode.Predifined;
		}
		
		public TagMode Mode{
			set{
				bool isPredef = (value == TagMode.Predifined);
				table1.Visible = isPredef;
				starttagbutton.Visible = !isPredef;
				cancelbutton.Visible = false;
				tagMode = value;				
			}
		}

		public Sections Sections {
			set {
				foreach (Widget w in table1.AllChildren) {
					table1.Remove(w);
					w.Destroy();
				}
				sections = value;
				if (value == null)
					return;

				int sectionsCount = value.Count;

				table1.NColumns =(uint) 10;
				table1.NRows =(uint)(sectionsCount/10);

				for (int i=0;i<sectionsCount;i++) {
					Button b = new Button();
					Label l = new Label();
					uint row_top =(uint)(i/table1.NColumns);
					uint row_bottom = (uint) row_top+1 ;
					uint col_left = (uint) i%table1.NColumns;
					uint col_right = (uint) col_left+1 ;

					l.Markup = sections.GetName(i);
					l.Justify = Justification.Center;
					l.Ellipsize = Pango.EllipsizeMode.Middle;

					b.Add(l);
					b.Name = i.ToString();
					b.Clicked += new EventHandler(OnButtonClicked);
					l.Show();
					b.Show();

					table1.Attach(b,col_left,col_right,row_top,row_bottom);
				}
			}
		}

		protected virtual void OnButtonClicked(object sender,  System.EventArgs e)
		{
			if (sections == null)
				return;
			Widget w = (Button)sender;
			if (tagMode == TagMode.Predifined){
				if (NewMarkEvent != null)
					NewMarkEvent(int.Parse(w.Name));
			} else {
				starttagbutton.Visible = true;
				table1.Visible = false;
				cancelbutton.Visible = false;
				if (NewMarkStopEvent != null)
					NewMarkStopEvent(int.Parse(w.Name));
			}			
		}

		protected virtual void OnStartTagClicked (object sender, System.EventArgs e)
		{
			if (sections == null)
				return;
			
			starttagbutton.Visible = false;
			table1.Visible = true;
			cancelbutton.Visible = true;
			
			if (NewMarkStartEvent != null)
				NewMarkStartEvent();
		}

		protected virtual void OnCancelbuttonClicked (object sender, System.EventArgs e)
		{
			starttagbutton.Visible = true;
			table1.Visible = false;
			cancelbutton.Visible = false;
		}
	}
}
