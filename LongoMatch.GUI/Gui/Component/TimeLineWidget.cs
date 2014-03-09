// TimeLineWidget.cs
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
using System.Linq;
using Gtk;

using LongoMatch.Gui.Base;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Common;
using LongoMatch.Interfaces;

namespace LongoMatch.Gui.Component {

	public class TimeLineWidget : TimelineBase<TimeScale, Play> 
	{

		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlaySelectedHandler TimeNodeSelected;
		public event PlaysDeletedHandler TimeNodeDeleted;
		public event NewTagAtFrameHandler NewMarkEvent;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeries;
		public event TagPlayHandler TagPlay;
		public event RenderPlaylistHandler RenderPlaylist;

		private Categories categories;

		public TimeLineWidget(): base()
		{
		}
		
		public void SetProject (Project project, PlaysFilter filter) {
			ResetGui();
			
			if(project == null) {
				categories = null;
				tsList.Clear();
				loaded = false;
				return;
			}
			loaded = true;
			categories = project.Categories;
			tsList.Clear(); 
			frames = project.Description.File.GetFrames();
			
			cs.Categories = categories;
			cs.Filter = filter;
			cs.Show();
			
			tr.Frames = frames;
			tr.FrameRate = project.Description.File.Fps;
			tr.Show();
			
			foreach(Category cat in  categories) {
				List<Play> playsList = project.PlaysInCategory(cat);
				TimeScale ts = new TimeScale(cat, playsList, frames, filter,
				                             project.Description.File);
				tsList[cat] = ts;
				ts.TimeNodeChanged += HandleTimeNodeChanged;
				ts.TimeNodeSelected += HandleTimeNodeSelected;
				ts.TimeNodeDeleted += HandleTimeNodeDeleted;
				ts.NewMarkAtFrameEvent += HandleNewMark;
				ts.TagPlay += HandleTagPlay;
				ts.RenderPlaylist += HandleRenderPlaylist;
				ts.SnapshotSeries += HandleSnapshotSeries;
				ts.PlayListNodeAdded += HandlePlayListNodeAdded; 
				TimelineBox.PackStart(ts,false,true,0);
				ts.Show();
			}
			SetPixelRatio(3);
		}

		public void AddPlay(Play play) {
			TimeScale ts;
			if(tsList.TryGetValue(play.Category, out ts))
				ts.AddPlay(play);
		}

		public void RemovePlays(List<Play> plays) {
			TimeScale ts;
			foreach(Play play in plays) {
				if(tsList.TryGetValue(play.Category, out ts))
					ts.RemovePlay(play);
			}
		}
		
		void HandleNewMark(Category category, int frame) {
			if(NewMarkEvent != null)
				NewMarkEvent(category,frame);
		}

		void HandleTimeNodeChanged(TimeNode tn, object val) {
			if(TimeNodeChanged != null)
				TimeNodeChanged(tn,val);
		}

		void HandleTimeNodeSelected(Play tn) {
			if(TimeNodeSelected != null)
				TimeNodeSelected(tn);
		}
		
		void HandleTimeNodeDeleted(List<Play> plays) {
			if(TimeNodeDeleted != null)
				TimeNodeDeleted(plays);
		}
		
		void HandleTagPlay (Play play)
		{
			if (TagPlay != null)
				TagPlay (play);
		}

		void HandlePlayListNodeAdded (List<Play> plays)
		{
			if (PlayListNodeAdded != null)
				PlayListNodeAdded (plays);
		}

		void HandleSnapshotSeries (Play play)
		{
			if (SnapshotSeries != null)
				SnapshotSeries (play);
		}

		void HandleRenderPlaylist (IPlayList playlist)
		{
			if (RenderPlaylist != null)
				RenderPlaylist (playlist);
		}

	}
}
