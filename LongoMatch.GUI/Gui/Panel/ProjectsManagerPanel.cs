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
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Helpers;
using LongoMatch.Core;
using Pango;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProjectsManagerPanel : Gtk.Bin, IPanel
	{
		public event BackEventHandle BackEvent;

		Project openedProject, loadedProject;
		List<Project> selectedProjects;
		List<VideoFileInfo> videoFileInfos;
		IStorage DB;
		IGUIToolkit gkit;
		bool edited;

		public ProjectsManagerPanel (Project openedProject)
		{
			this.openedProject = openedProject;
			this.DB = Config.DatabaseManager.ActiveDB;
			this.gkit = Config.GUIToolkit;
			this.Build ();

			this.videoFileInfos = new List<VideoFileInfo> ();

			savebuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-save", 34);
			exportbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-export", 34);
			resyncbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-project-resync", 34);
			deletebuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-delete", 34);
			openbuttonimage.Pixbuf = Misc.LoadIcon ("longomatch-open", 34);

			// Force tooltips to be translatable as there seems to be a bug in stetic 
			// code generation for translatable tooltips.
			savebutton.TooltipMarkup = Catalog.GetString ("Save");
			exportbutton.TooltipMarkup = Catalog.GetString ("Export");
			openbutton.TooltipMarkup = Catalog.GetString ("Open");
			deletebutton.TooltipMarkup = Catalog.GetString ("Delete");

			notebook1.ShowTabs = false;
			notebook1.ShowBorder = false;
			projectlistwidget1.SelectionMode = SelectionMode.Multiple;
			projectlistwidget1.ProjectsSelected += HandleProjectsSelected;
			projectlistwidget1.ProjectSelected += HandleProjectSelected;
			projectlistwidget1.Fill (DB.RetrieveAll<Project> ().ToList ());

			seasonentry.Changed += HandleChanged;
			competitionentry.Changed += HandleChanged;
			savebutton.Clicked += HandleSaveClicked;
			exportbutton.Clicked += HandleExportClicked;
			resyncbutton.Clicked += HandleResyncClicked;
			deletebutton.Clicked += HandleDeleteClicked;
			openbutton.Clicked += HandleOpenClicked;
			datepicker.ValueChanged += HandleDateChanged;
			desctextview.Buffer.Changed += HandleChanged;

			notebook1.Page = 0;
			panelheader1.Title = Catalog.GetString ("PROJECTS MANAGER");
			panelheader1.ApplyVisible = false;
			panelheader1.BackClicked += HandleBackClicked;
			
			projectlistwidget1.ViewMode = ProjectListViewMode.List;

			// Only visible when multi camera is supported. Indeed periods can be edited in the timeline of the project.
			resyncbutton.Visible = Config.SupportsMultiCamera;

			SetStyle ();
		}

		public void OnLoaded ()
		{

		}

		public void OnUnloaded ()
		{

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

		void SaveLoadedProject (bool force)
		{
			if (loadedProject != null) {
				bool save = edited;

				if (edited && !force) {
					string msg = Catalog.GetString ("Do you want to save the current project?");
					if (!Config.GUIToolkit.QuestionMessage (msg, null, this).Result) {
						save = false;
					}
				}
				if (save) {
					try {
						IBusyDialog busy = Config.GUIToolkit.BusyDialog (Catalog.GetString ("Saving project..."), null);
						busy.ShowSync (() => DB.Store<Project> (loadedProject));
						projectlistwidget1.UpdateProject (loadedProject);
						edited = false;
					} catch (Exception ex) {
						Log.Exception (ex);
						Config.GUIToolkit.ErrorMessage (Catalog.GetString ("Error saving project:") + "\n" + ex.Message);
						return;
					}
				}
			}
		}

		void LoadProject (Project project)
		{
			ProjectDescription pd = project.Description;
			
			loadedProject = null;
			gamedescriptionheader1.ProjectDescription = pd;
			seasonentry.Text = pd.Season;
			competitionentry.Text = pd.Competition;
			datepicker.Date = pd.MatchDate;
			templatelabel.Text = pd.DashboardName;
			desctextview.Buffer.Clear ();
			desctextview.Buffer.InsertAtCursor (pd.Description ?? "");
			loadedProject = project;

			foreach (VideoFileInfo vfi in videoFileInfos) {
				videofileinfo_vbox.Remove (vfi);
			}
			videoFileInfos.Clear ();

			resyncbutton.Sensitive = project.Description.FileSet.Count > 1;

			int max = project.Description.FileSet.Count;
			// Cap to one media file for non multi camera version
			if (!Config.SupportsMultiCamera) {
				max = Math.Min (max, 1);
			}

			for (int i = 0; i < max; i++) {
				MediaFile mf = project.Description.FileSet [i];
				VideoFileInfo vfi = new VideoFileInfo ();

				vfi.SetMediaFileSet (project.Description.FileSet, mf);
				vfi.Changed += HandleChanged;

				vfi.ShowAll ();

				videoFileInfos.Add (vfi);

				videofileinfo_vbox.PackStart (vfi, true, true, 0);
			}

			projectbox.Visible = true;
			edited = false;
		}

		void HandleBackClicked (object sender, EventArgs e)
		{
			if (notebook1.Page == 0) {
				if (BackEvent != null) {
					BackEvent ();
				}
			} else {
				projectperiods1.Pause ();
				/* FIXME: we don't support adding new cameras, so there is nothing
				 * to fix or update */
				//projectperiods1.SaveChanges (false);

				// We need to reload project details
				LoadProject (loadedProject);
				// And remember that the project has changed
				edited = true;

				notebook1.Page--;
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
					desctextview.Buffer.GetText (desctextview.Buffer.StartIter,
					desctextview.Buffer.EndIter, true);
			}
			edited = true;
		}

		void HandleProjectSelected (Project project)
		{
			SaveLoadedProject (false);
			if (project != null) {
				Config.EventsBroker.EmitOpenProjectID (project.ID, project);
			}
		}

		void HandleProjectsSelected (List<Project> projects)
		{
			SaveLoadedProject (false);
			rbox.Visible = true;
			savebutton.Sensitive = projects.Count == 1;
			exportbutton.Sensitive = projects.Count == 1;
			openbutton.Sensitive = projects.Count == 1;
			deletebutton.Sensitive = projects.Count != 0;
			projectbox.Sensitive = projects.Count == 1;
			resyncbutton.Sensitive = projects.Count == 1;
			
			selectedProjects = projects;
			if (projects.Count == 1) {
				try {
					LoadProject (projects [0]);
				} catch (Exception ex) {
					Log.Exception (ex);
					Config.GUIToolkit.ErrorMessage (ex.Message, this);
				}
			}
		}

		void HandleResyncClicked (object sender, EventArgs e)
		{
			notebook1.Page = 1;
			// Load data in the project periods widget.
			projectperiods1.Project = loadedProject;
		}

		void HandleSaveClicked (object sender, EventArgs e)
		{
			SaveLoadedProject (true);
		}

		void HandleExportClicked (object sender, EventArgs e)
		{
			if (loadedProject != null) {
				string filename = gkit.SaveFile (
					                  Catalog.GetString ("Export project"),
					                  Utils.SanitizePath (loadedProject.Description.Title + Constants.PROJECT_EXT),
					                  Config.HomeDir, Constants.PROJECT_NAME,
					                  new string[] { Constants.PROJECT_EXT });
				if (filename != null) {
					filename = System.IO.Path.ChangeExtension (filename, Constants.PROJECT_EXT);
					Serializer.Instance.Save (loadedProject, filename);
				}
			}
		}

		void HandleDateChanged (object sender, EventArgs e)
		{
			if (loadedProject == null)
				return;

			loadedProject.Description.MatchDate = datepicker.Date;
			edited = true;
		}

		void HandleDeleteClicked (object sender, EventArgs e)
		{
			List<Project> deletedProjects;

			if (selectedProjects == null)
				return;
				
			deletedProjects = new List<Project> ();
			foreach (Project selectedProject in selectedProjects) {
				if (openedProject != null && openedProject.ID == selectedProject.ID) {
					MessagesHelpers.WarningMessage (this,
						Catalog.GetString ("This Project is actually in use.") + "\n" +
						Catalog.GetString ("Close it first to allow its removal from the database"));
					continue;
				}
				string msg = Catalog.GetString ("Do you really want to delete:") + "\n" +
				             selectedProject.Description.Title;
				if (MessagesHelpers.QuestionMessage (this, msg)) {
					// Unload first
					if (loadedProject != null && loadedProject.ID == selectedProject.ID) {
						loadedProject = null;
					}
					IBusyDialog busy = Config.GUIToolkit.BusyDialog (Catalog.GetString ("Deleting project..."), null);
					busy.ShowSync (() => {
						try {
							DB.Delete<Project> (selectedProject);
						} catch (StorageException ex) {
							Config.GUIToolkit.ErrorMessage (ex.Message);
						}
					});
					deletedProjects.Add (selectedProject);
				}
			}
			projectlistwidget1.RemoveProjects (deletedProjects);

			// In the case where there are no projects left we need to clear the project desc widget
			if (DB.Count<Project> () == 0) {
				rbox.Visible = false;
			}
		}

		void HandleOpenClicked (object sender, EventArgs e)
		{
			if (loadedProject != null) {
				Config.EventsBroker.EmitOpenProjectID (loadedProject.ID, loadedProject);
			}
		}
	}
}

