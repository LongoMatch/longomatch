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

using System;
using Gtk;
using Mono.Unix;
using LongoMatch.DB;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;
using LongoMatch.Common;


namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayersListTreeWidget : Gtk.Bin
	{

		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;

		private TeamTemplate template;

		public PlayersListTreeWidget()
		{
			this.Build();
		}

		public Team Team {
			set {
				playerstreeview.Team = value;
			}
		}
		
		public bool ProjectIsLive{
			set{
				playerstreeview.ProjectIsLive = value;
			}
		}

		public void DeleteTimeNode(MediaTimeNode tNode, int player) {
			if (template != null) {
				TreeIter iter;
				TreeStore model = (TreeStore)playerstreeview.Model;
				model.GetIterFromString(out iter, player.ToString());
				TreeIter child;
				model.IterChildren(out child, iter);
				// Searching the TimeNode to remove it
				while (model.IterIsValid(child)) {
					MediaTimeNode mtn = (MediaTimeNode) model.GetValue(child,0);
					if (mtn == tNode) {
						model.Remove(ref child);
						break;
					}
					TreeIter prev = child;
					model.IterNext(ref child);
					if (prev.Equals(child))
						break;
				}
			}
		}


		public void AddTimeNode(MediaTimeNode tNode,int  playerindex) {
			if (template != null) {
				TreeIter iter;
				TreeStore model = (TreeStore)playerstreeview.Model;
				model.GetIterFromString(out iter, playerindex.ToString());
				Player player = (Player)model.GetValue(iter,0);
				if (template.GetPlayer(playerindex) == player)
					model.AppendValues(iter,tNode);
			}
		}

		public void SetTeam(TeamTemplate template, TreeStore model) {
			this.template = template;
			playerstreeview.Model = model;
		}

		public void UpdatePlaysList(TreeStore model) {
			playerstreeview.Model = model;
		}

		public bool PlayListLoaded {
			set {
				playerstreeview.PlayListLoaded=value;
			}
		}

		public void Clear() {
			playerstreeview.Model = null;
			template = null;
		}

		protected virtual void OnTimeNodeSelected(MediaTimeNode tNode) {
			if (TimeNodeSelected != null)
				TimeNodeSelected(tNode);
		}

		protected virtual void OnSnapshotSeriesEvent(LongoMatch.TimeNodes.MediaTimeNode tNode)
		{
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent(tNode);
		}

		protected virtual void OnTimeNodeChanged(LongoMatch.TimeNodes.TimeNode tNode, object val)
		{
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode, val);
		}

		protected virtual void OnPlayerstreeviewPlayListNodeAdded(LongoMatch.TimeNodes.MediaTimeNode tNode)
		{
			if (PlayListNodeAdded != null)
				PlayListNodeAdded(tNode);
		}

	}
}
