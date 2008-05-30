// PlayListWidget.cs created with MonoDevelop
// User: ando at 19:55 09/12/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using Gdk;

namespace LongoMatch
{
	
	
	public partial class PlayListWidget : Gtk.Bin
	{
		public event PlayListNodeSelectedHandler PlayListNodeSelected;
		public PlayListWidget()
		{
			this.Build();			
		}



		
		public ListStore Model {
			set {this.playlisttreeview1.Model = value;}
			get {return (ListStore)this.playlisttreeview1.Model;}
		}
		
		public void AddPlayListNode (PlayListNode plNode){
			this.Model.AppendValues(plNode);
		}
		
		private void SelectPlayListNode (TreePath path){
			
			Gtk.TreeIter iter;
			bool hasNext= false;
			this.Model.GetIter (out iter, path);
			PlayListNode plNode = (PlayListNode)this.Model.GetValue (iter, 0);
			
			//Desplazamos una posición en el arbol en busca de un siguiente nodo
			path.Next();

			//comprobamos que el siguiente elemento en el arbol no sea nulo
			this.Model.GetIter (out iter, path);
			hasNext = this.Model.IterIsValid(iter);

			if (this.PlayListNodeSelected != null)
				this.PlayListNodeSelected(plNode,hasNext);
		
		}
		
		public void playNext(){
			TreePath path;
			
			path = (this.playlisttreeview1.Selection.GetSelectedRows())[0];
			path.Next();
			this.playlisttreeview1.Selection.SelectPath(path);
			
			
			
			this.SelectPlayListNode(path);


			
			
		}

		protected virtual void OnPlaylisttreeview1RowActivated (object o, Gtk.RowActivatedArgs args)
		{
			this.SelectPlayListNode(args.Path);
			
			
		}

	}
}
