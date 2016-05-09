//
//  Copyright (C) 2015 Fluendo S.A.
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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Drawing.CanvasObjects;

namespace LongoMatch.Drawing.CanvasObjects.Dashboard
{
	public class LinkAnchorObject: CanvasObject, ICanvasSelectableObject
	{

		static ISurface OutIcon;
		static ISurface OutPrelightIcon;
		static ISurface InIcon;
		static ISurface InPrelightIcon;
		static bool surfacesCached = false;

		readonly int iconWidth;
		readonly int iconHeight;
		const int radius = 5;
		double width;
		double height;

		public LinkAnchorObject (DashboardButtonObject button, List<Tag> tags, Point relPos)
		{
			RelativePosition = relPos;
			width = height = 0;
			Button = button;
			if (tags == null)
				tags = new List<Tag> ();
			Tags = tags;
			LoadSurfaces ();
			iconHeight = InIcon.Height;
			iconWidth = InIcon.Width;
		}

		public DashboardButtonObject Button {
			get;
			set;
		}

		public Point RelativePosition {
			get;
			set;
		}

		public double Width {
			get {
				if (width == 0)
					return Button.Width;
				return width;
			}
			set {
				width = value;
			}
		}

		public double Height {
			get {
				if (height == 0)
					return Button.Height;
				return height;
			}
			set {
				height = value;
			}
		}

		public List<Tag> Tags {
			get;
			set;
		}

		public Point Position {
			get {
				return Button.Position + RelativePosition;
			}
		}

		public Point Out {
			get {
				Rectangle rect = SelectionArea;
				return new Point (rect.TopLeft.X + iconWidth + 2 + iconWidth / 2,
					rect.TopLeft.Y + iconHeight / 2);
			}
		}

		public Point In {
			get {
				Rectangle rect = SelectionArea;
				return new Point (rect.TopLeft.X + iconWidth / 2,
					rect.TopLeft.Y + iconHeight / 2);
			}
		}

		public Rectangle SelectionArea {
			get {
				return new Rectangle (
					new Point (Position.X + Width - (iconWidth * 2 + 2), Position.Y),
					(iconWidth * 2) + 2, iconHeight);
			}
		}

		public bool CanLink (LinkAnchorObject anchor)
		{
			if (anchor == null)
				return false;
			else if (this == anchor)
				return false;
			else if (Button == anchor.Button)
				return false;
			else if (Button is TimerObject && anchor.Button is TimerObject)
				return true;
			else if (Button is TagObject && anchor.Button is TagObject)
				return true;
			else if (Button.Button is EventButton && anchor.Button.Button is EventButton)
				return true;
			return false;
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection sel;

			sel = SelectionArea.GetSelection (point, precision, inMotion);
			if (sel != null) {
				sel.Drawable = this;
				sel.Position = SelectionPosition.All;
			}
			return sel;
		}

		public void Move (Selection s, Point dst, Point start)
		{
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			ISurface linkIn, linkOut;

			if (Highlighted) {
				linkIn = InPrelightIcon;
				linkOut = OutPrelightIcon;
			} else {
				linkIn = InIcon;
				linkOut = OutIcon;
			}
			Point inPoint = new Point (In.X - iconWidth / 2, In.Y - iconHeight / 2);
			Point outPoint = new Point (Out.X - iconWidth / 2, In.Y - iconHeight / 2);

			tk.Begin ();
			tk.DrawSurface (inPoint, StyleConf.LinkInWidth, StyleConf.LinkInHeight, linkIn, ScaleMode.AspectFit);
			tk.DrawSurface (outPoint, StyleConf.LinkOutWidth, StyleConf.LinkOutHeight, linkOut, ScaleMode.AspectFit);
			tk.End ();
		}

		void LoadSurfaces ()
		{
			if (!surfacesCached) {
				InIcon = CreateSurface (StyleConf.LinkIn);
				InPrelightIcon = CreateSurface (StyleConf.LinkInPrelight);
				OutIcon = CreateSurface (StyleConf.LinkOut);
				OutPrelightIcon = CreateSurface (StyleConf.LinkOutPrelight);
				surfacesCached = true;
			}
		}

		ISurface CreateSurface (string name)
		{
			Image img = Resources.LoadImage (name);
			return Config.DrawingToolkit.CreateSurface (img.Width, img.Height, img, false);
		}

	}
}
