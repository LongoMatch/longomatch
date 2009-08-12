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
using LongoMatch.Video;
using LongoMatch.Video.Player;
using Mono.Unix;
using Gdk;

namespace LongoMatch.Video.Utils
{
	
	
	public class PreviewMediaFile:MediaFile
	{
		
		private byte[] thumbnailBuf;
		
		const int THUMBNAIL_MAX_HEIGHT=72;
		const int THUMBNAIL_MAX_WIDTH=96;
		
		public PreviewMediaFile(){}
		
		public PreviewMediaFile(string filePath,
		                 long length,
		                 ushort fps,
		                 bool hasAudio, 
		                 bool hasVideo, 
		                 string videoCodec, 
		                 string audioCodec, 
		                 uint videoWidth, 
		                 uint videoHeight,
		                 Pixbuf preview):base (filePath,length,fps,hasAudio,hasVideo,videoCodec,audioCodec,videoWidth,videoHeight)
		{
			this.Preview=preview;
		}
		
		public Pixbuf Preview{
			get{ 
				if (thumbnailBuf != null)
					return new Pixbuf(thumbnailBuf);
				else return null;
			}
			set{
				if (value != null){
					int h = value.Height;
					int w = value.Width;
					double ratio = (double)w/(double)h;
					if (h>w)
						thumbnailBuf = value.ScaleSimple((int)(THUMBNAIL_MAX_HEIGHT*ratio),THUMBNAIL_MAX_HEIGHT,InterpType.Bilinear).SaveToBuffer("png");
					else
						thumbnailBuf = value.ScaleSimple(THUMBNAIL_MAX_WIDTH,(int)(THUMBNAIL_MAX_WIDTH/ratio),InterpType.Bilinear).SaveToBuffer("png");
				}
								
				else thumbnailBuf = null;
			}
		}
		
		public static PreviewMediaFile GetMediaFile(string filePath){
			int duration;			
			bool hasVideo;
			bool hasAudio;
			string audioCodec = "";
			string videoCodec = "";
			int fps=0;
			int height=0;
			int width=0;		
			Pixbuf preview=null;
			MultimediaFactory factory;
			IMetadataReader reader;
			IFramesCapturer thumbnailer;
			
			try{
				factory =  new MultimediaFactory();
				reader = factory.getMetadataReader();
				reader.Open(filePath);
				duration = (int)reader.GetMetadata(GstMetadataType.Duration);						
				hasVideo = (bool) reader.GetMetadata(GstMetadataType.HasVideo);
				hasAudio = (bool) reader.GetMetadata(GstMetadataType.HasAudio);
				if (hasAudio){
					audioCodec = (string) reader.GetMetadata(GstMetadataType.AudioCodec);					
				}
				if (hasVideo){
					videoCodec = (string) reader.GetMetadata(GstMetadataType.VideoCodec);	
					fps = (int) reader.GetMetadata(GstMetadataType.Fps);
					thumbnailer = factory.getFramesCapturer();
					thumbnailer.Open(filePath);
					thumbnailer.SeekTime(1000,false);
					preview = thumbnailer.CurrentFrame;
					thumbnailer.Dispose();
				}			
				height = (int) reader.GetMetadata(GstMetadataType.DimensionY);
				width = (int) reader.GetMetadata (GstMetadataType.DimensionX);
				reader.Close();	
				reader.Dispose();	
				
				return new PreviewMediaFile(filePath,duration*1000,(ushort)fps,hasAudio,hasVideo,videoCodec,audioCodec,(uint)height,(uint)width,preview);
			}
			catch (GLib.GException ex){
			    throw new Exception (Catalog.GetString("Invalid video file:")+"\n"+ex.Message);
			}
		}
	}
}
