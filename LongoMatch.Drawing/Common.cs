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
using LongoMatch.Store;
using LongoMatch.Common;

namespace LongoMatch.Drawing
{
	public class Common
	{
		public const int CATEGORY_HEIGHT = 20;
		public const int CATEGORY_WIDTH = 150;
		public const int CATEGORY_H_SPACE = 5;
		public const double TIMELINE_ACCURACY = 5;
		public static Color TEXT_COLOR = Color.Black;
		public static Color TIMELINE_LINE_COLOR = Color.Black;
		public static Color TIMERULE_BACKGROUND = Color.White;
		public static Color PLAY_OBJECT_SELECTED_COLOR = Color.Black;
		public static Color PLAY_OBJECT_UNSELECTED_COLOR = Color.Grey;
		public const int TIMELINE_LINE_WIDTH = 1;
		
		public static double TimeToPos (Time time, double secondsPerPixel) {
			return (double)time.MSeconds / 1000 / secondsPerPixel;
		}
		
		public static Time PosToTime (Point p, double secondsPerPixel) {
			return new Time ((int) (p.X * 1000 * secondsPerPixel));
		}
	}
}

