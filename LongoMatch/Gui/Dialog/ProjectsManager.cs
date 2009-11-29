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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using LongoMatch.Common;
using LongoMatch.DB;
using LongoMatch.Gui.Component;

namespace LongoMatch.Gui.Dialog
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class ProjectsManager : Gtk.Dialog
	{

		private string originalFilePath;

		public ProjectsManager()
		{
			this.Build();
			this.Fill();
			this.projectdetails.Use = ProjectType.EditProject;
			projectdetails.Edited = false;
		}

		public void Fill() {
			List<ProjectDescription> projectsList = MainClass.DB.GetAllProjects();
			projectlistwidget1.Fill(projectsList);
			projectlistwidget1.ClearSearch();
			projectdetails.Clear();
			projectdetails.Sensitive = false;
			saveButton.Sensitive = false;
			deleteButton.Sensitive = false;
			originalFilePath=null;
		}
		
		private void PromptToSaveEditedProject(){			
			MessageDialog md = new MessageDialog((Window)this.Toplevel,DialogFlags.Modal,
			                                     MessageType.Question, ButtonsType.YesNo,
			                                     Catalog.GetString("The Project has been edited, do you want to save the changes?"));
			if (md.Run() == (int)ResponseType.Yes) {
				SaveProject();
				projectdetails.Edited=false;
			}
			md.Destroy();
		}

		private void SaveProject() {
			Project project = projectdetails.GetProject();

			if (project == null)
				return;

			if (project.File.FilePath == originalFilePath) {
				MainClass.DB.UpdateProject(project);
				saveButton.Sensitive = false;
			}
			else {
				try {
					MainClass.DB.UpdateProject(project,originalFilePath);
					saveButton.Sensitive = false;
				}
				catch {
					MessagePopup.PopupMessage(this, MessageType.Warning,
					                          Catalog.GetString("A Project is already using this file."));
				}
			}
			projectlistwidget1.QueueDraw();
		}


		protected virtual void OnDeleteButtonPressed(object sender, System.EventArgs e)
		{
			ProjectDescription selectedProject = projectlistwidget1.GetSelection();
			if (selectedProject != null) {
				if (MainWindow.OpenedProject() != null &&selectedProject.File == MainWindow.OpenedProject().File.FilePath) {
					MessagePopup.PopupMessage(this, MessageType.Warning,
					                          Catalog.GetString("This Project is actually in use.")+"\n"+
					                          Catalog.GetString("Close it first to allow its removal from the database"));
				}
				else {
					MessageDialog md = new MessageDialog(this,DialogFlags.Modal,
					                                     MessageType.Question,
					                                     ButtonsType.YesNo,
					                                     Catalog.GetString("Do yo really want to delete:")+
					                                     "\n"+selectedProject.File);
					if (md.Run()== (int)ResponseType.Yes) {
						projectdetails.Clear();
						MainClass.DB.RemoveProject(selectedProject.File);
						Fill();
					}
					md.Destroy();
				}
			}
		}

		protected virtual void OnSaveButtonPressed(object sender, System.EventArgs e)
		{
			SaveProject();
			projectdetails.Edited=false;
			Fill();
		}


		protected virtual void OnButtonOkClicked(object sender, System.EventArgs e)
		{
			if (projectdetails.Edited) {
				PromptToSaveEditedProject();
			}
			this.Destroy();
		}

		protected virtual void OnProjectlistwidget1ProjectSelectedEvent(ProjectDescription project)
		{
			if (projectdetails.Edited) {
				PromptToSaveEditedProject();
			}
			
			if (MainWindow.OpenedProject() != null && project.File == MainWindow.OpenedProject().File.FilePath) {

				MessagePopup.PopupMessage(this, MessageType.Warning,
				                          Catalog.GetString("The Project you are trying to load is actually in use.")+"\n" +Catalog.GetString("Close it first to edit it"));
				projectdetails.Clear();
				projectdetails.Sensitive = false;
				saveButton.Sensitive = false;
				deleteButton.Sensitive = false;
			}
			else {
				projectdetails.Sensitive = true;
				projectdetails.SetProject(MainClass.DB.GetProject(project.File));
				originalFilePath = project.File;
				saveButton.Sensitive = false;
				deleteButton.Sensitive = true;
			}
		}

		protected virtual void OnProjectdetailsEditedEvent(object sender, System.EventArgs e)
		{
			saveButton.Sensitive = true;
		}
	}
}
