//
//  Copyright (C) 2007-2010 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

using System;
#if HAVE_GTK
using Gdk;
#endif


namespace LongoMatch.Common
{
	public class Constants {
		public const string SOFTWARE_NAME = "LongoMatch";

		public const string PROJECT_NAME = SOFTWARE_NAME + " project";

		public const string DB_FILE = "longomatch.1.db";
		
		public const string COPYRIGHT =  "Copyright ©2007-2010 Andoni Morales Alastruey";
		
		public const string FAKE_PROJECT = "@Fake Project@";
		
		public const string PORTABLE_FILE = "longomatch.portable";
		
		public const string LICENSE =
		        @"This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.";

		public const string TRANSLATORS =
		        @"Andoni Morales Alastruey (es)
Aron Xu (cn_ZH)
Barkın Tanmann (tr)
Bruno Brouard (fr)
Daniel Nylander (sv)
G. Baylard (fr)
Joan Charmant (fr)
João Paulo Azevedo (pt)
Joe Hansen (da)
Jorge González (es)
Kenneth Nielsen (da)
Kjartan Maraas (nb)
Peter Strikwerda (nl)
Laurent Coudeur (fr)
Marek Cernocky (cs)
Mario Blättermann (de)
Matej Urbančič (sl)
Maurizio Napolitano (it)
Pavel Bárta (cs)
Petr Kovar (cs)
Xavier Queralt Mateu (ca)";

		public const int THUMBNAIL_MAX_WIDTH = 100;

		public const int THUMBNAIL_MAX_HEIGHT = 100;

		public const string WEBSITE = "http://www.longomatch.ylatuya.es";

		public const string MANUAL = "http://www.longomatch.ylatuya.es/documentation/manual.html";

#if HAVE_GTK
		public const ModifierType STEP = Gdk.ModifierType.ShiftMask;

		public const Key SEEK_BACKWARD = Gdk.Key.Left;

		public const Key SEEK_FORWARD = Gdk.Key.Right;

		public const Key FRAMERATE_UP = Gdk.Key.Up;

		public const Key FRAMERATE_DOWN = Gdk.Key.Down;

		public const Key TOGGLE_PLAY = Gdk.Key.space;
#endif

		public const string TEMPLATES_DIR = "templates";
		public const string TEAMS_TEMPLATE_EXT = ".ltt";
		public const string CAT_TEMPLATE_EXT = ".lct";
		public const string SUBCAT_TEMPLATE_EXT = ".lst";
		public const string PLAYLIST_EXT = ".lpl";
	}
}
