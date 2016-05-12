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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces.Drawing;
using VAS.Core.Common;
using VAS.Core.Store.Drawables;
using VAS.Core.Store;
using VAS.Core;
using LongoMatch.Core.Store;

namespace LongoMatch.Drawing.CanvasObjects.Timeline
{
	public abstract class TimelineObject: CanvasObject, ICanvasSelectableObject
	{
		double secondsPerPixel;
		protected List<TimeNodeObject> nodes;
		protected Time maxTime;
		protected ISurface selectionBorderL, selectionBorderR;

		public TimelineObject (Time maxTime, int height, double offsetY, Color background)
		{
			this.BackgroundColor = background;
			this.nodes = new List<TimeNodeObject> ();
			this.maxTime = maxTime;
			selectionBorderL = LoadBorder (StyleConf.TimelineSelectionLeft);
			selectionBorderR = LoadBorder (StyleConf.TimelineSelectionRight);

			Visible = true;
			CurrentTime = new Time (0);
			OffsetY = offsetY;
			SecondsPerPixel = 0.1;
			Height = height;
		}

		protected override void Dispose (bool disposing)
		{
			ClearObjects ();
			selectionBorderL.Dispose ();
			selectionBorderR.Dispose ();
			base.Dispose (disposing);
		}

		public double SecondsPerPixel {
			set {
				secondsPerPixel = value;
				foreach (TimeNodeObject to in nodes) {
					to.SecondsPerPixel = secondsPerPixel;
					to.ResetDrawArea ();
				}
			}
			protected get {
				return secondsPerPixel;
			}
		}

		public Color BackgroundColor {
			get;
			set;
		}

		public Time CurrentTime {
			set;
			protected get;
		}

		public double Height {
			get;
			set;
		}

		public double Width {
			set;
			protected get;
		}

		public double OffsetY {
			set;
			get;
		}

		public void AddNode (TimeNodeObject o)
		{
			nodes.Add (o);
			o.RedrawEvent += HandleRedrawEvent;
		}

		public void InsertNode (int index, TimeNodeObject o)
		{
			nodes.Insert (index, o);
			o.RedrawEvent += HandleRedrawEvent;
		}

		public void RemoveNode (TimeNode node)
		{
			TimeNodeObject to;
			
			to = nodes.FirstOrDefault (n => n.TimeNode == node);
			if (to != null) {
				RemoveObject (to, true);
			}
		}

		public TimeNodeObject GetNodeAtPosition (double position)
		{
			TimeNodeObject node = nodes.FirstOrDefault (n => position >= n.StartX && position <= n.StopX);
			if (node == null) {
				node = nodes.LastOrDefault ();
			}
			return node;
		}

		protected void ClearObjects ()
		{
			foreach (TimeNodeObject tn in nodes) {
				RemoveObject (tn, false);
			}
			nodes.Clear ();
		}

		protected void RemoveObject (TimeNodeObject to, bool full)
		{
			to.RedrawEvent -= HandleRedrawEvent;
			to.Dispose ();
			if (full) {
				nodes.Remove (to);
			}
		}

		protected virtual bool TimeNodeObjectIsVisible (TimeNodeObject tn)
		{
			return true;
		}

		protected virtual void DrawBackground (IDrawingToolkit tk, Area area)
		{
			tk.FillColor = BackgroundColor;
			tk.StrokeColor = BackgroundColor;
			tk.LineWidth = 0;
			
			tk.DrawRectangle (new Point (area.Start.X, OffsetY), area.Width, Height);
		}

		void HandleRedrawEvent (ICanvasObject co, Area area)
		{
			EmitRedrawEvent (co as CanvasObject, area);
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double position;
			List<TimeNodeObject> selected;
			
			selected = new List<TimeNodeObject> ();

			if (!UpdateDrawArea (tk, area, new Area (new Point (0, OffsetY), Width, Height))) {
				return;
			}
			;

			tk.Begin ();
			DrawBackground (tk, area);
			foreach (TimeNodeObject p in nodes) {
				if (!TimeNodeObjectIsVisible (p))
					continue;
				if (p.Selected) {
					selected.Add (p);
					continue;
				}
				p.OffsetY = OffsetY;
				p.Draw (tk, area);
			}
			foreach (TimeNodeObject p in selected) {
				p.OffsetY = OffsetY;
				p.Draw (tk, area);
			}

			tk.FillColor = Config.Style.PaletteTool;
			tk.StrokeColor = Config.Style.PaletteTool;
			tk.LineWidth = Constants.TIMELINE_LINE_WIDTH;
			position = Utils.TimeToPos (CurrentTime, secondsPerPixel);
			tk.DrawLine (new Point (position, OffsetY),
				new Point (position, OffsetY + Height));
			
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection selection = null;

			if (point.Y >= OffsetY && point.Y < OffsetY + Height) {
				foreach (TimeNodeObject po in nodes) {
					Selection tmp;
					if (!TimeNodeObjectIsVisible (po))
						continue;
					tmp = po.GetSelection (point, precision);
					if (tmp == null) {
						continue;
					}
					if (tmp.Accuracy == 0) {
						selection = tmp;
						break;
					}
					if (selection == null || tmp.Accuracy < selection.Accuracy) {
						selection = tmp;
					}
				}
			}
			return selection;
		}

		public void Move (Selection s, Point p, Point start)
		{
			s.Drawable.Move (s, p, start);
		}

		ISurface LoadBorder (string name)
		{
			Image img = Resources.LoadImage (name);
			img.Scale (StyleConf.TimelineCategoryHeight, StyleConf.TimelineCategoryHeight);
			return Config.DrawingToolkit.CreateSurface (img.Width, img.Height, img);
		}
	}

