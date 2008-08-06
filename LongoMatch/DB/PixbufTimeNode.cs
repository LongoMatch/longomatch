// PixbufTimeNode.cs
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

namespace LongoMatch
{
	
	
	public class PixbufTimeNode : TimeNode
	{
		private string miniaturePath;
		
		
		public PixbufTimeNode(){
		}
	
		
		public PixbufTimeNode(string name, Time start, Time stop, uint fps,string miniaturePath): base (name,start,stop,fps)
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
		}
	}
}
