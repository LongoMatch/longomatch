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

using System.Collections.Generic;
using Gtk;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;


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
			playerstreeview.TimeNodeChanged += OnTimeNodeChanged;
            playerstreeview.TimeNodeSelected += OnTimeNodeSelected;
            playerstreeview.PlayListNodeAdded += OnPlayListNodeAdded;
            playerstreeview.SnapshotSeriesEvent += OnSnapshotSeriesEvent;
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

		public void RemovePlays(List<Play> plays) {
			TreeIter iter, child;
			TreeStore model;
			List<TreeIter> removeIters;
			
			if (template == null)
				return;
				
			removeIters = new List<TreeIter>();
			model = (TreeStore)playerstreeview.Model;
			model.GetIterFirst(out iter);
			do{
				if (!model.IterHasChild(iter))
					continue;
				
				model.IterChildren(out child, iter);
				do {
					Play play = (Play) model.GetValue(child,0);
					if (plays.Contains(play)) {
						removeIters.Add(child);
					}
				} while (model.IterNext(ref child)); 
			} while (model.IterNext(ref iter));
			
			for (int i=0; i < removeIters.Count; i++){
				iter = removeIters[i];
				model.Remove(ref iter);
			}
		}


		public void AddPlay(Play play, Player player) {
			TreeIter iter;
			TreeStore model;
				
			if (template == null)
				return;
			model = (TreeStore)playerstreeview.Model;
			model.GetIterFirst(out iter);
			do{
				if (model.GetValue(iter, 0) == player){
					model.AppendValues(iter, player);
					break;
				}
			} while (model.IterNext(ref iter));
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

		protected virtual void OnTimeNodeSelected(Play tNode) {
			if (TimeNodeSelected != null)
				TimeNodeSelected(tNode);
		}

		protected virtual void OnSnapshotSeriesEvent(Play tNode)
		{
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent(tNode);
		}

		protected virtual void OnTimeNodeChanged(TimeNode tNode, object val)
		{
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode, val);
		}

		protected virtual void OnPlayListNodeAdded(Play tNode)
		{
			if (PlayListNodeAdded != null)
				PlayListNodeAdded(tNode);
		}

	}
}
