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
using LongoMatch.Store.Templates;
using LongoMatch.Common;
using LongoMatch.Gui.Helpers;
using LongoMatch.Store;
using Mono.Unix;
using System.Collections.Generic;
using Gdk;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnalysisTemplateEditor : Gtk.Bin
	{
		bool edited;
		Categories template;

		public AnalysisTemplateEditor ()
		{
			this.Build ();
			buttonswidget.Mode = TagMode.Edit;
			savebutton.Clicked += HandleSaveClicked;
		}

		public Categories Template {
			set {
				template = value;
				buttonswidget.Template = value;
				Edited = false;
			}
		}
		
		public bool Edited {
			set {
				edited = value;
			}
			get {
				return buttonswidget.Edited;
			}
		}
		
		void HandleSaveClicked (object sender, EventArgs e)
		{
			if (template != null) {
				Config.CategoriesTemplatesProvider.Update (template);
			}
		}
		
	}
}

