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

namespace LongoMatch.Widgets.Dialog
{
	
	
	public partial class DBManager : Gtk.Dialog
	{

		
		
		public DBManager()
		{
			this.Build();
			this.Fill();
			
		}
		
		public void Fill(){
			ArrayList allDB = MainClass.DB.GetAllDB();
			projectlistwidget1.Fill(allDB);
			this.filedescriptionwidget3.Clear();
			this.filedescriptionwidget3.Sensitive = false;
			this.saveButton.Sensitive = false;
			this.deleteButton.Sensitive = false;
			
		
		}


		protected virtual void OnDeleteButtonPressed (object sender, System.EventArgs e)
		{
			Project selectedProject = projectlistwidget1.GetSelection();
			if (selectedProject != null){
				if (MainWindow.OpenedProject()!= null && selectedProject.Equals(MainWindow.OpenedProject())) {
				
					MessageDialog md = new MessageDialog(this,DialogFlags.Modal,MessageType.Warning,ButtonsType.Ok,
					                                     Catalog.GetString("This Project is actually in use.\n Close it first to allow its removal from the database"));
					md.Run();				
					md.Destroy();
				}
				else {
					MessageDialog md = new MessageDialog(this,DialogFlags.Modal,MessageType.Question,ButtonsType.YesNo,
					                                     Catalog.GetString("Do yo really want to delete:\n")+selectedProject.File.FilePath);
					if (md.Run()== (int)ResponseType.Yes){
						this.filedescriptionwidget3.Clear();
						MainClass.DB.RemoveProject(selectedProject);	
						string directory = MainClass.ThumbnailsDir()+"/"+selectedProject.Title;
						foreach (string path in System.IO.Directory.GetFiles(directory,"*")){
							System.IO.File.Delete(path);
						}
						System.IO.Directory.Delete(directory);
						this.Fill();
						
					}
					md.Destroy();
				}
			}
		
		}
		
	


		protected virtual void OnSaveButtonPressed (object sender, System.EventArgs e)
		{
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
						MessageDialog error = new MessageDialog(this,
						                                        DialogFlags.DestroyWithParent,
						                                        MessageType.Error,
						                                        ButtonsType.Ok,
						                                        "The Project for this file already exists.\nTry to edit it.");
						error.Run();
						error.Destroy();	
					}
				}
				this.Fill();
			}
			
		}

		protected virtual void OnFiledatalistwidget1ProjectSelectedEvent (Project project)
		{
			
			
				if (MainWindow.OpenedProject()!= null && project.Equals(MainWindow.OpenedProject())) {
				
					MessageDialog md = new MessageDialog(this,DialogFlags.Modal,MessageType.Warning,ButtonsType.Ok,
					                                     Catalog.GetString("This Project is actually in use.\n Close it first to allow its removal from the database"));
					md.Run();				
					md.Destroy();
				}
				else{
					this.filedescriptionwidget3.Sensitive = true;
					this.filedescriptionwidget3.SetProject(project);
					this.saveButton.Sensitive = true;
					this.deleteButton.Sensitive = true;
				}
			

		}

		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			this.Destroy();
		}

		
	}
}
