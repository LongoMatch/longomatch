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


namespace LongoMatch
{
	
   
 
	
	public partial class MainWindow : Gtk.Window
	{

		private static FileData openedFileData;
		private CesarPlayer.IPlayer player;

	
 
		
		
		public MainWindow() : 
				base("LongoMatch")
		{			
			this.Build();
			//this.PopulateMenuBar();
			playerbin1.SetLogo(MainClass.ImagesDir()+"background.png");
			player = playerbin1.Player ;
			player.LogoMode = true;
			this.playerbin1.PlayListSegmentDoneEvent += new CesarPlayer.PlayListSegmentDoneHandler(OnPlayListSegmentDone);
			this.playlistwidget2.SetPlayer(player);
		}

		

		private void SetFileData(FileData fData){			
			openedFileData = fData;
			
			
			if (fData!=null){		
				if(!File.Exists(fData.Filename)){
					MessageDialog infoDialog = new MessageDialog (this,DialogFlags.Modal,MessageType.Warning,ButtonsType.Ok,Catalog.GetString("The file associated to this proyect doesn't exits.\n If the location of the file has changed try to change it with de DataBase Manager.") );
					infoDialog.Run();
					infoDialog.Destroy();
					
				}
				else {
					this.Title = System.IO.Path.GetFileNameWithoutExtension(fData.Filename) + " - LongoMatch";
					this.ShowWidgets();
					playerbin1.File=fData.Filename;				
					buttonswidget1.SetSections(fData.Sections);
					treewidget1.Model=fData.GetModel();	
					//timeprecisionadjustwidget1.Reset();
					player.LogoMode = false;
				}
			}			
		}
		
		public static FileData OpenedFileData(){			
			return openedFileData;
		}
		
		private void ShowWidgets(){
			this.buttonswidget1.Show();
			this.leftbox.Show();
		}
		
		private void HideWidgets(){
			buttonswidget1.Hide();
			this.leftbox.Hide();			
		}
				
	    private void CloseActualProyect(){
			this.Title = "LongoMatch";
			this.HideWidgets();
			this.player.Close();
			this.playerbin1.UnSensitive();
			this.player.LogoMode = true;
			openedFileData = null;			
		}
		

		protected virtual void OnUnrealized(object sender, System.EventArgs e){
			this.Destroy();			
			Application.Quit();					
		}

		
			


				
		protected virtual void OnNewMark(int i, Time startTime, Time stopTime){
			if (player != null && openedFileData != null){

				long pos = player.CurrentTime;
				long start = pos - startTime.MSeconds;
				long stop = pos + stopTime.MSeconds;
				long fStart = (start<0) ? 0 : start;
				//La longitud tiene que ser en ms
				long fStop = (stop > player.Length*1000) ? player.Length : stop;
				TimeNode tn = openedFileData.AddTimeNode(i,new Time((int)fStart),new Time((int)fStop));			
				treewidget1.AddTimeNode(tn,i);							
				MainClass.DB.UpdateFileData(openedFileData);
			}
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

		protected virtual void OnCloseActivated (object sender, System.EventArgs e)
		{
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

		protected virtual void OnTimeNodeSelected (LongoMatch.TimeNode tNode)
		{
			
			
			this.buttonswidget1.Hide();
			this.timeline2.Enabled = false;
			this.timeline2.SetTimeNode(tNode,25);

			this.playerbin1.SetStartStop(tNode.Start.MSeconds,tNode.Stop.MSeconds);
			this.timeline2.Enabled = true;
			
			
			
		}

		
		protected virtual void OnTimeNodeChanged (LongoMatch.TimeNode tNode, object val)
		{
			//Si hemos modificado el valor de un nodo de tiempo a través del 
			//widget de ajuste de tiempo posicionamos el reproductor en el punto
			//
			if (val is Time ){
				
				Time pos = (Time)val;
				Console.WriteLine(pos.MSeconds);
				this.player.Pause();
				if (pos == tNode.Start){
					this.playerbin1.UpdateSegmentStartTime(pos.MSeconds);
					this.timeline2.UpdateStartTime(pos);
				}
				
				else{
					this.playerbin1.UpdateSegmentStopTime(pos.MSeconds);
					this.timeline2.UpdateStopTime(pos);
				}	
			}
			
			//Si modificamos un padre actualizamos los nombres de los botones
			if (tNode.IsRoot()){
				this.buttonswidget1.SetNames(openedFileData.GetSectionsNames());
			}
				MainClass.DB.UpdateFileData(openedFileData);

		}

		protected virtual void OnTimeNodeDeleted (LongoMatch.TimeNode tNode)
		{
			openedFileData.DelTimeNode(tNode);
			MainClass.DB.UpdateFileData(openedFileData);
		}


		protected virtual void OnDeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			this.playerbin1.Dispose();
			Application.Quit();
			
		}

		protected virtual void OnPlayListNodeAdded (LongoMatch.TimeNode tNode)
		{
			this.playlistwidget2.Add(new PlayListNode(openedFileData.Filename,tNode));
		}

		protected virtual void OnPlaylistwidget2PlayListNodeSelected (LongoMatch.PlayListNode plNode, bool hasNext)
		{
			
			this.playerbin1.SetPlayListElement(plNode.FileName,plNode.StartTime.MSeconds,plNode.StopTime.MSeconds,hasNext);

		}
		
		protected virtual void OnPlayListSegmentDone ()
		{	
			playlistwidget2.Next();
		}
		
		
		

		

		protected virtual void OnPlayerbin1TickEvent (long currentTime, long streamLength, float position, bool seekable)
		{
			if (this.timeline2.Enabled)
				this.timeline2.SetPosition(new Time ((int)(currentTime)));
		}

		protected virtual void OnPlayerbin1SegmentClosedEvent ()
		{
			//this.timeprecisionadjustwidget1.Reset();
		    //this.timeprecisionadjustwidget1.Hide();
			this.buttonswidget1.Show();
			this.timeline2.Enabled = false;
		}

		protected virtual void OnTimeline2PositionChanged (Time pos)
		{
			this.player.SeekInSegment(pos.MSeconds);
		}

		protected virtual void OnPlayerbin1ErrorEvent (string error)
		{
			MessageDialog errorDialog = new MessageDialog (this,DialogFlags.Modal,MessageType.Error,ButtonsType.Ok,Catalog.GetString("The actual Proyect will bo closed due to this error on the media player:\n") +error);
			errorDialog.Run();
			errorDialog.Destroy();	
			this.CloseActualProyect();
		}

		protected virtual void OnQuitActivated (object sender, System.EventArgs e)
		{
			Application.Quit();
		}


		protected virtual void OnPlaylistActionToggled (object sender, System.EventArgs e)
		{			
				this.playlistwidget2.Visible=((Gtk.ToggleAction)sender).Active;			
		}

		

		

	
	


			
	}
}