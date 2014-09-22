//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Linq;
using LongoMatch.Core.Store;
using LongoMatch.Drawing.CanvasObjects;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store.Drawables;

namespace LongoMatch.Drawing.Widgets
{
	public class PlaysTimeline: SelectionCanvas
	{
	
		public event ShowTimelineMenuHandler ShowMenuEvent;

		Project project;
		EventsFilter playsFilter;
		double secondsPerPixel;
		Time duration;
		Dictionary<EventType, CategoryTimeline> eventsTimelines;

		public PlaysTimeline (IWidget widget): base(widget)
		{
			eventsTimelines = new Dictionary<EventType, CategoryTimeline> ();
			secondsPerPixel = 0.1;
			Accuracy = Constants.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
		}
		
		protected override void Dispose (bool disposing)
		{
			foreach (CategoryTimeline ct in eventsTimelines.Values) {
				ct.Dispose ();
			}
			base.Dispose (disposing);
		}

		public void LoadProject (Project project, EventsFilter filter)
		{
			int height;

			this.project = project;
			ClearObjects ();
			eventsTimelines.Clear ();
			duration = project.Description.File.Duration;
			height = project.EventTypes.Count * StyleConf.TimelineCategoryHeight;
			widget.Height = height;
			playsFilter = filter;
			FillCanvas ();
			filter.FilterUpdated += UpdateVisibleCategories;
		}

		public Time CurrentTime {
			set {
				foreach (CategoryTimeline tl in eventsTimelines.Values) {
					tl.CurrentTime = value;
				}
			}
		}

		public double SecondsPerPixel {
			set {
				secondsPerPixel = value;
				Update ();
			}
			get {
				return secondsPerPixel;
			}
		}

		public void AddPlay (TimelineEvent play)
		{
			eventsTimelines [play.EventType].AddPlay (play);
		}

		public void RemovePlays (List<TimelineEvent> plays)
		{
			foreach (TimelineEvent p in plays) {
				eventsTimelines [p.EventType].RemoveNode (p);
				Selections.RemoveAll (s => (s.Drawable as PlayObject).Play == p);
			}
		}

		void Update ()
		{
			double width = duration.Seconds / SecondsPerPixel;
			widget.Width = width + 10;
			foreach (TimelineObject tl in eventsTimelines.Values) {
				tl.Width = width + 10;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
		}

		void FillCanvas ()
		{
			CategoryTimeline tl;
			int i = 0;

			foreach (EventType type in project.EventTypes) {
				tl = new CategoryTimeline (project.EventsByType (type), duration,
				                           i * StyleConf.TimelineCategoryHeight,
				                           Utils.ColorForRow (i), playsFilter);
				eventsTimelines [type] = tl;
				AddObject (tl);
				i++;
			}
			UpdateVisibleCategories ();
			Update ();
		}

		void UpdateVisibleCategories ()
		{
			int i = 0;
			foreach (EventType type in project.EventTypes) {
				CategoryTimeline timeline = eventsTimelines [type];
				if (playsFilter.VisibleEventTypes.Contains (type)) {
					timeline.OffsetY = i * timeline.Height;
					timeline.Visible = true;
					timeline.BackgroundColor = Utils.ColorForRow (i);
					i++;
				} else {
					timeline.Visible = false;
				}
			}
			widget.ReDraw ();
		}

		void RedrawSelection (Selection sel)
		{
			PlayObject po = sel.Drawable as PlayObject;
			widget.ReDraw (eventsTimelines [po.Play.EventType]);
		}

		protected override void SelectionChanged (List<Selection> selections)
		{
			if (selections.Count > 0) {
				PlayObject po = selections.Last ().Drawable as PlayObject;
				Config.EventsBroker.EmitLoadEvent (po.Play);
			}
		}

		protected override void StartMove (Selection sel)
		{
			if (sel == null)
				return;

			if (sel.Position != SelectionPosition.All) {
				widget.SetCursor (CursorType.DoubleArrow);
			}
		}

		protected override void StopMove ()
		{
			widget.SetCursor (CursorType.Arrow);
		}

		protected override void ShowMenu (Point coords)
		{
			EventType ev = null;
			List<TimelineEvent> plays = Selections.Select (p => (p.Drawable as PlayObject).Play).ToList ();
			
			foreach (EventType evType in eventsTimelines.Keys) {
				TimelineObject tl;

				tl = eventsTimelines [evType];
				if (!tl.Visible)
					continue;
				if (coords.Y >= tl.OffsetY && coords.Y < tl.OffsetY + tl.Height) {
					ev = evType;
					break;
				}
			}
			
			if ((ev != null || plays.Count > 0) && ShowMenuEvent != null) {
				ShowMenuEvent (plays, ev, Utils.PosToTime (coords, SecondsPerPixel));
			}
		}

		protected override void SelectionMoved (Selection sel)
		{
			Time moveTime;
			TimelineEvent play = (sel.Drawable as PlayObject).Play;
			
			if (sel.Position == SelectionPosition.Right) {
				moveTime = play.Stop;
			} else {
				moveTime = play.Start;
			}
			Config.EventsBroker.EmitTimeNodeChanged (play, moveTime);
		}
		
		public override void Draw (IContext context, Area area)
		{
			tk.Context = context;
			tk.Begin ();
			tk.Clear (Config.Style.PaletteBackground);
			tk.End ();
			base.Draw (context, area);
		}
	}
}

