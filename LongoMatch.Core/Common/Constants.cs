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
using Color = VAS.Core.Common.Color;

#if HAVE_GTK
using Gdk;
#endif


namespace LongoMatch.Core.Common
{
	public class Constants : VAS.Core.Common.Constants
	{
		new public const string SOFTWARE_NAME = "LongoMatch";

		new public const string PROJECT_NAME = SOFTWARE_NAME + " project";

		new public const string DEFAULT_DB_NAME = "longomatch";

		public const string COPYRIGHT = "Copyright ©2014-2015 FLUENDO S.A.\nCopyright ©2007-2014 Andoni Morales Alastruey";

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
Pop Eugen (po)
Xavier Queralt Mateu (ca)";

		public const string WEBSITE = "http://www.longomatch.com";

#if DEBUG
		public const string LATEST_VERSION_URL = "http://cdn.longomatch.com/latest-longomatch.json";
#else
		public const string LATEST_VERSION_URL = "http://www.longomatch.com/latest-longomatch";
#endif

#if OSTYPE_ANDROID || OSTYPE_IOS
		public const string MANUAL = "https://fluendo.atlassian.net/wiki/display/LPD/Tag2win+app";
#else
		public const string MANUAL = "http://www.longomatch.com/documentation/";
#endif

#if HAVE_GTK
		public const int STEP = (int)Gdk.ModifierType.ShiftMask;

		public const int SEEK_BACKWARD = (int)Gdk.Key.Left;

		public const int SEEK_FORWARD = (int)Gdk.Key.Right;

		public const int FRAMERATE_UP = (int)Gdk.Key.Up;

		public const int FRAMERATE_DOWN = (int)Gdk.Key.Down;

		public const int TOGGLE_PLAY = (int)Gdk.Key.space;
#endif

		public const string TEMPLATES_DIR = "templates";
		public const string TEAMS_TEMPLATE_EXT = ".ltt";
		public const string CAT_TEMPLATE_EXT = ".lct";
		public const string SUBCAT_TEMPLATE_EXT = ".lst";
		public const string PLAYLIST_EXT = ".lpl";
		public const string PROJECT_EXT = ".lgm";
		public const string PROJECT_ENCRYPTED_EXT = ".lgmx";

		public const string LOGO_ICON = "lm-any";
		public const string LOGO_BASIC_ICON = "lm-basic";
		public const string LOGO_STARTER_ICON = "lm-starter";
		public const string LOGO_PRO_ICON = "lm-pro";

		//FIXME: this should go to a new StyleConf in longomatch
		public const string LM_LOGO_ANY = "images/lm-any-logo-green" + IMAGE_EXT;

		public const string SPLASH = "images/lm-splash" + IMAGE_EXT;
		public const string COMMON_TAG = "LGM_COMMON";

		public static Color HOME_COLOR = Color.Red1;
		public static Color AWAY_COLOR = Color.Blue1;

		public static Guid PenaltyCardID = new Guid ("da4df338-3392-11e4-be8d-0811963e3880");
		public static Guid ScoreID = new Guid ("dc4df338-3392-11e4-be8d-0811963e3880");
		public static Guid SubsID = new Guid ("db4df338-3392-11e4-be8d-0811963e3880");

		// Pixels limit to start scrolling a window when using drag and drop
		public const uint FAST_SCROLL_PIXELS = 5;
		public const uint SLOW_SCROLL_PIXELS = 10;

		public const string LICENSE_CODE_URL = "https://longomatch.com/profile/register_personal/";
		public const int DB_VERSION_MAJOR = 1;
		public const int DB_VERSION_MINOR = 0;
	}
}