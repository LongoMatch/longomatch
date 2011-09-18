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
using System.IO;
using Gtk;
using Mono.Unix;

using LongoMatch.Interfaces;
using LongoMatch.Gui.Component;
using LongoMatch.Store.Templates;
using LongoMatch.Services;

namespace LongoMatch.Gui.Dialog
{

	/* HACK: Stetic doesn't allow the use of generics, which is needed for 
	 * the different types of Template classes */
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class TemplatesManager : Gtk.Dialog
	{
		protected virtual void OnSavebuttonClicked(object sender, System.EventArgs e) {}
		protected virtual void OnNewbuttonClicked(object sender, System.EventArgs e) {}
		protected virtual void OnDeletebuttonClicked(object sender, System.EventArgs e) {}
		protected virtual void OnButtonCancelClicked(object sender, System.EventArgs e) {}
		protected virtual void OnTreeviewCursorChanged(object sender, System.EventArgs e) {}
		protected virtual void OnButtonOkClicked(object sender, System.EventArgs e) {}
		protected virtual void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args) {}
		
		public TemplatesManager () {
			this.Build();
		}
		
		public TreeView TreeView {
			get {
				return treeview;
			}
		}
		
		public new bool Sensitive {
			set{
				savebutton.Sensitive = value;
				deletebutton.Sensitive = value;
				base.Sensitive = value;
			}
		}
		
		public void AddTemplateEditor (Widget w) {
			templateditorbox.Add(w);
			w.Show();
		}
	}
	
	public class TemplatesManager<T, U> : TemplatesManager where T: ITemplate<U>
	{
		private TreeView treeview;
		private Gtk.ListStore dataFileListStore;
		private ITemplateProvider<T, U> templatesProvider;
		private T selectedTemplate;
		private ITemplateWidget<T, U> templatesWidget;
		private string templateName;

		public TemplatesManager() : base()
		{
			treeview = this.TreeView;
			templatesProvider = MainClass.ts.GetTemplateProvider<T, U> ();
			templatesWidget = MainClass.ts.GetTemplateEditor <T, U> ();
			AddTemplateEditor ((Widget) templatesWidget);
			
			Gtk.TreeViewColumn templateFileColumn = new Gtk.TreeViewColumn();
			templateFileColumn.Title = Catalog.GetString("Templates Files");
			Gtk.CellRendererText templateFileCell = new Gtk.CellRendererText();
			templateFileColumn.PackStart(templateFileCell, true);
			templateFileColumn.SetCellDataFunc(templateFileCell, new Gtk.TreeCellDataFunc(RenderTemplateFile));
			treeview.AppendColumn(templateFileColumn);
			Fill();
		}
		
		public new bool Sensitive {
			set{
				(templatesWidget as Widget).Sensitive = value;
				base.Sensitive = value;
			}
		}
		
		public bool Edited {
			set {
				templatesWidget.Edited = value;
			}
			get {
				return templatesWidget.Edited;
			}
		}
		private void Fill() {
			dataFileListStore = new Gtk.ListStore(typeof(string));

			foreach(string filePath in templatesProvider.TemplatesNames) {
				dataFileListStore.AppendValues(filePath);
			}
			treeview.Model = dataFileListStore;
		}

		private void RenderTemplateFile(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			(cell as Gtk.CellRendererText).Text = (string) model.GetValue(iter, 0);
		}
		
		public void SetTemplate (T template) {
			templatesWidget.Template = template;
		}
		
		private void UpdateSelectedTemplate() {
			templatesWidget.Template = templatesProvider.Load(templateName); 
		}
		
		private void SaveTemplate() {
			selectedTemplate = templatesWidget.Template;
			templatesProvider.Update(selectedTemplate);
		}

		private void SelectTemplate(string name) {
			TreeIter iter;
			string tName;
			ListStore model = (ListStore)treeview.Model;

			model.GetIterFirst(out iter);
			while(model.IterIsValid(iter)) {
				tName = (string) model.GetValue(iter,0);
				if(tName == name) {
					this.templateName = tName;
					treeview.SetCursor(model.GetPath(iter),null,false);
					return;
				}
				model.IterNext(ref iter);
			}
		}

		private void PromptForSave() {
			MessageDialog mes = new MessageDialog(this,DialogFlags.Modal,
			                                      MessageType.Question,
			                                      ButtonsType.YesNo,
			                                      Catalog.GetString("The template has been modified. " +
			                                                        "Do you want to save it? "));
			if(mes.Run() == (int)ResponseType.Yes) {
				SaveTemplate();
			}
			mes.Destroy();
		}

		protected override void OnSavebuttonClicked(object sender, System.EventArgs e)
		{
			SaveTemplate();
			Edited = false;
		}

		protected override void OnNewbuttonClicked(object sender, System.EventArgs e)
		{
			string name;
			int count;
			bool showCount;
			List<string> availableTemplates = new List<string>();
			
			showCount = typeof(T) == typeof(TeamTemplate);
			EntryDialog ed = new  EntryDialog{ShowCount = showCount,
				Title = Catalog.GetString("Template name")};
			
			foreach(string templateName in templatesProvider.TemplatesNames) {
				if(templateName != "default")
					availableTemplates.Add(templateName);
			}
			ed.AvailableTemplates = availableTemplates;

			if(ed.Run() == (int)ResponseType.Ok) {
				name = ed.Text;
				count = ed.Count;
				if(name == "") {
					MessagePopup.PopupMessage(ed, MessageType.Warning,
					                          Catalog.GetString("You cannot create a template with a void name"));
					ed.Destroy();
					return;
				} else if (templatesProvider.Exists(name)) {
					MessagePopup.PopupMessage(ed, MessageType.Warning,
					                          Catalog.GetString("A template with this name already exists"));
					ed.Destroy();
					return;
				}

				/* Check if we are copying an existing template */
				if(ed.SelectedTemplate != null)
					templatesProvider.Copy(ed.SelectedTemplate, name);
				else
					templatesProvider.Create(name, count);
				
				Fill();
				SelectTemplate(name);
			}
			ed.Destroy();
		}

		protected override void OnDeletebuttonClicked(object sender, System.EventArgs e)
		{
			if(templateName =="default") {
				MessagePopup.PopupMessage(this,MessageType.Warning,
				                          Catalog.GetString("You can't delete the 'default' template"));
				return;
			}

			MessageDialog mes = new MessageDialog(this,DialogFlags.Modal,MessageType.Warning,ButtonsType.YesNo,
			                                      Catalog.GetString("Do you really want to delete the template: ")+
			                                      templateName);
			if(mes.Run() == (int)ResponseType.Yes) {
				templatesProvider.Delete(templateName);
				this.Fill();
				//The default template is always there so we select this one.
				//This allow to reset all the fields in the sections/players
				//properties.
				SelectTemplate("default");
			}
			mes.Destroy();
		}

		protected override void OnButtonCancelClicked(object sender, System.EventArgs e)
		{
			this.Destroy();
		}

		protected override void OnTreeviewCursorChanged(object sender, System.EventArgs e)
		{
			TreeIter iter;

			if(Edited)
				PromptForSave();

			treeview.Selection.GetSelected(out iter);
			templateName = (string) this.dataFileListStore.GetValue(iter, 0);

			UpdateSelectedTemplate();
			Sensitive = true;
		}

		protected override void OnButtonOkClicked(object sender, System.EventArgs e)
		{
			if(Edited)
				PromptForSave();
			this.Destroy();
		}

		protected override void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			UpdateSelectedTemplate();
		}
	}
}
