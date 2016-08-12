// PlayListTreeView.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Menus;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store.Playlists;
using LMCommon = LongoMatch.Core.Common;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class PlayListTreeView : Gtk.TreeView
	{
		ProjectLongoMatch project;
		TreeIter selectedIter;
		TreePath pathClicked;
		TreeStore store;
		Playlist dragSourcePlaylist;
		IPlaylistElement dragSourceElement;
		Dictionary<ObservableCollection<IPlaylistElement>, TreeIter> elementsListToIter;
		Dictionary<Playlist, TreeIter> playlistToIter;
		bool dragStarted;

		public PlayListTreeView ()
		{
			HeadersVisible = false;
			EnableGridLines = TreeViewGridLines.None;
			EnableTreeLines = false;
			ShowExpanders = false;
			
			TreeViewColumn custColumn = new TreeViewColumn ();
			CellRenderer cr = new PlaysCellRenderer ();
			custColumn.PackStart (cr, true);
			custColumn.SetCellDataFunc (cr, RenderElement); 
			AppendColumn (custColumn);
		}

		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			Cleanup ();
		}

		public ProjectLongoMatch Project {
			set {
				if (project != null) {
					Cleanup ();
				}

				project = value;
				store = new TreeStore (typeof(object));
				if (project != null) {
					int i = 0;
					project.Playlists.CollectionChanged += HandleProjectPlaylistsCollectionChanged;
					elementsListToIter = new Dictionary<ObservableCollection<IPlaylistElement>, TreeIter> ();
					playlistToIter = new Dictionary<Playlist, TreeIter> ();
					foreach (Playlist playlist in project.Playlists) {
						AddPlaylist (playlist, i);
						i++;
					}
				}
				Model = store;
			}
			get {
				return project;
			}
		}

		void Cleanup ()
		{
			TreeIter current;

			if (project == null) {
				return;
			}
			store.GetIterFirst (out current);
			while (store.IterIsValid (current)) {
				RemovePlaylist (store.GetValue (current, 0) as Playlist, 0);
				store.GetIterFirst (out current);
			}
			store.Clear ();
			project.Playlists.CollectionChanged -= HandleProjectPlaylistsCollectionChanged;
		}

		void AddPlaylist (Playlist playlist, int index)
		{
			TreeIter iter = store.InsertWithValues (index, playlist);
			foreach (IPlaylistElement el in playlist.Elements) {
				store.AppendValues (iter, el);
			}
			elementsListToIter [playlist.Elements] = iter;
			playlistToIter [playlist] = iter;
			playlist.Elements.CollectionChanged += HandlePlaylistElementsCollectionChanged;
		}

		void RemovePlaylist (Playlist playlist, int index)
		{
			TreeIter iter;
			store.GetIterFromString (out iter, index.ToString ());
			store.Remove (ref iter);
			elementsListToIter.Remove (playlist.Elements);
			playlistToIter.Remove (playlist);
			playlist.Elements.CollectionChanged -= HandlePlaylistElementsCollectionChanged;
		}

		void RenderElement (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			var item = model.GetValue (iter, 0);
			PlaysCellRenderer c = cell as PlaysCellRenderer;
			c.Item = item;
			c.Count = model.IterNChildren (iter);
		}

		void ShowPlaylistElementMenu (Playlist playlist, IPlaylistElement element)
		{
			PlaylistElementMenu menu = new PlaylistElementMenu (this, playlist, new List<IPlaylistElement> { element });
			menu.Popup ();
		}

		void ShowPlaylistMenu (Playlist playlist)
		{
			Menu menu;
			MenuItem edit, delete, render;

			menu = new Menu ();

			edit = new MenuItem (Catalog.GetString ("Edit name"));
			edit.Activated += (sender, e) => {
				string name = App.Current.Dialogs.QueryMessage (Catalog.GetString ("Name:"), null,
					              playlist.Name).Result;
				if (!String.IsNullOrEmpty (name)) {
					playlist.Name = name;
				}
			};
			menu.Append (edit);

			render = new MenuItem (Catalog.GetString ("Render"));
			render.Activated += (sender, e) => App.Current.EventsBroker.Publish<RenderPlaylistEvent> (
				new RenderPlaylistEvent { 
					Playlist = playlist
				}
			);		
			menu.Append (render);

			delete = new MenuItem (Catalog.GetString ("Delete"));
			delete.Activated += (sender, e) => project.Playlists.Remove (playlist);
			menu.Append (delete);
			
			menu.ShowAll ();
			menu.Popup ();
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
		{
			if (Misc.RightButtonClicked (evnt)) {
				TreePath path;
				GetPathAtPos ((int)evnt.X, (int)evnt.Y, out path);
				if (path != null) {
					Model.GetIter (out selectedIter, path);
					object el = Model.GetValue (selectedIter, 0);
					if (el is Playlist) {
						ShowPlaylistMenu (el as Playlist);
					} else {
						TreeIter parent;
						Model.IterParent (out parent, selectedIter);
						Playlist playlist = Model.GetValue (parent, 0) as Playlist;
						ShowPlaylistElementMenu (playlist, el as IPlaylistElement);
					}
				}
			} else {
				GetPathAtPos ((int)evnt.X, (int)evnt.Y, out pathClicked);
			}
			return base.OnButtonPressEvent (evnt);
		}

		void FillElementAndPlaylist (TreeIter iter, out Playlist playlist, out IPlaylistElement element)
		{
			TreeIter parent;

			var obj = Model.GetValue (iter, 0);
			if (obj is IPlaylistElement) {
				Model.IterParent (out parent, iter);
				element = obj as IPlaylistElement;
				playlist = Model.GetValue (parent, 0) as Playlist;
			} else {
				element = null;
				playlist = obj as Playlist;
			}
		}

		protected override void OnDragDataReceived (DragContext context, int x, int y, SelectionData selection, uint info, uint time)
		{
			TreeIter iter;
			TreePath path;
			TreeViewDropPosition pos;
			Playlist destPlaylist;
			IPlaylistElement destElement;
			TreeStore store = Model as TreeStore;

			if (GetDestRowAtPos (x, y, out path, out pos)) {
				store.GetIter (out iter, path);
				FillElementAndPlaylist (iter, out destPlaylist, out destElement);
				
				/* Moving playlists */
				if (dragSourceElement == null) {
					project.Playlists.Remove (dragSourcePlaylist);
					project.Playlists.Insert (path.Indices [0], dragSourcePlaylist);
				} else {
					IPlaylistElement srcCurrent, dstCurrent;
					int destIndex;

					if (pos == TreeViewDropPosition.Before ||
					    pos == TreeViewDropPosition.IntoOrBefore) {
						destIndex = store.GetPath (iter).Indices [1];
					} else {
						destIndex = store.GetPath (iter).Indices [1] + 1;
					}

					if (dragSourcePlaylist == destPlaylist) {
						// If the element is dragged to bigger index, when it's removed from the playlist
						// the new index needs to be decremented by one because there is one element less in the
						// playlist now
						if (dragSourcePlaylist.Elements.IndexOf (dragSourceElement) <= destIndex) {
							destIndex--;
						}
					}

					srcCurrent = dragSourcePlaylist.Selected;
					dstCurrent = destPlaylist.Selected;

					dragSourcePlaylist.Elements.Remove (dragSourceElement);
					destPlaylist.Elements.Insert (destIndex, dragSourceElement);

					if (dragSourcePlaylist != destPlaylist) {
						dragSourcePlaylist.SetActive (srcCurrent);
					}
					destPlaylist.SetActive (dstCurrent);
				}
			}
			Gtk.Drag.Finish (context, true, false, time);
		}

		void DisableDragInto (TreePath path, DragContext context, uint time, TreeViewDropPosition pos)
		{
			if (pos == TreeViewDropPosition.IntoOrAfter) {
				pos = TreeViewDropPosition.After;
			} else if (pos == TreeViewDropPosition.IntoOrBefore) {
				pos = TreeViewDropPosition.Before;
			}
			SetDragDestRow (path, pos);
			Gdk.Drag.Status (context, context.SuggestedAction, time);
		}

		protected override bool OnDragMotion (DragContext context, int x, int y, uint time)
		{
			TreePath path;
			TreeViewDropPosition pos;
			TreeIter iter;
			IPlaylistElement element;
			Playlist playlist;
			
			if (GetDestRowAtPos (x, y, out path, out pos)) {
				Model.GetIter (out iter, path);
				
				FillElementAndPlaylist (iter, out playlist, out element);

				/* Drag a playlist*/
				if (dragSourceElement == null) {
					if (element == null) {
						DisableDragInto (path, context, time, pos);
						return true;
					} else {
						return false;
					}
				}
				/* Drag an element */
				else {
					if (element != null) {
						DisableDragInto (path, context, time, pos);
						return true;
					} else {
						return false;
					}
				}
			}
			return false;
		}

		protected override void OnDragBegin (DragContext context)
		{
			Selection.GetSelected (out selectedIter);
			FillElementAndPlaylist (selectedIter, out dragSourcePlaylist,
				out dragSourceElement);
			dragStarted = true;
			base.OnDragBegin (context);
		}

		protected override bool OnButtonReleaseEvent (Gdk.EventButton evnt)
		{
			if (pathClicked != null && !dragStarted) {
				if (GetRowExpanded (pathClicked)) {
					CollapseRow (pathClicked);
				} else {
					ExpandRow (pathClicked, true);
				}
				pathClicked = null;
			}
			return base.OnButtonReleaseEvent (evnt);
		}

		protected override void OnDragEnd (DragContext context)
		{
			base.OnDragEnd (context);
			dragStarted = false;
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			return false;
		}

		void HandlePlaylistElementsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			TreeIter playlistIter = elementsListToIter [sender as ObservableCollection<IPlaylistElement>];

			if (e.Action == NotifyCollectionChangedAction.Add) {
				int i = 0;
				foreach (var newElement in e.NewItems.OfType<IPlaylistElement>()) {
					store.InsertWithValues (playlistIter, e.NewStartingIndex + i, newElement);
					i++;
				}
			} else if (e.Action == NotifyCollectionChangedAction.Remove) {
				TreeIter iter;
				foreach (var oldElement in e.OldItems.OfType<IPlaylistElement>()) {
					store.IterNthChild (out iter, playlistIter, e.OldStartingIndex);
					store.Remove (ref iter);
				}
			}
		}

		void HandleProjectPlaylistsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add) {
				int i = 0;
				foreach (var newPlaylist in e.NewItems.OfType<Playlist>()) {
					AddPlaylist (newPlaylist, e.NewStartingIndex + i);
					i++;
				}
			} else if (e.Action == NotifyCollectionChangedAction.Remove) {
				foreach (var oldPlaylist in e.OldItems.OfType<Playlist>()) {
					RemovePlaylist (oldPlaylist, e.OldStartingIndex);
				}
			}
		}
	}
}
