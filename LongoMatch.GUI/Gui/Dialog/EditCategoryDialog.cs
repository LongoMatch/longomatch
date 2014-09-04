//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using System.Collections.Generic;
using LongoMatch.Gui.Helpers;
using LongoMatch.Store;
using Mono.Unix;

namespace LongoMatch.Gui.Dialog
{

	public partial class EditCategoryDialog : Gtk.Dialog
	{
		List<HotKey> hkList;

		public EditCategoryDialog(Project project, DashboardButton tagger)
		{
			this.Build();
			timenodeproperties2.Tagger = tagger;
			timenodeproperties2.Project = project;
			timenodeproperties2.HotKeyChanged += OnHotKeyChanged;
		}

		public List<HotKey> HotKeysList {
			set {
				hkList = value;
				timenodeproperties2.CanChangeHotkey = hkList != null;
			}
		}

		protected virtual void OnHotKeyChanged(HotKey prevHotKey, DashboardButton button) {
			if(hkList.Contains(button.HotKey)) {
				MessagesHelpers.WarningMessage(this,
				                               Catalog.GetString("This hotkey is already in use."));
				button.HotKey=prevHotKey;
				timenodeproperties2.Tagger = button; //Update Gui
			}
			else if(button.HotKey.Defined) {
				hkList.Remove(prevHotKey);
				hkList.Add(button.HotKey);
			}
		}
	}
}
