// 
//  Copyright (C) 2009 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;

namespace LongoMatch.DB
{
	
	
	public class Tag
	{
		string text;
		public Tag(string text)
		{
			this.text=text;
		}
		
		public string Text{
			get{return text;}
			set{text=value;}
		}
		
		public bool Equals(Tag tagComp){
			return (this.text == tagComp.Text);
		}
		
		public override bool Equals (object obj)
		{
			Tag tag= obj as Tag;
   		 	if (tag != null)
        		return Equals(tag);
    		else
        		return false;
		}
		
		public override int GetHashCode ()
		{
			return text.CompareTo("XXXXX") ^ 3 ;
		}
	}
}
