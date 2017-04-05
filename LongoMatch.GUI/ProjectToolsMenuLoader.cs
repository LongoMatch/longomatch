//
//  Copyright (C) 2017 FLUENDO S.A
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
using System.Threading.Tasks;
using Gtk;
using LongoMatch.Core;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Plugins;
using VAS.Core.Store;
using VAS.UI.Helpers;

namespace LongoMatch.Gui
{
	/// <summary>
	/// Project tools menu loader.
	/// </summary>
	public sealed class ProjectToolsMenuLoader : MenuLoaderBase
	{
		/// <summary>
		/// Loads the tools menu for a given project
		/// </summary>
		/// <param name="projectVM">Project view model.</param>
		public void LoadMenu (LMProjectAnalysisVM projectVM)
		{
			MainWindow window = App.Current.GUIToolkit.MainController as MainWindow;
			MenuItem toolMenu = ((MenuItem)window.GetUIManager ().GetWidget (window.ToolMenuEntry.MenuName));

			// Add separator
			SeparatorMenuItem separator = new SeparatorMenuItem () { Visible = true };
			(toolMenu.Submenu as Menu).Insert (separator, window.ToolMenuEntry.LastPosition);
			window.ToolMenuEntry.UpdateLastPosition ();

			// show stats menu item
			MenuItem show = projectVM.ShowStatsCommand.CreateMenuItem (
				Catalog.GetString ("Show projects stats"), window.GetUIManager ().AccelGroup, null);
			RegisterMenuItem (show, toolMenu.Submenu as Menu, window.ToolMenuEntry);

			// Export menu item
			MenuItem exportMenu = new MenuItem (Catalog.GetString ("Export Project")) {
				Name = "ExportProjectAction", Submenu = new Menu (), Visible = true
			};
			(toolMenu.Submenu as Menu).Insert (exportMenu, window.ToolMenuEntry.LastPosition);
			window.ToolMenuEntry.UpdateLastPosition ();
			this.MenuItems.Add (exportMenu);

			foreach (IProjectExporter exporter in
				App.Current.DependencyRegistry.RetrieveAll<IProjectExporter> (InstanceType.Default)) {
				AddExportEntry (exportMenu, exporter.Description, new Func<Project, bool, Task> (exporter.Export), projectVM);
			}
		}

		/// <summary>
		/// Unloads the tools menu
		/// </summary>
		public void UnloadMenu ()
		{
			MainWindow window = App.Current.GUIToolkit.MainController as MainWindow;
			MenuItem toolsMenu = ((MenuItem)window.GetUIManager ().GetWidget (window.ToolMenuEntry.MenuName));
			window.ToolMenuEntry.ResetMenuEntry ();
			CleanMenu (toolsMenu);
		}

		void AddExportEntry (MenuItem parent, string name, Func<Project, bool, Task> exportAction, LMProjectAnalysisVM viewModel)
		{
			MenuItem item = new MenuItem (name) { Visible = true };
			item.Activated += (sender, e) => (exportAction (viewModel.Project.Model, false));
			(parent.Submenu as Menu).Append (item);
		}
	}
}
