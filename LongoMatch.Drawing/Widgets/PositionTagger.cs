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
using LongoMatch.Core.Filters;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Drawing.CanvasObjects;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Drawing;
using LMCommon = LongoMatch.Core.Common;
using VASDrawing = VAS.Drawing;
using VAS.Core.Store;

namespace LongoMatch.Drawing.Widgets
{
	public class PositionTagger: BackgroundCanvas
	{
	
		public event ShowTaggerMenuHandler ShowMenuEvent;

		EventsFilter filter;
		TimelineEventLongoMatch playSelected;

		public PositionTagger (IWidget widget) : base (widget)
		{
			Accuracy = VASDrawing.Constants.TAGGER_POINT_SIZE + 3;
			EmitSignals = true;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			BackgroundColor = Config.Style.PaletteBackground;
		}

		public PositionTagger (IWidget widget, ProjectLongoMatch project, List<TimelineEventLongoMatch> plays,
		                       Image background, FieldPositionType position) : base (widget)
		{
			Project = project;
			Background = background;
			Plays = plays;
			FieldPosition = position;
			BackgroundColor = Config.Style.PaletteBackground;
		}

		public PositionTagger () : this (null)
		{
		}

		public ProjectLongoMatch Project {
			get;
			set;
		}

		public EventsFilter Filter {
			get {
				return filter;
			}
			set {
				if (filter != null) {
					filter.FilterUpdated -= HandleFilterUpdated;
				}
				filter = value;
				filter.FilterUpdated += HandleFilterUpdated;
			}
		}

		public FieldPositionType FieldPosition {
			get;
			set;
		}

		public bool EmitSignals {
			get;
			set;
		}

		public void SelectPlay (TimelineEventLongoMatch play)
		{
			PositionObject po;
			
			if (play == playSelected) {
				playSelected = null;
				return;
			}
			playSelected = null;
			ClearSelection ();
			var tpo = Objects.FirstOrDefault (o => (o as PositionObject).Play == play);
			if (tpo != null) {
				po = tpo as PositionObject;
				po.Selected = true;
				widget?.ReDraw ();
			}
		}

		public List<Coordinates> Coordinates {
			set {
				ClearObjects ();
				foreach (Coordinates coord in value) {
					AddObject (new PositionObject (coord.Points, Background.Width, Background.Height));
				}
			}
		}

		public IList<Point> Points {
			set {
				ClearObjects ();
				AddObject (new PositionObject (value, Background.Width, Background.Height));
			}
		}

		public IEnumerable<TimelineEventLongoMatch> Plays {
			set {
				ClearObjects ();
				foreach (TimelineEventLongoMatch p in value) {
					AddPlay (p);
				}
			}
		}

		public void AddPlay (TimelineEventLongoMatch play)
		{
			PositionObject po;
			Coordinates coords;
			
			coords = play.CoordinatesInFieldPosition (FieldPosition);
			if (coords == null)
				return;
			
			po = new PositionObject (coords.Points, Background.Width,
				Background.Height);
			po.Play = play;
			po.Project = Project;
			if (Filter != null) {
				po.Visible = Filter.IsVisible (play);
			}
			AddObject (po);
		}

		public void RemovePlays (List<TimelineEventLongoMatch> plays)
		{
			foreach (ICanvasObject co in 
			         Objects.Where (o => plays.Contains ((o as PositionObject).Play)).ToList()) {
				RemoveObject (co);
			}
		}

		void HandleFilterUpdated ()
		{
			foreach (PositionObject po in Objects) {
				po.Visible = Filter.IsVisible (po.Play);
			}
			widget?.ReDraw ();
		}

		protected override void SelectionChanged (List<Selection> selections)
		{
			if (selections.Count > 0) {
				TimelineEventLongoMatch p = (selections.Last ().Drawable as PositionObject).Play;
				playSelected = p;
				if (EmitSignals) {
					((LMCommon.EventsBroker)Config.EventsBroker).EmitLoadEvent (p);
				}
			}
		}

		protected override void ShowMenu (Point coords)
		{
			if (ShowMenuEvent != null) {
				List<TimelineEventLongoMatch> plays = Selections.Select (p => (p.Drawable as PositionObject).Play).ToList ();
				ShowMenuEvent (plays.Cast<TimelineEvent> ().ToList ());
			}
		}
	}
}