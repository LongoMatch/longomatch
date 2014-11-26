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
using LongoMatch.Core.Interfaces.GUI;

namespace LongoMatch.Gui.Dialog
{
	public class BusyDialog: Gtk.Dialog, IBusyDialog
	{
		VBox box;
		Label titleLabel;
		ProgressBar progressBar;

		public BusyDialog ()
		{
			box = new VBox (false, 10);
			titleLabel = new Label ();
			progressBar = new ProgressBar ();
			box.PackStart (titleLabel, true, true, 0);
			box.PackStart (progressBar, true, true, 0);
			box.ShowAll ();
			VBox.PackStart (box);
			Icon = LongoMatch.Gui.Helpers.Misc.LoadIcon ("longomatch", 28);
			TypeHint = Gdk.WindowTypeHint.Dialog;
			WindowPosition = WindowPosition.Center;
			Modal = true;
			Resizable = false;
			Gravity = Gdk.Gravity.Center; 
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			DefaultWidth = 300;
			DefaultHeight = 100;
		}

		public string Message {
			set {
				titleLabel.Text = value;
			}
		}

		public void Pulse ()
		{
			progressBar.Pulse ();
		}

		public void ShowSync ()
		{
			Run ();
		}
	}
}

