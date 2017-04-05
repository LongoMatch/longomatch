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
using Gtk;
using LongoMatch.Core.Store;
using VAS.Core.Store;

namespace LongoMatch.Gui.Dialog
{
	public partial class EditCategoryDialog : Gtk.Dialog
	{
		public EditCategoryDialog (LMProject project, DashboardButton tagger, Window parent)
		{
			TransientFor = parent;
			this.Build ();
			timenodeproperties2.Tagger = tagger;
			timenodeproperties2.Dashboard = project.Dashboard;
		}

		public EditCategoryDialog (LMProject project, EventType eventType, Window parent)
		{
			TransientFor = parent;
			this.Build ();
			timenodeproperties2.EventType = eventType;
			timenodeproperties2.Dashboard = project.Dashboard;
		}

		protected override void OnRealized ()
		{
			base.OnRealized ();
			Resize (Allocation.Width, ActionArea.Allocation.Height +
			timenodeproperties2.Allocation.Height);
		}
	}
}
