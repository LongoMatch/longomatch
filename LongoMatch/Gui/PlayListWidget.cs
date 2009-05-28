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
			this.videoEditor = pm.getVideoEditor();
			this.videoEditor.Progress += new LongoMatch.Video.Handlers.ProgressHandler(OnProgress);
			this.savebutton.Sensitive = false;			
		}

	
		public void SetPlayer(PlayerBin player){
			this.player = player;
			this.closebutton.Hide();
			this.newvideobutton.Hide();
		}
		
		public void Load(string filePath){
			this.label1.Visible = false;
			this.newvideobutton.Show();
			this.playList = new PlayList(filePath);
			this.Model = playList.GetModel();
			this.playlisttreeview1.PlayList = playList;
			this.playlisttreeview1.Sensitive = true;
			this.savebutton.Sensitive = true;
		}
		
		public ListStore Model {
			set {this.playlisttreeview1.Model = value;}
			get {return (ListStore)this.playlisttreeview1.Model;}
		}
		
		public void Add (PlayListTimeNode plNode){
			if (playList!=null){
				this.Model.AppendValues(plNode);
				this.playList.Add(plNode);
			}			
		}
		
		
		public PlayListTimeNode Next(){
			if (this.playList.HasNext()){								
				this.plNode = this.playList.Next();
				this.playlisttreeview1.Selection.SelectPath(new TreePath(this.playList.GetCurrentIndex().ToString()));
				if (this.PlayListNodeSelected != null)
					this.PlayListNodeSelected(plNode,this.playList.HasNext());
				else 
					this.Next();
				this.StartClock();					
			}
			return plNode;			
		}
		
		public void Prev(){

			if ((this.player.AccurateCurrentTime - this.plNode.Start.MSeconds) < 500){
				//Seleccionando el elemento anterior si no han pasado más 500ms
				if (this.playList.HasPrev()){								
					this.plNode = this.playList.Prev();
					this.playlisttreeview1.Selection.SelectPath(new TreePath(this.playList.GetCurrentIndex().ToString()));
					if (this.PlayListNodeSelected != null)
						this.PlayListNodeSelected(plNode,this.playList.HasNext());
					this.StartClock();					
				}				
			}
			else 
				//Nos situamos al inicio del segmento
				this.player.SeekTo(plNode.Start.MSeconds,true);							
		}
		
		public void StopEdition(){
			if (this.videoEditor != null)
				this.videoEditor.Cancel();
		}
		
		public void Stop(){
			this.StopClock();
		}		
		
		
		
		public void StartClock ()	{
			if (player!=null && !clock_started){			
				timeout = GLib.Timeout.Add (20,CheckStopTime);
				clock_started=true;
			}
		}
		
		private void StopClock(){
			if (this.clock_started){
				GLib.Source.Remove(timeout);
				this.clock_started = false;
			}
		}

		private bool CheckStopTime(){
			
			lock (this.lock_node){
				
				if (player != null){
					if (plNode == null)
						this.StopClock();
					
					else {
						
						if (player.AccurateCurrentTime >= plNode.Stop.MSeconds){
					
							this.Next();
						}
					}
				}
				return true;
			}
		}
		private void SelectPlayListNode (TreePath path){
			
			this.plNode = this.playList.Select(Int32.Parse(path.ToString()));
			if (this.PlayListNodeSelected != null && plNode.Valid)
				this.PlayListNodeSelected(plNode,this.playList.HasNext());				
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
			this.SelectPlayListNode(args.Path);			
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
			fChooser.AddFilter(this.FileFilter);
			fChooser.DoOverwriteConfirmation = true;
			if (fChooser.Run() == (int)ResponseType.Accept){
				
				this.Load(fChooser.Filename);
				
			}
		
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
			fChooser.AddFilter(this.FileFilter);
		
			
			if (fChooser.Run() == (int)ResponseType.Accept){
				this.Load(fChooser.Filename);
			}
			fChooser.Destroy();
				
		}

		protected virtual void OnPlaylisttreeview1DragEnd (object o, Gtk.DragEndArgs args)
		{
			
			this.playList.SetModel((ListStore)this.playlisttreeview1.Model);
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
				foreach (PlayListTimeNode segment in playList)
					videoEditor.AddSegment(segment.FileName, 
					                       segment.Start.MSeconds, 
					                       segment.Duration.MSeconds, 
					                       segment.Rate, 
					                       segment.Name);
				videoEditor.VideoQuality = vq;
				videoEditor.OutputFile = vep.Filename;
				videoEditor.Start();
				this.closebutton.Show();
				this.newvideobutton.Hide();
			}
			
		}

		protected virtual void OnClosebuttonClicked (object sender, System.EventArgs e)
		{
			this.videoEditor.Cancel();
			this.closebutton.Hide();
			this.newvideobutton.Show();
		}

		protected virtual void OnProgress (float progress){
			if (this.Progress!= null)
				this.Progress(progress);
			
			if (progress ==1){
				this.closebutton.Hide();
				this.newvideobutton.Show();				
			}
			
		}
		
		~PlayListWidget(){
			this.videoEditor.Cancel();		
		}

	}
}
