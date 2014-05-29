// MainWindow.cs
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
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using System.IO;
using Gdk;
using GLib;
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Gui.Dialog;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Video.Common;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Helpers;
using LongoMatch.Gui.Panel;
using LongoMatch.Interfaces.Multimedia;


namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class MainWindow : Gtk.Window, IMainController, IProjectOptionsController
	{
		/* IMainController */
		public event NewProjectHandler NewProjectEvent;
		public event OpenNewProjectHandler OpenNewProjectEvent;
		public event OpenProjectHandler OpenProjectEvent;
		public event OpenProjectIDHandler OpenProjectIDEvent;
		public event ImportProjectHandler ImportProjectEvent;
		public event ExportProjectHandler ExportProjectEvent;
		public event QuitApplicationHandler QuitApplicationEvent;
		
		public event ManageJobsHandler ManageJobsEvent; 
		public event ManageTeamsHandler ManageTeamsEvent;
		public event ManageCategoriesHandler ManageCategoriesEvent;
		public event ManageProjects ManageProjectsEvent;
		public event ManageDatabases ManageDatabasesEvent;
		public event EditPreferences EditPreferencesEvent;
		public event ConvertVideoFilesHandler ConvertVideoFilesEvent;
		
		/* IProjectOptionsController */
		public event SaveProjectHandler SaveProjectEvent;
		public event CloseOpenendProjectHandler CloseOpenedProjectEvent;
		public event ShowProjectStats ShowProjectStatsEvent;
		public event ShowFullScreenHandler ShowFullScreenEvent;
		public event PlaylistVisibiltyHandler PlaylistVisibilityEvent;
		public event AnalysisWidgetsVisibilityHandler AnalysisWidgetsVisibilityEvent;
		public event AnalysisModeChangedHandler AnalysisModeChangedEvent;
		public event TagSubcategoriesChangedHandler TagSubcategoriesChangedEvent;
		
		IGUIToolkit guiToolKit;
		IAnalysisWindow analysisWindow;
		Project openedProject;
		ProjectType projectType;

		#region Constructors
		public MainWindow(IGUIToolkit guiToolkit) :
		base(Constants.SOFTWARE_NAME)
		{
			Screen screen;
			
			this.Build();
			this.guiToolKit = guiToolkit;
			welcomepanel1.Bind (this);
			
			Title = Constants.SOFTWARE_NAME;
			TagSubcategoriesAction.Active = !Config.FastTagging;
			projectType = ProjectType.None;
			
			ConnectSignals();
			ConnectMenuSignals();
			
			if (!Config.UseGameUnits)
				GameUnitsViewAction.Visible = false;
			
			MenuItem parent = ImportProjectActionMenu;
			parent.Submenu = new Menu();
			AddImportEntry(Catalog.GetString("Import file project"), "ImportFileProject",
			               Constants.PROJECT_NAME + " (" + Constants.PROJECT_EXT + ")",
			               "*" + Constants.PROJECT_EXT, Project.Import,
			               false);
			screen = Display.Default.DefaultScreen;
			this.Resize(screen.Width * 80 / 100, screen.Height * 80 / 100);
			analysisWindow = new AnalysisComponent();
		}

		#endregion
		
		#region Plubic Methods
		public IRenderingStateBar RenderingStateBar{
			get {
				return renderingstatebar1;
			}
		}
		
		public void SetPanel (Widget panel) {
			if (panel == null) {
				ResetGUI ();
			} else {
				foreach (Widget widget in centralbox.AllChildren) {
					if (widget != welcomepanel1) {
						centralbox.Remove (widget);
					}
				}
				panel.Show();
				if (panel is IPanel) {
					(panel as IPanel).BackEvent += ResetGUI;
				}
				centralbox.PackStart (panel, true, true, 0);
				welcomepanel1.Hide ();
			}
		}
		
		public void AddExportEntry (string name, string shortName, Action<Project, IGUIToolkit> exportAction) {
			MenuItem parent = (MenuItem) this.UIManager.GetWidget("/menubar1/ToolsAction/ExportProjectAction1");
			
			MenuItem item = new MenuItem(name);
			item.Activated += (sender, e) => (exportAction(openedProject, guiToolKit));
			item.Show();
			(parent.Submenu as Menu).Append(item);
		}
		
		public void AddImportEntry (string name, string shortName, string filterName,
		                            string filter, Func<string, Project> importFunc,
		                            bool requiresNewFile) {
			MenuItem parent = ImportProjectActionMenu;
			MenuItem item = new MenuItem(name);
			item.Activated += (sender, e) => (EmitImportProject(name, filterName, filter, importFunc, requiresNewFile));
			item.Show();
			(parent.Submenu as Menu).Append(item);
		}
		
		public IAnalysisWindow SetProject(Project project, ProjectType projectType, CaptureSettings props, PlaysFilter filter)
		{
			ExportProjectAction1.Sensitive = true;
			
			this.projectType = projectType;
			openedProject = project;
			if (projectType == ProjectType.FileProject) {
				Title = System.IO.Path.GetFileNameWithoutExtension(
					openedProject.Description.File.FilePath) + " - " + Constants.SOFTWARE_NAME;
			} else {
				Title = Constants.SOFTWARE_NAME;
			}
			MakeActionsSensitive(true, projectType);
			analysisWindow.SetProject (project, projectType, props, filter);
			SetPanel (analysisWindow as Widget);
			return analysisWindow;
		}
		
		public void CloseProject () {
			openedProject = null;
			projectType = ProjectType.None;
			centralbox.Remove (analysisWindow as Gtk.Widget);
			ResetGUI ();
		}
		
		public void SelectProject (List<ProjectDescription> projects) {
			OpenProjectPanel panel  = new OpenProjectPanel ();
			panel.Projects = projects;
			panel.OpenProjectEvent += EmitOpenProjectID;
			SetPanel (panel);
		}
		
		public void CreateNewProject (Project project) {
			NewProjectPanel panel = new NewProjectPanel (project);
			panel.OpenNewProjectEvent += EmitOpenNewProject;
			SetPanel (panel);
		}

		#endregion
		
		#region Private Methods
		
		MenuItem ImportProjectActionMenu {
			get {
				return (MenuItem) this.UIManager.GetWidget("/menubar1/FileAction/ImportProjectAction");
			}
		}
		
		private void ConnectSignals() {
			/* Adding Handlers for each event */
			renderingstatebar1.ManageJobs += (e, o) => {EmitManageJobs();};
			openAction.Activated += (sender, e) => {EmitSaveProject();};
 		}
		
		private void ConnectMenuSignals() {
			SaveProjectAction.Activated += (o, e) => {EmitSaveProject();};
			CloseProjectAction.Activated += (o, e) => {EmitCloseOpenedProject ();};
			ExportToProjectFileAction.Activated += (o, e) => {EmitExportProject();};
			QuitAction.Activated += (o, e) => {CloseAndQuit();};
			CategoriesTemplatesManagerAction.Activated += (o, e) => {EmitManageCategories();};
			TeamsTemplatesManagerAction.Activated += (o, e) => {EmitManageTeams();};
			ProjectsManagerAction.Activated += (o, e) => {EmitManageProjects();};
			DatabasesManagerAction.Activated +=  (o, e) => {EmitManageDatabases();};
			PreferencesAction.Activated += (sender, e) => {EmitEditPreferences();};
			ShowProjectStatsAction.Activated += (sender, e) => {EmitShowProjectStats();}; 
		}
		
		private void ResetGUI() {
			Title = Constants.SOFTWARE_NAME;
			MakeActionsSensitive(false, projectType);
			foreach (Widget widget in centralbox.AllChildren) {
				if (widget != welcomepanel1) {
					centralbox.Remove (widget);
				}
			}
			welcomepanel1.Show ();
		}

		private void MakeActionsSensitive(bool sensitive, ProjectType projectType) {
			bool sensitive2 = sensitive && projectType == ProjectType.FileProject;
			CloseProjectAction.Sensitive=sensitive;
			TaggingViewAction.Sensitive = sensitive;
			ManualTaggingViewAction.Sensitive = sensitive;
			ExportProjectAction1.Sensitive = sensitive;
			ShowProjectStatsAction.Sensitive = sensitive;
			//GameUnitsViewAction.Sensitive = sensitive2 && gameUnitsActionVisible;
			TimelineViewAction.Sensitive = sensitive2;
			HideAllWidgetsAction.Sensitive=sensitive2;
			SaveProjectAction.Sensitive = sensitive2;
		}

		private void CloseAndQuit() {
			EmitCloseOpenedProject ();
			
			if (openedProject == null && QuitApplicationEvent != null) {
				QuitApplicationEvent ();
			}
		}
		
		protected override bool OnDeleteEvent (Event evnt)
		{
			CloseAndQuit ();
			return true;
		}
		
		#endregion

		#region Callbacks
		#region File
		protected virtual void OnNewActivated(object sender, System.EventArgs e)
		{
			EmitNewProject();
		}

		protected virtual void OnOpenActivated(object sender, System.EventArgs e)
		{
			EmitOpenProject();
		}
		#endregion
		
		#region Tool
		protected void OnVideoConverterToolActionActivated (object sender, System.EventArgs e)
		{
			int res;
			VideoConversionTool converter = new VideoConversionTool();
			res = converter.Run ();
			converter.Destroy();
			if (res == (int) ResponseType.Ok) {
				if (ConvertVideoFilesEvent != null)
					ConvertVideoFilesEvent (converter.Files, converter.EncodingSettings);
			}
		}
		
		private void EmitShowProjectStats () {
			if (ShowProjectStatsEvent != null)
				ShowProjectStatsEvent (openedProject);
		}

		#endregion
		
		#region View
		protected void OnTagSubcategoriesActionToggled (object sender, System.EventArgs e)
		{
			if (TagSubcategoriesChangedEvent != null)
				TagSubcategoriesChangedEvent (TagSubcategoriesAction.Active);
		}

		protected virtual void OnFullScreenActionToggled(object sender, System.EventArgs e)
		{
			if (ShowFullScreenEvent != null)
				ShowFullScreenEvent ((sender as Gtk.ToggleAction).Active);
		}

		protected virtual void OnPlaylistActionToggled(object sender, System.EventArgs e)
		{
			if (PlaylistVisibilityEvent != null)
				PlaylistVisibilityEvent ((sender as Gtk.ToggleAction).Active);
		}

		protected virtual void OnHideAllWidgetsActionToggled(object sender, System.EventArgs e)
		{
			if (AnalysisWidgetsVisibilityEvent != null) {
				AnalysisWidgetsVisibilityEvent ((sender as ToggleAction).Active);
			}
		}

		protected virtual void OnViewToggled(object sender, System.EventArgs e)
		{
			ToggleAction action = sender as Gtk.ToggleAction;
			
			if (!action.Active)
				return;
			
			if (AnalysisModeChangedEvent != null) {
				VideoAnalysisMode mode;
				
				if (sender == ManualTaggingViewAction) {
					mode = VideoAnalysisMode.ManualTagging;
				} else if (sender == TaggingViewAction) {
					mode = VideoAnalysisMode.PredefinedTagging;
				} else if (sender ==  TimelineViewAction) {
					mode = VideoAnalysisMode.Timeline;
				} else if (sender == GameUnitsViewAction) {
					mode = VideoAnalysisMode.GameUnits;
				} else {
					return;
				}
				AnalysisModeChangedEvent (mode);
			}
		}
		#endregion
		
		#region Help
		protected virtual void OnHelpAction1Activated(object sender, System.EventArgs e)
		{
			try {
				System.Diagnostics.Process.Start(Constants.MANUAL);
			} catch {}
		}

		protected virtual void OnAboutActionActivated(object sender, System.EventArgs e)
		{
			var about = new LongoMatch.Gui.Dialog.AboutDialog(guiToolKit.Version);
			about.TransientFor = this;
			about.Run();
			about.Destroy();
		}
		
		protected void OnDialogInfoActionActivated (object sender, System.EventArgs e)
		{
			var info = new LongoMatch.Gui.Dialog.ShortcutsHelpDialog();
			info.TransientFor = this;
			info.Run();
			info.Destroy();
		}
		
		#endregion
		#endregion

		#region Events
		
		public void EmitNewProject () {
			if (NewProjectEvent != null)
				NewProjectEvent();
		}
		
		public void EmitOpenProject () {
			if(OpenProjectEvent != null)
				OpenProjectEvent();
		}
				
		public void EmitEditPreferences ()
		{
			if (EditPreferencesEvent != null)
				EditPreferencesEvent();
		}
		
		public void EmitManageJobs() {
			if(ManageJobsEvent != null)
				ManageJobsEvent();
		}
		
		public void EmitManageTeams() {
			if(ManageTeamsEvent != null)
				ManageTeamsEvent();
		}
		
		public void EmitManageProjects()
		{
			if (ManageProjectsEvent != null)
				ManageProjectsEvent();
		}
		
		public void EmitManageDatabases()
		{
			if (ManageDatabasesEvent != null)
				ManageDatabasesEvent();
		}
		
		public void EmitManageCategories() {
			if(ManageCategoriesEvent != null)
				ManageCategoriesEvent();
		}
		
		void EmitSaveProject() {
			if (SaveProjectEvent != null)
				SaveProjectEvent(openedProject, projectType);
		}
		
		void EmitCloseOpenedProject() {
			if (CloseOpenedProjectEvent != null)
				CloseOpenedProjectEvent ();
		}
		
		void EmitImportProject(string name, string filterName, string filter,
		                               Func<string, Project> func, bool requiresNewFile) {
			if (ImportProjectEvent != null)
				ImportProjectEvent(name, filterName, filter, func, requiresNewFile);
		}
		
		void EmitExportProject() {
			if(ExportProjectEvent != null)
				ExportProjectEvent();
		}
		
		void EmitOpenProjectID (Guid projectID ) {
			if (OpenProjectIDEvent != null) {
				OpenProjectIDEvent (projectID);
			}
		}
		
		void EmitOpenNewProject (Project project, ProjectType projectType, CaptureSettings captureSettings)
		{
			if (OpenNewProjectEvent != null) {
				OpenNewProjectEvent (project, projectType, captureSettings);
			}
		}
		
		#endregion
	}
}
