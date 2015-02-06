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
using System.Collections.Generic;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Helpers;
using Mono.Unix;
using Pango;
using Misc = LongoMatch.Gui.Helpers.Misc;

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

			savebuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-project-save", 34);
			exportbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-project-export", 34);
			deletebuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-project-delete", 34);
			openbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-project-open", 34);

			notebook1.ShowTabs = false;
			notebook1.ShowBorder = false;
			projectlistwidget1.SelectionMode = SelectionMode.Multiple;
			projectlistwidget1.ProjectsSelected += HandleProjectsSelected;
			projectlistwidget1.Fill (DB.GetAllProjects ());

			seasonentry.Changed += HandleChanged;
			competitionentry.Changed += HandleChanged;
			savebutton.Clicked += HandleSaveClicked;
			exportbutton.Clicked += HandleExportClicked;
			deletebutton.Clicked += HandleDeleteClicked;
			openbutton.Clicked += HandleOpenClicked;
			datepicker.ValueChanged += HandleDateChanged;
			desctextview.Buffer.Changed += HandleChanged;

			notebook1.Page = 0;
			panelheader1.Title = Catalog.GetString ("PROJECTS MANAGER");
			panelheader1.ApplyVisible = false;
			panelheader1.BackClicked += HandleBackClicked;
			
			projectlistwidget1.ShowList = true;

			SetStyle ();
		}

		void SetStyle ()
		{
			FontDescription desc = FontDescription.FromString (Config.Style.Font + " 18");
			infoeventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			infolabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			infolabel.ModifyFont (desc);
			videoseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			videoslabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			videoslabel.ModifyFont (desc);
		}

		void LoadProject (Project project)
		{
			ProjectDescription pd = project.Description;
			MediaFile f = pd.FileSet.GetAngle (MediaFileAngle.Angle1);
			
			loadedProject = null;
			gamedescriptionheader1.ProjectDescription = pd;
			seasonentry.Text = pd.Season;
			competitionentry.Text = pd.Competition;
			datepicker.Date = pd.MatchDate;
			templatelabel.Text = project.Dashboard.Name;
			desctextview.Buffer.Clear ();
			desctextview.Buffer.InsertAtCursor (project.Description.Description ?? "");
			loadedProject = project;

			videofileinfo1.SetMediaFile (project.Description.FileSet, MediaFileAngle.Angle1);
			videofileinfo2.SetMediaFile (project.Description.FileSet, MediaFileAngle.Angle2);
			videofileinfo3.SetMediaFile (project.Description.FileSet, MediaFileAngle.Angle3);
			videofileinfo4.SetMediaFile (project.Description.FileSet, MediaFileAngle.Angle4);
			projectbox.Visible = true;
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
			} else if (sender == desctextview.Buffer) {
				loadedProject.Description.Description =
					desctextview.Buffer.GetText(desctextview.Buffer.StartIter,
					                            desctextview.Buffer.EndIter,true);
			}
		}

		void HandleProjectsSelected (List<ProjectDescription> projects)
		{
			rbox.Visible = true;
			savebutton.Sensitive = projects.Count == 1;
			exportbutton.Sensitive = projects.Count == 1;
			openbutton.Sensitive = projects.Count == 1;
			deletebutton.Sensitive = projects.Count != 0;
			projectbox.Sensitive = projects.Count == 1;
			
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
				projectlistwidget1.UpdateProject (loadedProject.Description);
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
					Serializer.Save (loadedProject, filename);
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
				
			deletedProjects = new List<ProjectDescription> ();
			foreach (ProjectDescription selectedProject in selectedProjects) {
				if (openedProject != null && openedProject.ID == selectedProject.ID) {
					MessagesHelpers.WarningMessage (this,
					                                Catalog.GetString ("This Project is actually in use.") + "\n" +
						Catalog.GetString ("Close it first to allow its removal from the database"));
					continue;
				}
				string msg = Catalog.GetString ("Do you really want to delete:") + "\n" + selectedProject.Title;
				if (MessagesHelpers.QuestionMessage (this, msg)) {
					DB.RemoveProject (selectedProject.ID);
					deletedProjects.Add (selectedProject);
				}
			}
			projectlistwidget1.RemoveProjects (deletedProjects);
		}
		
		void HandleOpenClicked (object sender, EventArgs e)
		{
			if (loadedProject != null) {
				Config.EventsBroker.EmitOpenProjectID (loadedProject.ID);
			}
		}
	}
}

