// DBManager.cs
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
using System.Collections;
using Gtk;
using Mono.Unix;
using LongoMatch.DB;
using LongoMatch.Gui.Component;

namespace LongoMatch.Gui.Dialog
{
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class ProjectsManager : Gtk.Dialog
	{

		public bool edited;
		public string originalFilePath;
		
		public ProjectsManager()
		{
			this.Build();
			this.Fill();
			this.filedescriptionwidget3.Use = LongoMatch.Gui.Component.UseType.EditProject;
		}
		
		public void Fill(){
			ArrayList allDB = MainClass.DB.GetAllDB();
			projectlistwidget1.Fill(allDB);
			projectlistwidget1.ClearSearch();
			filedescriptionwidget3.Clear();
			filedescriptionwidget3.Sensitive = false;
			saveButton.Sensitive = false;
			deleteButton.Sensitive = false;
			originalFilePath=null;	
		}
		
		private void SaveProject(){
			Project project = filedescriptionwidget3.GetProject();
						
			if (project == null)
				return;
							
			if (project.File.FilePath == originalFilePath)
				MainClass.DB.UpdateProject(project);
			else{
				try{
					MainClass.DB.UpdateProject(project,originalFilePath);
				}
				catch{
					MessagePopup.PopupMessage(this, MessageType.Warning, 
					                          Catalog.GetString("A Project is already using this file."));
				}
			}
			Fill();
			
		}


		protected virtual void OnDeleteButtonPressed (object sender, System.EventArgs e)
		{
			Project selectedProject = projectlistwidget1.GetSelection();
			if (selectedProject != null){
				if (MainWindow.OpenedProject()!= null && selectedProject.Equals(MainWindow.OpenedProject())) {
				
					MessagePopup.PopupMessage(this, MessageType.Warning, 
				                          Catalog.GetString("This Project is actually in use.")+"\n"+Catalog.GetString("Close it first to allow its removal from the database"));
				}
				else {
					MessageDialog md = new MessageDialog(this,DialogFlags.Modal,MessageType.Question,ButtonsType.YesNo,
					                                     Catalog.GetString("Do yo really want to delete:")+"\n"+selectedProject.File.FilePath);
					if (md.Run()== (int)ResponseType.Yes){
						filedescriptionwidget3.Clear();
						MainClass.DB.RemoveProject(selectedProject);	
						Fill();						
					}
					md.Destroy();
				}
			}		
		}	


		protected virtual void OnSaveButtonPressed (object sender, System.EventArgs e)
		{
			SaveProject();			
		}		
	

		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			this.Destroy();
		}

		protected virtual void OnProjectlistwidget1ProjectSelectedEvent (LongoMatch.DB.Project project)
		{
			if (edited){
				MessageDialog md = new MessageDialog((Window)this.Toplevel,DialogFlags.Modal,
				                                     MessageType.Question, ButtonsType.YesNo,
				                                     Catalog.GetString("The Project has been edited, do you want to save the changes?"));
				if (md.Run() == (int)ResponseType.Yes)
					SaveProject();
				edited=false;
					
			}
			if (MainWindow.OpenedProject()!= null && project.Equals(MainWindow.OpenedProject())) {
			
				MessagePopup.PopupMessage(this, MessageType.Warning, 
				                          Catalog.GetString("The Project you are trying to load is actually in use.")+"\n" +Catalog.GetString ("Close it first to edit it"));
				filedescriptionwidget3.Clear();
				filedescriptionwidget3.Sensitive = false;
				saveButton.Sensitive = false;
				deleteButton.Sensitive = false;				
			}
			else{
				filedescriptionwidget3.Sensitive = true;
				filedescriptionwidget3.SetProject(project);
				originalFilePath = project.File.FilePath;
				saveButton.Sensitive = true;
				deleteButton.Sensitive = true;
			}
		}		
	}
}
