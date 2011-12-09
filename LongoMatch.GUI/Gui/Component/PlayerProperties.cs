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

using System;
using System.IO;
using Gtk;
using Gdk;
using Mono.Unix;
using LongoMatch.Store;
using LongoMatch.Gui.Popup;
using LongoMatch.Gui.Dialog;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayerProperties : Gtk.Bin
	{
		private const int THUMBNAIL_MAX_WIDTH = 50;
		private const int THUMBNAIL_MAX_HEIGHT = 50;

		private Player player;
		private CalendarPopup cp;

		public PlayerProperties()
		{
			this.Build();
			//HACK:The calendar dialog does not respond on win32
			if(Environment.OSVersion.Platform != PlatformID.Win32NT) {
				cp = new CalendarPopup();
				cp.Hide();
				cp.DateSelectedEvent += delegate(DateTime selectedDate) {
					Player.Birthday = selectedDate;
					bdaylabel.Text = selectedDate.ToShortDateString();
				};
			}
		}

		public Player Player {
			set {
				this.player = value;
				nameentry.Text = value.Name;
				positionentry.Text = value.Position;
				nationalityentry.Text = value.Nationality;
				numberspinbutton.Value = value.Number;
				weightspinbutton.Value = value.Weight;
				heightspinbutton.Value = value.Height;
				image.Pixbuf = value.Photo.Value;
				playscombobox.Active = value.Playing ? 0 : 1;
			}
			get {
				return player;
			}
		}

		private FileFilter FileFilter {
			get {
				FileFilter filter = new FileFilter();
				filter.Name = "Images";
				filter.AddPattern("*.png");
				filter.AddPattern("*.jpg");
				filter.AddPattern("*.jpeg");
				return filter;
			}
		}

		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			Pixbuf pimage;
			StreamReader file;

			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Choose an image"),
			                (Gtk.Window)this.Toplevel,
			                FileChooserAction.Open,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-open",ResponseType.Accept);
			fChooser.AddFilter(FileFilter);
			if(fChooser.Run() == (int)ResponseType.Accept)	{
				// For Win32 compatibility we need to open the image file
				// using a StreamReader. Gdk.Pixbuf(string filePath) uses GLib to open the
				// input file and doesn't support Win32 files path encoding
				file = new StreamReader(fChooser.Filename);
				pimage= new Gdk.Pixbuf(file.BaseStream);
				if(pimage != null) {
					var img = new LongoMatch.Common.Image(pimage);
					img.Scale(THUMBNAIL_MAX_WIDTH, THUMBNAIL_MAX_HEIGHT);
					player.Photo = img;
					image.Pixbuf = img.Value;
				}
			}
			fChooser.Destroy();
		}

		protected virtual void OnNameentryChanged(object sender, System.EventArgs e)
		{
			player.Name = nameentry.Text;
		}

		protected virtual void OnPositionentryChanged(object sender, System.EventArgs e)
		{
			player.Position = positionentry.Text;
		}

		protected virtual void OnNumberspinbuttonChanged(object sender, System.EventArgs e)
		{
			player.Number =(int) numberspinbutton.Value;
		}

		protected virtual void OnNumberspinbuttonValueChanged(object sender, System.EventArgs e)
		{
			player.Number =(int) numberspinbutton.Value;
		}

		protected virtual void OnDatebuttonClicked(object sender, System.EventArgs e)
		{
			if(Environment.OSVersion.Platform == PlatformID.Win32NT) {
				var win32CP = new Win32CalendarDialog();
				win32CP.TransientFor = (Gtk.Window)this.Toplevel;
				win32CP.Run();
				player.Birthday = win32CP.getSelectedDate();
				bdaylabel.Text = win32CP.getSelectedDate().ToShortDateString();
				win32CP.Destroy();
			}
			else {
				cp.TransientFor=(Gtk.Window)this.Toplevel;
				cp.Show();
			}
		}

		protected virtual void OnWeightspinbuttonValueChanged(object sender, System.EventArgs e)
		{
			player.Weight = (int)weightspinbutton.Value;
		}

		protected virtual void OnHeightspinbuttonValueChanged(object sender, System.EventArgs e)
		{
			player.Height = (float)heightspinbutton.Value;
		}

		protected virtual void OnNationalityentryChanged(object sender, System.EventArgs e)
		{
			player.Nationality = nationalityentry.Text;
		}

		protected virtual void OnPlayscomboboxChanged(object sender, System.EventArgs e)
		{
			player.Playing = playscombobox.ActiveText == Catalog.GetString("Yes");
		}


	}
}
