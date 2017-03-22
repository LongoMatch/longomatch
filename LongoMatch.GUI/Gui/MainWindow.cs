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
using System.Threading.Tasks;
using Gdk;
using Gtk;
using LongoMatch.Core.Events;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Panel;
using LongoMatch.Services.State;
using LongoMatch.Services.States;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Plugins;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using Constants = LongoMatch.Core.Common.Constants;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (false)]
	public partial class MainWindow : Gtk.Window, IMainController
	{
		IGUIToolkit guiToolKit;
		LMProjectVM openedProject;
		ProjectType projectType;
		Widget currentPanel;

		#region Constructors

		public MainWindow (IGUIToolkit guiToolkit) :
			base (Constants.SOFTWARE_NAME)
		{
			this.Build ();
			this.guiToolKit = guiToolkit;
			Title = Constants.SOFTWARE_NAME;
			projectType = ProjectType.None;

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

		public IMenuExtensionEntry FileMenuEntry { get; protected set; }

		public IMenuExtensionEntry ToolMenuEntry { get; protected set; }

		/// <summary>
		/// Sets the panel. When panel is null, welcome panel is shown. Depending on current panel and new panel stacking may happen
		/// </summary>
		/// <param name="panel">Panel.</param>
		public bool SetPanel (IPanel panel)
		{
			if (panel == null) {
				return App.Current.StateController.MoveToHome ().Result;
			}

			if (currentPanel != null) {
				((IPanel)currentPanel).OnUnload ();
				centralbox.Remove (currentPanel);
			}
			Title = panel.Title;
			panel.OnLoad ();
			currentPanel = (Widget)panel;
			centralbox.PackStart (currentPanel, true, true, 0);
			currentPanel.Show ();
			// FIXME: Remove this once everything uses the ITool implementation correctly
			if (panel is WelcomePanel) {
				ResetGUI ();
			}
			return true;
		}

		public void Initialize ()
		{
			Show ();

			// Configure window icon
			Icon = Misc.LoadIcon (App.Current.SoftwareIconName, IconSize.Dialog);

			// Populate the menu items from pluggable tools
			List<ITool> tools = new List<ITool> ();

			App.Current.EventsBroker.Publish<QueryToolsEvent> (
				new QueryToolsEvent {
					Tools = tools
				}
			);

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
					itemAction.Activated += async (sender, e) => {
						bool loadTool = true;
						if (openedProject != null) {
							loadTool = await App.Current.EventsBroker.PublishWithReturn (new CloseEvent<ProjectVM> { Object = openedProject });
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
			renderingstatebarview1.SetViewModel (App.Current.JobsManager);

			ConnectSignals ();

			this.FileMenuEntry = new MenuExtensionEntry ("/menubar1/FileAction", 3);
			this.ToolMenuEntry = new MenuExtensionEntry ("/menubar1/ToolsAction", 6);
		}

		/// <summary>
		/// Quit application, proposing to close a potentially opened project before.
		/// </summary>
		/// <returns><c>true</c>, if the application is quitting, <c>false</c> if quit was cancelled by opened project.</returns>
		public async Task<bool> CloseAndQuit ()
		{
			return await App.Current.GUIToolkit.Quit ();
		}

		#endregion

		#region Private Methods

		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			if (!base.OnKeyPressEvent (evnt) || !(Focus is Entry)) {
				App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseEvent (evnt));
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
			App.Current.EventsBroker.Subscribe<OpenEvent<ProjectVM>> (HandleOpen);
		}

		private void ConnectMenuSignals ()
		{
			/*SaveProjectAction.Activated += (o, e) => {
				App.Current.EventsBroker.Publish (new SaveEvent<LMProjectVM> { Object = openedProject });
			};*/
			CloseProjectAction.Activated += (o, e) => {
				App.Current.StateController.MoveToHome ();
			};
			CategoriesTemplatesManagerAction.Activated += (o, e) => {
				App.Current.StateController.MoveTo (DashboardsManagerState.NAME, null, true);
			};
			TeamsTemplatesManagerAction.Activated += (o, e) => {
				App.Current.StateController.MoveTo (TeamsManagerState.NAME, null, true);
			};
			ProjectsManagerAction.Activated += (o, e) => {
				App.Current.StateController.MoveTo (ProjectsManagerState.NAME, null, true);
			};
			DatabasesManagerAction.Activated += (o, e) => {
				App.Current.EventsBroker.Publish<ManageDatabasesEvent> (new ManageDatabasesEvent ());
			};
			PreferencesAction.Activated += (sender, e) => {
				App.Current.StateController.MoveTo (PreferencesState.NAME, null);
			};
			ShowProjectStatsAction.Activated += (sender, e) => {
				App.Current.EventsBroker.Publish<ShowProjectStatsEvent> (
					new ShowProjectStatsEvent {
						Project = openedProject.Model
					}
				);
			};
			QuitAction.Activated += (o, e) => {
				App.Current.GUIToolkit.Quit ();
			};
			OpenProjectAction.Activated += (sender, e) => {
				App.Current.EventsBroker.Publish<SaveProjectEvent> (
					new SaveProjectEvent {
						Project = openedProject.Model,
						ProjectType = projectType
					}
				);
				App.Current.StateController.MoveTo (OpenProjectState.NAME, null, true);
			};
			NewPojectAction.Activated += (sender, e) => {
				App.Current.StateController.MoveTo (NewProjectState.NAME, null, true);
			};
			ImportProjectAction.Activated += (sender, e) => {
				App.Current.EventsBroker.Publish<ImportProjectEvent> (new ImportProjectEvent ());
			};
			FullScreenAction.Activated += (object sender, EventArgs e) => {
				App.Current.EventsBroker.Publish<ShowFullScreenEvent> (
					new ShowFullScreenEvent {
						Active = FullScreenAction.Active
					}
				);
			};
		}

		private void ResetGUI ()
		{
			Title = Constants.SOFTWARE_NAME;
			MakeActionsSensitive (false, projectType);
		}

		private void MakeActionsSensitive (bool sensitive, ProjectType projectType)
		{
			ExportProjectAction1.Sensitive = sensitive;
			ShowProjectStatsAction.Sensitive = sensitive;
		}

		protected override bool OnDeleteEvent (Gdk.Event evnt)
		{
			App.Current.GUIToolkit.Quit ();
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
				ConversionJob job = new ConversionJob (converter.Files, converter.EncodingSettings);
				App.Current.JobsManager.Add (job);
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
			App.Current.EventsBroker.Publish<MigrateDBEvent> ();
		}

		#endregion

		void HandleOpen (OpenEvent<ProjectVM> e)
		{
			openedProject = e.Object as LMProjectVM;
			MakeActionsSensitive (true, openedProject.ProjectType);
		}
	}
}
