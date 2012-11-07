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
using System.Linq;
using System.Collections.Generic;
using Gtk;

using LongoMatch.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class StringTaggerWidget : Gtk.Bin
	{
		Dictionary<StringTag, CheckButton> dict;
		StringTagStore tags;
		RadioButton firstRB;
		uint count;
		const uint BUTTONS_PER_ROW = 3;
		TagSubCategory subcategory;
		
		public StringTaggerWidget (TagSubCategory subcategory, StringTagStore tags)
		{
			this.Build ();
			this.subcategory = subcategory;
			this.tags = tags;
			count = 0;
			table.NColumns = 3;
			PopulateGui();
			UpdateTags();
		}
		
		private void PopulateGui() {
			Title = subcategory.Name;
			dict = new Dictionary<StringTag, CheckButton>();
			foreach (string tag in subcategory)
				AddTagWidget(new StringTag{Value=tag, SubCategory=subcategory},
				             !subcategory.AllowMultiple);
		}
		
		public void UpdateTags() {
			List<StringTag> stringTags = tags.GetTags(subcategory);
			
			/* Set a default value */
			if (stringTags.Count == 0) {
				StringTag tag = new StringTag{Value=subcategory[0], SubCategory=subcategory}; 
				tags.Add(tag);
				stringTags.Add(tag);
			}
			
			foreach (var tag in stringTags) {
				if (dict.ContainsKey(tag)) 	
					dict[tag].Active = true;
			}
		}
		
		private void AddTagWidget (StringTag tag, bool radio){
			CheckButton button;
			uint row, col;
			
			if (radio) {
				if (firstRB == null) 
					button = firstRB = new RadioButton (tag.Value);
				else
					button = new RadioButton(firstRB, tag.Value);
			} else {
				button = new CheckButton(tag.Value);
			}
			
			button.Toggled += delegate(object sender, EventArgs e) {
				if (button.Active) {
					if (!tags.Contains(tag))
						tags.Add(tag);
				} else
					tags.Remove(tag);
			};
			dict.Add(tag, button);
			row = count / BUTTONS_PER_ROW;
			col = count % BUTTONS_PER_ROW;
			table.Attach (button, row, row+1, col, col+1);
			button.ShowAll();
			count ++;
		} 
		
		private string Title {
			set {
				titlelabel.Markup = "<b>" + value + "</b>";
			}
		}
	}
}

