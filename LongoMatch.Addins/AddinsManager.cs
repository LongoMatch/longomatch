// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.IO;
using System.Linq;
using LongoMatch.Addins.ExtensionPoints;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using Mono.Addins;
using Mono.Addins.Description;
using VAS.Core.Addins;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store.Templates;

[assembly: AddinRoot ("LongoMatch", "1.1")]
namespace LongoMatch.Addins
{
	public class AddinsManager
	{
		private static Dictionary<AddinDescription, List<ConfigurablePlugin>> pluginsDict;

		public static void Initialize (string configPath, string searchPath)
		{
			Log.Information ("Delete addins cache at " + configPath);
			try {
				Directory.Delete (configPath, true);
			} catch (Exception ex) {
			}
			searchPath = Path.GetFullPath (searchPath);
			Log.Information ("Initializing addins at path: " + searchPath);
			/* First initialization can fail after upgrades */
			try {
				AddinManager.Initialize (configPath, searchPath);
			} catch (Exception ex1) {
				Log.Exception (ex1);
				try {
					Directory.Delete (configPath, true);
				} catch (Exception ex2) {
					Log.Exception (ex2);
					/* Something can be very wrong at this point if we couldn't delete
					 * the addins cache. This can happens in windows if the directory
					 * is un use by another instance.
					 * Ignore it and try to intialize again just in case. */
				}
				AddinManager.Initialize (configPath, searchPath);
			}
			AddinManager.Registry.Update ();
			foreach (Addin addin in AddinManager.Registry.GetAddins ()) {
				string addinPath = addin.Description.AddinFile;

				if (!App.Current.Uninstalled && !addinPath.StartsWith (searchPath) &&
					!addinPath.StartsWith (Path.GetFullPath (App.Current.baseDirectory))) {
					AddinManager.Registry.DisableAddin (addin.Id);
					Log.Debug ("Disable addin at path " + addinPath);
				} else {
					AddinManager.Registry.EnableAddin (addin.Id);
				}
			}

			pluginsDict = ListPlugins ();
		}

		public static Dictionary<AddinDescription, List<ConfigurablePlugin>> Plugins {
			get { return pluginsDict; }
		}

		private static Dictionary<AddinDescription, List<ConfigurablePlugin>> ListPlugins ()
		{
			HashSet<string> paths;
			Dictionary<AddinDescription, List<ConfigurablePlugin>> plugins;

			paths = new HashSet<string> ();
			plugins = new Dictionary<AddinDescription, List<ConfigurablePlugin>> ();

			foreach (Addin addin in AddinManager.Registry.GetAddins ()) {
				if (!addin.Enabled) {
					continue;
				}
				foreach (Extension ext in addin.Description.MainModule.Extensions) {
					paths.Add (ext.Path);
				}
				plugins.Add (addin.Description, new List<ConfigurablePlugin> ());
			}

			foreach (string path in paths) {
				foreach (TypeExtensionNode n in AddinManager.GetExtensionNodes (path)) {
					var list = plugins.FirstOrDefault (a => a.Key.LocalId == n.Addin.Id).Value;
					try {
						var instance = n.GetInstance ();
						if (instance is ConfigurablePlugin) {
							list.Add ((ConfigurablePlugin)instance);
						}
					} catch (Exception ex) {
						if (ex.GetBaseException () is AddinRequestShutdownException) {
							throw ex.GetBaseException ();
						} else {
							Log.Exception (ex);
						}
					}
				}
			}
			return plugins;
		}

		public static bool ShowStats (LMProject project)
		{
			//IStatsUI statsUI = AddinManager.GetExtensionObjects<IStatsUI> ().OrderByDescending (p => p.Priority).FirstOrDefault ();
			IStatsUI statsUI = App.Current.DependencyRegistry.Retrieve<IStatsUI> ();
			if (statsUI != null) {
				statsUI.ShowStats (project);
				return true;
			}
			return false;
		}
	}
}

