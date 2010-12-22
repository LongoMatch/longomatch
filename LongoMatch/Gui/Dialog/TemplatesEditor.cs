// TemplatesManager.cs
//
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
using Gtk;
using LongoMatch.Store.Templates;
using Mono.Unix;

namespace LongoMatch.Gui.Dialog
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class TemplatesManager : Gtk.Dialog
	{

		public enum UseType {
			TeamTemplate,
			CategoriesTemplate,
		}


		private Gtk.ListStore dataFileListStore;
		private Categories selectedCategoriesTemplate;
		private TeamTemplate selectedTeamTemplate;
		private UseType useType;
		private string templateName;
		private string fileExtension;

		public TemplatesManager(UseType type)
		{
			this.Build();
			Gtk.TreeViewColumn templateFileColumn = new Gtk.TreeViewColumn();
			templateFileColumn.Title = Catalog.GetString("Templates Files");
			Gtk.CellRendererText templateFileCell = new Gtk.CellRendererText();
			templateFileColumn.PackStart(templateFileCell, true);
			templateFileColumn.SetCellDataFunc(templateFileCell, new Gtk.TreeCellDataFunc(RenderTemplateFile));
			treeview.AppendColumn(templateFileColumn);
			Use = type;
			sectionspropertieswidget1.CanExport = false;
		}

		public UseType Use {
			set {
				useType = value;
				if (useType == UseType.TeamTemplate) {
					fileExtension = ".tem";
					teamtemplatewidget1.Visible = true;

				}
				else {
					fileExtension=".sct";
					sectionspropertieswidget1.Visible = true;
				}
				Fill();
			}
		}

		//Recorrer el directorio en busca de los archivos de configuración validos
		private void Fill() {
			string[] allFiles = System.IO.Directory.GetFiles(MainClass.TemplatesDir(),"*"+fileExtension);

			dataFileListStore = new Gtk.ListStore(typeof(string));

			foreach (string filePath in allFiles) {
				dataFileListStore.AppendValues(filePath);
			}
			treeview.Model = dataFileListStore;
		}

		private void RenderTemplateFile(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			string _templateFilePath = (string) model.GetValue(iter, 0);
			(cell as Gtk.CellRendererText).Text = System.IO.Path.GetFileNameWithoutExtension(_templateFilePath.ToString());
		}

		public void SetCategoriesTemplate(Categories sections) {
			if (useType != UseType.CategoriesTemplate)
				return;
			sectionspropertieswidget1.Categories=sections;
		}

		public void SetTeamTemplate(TeamTemplate template) {
			if (useType != UseType.TeamTemplate)
				return;
			teamtemplatewidget1.TeamTemplate=template;
		}

		private void UpdateCategories() {
			selectedCategoriesTemplate = Categories.Load(templateName);
			SetCategoriesTemplate(selectedCategoriesTemplate);
			SetSensitive(true);
		}

		private void UpdateTeamTemplate() {
			selectedTeamTemplate = TeamTemplate.Load(templateName);
			SetTeamTemplate(selectedTeamTemplate);
			SetSensitive(true);
		}

		private void SetSensitive(bool sensitive) {
			if (useType == UseType.CategoriesTemplate)
				sectionspropertieswidget1.Sensitive = true;
			else
				teamtemplatewidget1.Sensitive = true;
			savebutton.Sensitive = sensitive;
			deletebutton.Sensitive = sensitive;
		}

		private void SelectTemplate(string templateName) {
			TreeIter iter;
			string tName;
			ListStore model = (ListStore)treeview.Model;

			model.GetIterFirst(out iter);
			while (model.IterIsValid(iter)) {
				tName = System.IO.Path.GetFileNameWithoutExtension((string) model.GetValue(iter,0));
				if (tName == templateName) {
					//Do not delete 'this' as we want to change the class attribute
					this.templateName = templateName = (string) this.dataFileListStore.GetValue(iter, 0);
					treeview.SetCursor(model.GetPath(iter),null,false);
					return;
				}
				model.IterNext(ref iter);
			}
		}

		private void SaveTemplate() {
			if (useType == UseType.CategoriesTemplate) {
				selectedCategoriesTemplate = sectionspropertieswidget1.Categories;
				selectedCategoriesTemplate.Save(templateName);
			}
			else {
				selectedTeamTemplate = teamtemplatewidget1.TeamTemplate;
				selectedTeamTemplate.Save(templateName);
			}
		}

		private void PromptForSave() {
			MessageDialog mes = new MessageDialog(this,DialogFlags.Modal,MessageType.Question,ButtonsType.YesNo,
			                                      Catalog.GetString("The template has been modified. Do you want to save it? "));
			if (mes.Run() == (int)ResponseType.Yes) {
				SaveTemplate();
			}
			mes.Destroy();
		}

		protected virtual void OnSavebuttonClicked(object sender, System.EventArgs e)
		{
			SaveTemplate();
			sectionspropertieswidget1.Edited=false;
			teamtemplatewidget1.Edited=false;
		}

		protected virtual void OnNewbuttonClicked(object sender, System.EventArgs e)
		{
			string name;
			int count;
			string [] templates = null;
			List<string> availableTemplates = new List<string>();
			EntryDialog ed= new  EntryDialog();

			ed.Title = Catalog.GetString("Template name");
			
			if (useType == UseType.TeamTemplate){
				ed.ShowCount=true;				
			}
			
			templates = System.IO.Directory.GetFiles(MainClass.TemplatesDir());
			foreach (String text in templates){
				string templateName = System.IO.Path.GetFileName(text);
				if (templateName.EndsWith(fileExtension) && templateName != "default"+fileExtension)
					availableTemplates.Add(templateName);					
			}
			ed.AvailableTemplates = availableTemplates; 


			if (ed.Run() == (int)ResponseType.Ok) {
				name = ed.Text;
				count = ed.Count;
				if (name == "") {
					MessagePopup.PopupMessage(ed, MessageType.Warning,
					                          Catalog.GetString("You cannot create a template with a void name"));
					ed.Destroy();
					return;
				}
				if (System.IO.File.Exists(System.IO.Path.Combine(MainClass.TemplatesDir(),name+fileExtension))) {
					MessagePopup.PopupMessage(ed, MessageType.Warning,
					                          Catalog.GetString("A template with this name already exists"));
					ed.Destroy();
					return;
				}	
				
				if (ed.SelectedTemplate != null)
						System.IO.File.Copy(System.IO.Path.Combine(MainClass.TemplatesDir(),ed.SelectedTemplate),
						                    System.IO.Path.Combine(MainClass.TemplatesDir(),name+fileExtension));
				else if (useType == UseType.CategoriesTemplate){
					Categories cat = Categories.DefaultTemplate();
					cat.Save(name+fileExtension);
				}
				else {
					TeamTemplate tt = TeamTemplate.DefaultTemplate(count);
					tt.Save(System.IO.Path.Combine(MainClass.TemplatesDir(), name+fileExtension));
				}

				Fill();
				SelectTemplate(name);
			}
			ed.Destroy();
		}

		protected virtual void OnDeletebuttonClicked(object sender, System.EventArgs e)
		{
			if (System.IO.Path.GetFileNameWithoutExtension(templateName) =="default") {
				MessagePopup.PopupMessage(this,MessageType.Warning,Catalog.GetString("You can't delete the 'default' template"));
				return;
			}

			MessageDialog mes = new MessageDialog(this,DialogFlags.Modal,MessageType.Warning,ButtonsType.YesNo,
			                                      Catalog.GetString("Do you really want to delete the template: ")+
			                                      System.IO.Path.GetFileNameWithoutExtension(templateName));
			if (mes.Run() == (int)ResponseType.Yes) {
				System.IO.File.Delete(templateName);
				this.Fill();
				//The default template is always there so we select this one.
				//This allow to reset all the fields in the sections/players
				//properties.
				SelectTemplate("default");
			}
			mes.Destroy();
		}

		protected virtual void OnButtonCancelClicked(object sender, System.EventArgs e)
		{
			this.Destroy();
		}

		protected virtual void OnTreeviewCursorChanged(object sender, System.EventArgs e)
		{
			TreeIter iter;

			if (sectionspropertieswidget1.Edited || teamtemplatewidget1.Edited)
				PromptForSave();

			treeview.Selection.GetSelected(out iter);
			templateName = (string) this.dataFileListStore.GetValue(iter, 0);

			if (useType == UseType.CategoriesTemplate)
				UpdateCategories();

			else
				UpdateTeamTemplate();
		}

		protected virtual void OnButtonOkClicked(object sender, System.EventArgs e)
		{
			if (sectionspropertieswidget1.Edited)
				PromptForSave();
			this.Destroy();
		}

		protected virtual void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if (useType == UseType.CategoriesTemplate)
				UpdateCategories();
			else
				UpdateTeamTemplate();
		}
	}
}
