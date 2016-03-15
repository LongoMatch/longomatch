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
using System.Collections.Generic;
using System.IO;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObjects.Dashboard
{
	public class CategoryObject: TimedTaggerObject
	{
		public event ButtonSelectedHandler EditButtonTagsEvent;

		static Image iconImage;
		static Image recImage;
		static Image editImage;
		static Image cancelImage;
		static Image applyImage;
		Dictionary <Rectangle, object> rects, buttonsRects;
		Dictionary <string, List<Tag>> tagsByGroup;
		bool emitEvent, delayEvent, editClicked;
		bool cancelClicked, applyClicked, moved;
		int nrows;
		const int TIMEOUT_MS = 800;
		System.Threading.Timer timer;
		object cancelButton = new object ();
		object editbutton = new object ();
		object applyButton = new object ();
		Rectangle editRect, cancelRect, applyRect;
		double catWidth, heightPerRow;
		Dictionary <Tag, LinkAnchorObject> subcatAnchors, cachedAnchors;

		public CategoryObject (AnalysisEventButton category) : base (category)
		{
			Button = category;
			rects = new Dictionary <Rectangle, object> ();
			buttonsRects = new Dictionary <Rectangle, object> ();
			SelectedTags = new List<Tag> ();
			cancelRect = new Rectangle (new Point (0, 0), 0, 0);
			editRect = new Rectangle (new Point (0, 0), 0, 0);
			applyRect = new Rectangle (new Point (0, 0), 0, 0);
			if (iconImage == null) {
				iconImage = Resources.LoadImage (StyleConf.ButtonEventIcon);
			}
			if (recImage == null) {
				recImage = Resources.LoadImage (StyleConf.RecordButton);
			}
			if (editImage == null) {
				editImage = Resources.LoadImage (StyleConf.EditButton);
			}
			if (cancelImage == null) {
				cancelImage = Resources.LoadImage (StyleConf.CancelButton);
			}
			if (applyImage == null) {
				applyImage = Resources.LoadImage (StyleConf.ApplyButton);
			}
			MinWidth = 100;
			MinHeight = HeaderHeight * 2;
			subcatAnchors = new Dictionary<Tag, LinkAnchorObject> ();
			foreach (Tag tag in category.AnalysisEventType.Tags) {
				AddSubcatAnchor (tag, new Point (0, 0), 100, HeaderHeight);
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (timer != null) {
				timer.Dispose ();
				timer = null;
			}
			foreach (LinkAnchorObject anchor in subcatAnchors.Values.ToList()) {
				RemoveAnchor (anchor);
			}
			base.Dispose (disposing);
		}

		public AnalysisEventButton Button {
			get;
			set;
		}

		public override Image Icon {
			get {
				return iconImage;
			}
		}

		public override Color BackgroundColor {
			get {
				return Button.BackgroundColor;
			}
		}

		bool ShowApplyButton {
			get {
				return ShowTags && tagsByGroup.Count > 1
				&& Button.TagMode == TagMode.Predefined
				&& Mode != DashboardMode.Edit;
			}
		}

		bool ShowTags {
			get {
				return (Button.ShowSubcategories || ShowLinks) && Button.AnalysisEventType.Tags.Count != 0;
			}
		}

		int HeaderHeight {
			get {
				return StyleConf.ButtonHeaderHeight + 5;
			}
		}

		int HeaderTextOffset {
			get {
				return StyleConf.ButtonHeaderWidth + 5;
			}
		}

		double HeaderTextWidth {
			get {
				if (Button.TagMode == TagMode.Free) {
					return Width - HeaderTextOffset - StyleConf.ButtonRecWidth; 
				} else {
					return Width - HeaderTextOffset;
				}
			}
		}

		void UpdateRows ()
		{
			/* Header */
			int tagsPerRow = Math.Max (1, Button.TagsPerRow);
			nrows = 0;

			foreach (List<Tag> tags in tagsByGroup.Values) {
				nrows += (int)Math.Ceiling ((float)tags.Count / tagsPerRow);
			}
		}

		public void ClickTag (Tag tag)
		{
			SelectedTags.Add (tag);
			ReDraw ();
		}

		override protected void Clear ()
		{
			base.Clear ();
			emitEvent = false;
			cancelClicked = false;
			SelectedTags.Clear ();
		}

		void RemoveAnchor (LinkAnchorObject anchor)
		{
			anchor.Dispose ();
			subcatAnchors.RemoveKeysByValue (anchor);
		}

		void EmitCreateEvent ()
		{
			EmitClickEvent ();
			Clear ();
		}

		void TimerCallback (Object state)
		{
			Config.DrawingToolkit.Invoke (delegate {
				EmitCreateEvent ();
			});
		}

		void DelayTagClicked ()
		{
			if (tagsByGroup.Keys.Count == 1 || Mode == DashboardMode.Edit) {
				TimerCallback (null);
				return;
			}
			//if (timer == null) {
			//	timer = new System.Threading.Timer (TimerCallback, null, TIMEOUT_MS, 0);
			//} else {
			//	timer.Change (TIMEOUT_MS, 0);
			//} 
		}

		void CategoryClicked (AnalysisEventButton category)
		{
			if (Button.TagMode == TagMode.Predefined) {
				emitEvent = true;
				Active = true;
			} else if (Button.TagMode == TagMode.Free) {
				if (!Recording) {
					StartRecording ();
				} else {
					emitEvent = true;
				}
			}
		}

		void TagClicked (Tag tag)
		{
			if (SelectedTags.Contains (tag)) {
				SelectedTags.Remove (tag);
			} else {
				SelectedTags.RemoveAll (t => t.Group == tag.Group);
				SelectedTags.Add (tag);
			}
			if (Button.TagMode == TagMode.Free) {
				StartRecording ();
			} else {
				Active = true;
				delayEvent = true;
			}
			ReDraw ();
		}

		void UpdateGroups ()
		{
			tagsByGroup = Button.AnalysisEventType.TagsByGroup;
		}

		public List<Tag> SelectedTags {
			get;
			set;
		}

		void AddSubcatAnchor (Tag tag, Point point, double width, double height)
		{
			LinkAnchorObject anchor;

			if (subcatAnchors.ContainsKey (tag)) {
				anchor = subcatAnchors [tag];
				anchor.RelativePosition = point;
			} else {
				anchor = new LinkAnchorObject (this, new List<Tag> { tag }, point);
				anchor.RedrawEvent += (co, area) => {
					EmitRedrawEvent (anchor, area);
				};
				subcatAnchors.Add (tag, anchor);
			}
			anchor.Width = width;
			anchor.Height = height;
		}

		bool CheckRect (Point p, Rectangle rect, object obj)
		{
			Selection subsel;
		
			if (obj == null) {
				return false;
			}
			subsel = rect.GetSelection (p, 0);
			if (subsel != null) {
				if (obj is AnalysisEventButton) {
					CategoryClicked (Button);
				} else if (obj is Tag) {
					TagClicked (obj as Tag);
				} else if (obj == cancelButton) {
					cancelClicked = true;
				} else if (obj == editbutton) {
					editClicked = true;
				} else if (obj == applyButton) {
					applyClicked = true;
				}
				return true;
			}
			return false;
		}

		public override Selection GetSelection (Point p, double precision, bool inMotion = false)
		{
			if (ShowLinks) {
				Selection sel = anchor.GetSelection (p, precision, inMotion);
				if (sel != null)
					return sel;
				foreach (LinkAnchorObject subcatAnchor in subcatAnchors.Values) {
					sel = subcatAnchor.GetSelection (p, precision, inMotion);
					if (sel != null)
						return sel;
				}
			}
			return base.GetSelection (p, precision, inMotion);
		}

		public override LinkAnchorObject GetAnchor (IList<Tag> sourceTags)
		{
			/* Only one tag is supported for now */
			if (sourceTags == null || sourceTags.Count == 0) {
				return base.GetAnchor (sourceTags);
			} else {
				return subcatAnchors [sourceTags [0]];
			}
		}

		public override void ClickPressed (Point p, ButtonModifier modif)
		{
			if (Mode == DashboardMode.Edit) {
				editClicked = CheckRect (p, editRect, editbutton);
				return;
			}
			foreach (Rectangle rect in buttonsRects.Keys) {
				if (CheckRect (p, rect, buttonsRects [rect])) {
					return;
				}
			}
			foreach (Rectangle rect in rects.Keys) {
				if (CheckRect (p, rect, rects [rect])) {
					return;
				}
			}
		}

		public override void ClickReleased ()
		{
			if (editClicked && !moved && EditButtonTagsEvent != null) {
				EditButtonTagsEvent (Button);
			} else if (cancelClicked) {
				Clear ();
			} else if (emitEvent) {
				EmitCreateEvent ();
			} else if (delayEvent) {
				if (SelectedTags.Count == tagsByGroup.Count) {
					EmitCreateEvent ();
				} else {
					DelayTagClicked ();
				}
			} else if (applyClicked) {
				if (SelectedTags.Count > 1) {
					EmitCreateEvent ();
				}
			}
			emitEvent = delayEvent = moved = editClicked = applyClicked = false;
		}

		void DrawTagsGroup (IDrawingToolkit tk, List<Tag> tags, ref double yptr)
		{
			double rowwidth;
			Point start;
			int tagsPerRow, row = 0;

			tagsPerRow = Math.Max (1, Button.TagsPerRow);
			rowwidth = catWidth / tagsPerRow;

			start = new Point (Position.X, Position.Y + HeaderHeight);
			tk.FontSize = 12;
			tk.FontWeight = FontWeight.Light;

			/* Draw tags */
			for (int i = 0; i < tags.Count; i++) {
				Point pos;
				int col;
				Tag tag;

				row = i / tagsPerRow;
				col = i % tagsPerRow;
				pos = new Point (start.X + col * rowwidth,
					start.Y + yptr + row * heightPerRow);
				tag = tags [i];

				AddSubcatAnchor (tag, new Point (pos.X - Position.X, pos.Y - Position.Y),
					rowwidth, heightPerRow);
				if (!ShowTags) {
					continue;
				}

				tk.StrokeColor = Button.DarkColor;
				tk.LineWidth = 1;
				/* Draw last vertical line when the last row is not fully filled*/
				if (col == 0) {
					if (i + tagsPerRow > tags.Count) {
						tk.LineStyle = LineStyle.Dashed;
						var st = new Point (pos.X + rowwidth * ((i + 1) % tagsPerRow), pos.Y);
						tk.DrawLine (st, new Point (st.X, st.Y + heightPerRow)); 
						tk.LineStyle = LineStyle.Normal;
					}
				}

				if (col == 0) {
					if (row != 0) {
						tk.LineStyle = LineStyle.Dashed;
					}
					/* Horizontal line */
					tk.DrawLine (pos, new Point (pos.X + catWidth, pos.Y));
					tk.LineStyle = LineStyle.Normal;
				} else {
					/* Vertical line */
					tk.LineStyle = LineStyle.Dashed;
					tk.DrawLine (pos, new Point (pos.X, pos.Y + heightPerRow));
					tk.LineStyle = LineStyle.Normal;
				}
				tk.StrokeColor = Button.TextColor;
				tk.DrawText (pos, rowwidth, heightPerRow, tag.Value);
				rects.Add (new Rectangle (pos, rowwidth, heightPerRow), tag);
			}
			yptr += heightPerRow * (row + 1);
		}

		void DrawHeader (IDrawingToolkit tk)
		{
			Color textColor;
			Point pos;
			double width, height;
			int fontSize;
			bool ellipsize = true;

			if (Active) {
				textColor = BackgroundColor;
			} else {
				textColor = TextColor;
			}

			width = HeaderTextWidth;
			height = HeaderHeight;
			pos = new Point (Position.X + HeaderTextOffset, Position.Y);
			fontSize = StyleConf.ButtonHeaderFontSize;

			if (ShowTags) {
				rects.Add (new Rectangle (Position, Width, HeaderHeight), Button);
				if (Recording) {
					/* Draw Timer instead */
					return;
				}
			} else {
				if (!Recording) {
					width = Width;
					height = Height - HeaderHeight;
					pos = new Point (Position.X, Position.Y + HeaderHeight);
					fontSize = StyleConf.ButtonNameFontSize;
					ellipsize = false;
				}
				rects.Add (new Rectangle (Position, Width, Height), Button);
			}
			tk.FontSize = fontSize;
			tk.StrokeColor = BackgroundColor;
			tk.StrokeColor = textColor;
			tk.FontWeight = FontWeight.Light;
			tk.DrawText (pos, width, height, Button.AnalysisEventType.Name, false, ellipsize);
		}

		void DrawEditButton (IDrawingToolkit tk)
		{
			Point pos;
			Color c;
			double width, height;

			if (Mode != DashboardMode.Edit || ShowLinks || !Button.ShowSubcategories) {
				return;
			}

			c = Config.Style.PaletteBackgroundDark;
			width = StyleConf.ButtonRecWidth;
			height = HeaderHeight;
			pos = new Point (Position.X + Width - StyleConf.ButtonRecWidth,
				Position.Y + Height - height);
			tk.LineWidth = 0;
			tk.FillColor = new Color (c.R, c.G, c.B, 200);
			tk.StrokeColor = BackgroundColor;
			tk.DrawRectangle (pos, width, height);
			tk.StrokeColor = Color.Green1;
			tk.FillColor = Color.Green1;
			tk.FontSize = StyleConf.ButtonButtonsFontSize;
			editRect.Update (pos, width, height);
			buttonsRects [editRect] = editbutton;
			pos = new Point (pos.X, pos.Y + 5);
			tk.DrawImage (pos, width, height - 10, editImage, ScaleMode.AspectFit, true);
		}

		void DrawSelectedTags (IDrawingToolkit tk)
		{
			if (Mode == DashboardMode.Edit) {
				return;
			}
			foreach (Rectangle r in rects.Keys) {
				object obj = rects [r];
				if (obj is Tag && SelectedTags.Contains (obj as Tag)) {
					tk.LineWidth = 0;
					tk.FontWeight = FontWeight.Light;
					tk.FillColor = TextColor;
					tk.FontSize = 12;
					tk.DrawRectangle (new Point (r.TopLeft.X, r.TopLeft.Y), r.Width, r.Height);
					tk.StrokeColor = BackgroundColor;
					tk.DrawText (new Point (r.TopLeft.X, r.TopLeft.Y), r.Width, r.Height,
						(obj as Tag).Value);
				}
			}
		}

		void DrawRecordTime (IDrawingToolkit tk)
		{
			if (Recording && Mode != DashboardMode.Edit) {
				if (ShowTags) {
					tk.FontSize = 12;
					tk.FontWeight = FontWeight.Normal;
					tk.StrokeColor = BackgroundColor;
					tk.DrawText (new Point (Position.X + HeaderTextOffset, Position.Y),
						HeaderTextWidth, HeaderHeight,
						(CurrentTime - Start).ToSecondsString ());
				} else {
					tk.FontSize = 24;
					tk.FontWeight = FontWeight.Bold;
					tk.StrokeColor = BackgroundColor;
					tk.DrawText (new Point (Position.X, Position.Y + HeaderHeight),
						Width, Height - HeaderHeight,
						(CurrentTime - Start).ToSecondsString ());
				}
			}
		}

		void DrawApplyButton (IDrawingToolkit tk)
		{
			Point pos;
			double width, height;

			if (!ShowApplyButton || SelectedTags.Count == 0) {
				rects [applyRect] = null;
				return;
			}
			
			pos = new Point (Position.X + Width - StyleConf.ButtonRecWidth,
				Position.Y);
			width = StyleConf.ButtonRecWidth;
			height = HeaderHeight;
			tk.FillColor = Config.Style.PaletteBackgroundDark;
			tk.LineWidth = 0;
			tk.DrawRectangle (pos, width, height);
			tk.StrokeColor = Color.Green1;
			tk.FillColor = Color.Green1;
			tk.FontSize = 12;
			applyRect.Update (pos, width, height);
			buttonsRects [applyRect] = applyButton; 
			pos = new Point (pos.X, pos.Y + 5);
			tk.DrawImage (pos, width, height - 10, applyImage, ScaleMode.AspectFit, true);
		}

		void DrawRecordButton (IDrawingToolkit tk)
		{
			Point pos, bpos;
			double width, height;

			if (Button.TagMode != TagMode.Free || ShowLinks) {
				return;
			}
			
			pos = new Point (Position.X + Width - StyleConf.ButtonRecWidth,
				Position.Y);
			bpos = new Point (pos.X, pos.Y + 5);
			
			width = StyleConf.ButtonRecWidth;
			height = HeaderHeight;
			tk.FontSize = StyleConf.ButtonButtonsFontSize;
			if (!Recording) {
				tk.FillColor = Config.Style.PaletteBackgroundDark;
				tk.StrokeColor = BackgroundColor;
				tk.LineWidth = StyleConf.ButtonLineWidth;
				tk.DrawRectangle (pos, width, height);
				tk.StrokeColor = Color.Red1;
				tk.FillColor = Color.Red1;
				tk.DrawImage (bpos, width, height - 10, recImage, ScaleMode.AspectFit, true);
			} else {
				tk.FillColor = tk.StrokeColor = BackgroundColor;
				tk.DrawRectangle (pos, width, height);
				tk.StrokeColor = TextColor;
				tk.FillColor = TextColor;
				tk.DrawImage (bpos, width, height - 10, cancelImage, ScaleMode.AspectFit, true);
				cancelRect.Update (pos, width, height);
				buttonsRects [cancelRect] = cancelButton;
			}
		}

		void DrawAnchors (IDrawingToolkit tk)
		{
			if (!ShowLinks)
				return;

			anchor.Height = HeaderHeight;
			DrawAnchor (tk, null);
			foreach (LinkAnchorObject a in subcatAnchors.Values) {
				a.Draw (tk, null);
			}
		}

		new void DrawButton (IDrawingToolkit tk)
		{
			if (!ShowTags) {
				base.DrawButton (tk);
			} else {
				tk.FillColor = BackgroundColor;
				tk.StrokeColor = TextColor;
				tk.LineWidth = 0;
				tk.DrawRectangle (Position, Width, Height);
				if (Active) {
					tk.FillColor = TextColor;
					tk.DrawRectangle (Position, Width, HeaderHeight);
				}
				if (Icon != null) {
					if (Active) {
						tk.FillColor = BackgroundColor;
					} else {
						tk.FillColor = TextColor;
					}
					tk.DrawImage (new Point (Position.X + 5, Position.Y + 5),
						StyleConf.ButtonHeaderWidth, StyleConf.ButtonHeaderHeight, Icon, ScaleMode.AspectFit, true);
				}
			}
		}

		void DrawBackbuffer (IDrawingToolkit tk)
		{
			Point pos;
			double yptr = 0;

			rects.Clear ();
			buttonsRects.Clear ();
			UpdateGroups ();
			UpdateRows ();
			heightPerRow = (Height - HeaderHeight) / nrows;
			catWidth = Width;
			pos = Position;

			tk.Begin ();
			tk.TranslateAndScale (new Point (-Position.X, -Position.Y),
				new Point (1, 1));
			tk.FontWeight = FontWeight.Bold;

			/* Draw Rectangle */
			DrawButton (tk);
			DrawImage (tk);
			DrawHeader (tk);
			DrawRecordButton (tk);

			foreach (List<Tag> tags in tagsByGroup.Values) {
				DrawTagsGroup (tk, tags, ref yptr);
			}

			/* Remove anchor object that where not reused
				 * eg: after removinga a subcategory tag */
			foreach (Tag tag in subcatAnchors.Keys.ToList ()) {
				if (!Button.AnalysisEventType.Tags.Contains (tag)) {
					RemoveAnchor (subcatAnchors [tag]);
				}
			}
			if (!ShowLinks) {
				DrawEditButton (tk);
			}

			tk.End ();
		}

		void CreateBackBufferSurface ()
		{
			IDrawingToolkit tk = Config.DrawingToolkit;

			ResetBackbuffer ();

			backBufferSurface = tk.CreateSurface ((int)Width, (int)Height);
			using (IContext c = backBufferSurface.Context) {
				tk.Context = c;
				DrawBackbuffer (tk);
			}
		}

		public override void Move (Selection s, Point p, Point start)
		{
			base.Move (s, p, start);
			moved = true;
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
			DrawSelectedTags (tk);
			DrawRecordTime (tk);
			DrawApplyButton (tk);
			DrawSelectionArea (tk);
			DrawAnchors (tk);
			tk.End ();
		}
	}
}

