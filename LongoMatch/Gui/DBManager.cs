// DBManager.cs
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
using System.Collections;
using Gtk;
using Mono.Unix;
using LongoMatch.DB;
using LongoMatch.Gui.Component;

namespace LongoMatch.Gui.Dialog
{
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class DBManager : Gtk.Dialog
	{

		public bool edited;
		
		public DBManager()
		{
			this.Build();
			this.Fill();
			this.filedescriptionwidget3.Use = LongoMatch.Gui.Component.UseType.EditProject;
		}
		
		public void Fill(){
			ArrayList allDB = MainClass.DB.GetAllDB();
			projectlistwidget1.Fill(allDB);
			this.filedescriptionwidget3.Clear();
			this.filedescriptionwidget3.Sensitive = false;
			this.saveButton.Sensitive = false;
			this.deleteButton.Sensitive = false;
			
		
		}
		
		private void SaveProject(){
			String previousFileName;			
			Project changedProject;
			
			previousFileName = projectlistwidget1.GetSelection().File.FilePath;			
			changedProject = this.filedescriptionwidget3.GetProject();
			
			if (changedProject != null){
				
				if (changedProject.File.FilePath == previousFileName)
					MainClass.DB.UpdateProject(changedProject);
				else{
					try{
						MainClass.DB.UpdateProject(changedProject,previousFileName);
					}
					catch{
						MessagePopup.PopupMessage(this, MessageType.Warning, 
				                          Catalog.GetString("A Project is already using this file."));
					}
				}
				this.Fill();
			}
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
						this.filedescriptionwidget3.Clear();
						MainClass.DB.RemoveProject(selectedProject);	
						this.Fill();						
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
			}
			else{
				this.filedescriptionwidget3.Sensitive = true;
				this.filedescriptionwidget3.SetProject(project);
				this.saveButton.Sensitive = true;
				this.deleteButton.Sensitive = true;
			}
		}		
	}
}
