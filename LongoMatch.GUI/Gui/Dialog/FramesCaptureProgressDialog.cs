// FramesCaptureProgressDialog.cs
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
using Gtk;
using Gdk;
using LongoMatch.Core;

using LongoMatch.Core.Handlers;
using LongoMatch.Video.Utils;
using LongoMatch.Video.Common;

namespace LongoMatch.Gui.Dialog
{

	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (false)]
	public partial class FramesCaptureProgressDialog : Gtk.Dialog
	{
		private FramesSeriesCapturer capturer;

		public FramesCaptureProgressDialog (FramesSeriesCapturer capturer, Gtk.Window parent)
		{
			TransientFor = parent;
			this.Build ();
			this.Deletable = false;
			this.capturer = capturer;
			capturer.Progress += Update;
			capturer.Start ();
		}

		protected virtual void Update (int actual, int total, LongoMatch.Core.Common.Image frame)
		{
			if (actual <= total) {
				progressbar.Text = Catalog.GetString ("Capturing frame: ") + actual + "/" + total;
				progressbar.Fraction = (double)actual / (double)total;
				if (frame != null) {
					if (image.Pixbuf != null)
						image.Pixbuf.Dispose ();
					image.Pixbuf = frame.Value;
				}
			}
			if (actual == total) {
				progressbar.Text = Catalog.GetString ("Done");
				cancelbutton.Visible = false;
				okbutton.Visible = true;
			}
		}

		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			capturer.Cancel ();
		}
	}
}
