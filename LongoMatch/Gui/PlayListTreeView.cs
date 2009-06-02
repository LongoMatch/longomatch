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
using LongoMatch.Playlist;
using LongoMatch.Handlers;

namespace LongoMatch.Gui.Component
{
	
	
[System.ComponentModel.Category("LongoMatch")]
[System.ComponentModel.ToolboxItem(true)]
public class PlayListTreeView : Gtk.TreeView
	{		
		private Menu menu;
		private MenuItem setRate;
		private ListStore ls;
		private PlayList playlist;
		private PlayListTimeNode loadedTimeNode = null; //The play currently loaded in the player
		private PlayListTimeNode selectedTimeNode = null; //The play selected in the tree
		private TreeIter selectedIter;
		
		public event ApplyCurrentRateHandler ApplyCurrentRate;
		
		
		public PlayListTreeView(){			

			this.HeadersVisible = false;

			ls = new ListStore(typeof(PlayListTimeNode));
			this.Model = ls;		
			
			menu = new Menu();
			MenuItem delete = new MenuItem(Catalog.GetString("Delete"));
			delete.Activated += new EventHandler(OnDelete);
			delete.Show();
			setRate = new MenuItem(Catalog.GetString("Apply current play rate"));
			setRate.Activated += new EventHandler(OnApplyRate);
			setRate.Show();
			menu.Append(setRate);
			menu.Append(delete);		
			

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
		
		public PlayListTimeNode LoadedPlay{
			set { loadedTimeNode = value; this.QueueDraw();}
		}
		
		~PlayListTreeView()
		{
		}
		
		
		protected override bool OnButtonPressEvent (EventButton evnt)
		{			
			if( (evnt.Type == EventType.ButtonPress) && (evnt.Button == 3) )
			{
				TreePath path;				
				GetPathAtPos((int)evnt.X,(int)evnt.Y,out path);
				if (path!=null){
					ListStore list = ((ListStore)Model);					
					Model.GetIter (out selectedIter,path); 
					selectedTimeNode = (PlayListTimeNode)(list.GetValue(selectedIter,0));
					setRate.Sensitive = selectedTimeNode == loadedTimeNode;
				    menu.Popup();
				}
			}
			return base.OnButtonPressEvent(evnt);								
		}
		
		protected void OnDelete(object obj, EventArgs args){
			ListStore list = ((ListStore)Model);
			playlist.Remove(selectedTimeNode);
			list.Remove(ref selectedIter);		
		}
		
		protected void OnApplyRate(object obj, EventArgs args){
			ListStore list = ((ListStore)Model);
			if (ApplyCurrentRate != null)
				ApplyCurrentRate(selectedTimeNode);
		}
		
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PlayListTimeNode tNode = (PlayListTimeNode) model.GetValue (iter, 0); 
			(cell as Gtk.CellRendererText).Text = 	Catalog.GetString("Name: ")+tNode.Name +"\n"+
													Catalog.GetString("Start: ")+tNode.Start.ToMSecondsString()+Catalog.GetString(" sec")+"\n"+
													Catalog.GetString("Duration: ")+tNode.Duration.ToMSecondsString()+Catalog.GetString(" sec")+"\n"+
													Catalog.GetString("Play Rate: ")+tNode.Rate.ToString();
			if (!tNode.Valid)
				(cell as Gtk.CellRendererText).Foreground = "red";			
			else if (tNode == loadedTimeNode)
				(cell as Gtk.CellRendererText).Foreground = "blue";
			else 
				(cell as Gtk.CellRendererText).Foreground = "black";
			
		}
	}
}
