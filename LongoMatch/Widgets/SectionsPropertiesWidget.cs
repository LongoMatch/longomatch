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
using Gdk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;


namespace LongoMatch.Widgets.Component
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
		
			int j=19;
			foreach (TimeNodeProperties tnd in table20.Children){
				tndArray[j] = ((TimeNodeProperties)tnd);
				j--;
			}			
			
			for(int i=0;i<20;i++){
				tndArray[i].Title = Catalog.GetString("Section") +(i+1);
			}
		}
		
		public void SetSections(Sections sections){
			
			for(int i=0;i<20;i++){
				tndArray[i].TimeNode=sections.GetTimeNode(i);
				tndArray[i].Color=sections.GetColor(i);
			}
			
		}
		
		public Sections GetSections (){
			Sections sections = new Sections(20);
			SectionsTimeNode[] timeNodesArray = new SectionsTimeNode[20];
			Color[] colorsArray = new Color[20];
			for(int i=0;i<20;i++){
				timeNodesArray[i]=tndArray[i].TimeNode;
				colorsArray[i] = tndArray[i].Color;
			}
			sections.Colors = colorsArray;
			sections.SectionsTimeNodes = timeNodesArray;
			return sections;
		}
	}
}
