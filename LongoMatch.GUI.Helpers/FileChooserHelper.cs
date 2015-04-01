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
using Gtk;
using System.Collections.Generic;

namespace LongoMatch.Gui.Helpers
{
	public class FileChooserHelper
	{
	
		static public string SaveFile (Widget parent, string title, string defaultName,
		                               string defaultFolder, string filterName,
		                               string[] extensions)
		{
			return FileChooser (parent, title, defaultName, defaultFolder, filterName,
				extensions, FileChooserAction.Save);
		}

		static public string SelectFolder (Widget parent, string title, string defaultName,
		                                   string defaultFolder, string filterName,
		                                   string[] extensions)
		{
			return FileChooser (parent, title, defaultName, defaultFolder, filterName,
				extensions, FileChooserAction.SelectFolder);
		}

		static public string OpenFile (Widget parent, string title, string defaultName,
		                               string defaultFolder, string filterName,
		                               string[] extensions)
		{
			return FileChooser (parent, title, defaultName, defaultFolder, filterName,
				extensions, FileChooserAction.Open);
		}

		static public List<string> OpenFiles (Widget parent, string title, string defaultName,
		                                      string defaultFolder, string filterName,
		                                      string[] extensions)
		{
			return MultiFileChooser (parent, title, defaultName, defaultFolder, filterName,
				extensions, FileChooserAction.Open);
		}

		static string FileChooser (Widget parent, string title, string defaultName,
		                           string defaultFolder, string filterName,
		                           string[] extensions, FileChooserAction action)
		{
			List<string> res = MultiFileChooser (parent, title, defaultName, defaultFolder,
				                   filterName, extensions, action, false);
			if (res.Count == 1)
				return res [0];
			return null;
		}

		static List<string>  MultiFileChooser (Widget parent, string title, string defaultName,
		                                       string defaultFolder, string filterName,
		                                       string[] extensions, FileChooserAction action,
		                                       bool allowMultiple = true)
		{
			Window toplevel;
			FileChooserDialog fChooser;
			FileFilter filter;
			string button;
			List<string> path;
			
			if (action == FileChooserAction.Save)
				button = "gtk-save";
			else
				button = "gtk-open";
			
			if (parent != null)
				toplevel = parent.Toplevel as Window;
			else
				toplevel = null;
				
			fChooser = new FileChooserDialog (title, toplevel, action,
				"gtk-cancel", ResponseType.Cancel, button, ResponseType.Accept);
				
			fChooser.SelectMultiple = allowMultiple;
			if (defaultFolder != null) {
				fChooser.SetCurrentFolder (defaultFolder);
			} else if (Config.LastDir != null) {
				fChooser.SetCurrentFolder (Config.LastDir);
			}
			if (defaultName != null)
				fChooser.CurrentName = defaultName;
			if (filterName != null) {
				filter = new FileFilter ();
				filter.Name = filterName;
				if (extensions != null) {
					foreach (string p in extensions) {
						filter.AddPattern (p);
					}
				}
				fChooser.Filter = filter;
			}
			
			if (fChooser.Run () != (int)ResponseType.Accept) {
				path = new List<string> ();
			} else {
				path = new List<string> (fChooser.Filenames);
				if (defaultFolder == null && fChooser.Filenames.Length > 0) {
					Config.LastDir = System.IO.Path.GetDirectoryName (fChooser.Filenames [0]);
				}
			}

			fChooser.Destroy ();
			return path;
		}
	}
}

