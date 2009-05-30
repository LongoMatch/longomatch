// PlayListWidget.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using Gdk;
using LongoMatch.Video.Editor;
using Mono.Unix;
using System.IO;
using LongoMatch.Handlers;
using LongoMatch.Video.Handlers;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Player;
using LongoMatch.Video;
using LongoMatch.Gui;
using LongoMatch.Gui.Dialog;
using LongoMatch.Playlist;



namespace LongoMatch.Gui.Component
{
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayListWidget : Gtk.Bin
	{
		public event PlayListNodeSelectedHandler PlayListNodeSelected;
		public event ApplyCurrentRateHandler ApplyCurrentRate;
		public event LongoMatch.Video.Handlers.ProgressHandler Progress;		
		
		private PlayerBin player;
		private PlayListTimeNode plNode;
		private PlayList playList;
		private uint timeout;
		private object lock_node;
		private bool clock_started = false;
		private IVideoEditor videoEditor;
		
		
		public PlayListWidget()
		{
			this.Build();					
			lock_node = new System.Object();	
			PlayerMaker pm = new PlayerMaker();
			videoEditor = pm.getVideoEditor();
			videoEditor.Progress += new LongoMatch.Video.Handlers.ProgressHandler(OnProgress);
			playlisttreeview1.ApplyCurrentRate += new ApplyCurrentRateHandler(OnApplyRate);
			savebutton.Sensitive = false;			
		}
	
		public void SetPlayer(PlayerBin player){
			player = player;
			closebutton.Hide();
			newvideobutton.Hide();
		}
		
		public void Load(string filePath){
			label1.Visible = false;
			newvideobutton.Show();
			playList = new PlayList(filePath);
			Model = playList.GetModel();
			playlisttreeview1.PlayList = playList;
			playlisttreeview1.Sensitive = true;
			savebutton.Sensitive = true;
		}
		
		public ListStore Model {
			set {playlisttreeview1.Model = value;}
			get {return (ListStore)playlisttreeview1.Model;}
		}
		
		public void Add (PlayListTimeNode plNode){
			if (playList!=null){
				Model.AppendValues(plNode);
				playList.Add(plNode);
			}			
		}		
		
		public PlayListTimeNode Next(){
			if (playList.HasNext()){								
				plNode = playList.Next();
				playlisttreeview1.Selection.SelectPath(new TreePath(playList.GetCurrentIndex().ToString()));
				if (PlayListNodeSelected != null)
					PlayListNodeSelected(plNode,playList.HasNext());
				else 
					Next();
				StartClock();					
			}
			return plNode;			
		}
		
		public void Prev(){
			if ((player.AccurateCurrentTime - plNode.Start.MSeconds) < 500){
				//Seleccionando el elemento anterior si no han pasado más 500ms
				if (playList.HasPrev()){								
					plNode = playList.Prev();
					playlisttreeview1.Selection.SelectPath(new TreePath(playList.GetCurrentIndex().ToString()));
					if (PlayListNodeSelected != null)
						PlayListNodeSelected(plNode,playList.HasNext());
					StartClock();					
				}				
			}
			else 
				//Nos situamos al inicio del segmento
				player.SeekTo(plNode.Start.MSeconds,true);							
		}
		
		public void StopEdition(){
			if (videoEditor != null)
				videoEditor.Cancel();
		}
		
		public void Stop(){
			StopClock();
		}				
		
		public void StartClock ()	{
			if (player!=null && !clock_started){			
				timeout = GLib.Timeout.Add (20,CheckStopTime);
				clock_started=true;
			}
		}
		
		private void StopClock(){
			if (clock_started){
				GLib.Source.Remove(timeout);
				clock_started = false;
			}
		}

		private bool CheckStopTime(){			
			lock (lock_node){				
				if (player != null){
					if (plNode == null)
						StopClock();					
					else {						
						if (player.AccurateCurrentTime >= plNode.Stop.MSeconds)					
							Next();						
					}
				}
				return true;
			}
		}
		private PlayListTimeNode SelectPlayListNode (TreePath path){
			
			plNode = playList.Select(Int32.Parse(path.ToString()));
			if (PlayListNodeSelected != null && plNode.Valid)
				PlayListNodeSelected(plNode,playList.HasNext());	
			return plNode;
		}		
		
		private FileFilter FileFilter{
			get{
				FileFilter filter = new FileFilter();
				filter.Name = "LGM playlist";
				filter.AddPattern("*.lgm");
				return filter;
			}				
		}
		
		protected virtual void OnPlaylisttreeview1RowActivated (object o, Gtk.RowActivatedArgs args)
		{			
			playlisttreeview1.LoadedPlay = SelectPlayListNode(args.Path);	
		}
		

		protected virtual void OnSavebuttonClicked (object sender, System.EventArgs e)
		{		
			if (playList != null){
				playList.Save();
			}	
		}

		protected virtual void OnOpenbuttonClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Open playlist"),
			                                                   (Gtk.Window)this.Toplevel,
			                                                   FileChooserAction.Open,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.PlayListDir());
			fChooser.AddFilter(FileFilter);
			fChooser.DoOverwriteConfirmation = true;
			if (fChooser.Run() == (int)ResponseType.Accept)				
				Load(fChooser.Filename);				
			fChooser.Destroy();			
		}

		protected virtual void OnNewbuttonClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("New playlist"),
			                                                   (Gtk.Window)this.Toplevel,
			                                                   FileChooserAction.Save,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.PlayListDir());			
			fChooser.AddFilter(FileFilter);		
			
			if (fChooser.Run() == (int)ResponseType.Accept)
				Load(fChooser.Filename);
			fChooser.Destroy();				
		}

		protected virtual void OnPlaylisttreeview1DragEnd (object o, Gtk.DragEndArgs args)
		{			
			playList.SetModel((ListStore)playlisttreeview1.Model);
		}

		protected virtual void OnNewvideobuttonClicked (object sender, System.EventArgs e)
		{		
			VideoEditionProperties vep;
			VideoQuality vq;
			int response;
			
			vep = new VideoEditionProperties();
			vep.TransientFor = (Gtk.Window)this.Toplevel;
			response = vep.Run();
			while( response == (int)ResponseType.Ok && vep.Filename == ""){
				MessageDialog md = new MessageDialog(vep,
				                                     DialogFlags.Modal,
				                                     MessageType.Info,
				                                     ButtonsType.Ok,
				                                     Catalog.GetString("Please, select a video file."));
				md.Run();
				md.Destroy();	
				response=vep.Run();
			}
			vep.Destroy();
			if (response ==(int)ResponseType.Ok){
				vq = vep.VideoQuality;
				videoEditor.ClearList();
				foreach (PlayListTimeNode segment in playList){
					videoEditor.AddSegment(segment.FileName, 
					                       segment.Start.MSeconds, 
					                       segment.Duration.MSeconds, 
					                       segment.Rate, 
					                       segment.Name);
				}
				videoEditor.VideoQuality = vq;
				videoEditor.OutputFile = vep.Filename;
				videoEditor.Height = 480;
				videoEditor.Width = 720;
				videoEditor.Start();
				closebutton.Show();
				newvideobutton.Hide();
			}
			
		}

		protected virtual void OnClosebuttonClicked (object sender, System.EventArgs e)
		{
			videoEditor.Cancel();
			closebutton.Hide();
			newvideobutton.Show();
		}

		protected virtual void OnProgress (float progress){
			if (Progress!= null)
				Progress(progress);
			
			if (progress ==1){
				closebutton.Hide();
				newvideobutton.Show();				
			}							
		}
		
		protected virtual void OnApplyRate (PlayListTimeNode plNode){
			if (ApplyCurrentRate != null)
				ApplyCurrentRate(plNode);
		}
		
		~PlayListWidget(){
			videoEditor.Cancel();		
		}

	}
}
