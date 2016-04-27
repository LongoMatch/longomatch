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
using LongoMatch.Core.Store;
using LongoMatch.Drawing.CanvasObjects.Timeline;
using VAS.Core;
using VAS.Core.Store;
using VAS.Core.Common;

namespace LongoMatch.Drawing.Widgets
{
	public class TimelineLabels: Canvas
	{
		ProjectLongoMatch project;
		EventsFilter filter;
		Dictionary<LabelObject, object> labelToObject;

		public TimelineLabels (IWidget widget) : base (widget)
		{
			labelToObject = new Dictionary<LabelObject, object> ();
		}

		public TimelineLabels () : this (null)
		{
		}

		public double Scroll {
			set {
				foreach (var o in Objects) {
					LabelObject cl = o as LabelObject;
					cl.Scroll = value; 
				}
			}
		}

		public void LoadProject (ProjectLongoMatch project, EventsFilter filter)
		{
			ClearObjects ();
			this.project = project;
			this.filter = filter;
			if (project != null) {
				FillCanvas ();
				UpdateVisibleCategories ();
				filter.FilterUpdated += UpdateVisibleCategories;
			}
		}

		void AddLabel (LabelObject label, object obj)
		{
			Objects.Add (label);
			labelToObject [label] = obj;
		}

		void FillCanvas ()
		{
			LabelObject l;
			int i = 0, w, h;

			w = StyleConf.TimelineLabelsWidth;
			h = StyleConf.TimelineCategoryHeight;

			l = new LabelObject (w, h, i * h);
			l.Name = Catalog.GetString ("Periods");
			AddLabel (l, null);
			i++;

			foreach (Timer t in project.Timers) {
				l = new TimerLabelObject (t, w, h, i * h);
				AddLabel (l, t);
				i++;
			}

			foreach (EventType eventType in project.EventTypes) {
				/* Add the category label */
				l = new EventTypeLabelObject (eventType, w, h, i * h);
				AddLabel (l, eventType);
				i++;
			}
			
			double width = labelToObject.Keys.Max (la => la.RequiredWidth);
			foreach (LabelObject lo in labelToObject.Keys) {
				lo.Width = width;
			}
			WidthRequest = (int)width;
		}

		void UpdateVisibleCategories ()
		{
			int i = 0;
			foreach (LabelObject label in Objects) {
				if (filter.IsVisible (labelToObject [label])) {
					label.OffsetY = i * label.Height;
					label.Visible = true;
					label.BackgroundColor = Utils.ColorForRow (i);
					i++;
				} else {
					label.Visible = false;
				}
			}
			widget?.ReDraw ();
		}
	}
}
