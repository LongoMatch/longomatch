//
//  Copyright (C) 2014 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using LongoMatch.Store;
using System.Collections.Generic;
using LongoMatch.Interfaces;
using LongoMatch.Store.Templates;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Handlers;
using Gtk;
using LongoMatch.Common;
using LongoMatch.Gui.Helpers;
using LongoMatch.Interfaces.Multimedia;
using Mono.Unix;
using LongoMatch.Video;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectsManagerPanel : Gtk.Bin, IPanel
	{
		public event BackEventHandle BackEvent;
		
		Project openedProject, loadedProject;
		List<ProjectDescription> selectedProjects;
		IDatabase DB;
		IGUIToolkit gkit;
		
		public ProjectsManagerPanel (Project openedProject)
		{
			this.openedProject = openedProject;
			this.DB = Config.DatabaseManager.ActiveDB;
			this.gkit = Config.GUIToolkit;
			this.Build ();
			notebook1.ShowTabs = false;
			notebook1.ShowBorder = false;
			projectlistwidget1.Fill (DB.GetAllProjects());
			projectlistwidget1.ProjectsSelected += HandleProjectsSelected;
			projectlistwidget1.SelectionMode = SelectionMode.Multiple;
			seasonentry.Changed += HandleChanged;
			competitionentry.Changed += HandleChanged;
			localSpinButton.ValueChanged += HandleChanged;
			visitorSpinButton.ValueChanged += HandleChanged;
			filebutton.Clicked += HandleFileClicked;
			backbutton.Clicked += HandleBackClicked;
			savebutton.Clicked += HandleSaveClicked;
			exportbutton.Clicked += HandleExportClicked;
			deletebutton.Clicked += HandleDeleteClicked;
			templatebutton.Clicked += HandleTeamTemplateClicked;
			calendarbutton.Clicked += HandleCalendarClicked;
			notebook1.Page = 0;
		}

		void LoadProject (Project project) {
			ProjectDescription pd = project.Description;
			MediaFile f = pd.File;
			TeamTemplate lt = project.LocalTeamTemplate;
			TeamTemplate vt = project.VisitorTeamTemplate;
			
			seasonentry.Text = pd.Season;
			competitionentry.Text = pd.Competition;
			localSpinButton.Value = pd.LocalGoals;
			visitorSpinButton.Value = pd.VisitorGoals;
			datelabel.Text = pd.MatchDate.ToShortDateString ();
			templatelabel.Text = project.Categories.Name;
			
			fileimage.Pixbuf = f.Preview.Value;
			medialabel.Markup = f.Description;
			
			homelabel.Text = lt.TeamName;
			awaylabel.Text = vt.TeamName;
			if (lt.Shield != null) {
				homeimage.Pixbuf = lt.Shield.Value;
			} else {
				homeimage.Pixbuf = Gdk.Pixbuf.LoadFromResource ("logo.svg");
			}
			if (vt.Shield != null) {
				awayimage.Pixbuf = vt.Shield.Value;
			} else {
				awayimage.Pixbuf = Gdk.Pixbuf.LoadFromResource ("logo.svg");
			}
			
			loadedProject = project;
			descbox.Visible = true;
		}
		
		void HandleFileClicked (object sender, EventArgs e)
		{
			MediaFile file= Utils.Open.OpenFile (this);
			if (file != null) {
				loadedProject.Description.File = file;
				fileimage.Pixbuf = file.Preview.Value;
				medialabel.Markup = file.Description;
			}
		}

		void HandleBackClicked (object sender, EventArgs e)
		{
			if (notebook1.Page == 0) {
				if (BackEvent != null) {
					BackEvent ();
				}
			} else {
				notebook1.Page --;
			}
		}

		void HandleChanged (object sender, EventArgs e)
		{
			if (loadedProject == null)
				return;
				
			if (sender == competitionentry) {
				loadedProject.Description.Competition = (sender as Entry).Text;
			} else if (sender == seasonentry) {
				loadedProject.Description.Season = (sender as Entry).Text;
			} else if (sender == localSpinButton) {
				loadedProject.Description.LocalGoals = (int) (sender as SpinButton).Value;
			} else if (sender == visitorSpinButton) {
				loadedProject.Description.VisitorGoals = (int) (sender as SpinButton).Value;
			}
			
		}

		void HandleProjectsSelected (List<ProjectDescription> projects)
		{
			rbox.Visible = true;
			savebutton.Sensitive = projects.Count == 1;
			exportbutton.Sensitive = projects.Count == 1;
			deletebutton.Sensitive = projects.Count != 0;
			descbox.Sensitive = projects.Count == 1;
			
			selectedProjects = projects;
			if (projects.Count == 1) {
				LoadProject (DB.GetProject (projects[0].ID));
			}
		}
		
		void HandleSaveClicked (object sender, EventArgs e)
		{
			if (loadedProject != null) {
				DB.UpdateProject (loadedProject);
			}
		}
		
		void HandleExportClicked (object sender, EventArgs e)
		{
			if (loadedProject != null) {
				string filename = gkit.SaveFile (
					Catalog.GetString("Export project"),
					null, Config.HomeDir, Constants.PROJECT_NAME,
					new string[] {Constants.PROJECT_EXT});
				Serializer.Save(loadedProject, filename);
			}			
		}

		void HandleCalendarClicked (object sender, EventArgs e)
		{
			DateTime date;
			
			date = gkit.SelectDate (loadedProject.Description.MatchDate, this);
			loadedProject.Description.MatchDate = date;
			datelabel.Text = date.ToShortDateString ();
		}
		
		void HandleTeamTemplateClicked (object sender, EventArgs e)
		{
		}

		void HandleDeleteClicked (object sender, EventArgs e)
		{
			List<ProjectDescription> deletedProjects;

			if (selectedProjects == null)
				return;
				
			deletedProjects = new List<ProjectDescription>();
			foreach (ProjectDescription selectedProject in selectedProjects) {
				if(openedProject == loadedProject) {
					MessagesHelpers.WarningMessage (this,
					                                Catalog.GetString("This Project is actually in use.")+"\n"+
					                                Catalog.GetString("Close it first to allow its removal from the database"));
					continue;
				}
				string msg = Catalog.GetString ("Do you really want to delete:") + "\n" + selectedProject.Title;
				if (MessagesHelpers.QuestionMessage (this, msg)) {
					DB.RemoveProject (selectedProject.ID);
					deletedProjects.Add (selectedProject);
				}
			}
			projectlistwidget1.RemoveProjects(deletedProjects);
		}
	}
}

