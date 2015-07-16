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

namespace LongoMatch.Core.Common
{
	public class Keyboard
	{
		public static uint KeyvalFromName (string name)
		{
			#if ! OSTYPE_ANDROID && ! OSTYPE_IOS
			return Gdk.Keyval.FromName (name);
			#else
			return 0;
			#endif
		}

		public static string NameFromKeyval (uint keyval)
		{
			#if !OSTYPE_ANDROID && !OSTYPE_IOS
			return Gdk.Keyval.Name (keyval);
			#else
			return "";
			#endif
		}

		#if !OSTYPE_ANDROID && !OSTYPE_IOS
		public static HotKey ParseEvent (Gdk.EventKey evt)
		{
			int modifier = 0;

			if (evt.State == Gdk.ModifierType.ShiftMask) {
				modifier = (int)KeyvalFromName ("Shift_L");
			} else if (evt.State == Gdk.ModifierType.Mod1Mask || evt.State == Gdk.ModifierType.Mod5Mask) {
				modifier = (int)KeyvalFromName ("Alt_L");
			} else if (evt.State == Gdk.ModifierType.ControlMask) {
				modifier = (int)KeyvalFromName ("Control_L");
			}
			return new HotKey { Key = (int)Gdk.Keyval.ToLower (evt.KeyValue),
				Modifier = modifier
			};
		}
		#endif

		public static HotKey ParseName (string name)
		{
			int key = 0, modifier = 0, i;
			
			if (name.Contains (">+")) {
				i = name.IndexOf ('+');
				modifier = (int)KeyvalFromName (name.Substring (1, i - 2));
				key = (int)KeyvalFromName (name.Substring (i + 1)); 
			} else {
				key = (int)KeyvalFromName (name);
			}
			return new HotKey { Key = key, Modifier = modifier };
		}

		public static string HotKeyName (HotKey hotkey)
		{
			if (hotkey.Modifier != -1 && hotkey.Modifier != 0) {
				return string.Format ("<{0}>+{1}", NameFromKeyval ((uint)hotkey.Modifier),
				                      NameFromKeyval ((uint)hotkey.Key));
			} else {
				return string.Format ("{0}", NameFromKeyval ((uint)hotkey.Key));
			}
		}
	}
}

