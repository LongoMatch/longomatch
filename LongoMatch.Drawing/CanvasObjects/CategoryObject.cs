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
using System.IO;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class CategoryObject: TaggerObject
	{

		static Image RecordSurface = null;
		static Image CancelSurface = null;
		static Image EditSurface = null;
		static Image ApplySurface = null;
		static bool surfacesCached = false;

		Dictionary <Rectangle, object> rects;
		Dictionary <string, List<Tag>> tagsByGroup;
		bool catSelected, tagSelected;
		int nrows;
		const int TIMEOUT_MS = 800;
		Time currentTime;
		System.Threading.Timer timer;
		object recordButton = new object ();
		object cancelButton = new object ();
		ISurface backBufferSurface;
		Rectangle editRect, recordRect, cancelRect;
		double catWidth, heightPerRow, recordY;

		public CategoryObject (AnalysisEventButton category): base (category)
		{
			Button = category;
			rects = new Dictionary <Rectangle, object> ();
			SelectedTags = new List<Tag> ();
			CurrentTime = new Time (0);
			recordRect = new Rectangle ();
			cancelRect = new Rectangle ();
			editRect = new Rectangle ();
			LoadSurfaces ();
		}

		protected override void Dispose (bool disposing)
		{
			if (timer != null) {
				timer.Dispose ();
				timer = null;
			}
			ResetBackbuffer ();
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

		public Time CurrentTime {
			get {
				return currentTime;
			}
			set {
				currentTime = value;
				if (Start != null && currentTime < Start) {
					CancelClicked ();
				} else {
					ReDrawObject ();
				}
			}
		}

		public override Color BackgroundColor {
			get {
				return Tagger.BackgroundColor;
			}
		}

		public void ReDrawObject ()
		{
			ResetBackbuffer ();
			ReDraw ();
		}

		public override void ResetDrawArea ()
		{
			ResetBackbuffer ();
			base.ResetDrawArea ();
		}

		void ResetBackbuffer ()
		{
			if (backBufferSurface != null) {
				backBufferSurface.Dispose ();
				backBufferSurface = null;
			}
		}

		void LoadSurfaces ()
		{
			if (!surfacesCached) {
				RecordSurface = CreateSurface (StyleConf.RecordButton);
				CancelSurface = CreateSurface (StyleConf.CancelButton);
				EditSurface = CreateSurface (StyleConf.EditButton);
				ApplySurface = CreateSurface (StyleConf.ApplyButton);
				surfacesCached = true;
			}
		}

		Image CreateSurface (string name)
		{
			return new Image (Path.Combine (Config.IconsDir, name));
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
			
			if (Button.ShowSubcategories) {
				foreach (List<Tag> tags in tagsByGroup.Values) {
					nrows += (int)Math.Ceiling ((float)tags.Count / tagsPerRow);
				}
			}
			if (Mode == TagMode.Edit) {
				nrows ++;
			}
		}

		void EmitCreateEvent ()
		{
			EmitClickEvent ();
			tagSelected = false;
			catSelected = false;
			SelectedTags.Clear ();
		}

		void TimerCallback (Object state)
		{
			Config.DrawingToolkit.Invoke (delegate {
				EmitCreateEvent ();
			});
		}

		void DelayTagClicked ()
		{
			if (tagsByGroup.Keys.Count == 1 || Mode == TagMode.Edit) {
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
			} else if (Button.TagMode == TagMode.Free) {
				RecordClicked ();
			}
		}

		void TagClicked (Tag tag)
		{
			if (SelectedTags.Contains (tag)) {
				SelectedTags.Remove (tag);
			} else {
				SelectedTags.Add (tag);
				if (Button.TagMode == TagMode.Predefined || Mode == TagMode.Edit) {
					catSelected = true;
					tagSelected = true;
				}
			}
		}

		void CancelClicked ()
		{
			Start = null;
			ReDrawObject ();
		}

		void RecordClicked ()
		{
			if (Mode == TagMode.Edit) {
				return;
			}
			if (Start == null) {
				Start = CurrentTime;
			} else {
				EmitCreateEvent ();
				Start = null;
			}
			ReDrawObject ();
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
				Selection subsel;
				object obj = rects [rect];
				if (obj == null) {
					continue;
				}
				subsel = rect.GetSelection (p, 0);
				if (subsel != null) {
					if (obj is AnalysisEventButton) {
						CategoryClicked (Button);
					} else if (obj is Tag) {
						TagClicked (obj as Tag);
					} else if (obj == recordButton) {
						RecordClicked ();
					} else if (obj == cancelButton) {
						CancelClicked ();
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

		void DrawTagsGroup (IDrawingToolkit tk, List<Tag> tags, ref double yptr)
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
				tk.DrawText (pos, rowwidth, heightPerRow, tag.Value);
				rects.Add (new Rectangle (pos, rowwidth, heightPerRow), tag);
			}
			yptr += heightPerRow * (row + 1);
		}

		void DrawEditButton (IDrawingToolkit tk, ref double yptr)
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
			tk.DrawImage (start, catWidth / 2, heightPerRow, EditSurface, true);
			tk.DrawText (new Point (start.X + catWidth / 2, start.Y),
			             catWidth / 2, heightPerRow, Catalog.GetString ("Edit"));
			editRect.Update (start, catWidth, heightPerRow);
			rects [editRect] = tag;
			yptr += heightPerRow;
		}

		void DrawSelectedTags (IDrawingToolkit tk)
		{
			if (Mode == TagMode.Edit) {
				return;
			}
			foreach (Rectangle r in rects.Keys) {
				object obj = rects [r];
				if ((obj is Tag && SelectedTags.Contains (obj as Tag)) ||
					obj is AnalysisEventButton && catSelected) {
					double radius = (Math.Min (Math.Min (r.Height, r.Width), 20) / 2) - 2;
					tk.LineWidth = 0;
					tk.FillColor = Config.Style.PaletteActive;
					tk.DrawCircle (new Point (r.TopLeft.X + radius, r.TopLeft.Y + radius), radius);
				}
			}
		}

		void DrawRecordButton (IDrawingToolkit tk)
		{
			Point pos = Button.Position;
			/* Draw timer */
			if (Button.TagMode == TagMode.Free) {
				Point p = new Point (pos.X, pos.Y + recordY);
				/* Draw Tagger */
				tk.StrokeColor = Button.DarkColor;
				tk.LineWidth = 1;
				tk.DrawLine (p, new Point (pos.X + catWidth, pos.Y + recordY));
				tk.StrokeColor = Button.TextColor;
				if (Start == null) {
					recordRect.Update (p, catWidth, heightPerRow);
					rects [recordRect] = recordButton;
					rects [cancelRect] = null;
					tk.DrawImage (p, catWidth, heightPerRow, RecordSurface, true);
				} else {
					recordRect.Update (p, catWidth - 20, heightPerRow);
					rects [recordRect] = recordButton;
					tk.DrawImage (p, 20, heightPerRow, ApplySurface, true);
					p.X += 20;
					tk.DrawText (p, catWidth - 40, heightPerRow, (CurrentTime - Start).ToSecondsString ());
					p = new Point (pos.X + catWidth - 20, p.Y);
					cancelRect.Update (p, 20, heightPerRow);
					rects [cancelRect] = cancelButton;
					tk.StrokeColor = Button.DarkColor;
					tk.LineWidth = 1;
					tk.DrawLine (p, new Point (p.X, p.Y + heightPerRow));
					tk.DrawImage (p, 20, heightPerRow, CancelSurface, true);
				}
			}
		}

		void DrawBackbuffer (IDrawingToolkit tk)
		{
			Point pos;
			double yptr = 0;

			rects.Clear ();
			UpdateGroups ();
			UpdateRows ();
			heightPerRow = Button.Height / nrows;
			catWidth = Button.Width;
			pos = Button.Position;

			tk.Begin ();
			tk.TranslateAndScale (new Point (-Position.X, -Position.Y),
			                      new Point (1, 1));
			tk.FontWeight = FontWeight.Bold;

			/* Draw Rectangle */
			DrawButton (tk);
			DrawImage (tk);

			/* Draw header */
			tk.FillColor = LongoMatch.Core.Common.Color.Grey2;
			tk.LineWidth = 2;
			tk.StrokeColor = Button.TextColor;
			tk.DrawText (pos, catWidth, heightPerRow, Button.EventType.Name);
			rects.Add (new Rectangle (pos, catWidth, heightPerRow), Button);
			yptr += heightPerRow;

			if (Button.ShowSubcategories) {
				foreach (List<Tag> tags in tagsByGroup.Values) {
					DrawTagsGroup (tk, tags, ref yptr);
				}
			}

			DrawEditButton (tk, ref yptr);

			if (Button.TagMode == TagMode.Free) {
				recordY = yptr;
			}

			tk.End ();
		}

		void CreateBackBufferSurface ()
		{
			IDrawingToolkit tk = Config.DrawingToolkit;

			if (backBufferSurface != null) {
				backBufferSurface.Dispose ();
			}

			backBufferSurface = tk.CreateSurface ((int)Width, (int)Height);
			using (IContext c = backBufferSurface.Context) {
				tk.Context = c;
				DrawBackbuffer (tk);
			}
		}

		public override void Move (Selection s, Point p, Point start)
		{
			base.Move (s, p, start);
			SelectedTags.Clear ();
			switch (s.Position) {
			case SelectionPosition.Right:
			case SelectionPosition.Bottom:
			case SelectionPosition.BottomRight:
				CreateBackBufferSurface ();
				break;
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			IContext ctx = tk.Context;
			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}
			if (backBufferSurface == null) {
				CreateBackBufferSurface ();
			}
			tk.Context = ctx;
			tk.Begin ();
			tk.DrawSurface (backBufferSurface, Position);
			DrawRecordButton (tk);
			DrawSelectedTags (tk);
			DrawSelectionArea (tk);
			tk.End ();
		}
	}
}

