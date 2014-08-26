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
using LongoMatch.Drawing.CanvasObjects;
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
		Dictionary<AnalysisCategory, CategoryTimeline> categories;

		public PlaysTimeline (IWidget widget): base(widget)
		{
			categories = new Dictionary<AnalysisCategory, CategoryTimeline> ();
			secondsPerPixel = 0.1;
			Accuracy = Constants.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
		}

		public void LoadProject (Project project, PlaysFilter filter)
		{
			this.project = project;
			ClearObjects ();
			categories.Clear ();
			duration = project.Description.File.Duration;
			widget.Height = project.Categories.CategoriesList.Count * StyleConf.TimelineCategoryHeight;
			if (project.Categories.Scores.Count > 0) {
				widget.Height += StyleConf.TimelineCategoryHeight;
			}
			if (project.Categories.PenaltyCards.Count > 0) {
				widget.Height += StyleConf.TimelineCategoryHeight;
			}
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

		public void AddPlay (Play play)
		{
			categories [play.Category].AddPlay (play);
		}

		public void RemovePlays (List<Play> plays)
		{
			foreach (Play p in plays) {
				categories [p.Category].RemoveNode (p);
				Selections.RemoveAll (s => (s.Drawable as PlayObject).Play == p);
			}
		}

		void Update ()
		{
			double width = duration.Seconds / SecondsPerPixel;
			widget.Width = width + 10;
			foreach (TimelineObject tl in categories.Values) {
				tl.Width = width + 10;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
		}

		Color ColorForRow (int row)
		{
			Color c;

			if (row % 2 == 0) {
				c = Config.Style.PaletteBackground;
			} else {
				c = Config.Style.PaletteBackgroundLight;
			}
			return c;
		}

		void FillCanvas ()
		{
			CategoryTimeline tl;
			int i = 0;

			List<Category> cats = project.Categories.CategoriesList; 
			List<Score> scores = project.Categories.Scores; 
			List<PenaltyCard> cards = project.Categories.PenaltyCards; 

			if (scores.Count > 0) {
				tl = new CategoryTimeline (project.ScorePlays, duration,
				                           i * StyleConf.TimelineCategoryHeight,
				                           ColorForRow (i));
				Objects.Add (tl);
				i++;
				foreach (Score s in scores) {
					categories [s] = tl;
				}
			}

			if (cards.Count > 0) {
				tl = new CategoryTimeline (project.PenaltyCardsPlays, duration,
				                           i * StyleConf.TimelineCategoryHeight,
				                           ColorForRow (i));
				Objects.Add (tl);
				i++;
				foreach (PenaltyCard pc in cards) {
					categories [pc] = tl;
				}
			}
			
			for (int j = 0; j < cats.Count; j++) {
				AnalysisCategory cat;
				cat = cats [j];
				tl = new CategoryTimeline (project.PlaysInCategory (cat), duration,
				                           i * StyleConf.TimelineCategoryHeight,
				                           ColorForRow (i));
				categories [cat] = tl;
				Objects.Add (tl);
				i++;
			}

			UpdateVisibleCategories ();
			Update ();
		}

		void UpdateVisibleCategories ()
		{
			int i = 0;
			foreach (CategoryTimeline ct in categories.Values) {
				ct.Visible = false;
				ct.OffsetY = -1;
			}
			
			foreach (AnalysisCategory cat in categories.Keys) {
				TimelineObject timeline = categories [cat];
				if (playsFilter.VisibleCategories.Contains (cat)) {
					if (timeline.OffsetY == -1) {
						timeline.OffsetY = i * timeline.Height;
						i++;
					}
					timeline.Visible |= true;
				} else {
					timeline.Visible |= false;
				}
			}
			widget.ReDraw ();
		}

		void RedrawSelection (Selection sel)
		{
			PlayObject po = sel.Drawable as PlayObject;
			widget.ReDraw (categories [po.Play.Category]);
		}

		protected override void SelectionChanged (List<Selection> selections)
		{
			if (selections.Count > 0) {
				PlayObject po = selections.Last ().Drawable as PlayObject;
				Config.EventsBroker.EmitPlaySelected (po.Play);
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
			AnalysisCategory cat = null;
			List<Play> plays = Selections.Select (p => (p.Drawable as PlayObject).Play).ToList ();
			
			foreach (AnalysisCategory ac in categories.Keys) {
				TimelineObject tl;
				Category c = ac as Category;
				if (c == null)
					continue;

				tl = categories [c];
				if (!tl.Visible)
					continue;
				if (coords.Y >= tl.OffsetY && coords.Y < tl.OffsetY + tl.Height) {
					cat = c;
					break;
				}
			}
			
			if ((cat != null || plays.Count > 0) && ShowMenuEvent != null) {
				ShowMenuEvent (plays, cat,
				               Utils.PosToTime (coords, SecondsPerPixel));
			}
		}

		protected override void SelectionMoved (Selection sel)
		{
			Time moveTime;
			Play play = (sel.Drawable as PlayObject).Play;
			
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

