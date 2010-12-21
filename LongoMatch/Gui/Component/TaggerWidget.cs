// 
//  Copyright (C) 2009 Andoni Morales Alastruey
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
using Gtk;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{
	
	
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TaggerWidget : Gtk.Bin
	{
		private Dictionary<Tag, CheckButton> tagsDict;
		
		public TaggerWidget()
		{
			this.Build();
			tagsDict = new Dictionary<Tag, CheckButton>();
			table1.NColumns = 5;
		}
		
		public TagsTemplate ProjectsTags{
			set{
				int tagsCount = value.Count();
				scrolledwindow1.Visible = tagsCount > 0;
				label1.Visible = !(tagsCount > 0);
							
				tagsDict.Clear();				
				
				foreach (Widget w in table1.AllChildren){
					w.Unrealize();
					table1.Remove(w);
				}
				
				for(int i=0;i<tagsCount;i++){
					AddTagWidget(value.GetTag(i), false);			
				}
			}
		}		
		
		public List<Tag> Tags{
			set{
				CheckButton button = null;
				foreach (Tag tag in value){
					if (tagsDict.TryGetValue(tag, out button))
						button.Active = true;
				}
			}
			get{
				List<Tag> list = new List<Tag>();
				foreach (KeyValuePair<Tag, CheckButton> pair in tagsDict){
					if (pair.Value.Active)
						list.Add(pair.Key);
				}
				return list;
			}
		}
		
		private void AddTag(string text, bool check){
			Tag tag = new Tag {
				Value = text,
			};
			if (tagsDict.ContainsKey(tag))
				return;
			AddTagWidget(tag, check);
		}
		
		private void AddTagWidget(Tag tag, bool check){
			CheckButton button = new CheckButton(tag.Value);
			button.Name = tag.Value;
			AddElementToTable(button);
			button.Active = check;
			tagsDict.Add(tag, button);
		}
			
		private void AddElementToTable(CheckButton button){
			uint row_top,row_bottom,col_left,col_right;
			int index = tagsDict.Count;
			
			table1.NRows =(uint) (index/5 + 1);			
			row_top =(uint) (index/table1.NColumns);
			row_bottom = (uint) row_top+1 ;
			col_left = (uint) index%table1.NColumns;
			col_right = (uint) col_left+1 ;
			
			table1.Attach(button,col_left,col_right,row_top,row_bottom);	
			button.Show();
		}

		protected virtual void OnTagbuttonClicked (object sender, System.EventArgs e)
		{
			Tag tag;
			CheckButton button;
			
			// Don't allow tags with void strings
			if (entry1.Text == "")
				return;
			// Check if it's the first tag and show the tags table
			if (tagsDict.Count == 0){
				scrolledwindow1.Visible = true;
				label1.Visible = false;
			}
			tag = new Tag{
				Value = entry1.Text,
			};
			if (tagsDict.TryGetValue(tag, out button))
				button.Active = true;
			else
				AddTag(entry1.Text, true);	
			entry1.Text = "";
		}

		protected virtual void OnEntry1Activated (object sender, System.EventArgs e)
		{
			tagbutton.Click();
		}	
	}
}