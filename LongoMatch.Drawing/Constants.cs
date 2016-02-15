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
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;

namespace LongoMatch.Drawing
{
	public class Constants
	{
		public const int TIMER_HEIGHT = 20;
		public const int TIMERULE_HEIGHT = 30;
		public const int TIMERULE_PLAYER_HEIGHT = 20;
		public const int TIMERULE_RULE_PLAYER_HEIGHT = 17;
		public const int TAGGER_POINT_SIZE = 5;
		public const int TAGGER_LINE_WIDTH = 3;
		public static Color TAGGER_POINT_COLOR = Color.Blue1;
		public static Color TAGGER_SELECTION_COLOR = Color.Grey1;
		public const int CATEGORY_TPL_GRID = 10;
		public const double TIMELINE_ACCURACY = 5;
		public static Color TEXT_COLOR = Color.Black;
		public static Color TIMERULE_BACKGROUND = Color.White;
		public static Color PLAY_OBJECT_SELECTED_COLOR = Color.Black;
		public static Color PLAY_OBJECT_UNSELECTED_COLOR = Color.Grey1;
		public static Color PLAYER_SELECTED_COLOR = Color.Green1;
		public static Color PLAYER_UNSELECTED_COLOR = Color.Grey2;
		public static Color PLAYER_PLAYING_COLOR = Color.Green;
		public static Color PLAYER_NOT_PLAYING_COLOR = Color.Red;
		public static Color TIMER_UNSELECTED_COLOR = Color.Blue1;
		public static Color TIMER_SELECTED_COLOR = Color.Red1;
		public static Color SELECTION_INDICATOR_COLOR = Color.Blue1;
		public const int TIMELINE_LINE_WIDTH = 1;
	}
}

