// 
//  Copyright (C) 2011 andoni
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
using System.Collections.Generic;

using LongoMatch.Store;

namespace LongoMatch.Store
{
	[Serializable]
	public class GameUnitsList: List<GameUnit>
	{
		public GameUnitsList ()
		{
		}
		
		public int GameUnitDepth(GameUnit gameUnit) {
			return this.IndexOf(gameUnit);
		}
		
		public GameUnit GetParent(GameUnit gameUnit) {
			int index;
			
			if (!this.Contains(gameUnit))
				return null;
			
			index = this.IndexOf(gameUnit);
			if (index == 0)
				return null;
			return this[index-1];
		}

		public GameUnit GetChild(GameUnit gameUnit) {
			int index;
			
			if (!this.Contains(gameUnit))
				return null;
			
			index = this.IndexOf(gameUnit);
			if (index == this.Count - 1)
				return null;
			return this[index+1];
		}
		
		public GameUnit GetLast() {
			return this[this.Count-1];
		}
	}
}

