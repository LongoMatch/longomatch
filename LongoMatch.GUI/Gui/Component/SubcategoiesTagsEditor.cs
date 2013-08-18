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
using LongoMatch.Store;
using System.Collections.Generic;
using Gdk;
using Gtk;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SubcategoiesTagsEditor : Gtk.Bin
	{
		TagSubCategory template;
		Dictionary<string, Widget>  tagsDict;
		Color templateExistsColor;
		
		public SubcategoiesTagsEditor ()
		{
			this.Build ();
			Gdk.Color.Parse("red", ref templateExistsColor);
			tagsDict = new Dictionary<string, Widget>();
			addtagbutton.Clicked += OnAddTag;
			tagentry.Activated += OnAddTag;
			nameentry.Changed += OnNameChanged;
			multicheckbutton.Toggled += OnMultiToggled;
			fastcheckbutton.Toggled += OnFastToggled;
			TemplatesNames = new List<string>();
		}
		
		public List<string> TemplatesNames {
			get;
			set;
		}
		
		public TagSubCategory Template {
			set{
				template = value;
				nameentry.Text = template.Name;
				fastcheckbutton.Active = template.FastTag;
				multicheckbutton.Active = template.AllowMultiple;
				
				foreach (string tag in template)
					AddTag(tag, false);
			}
			get {
				return template;
			}
		}
		
		public bool CheckName {
			set;
			get;
		}
		
		private void RemoveTag (string tag) {
			tagsbox.Remove(tagsDict[tag]);
			tagsDict.Remove(tag);
			template.Remove(tag);
		}
		
		private void AddTag(string tag, bool update) {
			HBox box;
			Label label;
			Button button;
			
			if (tagsDict.ContainsKey(tag))
				return;
			
			box = new HBox();
			label = new Label(tag);
			label.Justify = Justification.Left;
			button = new Button("gtk-delete");
			button.Clicked += delegate {
				RemoveTag(tag);
			};
			box.PackStart(label, true, false, 0);
			box.PackStart(button, false, false, 0);
			
			tagsbox.PackStart(box, false, false, 0);
			box.ShowAll();
			
			tagsDict.Add(tag, box);
			if (update)
				template.Add(tag);
		}
		
		protected virtual void OnNameChanged (object sender, EventArgs e)
		{
			if ((CheckName && TemplatesNames.Contains(nameentry.Text)) ||
			    nameentry.Text == "") {
				nameentry.ModifyText(StateType.Normal, templateExistsColor);
			} else { 
				nameentry.ModifyText(StateType.Normal);
				template.Name = nameentry.Text;
			}
		}
		
		protected virtual void OnMultiToggled (object sender, System.EventArgs e)
		{
			template.AllowMultiple = multicheckbutton.Active;
		}
		
		protected virtual void OnFastToggled (object sender, System.EventArgs e)
		{
			template.FastTag = fastcheckbutton.Active;
			
		}
		
		protected virtual void OnAddTag (object sender, System.EventArgs e)
		{
			AddTag(tagentry.Text, true);
		}
	}
}

