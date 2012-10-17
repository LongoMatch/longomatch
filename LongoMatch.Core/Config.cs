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
using System.IO;

using LongoMatch.Common;

namespace LongoMatch
{
	public class Config
	{
		public static string baseDirectory;
		public static string homeDirectory;
		public static string configDirectory;
		
		public static string HomeDir() {
			return homeDirectory;
		}

		public static string PlayListDir() {
			return Path.Combine(homeDirectory, "playlists");
		}

		public static string SnapshotsDir() {
			return Path.Combine(homeDirectory, "snapshots");
		}

		public static string TemplatesDir() {
			return Path.Combine(configDirectory, "templates");
		}

		public static string VideosDir() {
			return Path.Combine(homeDirectory, "videos");
		}

		public static string TempVideosDir() {
			return Path.Combine(configDirectory, "temp");
		}

		public static string ImagesDir() {
			return RelativeToPrefix(String.Format("share/{0}/images",
						Constants.SOFTWARE_NAME.ToLower()));
		}
		
		public static string PluginsDir() {
			return RelativeToPrefix(String.Format("share/{0}/plugins",
						Constants.SOFTWARE_NAME.ToLower()));
		}
		
		public static string PluginsConfigDir() {
			return Path.Combine(configDirectory, "addins");
		}

		public static string DBDir() {
			return Path.Combine(configDirectory, "db");
		}
		
		public static string RelativeToPrefix(string relativePath) {
			return Path.Combine(baseDirectory, relativePath);
		}
		
		
		/* Properties */
		public static bool useGameUnits = false;

	}
}

