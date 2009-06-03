// 
//  Copyright (C) 2009 Andoni Morales Alastruey
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
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Gtk;

namespace LongoMatch.Video.Editor
{
	
	
	public class GnlMkvEditor : IVideoEditor
	{
		public event LongoMatch.Video.Handlers.ProgressHandler Progress;	
		
		private GstVideoSplitter splitter;
		private Queue<VideoSegment> segmentsList;
		private Queue<string> segmentsTempFiles;
		private int height;
		private int width;
		private string outputFile;
		private string tempDir;
		private int segmentCoded;
		private Thread thread;
		private bool readyToMerge;
		
		public GnlMkvEditor()
		{			
			splitter = new GstVideoSplitter();
			splitter.PercentCompleted += new PercentCompletedHandler(OnProgress); 
			splitter.Error += new ErrorHandler(OnError);
			segmentsList = new Queue<VideoSegment>();	
			segmentsTempFiles = new Queue<string>();
			tempDir = System.IO.Path.GetTempPath();
			segmentCoded = -1;
		}		
		
		public VideoQuality VideoQuality{
			set { splitter.VideoBitrate = (int)value;}
		}
		
		public AudioQuality AudioQuality{
			set{ splitter.VideoBitrate = (int)value;}
		}
		
		public VideoFormat VideoFormat{
			set {
				if (value == VideoFormat.TV){
					height = 540;
					width = 720;
				}
				else if (value == VideoFormat.HD720p){
					height = 720;
					width = 1280;
				}
				else if (value == VideoFormat.HD1080p){
					height = 1080;
					width = 1920;
				}
				splitter.Height = height;
				splitter.Width = width;
			}
		}
		
				
		public string OutputFile{
			set{ 
				outputFile = value;
				splitter.OutputFile = value;}
		}
		
		public string TempDir{
			set{tempDir = value;}
		}
		
		public bool EnableAudio{
			set{;}
		}		
			
		public void AddSegment (string filePath, long start, long duration, double rate, string title)
		{
			segmentsList.Enqueue(new VideoSegment(filePath, start, duration, rate, title));
		}
		
		public void ClearList(){
			segmentsList.Clear();			
		}
		
		public void Start(){
			thread = new Thread(new ThreadStart(EncodeSegments));
			thread.Start();			
		}
		
		public void Cancel(){
			splitter.Cancel();
			if (thread != null)
				thread.Abort();
			segmentCoded = -1;
			if (Progress != null)
				Progress ((float)EditorState.CANCELED);
		}
		
		private void EncodeSegments(){
			int i = 1;
			string tempFile;
			
			segmentsTempFiles.Clear();
			foreach (VideoSegment segment in segmentsList){					
				segmentCoded = i;
				tempFile = System.IO.Path.Combine ( tempDir, "segment"+i+".mkv");
				segmentsTempFiles.Enqueue(tempFile);
				splitter.OutputFile= tempFile;
				splitter.SetSegment(segment.FilePath, segment.Start, segment.Duration, segment.Rate, segment.Title);
				splitter.Start();
				i++;
				while (segmentCoded != -1);
			}			
			MergeSegments();		
		}
		
		private void MergeSegments (){
			Process process = new Process();
			ProcessStartInfo pinfo = new ProcessStartInfo();
			if (System.Environment.OSVersion.Platform != PlatformID.Unix)
				pinfo.FileName=System.IO.Path.Combine(System.Environment.CurrentDirectory,"mkvmerge.exe");
			else 
				pinfo.FileName="mkvmerge";			
			pinfo.Arguments = CreateMkvMergeCommandLine();
			pinfo.CreateNoWindow = true;
			pinfo.UseShellExecute = false;
			process.StartInfo = pinfo;
			process.Start();
			process.WaitForExit();			
			this.DeleteTempFiles();
			Application.Invoke(delegate {Progress ((float)EditorState.FINISHED);});

		}
		
		private string CreateMkvMergeCommandLine(){
		 	int i=0;
			string appendTo="";
			string args = String.Format("-o {0}  --language 1:eng --track-name 1:Video --default-track 1:yes --display-dimensions 1:{1}x{2} ",
			                            outputFile, width, height);
			
			foreach (String path in segmentsTempFiles){
				if (i==0){
					args += String.Format ("-d 1 -A -S {0} ", path);
				}
				if (i==1){
					args += String.Format ("-d 1 -A -S +{0} ", path);
					appendTo += String.Format(" --append-to {0}:1:{1}:1",i,i-1);
				}
				else if (i>1){
					args += String.Format ("-d 1 -A -S +{0} ", path);
					appendTo += String.Format(",{0}:1:{1}:1",i,i-1);
				}				
				i++;
			}
			
			args += String.Format("--track-order 0:1 {0}", appendTo);
			
			Console.WriteLine(args);
			return args;
		}
		
		private void DeleteTempFiles(){
			foreach (String path in segmentsTempFiles)
				System.IO.File.Delete(path);
		}
		
		protected virtual void OnError (object o, ErrorArgs args){
			if (Progress != null)
				Application.Invoke(delegate {Progress ((float)EditorState.ERROR);});
		}
		
		protected virtual void OnProgress (object o, PercentCompletedArgs args){			
			float percent = args.Percent;	
			float totalPercent = percent/segmentsList.Count + (float)(segmentCoded-1)/segmentsList.Count;
			if (Progress != null && totalPercent != 1) //We have to wait to merge the segment before senden the FINISHED event
				Application.Invoke(delegate {Progress (totalPercent);});
			if (percent == 1){
				segmentCoded = -1;
			}
		}
	}
}
