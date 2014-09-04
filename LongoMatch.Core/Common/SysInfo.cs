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
using System.Reflection;

namespace LongoMatch.Core.Common
{
	public class SysInfo
	{
		static public string PrintInfo(Version version)
		{
			StringWriter info = new StringWriter();
			
			info.WriteLine("LongoMatch Version: {0}", version);
			info.WriteLine("Operating System: {0} - {1} ",
			               Environment.OSVersion.Platform,
			               Environment.OSVersion.VersionString);
			info.WriteLine();
			return info.ToString();
		}
	}
}

