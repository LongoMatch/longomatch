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
using LongoMatch.DB;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;

namespace LongoMatch.Widgets.Component
{
	
	
	public partial class TreeWidget : Gtk.Bin
	{

		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeDeletedHandler TimeNodeDeleted;
		public event PlayListNodeAddedHandler PlayListNodeAdded;

		private FileData fileData;

		
		public TreeWidget()
		{		
			this.Build();		                   
		}
		
		public void AddTimeNode(MediaTimeNode tNode, int i){
			if (fileData != null){
				TreeIter iter;
				TreeStore model = (TreeStore)treeview.Model;
				model.GetIterFromString (out iter, i.ToString());
				model.AppendValues (iter,tNode);
			}
			
		
		}

	
	
		public FileData FileData{
			set{ 
				this.fileData = value;
				treeview.Model = this.fileData.GetModel();
				treeview.Colors = this.fileData.Sections.Colors;
			}
			
		}
		
		
		
	

	
		

		protected virtual void OnTimeNodeChanged(TimeNode tNode,object val){
			this.TimeNodeChanged(tNode,val);
		}
		
		protected virtual void OnTimeNodeSelected(MediaTimeNode tNode){
			this.TimeNodeSelected(tNode);
		}
		
		protected virtual void OnTimeNodeDeleted(MediaTimeNode tNode){
			this.TimeNodeDeleted(tNode);
		}

		protected virtual void OnPlayListNodeAdded (MediaTimeNode tNode)
		{
			this.PlayListNodeAdded(tNode);
		}

		
	}
}
