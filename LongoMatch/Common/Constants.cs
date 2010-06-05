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
using Gdk;

namespace LongoMatch.Common
{
	class Constants{
		
		public const string COPYRIGHT =  "Copyright ©2007-2010 Andoni Morales Alastruey";
		
		public const string FAKE_PROJECT = "@Fake Project@";
		
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
Daniel Nylander (sv)
Joan Charmant (fr)
Joe Hansen (da)
Jorge González (es)
Kenneth Nielsen (da)
Kjartan Maraas (nb)
Mario Blättermann (de)
Matej Urbančič (sl)
Marek Cernocky (cs)
Pavel Bárta (cs)
Petr Kovar (cs)";
		
		public const int THUMBNAIL_MAX_WIDTH = 100;
		
		public const int THUMBNAIL_MAX_HEIGHT = 100;
		
		public const string WEBSITE = "http://www.longomatch.ylatuya.es";
		
		public const ModifierType STEP = Gdk.ModifierType.ShiftMask;
		
		public const Key SEEK_BACKWARD = Gdk.Key.Left;
		
		public const Key SEEK_FORWARD = Gdk.Key.Right;		
		
		public const Key FRAMERATE_UP = Gdk.Key.Up;
		
		public const Key FRAMERATE_DOWN = Gdk.Key.Down;
		
		public const Key TOGGLE_PLAY = Gdk.Key.space;	
		
		/* Output formats */
		public const string AVI = "AVI (XVID + MP3)";
		public const string MP4  = "MP4 (H264 + AAC)";
		public const string OGG  = "OGG (Theora + Vorbis)";
		public const string WEBM = "WebM (VP8 + Vorbis)";
		public const string DVD="DVD (MPEG-2 + MP3)";
	}
}
