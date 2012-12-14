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
using System.Diagnostics;
using System.IO;
using System.Threading;
using Mono.Unix;
using GLib;
using Gtk;

namespace LongoMatch.Video.Utils
{
	public class MpegRemuxer
	{
		static string[] EXTENSIONS =  {"mts", "m2ts", "m2t", "ts", "mpeg", "mpg"};
		const string FORMAT = "mp4";
		const string BACKUP_FORMAT = "mkv";
		string filepath;
		string newFilepath;
		Dialog dialog;
		ProgressBar pb;
		System.Threading.Thread remuxThread;
		uint timeout;
		bool cancelled;
		
		public MpegRemuxer (string filepath)
		{
			this.filepath = filepath;
			newFilepath = Path.ChangeExtension(filepath, FORMAT);
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
			cancellButton.Clicked += OnStop; 
			cancellButton.Show();
			dialog.VBox.Add(cancellButton);
			
			/* Start the remux task in a separate thread */
			remuxThread = new System.Threading.Thread(new ThreadStart(RemuxTask));
			remuxThread.Start();
			
			/* Add a timeout to refresh the progress bar */ 
			pb.Pulse();
			timeout = GLib.Timeout.Add (1000, new GLib.TimeoutHandler (Update));
			
			/* Wait until the thread call Destroy on the dialog */
			dialog.Run();
			return cancelled ? null : newFilepath;
		}
		
		private bool Update() {
			pb.Pulse();			
			return true;
		}
		
		private void Stop() {
			if (cancelled) {
				if (remuxThread.IsAlive)
					remuxThread.Interrupt();
				File.Delete (newFilepath);
			}
			GLib.Source.Remove (timeout);
			dialog.Destroy();
		}
		
		private int LaunchRemuxer () {
			int ret = 1;
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = true;
			if (System.Environment.OSVersion.Platform != PlatformID.Win32NT) {
				startInfo.UseShellExecute = false;
			}
			startInfo.FileName = "ffmpeg";
			startInfo.Arguments = String.Format("-i {0} -vcodec copy -acodec copy -y -sn {1} ",
			                                    filepath, newFilepath);

			using (System.Diagnostics.Process exeProcess = System.Diagnostics.Process.Start(startInfo))
			{
				exeProcess.WaitForExit();
				ret = exeProcess.ExitCode;
			}
			return ret;
		}
		
		private void RemuxTask(){
			int ret;
			ret = LaunchRemuxer ();
			if (ret != 0) {
				/* Try with the backup format instead */
				System.IO.File.Delete (newFilepath);
				newFilepath = Path.ChangeExtension(filepath, BACKUP_FORMAT);
				ret = LaunchRemuxer ();
			}
			
			if (ret != 0)
				cancelled = true;
			Application.Invoke (delegate {Stop();});
		}
		
		private void OnStop (object sender, System.EventArgs args) {
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

