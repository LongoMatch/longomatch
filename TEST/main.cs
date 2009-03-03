// Main.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//


using System;
using Gtk;
using LongoMatch.Video.Capturer;
using LongoMatch.Gui;
using System.Runtime.InteropServices;

namespace LongoMatch
	
{
	
	class MainClass
	{
		
		
		public static void Main (string[] args)
		{	
			string baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
			if (Environment.OSVersion.Platform != PlatformID.Unix)
				Environment.SetEnvironmentVariable("GST_PLUGIN_PATH",System.IO.Path.Combine(baseDirectory,"..\\lib\\gstreamer-0.10"));
			Application.Init ();
			Gtk.Window win = new Window(Gtk.WindowType.Toplevel);
			LongoMatch.Video.Capturer.GstCameraCapturer.InitBackend("");
			LongoMatch.Gui.CapturerBin cap = new CapturerBin();
			cap.OutputFile="testtt.avi";		
			cap.Run();
			win.Add((Gtk.Widget)cap);
			cap.Show();
			win.ShowAll ();		
			Application.Run ();
			
		}
		
	

	}
}
