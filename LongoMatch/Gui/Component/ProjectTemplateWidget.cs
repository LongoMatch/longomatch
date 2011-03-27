// CategoriesPropertiesWidget.cs
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using System.IO;
using Gdk;
using Gtk;

using LongoMatch.Gui.Dialog;
using LongoMatch.Interfaces;
using LongoMatch.IO;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using Mono.Unix;


namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectTemplateWidget : Gtk.Bin, ITemplateWidget<Categories>
	{
		private List<HotKey> hkList;
		private Project project;
		private Categories categories;
		private List<Category> selectedCategories;

		public ProjectTemplateWidget()
		{
			this.Build();
			hkList = new List<HotKey>();
		}

		public Project Project {
			set {
				project = value;
				if(project != null)
					Template = project.Categories;
			}
		}

		public Categories Template {
			get {
				return categories;
			}
			set {
				categories = value;
				Edited = false;
				Gtk.TreeStore categoriesListStore = new Gtk.TreeStore(typeof(Category));
				hkList.Clear();

				foreach(var cat in categories) {
					categoriesListStore.AppendValues(cat);
					try {
						hkList.Add(cat.HotKey);
					} catch {}; //Do not add duplicated hotkeys
				}
				categoriestreeview.Model = categoriesListStore;
				ButtonsSensitive = false;
			}
		}

		public bool CanExport {
			set {
				hseparator1.Visible = value;
				exportbutton.Visible = value;
			}
		}
		public bool Edited {
			get;
			set;
		}

		private void UpdateModel() {
			Template = Template;
		}

		private void AddCategory(int index) {
			Category tn;
			HotKey hkey = new HotKey();

			Time start = new Time {MSeconds = 10*Time.SECONDS_TO_TIME};
			Time stop = new Time {MSeconds = 10*Time.SECONDS_TO_TIME};

			tn  = new Category {
				Name = "New Section",
				Start = start,
				Stop = stop,
				HotKey = hkey,
				Color =	new Color(Byte.MaxValue,Byte.MinValue,Byte.MinValue)
			};

			if(project != null) {
				/* Editing a project template */
				project.Categories.Insert(index,tn);
			} else {
				/* Editing a template in the templates editor */
				categories.Insert(index,tn);
			}
			UpdateModel();
			Edited = true;
		}

		private void RemoveSelectedCategories() {
			if(project!= null) {
				MessageDialog dialog = new MessageDialog((Gtk.Window)this.Toplevel,DialogFlags.Modal,MessageType.Question,
				                ButtonsType.YesNo,true,
				                Catalog.GetString("You are about to delete a category and all the plays added to this category. Do you want to proceed?"));
				if(dialog.Run() == (int)ResponseType.Yes) {
					try {
						foreach(Category cat in selectedCategories)
							project.Categories.Remove(cat);
					} catch {
						MessagePopup.PopupMessage(this,MessageType.Warning,
						                          Catalog.GetString("A template needs at least one category"));
					}
				}
				dialog.Destroy();
				categories = project.Categories;
			} else {
				foreach(Category cat in selectedCategories) {
					if(categories.Count == 1) {
						MessagePopup.PopupMessage(this,MessageType.Warning,
						                          Catalog.GetString("A template needs at least one category"));
					} else
						categories.Remove(cat);
				}
			}
			UpdateModel();
			Edited = true;
			selectedCategories = null;
			ButtonsSensitive=false;
		}

		private bool ButtonsSensitive {
			set {
				newprevbutton.Sensitive = value;
				newafterbutton.Sensitive = value;
				removebutton.Sensitive = value;
				editbutton.Sensitive = value;
			}
		}

		private void EditSelectedSection() {
			EditCategoryDialog dialog = new EditCategoryDialog();
			dialog.Category = selectedCategories[0];
			dialog.HotKeysList = hkList;
			dialog.TransientFor = (Gtk.Window) Toplevel;
			dialog.Run();
			dialog.Destroy();
			Edited = true;
		}

		protected virtual void OnNewAfter(object sender, EventArgs args) {
			AddCategory(categories.IndexOf(selectedCategories[0])+1);
		}

		protected virtual void OnNewBefore(object sender, EventArgs args) {
			AddCategory(categories.IndexOf(selectedCategories[0]));
		}

		protected virtual void OnRemove(object sender, EventArgs args) {
			RemoveSelectedCategories();
		}

		protected virtual void OnEdit(object sender, EventArgs args) {
			EditSelectedSection();
		}

		protected virtual void OnCategoriestreeviewSectionClicked(LongoMatch.Store.Category tNode)
		{
			EditSelectedSection();
		}

		protected virtual void OnCategoriestreeviewCategoriesSelected(List<Category> tNodesList)
		{
			selectedCategories = tNodesList;
			if(tNodesList.Count == 0)
				ButtonsSensitive = false;
			else if(tNodesList.Count == 1) {
				ButtonsSensitive = true;
			}
			else {
				newprevbutton.Sensitive = false;
				newafterbutton.Sensitive = false;
				removebutton.Sensitive = true;
				editbutton.Sensitive = false;
			}
		}

		protected virtual void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Delete && selectedCategories != null)
				RemoveSelectedCategories();
		}

		protected virtual void OnExportbuttonClicked(object sender, System.EventArgs e)
		{
			EntryDialog dialog = new EntryDialog();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;
			dialog.ShowCount = false;
			dialog.Text = Catalog.GetString("New template");
			if(dialog.Run() == (int)ResponseType.Ok) {
				if(dialog.Text == "")
					MessagePopup.PopupMessage(dialog, MessageType.Error,
					                          Catalog.GetString("The template name is void."));
				else if(File.Exists(System.IO.Path.Combine(MainClass.TemplatesDir(),dialog.Text+".sct"))) {
					MessageDialog md = new MessageDialog(null,
					                                     DialogFlags.Modal,
					                                     MessageType.Question,
					                                     Gtk.ButtonsType.YesNo,
					                                     Catalog.GetString("The template already exists. " +
					                                                     "Do you want to overwrite it ?")
					                                    );
					if(md.Run() == (int)ResponseType.Yes)
						Template.Save(dialog.Text);
					md.Destroy();
				}
				else
					Template.Save(dialog.Text);
			}
			dialog.Destroy();
		}
	}
}
