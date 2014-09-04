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

using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Core.Store
{

	[Serializable]
	public class Tag
	{
		public Tag (string value, string grp="") {
			Group = grp;
			Value = value;
		}
		
		public string Group {
			set;
			get;
		}
		
		public string Value {
			get;
			set;
		}
		
		public override bool Equals (object obj)
		{
			Tag tag = obj as Tag;
            if (tag == null)
				return false;
			return Value == tag.Value && Group == tag.Group;
		}
		
		public override int GetHashCode ()
		{
			if (Value != null && Group != null) {
				return (Value + Group).GetHashCode ();
			} else {
				return base.GetHashCode ();
			}
		}
	}
}
