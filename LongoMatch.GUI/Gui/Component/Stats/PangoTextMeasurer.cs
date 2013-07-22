//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using Pango;
using OxyPlot;

namespace LongoMatch.Gui.Component.Stats
{
	public class PangoTextMeasurer: ITextMeasurer
	{
		Layout layout;
		
		public PangoTextMeasurer ()
		{
			layout = new Layout (Gdk.PangoHelper.ContextGet());
		}
		
		public OxySize MeasureText(string text, string fontFamily = "",
		                    double fontSize = 10, double fontWeight = 500) {
			FontDescription desc = new FontDescription();
			OxySize size = new OxySize();
			int width, height;
			
			desc.Family = fontFamily;
			desc.Size = (int) fontSize;
			desc.Weight = PangoWeightFromDouble (fontWeight);
			layout.SetText (text);
			layout.GetPixelSize (out width, out height);
			size.Width = (double) width;
			size.Height = (double) height;
			return size;
		}
		
		Weight PangoWeightFromDouble (double weight) {
			if (weight <= 250)
				return Weight.Ultralight;
			if (weight <= 350)
				return Weight.Light;
			if (weight <= 600)
				return Weight.Normal;
			if (weight <= 750)
				return Weight.Bold;
			if (weight <= 850)
				return Weight.Ultrabold;
			return Weight.Heavy;
		}
	}
}

