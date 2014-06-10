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
using LongoMatch.Store;
using LongoMatch.Drawing.CanvasObject;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store.Drawables;

namespace LongoMatch.Drawing.Widgets
{
	 
	public class PlaysTimeline: SelectionCanvas
	{
	
		public event ShowTimelineMenuHandler ShowMenuEvent;

		Project project;
		PlaysFilter playsFilter;
		double secondsPerPixel;
		Time duration;
		Dictionary<Category, CategoryTimeline> categories;
		
		public PlaysTimeline (IWidget widget): base(widget)
		{
			categories = new Dictionary<Category, CategoryTimeline> ();
			secondsPerPixel = 0.1;
			Accuracy = Common.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
		}

		public void LoadProject (Project project, PlaysFilter filter) {
			this.project = project;
			Objects.Clear();
			categories.Clear();
			duration = project.Description.File.Duration;
			widget.Height = project.Categories.List.Count * Common.CATEGORY_HEIGHT;
			playsFilter = filter;
			FillCanvas ();
			filter.FilterUpdated += UpdateVisibleCategories;
		}
		
		public Time CurrentTime {
			set {
				foreach (CategoryTimeline tl in categories.Values) {
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
		
		public void AddPlay(Play play) {
			categories[play.Category].AddPlay (play);
		}

		public void RemovePlays(List<Play> plays) {
			foreach (Play p in plays) {
				categories[p.Category].RemoveNode (p);
				Selections.RemoveAll (s => (s.Drawable as PlayObject).Play == p);
			}
		}
		
		void Update () {
			double width = duration.Seconds / SecondsPerPixel;
			widget.Width = width;
			foreach (TimelineObject tl in categories.Values) {
				tl.Width = width;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
		}
		
		void FillCanvas () {
			for (int i=0; i<project.Categories.List.Count; i++) {
				Category cat;
				CategoryTimeline tl;
				Color c;
				
				if (i % 2 == 0) {
					c = Color.White;
				} else {
					c = Color.Grey1;
				}
				
				cat = project.Categories.List[i];
				tl = new CategoryTimeline (project.PlaysInCategory (cat),
				                           duration, i * Common.CATEGORY_HEIGHT, c);
				categories[cat] = tl;
				Objects.Add (tl);
			}
			UpdateVisibleCategories ();
			Update ();
		}
		
		void UpdateVisibleCategories () {
			int i=0;
			foreach (Category cat in categories.Keys) {
				TimelineObject timeline = categories[cat];
				if (playsFilter.VisibleCategories.Contains (cat)) {
					timeline.OffsetY = i * Common.CATEGORY_HEIGHT;
					timeline.Visible = true;
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
			widget.ReDraw (categories[po.Play.Category]);
		}		
		
		protected override void SelectionChanged (List<Selection> selections) {
			if (selections.Count > 0) {
				PlayObject po = selections.Last().Drawable as PlayObject;
				Config.EventsBroker.EmitPlaySelected (po.Play);
			}
		}
		
		protected override void StartMove (Selection sel) {
			if (sel.Position != SelectionPosition.All) {
				widget.SetCursor (CursorType.DoubleArrow);
			}
		}
		
		protected override void StopMove () {
			widget.SetCursor (CursorType.Arrow);
		}

		protected override void ShowMenu (Point coords) {
			Category cat = null;
			List<Play> plays = Selections.Select (p => (p.Drawable as PlayObject).Play).ToList();
			
			foreach (Category c in categories.Keys) {
				TimelineObject tl = categories[c];
				if (!tl.Visible)
					continue;
				if (coords.Y >= tl.OffsetY && coords.Y < tl.OffsetY + Common.CATEGORY_HEIGHT) {
					cat = c;
					break;
				}
			}
			
			if (cat != null && ShowMenuEvent != null) {
				ShowMenuEvent (plays, cat,
				          Common.PosToTime (coords, SecondsPerPixel));
			}
		}
		
		protected override void SelectionMoved (Selection sel) {
			Time moveTime;
			Play play = (sel.Drawable as PlayObject).Play;
			
			if (sel.Position == SelectionPosition.Right) {
				moveTime = play.Stop;
			} else {
				moveTime = play.Start;
			}
			Config.EventsBroker.EmitTimeNodeChanged (play, moveTime);
		}
	}
}