	public class CategoryTimeline: TimelineObject
	{
		EventsFilter filter;
		ProjectLongoMatch project;

		public CategoryTimeline (ProjectLongoMatch project, IEnumerable<TimelineEventLongoMatch> plays, Time maxTime,
		                         double offsetY, Color background, EventsFilter filter) :
			base (maxTime, StyleConf.TimelineCategoryHeight, offsetY, background)
		{
			this.filter = filter;
			this.project = project;
			foreach (TimelineEventLongoMatch p in plays) {
				AddPlay (p);
			}
		}

		public TimelineEventObject Load (TimelineEvent evt)
		{
			return nodes.FirstOrDefault (n => (n as TimelineEventObject).Event == evt) as TimelineEventObject;
		}

		protected override bool TimeNodeObjectIsVisible (TimeNodeObject tn)
		{
			return filter.IsVisible ((tn as TimelineEventObject).Event);
		}

		public void AddPlay (TimelineEventLongoMatch play)
		{
			TimelineEventObject po = new TimelineEventObject (play, project);
			po.SelectionLeft = selectionBorderL; 
			po.SelectionRight = selectionBorderR; 
			po.OffsetY = OffsetY;
			po.Height = Height;
			po.SecondsPerPixel = SecondsPerPixel;
			po.MaxTime = maxTime;
			AddNode (po);
		}
	}

	public class TimerTimeline: TimelineObject
	{

		List<Timer> timers;

		public TimerTimeline (List<Timer> timers, bool showName, NodeDraggingMode draggingMode, bool showLine,
		                      Time maxTime, int height, double offsetY, Color background, Color lineColor) :
			base (maxTime, height, offsetY, background)
		{
			this.timers = timers;
			ShowName = showName;
			DraggingMode = draggingMode;
			ShowLine = showLine;
			LineColor = lineColor;
	
			ReloadPeriods (timers);
		}

		public TimerTimeline (List<Timer> timers, bool showName, NodeDraggingMode draggingMode, bool showLine,
		                      Time maxTime, double offsetY, Color background, Color lineColor) :
			this (timers, showName, draggingMode, showLine, maxTime, StyleConf.TimelineCategoryHeight, offsetY, background, lineColor)
		{

		}

		Color LineColor {
			get;
			set;
		}

		bool ShowLine {
			get;
			set;
		}

		bool ShowName {
			get;
			set;
		}

		NodeDraggingMode DraggingMode {
			get;
			set;
		}

		public bool HasNode (TimeNode tn)
		{
			return nodes.FirstOrDefault (n => n.TimeNode == tn) != null;
		}

		public bool HasTimer (Timer timer)
		{
			return timers.Contains (timer);
		}

		public void AddTimer (Timer timer, bool newtimer = true)
		{
			foreach (TimeNode tn in timer.Nodes) {
				AddTimeNode (timer, tn);
			}
			if (newtimer) {
				timers.Add (timer);
			}
			ReDraw ();
		}

		public void RemoveTimer (Timer timer)
		{
			TimerTimeNodeObject to = (TimerTimeNodeObject)nodes.FirstOrDefault (t => (t as TimerTimeNodeObject).Timer == timer);
			if (to != null) {
				RemoveObject (to, true);
			}
			if (timers.Contains (timer)) {
				timers.Remove (timer);
			}
			ReDraw ();
		}

		public void AddTimeNode (Timer t, TimeNode tn)
		{
			TimerTimeNodeObject to = new TimerTimeNodeObject (t, tn);
			to.OffsetY = OffsetY;
			to.Height = Height;
			to.SecondsPerPixel = SecondsPerPixel;
			to.MaxTime = maxTime;
			to.DraggingMode = DraggingMode;
			to.ShowName = ShowName;
			to.LineColor = LineColor;
			AddNode (to);
		}

		public void ReloadPeriods (List<Timer> timers)
		{
			ClearObjects ();
			foreach (Timer t in timers) {
				AddTimer (t, false);
			}
		}

		protected override void DrawBackground (IDrawingToolkit tk, Area area)
		{
			base.DrawBackground (tk, area);

			if (ShowLine) {
				// We want the background line and overlay to use the same starting point although they have different sizes.
				double linepos = OffsetY + Height / 2 + StyleConf.TimelineLineSize / 2;
				tk.FillColor = Config.Style.PaletteBackgroundDark;
				tk.StrokeColor = Config.Style.PaletteBackgroundDark;
				tk.LineWidth = StyleConf.TimelineBackgroundLineSize;
				tk.DrawLine (new Point (0, linepos),
					new Point (Width, linepos));
			}
		}
	}

	public class CameraTimeline: TimelineObject
	{
		public CameraTimeline (MediaFile mediaFile, bool showName, bool showLine,
		                       Time maxTime, double offsetY, Color background, Color lineColor) :
			base (maxTime, StyleConf.TimelineCameraHeight, offsetY, background)
		{
			ShowName = showName;
			ShowLine = showLine;
			LineColor = lineColor;
			Height = StyleConf.TimelineCameraHeight;

			AddMediaFile (mediaFile);
		}

		Color LineColor {
			get;
			set;
		}

		bool ShowLine {
			get;
			set;
		}

		bool ShowName {
			get;
			set;
		}

		public void AddMediaFile (MediaFile mediaFile)
		{
			CameraObject co = new CameraObject (mediaFile);
			co.OffsetY = OffsetY;
			co.Height = Height;
			co.SecondsPerPixel = SecondsPerPixel;
			co.DraggingMode = NodeDraggingMode.Segment;
			co.MaxTime = maxTime;
			co.ShowName = ShowName;
			co.LineColor = LineColor;
			AddNode (co);
		}
	}
}

