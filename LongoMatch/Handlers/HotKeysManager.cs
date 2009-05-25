// HotKeysManager.cs
//
//  Copyright (C) 2009 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
		
		
		public void SetSections(Sections sections){
			dic.Clear();
			for (int i=0;i<20;i++){
				if (sections.GetHotKey(i).Defined)	
					dic.Add(sections.GetHotKey(i),i);					
			}
					    
		}
		
		
		public void KeyListener(object sender, KeyPressEventArgs args){
			if ((args.Event.State  & (ModifierType.ControlMask | ModifierType.ShiftMask | ModifierType.SuperMask)) != 0){
				int section=-1;
				HotKey hotkey = new HotKey();
				hotkey.Key=args.Event.Key;
				hotkey.Modifier=args.Event.State & (ModifierType.ControlMask | ModifierType.ShiftMask | ModifierType.SuperMask);
				if (dic.TryGetValue(hotkey,out section)){
					
					if (newMarkEvent != null){
						newMarkEvent(section);
					}
				}
			}
		}
		
		
		
		
	}
}
