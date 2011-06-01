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
	public class TagsStore<T, W> where T:ISubCategory
	{
		public TagsStore(){
			Tags = new Dictionary<T, List<W>>();
		}
		
		private Dictionary<T, List<W>> Tags {
			get;
			set;
		}
		
		public void Add(T subCategory, W tag) {
			Log.Debug(String.Format("Adding tag {0} to subcategory{1}", subCategory, tag));
			if (!Tags.ContainsKey(subCategory))
				Tags.Add(subCategory, new List<W>());
			Tags[subCategory].Add(tag);
		}
		
		public void Remove(T subCategory, W tag) {
			if (!Tags.ContainsKey(subCategory)) {
				Log.Warning(String.Format("Trying to remove tag {0} from unknown subcategory{1}",
				                          subCategory, tag));
				return;
			}
			Tags[subCategory].Remove(tag);
			if (Tags[subCategory].Count == 0)
				Tags.Remove(subCategory);
		}
		
		public bool Contains(T subCategory) {
			return (Tags.ContainsKey(subCategory));
		}
		
		public bool Contains(T subCategory, W tag) {
			return (Contains(subCategory) && Tags[subCategory].Contains(tag));
		}
		
		public List<W> AllUniqueElements {
			get {
				return (from list in Tags.Values
				        from player in list
				        group player by player into g
				        select g.Key).ToList();
			}
		}
		
		public List<W> GetTags(T subCategory) {
			if (!Tags.ContainsKey(subCategory)) {
				Log.Warning("Trying to get the tags of an unknow subcategory");
				return new List<W>();
			}
			return Tags[subCategory];			
		}

	}
	
	
	public class StringTagStore: TagsStore<TagSubCategory, StringTag> {}
	
	public class PlayersTagStore: TagsStore<PlayerSubCategory, Player> {}
	
	public class TeamsTagStore: TagsStore<PlayerSubCategory, Team> {}

	
	[Serializable]
	public class Tag<T>
	{
		public Tag() {
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
	}

	[Serializable]
	public class PlayerTag: Tag<Player>
	{
		public PlayerTag() {}
	}

	[Serializable]
	public class TeamTag: Tag<Team>
	{
		public TeamTag() {}
	}
}
