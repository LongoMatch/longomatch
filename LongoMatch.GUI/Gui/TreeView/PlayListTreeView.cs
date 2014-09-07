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
using Gtk;
using Gdk;
using Mono.Unix;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Interfaces;
using LongoMatch.Gui.Dialog;
using LongoMatch.Core.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class PlayListTreeView : Gtk.TreeView
	{
		Project project;
		Playlist selectedPlaylist;
		IPlaylistElement selectedElement;
		TreeIter selectedIter;
		Playlist dragSourcePlaylist;
		IPlaylistElement dragSourceElement;

		public PlayListTreeView ()
		{
			HeadersVisible = false;
			EnableGridLines = TreeViewGridLines.None;
			EnableTreeLines = false;
			
			TreeViewColumn custColumn = new TreeViewColumn ();
			CellRenderer cr = new PlaysCellRenderer ();
			custColumn.PackStart (cr, true);
			custColumn.SetCellDataFunc (cr, RenderElement); 
			AppendColumn (custColumn);
		}

		public Project Project {
			set {
				project = value;
				Reload ();
			}
			get {
				return project;
			}
		}

		public void Reload ()
		{
			TreeIter iter;
			TreeStore store = new TreeStore (typeof(object));
			
			if (project != null) {
				foreach (Playlist playlist in project.Playlists) {
					iter = store.AppendValues (playlist);
					foreach (IPlaylistElement el in playlist.Elements) {
						store.AppendValues (iter, el);
					}
				}
			}
			Model = store;
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
			Menu menu;
			MenuItem edit, delete;

			menu = new Menu ();

			delete = new MenuItem (Catalog.GetString ("Delete"));
			delete.Activated += (sender, e) => {
				project.Playlists.Remove (playlist);
				(Model as TreeStore).Remove (ref selectedIter);
				Config.EventsBroker.EmitPlaylistsChanged (this);
			};
			menu.Append (delete);
			
			if (element is PlaylistPlayElement) {
				PlaylistPlayElement pl = element as PlaylistPlayElement;
				edit = new MenuItem (Catalog.GetString ("Edit"));
				edit.Activated += (sender, e) => {
					string name = Config.GUIToolkit.QueryMessage (Catalog.GetString ("Name:"), null,
					                                             pl.Title);
					if (!String.IsNullOrEmpty (name)) {
						pl.Title = name; 
					}
				};
				menu.Append (edit);
			}
			
			menu.ShowAll ();
			menu.Popup ();
		}

		void ShowPlaylistMenu (Playlist playlist)
		{
			Menu menu;
			MenuItem delete, render;

			menu = new Menu ();

			delete = new MenuItem (Catalog.GetString ("Delete"));
			delete.Activated += (sender, e) => {
				project.Playlists.Remove (playlist);
				(Model as TreeStore).Remove (ref selectedIter);
				Config.EventsBroker.EmitPlaylistsChanged (this);
			};
			menu.Append (delete);
			
			render = new MenuItem (Catalog.GetString ("Render"));
			render.Activated += (sender, e) => {
				Config.EventsBroker.EmitRenderPlaylist (playlist);
			};
			menu.Append (render);
			
			menu.ShowAll ();
			menu.Popup ();
		}

		protected override bool OnButtonPressEvent (Gdk.EventButton evnt)
		{
			if ((evnt.Type == Gdk.EventType.ButtonPress) && (evnt.Button == 3)) {
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
			}
			return base.OnButtonPressEvent (evnt);
		}

		protected void OnTitle (object o, EventArgs args)
		{
			PlaylistPlayElement ple;
			EntryDialog ed;
			
			ple = selectedElement as PlaylistPlayElement;
			ed = new EntryDialog ();
			ed.Title = Catalog.GetString ("Edit Title");
			ed.Text = ple.Title;
			if (ed.Run () == (int)ResponseType.Ok) {
				ple.Title = ed.Text;
				this.QueueDraw ();
			}
			ed.Destroy ();
		}

		protected void OnDelete (object obj, EventArgs args)
		{
			selectedPlaylist.Remove (selectedElement);
			(Model as TreeStore).Remove (ref selectedIter);
		}

		void FillElementAndPlaylist (TreeIter iter, out Playlist playlist, out IPlaylistElement element)
		{
			TreeIter parent;

			var obj = Model.GetValue (iter, 0);
			if (obj is IPlaylistElement) {
				Model.IterParent (out parent, selectedIter);
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

			if (GetDestRowAtPos (x, y, out path, out pos)) {
				Model.GetIter (out iter, path);
				FillElementAndPlaylist (iter, out destPlaylist, out destElement);
				
				/* Moving playlists */
				if (dragSourceElement == null) {
					project.Playlists.Remove (dragSourcePlaylist);
					project.Playlists.Insert (path.Indices [0], dragSourcePlaylist);
				} else {
					dragSourcePlaylist.Elements.Remove (dragSourceElement);
					destPlaylist.Elements.Insert (path.Indices [1], dragSourceElement);
				}
				
				if (pos == TreeViewDropPosition.Before ||
					pos == TreeViewDropPosition.IntoOrBefore) {
					(Model as TreeStore).MoveBefore (selectedIter, iter);
				} else {
					(Model as TreeStore).MoveAfter (selectedIter, iter);
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
			
			if (GetDestRowAtPos (x, y, out path, out pos)) {
				Model.GetIter (out iter, path);
				var el = Model.GetValue (iter, 0);

				/* Drag a playlist*/
				if (dragSourceElement == null) {
					if (el is Playlist) {
						DisableDragInto (path, context, time, pos);
						return true;
					} else {
						return false;
					}
				}
				/* Drag an element */
				else {
					if (el is IPlaylistElement) {
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
			base.OnDragBegin (context);
		}
	}
}
