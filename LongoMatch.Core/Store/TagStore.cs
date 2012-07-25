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
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;

namespace LongoMatch.Store
{
	[Serializable]
	public class TagsStore<T, W> where T:ITag<W>
	{
		protected List<T> tagsList;
		
		public TagsStore(){
			tagsList = new List<T>();
		}
		
		public List<T> Tags {
			get{
				return tagsList;
			}
			set {
				tagsList = value;
			}
		}
		
		public void Add(T tag) {
			Log.Debug(String.Format("Adding tag {0} with subcategory{1}", tag, tag.SubCategory));
			tagsList.Add(tag);
		}
		
		public void Remove(T tag) {
			try {
				tagsList.Remove (tag);
			} catch (Exception e) {
				Log.Warning("Error removing tag " + tag.ToString());
				Log.Exception(e);
			}
		}
		
		public bool Contains(T tag) {
			return tagsList.Contains(tag);
		}
		
		public void RemoveBySubcategory(ISubCategory subcat) {
			tagsList.RemoveAll(t => t.SubCategory == subcat);
		}
		
		public List<T> AllUniqueElements {
			get {
				return (from tag in tagsList
				        group tag by tag into g
				        select g.Key).ToList();
			}
		}
		
		public List<T> GetTags(ISubCategory subCategory) {
			return (from tag in tagsList
			        where tag.SubCategory.Equals(subCategory)
			        select tag).ToList();
		}
		
		public List<W> GetTagsValues() {
			return (from tag in tagsList
			        select tag.Value).ToList();
		}
	}
	
	[Serializable]
	public class StringTagStore: TagsStore<StringTag, string> {}
	
	[Serializable]
	public class PlayersTagStore: TagsStore<PlayerTag, Player> {
		
		public void RemoveByPlayer(Player player) {
			tagsList.RemoveAll(t => t.Value == player);
		}
		
	}
	
	[Serializable]
	public class TeamsTagStore: TagsStore<TeamTag, Team> {}
}

