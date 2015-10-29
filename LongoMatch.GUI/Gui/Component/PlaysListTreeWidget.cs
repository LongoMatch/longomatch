// TreeWidget.cs
//
//  Copyright(C) 20072009 Andoni Morales Alastruey
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
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using Gtk;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Gui.Dialog;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlaysListTreeWidget : Gtk.Bin
	{

		Project project;
		Dictionary<EventType, TreeIter> itersDic;

		public PlaysListTreeWidget ()
		{
			this.Build ();
			treeview.EditProperties += OnEditProperties;
			treeview.NewRenderingJob += OnNewRenderingJob;
			itersDic = new Dictionary<EventType, TreeIter> ();
			Config.EventsBroker.EventLoadedEvent += HandlePlayLoaded;
		}

		protected override void OnDestroyed ()
		{
			Config.EventsBroker.EventLoadedEvent -= HandlePlayLoaded;
			base.OnDestroyed ();
		}

		public EventsFilter Filter {
			set {
				treeview.Filter = value;
			}
		}

		public void RemovePlays (List<TimelineEvent> plays)
		{
			TreeIter iter, child;
			TreeStore model;
			List<TreeIter> removeIters;

			if (project == null)
				return;

			removeIters = new List<TreeIter> ();
			model = (TreeStore)treeview.Model;
			model.GetIterFirst (out iter);
			/* Scan all the tree and store the iter of each play
			 * we need to delete, but don't delete it yet so that
			 * we don't alter the tree */
			do {
				if (!model.IterHasChild (iter))
					continue;

				model.IterChildren (out child, iter);
				do {
					TimelineEvent play = (TimelineEvent)model.GetValue (child, 0);
					if (plays.Contains (play)) {
						removeIters.Add (child);
					}
				} while(model.IterNext (ref child));
			} while(model.IterNext (ref iter));

			/* Remove the selected iters now */
			for (int i = 0; i < removeIters.Count; i++) {
				iter = removeIters [i];
				model.Remove (ref iter);
			}
		}

		public void AddPlay (TimelineEvent play)
		{
			TreePath path;

			if (project == null)
				return;

			path = treeview.AddEvent (play, itersDic [play.EventType]);
			treeview.ExpandToPath (path);
			treeview.SetCursor (path, null, false);
			var cellRect = treeview.GetBackgroundArea (path, null);
			treeview.ScrollToPoint (cellRect.X, Math.Max (cellRect.Y, 0));
		}

		public Project Project {
			set {
				project = value;
				if (project != null) {
					treeview.Model = GetModel (project);
					treeview.Colors = true;
					treeview.Project = value;
				} else {
					treeview.Model = null;
				}
			}
			get {
				return project;
			}
		}

		private TreeStore GetModel (Project project)
		{
			Gtk.TreeIter iter;
			Gtk.TreeStore dataFileListStore = new Gtk.TreeStore (typeof(object));

			itersDic.Clear ();

			foreach (EventType evType in project.EventTypes) {
				iter = dataFileListStore.AppendValues (evType);
				itersDic.Add (evType, iter);
			}
			
			var queryPlaysByCategory = project.EventsGroupedByEventType;
			foreach (var playsGroup in queryPlaysByCategory) {
				EventType cat = playsGroup.Key;
				if (!itersDic.ContainsKey (cat))
					continue;
				foreach (TimelineEvent play in playsGroup) {
					dataFileListStore.AppendValues (itersDic [cat], play);
				}
			}
			return dataFileListStore;
		}

		protected virtual void OnEditProperties (EventType eventType)
		{
			EditCategoryDialog dialog = new EditCategoryDialog (project, eventType,
				                            this.Toplevel as Window);
			dialog.Run ();
			dialog.Destroy ();
		}

		protected virtual void OnNewRenderingJob (object sender, EventArgs args)
		{
			Playlist playlist;
			TreePath[] paths;

			playlist = new Playlist ();
			paths = treeview.Selection.GetSelectedRows ();

			foreach (var path in paths) {
				TreeIter iter;
				PlaylistPlayElement element;
				
				treeview.Model.GetIter (out iter, path);
				element = new PlaylistPlayElement (treeview.Model.GetValue (iter, 0) as TimelineEvent,
					project.Description.FileSet);
				playlist.Elements.Add (element);
			}
			
			Config.EventsBroker.EmitRenderPlaylist (playlist);
		}

		void HandlePlayLoaded (TimelineEvent play)
		{
			treeview.QueueDraw ();
		}
	}
}
