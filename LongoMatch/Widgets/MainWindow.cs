// MainWindow.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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
using Gtk;
using Mono.Unix;
using System.IO;
using GLib;
using System.Threading;
using Gdk;


namespace LongoMatch
{	
	public partial class MainWindow : Gtk.Window
	{
		private static  FileData openedFileData;
		private CesarPlayer.IPlayer player;
		private TimeNode selectedTimeNode;
		bool fileDataModified;	

		
		public MainWindow() : 
				base("LongoMatch")
		{			
			this.Build();
			playerbin1.SetLogo(MainClass.ImagesDir()+"background.png");
			player = playerbin1.Player ;
			player.LogoMode = true;
			this.playlistwidget2.SetPlayer(player);


		}

		

		private void SetFileData(FileData fData){			
			openedFileData = fData;			
			if (fData!=null){		
				if(!File.Exists(fData.File.FilePath)){
					MessageDialog infoDialog = new MessageDialog (this,DialogFlags.Modal,MessageType.Warning,ButtonsType.Ok,Catalog.GetString("The file associated to this proyect doesn't exits.\n If the location of the file has changed try to change it with de DataBase Manager.") );
					infoDialog.Run();
					infoDialog.Destroy();
					this.CloseActualProyect();					
				}
				else {
					
					this.Title = System.IO.Path.GetFileNameWithoutExtension(fData.File.FilePath) + " - LongoMatch";
					playerbin1.File=fData.File.FilePath;
					this.playlistwidget2.Stop();
					
					treewidget1.FileData=fData;						
					this.timelinewidget1.FileData = fData;
					this.buttonswidget1.SetSections(fData.Sections);	
					if (fData.File.HasVideo){
						player.LogoMode = false;
						this.FullScreenAction.Sensitive = true;
					}
					this.CloseProjectAction.Sensitive=true;
					this.SaveProjectAction.Sensitive = true;
					this.PlayerAction.Sensitive= true;
					this.CaptureModeAction.Sensitive = true;
					this.AnalyzeModeAction.Sensitive = true;
				
					this.ShowWidgets();
				}
			}			
		}
		
		public static FileData OpenedFileData(){			
			return openedFileData;
		}
		
		private void ShowWidgets(){
			this.leftbox.Show();
			if (this.CaptureModeAction.Active)
				this.buttonswidget1.Show();
			else 
				this.timelinewidget1.Show();
		}
		
		private void HideWidgets(){
			this.leftbox.Hide();
			this.buttonswidget1.Hide();
			this.timelinewidget1.Hide();
		}
				
	    private void CloseActualProyect(){
			this.Title = "LongoMatch";
			this.HideWidgets();
			this.playerbin1.Close();			
			this.player.LogoMode = true;
			this.SaveDB();			
			openedFileData = null;	
			this.selectedTimeNode = null;
			this.CloseProjectAction.Sensitive=false;
			this.SaveProjectAction.Sensitive = false;
			this.PlayerAction.Sensitive= false;
			this.CaptureModeAction.Sensitive = false;
			this.AnalyzeModeAction.Sensitive = false;
			this.FullScreenAction.Sensitive = false;
		}
		
		private void SaveDB(){			
			if (openedFileData != null){
				MainClass.DB.UpdateFileData(OpenedFileData());
				this.fileDataModified=false;
			}
			
		}
		
		protected virtual void OnUnrealized(object sender, System.EventArgs e){
			this.Destroy();			
			Application.Quit();					
		}
		
		
		protected virtual void OnSectionsTemplatesManagerActivated (object sender, System.EventArgs e)
		{
			SectionsTemplates st = new SectionsTemplates();
			st.Show();
		}

		protected virtual void OnOpenActivated (object sender, System.EventArgs e)
		{
			FileData fData;
			OpenProjectDialog opd = new OpenProjectDialog();
			int answer=opd.Run();
			while (answer == (int)ResponseType.Reject){
				fData = opd.GetSelection();
				MainClass.DB.RemoveFileData(fData);
				opd.Fill();
				answer=opd.Run();
			}
			if (answer == (int)ResponseType.Ok){
				fData = opd.GetSelection();
				this.SetFileData(fData);
			}
			opd.Destroy();
		}

