//
//  Copyright (C) 2014 dolphy
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
using LongoMatch;
using Pango;
using VAS;
using VAS.Core;
using VAS.Core.Common;

namespace LongoMatch.Gui.Dialog
{
	public partial class CodecsChoiceDialog : Gtk.Dialog
	{
		public CodecsChoiceDialog ()
		{
			this.Build ();

			Image img = App.Current.ResourcesLocator.LoadImage ("images/longomatch-pro-small.png");
			buttonOKimage.Pixbuf = img.Value;

			titlelabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 14"));

			// Configure URL handler for the links
			label1.SetLinkHandler (url => {
				try {
					System.Diagnostics.Process.Start (url);
				} catch {
				}
			});
		}
	}
}

