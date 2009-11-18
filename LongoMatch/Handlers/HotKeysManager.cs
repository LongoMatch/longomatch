// HotKeysManager.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;

namespace LongoMatch.Handlers
{


	public class HotKeysManager
	{
		private Dictionary<HotKey,int> dic;
		public event NewMarkEventHandler newMarkEvent;

		public HotKeysManager()
		{
			dic = new Dictionary<HotKey,int>();
		}

		public Sections Sections {
			set {
				dic.Clear();
				if (value == null)
					return;
				for (int i=0;i<value.Count;i++) {
					if (value.GetHotKey(i).Defined &&
					                !dic.ContainsKey(value.GetHotKey(i)))
						dic.Add(value.GetHotKey(i),i);
				}
			}
		}

		public void KeyListener(object sender, KeyPressEventArgs args) {
			if ((args.Event.State  & (ModifierType.Mod1Mask | ModifierType.Mod5Mask | ModifierType.ShiftMask)) != 0) {
				int section=-1;
				HotKey hotkey = new HotKey();
				hotkey.Key=args.Event.Key;
				hotkey.Modifier=args.Event.State & (ModifierType.Mod1Mask | ModifierType.Mod5Mask | ModifierType.ShiftMask);
				if (dic.TryGetValue(hotkey,out section)) {
					if (newMarkEvent != null) {
						newMarkEvent(section);
					}
				}
			}
		}




	}
}
