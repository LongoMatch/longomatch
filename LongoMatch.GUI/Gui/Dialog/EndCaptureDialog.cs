//
//  Copyright (C) 2010 Andoni Morales Alastruey
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
using VAS.Core.Common;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Dialog
{


	public partial class EndCaptureDialog : Gtk.Dialog
	{

		public EndCaptureDialog (Window parent, bool isCapturing)
		{
			TransientFor = parent;
			this.Build ();
			if (!isCapturing)
				savebutton.Visible = false;

			image439.Pixbuf = Helpers.Misc.LoadStockIcon (this, "gtk-dialog-question", Gtk.IconSize.Dialog);
		}

		protected virtual void OnQuit (object sender, System.EventArgs e)
		{
			if (sender == quitbutton)
				Respond ((int)EndCaptureResponse.Quit);
			else if (sender == savebutton)
				Respond ((int)EndCaptureResponse.Save);
			else
				Respond ((int)EndCaptureResponse.Return);
		}
	}
}
