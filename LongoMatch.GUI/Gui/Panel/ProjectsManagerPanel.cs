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
using LongoMatch.Core.Store;
using System.Collections.Generic;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Handlers;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Gui.Helpers;
using LongoMatch.Core.Interfaces.Multimedia;
using Mono.Unix;
using LongoMatch.Video;
using LongoMatch.Gui.Component;

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

			savebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-project-save", 34, IconLookupFlags.ForceSvg);
			exportbuttonimage.Pixbuf  = Helpers.Misc.LoadIcon ("longomatch-project-export", 34, IconLookupFlags.ForceSvg);
			deletebuttonimage.Pixbuf  = Helpers.Misc.LoadIcon ("longomatch-project-delete", 34, IconLookupFlags.ForceSvg);

			notebook1.ShowTabs = false;
			notebook1.ShowBorder = false;
			projectlistwidget1.Fill (DB.GetAllProjects());
			projectlistwidget1.ProjectsSelected += HandleProjectsSelected;
			projectlistwidget1.SelectionMode = SelectionMode.Multiple;

			seasonentry.Changed += HandleChanged;
			competitionentry.Changed += HandleChanged;
			savebutton.Clicked += HandleSaveClicked;
			exportbutton.Clicked += HandleExportClicked;
			deletebutton.Clicked += HandleDeleteClicked;
			datepicker.ValueChanged += HandleDateChanged;
			mediafilechooser1.ChangedEvent += HandleFileChanged;
			mediafilechooser2.ChangedEvent += HandleFileChanged;
			mediafilechooser3.ChangedEvent += HandleFileChanged;
			mediafilechooser4.ChangedEvent += HandleFileChanged;

			notebook1.Page = 0;
			panelheader1.Title = "PROJECTS MANAGER";
			panelheader1.ApplyVisible = false;
			panelheader1.BackClicked += HandleBackClicked;
		}

		void LoadProject (Project project) {
			ProjectDescription pd = project.Description;
			MediaFile f = pd.FileSet.GetAngle (MediaFileAngle.Angle1);
			TeamTemplate lt = project.LocalTeamTemplate;
			TeamTemplate vt = project.VisitorTeamTemplate;
			
			seasonentry.Text = pd.Season;
			competitionentry.Text = pd.Competition;
			scorelabel.Text = String.Format ("{0} - {1}", pd.LocalGoals, pd.VisitorGoals);
			datepicker.Date = pd.MatchDate;
			templatelabel.Text = project.Dashboard.Name;
	
			if (f.Preview != null) {
				fileimage1.Pixbuf = f.Preview.Value;
			} else {
				fileimage1.Pixbuf = Stetic.IconLoader.LoadIcon (this, Gtk.Stock.Harddisk,
				                                               IconSize.Dialog);
			}
			medialabel1.Markup = f.Description;
			
			homelabel.Text = lt.TeamName;
			awaylabel.Text = vt.TeamName;
			if (lt.Shield != null) {
				homeimage.Pixbuf = lt.Shield.Value;
			} else {
				homeimage.Pixbuf = Helpers.Misc.LoadIcon (Constants.LOGO_ICON,
				                                          Constants.MAX_SHIELD_ICON_SIZE);
			}
			if (vt.Shield != null) {
				awayimage.Pixbuf = vt.Shield.Value;
			} else {
				awayimage.Pixbuf = Helpers.Misc.LoadIcon (Constants.LOGO_ICON,
				                                          Constants.MAX_SHIELD_ICON_SIZE);
			}
			
			loadedProject = project;
			
			UpdateFile (mediafilechooser1, project.Description.FileSet.GetAngle (MediaFileAngle.Angle1),
			            MediaFileAngle.Angle1, fileimage1, medialabel1);
			UpdateFile (mediafilechooser2, project.Description.FileSet.GetAngle (MediaFileAngle.Angle2),
			            MediaFileAngle.Angle2, fileimage2, medialabel2);
			UpdateFile (mediafilechooser3, project.Description.FileSet.GetAngle (MediaFileAngle.Angle3),
			            MediaFileAngle.Angle3, fileimage3, medialabel3);
			UpdateFile (mediafilechooser4, project.Description.FileSet.GetAngle (MediaFileAngle.Angle4),
			            MediaFileAngle.Angle4, fileimage4, medialabel4);
			
			descbox.Visible = true;
		}
		
		void UpdateFile (MediaFileChooser mediafilechooser, MediaFile file, MediaFileAngle view,
		               Gtk.Image image, Label label)
		{
			mediafilechooser.MediaFile = file;
			if (file != null) {
				loadedProject.Description.FileSet.SetAngle (view, file);
				image.Pixbuf = file.Preview.Value;
				label.Markup = file.Description;
			} else {
				loadedProject.Description.FileSet.SetAngle (view, null);
				image.Pixbuf = null;
				label.Markup = null;
			}
		}

		void HandleFileChanged (object sender, EventArgs e)
		{
			if (loadedProject == null) {
				return;
			}

			if (sender == mediafilechooser1) {
				UpdateFile (mediafilechooser1, mediafilechooser1.MediaFile,
				            MediaFileAngle.Angle1, fileimage1, medialabel1);
			} else if (sender == mediafilechooser2) {
				UpdateFile (mediafilechooser2, mediafilechooser2.MediaFile,
				            MediaFileAngle.Angle2, fileimage2, medialabel2);
			} else if (sender == mediafilechooser3) {
				UpdateFile (mediafilechooser3, mediafilechooser3.MediaFile,
				            MediaFileAngle.Angle3, fileimage3, medialabel3);
			} else if (sender == mediafilechooser4) {
				UpdateFile (mediafilechooser4, mediafilechooser4.MediaFile,
				            MediaFileAngle.Angle4, fileimage4, medialabel4);
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
				try {
					LoadProject (DB.GetProject (projects [0].ID));
				} catch (Exception ex) {
					Log.Exception (ex);
					Config.GUIToolkit.ErrorMessage (ex.Message, this);
				}
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
					Catalog.GetString ("Export project"),
					loadedProject.Description.Title + Constants.PROJECT_EXT,
					Config.HomeDir, Constants.PROJECT_NAME,
					new string[] { Constants.PROJECT_EXT });
				if (filename != null) {
					filename = System.IO.Path.ChangeExtension (filename, Constants.PROJECT_EXT);
					Serializer.Save(loadedProject, filename);
				}
			}
		}

		void HandleDateChanged (object sender, EventArgs e)
		{
			if (loadedProject == null)
				return;

			loadedProject.Description.MatchDate = datepicker.Date;
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

