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


namespace LongoMatch.Core.Common
{
	public class Constants
	{
		public const string SOFTWARE_NAME = "LongoMatch";

		public const string PROJECT_NAME = SOFTWARE_NAME + " project";

		public const string DEFAULT_DB_NAME = "longomatch";
		
		public const string COPYRIGHT = "Copyright ©2014-2015 FLUENDO S.A.\nCopyright ©2007-2014 Andoni Morales Alastruey";
		
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
Pop Eugen (po)
Xavier Queralt Mateu (ca)";

		public const string WEBSITE = "http://www.longomatch.com";

		public const string LATEST_VERSION_URL = "http://cdn.longomatch.com/latest-longomatch.json";

		public const string MANUAL = "http://www.longomatch.com/documentation/";

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
		#if OSTYPE_ANDROID || OSTYPE_IOS
		public const string IMAGE_EXT = ".png";
		#else
		public const string IMAGE_EXT = ".svg";
		#endif

		public const string LOGO_ICON = "longomatch";
		public const string BACKGROUND = "images/longomatch-dark-bg" + IMAGE_EXT;
		public const string FIELD_BACKGROUND = "images/fields/field-full" + IMAGE_EXT;
		public const string HALF_FIELD_BACKGROUND = "images/fields/field-half" + IMAGE_EXT;
		public const string HHALF_FIELD_BACKGROUND = "images/fields/field-full-teameditor" + IMAGE_EXT;
		public const string GOAL_BACKGROUND = "images/fields/field-goal" + IMAGE_EXT;
		public const string SPLASH = "images/longomatch-splash" + IMAGE_EXT;
		public const string LINE_NORMAL = "line" + IMAGE_EXT;
		public const string LINE_DASHED = "dash-line" + IMAGE_EXT;
		public const string LINE_ARROW = "line-arrow" + IMAGE_EXT;
		public const string LINE_DOUBLE_ARROW = "line-double-arrow" + IMAGE_EXT;
		public const string LINE_DOT = "line-dot" + IMAGE_EXT;
		public const string LINE_DOUBLE_DOT = "line-double-dot" + IMAGE_EXT;
		public const string COMMON_TAG = "LGM_COMMON";
		
		public const int DB_VERSION = 1;

		public const int MAX_PLAYER_ICON_SIZE = 100;
		public const int MAX_SHIELD_ICON_SIZE = 100;
		public const int MAX_THUMBNAIL_SIZE = 100;
		public const int MAX_BACKGROUND_WIDTH = 600;
		public const int MAX_BACKGROUND_HEIGHT = 400;
		public const int BUTTON_WIDTH = 120;
		public const int BUTTON_HEIGHT = 80;
		
		public static Color HOME_COLOR = Color.Red1;
		public static Color AWAY_COLOR = Color.Blue1;
		
		public static Guid PenaltyCardID = new Guid ("da4df338-3392-11e4-be8d-0811963e3880");
		public static Guid ScoreID = new Guid ("dc4df338-3392-11e4-be8d-0811963e3880");
		public static Guid SubsID = new Guid ("db4df338-3392-11e4-be8d-0811963e3880");
	}
}
