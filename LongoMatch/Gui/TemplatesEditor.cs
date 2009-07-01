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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.IO;
using Gtk;
using Mono.Unix;
using System.Collections;
using LongoMatch.DB;
using LongoMatch.IO;
using LongoMatch.IO;

namespace LongoMatch.Gui.Dialog
{	
	
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class TemplatesManager : Gtk.Dialog
	{
		
		public enum UseType{
		TeamTemplate,
		SectionsTemplate,		
		}
	
		
		private Gtk.ListStore dataFileListStore;
		private Sections selectedSectionsTemplate;
		private TeamTemplate selectedTeamTemplate;
		private UseType useType;
		private string templateName;
		private string fileExtension;

		public TemplatesManager(UseType type)
		{
			this.Build();				
			Gtk.TreeViewColumn templateFileColumn = new Gtk.TreeViewColumn ();
			templateFileColumn.Title = Catalog.GetString("Templates Files");
			Gtk.CellRendererText templateFileCell = new Gtk.CellRendererText ();
			templateFileColumn.PackStart (templateFileCell, true);
			templateFileColumn.SetCellDataFunc (templateFileCell, new Gtk.TreeCellDataFunc (RenderTemplateFile));
			treeview.AppendColumn (templateFileColumn);
			Use = type;
		}	
		
		public UseType Use{
			set {
				useType = value;
				if (useType == UseType.TeamTemplate){
					fileExtension = ".tem";
					teamtemplatewidget1.Visible = true;
					
				}
				else{
					fileExtension=".sct";
					sectionspropertieswidget1.Visible = true;
				}
				Fill();
			}
		}
		
		//Recorrer el directorio en busca de los archivos de configuración validos
		private void Fill (){
			string[] allFiles = System.IO.Directory.GetFiles(MainClass.TemplatesDir(),"*"+fileExtension);
			
			dataFileListStore = new Gtk.ListStore (typeof (string));
			
			foreach (string filePath in allFiles){
				dataFileListStore.AppendValues (filePath);
			}			
			treeview.Model = dataFileListStore;
		}
		
		private void RenderTemplateFile (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			string _templateFilePath = (string) model.GetValue (iter, 0); 
			(cell as Gtk.CellRendererText).Text = System.IO.Path.GetFileNameWithoutExtension(_templateFilePath.ToString());
		}
		
		public void SetSectionsTemplate(Sections sections){	
			if (useType != UseType.SectionsTemplate)
				return;
			sectionspropertieswidget1.SetSections(sections);			
		}	
		
		public void SetTeamTemplate(TeamTemplate template){
			if (useType != UseType.TeamTemplate)
				return;
			teamtemplatewidget1.SetTeamTemplate(template);
		}
		
		private void UpdateSections(){
			SectionsReader sr = new SectionsReader(templateName);
			selectedSectionsTemplate = sr.GetSections();
			SetSectionsTemplate(sr.GetSections());
			SetSensitive (true);
		}	
		
	
		
		private void UpdateTeamTemplate(){
			SetTeamTemplate(TeamTemplate.LoadFromFile(templateName));
			SetSensitive(true);
			                
		}
		

		private void SetSensitive (bool sensitive){
			if (useType == UseType.SectionsTemplate)
				sectionspropertieswidget1.Sensitive = true;
			else
				teamtemplatewidget1.Sensitive = true;			    
			savebutton.Sensitive = sensitive;
			deletebutton.Sensitive = sensitive;
		}
		
		private void SelectTemplate (string templateName){
			TreeIter iter;
			string tName;
			ListStore model = (ListStore)treeview.Model;

			model.GetIterFirst(out iter);
			while (model.IterIsValid(iter)){				
				tName = System.IO.Path.GetFileNameWithoutExtension((string) model.GetValue(iter,0));
				if (tName == templateName){
					//Do not delete 'this' as we want to change the class attribute
					this.templateName = templateName = (string) this.dataFileListStore.GetValue (iter, 0);
					treeview.SetCursor(model.GetPath(iter),null,false);
					return;
				}
				model.IterNext(ref iter);
			}
		}

		protected virtual void OnSavebuttonClicked (object sender, System.EventArgs e)
		{
			if (useType == UseType.SectionsTemplate){
				selectedSectionsTemplate = sectionspropertieswidget1.GetSections();
				SectionsWriter.UpdateTemplate (templateName,selectedSectionsTemplate);	
			}
			else{
				selectedTeamTemplate = teamtemplatewidget1.GetTeamTemplate();
				selectedTeamTemplate.Save(templateName);
			}		
		}

		protected virtual void OnNewbuttonClicked (object sender, System.EventArgs e)
		{			
			string name;
			EntryDialog ed= new  EntryDialog();
			
			ed.Title = Catalog.GetString("Template name");
			
			if (ed.Run() == (int)ResponseType.Ok){
				name = ed.Text;
				if (name == ""){
					MessagePopup.PopupMessage(this, MessageType.Warning, 
				                          Catalog.GetString("You cannot create a template with a void name"));
					ed.Destroy();
					return;
				}
				if (System.IO.File.Exists(System.IO.Path.Combine(MainClass.TemplatesDir(),name+fileExtension))){
					MessagePopup.PopupMessage(this, MessageType.Warning, 
				                          Catalog.GetString("A template with this name already exists"));
					ed.Destroy();
					return;					
				}
				
				if (useType == UseType.SectionsTemplate)
					SectionsWriter.CreateNewTemplate(name+fileExtension);
				else {
					TeamTemplate tt = new TeamTemplate();
					tt.CreateDefaultTemplate(15);
					tt.Save(name+fileExtension);
				}
					
				Fill();
				SelectTemplate(name);
			}
			ed.Destroy();				
		}
		
		protected virtual void OnDeletebuttonClicked (object sender, System.EventArgs e)
		{
			MessageDialog mes = new MessageDialog(this,DialogFlags.Modal,MessageType.Warning,ButtonsType.YesNo,
			                                      Catalog.GetString("Do you really want to delete ")+templateName+Catalog.GetString(" template?"));
			if (mes.Run() == (int)ResponseType.Yes){
				System.IO.File.Delete(templateName);
				this.Fill();
			}
			mes.Destroy();			                                      
		}

		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			this.Destroy();
		}

		protected virtual void OnTreeviewCursorChanged (object sender, System.EventArgs e)
		{			
			TreeIter iter;

			treeview.Selection.GetSelected(out iter);
			templateName = (string) this.dataFileListStore.GetValue (iter, 0);
			
			if (useType == UseType.SectionsTemplate)
				UpdateSections();
			else
				UpdateTeamTemplate();
		}

		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			this.Destroy();
		}

		protected virtual void OnTreeviewRowActivated (object o, Gtk.RowActivatedArgs args)
		{			
			if (useType == UseType.SectionsTemplate)
				UpdateSections();
			else
				UpdateTeamTemplate();
		}
	}
}
	