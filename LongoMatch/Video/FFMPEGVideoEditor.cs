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
using System.Diagnostics;


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

			//process = new Process();
		}
		
		public void Cancel(){}
	}
}
