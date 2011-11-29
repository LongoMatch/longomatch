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

namespace LongoMatch.Gui.Component {

	public class TimeLineWidget : TimelineBase<TimeScale, Play> 
	{

		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlaySelectedHandler TimeNodeSelected;
		public event PlaysDeletedHandler TimeNodeDeleted;
		public event NewTagAtFrameHandler NewMarkEvent;

		private Categories categories;

		public TimeLineWidget(): base()
		{
		}

		public Project Project {
			set {
				ResetGui();

				if(value == null) {
					categories = null;
					tsList.Clear();
					loaded = false;
					return;
				}
				loaded = true;
				categories = value.Categories;
				tsList.Clear(); 
				frames = value.Description.File.GetFrames();

				cs.Categories = categories;
				cs.Show();

				tr.Frames = frames;
				tr.FrameRate = value.Description.File.Fps;
				tr.Show();

				foreach(Category cat in  categories) {
					List<Play> playsList = value.PlaysInCategory(cat);
					TimeScale ts = new TimeScale(cat, playsList,frames);
					tsList[cat] = ts;
					ts.TimeNodeChanged += new TimeNodeChangedHandler(OnTimeNodeChanged);
					ts.TimeNodeSelected += new PlaySelectedHandler(OnTimeNodeSelected);
					ts.TimeNodeDeleted += new PlaysDeletedHandler(OnTimeNodeDeleted);
					ts.NewMarkAtFrameEvent += new NewTagAtFrameHandler(OnNewMark);
					TimelineBox.PackStart(ts,true,true,0);
					ts.Show();
				}
				SetPixelRatio(3);
			}
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
		
		protected virtual void OnNewMark(Category category, int frame) {
			if(NewMarkEvent != null)
				NewMarkEvent(category,frame);
		}

		protected virtual void OnTimeNodeChanged(TimeNode tn, object val) {
			if(TimeNodeChanged != null)
				TimeNodeChanged(tn,val);
		}

		protected virtual void OnTimeNodeSelected(Play tn) {
			if(TimeNodeSelected != null)
				TimeNodeSelected(tn);
		}
		
		protected virtual void OnTimeNodeDeleted(List<Play> plays) {
			if(TimeNodeDeleted != null)
				TimeNodeDeleted(plays);
		}
	}
}
