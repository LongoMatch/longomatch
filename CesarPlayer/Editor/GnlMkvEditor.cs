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
using Gtk;

namespace LongoMatch.Video.Editor
{
	
	
	public class GnlMkvEditor : IVideoEditor
	{
		public event LongoMatch.Video.Handlers.ProgressHandler Progress;	
		
		private GstVideoSplitter splitter;
		private Queue<VideoSegment> segmentsList;
		private string tempDir;
		private int segmentCoded;
		private Thread thread;
		
		public GnlMkvEditor()
		{			
			splitter = new GstVideoSplitter();
			splitter.PercentCompleted += new PercentCompletedHandler(OnProgress);
			segmentsList = new Queue<VideoSegment>();	
			tempDir = System.IO.Path.GetTempPath();
			segmentCoded = -1;
		}		
		
		public VideoQuality VideoQuality{
			set { splitter.VideoBitrate = (int)value;}
		}
		
		public AudioQuality AudioQuality{
			set{ splitter.VideoBitrate = (int)value;}
		}
		
		public int Height{
			set{ splitter.Height = value;}
			get{ return splitter.Height;}
		}
		
		public int Width{
			set{ splitter.Width = value;}
			get{ return splitter.Width;}
		}
		
		public string OutputFile{
			set{ splitter.OutputFile = value;}
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
			thread.Abort();
			segmentCoded = -1;
			if (Progress != null)
				Progress (-1);
		}
		
		private void EncodeSegments(){
			int i = 1;
			foreach (VideoSegment segment in segmentsList){
				while (segmentCoded != -1);				
				segmentCoded = i;
				Console.WriteLine("Encoding segment "+segmentCoded);
				splitter.OutputFile= System.IO.Path.Combine ( tempDir, "segment"+i+".mkv");
				splitter.SetSegment(segment.FilePath, segment.Start, segment.Duration, segment.Rate, segment.Title);
				splitter.Start();
				i++;
			}
		}
		
		protected virtual void OnProgress (object o, PercentCompletedArgs args){			
			float percent = args.Percent;			
			if (Progress != null)
				Application.Invoke(delegate {Progress (percent*segmentCoded/segmentsList.Count);});
			if (percent == 1){
				segmentCoded = -1;
				if (segmentCoded == segmentsList.Count);
					Application.Invoke(delegate {Progress (-1);});
			}
		}
	}
}
