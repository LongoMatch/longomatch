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
using LongoMatch.Drawing.CanvasObject;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store.Drawables;
using LongoMatch.Handlers;

namespace LongoMatch.Drawing.Widgets
{
	public class PositionTagger: BackgroundCanvas
	{
	
		public event ShowTaggerMenuHandler ShowMenuEvent;
		List<Play> plays;

		public PositionTagger (IWidget widget): base (widget)
		{
			Accuracy = Common.TAGGER_POINT_SIZE + 3;
		}
		
		public PositionTagger (IWidget widget, List<Play> plays, Image background, FieldPositionType position): base (widget)
		{
			Background = background;
			Plays = plays;
			FieldPosition = position;
		}
		
		FieldPositionType FieldPosition {
			get;
			set;
		}
		
		public void SelectPlay (Play play) {
		}
		
		public List<Point> Points {
			set {
				Objects.Clear ();
				Objects.Add (new PositionObject (value, Background.Width, Background.Height));
			}
		}
		
		public List<Play> Plays {
			set {
				plays = value;
				foreach (Play p in value) {
					Coordinates coords = p.CoordinatesInFieldPosition (FieldPosition);
					
					Objects.Clear ();
					if (coords == null)
						continue;
					
					PositionObject po = new PositionObject (coords.Points,
					                                        Background.Width, Background.Height);
					po.Play = p;
					Objects.Add (po);
				}
			}
		}

		protected override void SelectionChanged (List<Selection> selections) {
			if (selections.Count > 0) {
				Play p = (selections.Last().Drawable as PositionObject).Play;
				Config.EventsBroker.EmitPlaySelected (p);
			}
		}
		
		protected override void ShowMenu (Point coords) {
			if (ShowMenuEvent != null) {
				List<Play> plays = Selections.Select (p => (p.Drawable as PositionObject).Play).ToList();
				ShowMenuEvent (plays);
			}
		}
	}
}

