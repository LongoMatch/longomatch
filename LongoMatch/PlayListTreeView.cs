// PlayListTreeView.cs created with MonoDevelop
// User: ando at 0:29 10/12/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using Gdk;

namespace LongoMatch
{
	
	
public class PlayListTreeView : Gtk.TreeView
	{
		

		private TreeIter selectedIter;
		private Menu menu;
		private PlayListNode selectedPlayListNode;
		private ListStore ls;

		
		public PlayListTreeView(){
			

			this.HeadersVisible = false;

			ls = new ListStore(typeof(PlayListNode));
			this.Model = ls;
			
		
			
			menu = new Menu();
			MenuItem quit = new MenuItem("Delete");
			quit.Activated += new EventHandler(OnMenuFilePopup);
			quit.Show();
			menu.Append(quit);		
			

			Gtk.TreeViewColumn nameColumn = new Gtk.TreeViewColumn ();
			
			nameColumn.Title = "Name";
			Gtk.CellRendererText nameCell = new Gtk.CellRendererText ();
			nameColumn.PackStart (nameCell, true);
 
			Gtk.TreeViewColumn startTimeColumn = new Gtk.TreeViewColumn ();
			startTimeColumn.Title = "Start";
			Gtk.CellRendererText startTimeCell = new Gtk.CellRendererText ();
			startTimeColumn.PackStart (startTimeCell, true);
			
			Gtk.TreeViewColumn stopTimeColumn = new Gtk.TreeViewColumn ();
			stopTimeColumn.Title = "Stop";
			Gtk.CellRendererText stopTimeCell = new Gtk.CellRendererText ();
			stopTimeColumn.PackStart (stopTimeCell, true);

			
			nameColumn.SetCellDataFunc (nameCell, new Gtk.TreeCellDataFunc (RenderName));
			startTimeColumn.SetCellDataFunc (startTimeCell, new Gtk.TreeCellDataFunc (RenderStartTime));
			stopTimeColumn.SetCellDataFunc (stopTimeCell, new Gtk.TreeCellDataFunc (RenderStopTime));
			
			
			this.AppendColumn (nameColumn);
			this.AppendColumn (startTimeColumn);
			this.AppendColumn (stopTimeColumn);

		
		}
		
		
		~PlayListTreeView()
		{

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
					selectedPlayListNode = (PlayListNode)this.Model.GetValue (selectedIter, 0);
				    menu.Popup();
				}
			}
			return returnValue;
								
		}
		
		protected void OnMenuFilePopup(object obj, EventArgs args){
//			((TreeStore)this.Model).Remove(ref selectedIter);
			
		}
		
		private void RenderName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PlayListNode tNode = (PlayListNode) model.GetValue (iter, 0);
			
 
			/*if (song.Artist.StartsWith ("X") == true) {
				(cell as Gtk.CellRendererText).Foreground = "red";
			} else {
				(cell as Gtk.CellRendererText).Foreground = "darkgreen";
			}*/
 
			(cell as Gtk.CellRendererText).Text = tNode.Name;
		}
 
		
		private void RenderStartTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PlayListNode tNode = (PlayListNode) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = TimeString.MSecondsToSecondsString(tNode.StartTime);
				
			
		}
		
		private void RenderStopTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			PlayListNode tNode = (PlayListNode) model.GetValue (iter, 0);
			(cell as Gtk.CellRendererText).Text = TimeString.MSecondsToMSecondsString(tNode.StopTime);
		}
		
			



	}
}
