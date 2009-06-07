// SectionsTemplates.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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

namespace LongoMatch.Gui.Dialog
{
	
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class SectionsTemplates : Gtk.Dialog
	{
		
		private Gtk.ListStore dataFileListStore;
		private Sections selectedSections;
		private string templateName;

		public SectionsTemplates()
		{
			this.Build();				
			Gtk.TreeViewColumn templateFileColumn = new Gtk.TreeViewColumn ();
			templateFileColumn.Title = Catalog.GetString("Templates Files");
			Gtk.CellRendererText templateFileCell = new Gtk.CellRendererText ();
			templateFileColumn.PackStart (templateFileCell, true);
			templateFileColumn.SetCellDataFunc (templateFileCell, new Gtk.TreeCellDataFunc (RenderTemplateFile));
			treeview.AppendColumn (templateFileColumn);
			this.Fill();

		}	
		
		//Recorrer el directorio en busca de los archivos de configuración validos
		private void Fill (){
			this.dataFileListStore = new Gtk.ListStore (typeof (string));
			string[] allFiles = System.IO.Directory.GetFiles(MainClass.TemplatesDir(),"*.sct");
			foreach (string filePath in allFiles){
				dataFileListStore.AppendValues (filePath);
			}			
			this.treeview.Model = dataFileListStore;
		}
		
		private void RenderTemplateFile (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			string _templateFilePath = (string) model.GetValue (iter, 0); 
			(cell as Gtk.CellRendererText).Text = System.IO.Path.GetFileNameWithoutExtension(_templateFilePath.ToString());
		}
		
		public void SetSections(Sections sections){				

			this.sectionspropertieswidget1.SetSections(sections);
			
		}
		
	

		private void SetSensitive (bool sensitive){
			this.sectionspropertieswidget1.Sensitive = true;
			this.savebutton.Sensitive = sensitive;
			this.deletebutton.Sensitive = sensitive;
		}



		protected virtual void OnSavebuttonClicked (object sender, System.EventArgs e)
		{
			this.selectedSections = this.sectionspropertieswidget1.GetSections();
			SectionsWriter.UpdateTemplate (this.templateName,this.selectedSections);
			
		}

		protected virtual void OnNewbuttonClicked (object sender, System.EventArgs e)
		{
			
			string name;
			EntryDialog ed= new  EntryDialog();
			ed.Title = Catalog.GetString("Template name");
			//FIXME check if the template already exists or the name is null
			if (ed.Run() == (int)ResponseType.Ok){
				name = ed.Text;
				if (name == ""){
					MessageDialog mes = new MessageDialog(this,DialogFlags.Modal,MessageType.Warning,ButtonsType.Ok,
			                                      Catalog.GetString("You cannot create a template with a void name"));
					mes.Run();
					mes.Destroy();
					ed.Destroy();
					return;
				}
				if (System.IO.File.Exists(System.IO.Path.Combine(MainClass.TemplatesDir(),name+".sct"))){
					MessageDialog mes = new MessageDialog(this,DialogFlags.Modal,MessageType.Warning,ButtonsType.Ok,
			                                      Catalog.GetString("A template with this name already exists"));
					mes.Run();
					mes.Destroy();
					ed.Destroy();
					return;					
				}
				SectionsWriter.CreateNewTemplate(name+".sct");
				this.Fill();
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

			this.treeview.Selection.GetSelected(out iter);
			this.templateName = (string) this.dataFileListStore.GetValue (iter, 0);

			SectionsReader sr = new SectionsReader(this.templateName);
			this.selectedSections = sr.GetSections();
			this.SetSections(sr.GetSections());
			this.SetSensitive (true);
		}

		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			this.Destroy();
		}

		

	}
}
	