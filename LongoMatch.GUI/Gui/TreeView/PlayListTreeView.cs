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
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public class PlayListTreeView : Gtk.TreeView
	{
		Project project;
		TreeIter selectedIter;
		TreePath pathClicked;
		Playlist dragSourcePlaylist;
		IPlaylistElement dragSourceElement;
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

		void AddVideo (Playlist playlist, IPlaylistElement element, bool prepend, TreeIter parent)
		{
			MediaFile file = LongoMatch.Gui.Helpers.Misc.OpenFile (this);
			if (file != null) {
				PlaylistVideo video = new PlaylistVideo (file);
				int index = playlist.Elements.IndexOf (element);
				if (!prepend) {
					index++;
				}
				playlist.Elements.Insert (index, video);
				(Model as TreeStore).InsertWithValues (parent, index, video);
			}
		}

		void AddImage (Playlist playlist, IPlaylistElement element, bool prepend, TreeIter parent)
		{
			Pixbuf pix = LongoMatch.Gui.Helpers.Misc.OpenImage (this);
			if (pix != null) {
				var image = new LongoMatch.Core.Common.Image (pix);
				PlaylistImage plimage = new PlaylistImage (image, new Time (5000));
				int index = playlist.Elements.IndexOf (element);
				if (!prepend) {
					index++;
				}
				playlist.Elements.Insert (index, plimage);
				(Model as TreeStore).InsertWithValues (parent, index, plimage);
			}
		}

		Menu CreateExternalsMenu (Playlist playlist, IPlaylistElement element, bool prepend, TreeIter parent)
		{
			Menu addMenu = new Menu ();
			MenuItem video = new MenuItem (Catalog.GetString ("External video"));
			video.Activated += (sender, e) => AddVideo (playlist, element, prepend, parent);
			addMenu.Append (video);
			MenuItem stillImage = new MenuItem (Catalog.GetString ("External image"));
			stillImage.Activated += (sender, e) => AddImage (playlist, element, prepend, parent);
			addMenu.Append (stillImage);
			return addMenu;
		}

		void ShowPlaylistElementMenu (Playlist playlist, IPlaylistElement element, TreeIter parent)
		{
			Menu menu;
			MenuItem edit, delete, prepend, append;

			menu = new Menu ();

			if (!(element is PlaylistVideo)) {
				edit = new MenuItem (Catalog.GetString ("Edit properties"));
				edit.Activated += (sender, e) => {
					EditPlaylistElementProperties dialog = new EditPlaylistElementProperties ((Gtk.Window)Toplevel, element);
					dialog.Run ();
					dialog.Destroy ();
				};
				menu.Append (edit);
			}

			prepend = new MenuItem (Catalog.GetString ("Insert before"));
			prepend.Submenu = CreateExternalsMenu (playlist, element, true, parent);
			menu.Append (prepend);
			
			append = new MenuItem (Catalog.GetString ("Insert after"));
			append.Submenu = CreateExternalsMenu (playlist, element, false, parent);
			menu.Append (append);

			delete = new MenuItem (Catalog.GetString ("Delete"));
			delete.Activated += (sender, e) => {
				playlist.Remove (element);
				(Model as TreeStore).Remove (ref selectedIter);
				Config.EventsBroker.EmitPlaylistsChanged (this);
			};
			menu.Append (delete);
			
			menu.ShowAll ();
			menu.Popup ();
		}

		void ShowPlaylistMenu (Playlist playlist)
		{
			Menu menu;
			MenuItem edit, delete, render;

			menu = new Menu ();

			edit = new MenuItem (Catalog.GetString ("Edit name"));
			edit.Activated += (sender, e) => {
				string name = Config.GUIToolkit.QueryMessage (Catalog.GetString ("Name:"), null,
					              playlist.Name).Result;
				if (!String.IsNullOrEmpty (name)) {
					playlist.Name = name;
				}
			};
			menu.Append (edit);

			render = new MenuItem (Catalog.GetString ("Render"));
			render.Activated += (sender, e) => {
				Config.EventsBroker.EmitRenderPlaylist (playlist);
			};
			menu.Append (render);

			delete = new MenuItem (Catalog.GetString ("Delete"));
			delete.Activated += (sender, e) => {
				project.Playlists.Remove (playlist);
				(Model as TreeStore).Remove (ref selectedIter);
				Config.EventsBroker.EmitPlaylistsChanged (this);
			};
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
						ShowPlaylistElementMenu (playlist, el as IPlaylistElement, parent);
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
					if (pos == TreeViewDropPosition.Before ||
					    pos == TreeViewDropPosition.IntoOrBefore) {
						store.MoveBefore (selectedIter, iter);
					} else {
						store.MoveAfter (selectedIter, iter);
					}
				} else {
					/* For elements moves can happen between 2 playlists and Move{Before|After}
					 * requires iter to have the same parent */
					TreeIter newIter;
					IPlaylistElement srcCurrent, dstCurrent;

					if (pos == TreeViewDropPosition.Before ||
					    pos == TreeViewDropPosition.IntoOrBefore) {
						newIter = (Model as TreeStore).InsertNodeBefore (iter);
					} else {
						newIter = (Model as TreeStore).InsertNodeAfter (iter);
					}
					store.SetValue (newIter, 0, dragSourceElement);
					store.Remove (ref selectedIter);
					
					srcCurrent = dragSourcePlaylist.Selected;
					dstCurrent = destPlaylist.Selected;
					
					dragSourcePlaylist.Elements.Remove (dragSourceElement);
					destPlaylist.Elements.Insert (store.GetPath (newIter).Indices [1], dragSourceElement);
					
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
	}
}
