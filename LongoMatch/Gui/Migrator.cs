// 
//  Copyright (C) 2009 Andoni Morales Alastruey
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
using LongoMatch.Compat;

namespace LongoMatch.Gui.Dialog
{
	
	
	public partial class Migrator : Gtk.Dialog
	{
		DatabaseMigrator dbMigrator;

			
		public Migrator(string oldDBFile)
		{
			this.Build();
			dbMigrator = new DatabaseMigrator(oldDBFile);
			dbMigrator.ConversionProgressEvent += new ConversionProgressHandler(OnProgress);
			dbMigrator.Start();			
		}
		
		protected void OnProgress (string progress){
			textview2.Buffer.Text+=progress+"\n";
			if (progress == DatabaseMigrator.DONE){
				buttonCancel.Visible=false;
				buttonOk.Visible=true;
			}
		}
		
		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			dbMigrator.Cancel();
		}	
	}
}
