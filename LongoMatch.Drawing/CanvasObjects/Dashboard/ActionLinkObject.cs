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
using LongoMatch.Core.Common;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObjects.Dashboard
{
	/// <summary>
	/// Represents an <see cref="ActionLink"/> in the canvas.
	/// </summary>
	public class ActionLinkObject: CanvasObject, ICanvasSelectableObject
	{
		readonly Line line;
		const int selectionSize = 4;
		Point stop;

		public ActionLinkObject (LinkAnchorObject source,
		                         LinkAnchorObject destination,
		                         ActionLink link)
		{
			Link = link;
			Source = source;
			Destination = destination;
			if (destination == null) {
				stop = source.Out;
			} else {
				stop = destination.In;
			}
			line = new Line ();
			line.Start = source.Out;
			line.Stop = stop;
		}

		public LinkAnchorObject Source {
			get;
			set;
		}

		public ActionLink Link {
			get;
			set;
		}

		public LinkAnchorObject Destination {
			get;
			set;
		}

		public virtual Area Area {
			get {
				line.Start = Source.Out;
				if (Destination != null) {
					line.Stop = Destination.In;
				} else {
					line.Stop = stop;
				}
				Area area = line.Area;
				area.Start.X -= selectionSize + 2;
				area.Start.Y -= selectionSize + 2;
				area.Width += selectionSize * 2 + 4;
				area.Height += selectionSize * 2 + 4;
				return area;
			}
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection sel = line.GetSelection (point, precision, inMotion);
			if (sel != null) {
				sel.Drawable = this;
			}
			return sel;
		}

		public void Move (Selection s, Point dst, Point start)
		{
			line.Move (s, dst, start);
			stop = line.Stop;
		}

		public bool CanLink (LinkAnchorObject dest)
		{
			/* Check if the link is possible between the 2 types of anchors */
			if (!Source.CanLink (dest)) {
				return false;
			}

			/* Check if this link will result into a duplicated link */
			foreach (ActionLink link in Source.Button.Button.ActionLinks) {
				if (link.DestinationButton == dest.Button.Button &&
				    link.SourceTags.SequenceEqualSafe (Source.Tags) &&
				    link.DestinationTags.SequenceEqualSafe (dest.Tags)) {
					return false;
				}
			}
			return true;
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Color lineColor;
			int lineWidth = 4;

			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}

			if (Selected) {
				lineColor = StyleConf.ActionLinkSelected;
			} else if (Highlighted) {
				lineColor = StyleConf.ActionLinkPrelight;
			} else {
				lineColor = StyleConf.ActionLinkNormal;
			}

			tk.Begin ();
			tk.FillColor = lineColor;
			tk.StrokeColor = lineColor;
			tk.LineWidth = lineWidth;
			tk.LineStyle = LineStyle.Normal;
			tk.DrawLine (line.Start, line.Stop);
			tk.FillColor = tk.StrokeColor = Config.Style.PaletteActive;
			tk.DrawCircle (line.Stop, 2);
			tk.End ();
		}
	}
}

