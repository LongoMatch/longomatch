//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using System.Threading.Tasks;
using Gtk;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Plugins;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.UI.Helpers;
using Constants = LongoMatch.Core.Common.Constants;
using LKeyAction = LongoMatch.Core.Common.KeyAction;
using VKeyAction = VAS.Core.Hotkeys.KeyAction;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	[View (ProjectAnalysisState.NAME)]
	[View (LiveProjectAnalysisState.NAME)]
	public partial class AnalysisComponent : Gtk.Bin, IPanel<LMProjectAnalysisVM>
	{
		List<MenuItem> menuItems;
		bool detachedPlayer;
		LMProjectAnalysisVM viewModel;
		Gtk.Window playerWindow;

		public AnalysisComponent ()
		{
			this.Build ();
			detachedPlayer = false;
			menuItems = new List<MenuItem> ();
		}

		protected override void OnUnmapped ()
		{
			base.OnUnmapped ();
			// When a container widget is unmapped there are 2 options, either it has a window and it hides it,
			// or it just unmaps all children. In our case we are use an event box with its own window for theming.
			// Unmapping this child will just hide its window and won't unmap the children below such as as the 
			// player view and the video window. This can be a problem as this widget will never detect that it has been 
			// hidden and on windows the actual gdkwindow will be hidden and won't be shown again. So we make sure to
			// proxy the unmap to the children below the eventbox.
			centralpane.Unmap ();
		}

		protected override void OnDestroyed ()
		{
			if (detachedPlayer) {
				playerWindow.Destroy ();
				detachedPlayer = false;
			}
			playercapturer.Destroy ();
			base.OnDestroyed ();
		}

		public override void Destroy ()
		{
			if (detachedPlayer) {
				DetachPlayer ();
			}
			base.Destroy ();
		}

		public IVideoPlayerController Player {
			get {
				return playercapturer.ViewModel.Player;
			}
		}

		public ICapturerBin Capturer {
			get {
				return playercapturer.Capturer;
			}
		}

		public LMProjectAnalysisVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				playercapturer.ViewModel = viewModel.VideoPlayer;
				codingwidget.ViewModel = viewModel;
				playsSelection.ViewModel = viewModel.Project;
				if (viewModel.Project.Model.ProjectType == ProjectType.FileProject) {
					playercapturer.Mode = PlayerViewOperationMode.Analysis;
				} else {
					playercapturer.Mode = playercapturer.Mode = PlayerViewOperationMode.LiveAnalysisReview;
					Capturer.PeriodsNames = viewModel.Project.Model.Dashboard.GamePeriods.ToList ();
					Capturer.Periods = viewModel.Project.Model.Periods.ToList ();
				}
			}
		}

		public string Title {
			get {
				return ViewModel?.Project.ShortDescription;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMProjectAnalysisVM)viewModel;
		}

		public KeyContext GetKeyContext ()
		{
			var keyContext = new KeyContext ();
			/*keyContext.AddAction (
				new VKeyAction ("ZOOM_IN", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.ZoomIn],
								() => codingwidget.ZoomIn ()));
			keyContext.AddAction (
				new VKeyAction ("ZOOM_OUT", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.ZoomOut],
								() => codingwidget.ZoomOut ()));
			keyContext.AddAction (
				new VKeyAction ("FIT_TIMELINE", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.FitTimeline],
								() => codingwidget.FitTimeline ()));
			keyContext.AddAction (
				new VKeyAction ("SHOW_DASHBOARD", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.ShowDashboard],
								() => codingwidget.ShowDashboard ()));
			keyContext.AddAction (
				new VKeyAction ("SHOW_TIMELINE", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.ShowDashboard],
								() => codingwidget.ShowTimeline ()));
			keyContext.AddAction (
				new VKeyAction ("SHOW_ZONAL_TAGS", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.ShowDashboard],
								() => codingwidget.ShowZonalTags ()));*/
			return keyContext;
		}

		public void OnLoad ()
		{
			LoadFileMenu ();
			LoadToolsMenu ();
		}

		public void OnUnload ()
		{
			MainWindow window = App.Current.GUIToolkit.MainController as MainWindow;
			UIManager uimanager = window.GetUIManager ();

			window.FileMenuEntry.ResetMenuEntry ();
			MenuItem fileMenu = ((MenuItem)uimanager.GetWidget (window.FileMenuEntry.MenuName));
			foreach (MenuItem item in menuItems) {
				(fileMenu.Submenu as Menu).Remove (item);
			}

			window.ToolMenuEntry.ResetMenuEntry ();
			MenuItem toolsMenu = ((MenuItem)uimanager.GetWidget (window.ToolMenuEntry.MenuName));
			foreach (MenuItem item in menuItems) {
				(toolsMenu.Submenu as Menu).Remove (item);
			}

			menuItems.Clear ();
		}

		public void TagPlayer (Player player)
		{
			codingwidget.TagPlayer ((LMPlayer)player);
		}

		public void TagTeam (TeamType team)
		{
			codingwidget.TagTeam (team);
		}

		public void DetachPlayer ()
		{
			bool isPlaying = ViewModel.VideoPlayer.Playing;

			/* Pause the player here to prevent the sink drawing while the windows
			 * are beeing changed */
			ViewModel.VideoPlayer.Pause ();
			if (!detachedPlayer) {
				Log.Debug ("Detaching player");

				ExternalWindow playerWindow = new ExternalWindow ();
				this.playerWindow = playerWindow;
				playerWindow.Title = Constants.SOFTWARE_NAME;
				int player_width = playercapturer.Allocation.Width;
				int player_height = playercapturer.Allocation.Height;
				playerWindow.SetDefaultSize (player_width, player_height);
				playerWindow.DeleteEvent += (o, args) => DetachPlayer ();
				playerWindow.Show ();
				playercapturer.Reparent (playerWindow.Box);
				// Hack to reposition video window in widget for OSX
				playerWindow.Resize (player_width + 10, player_height);
				videowidgetsbox.Visible = false;
				playsSelection.ExpandTabs = true;
			} else {
				Log.Debug ("Attaching player again");
				videowidgetsbox.Visible = true;
				playercapturer.Reparent (this.videowidgetsbox);
				playerWindow.Destroy ();
				playsSelection.ExpandTabs = false;
			}
			if (isPlaying) {
				ViewModel.VideoPlayer.Play ();
			}
			detachedPlayer = !detachedPlayer;
			playercapturer.AttachPlayer (detachedPlayer);
		}

		public void CloseOpenedProject ()
		{
			if (detachedPlayer)
				DetachPlayer ();
		}

		void LoadFileMenu ()
		{
			MainWindow window = App.Current.GUIToolkit.MainController as MainWindow;
			MenuItem fileMenu = ((MenuItem)window.GetUIManager ().GetWidget (window.FileMenuEntry.MenuName));

			MenuItem save = this.ViewModel.SaveCommand.CreateMenuItem (
				Catalog.GetString ("Save Project"), UIManager.AccelGroup, "SAVE_PROJECT");
			RegisterMenuItem (save, fileMenu.Submenu as Menu, window.FileMenuEntry);

			MenuItem close = this.ViewModel.CloseCommand.CreateMenuItem (
				Catalog.GetString ("Close Project"), UIManager.AccelGroup, "CLOSE_PROJECT");
			RegisterMenuItem (close, fileMenu.Submenu as Menu, window.FileMenuEntry);
		}

		void LoadToolsMenu ()
		{
			MainWindow window = App.Current.GUIToolkit.MainController as MainWindow;
			MenuItem toolMenu = ((MenuItem)window.GetUIManager ().GetWidget (window.ToolMenuEntry.MenuName));

			// show stats menu item
			MenuItem show = this.ViewModel.ShowStatsCommand.CreateMenuItem (
				Catalog.GetString ("Show projects stats"), UIManager.AccelGroup, null);
			RegisterMenuItem (show, toolMenu.Submenu as Menu, window.ToolMenuEntry);

			// Export menu item
			MenuItem exportMenu = new MenuItem (Catalog.GetString ("Export Project")) {
				Name = "ExportProjectAction", Submenu = new Menu (), Visible = true };
			(toolMenu.Submenu as Menu).Insert (exportMenu, window.ToolMenuEntry.LastPosition);
			window.ToolMenuEntry.UpdateLastPosition ();
			this.menuItems.Add (exportMenu);

			foreach (IProjectExporter exporter in
			    App.Current.DependencyRegistry.RetrieveAll<IProjectExporter> (InstanceType.Default)) {
				AddExportEntry (exportMenu, exporter.Description, new Func<Project, bool, Task> (exporter.Export));
			}

			// Add final separator
			SeparatorMenuItem separator = new SeparatorMenuItem () { Visible = true };
			(toolMenu.Submenu as Menu).Insert (separator, window.ToolMenuEntry.LastPosition);
		}

		void RegisterMenuItem (MenuItem item, Menu menu, MenuExtensionEntry menuEntry)
		{
			menuItems.Add (item);
			menu.Insert (item, menuEntry.LastPosition);
			menuEntry.UpdateLastPosition ();
		}

		void AddExportEntry (MenuItem parent, string name, Func<Project, bool, Task> exportAction)
		{
			MenuItem item = new MenuItem (name) { Visible = true };
			item.Activated += (sender, e) => (exportAction (viewModel.Project.Model, false));
			(parent.Submenu as Menu).Append (item);
		}
	}
}
