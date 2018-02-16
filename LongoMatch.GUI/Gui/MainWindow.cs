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
using System.Dynamic;
using System.Linq;
using FluVAS.License.State;
using Gdk;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.ViewModel;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Panel;
using LongoMatch.Services.State;
using LongoMatch.Services.States;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Services.State;
using VAS.UI.Menus;
using Constants = LongoMatch.Core.Common.Constants;
using Misc = VAS.UI.Helpers.Misc;
using PreferencesState = LongoMatch.Services.State.PreferencesState;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (false)]
	public partial class MainWindow : Gtk.Window, IMainController
	{
		IGUIToolkit guiToolKit;
		Widget currentPanel;
		LimitationCommand databaseManagerCommand;
		LimitationCommand newProjectCommand;
		LimitationCommand importCommand;


		#region Constructors

		public MainWindow (IGUIToolkit guiToolkit) :
			base (Constants.SOFTWARE_NAME)
		{
			this.Build ();
			this.guiToolKit = guiToolkit;
			Title = Constants.SOFTWARE_NAME;

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
			databaseManagerCommand = new LimitationCommand (LongoMatchFeature.DatabaseManager.ToString (), () => {
				App.Current.StateController.MoveToModal (DatabasesManagerState.NAME, null, true);
			});
			newProjectCommand = new LimitationCommand (VASCountLimitedObjects.Projects.ToString (), () => {
				App.Current.StateController.MoveTo (NewProjectState.NAME, null, true);
			});
			importCommand = new LimitationCommand (VASCountLimitedObjects.Projects.ToString (), () => {
				App.Current.EventsBroker.Publish (new ImportProjectEvent ());
			});
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

		public MenuItem CheckForUpdatesMenu {
			get {
				return (MenuItem)this.UIManager.GetWidget ("/menubar1/HelpAction/CheckForUpdatesAction");
			}
		}

		public MenuItem EnterLicenseMenu {
			get {
				return (MenuItem)this.UIManager.GetWidget ("/menubar1/HelpAction/EnterLicenseKeyAction");
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
		/// Gets the file extension menu enter point
		/// </summary>
		/// <value>The file menu extension entry.</value>
		public MenuExtensionEntry FileMenuEntry { get; protected set; }

		/// <summary>
		/// Gets the tools extension menu enter point
		/// </summary>
		/// <value>The tools menu extension entry.</value>
		public MenuExtensionEntry ToolMenuEntry { get; protected set; }

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
						await App.Current.StateController.MoveTo (tool.UIFlow.First ().Key, null);
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

			FileMenuEntry = new MenuExtensionEntry ("/menubar1/FileAction", 3);
			ToolMenuEntry = new MenuExtensionEntry ("/menubar1/ToolsAction", 6);
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
			App.Current.EventsBroker.Subscribe<OpenedProjectEvent> (HandleOpened);
			App.Current.EventsBroker.Subscribe<CloseEvent<LMProjectVM>> (HandleClosed);
		}

		private void ConnectMenuSignals ()
		{
			CategoriesTemplatesManagerAction.Activated += (o, e) => {
				App.Current.StateController.MoveTo (DashboardsManagerState.NAME, null, true);
			};
			TeamsTemplatesManagerAction.Activated += (o, e) => {
				App.Current.StateController.MoveTo (TeamsManagerState.NAME, null, true);
			};
			ProjectsManagerAction.Activated += (o, e) => {
				App.Current.StateController.MoveTo (ProjectsManagerState.NAME, null, true);
			};
			//FIXME: this should be done by binding the LimitationCommand to the MenuItem
			DatabasesManagerAction.Activated += (o, e) => {
				databaseManagerCommand.Execute ();
			};
			PreferencesAction.Activated += (sender, e) => {
				App.Current.StateController.MoveTo (PreferencesState.NAME, null);
			};
			QuitAction.Activated += (o, e) => {
				App.Current.GUIToolkit.Quit ();
			};
			OpenProjectAction.Activated += (sender, e) => {
				App.Current.StateController.MoveTo (OpenProjectState.NAME, null, true);
			};
			NewPojectAction.Activated += (sender, e) => {
				newProjectCommand.Execute ();
			};
			ImportProjectAction.Activated += (sender, e) => {
				importCommand.Execute ();
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
			MakeActionsSensitive (true);
		}

		private void MakeActionsSensitive (bool sensitive)
		{
			DatabasesManagerAction.Sensitive = sensitive;
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
			App.Current.StateController.MoveToModal (AboutState.NAME, null);
		}
		/// <summary>
		/// Handles the check for updates action initializing the Sparkle CheckForUpdates Workflow.
		/// </summary>
		protected void HandleCheckForUpdatesAction (object sender, EventArgs e)
		{
			App.Current.DependencyRegistry
			   .Retrieve<IAppUpdater> (InstanceType.Default)
			   .CheckForUpdates ();
		}

		#endregion

		void HandleOpened (OpenedProjectEvent e)
		{
			MakeActionsSensitive (false);
		}

		// FIXME: This event does not mean that the project has been closed
		// if user selects no then the items are enabled incorrectly
		void HandleClosed (CloseEvent<LMProjectVM> e)
		{
			MakeActionsSensitive (true);
		}

		/// <summary>
		/// Enter license key action handler.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected void HandleEnterLicenseKeyAction (object sender, EventArgs e)
		{
			dynamic properties = new ExpandoObject ();
			properties.Url = Constants.LICENSE_CODE_URL;
			properties.LogoImage = App.Current.ResourcesLocator.LoadImage (Constants.LM_LOGO_ANY, 400, 200);

			App.Current.StateController.MoveToModal (LicenseEntryState.NAME, properties);
		}
	}
}
