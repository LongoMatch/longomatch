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
	public partial class MainWindow : Gtk.Window, IMainController
	{
		IGUIToolkit guiToolKit;
		IAnalysisWindow analysisWindow;
		Project openedProject;
		ProjectType projectType;
		Widget currentPanel;

		#region Constructors
		public MainWindow(IGUIToolkit guiToolkit) :
		base(Constants.SOFTWARE_NAME)
		{
			Screen screen;
			
			this.Build();
			this.guiToolKit = guiToolkit;
			
			Title = Constants.SOFTWARE_NAME;
			TagSubcategoriesAction.Active = !Config.FastTagging;
			projectType = ProjectType.None;
			
			ConnectSignals();
			ConnectMenuSignals();
			
			MenuItem parent = ImportProjectActionMenu;
			parent.Submenu = new Menu();
			AddImportEntry(Catalog.GetString("Import file project"), "ImportFileProject",
			               Constants.PROJECT_NAME + " (" + Constants.PROJECT_EXT + ")",
			               "*" + Constants.PROJECT_EXT, Project.Import,
			               false);
			screen = Display.Default.DefaultScreen;
			this.Resize(screen.Width * 80 / 100, screen.Height * 80 / 100);
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
				RemovePanel ();
				currentPanel = panel;
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
			item.Activated += (sender, e) => (
				Config.EventsBroker.EmitImportProject (name, filterName, filter,
			                                       importFunc, requiresNewFile));
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
			analysisWindow = new AnalysisComponent();
			analysisWindow.SetProject (project, projectType, props, filter);
			SetPanel (analysisWindow as Widget);
			return analysisWindow;
		}
		
		public void CloseProject () {
			openedProject = null;
			projectType = ProjectType.None;
			(analysisWindow as Gtk.Widget).Destroy();
			analysisWindow = null;
			ResetGUI ();
		}
		
		public void SelectProject (List<ProjectDescription> projects) {
			OpenProjectPanel panel  = new OpenProjectPanel ();
			panel.Projects = projects;
			SetPanel (panel);
		}
		
		public void CreateNewProject (Project project) {
			NewProjectPanel panel = new NewProjectPanel (project);
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
			renderingstatebar1.ManageJobs += (e, o) => {
				Config.EventsBroker.EmitManageJobs();};
 		}
		
		private void ConnectMenuSignals() {
			SaveProjectAction.Activated += (o, e) => {
				Config.EventsBroker.EmitSaveProject (openedProject, projectType);};
			CloseProjectAction.Activated += (o, e) => {
				Config.EventsBroker.EmitCloseOpenedProject ();};
			ExportToProjectFileAction.Activated += (o, e) => {
				Config.EventsBroker.EmitExportProject (openedProject);};
			CategoriesTemplatesManagerAction.Activated += (o, e) => {
				Config.EventsBroker.EmitManageCategories();};
			TeamsTemplatesManagerAction.Activated += (o, e) => {
				Config.EventsBroker.EmitManageTeams();};
			ProjectsManagerAction.Activated += (o, e) => {
				Config.EventsBroker.EmitManageProjects();};
			DatabasesManagerAction.Activated +=  (o, e) => {
				Config.EventsBroker.EmitManageDatabases();};
			PreferencesAction.Activated += (sender, e) => {
				Config.EventsBroker.EmitEditPreferences();};
			ShowProjectStatsAction.Activated += (sender, e) => {
				Config.EventsBroker.EmitShowProjectStats (openedProject);}; 
			QuitAction.Activated += (o, e) => {CloseAndQuit();};
			openAction.Activated += (sender, e) => {
				Config.EventsBroker.EmitSaveProject (openedProject, projectType);
				Config.EventsBroker.EmitOpenProject ();};
			NewPojectAction.Activated += (sender, e) => {
				Config.EventsBroker.EmitNewProject ();
			};
			TagSubcategoriesAction.Activated += (sender, e) => {
				Config.EventsBroker.EmitTagSubcategories (TagSubcategoriesAction.Active);
			};
		}
		
		void RemovePanel () {
			if (currentPanel != null) {
				if (currentPanel is IPanel) {
					(currentPanel as IPanel).BackEvent -= ResetGUI;
				}
				currentPanel.Destroy ();
				currentPanel.Dispose();
				System.GC.Collect();
			}
			currentPanel = null;
		}
		
		private void ResetGUI() {
			Title = Constants.SOFTWARE_NAME;
			MakeActionsSensitive(false, projectType);
			RemovePanel ();
			welcomepanel1.Show ();
		}

		private void MakeActionsSensitive(bool sensitive, ProjectType projectType) {
			bool sensitive2 = sensitive && projectType == ProjectType.FileProject;
			CloseProjectAction.Sensitive=sensitive;
			ExportProjectAction1.Sensitive = sensitive;
			ShowProjectStatsAction.Sensitive = sensitive;
			SaveProjectAction.Sensitive = sensitive2;
		}

		private void CloseAndQuit() {
			Config.EventsBroker.EmitCloseOpenedProject ();
			if (openedProject == null) {
				Config.EventsBroker.EmitQuitApplication ();
			}
		}
		
		protected override bool OnDeleteEvent (Event evnt)
		{
			CloseAndQuit ();
			return true;
		}
		
		#endregion

		#region Callbacks
		protected void OnVideoConverterToolActionActivated (object sender, System.EventArgs e)
		{
			int res;
			VideoConversionTool converter = new VideoConversionTool();
			res = converter.Run ();
			converter.Destroy();
			if (res == (int) ResponseType.Ok) {
				Config.EventsBroker.EmitConvertVideoFiles (converter.Files,
				                                           converter.EncodingSettings);
			}
		}
		
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
	}
}
