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

namespace LongoMatch.Gui.Component
{
	
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TreeWidget : Gtk.Bin
	{

		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeDeletedHandler TimeNodeDeleted;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;

		private Project project;

		
		public TreeWidget()
		{		
			this.Build();		    
			
		}
		
		public void DeleteTimeNode(MediaTimeNode tNode){
			if (project != null){
				TreeIter iter;
				TreeStore model = (TreeStore)treeview.Model;
				// Seeking the SectionTimeNode position in the tree
				// For some  configuration not all 
				// the sections are shown, eg: the 2nd may not be
				// at the 2nd row in the tree, it can be at the 1st
				// row if the 1st is hidden
				for (int j=0; j<20;j++){					
					model.GetIterFromString (out iter, j.ToString());
					TimeNode stNode = (TimeNode)model.GetValue (iter,0);
					
					if (project.Sections.GetTimeNode(tNode.DataSection) == stNode){		
						// Founded valid row
						TreeIter child;

						model.IterChildren(out child, iter);
						// Searching the TimeNode to remove it
						while (model.IterIsValid(child)){
						    //TODO Se queda en un bucle infinito en el último
							//hay que cambiar la condicón en el bucle while
							// comparando por ejemplo con el anterior y viendo 
							// si es el mismo
							MediaTimeNode mtn = (MediaTimeNode) model.GetValue( child,0);
							if(mtn == tNode){
								// Fetched TimeNode to remove
								model.Remove (ref child);
								break;
							}
							TreeIter prev = child;
							model.IterNext(ref child);
							if (prev.Equals(child))
								break;
						}
						break;
					}
				}
			}
			
		}
		
		
		public void AddTimeNode(MediaTimeNode tNode){
			if (project != null){
				TreeIter iter;
				TreeStore model = (TreeStore)treeview.Model;
				// Seeking the SectionTimeNode position in the tree
				// For some  configuration not all 
				// the sections are shown, eg: the 2nd may not be
				// at the 2nd row in the tree, it can be at the 1st
				// row if the 1st is hidden
				for (int j=0; j<19;j++){					
					model.GetIterFromString (out iter, j.ToString());
					TimeNode stNode = (TimeNode)model.GetValue (iter,0);
					if (project.Sections.GetTimeNode(tNode.DataSection) == stNode){				
						model.AppendValues (iter,tNode);
						break;
					}
				}
			}
			
		
		}
		
			
		public Project Project{
			set{ 
				project = value;
				treeview.Model = project.GetModel();
				treeview.Colors = project.Sections.Colors;
			}
			
		}
		
		

		protected virtual void OnTimeNodeChanged(TimeNode tNode,object val){
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode,val);
		}
		
		protected virtual void OnTimeNodeSelected(MediaTimeNode tNode){
			if (TimeNodeSelected != null)
				TimeNodeSelected(tNode);
		}
		
		protected virtual void OnTimeNodeDeleted(MediaTimeNode tNode){
			if (TimeNodeDeleted != null)
				TimeNodeDeleted(tNode);
		}

		protected virtual void OnPlayListNodeAdded (MediaTimeNode tNode)
		{
			if (PlayListNodeAdded != null)
				PlayListNodeAdded(tNode);
		}

		protected virtual void OnTreeviewSnapshotSeriesEvent (LongoMatch.TimeNodes.MediaTimeNode tNode)
		{
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent(tNode);
		}

		
	}
}
