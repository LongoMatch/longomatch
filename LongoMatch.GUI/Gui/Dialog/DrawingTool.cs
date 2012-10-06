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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

using System;
using Gdk;
using Gtk;
using Mono.Unix;

using Image = LongoMatch.Common.Image;
using LongoMatch.Common;
using LongoMatch.Gui.Component;
using LongoMatch.Store;

namespace LongoMatch.Gui.Dialog
{


	public partial class DrawingTool : Gtk.Dialog
	{
		Play play;
		int stopTime;

		public DrawingTool()
		{
			this.Build();
			drawingtoolbox1.DrawToolChanged += OnDrawingtoolbox1DrawToolChanged;
			drawingtoolbox1.TransparencyChanged += OnDrawingtoolbox1TransparencyChanged;
			drawingtoolbox1.ToolsVisible = true;
			drawingtoolbox1.InfoVisible = false;
		}

		~ DrawingTool() {
			drawingwidget1.Destroy();
		}

		public Pixbuf Image {
			set {
				drawingwidget1.SourceImage = value;
			}
		}

		public void SetPlay(Play play,int stopTime) {
			this.play = play;
			this.stopTime = stopTime;
			savetoprojectbutton.Visible = true;
		}

		protected virtual void OnDrawingtoolbox1LineWidthChanged(int width)
		{
			drawingwidget1.LineWidth = width;
		}

		protected virtual void OnDrawingtoolbox1ColorChanged(System.Drawing.Color color)
		{
			drawingwidget1.LineColor = Helpers.ToGdkColor(color);
		}

		protected virtual void OnDrawingtoolbox1VisibilityChanged(bool visible)
		{
			drawingwidget1.DrawingsVisible = visible;
		}

		protected virtual void OnDrawingtoolbox1ClearDrawing()
		{
			drawingwidget1.ClearDrawing();
		}

		protected virtual void OnDrawingtoolbox1DrawToolChanged(DrawTool tool)
		{
			drawingwidget1.DrawTool = tool;
		}

		protected virtual void OnDrawingtoolbox1TransparencyChanged(double transparency)
		{
			drawingwidget1.Transparency = transparency;
		}

		protected virtual void OnSavebuttonClicked(object sender, System.EventArgs e)
		{
			string filename;
			FileChooserDialog fChooser;
			FileFilter filter = new FileFilter();
			filter.Name = "PNG Images";
			filter.AddPattern("*.png");

			fChooser = new FileChooserDialog(Catalog.GetString("Save File as..."),
			                                 (Gtk.Window)this.Toplevel,
			                                 FileChooserAction.Save,
			                                 "gtk-cancel",ResponseType.Cancel,
			                                 "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(Config.SnapshotsDir());
			fChooser.Filter = filter;
			fChooser.DoOverwriteConfirmation = true;

			if(fChooser.Run() == (int)ResponseType.Accept) {
				filename = fChooser.Filename;
				if(System.IO.Path.GetExtension(filename) != "png")
					filename += ".png";
				drawingwidget1.SaveAll(filename);
			}
			fChooser.Destroy();
		}

		protected virtual void OnSavetoprojectbuttonClicked(object sender, System.EventArgs e)
		{
			string tempFile = System.IO.Path.GetTempFileName();
			drawingwidget1.SaveDrawings(tempFile);
			Pixbuf frame = new Pixbuf(tempFile);
			play.KeyFrameDrawing = new Drawing { Pixbuf= new Image(frame), RenderTime = stopTime};
			drawingwidget1.SaveAll(tempFile);
			frame.Dispose();
			play.Miniature = new Image(new Pixbuf(tempFile));
		}
	}
}
