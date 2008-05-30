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
