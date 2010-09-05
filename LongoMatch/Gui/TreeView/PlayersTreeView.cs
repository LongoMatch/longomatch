//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//

using Gdk;
using Gtk;
using LongoMatch.Common;
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayersTreeView : ListTreeViewBase
	{

		private Team team;


		public PlayersTreeView() {
			team = Team.LOCAL;
			tag.Visible = false;
			players.Visible = false;
			delete.Visible = false;
		}

		public Team Team {
			set {
				team = value;
			}
			get {
				return team ;
			}
		}
		
		new public TreeStore Model{
			set{
				if (value != null){
					value.SetSortFunc(0, SortFunction);
					value.SetSortColumnId(0,SortType.Ascending);
				}
				base.Model = value;					
			}
			get{
				return base.Model as TreeStore;
			}
		}

		protected int SortFunction(TreeModel model, TreeIter a, TreeIter b){
			object oa;
			object ob;
			
			if (model == null)
				return 0;	
			
			oa = model.GetValue (a, 0);
			ob = model.GetValue (b, 0);
			
			if (oa is Player)
				return (oa as Player).Name.CompareTo((ob as Player).Name);
			else 
				return (oa as TimeNode).Name.CompareTo((ob as TimeNode).Name);
		}
		
		override protected bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return false;
		}
		
		override protected void OnNameCellEdited(object o, Gtk.EditedArgs args)
		{
			base.OnNameCellEdited(o, args);
			Model.SetSortFunc(0, SortFunction);
		}

		override protected bool OnButtonPressEvent(EventButton evnt)
		{			
			TreePath[] paths = Selection.GetSelectedRows();
			
			if ((evnt.Type == EventType.ButtonPress) && (evnt.Button == 3))
			{
				// We don't want to unselect the play when several
				// plays are selected and we clik the right button
				// For multiedition
				if (paths.Length <= 1){
					base.OnButtonPressEvent(evnt);
					paths = Selection.GetSelectedRows();
				}
				
				if (paths.Length == 1) {
					TimeNode selectedTimeNode = GetValueFromPath(paths[0]) as TimeNode;
					if (selectedTimeNode is MediaTimeNode) {
						deleteKeyFrame.Sensitive = (selectedTimeNode as MediaTimeNode).KeyFrameDrawing != null;
						MultiSelectMenu(false);
						menu.Popup();
					}
				}
				else if (paths.Length > 1){
					MultiSelectMenu(true);
					menu.Popup();								
				}
			}
			else 
				base.OnButtonPressEvent(evnt);
			return true;
		}
				
		override protected bool SelectFunction(TreeSelection selection, TreeModel model, TreePath path, bool selected){
			// Don't allow multiselection for Players
			if (!selected && selection.GetSelectedRows().Length > 0){
				if (selection.GetSelectedRows().Length == 1 &&
				    GetValueFromPath(selection.GetSelectedRows()[0]) is Player)
					return false;	
				return !(GetValueFromPath(path) is Player);										
			}
			// Always unselect
			else
				return true;
		}
	}
}
