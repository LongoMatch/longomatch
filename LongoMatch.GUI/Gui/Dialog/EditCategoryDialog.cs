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
using Gtk;
using LongoMatch.Store;
using LongoMatch.Handlers;
using LongoMatch.Gui.Component;
using LongoMatch.Gui;
using Mono.Unix;

using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Interfaces;

namespace LongoMatch.Gui.Dialog
{


	public partial class EditCategoryDialog : Gtk.Dialog
	{
		private List<HotKey> hkList;

		public EditCategoryDialog(ITemplatesService ts)
		{
			this.Build();
			timenodeproperties2.HotKeyChanged += OnHotKeyChanged;
			timenodeproperties2.LoadSubcategories(ts);
		}

		public Category Category {
			set {
				timenodeproperties2.Category = value;
			}
		}
		
		public Project Project {
			set {
				timenodeproperties2.Project = value;
			}
		}

		public List<HotKey> HotKeysList {
			set {
				hkList = value;
			}
		}

		protected virtual void OnHotKeyChanged(HotKey prevHotKey, Category category) {
			if(hkList.Contains(category.HotKey)) {
				MessagePopup.PopupMessage(this,MessageType.Warning,
				                          Catalog.GetString("This hotkey is already in use."));
				category.HotKey=prevHotKey;
				timenodeproperties2.Category = category; //Update Gui
			}
			else if(category.HotKey.Defined) {
				hkList.Remove(prevHotKey);
				hkList.Add(category.HotKey);
			}
		}
	}
}
