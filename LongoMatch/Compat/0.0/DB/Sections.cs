// Sections.cs
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
using Gdk;
using LongoMatch.Compat.v00.TimeNodes;

namespace LongoMatch.Compat.v00.DB
{
	
	
	public class Sections
	{
		private SectionsTimeNode[] timeNodesArray;
		private Color[] colorsArray;
		private int visibleSections;
		private int totalSections;
		
		
		public Sections(int sections)
		{
			this.timeNodesArray = new SectionsTimeNode[sections];
			this.colorsArray = new Color[sections];
			this.totalSections = sections;
			this.visibleSections = sections;
			for (int i=0;i<20;i++){
				colorsArray[i] = new Color(254,0,0);
				timeNodesArray[i] = null;
			}			
		}
			
		public Color[] Colors{			
			set{this.colorsArray = value;}
			get{return this.colorsArray;}			
		}		
	
		
		public SectionsTimeNode[] SectionsTimeNodes{
			set{this.timeNodesArray = value;}
			get{return timeNodesArray;}
		}		
	
		public Color GetColor (int section){
			return this.colorsArray[section];
		}
	}
}
