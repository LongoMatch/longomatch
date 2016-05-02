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
using System.Collections.Generic;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Common;
using VAS.Core.Store.Playlists;
using LMCommon = LongoMatch.Core.Common;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlayersListTreeWidget : Gtk.Bin
	{
		TreeStore team;

		public PlayersListTreeWidget ()
		{
			this.Build ();
			playerstreeview.NewRenderingJob += OnNewRenderingJob;
		}

		public ProjectLongoMatch Project {
			set;
			get;
		}

		public TeamType Team {
			set {
				playerstreeview.Team = value;
			}
		}

		public EventsFilter Filter {
			set {
				playerstreeview.Filter = value;
			}
		}

		public void AddEvent (TimelineEventLongoMatch evt)
		{
			TreeIter piter;

			if (evt.Players == null) {
				return;
			}
			team.GetIterFirst (out piter);
			while (team.IterIsValid (piter)) {
				PlayerLongoMatch player = team.GetValue (piter, 0) as PlayerLongoMatch;
				if (evt.Players.Contains (player)) {
					team.AppendValues (piter, evt);
				}
				team.IterNext (ref piter);
			}
		}

		public void RemoveEvents (List<TimelineEventLongoMatch> events)
		{
			TreeIter piter;

			team.GetIterFirst (out piter);
			while (team.IterIsValid (piter)) {
				TreeIter evtIter;

				team.IterChildren (out evtIter, piter);
				while (team.IterIsValid (evtIter)) {
					TimelineEventLongoMatch evt = team.GetValue (evtIter, 0) as TimelineEventLongoMatch;
					if (events.Contains (evt)) {
						team.Remove (ref evtIter);
					}
					team.IterNext (ref evtIter);
				}
				team.IterNext (ref piter);
			}
		}

		public void SetTeam (Team template, IEnumerable<TimelineEventLongoMatch> plays)
		{
			Dictionary<PlayerLongoMatch, TreeIter> playersDict = new Dictionary<PlayerLongoMatch, TreeIter> ();
			
			Log.Debug ("Updating teams models with template:" + template);
			team = new TreeStore (typeof(object));

			foreach (var player in template.List) {
				/* Add a root in the tree with the option name */
				var iter = team.AppendValues (player);
				playersDict.Add (player, iter);
				Log.Debug ("Adding new player to the model: " + player);
			}
			
			foreach (var play in plays) {
				foreach (var player in play.Players) {
					if (playersDict.ContainsKey (player)) {
						team.AppendValues (playersDict [player], new object[1] { play });
						Log.Debug ("Adding new play to player: " + player);
					}
				}
			}
			playerstreeview.Model = team;
			playerstreeview.Colors = true;
			playerstreeview.Project = Project;
		}

		public void Clear ()
		{
			playerstreeview.Model = null;
		}

		protected virtual void OnNewRenderingJob (object sender, EventArgs args)
		{
			Playlist playlist = new Playlist ();
			TreePath[] paths = playerstreeview.Selection.GetSelectedRows ();

			foreach (var path in paths) {
				TreeIter iter;
				PlaylistPlayElement element;
				
				playerstreeview.Model.GetIter (out iter, path);
				element = new PlaylistPlayElement (playerstreeview.Model.GetValue (iter, 0) as TimelineEventLongoMatch);
				playlist.Elements.Add (element);
			}
			
			((LMCommon.EventsBroker)Config.EventsBroker).EmitRenderPlaylist (playlist);
		}

	}
}
