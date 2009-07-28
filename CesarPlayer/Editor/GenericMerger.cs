// 
//  Copyright (C) 2009 Andoni Morales Alastruey 2009
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
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Mono.Unix;
using LongoMatch.Video.Handlers;

namespace LongoMatch.Video.Editor
{
	
	
	abstract public class GenericMerger:IMerger
	{
		
		public  event EventHandler MergeDone;
		public  event ErrorHandler Error;
		
		protected List<string> filesToMergeList;
		protected string outputFile;
		protected Process process;
		protected Thread mergeThread;
		protected string command;
		protected ProcessStartInfo pinfo;
		protected VideoMuxer outputMuxer;
		protected VideoCodec inputFilesVideoCodec;
		
		public GenericMerger(string command)
		{
			filesToMergeList = new List<string>();
			mergeThread = new Thread(new ThreadStart(MergeSegments));
			this.command = command;
		}
		
		public List<string> FilesToMerge{
			set{filesToMergeList = value;}
		}
		
		public string OutputFile {
			set{outputFile = value;}
		}
		
		public VideoMuxer OutputMuxer{
			set{outputMuxer = value;}			
		}
		
		public VideoCodec InputFilesVideoCodec{
			set{inputFilesVideoCodec = value;}
		}
		
		public bool Start(){
			if (!mergeThread.IsAlive){
				mergeThread.Start();
				return true;
			}
			else return false;
		}
		
		public void Cancel(){
			if (process != null && !process.HasExited)
				process.Kill();
			
			if (mergeThread != null && mergeThread.IsAlive){
				mergeThread.Abort();
				mergeThread = null;					
			}			
		}
		
		virtual protected void MergeSegments (){
			process = new Process();
			pinfo = new ProcessStartInfo();
			if (System.Environment.OSVersion.Platform != PlatformID.Unix)
				pinfo.FileName=System.IO.Path.Combine(System.Environment.CurrentDirectory,command+".exe");
			else 
				pinfo.FileName=command;			
			pinfo.Arguments = CreateMergeCommandLineArgs();
			pinfo.CreateNoWindow = true;
			pinfo.UseShellExecute = false;
			process.StartInfo = pinfo;
			try {
				process.Start();
				ReadOutput();
				process.WaitForExit();	
			}
			catch {
				if (Error != null){
					ErrorArgs args = new ErrorArgs ();
					args.Args = new object[1];
					args.Args[0] = Catalog.GetString("Error merging video segments. Please retry again");
					Error (this, args);
				}
			}
			if (this.MergeDone != null)
				this.MergeDone(this,new EventArgs());
		}
		
		virtual public void ReadOutput(){
		}
		
		abstract protected string CreateMergeCommandLineArgs();
		
	}
}
