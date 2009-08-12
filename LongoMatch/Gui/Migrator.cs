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
using System.IO;
using LongoMatch.Compat;

namespace LongoMatch.Gui.Dialog
{
	
	
	public partial class Migrator : Gtk.Dialog
	{
		DatabaseMigrator dbMigrator;
		PlayListMigrator plMigrator;
		bool plFinished;
		bool dbFinished;
			
		public Migrator(string oldHomeFolder)
		{
			this.Build();
			
			CheckDataBase(oldHomeFolder);
			CheckPlayLists(oldHomeFolder);
		}
			    
		private void CheckDataBase(string oldHomeFolder){
			string oldDBFile = System.IO.Path.Combine(oldHomeFolder,"db/db.yap");
			if (File.Exists(oldDBFile)){
				dbMigrator = new DatabaseMigrator(oldDBFile);
				dbMigrator.ConversionProgressEvent += new ConversionProgressHandler(OnDBProgress);
				dbMigrator.Start();		
			}
			else{
				dbtextview.Buffer.Text = "No database to import";
				dbFinished = true;
			} 
		}
		
		private void CheckPlayLists(string oldHomeFolder){
			string[] playlistFiles;
			
			playlistFiles = Directory.GetFiles(System.IO.Path.Combine(oldHomeFolder,"playlists"),"*.lgm");
			if (playlistFiles.Length != 0){
				plMigrator = new PlayListMigrator(playlistFiles);
				plMigrator.ConversionProgressEvent += new ConversionProgressHandler (OnPLProgress);
				plMigrator.Start();
			}
			else {
				pltextview.Buffer.Text = "No playlists to import";
				plFinished = true;
			}
		}
		
		protected void OnDBProgress (string progress){
			dbtextview.Buffer.Text+=progress+"\n";
			if (progress == DatabaseMigrator.DONE){
				dbFinished = true;
				if (dbFinished && plFinished){
					buttonCancel.Visible=false;
					buttonOk.Visible=true;
				}
			}
		}
		
		protected void OnPLProgress (string progress){
			pltextview.Buffer.Text+=progress+"\n";
			if (progress == PlayListMigrator.DONE){
				plFinished = true;
				if (dbFinished && plFinished){
					buttonCancel.Visible=false;
					buttonOk.Visible=true;
				}			
			}
		}
		
		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			dbMigrator.Cancel();
		}	
	}
}
