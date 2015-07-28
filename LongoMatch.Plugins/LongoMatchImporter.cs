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
using System;
using System.Collections.Generic;
using System.Linq;
using LongoMatch;
using LongoMatch.Addins.ExtensionPoints;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Services;
using Mono.Addins;


namespace LongoMatch.Plugins
{
	[Extension]
	public class LongoMatchImporter: ILongoMatchPlugin, IImportProject
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
				return Catalog.GetString ("LongoMatch Importer");
			}
		}

		#endregion

		#region IImportProject implementation

		public Project ImportProject ()
		{
			List<ProjectImporter> importers = new List<ProjectImporter>();
			var ToolsManagerImporters = ((ToolsManager)CoreServices.ProjectsImporter).ProjectImporters;
			string extension = "*" + LongoMatch.Core.Common.Constants.PROJECT_EXT;
			IEnumerable<ProjectImporter> longoMatchImporters = 
				ToolsManagerImporters.Where(p => p.Internal == true && p.Extensions.Contains (extension));
			foreach (ProjectImporter LMimporter in longoMatchImporters) {
				importers.Add(LMimporter);
			}
			ProjectImporter importer;
			if (importers.Count () == 0) {
				throw new Exception (Catalog.GetString ("Plugin not found"));
			} else if (importers.Count () == 1) {
				importer = importers.First ();
			} else {
				importer = ChooseImporter (importers);
			}

			if (importer == null) {
				throw new ImportException (Catalog.GetString("Error opening importer"));
			}

			return importer.ImportFunction ();
		}

		public string FilterName {
			get {
				return LongoMatch.Core.Common.Constants.PROJECT_NAME;
			}
		}

		public string[] FilterExtensions { 
			get {
				return new string[] {"*" + LongoMatch.Core.Common.Constants.PROJECT_EXT};
			}
		}

		public bool NeedsEdition {
			get {
				return false;
			}
		}

		public bool CanOverwrite {
			get {
				return true;
			}
		}

		public bool Internal {
			get {
				return false;
			}
		}

		#endregion

		ProjectImporter ChooseImporter (IEnumerable<ProjectImporter> importers)
		{
			Dictionary<string, object> options = importers.ToDictionary (i => i.Description, i => (object)i);
			return (ProjectImporter)Config.GUIToolkit.ChooseOption (options);
		}
	}
}

