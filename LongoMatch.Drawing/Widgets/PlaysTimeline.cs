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
	/*      Widget schematic
	 *
	 *  time |___|___|___|___|__|
	 *  cat1    ---     -- -
	 *  cat2 --  -------  --
	 *  cat3    ----    ----
	 */
	 
	public class PlaysTimeline: Canvas
	{
	
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlaySelectedHandler TimeNodeSelected;
		public event ShowTimelineMenuHandler ShowMenu;

		Project project;
		double secondsPerPixel;
		int duration;
		uint lastTime;
		bool moving;
		List<Selection> selectionList;
		Point start;
		Dictionary<Category, CategoryTimeline> categories;
		
		public PlaysTimeline (IWidget widget): base(widget)
		{
			categories = new Dictionary<Category, CategoryTimeline> ();
			secondsPerPixel = 0.1;
			widget.ButtonPressEvent += HandleButtonPressEvent;
			widget.ButtonReleasedEvent += HandleButtonReleasedEvent;
			widget.MotionEvent += HandleMotionEvent;
			selectionList = new List<Selection> ();
		}

		public void LoadProject (Project project, PlaysFilter filter) {
			this.project = project;
			Objects.Clear();
			categories.Clear();
			duration = new Time ((int)project.Description.File.Length).Seconds; 
			widget.Height = project.Categories.Count * Common.CATEGORY_HEIGHT;
			FillCanvas ();
			filter.FilterUpdated += () => {
				//Visible = filter.VisibleCategories.Contains (category);
				//QueueDraw();
			};	
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
				categories[p.Category].RemovePlay (p);
				selectionList.RemoveAll (s => (s.Drawable as PlayObject).Play == p);
			}
		}
		
		void Update () {
			double width = duration / SecondsPerPixel;
			widget.Width = width;
			foreach (object o in Objects) {
				CategoryTimeline tl = o as CategoryTimeline;
				tl.Width = width;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
		}
		
		void FillCanvas () {
			for (int i=0; i<project.Categories.Count; i++) {
				Category cat;
				CategoryTimeline tl;
				Color c;
				
				if (i % 2 == 0) {
					c = Color.White;
				} else {
					c = Color.Grey;
				}
				
				cat = project.Categories[i];
				tl = new CategoryTimeline (project.PlaysInCategory (cat),
				                           i * Common.CATEGORY_HEIGHT, c);
				categories[cat] = tl;
				Objects.Add (tl);
			}
			Update ();
		}
		
		void RedrawSelection (Selection sel)
		{
			PlayObject po = sel.Drawable as PlayObject;
			widget.ReDraw (categories[po.Play.Category]);
		}		
		
		void ClearSelection () {
			foreach (Selection sel in selectionList) {
				PlayObject po = sel.Drawable as PlayObject;
				po.Selected = false;
				widget.ReDraw (po);
			}
			selectionList.Clear ();
		}
		
		void UpdateSelection (Selection sel) {
			PlayObject po = sel.Drawable as PlayObject;
			Selection seldup = selectionList.FirstOrDefault (s => s.Drawable == sel.Drawable);
			
			if (seldup != null) {
				po.Selected = false;
				selectionList.Remove (seldup);
			} else {
				po.Selected = true;
				selectionList.Add (sel);
				if (TimeNodeSelected != null) {
					TimeNodeSelected (po.Play);
				}
			}
			widget.ReDraw (po);
		}
		
		void HandleLeftButton (Point coords, ButtonModifier modif) {
			Selection sel = null;

			foreach (CategoryTimeline tl in categories.Values) {
				sel = tl.GetSelection (coords, Common.TIMELINE_ACCURACY);
				if (sel != null) {
					break;
				}
			}

			if (modif == ButtonModifier.Control || modif == ButtonModifier.Shift) {
				if (sel != null) {
					UpdateSelection (sel);
				}
			} else {
				ClearSelection ();
				if (sel == null) {
					return;
				}
				UpdateSelection (sel);
				start = coords;
				if (sel.Position != SelectionPosition.All) {
					widget.SetCursor (CursorType.DoubleArrow);
					moving = true;
				}
			}
		}
		
		void HandleRightButton (Point coords) {
			List<Play> plays = selectionList.Select (p => (p.Drawable as PlayObject).Play).ToList();
			
			if (ShowMenu != null) {
				ShowMenu (plays, null,
				          Common.PosToTime (coords, SecondsPerPixel));
			}
		}

		void HandleMotionEvent (Point coords)
		{
			Selection sel;

			if (!moving)
				return;
			
			sel = selectionList[0];
			sel.Drawable.Move (sel, coords, start);  
			RedrawSelection (selectionList[0]);
			if (TimeNodeChanged != null) {
				Time time;
				Play play = (sel.Drawable as PlayObject).Play;
				
				if (sel.Position == SelectionPosition.Left) {
					time = play.Start;
				} else {
					time = play.Stop;
				}
				TimeNodeChanged (play, time);
			}
		}

		void HandleButtonReleasedEvent (Point coords, ButtonType type, ButtonModifier modifier)
		{
			if (type == ButtonType.Left) {
				widget.SetCursor (CursorType.Arrow);
				moving = false;
			}
		}

		void HandleButtonPressEvent (Point coords, uint time, ButtonType type, ButtonModifier modifier)
		{
			if (time - lastTime < 500) {
				return;
			}
			if (type == ButtonType.Left) {
				HandleLeftButton (coords, modifier);
			} else if (type == ButtonType.Right) {
				HandleRightButton (coords);
			}
			lastTime = time;
		}
	}
}

