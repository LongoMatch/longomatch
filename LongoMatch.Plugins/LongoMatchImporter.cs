//
//  Copyright (C) 2015 vguzman
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
using LongoMatch;
using LongoMatch.Addins.ExtensionPoints;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using Mono.Addins;
using VAS.Addins.ExtensionPoints;
using VAS.Core;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;


namespace LongoMatch.Plugins
{
	[Extension]
	public class LongoMatchImporter: IPlugin, IImportProject
	{
		public LongoMatchImporter ()
		{
		}

		#region ILongoMatchPlugin implementation

		public string Name {
			get {
				return Catalog.GetString ("LongoMatch Importer");
			}
		}

		public string Description {
			get {
				return Catalog.GetString ("Import LongoMatch project");
			}
		}

		#endregion

		#region IImportProject implementation

		public Project ImportProject ()
		{
			LMProject project = null;

			string filename = App.Current.Dialogs.OpenFile (Catalog.GetString ("Import project"), null, App.Current.HomeDir,
				                  FilterName, FilterExtensions);
			if (filename == null)
				return null;

			IBusyDialog busy = App.Current.Dialogs.BusyDialog (Catalog.GetString ("Importing project..."));
			busy.ShowSync (() => {
				project = Project.Import (filename) as LMProject;
			});
			return project;
		}

		public string FilterName {
			get {
				return Constants.PROJECT_NAME;
			}
		}

		public string[] FilterExtensions { 
			get {
				return new string[] { "*" + Constants.PROJECT_EXT };
			}
		}

		public bool NeedsEdition {
			get {
				return false;
			}
		}

		public bool CanOverwrite {
			get {
				return false;
			}
		}

		#endregion
	}
}

