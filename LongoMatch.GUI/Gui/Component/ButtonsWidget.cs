// ButtonsWidget.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Gui;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ButtonsWidget : Gtk.Bin
	{
		public event NewTagHandler NewMarkEvent;
		public event NewTagStartHandler NewMarkStartEvent;
		public event NewTagStopHandler NewMarkStopEvent;
		public event NewTagCancelHandler NewMarkCancelEvent;

		Categories categories;
		TagMode tagMode;
		Dictionary<ButtonTagger, Category> buttonsDic;

		public ButtonsWidget()
		{
			this.Build();
			buttonsDic = new Dictionary<ButtonTagger, Category>();
			Mode = TagMode.Predifined;
		}

		public TagMode Mode {
			set {
				tagMode = value;
				foreach (ButtonTagger b in buttonsDic.Keys) {
					b.Mode = tagMode;
				}
			}
		}
		
		public Time CurrentTime {
			set {
				foreach (ButtonTagger b in buttonsDic.Keys) {
					b.CurrentTime = value;
				}
			}
		}

		public Categories Categories {
			set {
				foreach(Widget w in table1.AllChildren) {
					table1.Remove(w);
					w.Destroy();
				}
				categories = value;
				if(value == null)
					return;

				buttonsDic.Clear();
				int sectionsCount = value.Count;

				table1.NColumns =(uint) 10;
				table1.NRows =(uint)(sectionsCount/10);

				for(int i=0; i<sectionsCount; i++) {
					Category cat = value[i];
					ButtonTagger b = new ButtonTagger (cat);
					b.NewTag += (category) => {
						if (NewMarkEvent != null) {
							NewMarkEvent (category);
						}
					};
					b.NewTagStart += (category) => {
						if (NewMarkStartEvent != null) {
							NewMarkStartEvent (category);
						}
					};
					b.NewTagStop += (category) => {
						if (NewMarkStopEvent != null) {
							NewMarkStopEvent (category);
						}
					};
					b.NewTagCancel += (category) => {
						if (NewMarkCancelEvent != null) {
							NewMarkCancelEvent (category);
						}
					};
					b.Mode = tagMode;

					uint row_top =(uint)(i/table1.NColumns);
					uint row_bottom = (uint) row_top+1 ;
					uint col_left = (uint) i%table1.NColumns;
					uint col_right = (uint) col_left+1 ;

					table1.Attach(b,col_left,col_right,row_top,row_bottom);

					buttonsDic.Add(b, cat);
					b.Show();
				}
			}
		}
	}
}
