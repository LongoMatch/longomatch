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
using Mono.Addins;

using LongoMatch;
using LongoMatch.Addins.ExtensionPoints;

[assembly:AddinRoot ("LongoMatch", "1.0")]

namespace LongoMatch.Addins
{
	public class AddinsManager
	{
		public AddinsManager (string addinsPath)
		{
			Log.Information("Initializing addins at path: " + addinsPath);
			AddinManager.Initialize (addinsPath);
			AddinManager.Registry.Update();
		}
		
		public void LoadConfigModifierAddins() {
			foreach (IConfigModifier configModifier in AddinManager.GetExtensionObjects<IConfigModifier> ()) {
				configModifier.ModifyConfig();
			}
		}
		
		public void LoadExportProjectAddins() {
			foreach (IExportProject exportProject in AddinManager.GetExtensionObjects<IExportProject> ()) {
			}
		}
	}
}

