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
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using Mono.Unix;

namespace LongoMatch.Gui.Dialog
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class ProjectsManager : Gtk.Dialog
	{

		Project openedProject;
		List<ProjectDescription> selectedProjects;
		IDatabase DB;

		public ProjectsManager(Project openedProject, IDatabase DB, ITemplatesService ts)
		{
			this.Build();
			this.openedProject = openedProject;
			this.projectdetails.Use = ProjectType.EditProject;
			this.DB = DB;
			Fill();
			projectdetails.Edited = false;
			projectdetails.TemplatesService = ts; 
		}

		private void Fill() {
			projectlistwidget1.Fill(DB.GetAllProjects());
			projectlistwidget1.ClearSearch();
			projectlistwidget1.SelectionMode = SelectionMode.Multiple;
			Clear();
		}

		private void Clear() {
			projectdetails.Clear();
			projectdetails.Sensitive = false;
			saveButton.Sensitive = false;
			deleteButton.Sensitive = false;
			exportbutton.Sensitive = false;
		}

		private void PromptToSaveEditedProject() {
			MessageDialog md = new MessageDialog((Window)this.Toplevel,DialogFlags.Modal,
			                                     MessageType.Question, ButtonsType.YesNo,
			                                     Catalog.GetString("The Project has been edited, do you want to save the changes?"));
			if(md.Run() == (int)ResponseType.Yes) {
				SaveProject();
				projectdetails.Edited=false;
			}
			md.Destroy();
		}

		private void SaveProject() {
			Project project = projectdetails.GetProject();

			if(project == null)
				return;

			DB.UpdateProject(project);
			saveButton.Sensitive = false;
			projectlistwidget1.QueueDraw();
		}


		protected virtual void OnDeleteButtonPressed(object sender, System.EventArgs e)
		{
			List<ProjectDescription> deletedProjects = new List<ProjectDescription>();

			if(selectedProjects == null)
				return;

			foreach(ProjectDescription selectedProject in selectedProjects) {
				if(openedProject != null &&
				                selectedProject.File.FilePath == openedProject.Description.File.FilePath) {
					MessagePopup.PopupMessage(this, MessageType.Warning,
					                          Catalog.GetString("This Project is actually in use.")+"\n"+
					                          Catalog.GetString("Close it first to allow its removal from the database"));
					continue;
				}
				MessageDialog md = new MessageDialog(this,DialogFlags.Modal,
				                                     MessageType.Question,
				                                     ButtonsType.YesNo,
				                                     Catalog.GetString("Do you really want to delete:")+
				                                     "\n"+selectedProject.Title);
				if(md.Run()== (int)ResponseType.Yes) {
					DB.RemoveProject(selectedProject.UUID);
					deletedProjects.Add(selectedProject);
				}
				md.Destroy();
			}
			projectlistwidget1.RemoveProjects(deletedProjects);
			Clear();
		}

		protected virtual void OnSaveButtonPressed(object sender, System.EventArgs e)
		{
			SaveProject();
			projectdetails.Edited=false;
			Fill();
		}


		protected virtual void OnButtonOkClicked(object sender, System.EventArgs e)
		{
			if(projectdetails.Edited) {
				PromptToSaveEditedProject();
			}
			this.Destroy();
		}

		protected virtual void OnProjectlistwidget1ProjectsSelected(List<ProjectDescription> projects)
		{
			ProjectDescription project;

			/* prompt tp save the opened project if has changes */
			if(projectdetails.Edited) {
				PromptToSaveEditedProject();
			}

			selectedProjects = projects;

			/* if no projects are selected clear everything */
			if(projects.Count == 0) {
				Clear();
				return;
				/* if more than one project is selected clear everything but keep
				 * the delete button and the export button sensitives */
			} else if(projects.Count > 1) {
				Clear();
				deleteButton.Sensitive = true;
				exportbutton.Sensitive = true;
				return;
			}

			/* if only one project is selected try to load it in the editor */
			project = projects[0];

			if(openedProject != null &&
			                project.File.FilePath == openedProject.Description.File.FilePath) {
				MessagePopup.PopupMessage(this, MessageType.Warning,
				                          Catalog.GetString("The Project you are trying to load is actually in use.")+"\n" +Catalog.GetString("Close it first to edit it"));
				Clear();
			}
			else {
				projectdetails.Sensitive = true;
				projectdetails.SetProject(DB.GetProject(project.UUID));
				saveButton.Sensitive = false;
				deleteButton.Sensitive = true;
				exportbutton.Sensitive = true;
			}
		}

		protected virtual void OnProjectdetailsEditedEvent(object sender, System.EventArgs e)
		{
			saveButton.Sensitive = true;
		}

		protected virtual void OnExportbuttonClicked(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Save Project"),
			                (Gtk.Window)Toplevel,
			                FileChooserAction.Save,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(Config.HomeDir());
			FileFilter filter = new FileFilter();
			filter.Name = Constants.PROJECT_NAME;
			filter.AddPattern("*.lpr");

			fChooser.AddFilter(filter);
			if(fChooser.Run() == (int)ResponseType.Accept) {
				Project.Export(projectdetails.GetProject(), fChooser.Filename);
			}
			fChooser.Destroy();
		}
	}
}
