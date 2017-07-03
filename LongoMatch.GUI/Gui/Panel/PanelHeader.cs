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
using Gtk;
using VAS.Core.Common;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PanelHeader : Gtk.Bin
	{
	
		public event EventHandler BackClicked;
		public event EventHandler ApplyClicked;

		public PanelHeader ()
		{
			this.Build ();
			applyroundedbutton.Clicked += (sender, e) => {
				if (ApplyClicked != null) {
					ApplyClicked (this, null);
				}
			};
			backrectbutton.Clicked += (sender, e) => {
				if (BackClicked != null) {
					BackClicked (this, null);
				}
			};
			logoimage.Image = App.Current.ResourcesLocator.LoadIcon (App.Current.SoftwareIconName, 45);
			backrectbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("lm-back", 40);
			applyroundedbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-apply", 40);
			headerhbox.HeightRequest = StyleConf.HeaderHeight;
		}

		public string Title {
			set {
				titlelabel.Markup = String.Format ("<span font_desc=\"{0}\"><b>{1}</b></span>",
					StyleConf.HeaderFontSize, value);
			}
		}

		public bool BackVisible {
			set {
				backrectbutton.Visible = value;
			}
		}

		public bool ApplyVisible {
			set {
				applyroundedbutton.Visible = value;
			}
		}
	}
}

