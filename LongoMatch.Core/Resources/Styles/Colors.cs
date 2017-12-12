//
//  Copyright (C) 2017 Fluendo S.A.
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

using VAS.Core.Common;

namespace LongoMatch.Core.Resources.Styles
{
	public class Colors : VAS.Core.Resources.Styles.Colors
	{
		public const float PlayerSelectionAlpha = 0.4f;
		public const float PlayerNumberAlpha = 0.9f;

		public static Color PlayerBackground => App.Current.Style.ColorWhite;
		public static Color BorderSelection = Color.Parse ("#428C41");
	}
}
