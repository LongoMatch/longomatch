// 
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using Newtonsoft.Json;

namespace LongoMatch.Core.Common
{
	[Serializable]
	[JsonObject]
	public class Coordinates: List<Point>
	{
		public Coordinates ()
		{
		}
		
		public List<Point> Points {
			get {
				return this.ToList();
			}
		}
		
		public override bool Equals (object obj)
		{
			Coordinates c = obj as Coordinates;
            if (c == null)
				return false;
				
			if (c.Count != Count)
				return false;
			
			for (int i=0; i<Count; i++) {
				if (c[i] != this[i])
					return false;
			}
			return true;
		}
		
		public override int GetHashCode ()
		{
			string s = "";
			
			for (int i=0; i<Count; i++) {
				s += this[i].X.ToString() +  this[i].Y.ToString();
			}
			
			return int.Parse(s);
		}
	}
	
}