		protected virtual void OnNewActivated (object sender, System.EventArgs e)
		{
			FileData fData;
			NewProjectDialog npd = new NewProjectDialog();
			// Esperamos a que se pulse el boton aceptar y se cumplan las condiciones para 
			// crear un nuevo objeto del tipo FileData
			int response = npd.Run();
			while (response == (int)ResponseType.Ok && npd.GetFileData() == null){
				MessageDialog md = new MessageDialog(npd,
				                                     DialogFlags.DestroyWithParent,
				                                     MessageType.Info,
				                                     ButtonsType.Ok,
				                                     Catalog.GetString("Please, select a video file."));
				md.Run();
				md.Destroy();	
				response=npd.Run();
			}
			npd.Destroy();
			// Si se cumplen las condiciones y se ha pulsado el botón aceptar continuamos
			if (response ==(int)ResponseType.Ok){
				fData = npd.GetFileData();
				if (fData != null){
					try{
						MainClass.DB.AddFileData(fData);
						this.SetFileData(fData);
					}
					catch {						
						MessageDialog error = new MessageDialog(this,
						                                        DialogFlags.DestroyWithParent,
						                                        MessageType.Error,
						                                        ButtonsType.Ok,
						                                        "The FileData for this file already exists.\nTry to edit it.");
						error.Run();
						error.Destroy();							
					}
				}
			}
		}

		protected virtual bool UpdateFileData(){
			if (fileDataModified && openedFileData != null){
				System.Threading.Thread thread = new System.Threading.Thread(new ThreadStart(SaveDB));				                                            
				thread.Start();
			}
			return true;
		}
		
		protected virtual void OnCloseActivated (object sender, System.EventArgs e)
		{
			// FIXME Ask to Save the Project if it has changed
			this.SaveDB();
			this.CloseActualProyect();			
		}

		protected virtual void OnDatabaseManagerActivated (object sender, System.EventArgs e)
		{
			DBManager db = new DBManager();
			db.Show();
		}		

		protected virtual void OnTimeprecisionadjustwidget1SizeRequested (object o, Gtk.SizeRequestedArgs args)
		{
			if (args.Requisition.Width>= hpaned.Position)
				hpaned.Position = args.Requisition.Width;
		}
		
		protected virtual void OnNewMark(int i, Time startTime, Time stopTime){
			if (player != null && openedFileData != null){
				long pos = player.CurrentTime;
				long start = pos - startTime.MSeconds;
				long stop = pos + stopTime.MSeconds;
				long fStart = (start<0) ? 0 : start;
				//La longitud tiene que ser en ms
				long fStop = (stop > player.StreamLength*1000) ? player.StreamLength: stop;
				Pixbuf miniature = this.playerbin1.CurrentThumbnail;
				MediaTimeNode tn = openedFileData.AddTimeNode(i,new Time((int)fStart),new Time((int)fStop),miniature);				
				treewidget1.AddTimeNode(tn,i);
				this.fileDataModified = true;
				this.timelinewidget1.QueueDraw();
			}
		}

		protected virtual void OnTimeNodeSelected (MediaTimeNode tNode)
		{			
			this.selectedTimeNode = tNode;			
			this.timelinewidget1.SelectedTimeNode = tNode;
			this.playerbin1.SetStartStop(tNode.Start.MSeconds,tNode.Stop.MSeconds);		
		}

		
		protected virtual void OnTimeNodeChanged (LongoMatch.TimeNode tNode, object val)
		{
			//Si hemos modificado el valor de un nodo de tiempo a través del 
			//widget de ajuste de tiempo posicionamos el reproductor en el punto
			//
			if (tNode is MediaTimeNode && val is Time ){	
				if(tNode != selectedTimeNode)
					this.OnTimeNodeSelected((MediaTimeNode)tNode);
				Time pos = (Time)val;
				this.player.Pause();
				if (pos == tNode.Start){
					
					this.playerbin1.UpdateSegmentStartTime(pos.MSeconds);
				}				
				else{
					this.playerbin1.UpdateSegmentStopTime(pos.MSeconds);
				}	
			}	
			else if (tNode is SectionsTimeNode){
				this.buttonswidget1.SetSections(openedFileData.Sections);
			}
			this.fileDataModified = true;			
		}

		protected virtual void OnTimeNodeDeleted (LongoMatch.MediaTimeNode tNode)
		{
			openedFileData.DelTimeNode(tNode);		
			this.fileDataModified = true;
			this.timelinewidget1.QueueDraw();
		}


