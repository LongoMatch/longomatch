// PlayListTimeNode.cs 
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

namespace LongoMatch.TimeNodes
{
	
	[Serializable]
	public class PlayListTimeNode : TimeNode
	{
		private string fileName;
		private float rate=1;
		private bool valid=true;//True if the file is present in the system

		#region Constructors
		public PlayListTimeNode(){
		}
		
		public PlayListTimeNode(string fileName, MediaTimeNode tNode) : base(tNode.Name,tNode.Start,tNode.Stop)
		{
			this.fileName = fileName;
			
		}
		#endregion
		#region  Properties
		
		public string FileName{
			set{ this.fileName = value;}
			get{ return this.fileName;}
		}
		
		public float Rate{
			set{ this.rate = value;}
			get{ return this.rate;}
		}
		
		//FIXME Tiene que devolver la comprobación de si el fichero existe, así no hay que setearlo externamente
		public bool Valid{
			get{return this.valid;}
			set{this.valid = value;}
		}
		#endregion
	
		
		
	
		
	}
}
