// ButtonsWidget.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using Gtk;
using LongoMatch.DB;
using LongoMatch.Handlers;
using System.Collections.Generic;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ButtonsWidget : Gtk.Bin
	{
				
		private Sections sections;
		private const int MS = 1000;
		private List<Button> bList;
	
		public event NewMarkEventHandler NewMarkEvent;

		
		public ButtonsWidget()
		{		
			this.Build();
			bList = new List<Button>();			
		}
		
		public Sections Sections{
			set{
				this.sections = value;
				int sectionsCount = value.Count;
				
				foreach (Widget w in table1.AllChildren){
					w.Unrealize();
					table1.Remove(w);
				}
				
				table1.NColumns =(uint) 10;
				table1.NRows =(uint) (sectionsCount/10);
			
				for( int i=0;i<sectionsCount;i++){
					Button b = new Button();
					Label l = new Label();
					uint row_top =(uint) (i/table1.NColumns);
					uint row_bottom = (uint) row_top+1 ;
					uint col_left = (uint) i%table1.NColumns;
					uint col_right = (uint) col_left+1 ;
					
					l.Text = sections.GetName(i);
					b.Add(l);
					b.Name = i.ToString();
					b.Clicked += new EventHandler (OnButtonClicked);
					l.Show();
					b.Show();
					
					
					table1.Attach(b,col_left,col_right,row_top,row_bottom);					
				}
			}			
		}
		

		protected virtual void OnButtonClicked(object sender,  System.EventArgs e)
		{
			Widget w = (Button)sender;
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(int.Parse(w.Name));			
		}		
	}
}
