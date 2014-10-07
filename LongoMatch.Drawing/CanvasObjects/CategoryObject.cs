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
using Mono.Unix;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class CategoryObject: TaggerObject
	{

		Dictionary <Rectangle, object> rects;
		Dictionary <string, List<Tag>> tagsByGroup;
		bool catSelected, tagSelected;
		int nrows;
		const int TIMEOUT_MS = 800;
		System.Threading.Timer timer;

		public CategoryObject (AnalysisEventButton category): base (category)
		{
			Button = category;
			rects = new Dictionary <Rectangle, object> ();
			SelectedTags = new List<Tag> ();
		}

		protected override void Dispose (bool disposing)
		{
			if (timer != null) {
				timer.Dispose ();
				timer = null;
			}
			base.Dispose (disposing);
		}

		public AnalysisEventButton Button {
			get;
			set;
		}

		public Tag AddTag {
			get;
			set;
		}

		public override Color BackgroundColor {
			get {
				return Tagger.BackgroundColor;
			}
		}

		void UpdateRows ()
		{
			/* Header */
			int tagsPerRow = Math.Max (1, Button.TagsPerRow);
			nrows = 1;

			/* Recorder */
			if (Button.TagMode == TagMode.Free) {
				nrows ++;
			}
			foreach (List<Tag> tags in tagsByGroup.Values) {
				nrows += (int)Math.Ceiling ((float)tags.Count / tagsPerRow);
			}
			if (Mode == TagMode.Edit) {
				nrows ++;
			}
		}

		void TimerCallback (Object state)
		{
			Config.DrawingToolkit.Invoke (delegate {
				EmitClickEvent ();
				tagSelected = false;
				catSelected = false;
				SelectedTags.Clear ();
			});
		}

		void DelayTagClicked ()
		{
			if (tagsByGroup.Keys.Count == 1) {
				TimerCallback (null);
				return;
			}
			if (timer == null) {
				timer = new System.Threading.Timer (TimerCallback, null, TIMEOUT_MS, 0);
			} else {
				timer.Change (TIMEOUT_MS, 0);
			} 
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
				SelectedTags.Add (tag);
				if (Button.TagMode == TagMode.Predefined) {
					catSelected = true;
					tagSelected = true;
				}
			}
		}

		void RecordClicked ()
		{
		}

		void UpdateGroups ()
		{
			tagsByGroup = Button.AnalysisEventType.TagsByGroup;
			
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
						CategoryClicked (Button);
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
			if (catSelected && !tagSelected) {
				EmitClickEvent ();
				SelectedTags.Clear ();
				catSelected = false;
			} else if (tagSelected) {
				DelayTagClicked ();
			}
		}

		void DrawTagsGroup (IDrawingToolkit tk, double catWidth, double heightPerRow, List<Tag> tags, ref double yptr)
		{
			double rowwidth;
			int tagsPerRow, row = 0;

			tagsPerRow = Math.Max (1, Button.TagsPerRow);
			rowwidth = catWidth / tagsPerRow;

			/* Draw tags */
			for (int i=0; i < tags.Count; i++) {
				Point pos;
				int col;
				Tag tag;

				row = i / tagsPerRow;
				col = i % tagsPerRow;
				pos = new Point (Button.Position.X + col * rowwidth,
				                 Button.Position.Y + yptr + row * heightPerRow);

				if (col == 0) {
					if (i + tagsPerRow > tags.Count) {
						rowwidth = catWidth / (tags.Count - i);
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
				tag = tags [i];
				if (Mode == TagMode.Edit || !SelectedTags.Contains (tag)) {
					tk.DrawText (pos, rowwidth, heightPerRow, tag.Value);
				} else {
					tk.StrokeColor = Button.DarkColor;
					tk.DrawText (pos, rowwidth, heightPerRow, tag.Value);
				}
				rects.Add (new Rectangle (pos, rowwidth, heightPerRow), tag);
			}
			yptr += heightPerRow * (row + 1);
		}

		void DrawEditButton (IDrawingToolkit tk, double catWidth, double heightPerRow, ref double yptr)
		{
			Point start;
			Tag tag = AddTag;

			if (Mode != TagMode.Edit) {
				return;
			}
			tk.StrokeColor = Button.DarkColor;
			tk.LineWidth = 1;
			start = new Point (Button.Position.X, Button.Position.Y + yptr);
			tk.DrawLine (start, new Point (start.X + catWidth, start.Y));
			tk.StrokeColor = Button.TextColor;
			tk.DrawText (start, catWidth, heightPerRow, Catalog.GetString ("Edit"));
			rects.Add (new Rectangle (start, catWidth, heightPerRow), tag);
			yptr += heightPerRow;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Point pos;
			double catWidth, heightPerRow, yptr = 0;

			rects.Clear ();
			UpdateGroups ();
			UpdateRows ();
			heightPerRow = Button.Height / nrows;
			catWidth = Button.Width;
			pos = Button.Position;

			if (!UpdateDrawArea (tk, area, new Area (Position, Width, Height))) {
				return;
			}

			tk.Begin ();
			tk.FontWeight = FontWeight.Bold;

			/* Draw Rectangle */
			DrawButton (tk);
			DrawImage (tk);

			/* Draw header */
			tk.FillColor = LongoMatch.Core.Common.Color.Grey2;
			tk.LineWidth = 2;
			if (catSelected) {
				tk.StrokeColor = Button.DarkColor;
			} else {
				tk.StrokeColor = Button.TextColor;
			}
			tk.DrawText (pos, catWidth, heightPerRow, Button.EventType.Name);
			rects.Add (new Rectangle (pos, catWidth, heightPerRow), Button);
			yptr += heightPerRow;

			foreach (List<Tag> tags in tagsByGroup.Values) {
				DrawTagsGroup (tk, catWidth, heightPerRow, tags, ref yptr);
			}

			DrawEditButton (tk, catWidth, heightPerRow, ref yptr);

			if (Button.TagMode == TagMode.Free) {
				/* Draw Tagger */
				tk.DrawLine (new Point (pos.X, pos.Y + yptr),
				             new Point (pos.X + catWidth, pos.Y + yptr));
				tk.DrawText (new Point (pos.X, pos.Y + yptr), catWidth, heightPerRow, "Record");
			}
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

