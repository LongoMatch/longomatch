//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using Gtk;
using LongoMatch.Core;

using LongoMatch.Gui.Helpers;
using LongoMatch.Core.Common;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LiveAnalysisPreferences : Gtk.Bin
	{
		CheckButton rendercb, reviewcb;

		public LiveAnalysisPreferences ()
		{
			this.Build ();
			
			rendercb = new CheckButton ();
			table1.Attach (rendercb, 1, 2, 0, 1,
				AttachOptions.Shrink,
				AttachOptions.Shrink, 0, 0);
			rendercb.CanFocus = false;
			rendercb.Show ();
			rendercb.Active = Config.AutoRenderPlaysInLive;
			rendercb.Toggled += (sender, e) => {
				Config.AutoRenderPlaysInLive = rendercb.Active;
			};
			
			reviewcb = new CheckButton ();
			table1.Attach (reviewcb, 1, 2, 2, 3,
				AttachOptions.Shrink,
				AttachOptions.Shrink, 0, 0);
			reviewcb.CanFocus = false;
			reviewcb.Show ();
			reviewcb.Active = Config.ReviewPlaysInSameWindow;
			reviewcb.Toggled += (sender, e) => {
				Config.ReviewPlaysInSameWindow = reviewcb.Active;
			};
			
			mediafilechooser1.FileChooserMode = FileChooserMode.Directory;
			mediafilechooser1.CurrentPath = Config.AutoRenderDir;
			mediafilechooser1.ChangedEvent += HandleChangedEvent;
			
		}

		void HandleChangedEvent (object sender, EventArgs e)
		{
			Config.AutoRenderDir = mediafilechooser1.CurrentPath;
		}
	}
}
