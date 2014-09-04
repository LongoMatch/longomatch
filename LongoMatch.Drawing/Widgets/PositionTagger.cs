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
using System.Linq;
using LongoMatch.Common;
using System.Collections.Generic;
using LongoMatch.Store;
using LongoMatch.Drawing.CanvasObjects;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store.Drawables;
using LongoMatch.Handlers;

namespace LongoMatch.Drawing.Widgets
{
	public class PositionTagger: BackgroundCanvas
	{
	
		public event ShowTaggerMenuHandler ShowMenuEvent;

		TimelineEvent playSelected;

		public PositionTagger (IWidget widget): base (widget)
		{
			Accuracy = Constants.TAGGER_POINT_SIZE + 3;
			EmitSignals = true;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
		}

		public PositionTagger (IWidget widget, List<TimelineEvent> plays, Image background, FieldPositionType position): base (widget)
		{
			Background = background;
			Plays = plays;
			FieldPosition = position;
		}

		public FieldPositionType FieldPosition {
			get;
			set;
		}

		public bool EmitSignals {
			get;
			set;
		}

		public void SelectPlay (TimelineEvent play)
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
				widget.ReDraw ();
			}
		}

		public List<Point> Points {
			set {
				ClearObjects ();
				Objects.Add (new PositionObject (value, Background.Width, Background.Height));
			}
		}

		public List<TimelineEvent> Plays {
			set {
				ClearObjects ();
				foreach (TimelineEvent p in value) {
					AddPlay (p);
				}
			}
		}

		public void AddPlay (TimelineEvent play)
		{
			PositionObject po;
			Coordinates coords;
			
			coords = play.CoordinatesInFieldPosition (FieldPosition);
			if (coords == null)
				return;
			
			po = new PositionObject (coords.Points, Background.Width,
			                         Background.Height);
			po.Play = play;
			Objects.Add (po);
		}

		public void RemovePlays (List<TimelineEvent> plays)
		{
			Objects.RemoveAll (o => plays.Contains ((o as PositionObject).Play));
		}

		protected override void SelectionChanged (List<Selection> selections)
		{
			if (selections.Count > 0) {
				TimelineEvent p = (selections.Last ().Drawable as PositionObject).Play;
				playSelected = p;
				if (EmitSignals) {
					Config.EventsBroker.EmitLoadPlay (p);
				}
			}
		}

		protected override void ShowMenu (Point coords)
		{
			if (ShowMenuEvent != null) {
				List<TimelineEvent> plays = Selections.Select (p => (p.Drawable as PositionObject).Play).ToList ();
				ShowMenuEvent (plays);
			}
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

