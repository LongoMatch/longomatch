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
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using Gtk;



namespace LongoMatch.Video.Editor
{
	
	public class MencoderVideoEditor : IVideoEditor
	{
		
		public event LongoMatch.Video.Handlers.ProgressHandler Progress;
		
		private List<VideoSegment> encodeList;
		private string outputFile;
		private VideoQuality vq;
		private AudioQuality aq;
		private bool audioEnabled;
		private int steps;
		private Thread thread;
		private Thread mthread;
		private Process process;
		private string list;
		private string tempDir;

		#region Constructors
		public MencoderVideoEditor(List<VideoSegment> encodeList, string outputFile)
		{
			this.encodeList = encodeList;	
			this.outputFile = outputFile;
			this.aq = AudioQuality.Normal;
			this.vq = VideoQuality.Normal;	
			
		}
		
		public MencoderVideoEditor(){
			this.aq = AudioQuality.Normal;
			this.vq = VideoQuality.Normal;	
			
		}
		#endregion
		#region Private methods
		private void DeleteTempFiles(){
			string[] files = System.IO.Directory.GetFiles(MainClass.TempVideosDir());
			foreach (string f in files)
				System.IO.File.Delete(f);
		}
		
		private void KillProcess(){
			
			if (this.thread != null && this.thread.IsAlive){
				this.thread.Abort();				
			}
			
			if (this.mthread != null && this.mthread.IsAlive){
				this.mthread.Abort();				
			}
			
			if (this.process != null  && !this.process.HasExited ){
				this.process.Kill();
				this.process.WaitForExit();
				this.process.Dispose();
			}
		}
		
		private void EncodeVideo(object o){
			encodeList = (List<VideoSegment>)o;
			int i = 0;
            list = "";
			
			ArrayList parameters = new ArrayList();			
			
			//Split video segments
			foreach (VideoSegment segment in encodeList){
				if (plNode.Valid){
					string outputFile = System.IO.Path.Combine (MainClass.TempVideosDir(),"temp"+i+".avi");
					list  = list +"\"" + outputFile +"\" ";
					mthread = new Thread(new ParameterizedThreadStart(SplitVideo));
					parameters.Insert(0,segment);
					parameters.Insert(1,outputFile);
					mthread.Start(parameters);
					mthread.Join();				
							
					if (this.Progress != null)
						Application.Invoke(delegate {this.Progress ( ((float)i+1)*2/steps);});
						i++;
				}
				else {
					if (this.Progress != null)
						Application.Invoke(delegate {this.Progress ( ((float)i+1)*2/steps);});
						i++;
					}
				}
			    //Merge video segments
				mthread = new Thread(new ParameterizedThreadStart(MergeVideo));
				mthread.Start(list);  
				mthread.Join();
				if (this.Progress != null)
						Application.Invoke(delegate {this.Progress (1);});		
		}
		
		
		private void MergeVideo(object o){
			string list = (string) o;
				
			ProcessStartInfo pinfo = new ProcessStartInfo();
			if (System.Environment.OSVersion.Platform != PlatformID.Unix)
				pinfo.FileName=MainClass.RelativeToPrefix("bin\\mencoder.exe");
			else 
				pinfo.FileName="mencoder";			
			pinfo.Arguments = "  -nosound -ovc  copy "  + list +" -o \"" + System.IO.Path.Combine (MainClass.VideosDir(),this.OutputFile)+"\"";
			pinfo.CreateNoWindow = true;
			pinfo.UseShellExecute = false;
			process.StartInfo = pinfo;
			process.Start();
			process.WaitForExit();			
			this.DeleteTempFiles();
			
		}	
		
		
		private void SplitVideo(object o){
			VideoSegment segment =(VideoSegment)((ArrayList)o)[0]; 
			string outputFile = (string)((ArrayList)o)[1];
			string tempFile = System.IO.Path.Combine (MainClass.TempVideosDir(),
			                                          System.IO.Path.GetFileNameWithoutExtension(outputFile));
			
			
			ProcessStartInfo pinfo = new ProcessStartInfo();
			
			pinfo.FileName=MainClass.RelativeToPrefix("bin\\mencoder.exe");
			pinfo.Arguments = "\""+segment.FilePath+"\" -nosound -vf scale=720:576 -ovc  lavc -lavcopts autoaspect:vcodec=libx264:vbitrate="
					+ (int)this.VideoQuality+"  -ss "+ segment.Start 
					+ " -endpos "+ segment.Duration +" -o \""
					+ outputFile +"\"";				
			
			pinfo.CreateNoWindow = true;
			pinfo.UseShellExecute = false;
			process.StartInfo = pinfo;	
			process.Start();
			process.WaitForExit();		
		}
		
		#endregion
		
		#region Properties

		
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
		
		public string TempDir{
			set{tempDir = value;}
			get{return TempDir;}
			
		}
		
		public bool EnableTitle{
			set{;}
		}
		
		public bool EnableAudio{
			set{this.audioEnabled = value;}
			get{return this.audioEnabled;}
		}
		#endregion 
		#region Public methods		
		
		public void Start(){	
			//only one process at the same time
			if (this.Progress != null)
						this.Progress (0);
			if (this.thread == null || !this.thread.IsAlive ){				
				process = new Process();
				thread = new Thread(new ParameterizedThreadStart(EncodeVideo));
				thread.Start(this.encodeList);  
			}
		}
		
		public void Cancel(){				
			this.KillProcess();
			this.DeleteTempFiles();
			if (this.Progress != null)
						this.Progress (-1);			
		}
		
      	#endregion
		
		~MencoderVideoEditor ()
		{
			this.KillProcess();
		}

	}
}
