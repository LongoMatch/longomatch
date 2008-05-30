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
using Gtk;
using Mono.Unix;

namespace LongoMatch
{
	
	
	public partial class SectionsPropertiesWidget : Gtk.Bin
	{
		TimeNodeProperties[] tndArray;
		
		public SectionsPropertiesWidget()
		{
			this.Build();
			
			// Agrupamos todos los TimeNodeProperties en un array para 
			// tratarlos mas facilmente
			tndArray = new TimeNodeProperties[20];
			Table[] _tableArray = new Table[5];
			_tableArray[0] = this.table1;
			_tableArray[1] = this.table6;
			_tableArray[2] = this.table11;			
			_tableArray[3] = this.table16;
			_tableArray[4] = this.table21;
			
			for(int j=0; j<5; j++){
				Gtk.Widget[] children = new Gtk.Widget[5];
				children = _tableArray[j].Children;
				int i=3;
				foreach (TimeNodeProperties tnd in children){
					tndArray[i+j*4] = (TimeNodeProperties)tnd;
					i--;
				}
			}
			
			for(int i=0;i<20;i++){
				tndArray[i].SetTitle(Catalog.GetString("Section") +(i+1));
			}
		}
		
		public void SetSections(Sections sections){
			
			for(int i=0;i<20;i++){
				tndArray[i].SetTimeNode(sections.GetTimeNode(i));
			}
			
		}
		
		public Sections GetSections (){
			Sections sections = new Sections(20);
			TimeNode[] timeNodesArray = new TimeNode[20];
			for(int i=0;i<20;i++){
				timeNodesArray[i]=tndArray[i].GetTimeNode();
			}
			sections.SetTimeNodes(timeNodesArray);
			return sections;
		}
	}
}
