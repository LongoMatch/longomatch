// TreeWidgetPopup.cs
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
using Gdk;
using Gtk;
using Mono.Unix;

namespace LongoMatch
{
	
	
	public class TimeNodesTreeView : Gtk.TreeView
	{
		
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeDeletedHandler TimeNodeDeleted;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event NewMarkEventHandler NewMarkEvent;
		private TreeIter selectedIter;
		private Menu menu;
		private TimeNode selectedTimeNode;
		private Color[] colors;

		
		public TimeNodesTreeView(){
			
			this.RowActivated += new RowActivatedHandler(OnTreeviewRowActivated);
						
			menu = new Menu();
			MenuItem quit = new MenuItem("Delete");
			MenuItem addPLN = new MenuItem("Add to playlist");
			addPLN.Activated += new EventHandler(OnAdded);
			quit.Activated += new EventHandler(OnDeleted);
			addPLN.Show();
			quit.Show();
			menu.Append(addPLN);
			menu.Append(quit);		
			
			colors = new Color[20];
			
			Gtk.TreeViewColumn nameColumn = new Gtk.TreeViewColumn ();
			nameColumn.Title = "Name";
			Gtk.CellRendererText nameCell = new Gtk.CellRendererText ();
			nameCell.Editable = true;
			nameCell.Edited += OnNameCellEdited;
			Gtk.CellRendererPixbuf miniatureCell = new Gtk.CellRendererPixbuf ();
			nameColumn.PackStart (miniatureCell, true);
			nameColumn.PackEnd (nameCell, true);
 
			Gtk.TreeViewColumn startTimeColumn = new Gtk.TreeViewColumn ();
			startTimeColumn.Title = "Start";
			Gtk.CellRendererText startTimeCell = new Gtk.CellRendererText ();
			startTimeColumn.PackStart (startTimeCell, true);
			
			Gtk.TreeViewColumn stopTimeColumn = new Gtk.TreeViewColumn ();
			stopTimeColumn.Title = "Stop";
			Gtk.CellRendererText stopTimeCell = new Gtk.CellRendererText ();
			stopTimeColumn.PackStart (stopTimeCell, true);

			nameColumn.SetCellDataFunc (miniatureCell, new Gtk.TreeCellDataFunc(RenderMiniature));
			nameColumn.SetCellDataFunc (nameCell, new Gtk.TreeCellDataFunc (RenderName));
			startTimeColumn.SetCellDataFunc (startTimeCell, new Gtk.TreeCellDataFunc (RenderStartTime));
			stopTimeColumn.SetCellDataFunc (stopTimeCell, new Gtk.TreeCellDataFunc (RenderStopTime));
			
			
			this.AppendColumn (nameColumn);
			this.AppendColumn (startTimeColumn);
			this.AppendColumn (stopTimeColumn);
		
		}
		
		
		~TimeNodesTreeView()
		{

		}
		
		public Color[]  Colors{
			set {this.colors = value;}
		}
		
		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			//Call base class, to allow normal handling,
			//such as allowing the row to be selected by the right-click:
			bool returnValue = base.OnButtonPressEvent(evnt);
			
			//Then do our custom stuff:
			if( (evnt.Type == EventType.ButtonPress) && (evnt.Button == 3) )
			{
				TreePath path;
				this.GetPathAtPos((int)evnt.X,(int)evnt.Y,out path);
				if (path!=null){
					this.Model.GetIter (out selectedIter,path); 
					selectedTimeNode = (TimeNode)this.Model.GetValue (selectedIter, 0);
					if (selectedTimeNode is MediaTimeNode )
					    menu.Popup();
				}
			}
			return returnValue;
								
		}
		
		protected void OnDeleted(object obj, EventArgs args){
			if (TimeNodeDeleted != null)
				TimeNodeDeleted((MediaTimeNode)selectedTimeNode);
			((TreeStore)this.Model).Remove(ref selectedIter);
			
		}
		
