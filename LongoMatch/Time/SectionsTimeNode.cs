// SectionsTimeNode.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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

namespace LongoMatch.TimeNodes
{
	
	
	public class SectionsTimeNode:TimeNode
	{
		HotKey hotkey; 
		Gdk.Color color;
		
		#region Constructors
		public SectionsTimeNode(String name,Time start, Time stop, HotKey hotkey, Color color):base (name,start,stop)
		{
			this.hotkey = hotkey;
			this.color = color;
		}
		#endregion
		#region  Properties 
		
				
		public HotKey HotKey{
			get{return this.hotkey;}
			set{this.hotkey = value;}
		}
		
		public Color Color{
			get{return this.color;}
			set{this.color=value;}
		}
		#endregion
	}
}
