// XmlUpdateParser.cs
//
//  Copyright (C) 2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using LongoMatch.IO;

namespace LongoMatch.Updates
{
	
	
	public class XmlUpdateParser
	{
		XMLReader reader;		
		Version updateVersion;
		string downloadURL;
		string oSVersion;
		
		public XmlUpdateParser(string file)			
		{
			if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
				this.oSVersion = "win32";
			else
				this.oSVersion = "unix";
			reader = new XMLReader(file);
			ParseVersion();
			ParseURL();
			
			
		}
		
		private void ParseVersion(){
			int major,minor,build;
			 
			major = reader.GetIntValue("VersionInfo","major_"+oSVersion);
			minor = reader.GetIntValue("VersionInfo","minor_"+oSVersion);
			build = reader.GetIntValue("VersionInfo","build_"+oSVersion);
			
			updateVersion = new Version(major,minor,build);
		}
		
		private void ParseURL(){
			downloadURL = reader.GetStringValue("VersionInfo","url_"+oSVersion);			
		}
		
		public string DownloadURL{
			get{return downloadURL;}
		}
		
		public Version UpdateVersion{
			get{return updateVersion;}
		}
	}
}
