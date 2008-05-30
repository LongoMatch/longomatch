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
			this.PopulateMenuBar();
			playerbin1.SetLogo(MainClass.ImagesDir()+"background.png");
			player = playerbin1.Player ;
			player.LogoMode = true;
			this.playerbin1.PlayListSegmentDoneEvent += new CesarPlayer.PlayListSegmentDoneHandler(OnPlayListSegmentDone);
		}

		

		private void SetFileData(FileData fData){			
			openedFileData = fData;
			if (fData!=null){				
				this.Title = System.IO.Path.GetFileNameWithoutExtension(fData.Filename) + " - LongoMatch";
				this.ShowWidgets();
			    playerbin1.SetFile(fData.Filename);				
				buttonswidget1.SetSections(fData.Sections);
				treewidget1.Model=fData.GetModel();	
				//timeprecisionadjustwidget1.Reset();
				player.LogoMode = false;
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
				
			
		private void PopulateMenuBar(){

            Gtk.UIManager w1 = new Gtk.UIManager();
            Gtk.ActionGroup w2 = new Gtk.ActionGroup("Default");
            Gtk.Action w3 = new Gtk.Action("File", Mono.Unix.Catalog.GetString("File"), null, null);
            w3.ShortLabel = Mono.Unix.Catalog.GetString("File");
            w2.Add(w3, null);
            Gtk.Action w4 = new Gtk.Action("View", Mono.Unix.Catalog.GetString("View"), null, null);
            w4.ShortLabel = Mono.Unix.Catalog.GetString("View");
            w2.Add(w4, null);
            Gtk.Action w5 = new Gtk.Action("Tools", Mono.Unix.Catalog.GetString("Tools"), null, null);
            w5.ShortLabel = Mono.Unix.Catalog.GetString("Tools");
            w2.Add(w5, null);
            Gtk.Action w6 = new Gtk.Action("Open", Mono.Unix.Catalog.GetString("Open"), null, "gtk-open");
            w6.ShortLabel = Mono.Unix.Catalog.GetString("Open");
            w2.Add(w6, null);
            Gtk.Action w7 = new Gtk.Action("New", Mono.Unix.Catalog.GetString("New"), null, "gtk-new");
            w7.ShortLabel = Mono.Unix.Catalog.GetString("New");
            w2.Add(w7, null);
            Gtk.Action w8 = new Gtk.Action("Close", Mono.Unix.Catalog.GetString("Close"), null, "gtk-close");
            w8.ShortLabel = Mono.Unix.Catalog.GetString("Close");
            w2.Add(w8, null);
            Gtk.Action w9 = new Gtk.Action("DatabaseManager", Mono.Unix.Catalog.GetString("Database Manager"), null, null);
            w9.ShortLabel = Mono.Unix.Catalog.GetString("Database Manager");
            w2.Add(w9, null);
            Gtk.Action w10 = new Gtk.Action("SectionsTemplatesManager", Mono.Unix.Catalog.GetString("Sections Templates Manager"), null, null);
            w10.ShortLabel = Mono.Unix.Catalog.GetString("Sections Templates Manager");
            w2.Add(w10, null);
            Gtk.ToggleAction w11 = new Gtk.ToggleAction("ViewPlayList", Mono.Unix.Catalog.GetString("View Play List"), null, null);
            w11.ShortLabel = Mono.Unix.Catalog.GetString("View Play List");
            w2.Add(w11, null);
            Gtk.ToggleAction w12 = new Gtk.ToggleAction("ViewButtonsBar", Mono.Unix.Catalog.GetString("View Buttons Bar"), null, null);
            w12.ShortLabel = Mono.Unix.Catalog.GetString("View Buttons Bar");
            w2.Add(w12, null);
            w1.InsertActionGroup(w2, 0);
            this.AddAccelGroup(w1.AccelGroup);
            // Container child mainvbox.Gtk.Box+BoxChild
            w1.AddUiFromString("<ui><menubar name='menubar1'><menu action='File'><menuitem action='Open'/><menuitem action='New'/><menuitem action='Close'/></menu><menu action='View'><menuitem action='ViewPlayList'/><menuitem action='ViewButtonsBar'/></menu><menu action='Tools'><menuitem action='DatabaseManager'/><menuitem action='SectionsTemplatesManager'/></menu></menubar></ui>");
            Gtk.MenuBar w14 = ((Gtk.MenuBar)(w1.GetWidget("/menubar1")));

            w14.Name = "menubar1";
            this.menubox.Add(w14);
			
			
			w6.Activated += new EventHandler(OnOpenActivated);
			w7.Activated += new EventHandler(OnNewActivated);
            w8.Activated += new EventHandler(OnCloseActivated);
            w9.Activated += new EventHandler(OnDatabaseManagerActivated);
            w10.Activated += new EventHandler(OnSectionsTemplatesManagerActivated);
            w11.Activated += new EventHandler(OnViewPlaylistActivated);
           // w12.Activated += new EventHandler(OnViewButtonsBarActivated);
    

			
			
		}

		protected virtual void OnUnrealized(object sender, System.EventArgs e){
			this.Destroy();			
			Application.Quit();					
		}

		
			


				
		protected virtual void OnNewMark(int i, int startTime, int stopTime){
			if (player != null && openedFileData != null){
				long pos = player.CurrentTime;
				long start = pos - startTime;
				long stop = pos + stopTime;
				long fStart = (start<0) ? 0 : start;
				//La longitud tiene que ser en ms
				long fStop = (stop > player.Length*1000) ? player.Length : stop;
				TimeNode tn = openedFileData.AddTimeNode(i,fStart,fStop);			
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
			this.HideWidgets();
			this.player.Close();
			this.playerbin1.UnSensitive();
			this.player.LogoMode = true;
			openedFileData = null;
			
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
			
			//this.timeprecisionadjustwidget1.Show();
			//this.timeprecisionadjustwidget1.SetTimeNode(tNode);
			this.buttonswidget1.Hide();
			this.timeline2.Enabled = false;
			this.timeline2.SetTimeNode(tNode,25);
			this.playerbin1.SetStartStop(tNode.Start,tNode.Stop);
			this.timeline2.Enabled = true;
			
			
			
		}

		protected virtual void OnTimeScaleTimeNodeChanged (LongoMatch.TimeNode tNode, object val)
		{
			if (this.playerbin1.Player.Playing)
				this.playerbin1.Player.Pause();
			if ((long)val == tNode.Start)
					this.playerbin1.UpdateSegmentStartTime((long)val);
				else
					this.playerbin1.UpdateSegmentStopTime((long)val);
			//this.timeprecisionadjustwidget1.SetTimeNode(tNode);
		
		}
		protected virtual void OnTimeNodeChanged (LongoMatch.TimeNode tNode, object val)
		{
			//Si hemos modificado el valor de un nodo de tiempo a través del 
			//widget de ajuste de tiempo posicionamos el reproductor en el punto
			//
			if (val is long ){
				long pos = (long)val;
				this.player.Pause();
				if (pos == tNode.Start){
					this.playerbin1.UpdateSegmentStartTime(pos);
					this.timeline2.UpdateStartTime(pos);
				}
				
				else{
					this.playerbin1.UpdateSegmentStopTime(pos);
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
			this.playlistwidget2.AddPlayListNode(new PlayListNode(openedFileData.Filename,tNode));
		}

		protected virtual void OnPlaylistwidget2PlayListNodeSelected (LongoMatch.PlayListNode plNode, bool hasNext)
		{
			//Hay que seleccionar tb el archivo
			Console.WriteLine("{0}   {1}   {2}   ",plNode.FileName,plNode.StartTime,plNode.StopTime);
			this.playerbin1.SetPlayListElement(plNode.FileName,plNode.StartTime,plNode.StopTime,hasNext);

		}
		
		protected virtual void OnPlayListSegmentDone ()
		{
			
			
			
			//playlistwidget2.playNext();
		}
		
		protected virtual void OnViewPlaylistActivated (object sender, System.EventArgs e){
			this.playlistwidget2.Visible = !this.playlistwidget2.Visible;
		}
		

		

		protected virtual void OnPlayerbin1TickEvent (long currentTime, long streamLength, float position, bool seekable)
		{
			if (this.timeline2.Enabled)
				this.timeline2.SetPosition(currentTime);
		}

		protected virtual void OnPlayerbin1SegmentClosedEvent ()
		{
			//this.timeprecisionadjustwidget1.Reset();
		    //this.timeprecisionadjustwidget1.Hide();
			this.buttonswidget1.Show();
			this.timeline2.Enabled = false;
		}

		protected virtual void OnTimeline2PositionChanged (long pos)
		{
			this.player.SeekInSegment(pos);
		}

	
	


			
	}
}