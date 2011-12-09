//
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using Cairo;
using Gtk;
using Gdk;
using Pango;
using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{
	public class TimelineLabelsWidget: Gtk.DrawingArea
	{
		private const int SECTION_HEIGHT = 30;
		private const int SECTION_WIDTH = 100;
		private const int LINE_WIDTH = 2;
		private double scroll;
		Pango.Layout layout;
		Dictionary<string, Gdk.Color> labelsDict;

		[System.ComponentModel.Category("LongoMatch")]
		[System.ComponentModel.ToolboxItem(true)]
		public TimelineLabelsWidget()
		{
			layout =  new Pango.Layout(PangoContext);
			layout.Wrap = Pango.WrapMode.Char;
			layout.Alignment = Pango.Alignment.Left;
			labelsDict = new Dictionary<string, Gdk.Color> ();
		}

		public List<string> Labels {
			set {
				labelsDict.Clear();
				foreach (String label in value)
					labelsDict.Add(label, Gdk.Color.Zero);
			}
		}
		
		public Categories Categories {
			set {
				labelsDict.Clear();
				foreach (Category cat in value)
					labelsDict.Add(cat.Name, Helpers.ToGdkColor(cat.Color));
			}
		}

		public double Scroll {
			get	{
				return scroll;
			}
			set {
				scroll = value;
				QueueDraw();
			}
		}

		private void DrawCairoText(string text, int x1, int y1) {
			layout.Width = Pango.Units.FromPixels(SECTION_WIDTH - 2);
			layout.Ellipsize = EllipsizeMode.End;
			layout.SetMarkup(text);
			GdkWindow.DrawLayout(Style.TextGC(StateType.Normal),
			                     x1 + 2, y1 ,layout);
		}

		private void DrawCategories(Gdk.Window win) {
			int i = 0;

			if(labelsDict.Count == 0)
				return;

			using(Cairo.Context g = Gdk.CairoHelper.Create(win)) {
				foreach(String label in labelsDict.Keys) {
					int y = LINE_WIDTH/2 + i * SECTION_HEIGHT - (int)Scroll;
					CairoUtils.DrawRoundedRectangle(g, 2, y + 3 , Allocation.Width - 3,
					                                SECTION_HEIGHT - 3, SECTION_HEIGHT/7,
					                                CairoUtils.RGBToCairoColor(labelsDict[label]),
					                                CairoUtils.RGBToCairoColor(labelsDict[label]));
					DrawCairoText(label, 0 + 3, y + SECTION_HEIGHT / 2 - 5);
					i++;
				}
			}
		}

		protected override bool OnExposeEvent(EventExpose evnt)
		{
			DrawCategories(evnt.Window);
			return base.OnExposeEvent(evnt);
		}
	}
}
