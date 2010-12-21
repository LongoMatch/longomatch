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
using System.Collections.Generic;

namespace LongoMatch.Store.Templates
{

	[Serializable]
	public class TagsTemplate: Template
	{
		List<Tag> tagsList;
		public TagsTemplate()
		{
			tagsList = new List<Tag>();
		}

		public bool AddTag(Tag tag) {			
			if (tagsList.Contains(tag))
				return false;
			else
				tagsList.Add(tag);
			return true;
		}

		public bool RemoveTag (Tag tag) {
			return tagsList.Remove(tag);
		}
		
		public Tag GetTag(int index){
			return tagsList[index];
		}
		
		public int Count (){
			return tagsList.Count;
		}
		
		public IEnumerator<Tag> GetEnumerator(){
			return tagsList.GetEnumerator();
		}
		
		public void Save(string filePath){
			Save(this, filePath);
		}
		
		public static TagsTemplate Load(string filePath) {
			return Load<TagsTemplate>(filePath);
		}
		
		public static TagsTemplate DefaultTemplate() {
			TagsTemplate defaultTemplate = new TagsTemplate();
			defaultTemplate.FillDefaultTemplate();
			return defaultTemplate;
		}
		
		private void FillDefaultTemplate() {
			//FIXME: To implement
		}
	}
}
