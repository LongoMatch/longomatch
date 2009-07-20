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
using LongoMatch.Video.Handlers;

namespace LongoMatch.Video.Editor
{
	
	
	public class GnonlinEditor : IVideoEditor
	{
		public event ProgressHandler Progress;	
		public event ErrorHandler Error;
		
		private IVideoSplitter splitter;
		private IMerger merger;
		private List<VideoSegment> segmentsList;
		private List<string> segmentsTempFiles;
		
		private int height;
		private int width;
		private string outputFile;
		private string tempDir;
		
		private int segmentCoded;
		private bool readyToMerge;
		private Thread thread;
		
		private VideoCodec vcodec; //Used to handle theora files
		
		private MultimediaFactory factory;
	
		
		public GnonlinEditor()
		{		
			factory = new MultimediaFactory();
			ChangeMerger(VideoMuxer.MKV);
			splitter = new GstVideoSplitter();
			splitter.PercentCompleted += new PercentCompletedHandler(OnProgress); 
			splitter.Error += new ErrorHandler(OnError);
			segmentsList = new List<VideoSegment>();	
			segmentsTempFiles = new List<string>();
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
					height = 576;
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
		
		public AudioCodec AudioCodec{
			set{
				string error;
				splitter.SetAudioEncoder(out error,value);
				if (error != null)
					throw new Exception(error);
			}
		}
		
		public VideoCodec VideoCodec{
			set{
				string error;
				vcodec = value;
				splitter.SetVideoEncoder(out error, value);
				if (error != null)
					throw new Exception(error);
			}
		}
		
		public VideoMuxer VideoMuxer{
			set{
				string error;
				splitter.SetVideoMuxer(out error,value);
				if (error != null)
					throw new Exception(error);
				ChangeMerger(value);
			}
		}				
				
		public string OutputFile{
			set{ 
				outputFile = value;
				merger.OutputFile = value;}
		}
		
		public string TempDir{
			set{tempDir = value;}
		}
		
		public bool EnableTitle{
			set{splitter.EnableTitle = value;}
		}
		
		public bool EnableAudio{
			set{;}
		}		
			
		public void AddSegment (string filePath, long start, long duration, double rate, string title)
		{
			segmentsList.Add(new VideoSegment(filePath, start, duration, rate, title));
		}
		
		public void ClearList(){
			segmentsList.Clear();			
		}
		
		public void Start(){
			thread = new Thread(new ThreadStart(SplitAndMerge));
			thread.Start();			
		}
		
		public void Cancel(){
			splitter.Cancel();
			if (thread != null)
				thread.Abort();
			segmentCoded = -1;
			if (Progress != null)
				Progress ((float)EditorState.CANCELED);
			DeleteTempFiles();
		}
		
		private void ChangeMerger(VideoMuxer videoMuxer){
			merger = factory.GetVideoMerger(videoMuxer);
			merger.OutputMuxer = videoMuxer;
			merger.MergeDone += new EventHandler(OnMergeDone);
			merger.Error += new ErrorHandler(OnError);
		}		
		
		private void SplitAndMerge(){
			SplitSegments();
			merger.FilesToMerge = segmentsTempFiles;
			merger.Start();
		}
		
		private void SplitSegments(){
			int i = 1;
			string tempFile;
			string error;
			
			segmentsTempFiles.Clear();
			foreach (VideoSegment segment in segmentsList){					
				segmentCoded = i;
				tempFile = System.IO.Path.Combine ( tempDir, "segment"+i+".mkv");
				segmentsTempFiles.Add(tempFile);
				//When using the theora encoder, the splitted files must be muxed using ogg
				//and then merged using matroska as ogg concatenation does not works and
				//muxing theora encoded streams using matroska doens't work neither
				if (vcodec == VideoCodec.THEORA){
				    splitter.SetVideoMuxer(out error,VideoMuxer.OGG);
					if (error != null)
						throw new Exception (error);
				}
				
				splitter.OutputFile= tempFile;
				splitter.SetSegment(segment.FilePath, segment.Start, segment.Duration, segment.Rate, segment.Title);
				splitter.Start();
				i++;
				while (segmentCoded != -1);
			}				
		}

		private void SendErrorEvent(string error){
			if (Error != null){
					ErrorArgs args = new ErrorArgs ();
					args.Args = new object[1];
					args.Args[0] = error;
					Error (this, args);
			}
		}
		
		private void DeleteTempFiles(){
			foreach (String path in segmentsTempFiles){
				if (System.IO.File.Exists(path)){
					try{
						System.IO.File.Delete(path);}
					catch (Exception e){}
				}
			}
		}
		
		protected virtual void OnError (object o, ErrorArgs args){
			if (Error != null)
				Application.Invoke(delegate {Error (o,args);});			
		}
		
		protected virtual void OnProgress (object o, PercentCompletedArgs args){			
			float percent = args.Percent;	
			float totalPercent = percent/segmentsList.Count + (float)(segmentCoded-1)/segmentsList.Count;
			if (Progress != null && totalPercent != 1) //We have to wait to merge the segment before sending the FINISHED event
				Application.Invoke(delegate {Progress (totalPercent);});
			if (percent == 1){
				segmentCoded = -1;
			}
		}
		
		protected virtual void OnMergeDone(object sender, EventArgs args){
			if (Progress != null)
				Application.Invoke(delegate {Progress ((float)EditorState.FINISHED);});
		}
	}
}
