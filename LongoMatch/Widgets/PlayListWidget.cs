// PlayListWidget.cs
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
using Gdk;
using CesarPlayer;
using Mono.Unix;
using System.IO;



namespace LongoMatch
{
	
	
	public partial class PlayListWidget : Gtk.Bin
	{
		public event PlayListNodeSelectedHandler PlayListNodeSelected;
		private IPlayer player;
		private PlayListTimeNode plNode;
		private PlayList playList;
		private uint timeout;
		private object lock_node;
		private ListStore ls;
		private bool clock_started = false;
	
		
		
		
		public PlayListWidget()
		{
			this.Build();			
			playList = new PlayList();
			lock_node = new System.Object();
	
			
		}

		public void SetPlayer(IPlayer player){
			this.player = player;
		
		}
		
		public void Load(string filePath){
			this.label1.Visible = false;
			this.playList.Load(filePath);
			this.Model = playList.GetModel();
			this.playlisttreeview1.Sensitive = true;
		}
		public ListStore Model {
			set {this.playlisttreeview1.Model = value;}
			get {return (ListStore)this.playlisttreeview1.Model;}
		}
		
		public void Add (PlayListTimeNode plNode){
			if (playList.isLoaded()){
				this.Model.AppendValues(plNode);
			}
			
		}
		
		public PlayListTimeNode Next(){
			Console.WriteLine(player.AccurateCurrentTime );
			lock (this.lock_node){
				TreePath path;			
				path = (this.playlisttreeview1.Selection.GetSelectedRows())[0];
				path.Next();
				if (path!=null){
					this.playlisttreeview1.Selection.SelectPath(path);			
					this.SelectPlayListNode(path);			
				}
				return plNode;
			}
		}
		
		public void Prev(){
			//Comprobamos el tiempo transcurrido de reproducción
			Console.WriteLine(player.AccurateCurrentTime );
			if ((this.player.CurrentTime - this.plNode.Start.MSeconds) < 100){
				//Seleccionaod el elemento anterior
				lock (this.lock_node){
					TreePath path;			
					path = (this.playlisttreeview1.Selection.GetSelectedRows())[0];
					path.Prev();
					if (path!=null){
						this.playlisttreeview1.Selection.SelectPath(path);			
						this.SelectPlayListNode(path);			
					}
				}
				
				
			}
			else 
				//Nos situamos al inicio del segmento
				this.player.SeekTo(plNode.Start.MSeconds,true);
			
			
								
		}
		
		public void Stop(){
			this.StopClock();
		}
		
		
		
		private void StartClock ()
		{

			if (player!=null && !clock_started){
				timeout = Gtk.Timeout.Add (20, new Gtk.Function (CheckStopTime));
				clock_started=true;
			}
		}
		
		private void StopClock(){
			if (this.clock_started){
				Gtk.Timeout.Remove(timeout);
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
			
			Gtk.TreeIter iter;
			bool hasNext= false;
			this.Model.GetIter (out iter, path);
			if (this.Model.IterIsValid(iter)){
				PlayListTimeNode selectedNode = (PlayListTimeNode)this.Model.GetValue (iter, 0);
			
				this.plNode = selectedNode;
				//Desplazamos una posición en el arbol en busca de un siguiente nodo
				path.Next();

				//comprobamos que el siguiente elemento en el arbol no sea nulo
				this.Model.GetIter (out iter, path);
				hasNext = this.Model.IterIsValid(iter);
			
				this.plNode = plNode;

				if (this.PlayListNodeSelected != null)
					this.PlayListNodeSelected(plNode,hasNext);
				this.StartClock();

			}
			else this.plNode = null;
		
		}
		
		
		
		

		protected virtual void OnPlaylisttreeview1RowActivated (object o, Gtk.RowActivatedArgs args)
		{
			this.SelectPlayListNode(args.Path);
			
			
		}

		protected virtual void OnUpbuttonClicked (object sender, System.EventArgs e)
		{
		}

		protected virtual void OnDownbuttonClicked (object sender, System.EventArgs e)
		{
		}

		protected virtual void OnSavebuttonClicked (object sender, System.EventArgs e)
		{
			string filename = null;
			
			if (playList.isLoaded()){
				filename = playList.File;
			}
			else{
				FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Save playlist"),
				                                                   null,
				                                                   FileChooserAction.Open,
				                                                   "gtk-cancel",ResponseType.Cancel,
				                                                   "gtk-save",ResponseType.Accept);
				fChooser.SetCurrentFolder(MainClass.PlayListDir());
				fChooser.AddFilter(playList.FileFilter);
				if (fChooser.Run() == (int)ResponseType.Accept){
					filename = fChooser.Filename;
					
				}	
				fChooser.Destroy();
			}
			playList.SetModel(this.Model);
			playList.Save(filename);

			
		
		}

		protected virtual void OnOpenbuttonClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Open playlist"),
			                                                   null,
			                                                   FileChooserAction.Open,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.PlayListDir());
			fChooser.AddFilter(playList.FileFilter);
			if (fChooser.Run() == (int)ResponseType.Accept){
				
				this.Load(fChooser.Filename);
				
			}
		
			fChooser.Destroy();
			
			
			
		}

		protected virtual void OnNewbuttonClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("New playlist"),
			                                                   null,
			                                                   FileChooserAction.Save,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.PlayListDir());			
			fChooser.AddFilter(playList.FileFilter);
			
			if (fChooser.Run() == (int)ResponseType.Accept){
				this.label1.Visible = false;
				playList.New(fChooser.Filename);
				this.Model = playList.GetModel();
				this.playlisttreeview1.Sensitive = true;
				
			}
			fChooser.Destroy();
				
		}

	}
}
