// PixbufTimeNode.cs
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
using Gdk;

namespace LongoMatch.Compat.v00.TimeNodes
{
	
	
	public class PixbufTimeNode : TimeNode
	{
		private string miniaturePath;
		
		
		public PixbufTimeNode(){
		}
	
		
		public PixbufTimeNode(string name, Time start, Time stop, string miniaturePath): base (name,start,stop)
		{
			this.miniaturePath = miniaturePath;

		}
		
		public Pixbuf Miniature{
			get{ 

				if (System.IO.File.Exists(this.MiniaturePath)){
					
					return new Pixbuf(this.MiniaturePath);
				}
				else return null;
			}
		}
		
		public String MiniaturePath{
	
			get{return this.miniaturePath;}
			set{this.miniaturePath = value;}
		}
	}
}
