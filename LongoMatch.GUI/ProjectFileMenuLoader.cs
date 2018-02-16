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
using Gtk;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Hotkeys;
using VAS.UI.Helpers;
using VAS.UI.Menus;

namespace LongoMatch.Gui
{
	/// <summary>
	/// Project file menu loader.
	/// </summary>
	public sealed class ProjectFileMenuLoader : MenuLoaderBase
	{
		/// <summary>
		/// Loads the file menu for a given project
		/// </summary>
		/// <param name="projectVM">Project view model.</param>
		public void LoadMenu (LMProjectAnalysisVM projectVM)
		{
			MainWindow window = App.Current.GUIToolkit.MainController as MainWindow;
			MenuItem fileMenu = ((MenuItem)window.GetUIManager ().GetWidget (window.FileMenuEntry.MenuName));

			MenuItem save = projectVM.SaveCommand.CreateMenuItem (
				Catalog.GetString ("Save Project"), window.GetUIManager ().AccelGroup, GeneralUIHotkeys.SAVE);
			RegisterMenuItem (save, fileMenu.Submenu as Menu, window.FileMenuEntry);

			MenuItem close = projectVM.CloseCommand.CreateMenuItem (
				Catalog.GetString ("Close Project"), window.GetUIManager ().AccelGroup, GeneralUIHotkeys.CLOSE);
			RegisterMenuItem (close, fileMenu.Submenu as Menu, window.FileMenuEntry);
		}

		/// <summary>
		/// Unloads the file menu.
		/// </summary>
		public void UnloadMenu ()
		{
			MainWindow window = App.Current.GUIToolkit.MainController as MainWindow;
			MenuItem fileMenu = ((MenuItem)window.GetUIManager ().GetWidget (window.FileMenuEntry.MenuName));
			window.FileMenuEntry.ResetMenuEntry ();
			CleanMenu (fileMenu);
		}
	}
}
