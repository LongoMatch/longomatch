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
using System.Collections.Generic;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Handlers;

namespace LongoMatch.Services
{


	public class HotKeysManager
	{
		private Dictionary<HotKey, Category> dic;
		public event NewTagHandler newMarkEvent;

		public HotKeysManager()
		{
			dic = new Dictionary<HotKey,Category>();
		}

		// Set the active Hotkeys for the current project
		public Categories Categories {
			set {
				dic.Clear();
				if(value == null)
					return;
				foreach(Category cat in value) {
					if(cat.HotKey.Defined &&
					                !dic.ContainsKey(cat.HotKey))
						dic.Add(cat.HotKey, cat);
				}
			}
		}

		// Listen to key press events and fire a newMarkEvent event if the key combination
		// is associated to a Category
	/*	public void KeyListener(object sender, KeyPressEventArgs args) {
			Category cat = null;
			HotKey hotkey = new HotKey();

			hotkey.Key=args.Event.Key;
			hotkey.Modifier=args.Event.State & (ModifierType.Mod1Mask | ModifierType.Mod5Mask | ModifierType.ShiftMask);
			if(dic.TryGetValue(hotkey, out cat)) {
				if(newMarkEvent != null) {
					newMarkEvent(cat);
				}
			}
		}*/
	}
}
