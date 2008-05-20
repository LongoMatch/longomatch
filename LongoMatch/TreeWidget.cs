// TreeWidget.cs
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

namespace LongoMatch
{
	
	
	public partial class TreeWidget : Gtk.Bin
	{

		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeDeletedHandler TimeNodeDeleted;
		public event PlayListNodeAddedHandler PlayListNodeAdded;

		

		
		public TreeWidget()
		{
		
			this.Build();
		                   
		}
		
		public void AddTimeNode(TimeNode tNode, int i){
			TreeIter iter;
			this.Model.GetIterFromString (out iter, i.ToString());
			this.Model.AppendValues (iter,tNode);
			
		
		}
		
	
		public TreeStore Model {
			set {treeview.Model = value;}
			get {return (TreeStore)treeview.Model;}
		}

	
		

		protected virtual void OnTimeNodeChanged(TimeNode tNode,object val){
			this.TimeNodeChanged(tNode,val);
		}
		
		protected virtual void OnTimeNodeSelected(TimeNode tNode){
			this.TimeNodeSelected(tNode);
		}
		
		protected virtual void OnTimeNodeDeleted(TimeNode tNode){
			this.TimeNodeDeleted(tNode);
		}

		protected virtual void OnPlayListNodeAdded (TimeNode tNode)
		{
			this.PlayListNodeAdded(tNode);
		}

		

		
		
		
		
		
		
	}
}
