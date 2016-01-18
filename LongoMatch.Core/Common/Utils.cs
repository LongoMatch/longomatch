//
//  Copyright (C) 2015 Andoni Morales Alastruey
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

namespace LongoMatch.Core.Common
{
	public class Utils
	{
		static OperatingSystemID operatingSystem = OperatingSystemID.None;

		public static string SanitizePath (string path, params char[] replaceChars)
		{
			path = path.Trim ();
			foreach (char c in Path.GetInvalidFileNameChars ()) {
				path = path.Replace (c, '_');
			}
			foreach (char c in replaceChars) {
				path = path.Replace (c, '_');
			}
			return path;
		}

		static public string SysInfo {
			get {
				return string.Format (
					"Running LongoMatch {0} build:\"{1}\" OS:\"{2}\" OS Version:\"{3}\"",
					Config.Version,
					Config.BuildVersion,
					Utils.OS,
					Environment.OSVersion.VersionString);
			}
		}

		public static OperatingSystemID OS {
			get {
				if (operatingSystem == OperatingSystemID.None) {
					#if OSTYPE_ANDROID
					operatingSystem = OperatingSystemID.Android;
					#elif OSTYPE_IOS
					operatingSystem = OperatingSystemID.iOS;
					#else
					switch (Environment.OSVersion.Platform) {
					case PlatformID.MacOSX:
						operatingSystem = OperatingSystemID.OSX;
						break;
					case PlatformID.Unix:
						// OS X is detetected as a Unix system and needs an extra check using the filesystem layout
						if (Directory.Exists ("/Applications")
						    & Directory.Exists ("/System")
						    & Directory.Exists ("/Users")
						    & Directory.Exists ("/Volumes")) {
							operatingSystem = OperatingSystemID.OSX;
						} else {
							operatingSystem = OperatingSystemID.Linux;
						}
						break;
					case PlatformID.Win32NT:
						operatingSystem = OperatingSystemID.Windows;
						break;
					default:
						throw new NotSupportedException ();
					}
					#endif
				}
				return operatingSystem;
			}
		}
	}
}

