// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Linq;
using LongoMatch.Interfaces;

namespace LongoMatch.Store
{
	[Serializable]
	public class TagsStore<T, W> where T:ISubCategory
	{
		private Dictionary<T, List<W>> tags;
		
		public TagsStore(){
			tags = new Dictionary<T, List<W>>();
		}
		
		public void Add(T subCategory, W tag) {
			Log.Debug(String.Format("Adding tag {0} to subcategory{1}", subCategory, tag));
			if (!tags.ContainsKey(subCategory))
				tags.Add(subCategory, new List<W>());
			tags[subCategory].Add(tag);
		}
		
		public void Remove(T subCategory, W tag) {
			if (!tags.ContainsKey(subCategory)) {
				Log.Warning(String.Format("Trying to remove tag {0} from unknown subcategory{1}",
				                          subCategory, tag));
				return;
			}
			tags[subCategory].Remove(tag);
			if (tags[subCategory].Count == 0)
				tags.Remove(subCategory);
		}
		
		public bool Contains(T subCategory) {
			return (tags.ContainsKey(subCategory));
		}
		
		public bool Contains(T subCategory, W tag) {
			return (Contains(subCategory) && tags[subCategory].Contains(tag));
		}
		
		public List<W> AllUniqueElements {
			get {
				return (from list in tags.Values
				        from player in list
				        group player by player into g
				        select g.Key).ToList();
			}
		}
		
		public List<W> GetTags(T subCategory) {
			if (!tags.ContainsKey(subCategory)) {
				Log.Debug(String.Format("Adding subcategory {0} to store", subCategory.Name));
				tags[subCategory] = new List<W>();
			}
			return tags[subCategory];			
		}

	}
	
	
	public class StringTagStore: TagsStore<TagSubCategory, StringTag> {}
	
	public class PlayersTagStore: TagsStore<PlayerSubCategory, PlayerTag> {}
	
	public class TeamsTagStore: TagsStore<PlayerSubCategory, TeamTag> {}
}

