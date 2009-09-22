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

namespace LongoMatch.TimeNodes
{
	
	
	public class PixbufTimeNode : TimeNode
	{
		private byte[] thumbnailBuf;
		
		#region Contructors
		public PixbufTimeNode(){
		}	
		
		public PixbufTimeNode(string name, Time start, Time stop, Pixbuf thumbnail): base (name,start,stop)
		{
			if (thumbnail != null){
				this.thumbnailBuf = thumbnail.SaveToBuffer("png");
				thumbnail.Dispose();
			}
			else thumbnailBuf = null;
		}
		#endregion
		
		#region Properties
		
		public Pixbuf Miniature{
			get{ 
				if (thumbnailBuf != null)
					return new Pixbuf(thumbnailBuf);
				else return null;
			}
		}		
		
		
		#endregion
	}
}
