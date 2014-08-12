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

namespace LongoMatch.Common
{
	public class StyleConf
	{
		public int WelcomeBorder { get; set; }

		public int WelcomeIconSize { get; set; }

		public int WelcomeLogoWidth { get; set; }

		public int WelcomeLogoHeight { get; set; }

		public int WelcomeIconsHSpacing { get; set; }

		public int WelcomeIconsVSpacing { get; set; }

		public int WelcomeIconsTextSpacing { get; set; }

		public int WelcomeIconsTextHeight { get; set; }

		public int WelcomeIconsPerRow { get; set; }

		public int WelcomeTextHeight { get; set; }

		public int WelcomeMinWidthBorder { get; set; }

		public int TeamsComboColorHeight { get; set; }

		public static StyleConf Load (string filename)
		{
			return Serializer.Load <StyleConf> (filename);
		}
	}
}