		protected virtual void OnDeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			this.SaveDB();
			this.playerbin1.Dispose();
			Application.Quit();
					
		}

		protected virtual void OnPlayListNodeAdded (LongoMatch.MediaTimeNode tNode)
		{
			this.playlistwidget2.Add(new PlayListTimeNode(openedFileData.File.FilePath,tNode));
		}

		protected virtual void OnPlaylistwidget2PlayListNodeSelected (LongoMatch.PlayListTimeNode plNode, bool hasNext)
		{
			if (openedFileData == null){
				this.selectedTimeNode = plNode;
				this.playerbin1.SetPlayListElement(plNode.FileName,plNode.Start.MSeconds,plNode.Stop.MSeconds,hasNext);
			}
		}
		
		protected virtual void OnPlayListSegmentDone ()
		{	
			playlistwidget2.Next();
		}

		protected virtual void OnPlayerbin1SegmentClosedEvent ()
		{
			this.selectedTimeNode = null;
			this.timelinewidget1.SelectedTimeNode = null;
		}

		protected virtual void OnTimeline2PositionChanged (Time pos)
		{
			this.player.SeekInSegment(pos.MSeconds);
		}		

		protected virtual void OnQuitActivated (object sender, System.EventArgs e)
		{
			// FIXME Ask to Save the Project if it has changed
			this.UpdateFileData();
			this.playerbin1.Destroy();
			Application.Quit();
		}

		protected virtual void OnPlaylistActionToggled (object sender, System.EventArgs e)
		{			
				this.playlistwidget2.Visible=((Gtk.ToggleAction)sender).Active;			
		}

		protected virtual void OnOpenPlaylistActionActivated (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Open playlist"),
			                                                   null,
			                                                   FileChooserAction.Open,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.PlayListDir());
			FileFilter filter = new FileFilter();
			filter.Name = "LGM playlist";
			filter.AddPattern("*.lgm");
			
			fChooser.AddFilter(filter);
			if (fChooser.Run() == (int)ResponseType.Accept){
				if (openedFileData != null)
					this.CloseActualProyect();
				this.playlistwidget2.Load(fChooser.Filename);				
				this.PlaylistAction.Active = true;				
			}		
			fChooser.Destroy();			
		}

		protected virtual void OnPlayerbin1Error (object o, CesarPlayer.ErrorArgs args)
		{
			MessageDialog errorDialog = new MessageDialog (this,DialogFlags.Modal,MessageType.Error,ButtonsType.Ok,Catalog.GetString 
			                                               ("The actual Proyect will bo closed due to this error on the media player:\n") +args.Message);
			errorDialog.Run();
			errorDialog.Destroy();	
			this.CloseActualProyect();
		}

		protected virtual void OnPlayerbin1Next ()
		{
			this.playlistwidget2.Next();
		}

		protected virtual void OnPlayerbin1Prev ()
		{
			if (this.selectedTimeNode is MediaTimeNode){
				this.playerbin1.Player.SeekInSegment(this.selectedTimeNode.Start.MSeconds);
				player.Play();
			}
			else if (this.selectedTimeNode is PlayListTimeNode)
				this.playlistwidget2.Prev();
			else if (this.selectedTimeNode == null)
				this.playerbin1.Player.SeekTo(0,false);
		}

		protected virtual void OnPlayerbin1Tick (object o, CesarPlayer.TickArgs args)
		{
			if (args.CurrentTime != 0 && this.timelinewidget1 != null && openedFileData != null)
				this.timelinewidget1.CurrentFrame=(uint)(args.CurrentTime * openedFileData.File.Fps / 1000);
		}

		protected virtual void OnCaptureModeActionToggled (object sender, System.EventArgs e)
		{

			if (((Gtk.ToggleAction)sender).Active){
				this.buttonswidget1.Show();
				this.timelinewidget1.Hide();
			}
			else{
				this.buttonswidget1.Hide();
				this.timelinewidget1.Show();
			}
			
			
		}

		protected virtual void OnAnalyzeModeActionToggled (object sender, System.EventArgs e)
		{
		}

		protected virtual void OnFullScreenActionToggled (object sender, System.EventArgs e)
		{
		
				this.playerbin1.FullScreen = ((Gtk.ToggleAction)sender).Active;
		}

		protected virtual void OnSaveProjectActionActivated (object sender, System.EventArgs e)
		{
			this.SaveDB();
		}

		
		

		
	}
}
