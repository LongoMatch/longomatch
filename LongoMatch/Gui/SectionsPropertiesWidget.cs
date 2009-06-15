// SectionsPropertiesWidget.cs
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
using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using Gdk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;


namespace LongoMatch.Gui.Component
{
	
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SectionsPropertiesWidget : Gtk.Bin
	{
		List<TimeNodeProperties> tndlist;
		
		public SectionsPropertiesWidget()
		{
			this.Build();
			tndlist = new List<TimeNodeProperties>();
			
		}
		
		public void SetSections(Sections sections){
			int sectionsCount = sections.Count;
			table1.NColumns =(uint) 10;
			table1.NRows =(uint) (sectionsCount/10);
			
			tndlist.Clear();
			
			for( int i=0;i<sectionsCount;i++){
				TimeNodeProperties tnp = new TimeNodeProperties();
				tnp.Title =  sections.GetName(i);
					
				uint row_top =(uint) (i/table1.NColumns);
				uint row_bottom = (uint) row_top+1 ;
				uint col_left = (uint) i%table1.NColumns;
				uint col_right = (uint) col_left+1 ;
				
				tnp.Section = sections.GetSection(i);
				tnp.Show();
				tndlist.Add(tnp);					
				table1.Attach(tnp,col_left,col_right,row_top,row_bottom);					
			}
			
		}
		
		public Sections GetSections (){
			Sections sections = new Sections();
			foreach (TimeNodeProperties tnp in tndlist){
				sections.AddSection(tnp.Section);					
			}
			return sections;
		}
	}
}
