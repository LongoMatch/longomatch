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

		public event PlaySelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;

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

		public bool ProjectIsLive {
			set {
				playerstreeview.ProjectIsLive = value;
			}
		}
		
		public void SetTeam(TeamTemplate template, List<Play> plays) {
			TreeStore team;
			Dictionary<Player, TreeIter> playersDict = new Dictionary<Player, TreeIter>();
			
			team = new TreeStore(typeof(object));

			foreach(var player in template) {
				/* Add a root in the tree with the option name */
				var iter = team.AppendValues(player);
				playersDict.Add(player, iter);
			}
			
			foreach (var play in plays) {
				foreach (var player in play.Players.AllUniqueElements) {
					if (playersDict.ContainsKey(player.Value))
						team.AppendValues(playersDict[player.Value], new object[1] {play});
				}
			}
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
		}

		protected virtual void OnTimeNodeSelected(Play tNode) {
			if(TimeNodeSelected != null)
				TimeNodeSelected(tNode);
		}

		protected virtual void OnSnapshotSeriesEvent(Play tNode)
		{
			if(SnapshotSeriesEvent != null)
				SnapshotSeriesEvent(tNode);
		}

		protected virtual void OnTimeNodeChanged(TimeNode tNode, object val)
		{
			if(TimeNodeChanged != null)
				TimeNodeChanged(tNode, val);
		}

		protected virtual void OnPlayListNodeAdded(Play tNode)
		{
			if(PlayListNodeAdded != null)
				PlayListNodeAdded(tNode);
		}

	}
}
