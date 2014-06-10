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
using Newtonsoft.Json;

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;

namespace LongoMatch.Store
{
	[Serializable]
	public class TagsStore
	{
		public TagsStore(){
			Tags = new List<Tag>();
		}
		
		public List<Tag> Tags {
			get;
			set;
		}
		
		public void Add(Tag tag) {
			Log.Debug(String.Format("Adding tag {0} with subcategory{1}", tag, tag.SubCategory));
			Tags.Add(tag);
		}
		
		public void Remove(Tag tag) {
			try {
				Tags.Remove (tag);
			} catch (Exception e) {
				Log.Warning("Error removing tag " + tag.ToString());
				Log.Exception(e);
			}
		}
		
		public bool Contains(Tag tag) {
			return Tags.Contains(tag);
		}
		
		public void RemoveBySubcategory(SubCategory subcat) {
			Tags.RemoveAll(t => t.SubCategory == subcat);
		}
		
		[JsonIgnore]
		public List<Tag> AllUniqueElements {
			get {
				return (from tag in Tags
				        group tag by tag into g
				        select g.Key).ToList();
			}
		}
		
		public List<Tag> GetTags(SubCategory subCategory) {
			return (from tag in Tags
			        where tag.SubCategory.Equals(subCategory)
			        select tag).ToList();
		}
		
		public List<string> GetTagsValues() {
			return (from tag in Tags
			        select tag.Value).ToList();
		}
	}
}

