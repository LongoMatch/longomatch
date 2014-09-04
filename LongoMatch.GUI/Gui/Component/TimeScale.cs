// TimeScale.cs
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
using Cairo;
using Gdk;
using Gtk;
using Pango;
using Mono.Unix;
using LongoMatch.Common;
using LongoMatch.Gui.Base;
using LongoMatch.Core.Handlers;
using LongoMatch.Store;


namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class TimeScale : TimeScaleBase<Play>
	{
		Category category;
		MediaFile mediaFile;
		
		public event NewTagAtFrameHandler NewMarkAtFrameEvent;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlaySelectedHandler TimeNodeSelected;
		public event PlaysDeletedHandler TimeNodeDeleted;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeries;
		public event TagPlayHandler TagPlay;
		public event RenderPlaylistHandler RenderPlaylist;


		public TimeScale(Category category, List<Play> list, uint frames, PlaysFilter filter, MediaFile mediaFile): base(list, frames, filter)
		{
			this.category = category;
			this.mediaFile = mediaFile;
			elementName = Catalog.GetString("play");
			filter.FilterUpdated += () => {
				Visible = filter.VisibleCategories.Contains (category);
				QueueDraw();
			};
		}

		override protected void ExpandMenu (List<Play> plays, Dictionary<Play, Menu> menusDict) {
			foreach (Play play in plays) {
				MenuItem tag, addPLN, snapshot, render;
				Menu menu = menusDict[play];
			
				tag = new MenuItem(Catalog.GetString("Edit tags"));
				addPLN = new MenuItem(Catalog.GetString("Add to playlist"));
				render = new MenuItem(Catalog.GetString("Export to video file"));
				snapshot = new MenuItem(Catalog.GetString("Export to PGN images"));
				
				tag.Activated += HandleTag;
				addPLN.Activated += HandleAddPlayListNode;
				render.Activated += HandleRender;
				snapshot.Activated += HandleSnapshot;
				
				menu.Add (tag);
				menu.Add (addPLN);
				menu.Add (render);
				menu.Add (snapshot);
				menuToNodeDict.Add (tag, play);
				menuToNodeDict.Add (addPLN, play);
				menuToNodeDict.Add (render, play);
				menuToNodeDict.Add (snapshot, play);
			}
		}

		public void AddPlay(Play play) {
			AddTimeNode(play);
		}

		public void RemovePlay(Play play) {
			RemoveTimeNode(play);
		}

		override protected void HandleTimeNodeChanged(Play play, Time time) {
			if (TimeNodeChanged != null)
				TimeNodeChanged(play, time);
		}
		
		override protected void HandleTimeNodeSelected(Play play) {
			if (TimeNodeSelected != null)
				TimeNodeSelected(play);
		}
		
		override protected void HandleTimeNodeDeleted(List<Play> plays) {
			if (TimeNodeDeleted != null)
				TimeNodeDeleted(plays);
		}
		
		override protected void AddNewTimeNode() {
			if (NewMarkAtFrameEvent != null)
				NewMarkAtFrameEvent(category, cursorFrame);
		}
		
		void HandleSnapshot (object sender, EventArgs e)
		{
			if (SnapshotSeries != null)
				SnapshotSeries (menuToNodeDict[sender as MenuItem]);
			
		}

		void HandleRender (object sender, EventArgs e)
		{
			if (RenderPlaylist != null) {
				PlayList pl = new PlayList();
				pl.Add (new PlayListPlay (menuToNodeDict[sender as MenuItem],
				                          mediaFile, true));
				RenderPlaylist (pl);
			}
			
		}

		void HandleAddPlayListNode (object sender, EventArgs e)
		{
			if (PlayListNodeAdded != null) {
				List<Play> list = new List<Play>();
				list.Add (menuToNodeDict[sender as MenuItem]);
				PlayListNodeAdded (list);
			}
			
		}

		void HandleTag (object sender, EventArgs e)
		{
			if (TagPlay != null)
				TagPlay(menuToNodeDict[sender as MenuItem]);
		}
	}
}
