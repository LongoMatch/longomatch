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
using LongoMatch.Store;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Handlers;
using LongoMatch.Video.Utils;
using LongoMatch.Multimedia.Utils;

namespace LongoMatch.Video.Remuxer
{
	public class MpegRemuxer: IRemuxer
	{
		public event ErrorHandler Error;
		public event ProgressHandler Progress;
		const string FORMAT = "mp4";
		const string BACKUP_FORMAT = "mkv";
		string inputFilepath;
		string outputFilepath;
		System.Threading.Thread remuxThread;
		
		public MpegRemuxer (string inputFilepath, string outputFilepath)
		{
			this.inputFilepath = inputFilepath;
			this.outputFilepath = outputFilepath;
		}
		
		public void Start() {
			remuxThread = new System.Threading.Thread(new ThreadStart(RemuxTask));
			remuxThread.Start();
		}
		
		public void Cancel() {
			if (remuxThread.IsAlive)
				remuxThread.Interrupt();
			try {
				File.Delete (this.outputFilepath);
			} catch {
			}
		}
		
		private int LaunchRemuxer () {
			int ret = 1;
			
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = true;
			if (System.Environment.OSVersion.Platform != PlatformID.Win32NT) {
				startInfo.UseShellExecute = false;
			}
			startInfo.FileName = "avconv";
			startInfo.Arguments = String.Format("-i {0} -vcodec copy -acodec copy -y -sn {1} ",
			                                    inputFilepath, outputFilepath);

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
				System.IO.File.Delete (outputFilepath);
				outputFilepath = Path.ChangeExtension(inputFilepath, BACKUP_FORMAT);
				ret = LaunchRemuxer ();
			}
			
			if (ret != 0) {
				if (Error != null) {
					Error ("Unkown error");
				}
			} else {
				if (Progress != null) {
					Progress (1);
				}
			}
		}
	}
}