		protected void OnAdded(object obj, EventArgs args){
			if (PlayListNodeAdded != null)	
				PlayListNodeAdded((MediaTimeNode)selectedTimeNode);
		}
		
		private void RenderMiniature (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TimeNode tNode = (TimeNode) model.GetValue (iter, 0);
			if (tNode is MediaTimeNode){
				(cell as Gtk.CellRendererPixbuf).Pixbuf = ((MediaTimeNode)tNode).Miniature;
				(cell as Gtk.CellRendererPixbuf).CellBackgroundGdk = colors[((MediaTimeNode)tNode).DataSection];
			}
			else {
				(cell as Gtk.CellRendererPixbuf).Pixbuf = null;
				(cell as Gtk.CellRendererPixbuf).CellBackground = "white";
			}
		}
		
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TimeNode tNode = (TimeNode) model.GetValue (iter, 0);
			
 
			/*if (song.Artist.StartsWith ("X") == true) {
				(cell as Gtk.CellRendererText).Foreground = "red";
			} else {
				(cell as Gtk.CellRendererText).Foreground = "darkgreen";
			}*/
 
			(cell as Gtk.CellRendererText).Text = tNode.Name;
			
			if (tNode is MediaTimeNode){
				(cell as Gtk.CellRendererText).BackgroundGdk = colors[((MediaTimeNode)tNode).DataSection];
				(cell as Gtk.CellRendererText).CellBackgroundGdk = colors[((MediaTimeNode)tNode).DataSection];
			}
			else{
				(cell as Gtk.CellRendererText).Background = "white";
				(cell as Gtk.CellRendererText).CellBackground = "white";
			}
			
		}
 
		
		private void RenderStartTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TimeNode tNode = (TimeNode) model.GetValue (iter, 0);
			

			if (tNode is MediaTimeNode){
				(cell as Gtk.CellRendererText).Text =tNode.Start.ToMSecondsString();
				(cell as Gtk.CellRendererText).BackgroundGdk = colors[((MediaTimeNode)tNode).DataSection];
				(cell as Gtk.CellRendererText).CellBackgroundGdk = colors[((MediaTimeNode)tNode).DataSection];
				
			}
			else {
				(cell as Gtk.CellRendererText).Text = "";
				(cell as Gtk.CellRendererText).Background = "white";
				(cell as Gtk.CellRendererText).CellBackground = "white";
			}
				
			
		}
		
		private void RenderStopTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			TimeNode tNode = (TimeNode) model.GetValue (iter, 0);
			if (tNode is MediaTimeNode){
				(cell as Gtk.CellRendererText).Text = tNode.Stop.ToMSecondsString();
				(cell as Gtk.CellRendererText).BackgroundGdk = colors[((MediaTimeNode)tNode).DataSection];
				(cell as Gtk.CellRendererText).CellBackgroundGdk = colors[((MediaTimeNode)tNode).DataSection];
			}
			else {
				(cell as Gtk.CellRendererText).Text = "";
				(cell as Gtk.CellRendererText).Background = "white";
				(cell as Gtk.CellRendererText).CellBackground = "white";
			}
		}
		
		private void OnNameCellEdited (object o, Gtk.EditedArgs args)
		{
			Gtk.TreeIter iter;
			this.Model.GetIter (out iter, new Gtk.TreePath (args.Path)); 
			TimeNode tNode = (TimeNode)this.Model.GetValue (iter,0);
			tNode.Name = args.NewText;
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode,args.NewText);
		}
		
		protected virtual void OnTreeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter iter;
			this.Model.GetIter (out iter, args.Path);
			TimeNode tNode = (TimeNode)this.Model.GetValue (iter, 0);
			
			if (tNode is MediaTimeNode && TimeNodeSelected != null)
				this.TimeNodeSelected((MediaTimeNode)tNode);
			else if (tNode is SectionsTimeNode && this.NewMarkEvent != null)
				Console.WriteLine(args.Path);

	
		}
	}
}