// 
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
// 

using System;
using Gdk;

namespace LongoMatch.TimeNodes
{
	/* Represent a player in the database defined by its name, position in the field, number and photo
	 */
	[Serializable]
	public class Player
	{
		private string name;
		private string position;
		private int number;
		private byte[] photo;

		#region Constructors
		public Player(string name, string position, int number, Pixbuf photo)
		{
			this.name = name;
			this.position = position;
			this.number = number;
			Photo = photo;
		}
		#endregion

		#region Properties
		public string Name{
			get{return name;}
			set{name=value;}
		}
		
		public string Position{
			get{return position;}
			set{position=value;}
		}
		
		public int Number{
			get{return number;}
			set{number=value;}
		}
		
		public Pixbuf Photo{
			get{
				if(photo != null)
					return new Pixbuf(photo);
				else
					return null;
			}
			set{
				if(value != null)
					photo=value.SaveToBuffer("png");
				else 
					photo=null;
			}
		}
		#endregion
	}
}
