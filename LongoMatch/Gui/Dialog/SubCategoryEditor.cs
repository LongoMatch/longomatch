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
using Gtk;
using Mono.Unix;
using LongoMatch.Store;

namespace LongoMatch.Gui.Dialog
{
	public partial class SubCategoryEditor : Gtk.Dialog
	{
		private TagSubCategory subcat;
		
		public SubCategoryEditor ()
		{
			this.Build ();
		}
		
		public TagSubCategory SubCategory{
			set {
				ListStore store = new ListStore(typeof (string));
				foreach (string tag in value.Options)
					store.AppendValues(tag);
				subcat = value;
			}
			get {
				return subcat;
			}
		}
		
		protected virtual void OnDeletebuttonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			string tag;
			
			if (!treeview1.Selection.GetSelected(out iter))
				return;
			tag = (string)treeview1.Model.GetValue(iter, 0);
			SubCategory.Options.Remove(tag);
			(treeview1.Model as ListStore).Remove(ref iter);
		}		
		
		protected virtual void OnEditbuttonClicked (object sender, System.EventArgs e)
		{
			EntryDialog nameDialog;
			string old_option, new_option;
			TreeIter iter;
			
			if (!treeview1.Selection.GetSelected(out iter))
				return;
			
			old_option = (string)treeview1.Model.GetValue(iter, 0);
			
			nameDialog = new EntryDialog();
			nameDialog.ShowCount = false;
			nameDialog.Text = old_option;
			if (nameDialog.Run() == (int)ResponseType.Ok) {
				new_option = nameDialog.Text;
				if (new_option != old_option) {
					SubCategory.Options.Remove(old_option);
					SubCategory.Options.Add(new_option);
					old_option = new_option;
				}
			}
			nameDialog.Dispose();
		}
		
		protected virtual void OnAddbuttonClicked (object sender, System.EventArgs e)
		{
			EntryDialog nameDialog = new EntryDialog();
			nameDialog.ShowCount = false;
			if (nameDialog.Run() == (int)ResponseType.Ok) {
				SubCategory.Options.Add(nameDialog.Text);
				(treeview1.Model as ListStore).AppendValues(nameDialog.Text);
			}
			nameDialog.Dispose();
		}
		
		protected virtual void OnEntry1Changed (object sender, System.EventArgs e)
		{
			SubCategory.Name = nameentry.Text;
		}
		
		protected virtual void OnTreeview1RowActivated (object o, Gtk.RowActivatedArgs args)
		{
		}
		
		protected virtual void OnTreeview1CursorChanged (object sender, System.EventArgs e)
		{
		}
	}
}

