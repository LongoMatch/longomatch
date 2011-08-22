// TreeWidgetPopup.cs
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

using Gdk;
using Gtk;
using LongoMatch.Store;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class TagsTreeView : ListTreeViewBase
	{

		public TagsTreeView() {
			tag.Visible = false;
			delete.Visible = false;
		}

		override protected bool OnButtonPressEvent(EventButton evnt)
		{
			TreePath[] paths = Selection.GetSelectedRows();

			if((evnt.Type == EventType.ButtonPress) && (evnt.Button == 3))
			{
				// We don't want to unselect the play when several
				// plays are selected and we clik the right button
				// For multiedition
				if(paths.Length <= 1) {
					base.OnButtonPressEvent(evnt);
					paths = Selection.GetSelectedRows();
				}

				if(paths.Length == 1) {
					Play selectedTimeNode = GetValueFromPath(paths[0]) as Play;
					deleteKeyFrame.Sensitive = selectedTimeNode.KeyFrameDrawing != null;
					MultiSelectMenu(false);
					menu.Popup();
				}
				else if(paths.Length > 1) {
					MultiSelectMenu(true);
					menu.Popup();
				}
			}
			else
				base.OnButtonPressEvent(evnt);
			return true;
		}

		override protected bool SelectFunction(TreeSelection selection, TreeModel model, TreePath path, bool selected) {
			return true;
		}

		override protected bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			return false;
		}
	}
}

