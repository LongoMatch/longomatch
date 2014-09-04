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
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace LongoMatch.Core.Common
{
	public class Gettext
	{
		public static List<CultureInfo> Languages {
			get {
				List<CultureInfo> langs;
				string filename, localesDir;
				
				langs = new List<CultureInfo>();
				filename = String.Format ("{0}.mo", Constants.SOFTWARE_NAME.ToLower());
				localesDir = Config.RelativeToPrefix("share/locale");
				
				langs.Add (new CultureInfo ("en"));
				
				if (!Directory.Exists (localesDir))
					return langs;
					
				foreach (string dirpath in Directory.EnumerateDirectories (localesDir)) {
					if (File.Exists (Path.Combine (dirpath, "LC_MESSAGES", filename))) {
						try {
							string localeName = Path.GetFileName(dirpath).Replace("_", "-");
							langs.Add(new CultureInfo (localeName));
						} catch (Exception ex) {
							Log.Exception (ex);
						}
					}
				}
				return langs;
			}
		}
	}
}

