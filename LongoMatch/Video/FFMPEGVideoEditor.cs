// FFMPEGVideoEditor.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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
using System.Threading;
using System.Diagnostics;
using LongoMatch.TimeNodes;


namespace LongoMatch.Video.Editor
{
	
	
	public class FFMPEGVideoEditor : IVideoEditor
	{
		
		public event LongoMatch.Handlers.ProgressHandler Progress;
		
		private PlayList playlist;
		private string outputFile;
		private VideoQuality vq;
		private AudioQuality aq;
		private bool audioEnabled;
		private int steps;
		private Thread thread;
		private Process process;

		
		public FFMPEGVideoEditor(PlayList playlist, string outputFile)
		{
			this.PlayList = playlist;
			this.outputFile = outputFile;
			this.aq = AudioQuality.copy;
			this.vq = VideoQuality.copy;	
			
		}
		
		public FFMPEGVideoEditor(){
			this.aq = AudioQuality.copy;
			this.vq = VideoQuality.copy;	
			
		}
		
		public PlayList PlayList {
			set{
				this.playlist = value;
				this.steps = 2*this.playlist.Count + 1;
			}
			get{return this.playlist;}
		}
		
		public VideoQuality VideoQuality{
			set{this.vq = value;}
			get{return this.vq;}
		}
		
		public AudioQuality AudioQuality{
			set{this.aq = value;}
			get{return this.aq;}
		}
		
		public string OutputFile{
			set{this.outputFile = value;}
			get{return this.outputFile;}
		}
		
		public bool EnableAudio{
			set{this.audioEnabled = value;}
			get{return this.audioEnabled;}
		}
		
		public void Start(){	
			//only one process at the same time
			
			if (this.thread == null || !this.thread.IsAlive ){
				if (this.Progress != null)
						this.Progress (0);
				string[] files = System.IO.Directory.GetFiles(MainClass.TempVideosDir());
			    thread = new Thread(new ParameterizedThreadStart(EncodeVideo));
				thread.Start(this.playlist);  
			}			
		}
		
		public void Cancel(){	
			
			this.KillProcess();
			this.DeleteTempFiles();
			// -1 means we have cancelled the encoding
			if (this.Progress != null)
						this.Progress (-1);
			
		}
		
		private void DeleteTempFiles(){
			string[] files = System.IO.Directory.GetFiles(MainClass.TempVideosDir());
			foreach (string f in files)
				System.IO.File.Delete(f);
		}
		
		private void KillProcess(){
			if (this.thread != null && this.thread.IsAlive){
				this.thread.Abort();				
			}
			if (this.process != null  && !this.process.HasExited ){
				this.process.Kill();
				this.process.WaitForExit();
				this.process.Dispose();
			}
		}
		
		private void MergeVideo(){
			string svq;
			string saq;
			string list="";
			string[] files = System.IO.Directory.GetFiles(MainClass.TempVideosDir());
			foreach (String file in files)
				list = list +"\"" + file +"\" ";
			ProcessStartInfo pinfo = new ProcessStartInfo();
			if (System.Environment.OSVersion.Platform != PlatformID.Unix)
				pinfo.FileName=System.IO.Path.Combine (System.AppDomain.CurrentDomain.BaseDirectory,"mencoder.exe");
			else 
				pinfo.FileName="mencoder";
			
			
		    if (this.vq == VideoQuality.copy)
				svq = "copy";
			else
				svq = ((int)this.vq).ToString();
			
			if (this.aq == AudioQuality.copy)
				saq = "copy";
			else
				saq = ((int) this.aq).ToString(); 
			
			pinfo.Arguments = "-oac " + saq+ " -ovc "+ svq + " " + list +" -o \"" + System.IO.Path.Combine (MainClass.VideosDir(),this.OutputFile)+"\"";
			pinfo.CreateNoWindow = true;
			pinfo.RedirectStandardOutput = true;
			process.StartInfo = pinfo;
			Console.WriteLine(pinfo.Arguments);
			process.Start();
			process.WaitForExit();			
			this.DeleteTempFiles();
			if (this.Progress != null)
						this.Progress (1);
			
		}
		
		
		private void EncodeVideo(object o){
			int i= 0;
			if (o is PlayList){
				PlayList playList = (PlayList)o;
				process = new Process();
				foreach (PlayListTimeNode plNode in playList){				
					
					this.SplitVideo(plNode,i);
					if (this.Progress != null)
						this.Progress ( (((float)i+1)*2-1)/steps);
					// HACK to rebuild the index of the splitted video.				
					this.FixSplitedVideo(i);					
					if (this.Progress != null)
						this.Progress ( ((float)i+1)*2/steps);
					i++;
				}
				Thread thread = new Thread(new ThreadStart(MergeVideo));
				thread.Start(); 
			}
		}
		
		private void SplitVideo(PlayListTimeNode plNode,int i){
			
			ProcessStartInfo pinfo = new ProcessStartInfo();
			if (System.Environment.OSVersion.Platform != PlatformID.Unix)
				pinfo.FileName=System.IO.Path.Combine (System.AppDomain.CurrentDomain.BaseDirectory,"ffmpeg.exe");
			else 
				pinfo.FileName="ffmpeg";
			pinfo.Arguments = "-i \"" + plNode.FileName + "\" -f avi -y -ss " + plNode.Start.ToMSecondsString() 
				+ " -t " +plNode.Duration.ToMSecondsString() + " -vcodec  copy -acodec copy \""
					+ System.IO.Path.Combine (MainClass.TempVideosDir(),"temp"+i)+"\"";	
			Console.WriteLine(pinfo.Arguments);		
			pinfo.CreateNoWindow = true;
			pinfo.RedirectStandardOutput = true;
			process.StartInfo = pinfo;
	
			process.Start();
			process.WaitForExit();			
		}
		
		private void FixSplitedVideo(int i){
			
			ProcessStartInfo pinfo = new ProcessStartInfo();
			if (System.Environment.OSVersion.Platform != PlatformID.Unix)
				pinfo.FileName=System.IO.Path.Combine (System.AppDomain.CurrentDomain.BaseDirectory,"ffmpeg.exe");
			else 
				pinfo.FileName="ffmpeg";
			pinfo.Arguments = "-i \"" + System.IO.Path.Combine (MainClass.TempVideosDir(),"temp"+i) 
				+ "\" -vcodec  copy -acodec copy -y \""
					+ System.IO.Path.Combine (MainClass.TempVideosDir(),"temp"+i+".avi")+"\"";		
			pinfo.CreateNoWindow = true;
			pinfo.RedirectStandardOutput = true;
			process.StartInfo = pinfo;
			process.Start();
			process.WaitForExit();			
			System.IO.File.Delete(System.IO.Path.Combine (MainClass.TempVideosDir(),"temp"+i));
			
		}
		
		~FFMPEGVideoEditor ()
		{
			Console.WriteLine("Finalizing");
			this.KillProcess();
		}

	}
}
