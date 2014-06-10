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
using System.Linq;
using System.Collections.Generic;

using LongoMatch.Common;
using LongoMatch.Interfaces;

namespace LongoMatch.Store
{
[Serializable]
	
	
	public class Tag
	{
		public Tag() {
		}
		
		public SubCategory SubCategory {
			set;
			get;
		}
		
		public string Value {
			get;
			set;
		}
	}
	
	[Serializable]
	public class Tag<T>: ITag<T>
	{
		public Tag() {
		}
		
		public ISubCategory SubCategory {
			set;
			get;
		}
		
		public T Value {
			get;
			set;
		}
		
	}

	[Serializable]
	public class StringTag: Tag<string>
	{
		public StringTag() {}
		
		public override bool Equals (object obj)
		{
			StringTag tag = obj as StringTag;
            if (tag == null)
				return false;
			return Value.Equals (tag.Value) && SubCategory.Equals(tag.SubCategory);
		}
		
		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}

	[Serializable]
	public class PlayerTag: Tag<Player>
	{
		public PlayerTag() {}
		
		public override bool Equals (object obj)
		{
			PlayerTag tag = obj as PlayerTag;
            if (tag == null)
				return false;
			return Value.Equals (tag.Value) && SubCategory.Equals(tag.SubCategory) ;
		}
		
		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}

	[Serializable]
	public class TeamTag: Tag<Team>
	{
		public TeamTag() {}
		
		public override bool Equals (object obj)
		{
			TeamTag tag = obj as TeamTag;
            if (tag == null)
				return false;
			return Value.Equals (tag.Value) && SubCategory.Equals(tag.SubCategory);
		}
		
		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}
	
	[Serializable]
	public class CoordinatesTag: Tag<Coordinates>
	{
		public CoordinatesTag() {}
		
		public override bool Equals (object obj)
		{
			CoordinatesTag tag = obj as CoordinatesTag;
            if (tag == null)
				return false;
			return Value.Equals (tag.Value) && SubCategory.Equals(tag.SubCategory);
		}
		
		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}
}
