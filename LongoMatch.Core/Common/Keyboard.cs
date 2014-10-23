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
			return Gdk.Keyval.FromName (name);
		}

		public static string NameFromKeyval (uint keyval)
		{
			return Gdk.Keyval.Name (keyval);
		}

		public static HotKey ParseEvent (Gdk.EventKey evt)
		{
			int modifier = -1;

			if (evt.State == Gdk.ModifierType.ShiftMask) {
				modifier = (int)KeyvalFromName ("Shift_L");
			} else if (evt.State == Gdk.ModifierType.Mod1Mask || evt.State == Gdk.ModifierType.Mod5Mask) {
				modifier = (int)KeyvalFromName ("Alt_L");
			} else if (evt.State == Gdk.ModifierType.ControlMask) {
				modifier = (int)KeyvalFromName ("Control_L");
			}
			return new HotKey { Key = (int) evt.KeyValue, Modifier = modifier };
		}

		public static HotKey ParseName (string name)
		{
			int key = -1, modifier = -1, i;
			
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
			if (hotkey.Modifier != -1) {
				return string.Format ("<{0}>+{1}", NameFromKeyval ((uint)hotkey.Modifier),
				                      NameFromKeyval ((uint)hotkey.Key));
			} else {
				return string.Format ("{0}", NameFromKeyval ((uint)hotkey.Key));
			}
		}
	}
}

