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
using Gdk;
using Gtk;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Panel;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using Constants = LongoMatch.Core.Common.Constants;
using LMCommon = LongoMatch.Core.Common;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (false)]
	public partial class MainWindow : Gtk.Window, IMainController
	{
		IGUIToolkit guiToolKit;
		IAnalysisWindow analysisWindow;
		ProjectLongoMatch openedProject;
		ProjectType projectType;
		Widget currentPanel;
		Widget stackPanel;

		#region Constructors

		public MainWindow (IGUIToolkit guiToolkit) :
			base (Constants.SOFTWARE_NAME)
		{
			this.Build ();
			this.guiToolKit = guiToolkit;
			Title = Constants.SOFTWARE_NAME;
			projectType = ProjectType.None;
			
			ConnectSignals ();
			ConnectMenuSignals ();

			// Default screen
			Screen screen = Display.Default.DefaultScreen;
			// Which monitor is our window on
			int monitor = screen.GetMonitorAtWindow (this.GdkWindow);
			// Monitor size
			Rectangle monitor_geometry = screen.GetMonitorGeometry (monitor);
			// Resize to a convenient size
			this.Resize (monitor_geometry.Width * 80 / 100, monitor_geometry.Height * 80 / 100);
			if (Utils.OS == OperatingSystemID.OSX) {
				this.Move (monitor_geometry.Width * 10 / 100, monitor_geometry.Height * 10 / 100);
			}
		}

		#endregion

		#region Plubic Methods

		public IRenderingStateBar RenderingStateBar {
			get {
				return renderingstatebar1;
			}
		}

		public MenuShell Menu {
			get {
				return menubar1;
			}
		}

		public MenuItem QuitMenu {
			get {
				return (MenuItem)this.UIManager.GetWidget ("/menubar1/FileAction/QuitAction");
			}
		}

		public MenuItem PreferencesMenu {
			get {
				return (MenuItem)this.UIManager.GetWidget ("/menubar1/FileAction/PreferencesAction");
			}
		}

		public MenuItem AboutMenu {
			get {
				return (MenuItem)this.UIManager.GetWidget ("/menubar1/HelpAction/AboutAction");
			}
		}

		public UIManager GetUIManager ()
		{
			return UIManager;
		}

		/// <summary>
		/// Sets the panel. When panel is null, welcome panel is shown. Depending on current panel and new panel stacking may happen
		/// </summary>
		/// <param name="panel">Panel.</param>
		public void SetPanel (Widget panel)
		{
			if (panel == null) {
				ResetGUI ();
			} else {
				if (currentPanel is IAnalysisWindow && panel is PreferencesPanel) {
					RemovePanel (true);
				} else {
					RemovePanel (false);
				}
				currentPanel = panel;

				if (panel is IPanel) {
					(panel as IPanel).BackEvent += BackClicked;
					(panel as IPanel).OnLoaded ();
				}
				panel.Show ();
				centralbox.PackStart (panel, true, true, 0);
			}
		}

		public void AddExportEntry (string name, string shortName, Action<Project, IGUIToolkit> exportAction)
		{
			MenuItem parent = (MenuItem)this.UIManager.GetWidget ("/menubar1/ToolsAction/ExportProjectAction1");
			
			MenuItem item = new MenuItem (name);
			item.Activated += (sender, e) => (exportAction (openedProject, guiToolKit));
			item.Show ();
			(parent.Submenu as Menu).Append (item);
		}

		public IAnalysisWindow SetProject (ProjectLongoMatch project, ProjectType projectType, CaptureSettings props, EventsFilter filter)
		{
			ExportProjectAction1.Sensitive = true;
			
			this.projectType = projectType;
			openedProject = project;
			if (projectType == ProjectType.FileProject) {
				Title = openedProject.Description.Title +
				" - " + Constants.SOFTWARE_NAME;
			} else {
				Title = Constants.SOFTWARE_NAME;
			}
			MakeActionsSensitive (true, projectType);
			if (projectType == ProjectType.FakeCaptureProject) {
				analysisWindow = new FakeAnalysisComponent ();
			} else {
				analysisWindow = new AnalysisComponent ();
			}
			SetPanel (analysisWindow as Widget);
			analysisWindow.SetProject (project, projectType, props, filter);
			return analysisWindow;
		}

		public void CloseProject ()
		{
			openedProject = null;
			projectType = ProjectType.None;
			(analysisWindow as Gtk.Widget).Destroy ();
			analysisWindow = null;
			ResetGUI ();
		}

		public void Welcome ()
		{
			// Configure window icon
			Icon = Misc.LoadIcon (App.Current.SoftwareIconName, IconSize.Dialog);

			// Show the welcome panel
			SetPanel (null);
			// Populate the menu items from pluggable tools
			List<ITool> tools = new List<ITool> ();

			((LMCommon.EventsBroker)App.Current.EventsBroker).EmitQueryTools (tools);

			Menu menu = (this.UIManager.GetWidget ("/menubar1/ToolsAction") as MenuItem).Submenu as Menu;
			MenuItem before = this.UIManager.GetWidget ("/menubar1/ToolsAction/DatabasesManagerAction") as MenuItem;
			int idx = 1;

			// Find position of the database manager
			foreach (Widget child in menu.Children) {
				if (child == before)
					break;
				idx++;
			}

			// Get the default action group
			ActionGroup ag = this.UIManager.ActionGroups [0];
			uint mergeId;
			mergeId = this.UIManager.NewMergeId ();

			// Insert our tools
			foreach (ITool tool in tools) {
				if (tool.MenubarLabel != null) {
					// Use the class name as the action name to avoid collisions
					string actionName = tool.ToString ().Substring (tool.ToString ().LastIndexOf ('.') + 1) + "Action";
					Gtk.Action itemAction = new Gtk.Action (actionName, tool.MenubarLabel, null, null);
					itemAction.Sensitive = true;
					itemAction.ShortLabel = tool.MenubarLabel;
					itemAction.Activated += (sender, e) => {
						bool loadTool = true;
						if (openedProject != null) {
							loadTool = ((LMCommon.EventsBroker)App.Current.EventsBroker).EmitCloseOpenedProject ();
						}
						if (loadTool) {
							tool.Load (App.Current.GUIToolkit);
						}
					};

					this.UIManager.AddUi (mergeId, "/menubar1/ToolsAction", actionName, actionName, UIManagerItemType.Menuitem, false);
					ag.Add (itemAction, tool.MenubarAccelerator);

					// Ugly hack, given that we cannot add a menu item at a specific position
					// we remove it and finally add it back again.
					MenuItem item = (this.UIManager.GetWidget ("/menubar1/ToolsAction/" + actionName) as MenuItem);
					menu.Remove (item);
					menu.Insert (item, idx++);
				}
			}
			this.UIManager.EnsureUpdate ();
		}

		public void SelectProject (List<ProjectLongoMatch> projects)
		{
			OpenProjectPanel panel = new OpenProjectPanel ();
			panel.Projects = projects;
			SetPanel (panel);
		}

		public void CreateNewProject (ProjectLongoMatch project)
		{
			NewProjectPanel panel = new NewProjectPanel (project);
			panel.Name = "newprojectpanel";
			SetPanel (panel);
		}

		/// <summary>
		/// Quit application, proposing to close a potentially opened project before.
		/// </summary>
		/// <returns><c>true</c>, if the application is quitting, <c>false</c> if quit was cancelled by opened project.</returns>
		public bool CloseAndQuit ()
		{
			((LMCommon.EventsBroker)App.Current.EventsBroker).EmitCloseOpenedProject ();
			if (openedProject == null) {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitQuitApplication ();
			}
			return openedProject != null;
		}

		#endregion

		#region Private Methods

		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			if (!base.OnKeyPressEvent (evnt) || !(Focus is Entry)) {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitKeyPressed (this, VAS.Core.Common.Keyboard.ParseEvent (evnt));
			}
			return true;
		}

		MenuItem ImportProjectActionMenu {
			get {
				return (MenuItem)this.UIManager.GetWidget ("/menubar1/FileAction/ImportProjectAction");
			}
		}

		private void ConnectSignals ()
		{
			/* Adding Handlers for each event */
			renderingstatebar1.ManageJobs += (e, o) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitManageJobs ();
			};
		}

		private void ConnectMenuSignals ()
		{
			SaveProjectAction.Activated += (o, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitSaveProject (openedProject, projectType);
			};
			CloseProjectAction.Activated += (o, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitCloseOpenedProject ();
			};
			ExportToProjectFileAction.Activated += (o, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitExportProject (openedProject);
			};
			CategoriesTemplatesManagerAction.Activated += (o, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitManageCategories ();
			};
			TeamsTemplatesManagerAction.Activated += (o, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitManageTeams ();
			};
			ProjectsManagerAction.Activated += (o, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitManageProjects ();
			};
			DatabasesManagerAction.Activated += (o, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitManageDatabases ();
			};
			PreferencesAction.Activated += (sender, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitEditPreferences ();
			};
			ShowProjectStatsAction.Activated += (sender, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitShowProjectStats (openedProject);
			}; 
			QuitAction.Activated += (o, e) => {
				CloseAndQuit ();
			};
			OpenProjectAction.Activated += (sender, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitSaveProject (openedProject, projectType);
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitOpenProject ();
			};
			NewPojectAction.Activated += (sender, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitNewProject (null);
			};
			ImportProjectAction.Activated += (sender, e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitImportProject ();
			};
			FullScreenAction.Activated += (object sender, EventArgs e) => {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitShowFullScreen (FullScreenAction.Active);
			};
		}

		void DestroyPanel (Widget panel)
		{
			if (panel is IPanel) {
				(panel as IPanel).BackEvent -= BackClicked;
				(panel as IPanel).OnUnloaded ();
			}
			panel.Destroy ();
			panel.Dispose ();
			System.GC.Collect ();
		}

		void RemovePanel (bool stack)
		{
			if (currentPanel == null) {
				return;
			}
			if (stack) {
				stackPanel = currentPanel;
				stackPanel.Visible = false;
			} else {
				DestroyPanel (currentPanel);
				currentPanel = null;
				if (stackPanel != null) {
					DestroyPanel (stackPanel);
					stackPanel = null;
				}
			}
		}

		void BackClicked ()
		{
			if (stackPanel != null) {
				DestroyPanel (currentPanel);
				currentPanel = stackPanel;
				stackPanel.Visible = true;
			} else {
				ResetGUI ();
			}
		}

		private void ResetGUI ()
		{
			Title = Constants.SOFTWARE_NAME;
			MakeActionsSensitive (false, projectType);
			RemovePanel (false);
			currentPanel = new WelcomePanel ();
			currentPanel.Show ();
			centralbox.PackStart (currentPanel, true, true, 0);
		}

		private void MakeActionsSensitive (bool sensitive, ProjectType projectType)
		{
			bool sensitive2 = sensitive && projectType == ProjectType.FileProject;
			CloseProjectAction.Sensitive = sensitive;
			ExportProjectAction1.Sensitive = sensitive;
			ShowProjectStatsAction.Sensitive = sensitive;
			SaveProjectAction.Sensitive = sensitive2;
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
			VideoConversionTool converter = new VideoConversionTool ();
			res = converter.Run ();
			converter.Destroy ();
			if (res == (int)ResponseType.Ok) {
				((LMCommon.EventsBroker)App.Current.EventsBroker).EmitConvertVideoFiles (converter.Files,
					converter.EncodingSettings);
			}
		}

		protected virtual void OnHelpAction1Activated (object sender, System.EventArgs e)
		{
			try {
				System.Diagnostics.Process.Start (Constants.MANUAL);
			} catch {
			}
		}

		protected virtual void OnAboutActionActivated (object sender, System.EventArgs e)
		{
			var about = new LongoMatch.Gui.Dialog.AboutDialog (App.Current.Version);
			about.TransientFor = this;
			about.Run ();
			about.Destroy ();
		}

		protected void OnMigrationToolActionActivated (object sender, EventArgs e)
		{
			((LMCommon.EventsBroker)App.Current.EventsBroker).EmitMigrateDB ();
		}

		#endregion
	}
}
