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


namespace LongoMatch.Video
{
	
	
	public class FFMPEGVideoEditor : IVideoEditor
	{
		
		public event System.EventHandler EditionFinished;
		PlayList playlist;
		string outputFile;
		VideoQuality vq;
		AudioQuality aq;
		bool audioEnabled;
		Process process;

		
		public FFMPEGVideoEditor(PlayList playlist, string outputFile, VideoQuality vq, AudioQuality aq)
		{
			this.playlist = playlist;
			this.outputFile = outputFile;
			this.aq = aq;
			this.vq = vq;	
			this.process = new Process();
		}
		
		public PlayList PlayList {
			set{this.playlist = value;}
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
			string[] files = System.IO.Directory.GetFiles(MainClass.TempVideosDir());
			Thread thread = new Thread(new ParameterizedThreadStart(EncodeVideo));
			thread.Start(this.playlist);  
			
		}
		
		public void Cancel(){}
		
		private void MergeVideo(){
			string list="";
			string[] files = System.IO.Directory.GetFiles(MainClass.TempVideosDir());
			foreach (String file in files)
				list = list + file +" ";
			ProcessStartInfo pinfo = new ProcessStartInfo();
			pinfo.FileName="mencoder";
			pinfo.Arguments = "-oac copy -ovc copy " + list +" -o " + System.IO.Path.Combine (MainClass.VideosDir(),this.OutputFile);
			process.StartInfo = pinfo;
			process.Start();
			process.WaitForExit();			
		}
		
		
		private void EncodeVideo(object o){
			int i= 0;
			if (o is PlayList){
				PlayList playList = (PlayList)o;
				foreach (PlayListTimeNode plNode in playList){				
					
					ProcessStartInfo pinfo = new ProcessStartInfo();
					pinfo.FileName="ffmpeg";
					pinfo.Arguments = "-i '" + plNode.FileName + "' -f avi -y -ss " + plNode.Start.ToMSecondsString() 
						+ " -t " +plNode.Duration.ToMSecondsString() + " -vcodec  copy -acodec copy "
						+ System.IO.Path.Combine (MainClass.TempVideosDir(),"temp"+i);	
					Console.WriteLine(pinfo.Arguments);
					process.StartInfo = pinfo;
					process.Start();
					process.WaitForExit();
					// HACK to rebuild the index of the splitted video.
					pinfo = new ProcessStartInfo();
					pinfo.FileName="ffmpeg";
					pinfo.Arguments = "-i '" + System.IO.Path.Combine (MainClass.TempVideosDir(),"temp"+i) 
						+ "' -vcodec  copy -acodec copy "
						+ System.IO.Path.Combine (MainClass.TempVideosDir(),"temp"+i+".avi");					
					process.StartInfo = pinfo;
					process.Start();
					process.WaitForExit();
					
					System.IO.File.Delete(System.IO.Path.Combine (MainClass.TempVideosDir(),"temp"+i));					
					i++;
				}
				Thread thread = new Thread(new ThreadStart(MergeVideo));
				thread.Start(); 
			}
		}
	}
}
