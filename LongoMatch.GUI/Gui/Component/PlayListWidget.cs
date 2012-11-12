// PlayListWidget.cs
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
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using Gtk;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;


namespace LongoMatch.Gui.Component
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayListWidget : Gtk.Bin, IPlaylistWidget
	{
		public event PlayListNodeSelectedHandler PlayListNodeSelected;
		public event ApplyCurrentRateHandler ApplyCurrentRate;
		public event OpenPlaylistHandler OpenPlaylistEvent;
		public event NewPlaylistHandler NewPlaylistEvent;
		public event SavePlaylistHandler SavePlaylistEvent;
		public event RenderPlaylistHandler RenderPlaylistEvent;
		
		IPlayList playlist;
		
		public PlayListWidget()
		{
			this.Build();
			playlisttreeview1.Reorderable = true;
			playlisttreeview1.RowActivated += OnPlaylisttreeview1RowActivated;
			playlisttreeview1.ApplyCurrentRate += OnApplyRate;
			savebutton.Sensitive = false;
			newvideobutton.Sensitive = false;

			newbutton.CanFocus = false;
			openbutton.CanFocus = false;
			savebutton.CanFocus = false;
			newvideobutton.CanFocus = false;
		}

		public void Load(IPlayList playlist) {
			this.playlist = playlist;
			label1.Visible = false;
			newvideobutton.Show();
			playlisttreeview1.PlayList = playlist;
			playlisttreeview1.Sensitive = true;
			savebutton.Sensitive = true;
			newvideobutton.Sensitive = true;
			Model = GetModel(playlist);
		}

		public ListStore Model {
			set {
				playlisttreeview1.Model = value;
			}
			get {
				return (ListStore)playlisttreeview1.Model;
			}
		}

		public void Add(PlayListPlay plNode) {
			Model.AppendValues(plNode);
		}
		
		public void SetActivePlay (PlayListPlay plNode, int index) {
			playlisttreeview1.Selection.SelectPath(new TreePath(index.ToString()));
			playlisttreeview1.LoadedPlay = plNode;
		}
		
		ListStore GetModel(IPlayList playlist) {
			ListStore listStore = new ListStore(typeof(PlayListPlay));
			foreach(PlayListPlay plNode in playlist) {
				listStore.AppendValues(plNode);
			}
			return listStore;
		}

		protected virtual void OnPlaylisttreeview1RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if (PlayListNodeSelected != null) {
				TreeIter iter;
				Model.GetIterFromString(out iter, args.Path.ToString());
				PlayListNodeSelected(Model.GetValue(iter, 0) as PlayListPlay);
			}
		}
		
		protected virtual void OnApplyRate(PlayListPlay plNode) {
			if(ApplyCurrentRate != null)
				ApplyCurrentRate(plNode);
		}
		
		protected virtual void OnSavebuttonClicked(object sender, System.EventArgs e)
		{
			if (SavePlaylistEvent != null)
				SavePlaylistEvent();
		}

		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			if (OpenPlaylistEvent != null)
				OpenPlaylistEvent();
		}

		protected virtual void OnNewbuttonClicked(object sender, System.EventArgs e)
		{
			if (NewPlaylistEvent != null)
				NewPlaylistEvent();
		}
		
		protected virtual void OnNewvideobuttonClicked(object sender, System.EventArgs e)
		{
			if (RenderPlaylistEvent != null)
				RenderPlaylistEvent((PlayList)playlist);
		}
	}
}
