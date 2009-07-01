// 
//  Copyright (C) 2009 andoni
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;
using Gtk;
using Gdk;
using Mono.Unix;
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Component
{
	
	
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayerProperties : Gtk.Bin
	{
		private const int THUMBNAIL_WIDTH = 50;
		
		public PlayerProperties()
		{
			this.Build();
		}
		
		public string Title{
			set{
				titlelabel.Text=value;	
			}
		}
		
		public Player Player{
			get{
				return new Player (nameentry.Text, positionentry.Text, (int)numberspinbutton.Value, image.Pixbuf);
			}
			
			set{
				this.nameentry.Text = value.Name;
				positionentry.Text = value.Position;
				numberspinbutton.Value = value.Number;
				image.Pixbuf = value.Photo;
			}
		}
		
		private FileFilter FileFilter{
			get{
				FileFilter filter = new FileFilter();
				filter.Name = "Images";
				filter.AddPattern("*.png");
				filter.AddPattern("*.jpg");
				filter.AddPattern("*.jpeg");
				return filter;
			}				
		}

		protected virtual void OnOpenbuttonClicked (object sender, System.EventArgs e)
		{
			Pixbuf pimage;
			int h,w;
			double rate;
			
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Choose an image"),
			                                                   (Gtk.Window)this.Toplevel,
			                                                   FileChooserAction.Open,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-open",ResponseType.Accept);
			fChooser.AddFilter(FileFilter);
			if (fChooser.Run() == (int)ResponseType.Accept)	{		
				pimage= new Gdk.Pixbuf(fChooser.Filename);	
				if (pimage != null){
					h = pimage.Height;
					w = pimage.Width;
					rate = (double)w/(double)h;
					image.Pixbuf = pimage.ScaleSimple(THUMBNAIL_WIDTH,(int)(THUMBNAIL_WIDTH/rate),InterpType.Bilinear);
				}
			}
			fChooser.Destroy();	
		}
	}
}
