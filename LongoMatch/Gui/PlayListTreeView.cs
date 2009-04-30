// PlayListTreeView.cs
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
using Mono.Unix;
using LongoMatch.TimeNodes;
using LongoMatch.Video;
using LongoMatch.Playlist;

namespace LongoMatch.Gui.Component
{
	
	
[System.ComponentModel.Category("LongoMatch")]
[System.ComponentModel.ToolboxItem(true)]
public class PlayListTreeView : Gtk.TreeView
	{
		

		private TreeIter selectedIter;
		private Menu menu;
		private ListStore ls;
		private PlayList playlist;

		
		public PlayListTreeView(){
			

			this.HeadersVisible = false;

			ls = new ListStore(typeof(PlayListTimeNode));
			this.Model = ls;
			
		
			
			menu = new Menu();
			MenuItem quit = new MenuItem(Catalog.GetString("Delete"));
			quit.Activated += new EventHandler(OnMenuFilePopup);
			quit.Show();
			menu.Append(quit);		
			

			Gtk.TreeViewColumn nameColumn = new Gtk.TreeViewColumn ();
			
			nameColumn.Title = Catalog.GetString("Name");
		
			Gtk.CellRendererText nameCell = new Gtk.CellRendererText ();
			nameColumn.PackStart (nameCell, true);
			nameColumn.SetCellDataFunc (nameCell, new Gtk.TreeCellDataFunc (RenderName));
 
			
			
			
			
			this.AppendColumn (nameColumn);
			

		
		}
		
		public PlayList PlayList{
			set{ this.playlist = value;}
		}
		
		
		~PlayListTreeView()
		{

		}
		
		
		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			
			if( (evnt.Type == EventType.ButtonPress) && (evnt.Button == 3) )
			{
				TreePath path;
				this.GetPathAtPos((int)evnt.X,(int)evnt.Y,out path);
				if (path!=null){
					this.Model.GetIter (out selectedIter,path); 
				    menu.Popup();
				}
			}
			return base.OnButtonPressEvent(evnt);
								
		}
		
		protected void OnMenuFilePopup(object obj, EventArgs args){
			ListStore list = ((ListStore)this.Model);
			this.playlist.Remove((PlayListTimeNode)(list.GetValue(selectedIter,0)));
			list.Remove(ref selectedIter);
			
			
		}
		
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PlayListTimeNode tNode = (PlayListTimeNode) model.GetValue (iter, 0);
			
 
			
 
			(cell as Gtk.CellRendererText).Text = Catalog.GetString("Name: ")+tNode.Name +"\n"+Catalog.GetString("Start: ")+tNode.Start.ToMSecondsString()
				+Catalog.GetString(" sec")+"\n"+Catalog.GetString("Duration: ")+tNode.Duration.ToMSecondsString()+Catalog.GetString(" sec");
			if (!tNode.Valid){
				(cell as Gtk.CellRendererText).Foreground = "red";				
			}
			else {
				(cell as Gtk.CellRendererText).Foreground = "black";
			}
		}


	}
}
