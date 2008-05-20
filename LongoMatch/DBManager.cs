// DBManager.cs
//
//  Copyright (C) 2007 [name of author]
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
using System.Collections;
using Gtk;
using Mono.Unix;

namespace LongoMatch
{
	
	
	public partial class DBManager : Gtk.Dialog
	{

		
		public DBManager()
		{
			this.Build();
			this.Fill();
			
		}
		
		public void Fill(){
			ArrayList allDB = MainClass.DB.GetAllDB();
			filedatalistwidget1.Fill(allDB);
			this.filedescriptionwidget3.Clear();
			this.filedescriptionwidget3.Sensitive = false;
			this.saveButton.Sensitive = false;
			this.deleteButton.Sensitive = false;
			
		
		}


		protected virtual void OnDeleteButtonPressed (object sender, System.EventArgs e)
		{
			FileData selectedFileData = filedatalistwidget1.GetSelection();
			if (selectedFileData != null){
				if (MainWindow.OpenedFileData()!= null && selectedFileData.Equals(MainWindow.OpenedFileData())) {
				
					MessageDialog md = new MessageDialog(this,DialogFlags.Modal,MessageType.Warning,ButtonsType.Ok,
					                                     Catalog.GetString("This FileData is actually in use.\n Close it first to allow its removal from the database"));
					md.Run();				
					md.Destroy();
				}
				else {
					MessageDialog md = new MessageDialog(this,DialogFlags.Modal,MessageType.Question,ButtonsType.YesNo,
					                                     Catalog.GetString("Do yo really want to delete:\n")+selectedFileData.Filename);
					if (md.Run()== (int)ResponseType.Yes){
						this.filedescriptionwidget3.Clear();
						MainClass.DB.RemoveFileData(selectedFileData);				
						this.Fill();
						
					}
					md.Destroy();
				}
			}
		
		}
		
	


		protected virtual void OnSaveButtonPressed (object sender, System.EventArgs e)
		{
			String previousFileName;			
			FileData changedFileData;
			
			previousFileName = filedatalistwidget1.GetSelection().Filename;			
			changedFileData = this.filedescriptionwidget3.GetFileData();
			
			if (changedFileData != null){

				
				if (changedFileData.Filename == previousFileName)
					MainClass.DB.UpdateFileData(changedFileData);
				else{
					try{
						MainClass.DB.UpdateFileData(changedFileData,previousFileName);
					}
					catch{
						MessageDialog error = new MessageDialog(this,
						                                        DialogFlags.DestroyWithParent,
						                                        MessageType.Error,
						                                        ButtonsType.Ok,
						                                        "The FileData for this file already exists.\nTry to edit it.");
						error.Run();
						error.Destroy();	
					}
				}
				this.Fill();
			}
			
		}

		protected virtual void OnFiledatalistwidget1FileDataSelectedEvent (LongoMatch.FileData fData)
		{
			this.filedescriptionwidget3.Sensitive = true;
			this.filedescriptionwidget3.SetFileData(fData);
			this.saveButton.Sensitive = true;
			this.deleteButton.Sensitive = true;

		}

		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			this.Destroy();
		}

		
	}
}
