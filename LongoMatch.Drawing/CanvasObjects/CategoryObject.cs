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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class CategoryObject: TaggerObject
	{

		Dictionary <Rectangle, object> rects;
		bool catSelected;

		public CategoryObject (AnalysisEventButton category): base (category)
		{
			Button = category;
			rects = new Dictionary <Rectangle, object> ();
			SelectedTags = new List<Tag> ();
		}

		public AnalysisEventButton Button {
			get;
			set;
		}

		public Tag AddTag {
			get;
			set;
		}

		public override int NRows {
			get {
				/* Header */
				int rows = 1;
				int tagsPerRow = Math.Max (1, Button.TagsPerRow);

				/* Recorder */
				if (Button.TagMode == TagMode.Free) {
					rows ++;
				}
				rows += (int)Math.Ceiling ((float)TagsCount / tagsPerRow);
				return rows;
			}
		}

		int TagsCount {
			get {
				int tagsCount = Button.AnalysisEventType.Tags.Count;
				if (Mode == TagMode.Edit) {
					tagsCount ++;
				}
				return tagsCount;
			}
		}

		public List<Tag> SelectedTags {
			get;
			set;
		}

		public Tag GetTagForCoords (Point p)
		{
			Tag tag = null;

			foreach (Rectangle rect in rects.Keys) {
				Selection subsel = rect.GetSelection (p, 0);
				if (subsel != null) {
					if (rects [rect] is Tag) {
						tag = rects [rect] as Tag;
					}
					break;
				}
			}
			if (tag != AddTag) {
				return tag;
			} else {
				return null;
			}
		}

		public override void ClickPressed (Point p, ButtonModifier modif)
		{
			foreach (Rectangle rect in rects.Keys) {
				Selection subsel = rect.GetSelection (p, 0);
				if (subsel != null) {
					if (rects [rect] is AnalysisEventButton) {
						CategoryClicked (rects [rect] as AnalysisEventButton);
					} else if (rects [rect] is Tag) {
						TagClicked (rects [rect] as Tag);
					} else {
						RecordClicked ();
					}
					break;
				}
			}
		}

		public override void ClickReleased ()
		{
			if (catSelected) {
				EmitClickEvent ();
				SelectedTags.Clear ();
				catSelected = false;
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Point position;
			double heightPerRow, catWidth, rowwidth, yptr = 0;
			int tagsPerRow, tagsCount, row = 0;

			rects.Clear ();
			position = Button.Position;
			heightPerRow = Button.Height / NRows;
			catWidth = Button.Width;
			tagsCount = TagsCount;
			tagsPerRow = Math.Max (1, Button.TagsPerRow);
			rowwidth = catWidth / tagsPerRow;

			tk.Begin ();
			tk.FontWeight = FontWeight.Bold;

			/* Draw Rectangle */
			DrawButton (tk, true);

			/* Draw header */
			tk.FillColor = LongoMatch.Core.Common.Color.Grey2;
			tk.LineWidth = 2;
			if (catSelected && Mode != TagMode.Edit) {
				tk.StrokeColor = Button.DarkColor;
				tk.DrawText (position, catWidth, heightPerRow, Button.EventType.Name);
			} else {
				tk.StrokeColor = LongoMatch.Core.Common.Color.Grey2;
				tk.DrawText (position, catWidth, heightPerRow, Button.EventType.Name);
			}
			rects.Add (new Rectangle (position, catWidth, heightPerRow), Button);
			yptr += heightPerRow;

			/* Draw tags */
			for (int i=0; i < tagsCount; i++) {
				Point pos;
				int col;
				Tag tag;

				row = i / tagsPerRow;
				col = i % tagsPerRow;
				pos = new Point (position.X + col * rowwidth,
				                             position.Y + yptr + row * heightPerRow);

				if (col == 0) {
					if (i + tagsPerRow > tagsCount) {
						rowwidth = catWidth / (tagsCount - i);
					}
				}
				tk.StrokeColor = Button.DarkColor;
				tk.LineWidth = 1;
				if (col == 0) {
					/* Horizontal line */
					tk.DrawLine (pos, new Point (pos.X + catWidth, pos.Y));
				} else {
					/* Vertical line */
					tk.DrawLine (pos, new Point (pos.X, pos.Y + heightPerRow));
				}
				tk.StrokeColor = Button.TextColor;
				if (i < Button.AnalysisEventType.Tags.Count) {
					tag = Button.AnalysisEventType.Tags [i];
					if (Mode == TagMode.Edit || !SelectedTags.Contains (tag)) {
						tk.DrawText (pos, rowwidth, heightPerRow, tag.Value);
					} else {
						tk.StrokeColor = Button.DarkColor;
						tk.DrawText (pos, rowwidth, heightPerRow, tag.Value);
					}
				} else {
					tag = AddTag;
					tk.DrawText (pos, rowwidth, heightPerRow, "Add");
				}
				rects.Add (new Rectangle (pos, rowwidth, heightPerRow), tag);
			}
			yptr += heightPerRow * (row + 1);

			if (Button.TagMode == TagMode.Free) {
				/* Draw Tagger */
				tk.DrawLine (new Point (position.X, position.Y + yptr),
				                         new Point (position.X + catWidth, position.Y + yptr));
				tk.DrawText (new Point (position.X, position.Y + yptr), catWidth, heightPerRow, "Record");
			}
			DrawSelectionArea (tk);
			tk.End ();
		}

		void CategoryClicked (AnalysisEventButton category)
		{
			if (Button.TagMode == TagMode.Predefined) {
				catSelected = true;
			}
		}

		void TagClicked (Tag tag)
		{
			if (SelectedTags.Contains (tag)) {
				SelectedTags.Remove (tag);
			} else {
				SelectedTags.Clear ();
				SelectedTags.Add (tag);
				if (Button.TagMode == TagMode.Predefined) {
					catSelected = true;
				}
			}
		}

		void RecordClicked ()
		{
		}
	}
}

