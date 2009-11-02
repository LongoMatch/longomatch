// PlayListTimeNode.cs 
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using Gdk;
using LongoMatch.Video.Utils;

namespace LongoMatch.TimeNodes
{
	
	[Serializable]
	public class PlayListTimeNode : TimeNode
	{
		private MediaFile mediaFile;
		private float rate=1;
		private bool valid=true; //True if the file exists

		#region Constructors
		public PlayListTimeNode(){
		}
		
		public PlayListTimeNode(MediaFile mediaFile, MediaTimeNode tNode) : base(tNode.Name,tNode.Start,tNode.Stop)
		{
			MediaFile = mediaFile;
			
		}
		#endregion
		#region  Properties
		
		public MediaFile MediaFile{
			set{
				//PlayListTimeNode is serializable and only primary classes
				//can be serialiazed. In case it's a PreviewMidaFile we create
				//a new MediaFile object.
				if (value is PreviewMediaFile){
					MediaFile mf  = new MediaFile(value.FilePath,value.Length,value.Fps,
					                          value.HasAudio,value.HasVideo,value.VideoCodec,
					                          value.AudioCodec,value.VideoWidth,value.VideoHeight);
					this.mediaFile= mf;
				}
				else this.mediaFile = value;
			}
			get{ return this.mediaFile;}
		}
		
		public float Rate{
			set{ this.rate = value;}
			get{ return this.rate;}
		}
		

		public bool Valid{
			get{return this.valid;}
			set{this.valid = value;}
		}
		#endregion		
	}
}
