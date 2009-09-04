// 
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;

namespace LongoMatch.Gui.Dialog
{
	
	
	public partial class WorkspaceChooser : Gtk.Dialog
	{
		
		public WorkspaceChooser()
		{
			string defaultFolder;
			this.Build();
			defaultFolder = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal),
			                                       "../../");
			filechooserbutton1.SetCurrentFolder(defaultFolder);
		}
		
		public string WorkspaceFolder{
			get{return filechooserbutton1.CurrentFolder;}
		}
	}
}
