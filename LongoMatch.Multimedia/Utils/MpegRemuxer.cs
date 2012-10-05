// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.IO;
using Mono.Unix;
using GLib;
using Gtk;

using LongoMatch.Interfaces.Multimedia;

namespace LongoMatch.Video.Utils
{
	public class MpegRemuxer
	{
		static string[] EXTENSIONS =  {"mts", "m2ts", "m2t", "ts", "mpeg", "mpg"};
		string filepath;
		string newFilepath;
		Dialog dialog;
		ProgressBar pb;
		IRemuxer remuxer;
		IMultimediaToolkit multimedia;
		uint timeout;
		bool cancelled;
		
		public MpegRemuxer (string filepath)
		{
			this.filepath = filepath;
			this.multimedia = new MultimediaFactory();
			newFilepath = Path.ChangeExtension(filepath, "webm");
		}
		
		public string Remux(Window parent) {
			Button cancellButton;
			
			/* Create the dialog */
			dialog = new Dialog(Catalog.GetString("Remuxing file..."), parent, DialogFlags.Modal);
			dialog.AllowGrow = false;
			dialog.AllowShrink = false;
			dialog.Deletable = false;
			
			/* Add a progress bar */
			pb = new ProgressBar();
			pb.Show();
			dialog.VBox.Add(pb);
			
			/* Add a button to cancell the task */
			cancellButton = new Button("gtk-cancel");
			cancellButton.Clicked += (sender, e) => Cancel (); 
			cancellButton.Show();
			dialog.VBox.Add(cancellButton);
			
			/* Add a timeout to refresh the progress bar */ 
			pb.Pulse();
			timeout = GLib.Timeout.Add (1000, new GLib.TimeoutHandler (Update));
			
			remuxer = multimedia.GetRemuxer(filepath, newFilepath);
			remuxer.Progress += HandleRemuxerProgress;;
			remuxer.Start();
			
			/* Wait until the thread call Destroy on the dialog */
			dialog.Run();
			return cancelled ? null : newFilepath;
		}

		void HandleRemuxerProgress (float progress)
		{
			if (progress == 1) {
				Stop ();
			}
		}
		
		private bool Update() {
			pb.Pulse();			
			return true;
		}
		
		private void Stop() {
			GLib.Source.Remove (timeout);
			dialog.Destroy();
		}
		
		void Cancel() {
			cancelled = true;
			Stop();
		}
		
		public static bool FileIsMpeg(string filepath) {
			string extension = Path.GetExtension(filepath).Replace(".", "").ToLower();
			var extensions = new List<string> (MpegRemuxer.EXTENSIONS);
			return extensions.Contains(extension);
		}
		
		public static bool AskForConversion(Window parent) {
			bool ret;
			MessageDialog md = new MessageDialog(parent, DialogFlags.Modal, MessageType.Question,
			                                     ButtonsType.YesNo,
			                                     Catalog.GetString("The file you are trying to load is not properly " +
			                                     	"supported. Would you like to convert it into a more suitable " +
			                                     	"format?"));
			md.TransientFor = parent;
			ret = md.Run() == (int)ResponseType.Yes;
			md.Destroy();
			
			return ret;
		}
	}
}

