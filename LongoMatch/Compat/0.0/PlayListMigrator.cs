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
using LongoMatch.Playlist;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Utils;
using Gtk;

namespace LongoMatch.Compat
{
	
	
	public class PlayListMigrator
	{
		private string[]  oldPLFiles;
		
		public event ConversionProgressHandler ConversionProgressEvent;
		
		public const string DONE="Playlists files imported successfully";
		
		public const string ERROR="Error importing playlists";		
		
		private Thread  thread;		
			
		
		public PlayListMigrator(string[]  oldPLFiles)
		{
			this.oldPLFiles=  oldPLFiles;
		}
		
		public void Start(){
			thread = new Thread(new ThreadStart(StartConversion));
			thread.Start();
		}
		
		public void Cancel(){
			if (thread != null && thread.IsAlive)
				thread.Abort();
		}
		
		public void StartConversion(){
			foreach (string plFile in oldPLFiles){
				v00.PlayList.PlayList oldPL = null;
				PlayList newPL;
				LongoMatch.Video.Utils.MediaFile file;
								
				SendEvent(String.Format("Converting file {0}",plFile));
				try{
					oldPL = new LongoMatch.Compat.v00.PlayList.PlayList(plFile);
				}catch{
					SendEvent(String.Format("File {0} is not a valid playlist",plFile));
				}
				if (System.IO.File.Exists(plFile+".old")){
					SendEvent(String.Format("File {0} has already been converted",plFile));
					oldPL = null;
				}
				
				if (oldPL != null){
					System.IO.File.Copy(plFile,plFile+".old",true);	
					System.IO.File.Delete(plFile);
					newPL= new PlayList(plFile);				
					
					while (oldPL.HasNext()){					
						v00.TimeNodes.PlayListTimeNode oldPLNode = oldPL.Next();
						PlayListTimeNode newPLNode = new PlayListTimeNode();
					
						SendEvent(String.Format("Add element {0} to playlist {1}",oldPLNode.Name,plFile));
						
						newPLNode.Name = oldPLNode.Name;
						newPLNode.Start = new Time(oldPLNode.Start.MSeconds);
						newPLNode.Stop = new Time (oldPLNode.Stop.MSeconds);
						newPLNode.Rate = 1;
						newPLNode.Valid = true;
						
						try{
							file = LongoMatch.Video.Utils.MediaFile.GetMediaFile(oldPLNode.FileName);
						}
						catch{
							file = new LongoMatch.Video.Utils.MediaFile();
							file.FilePath = oldPLNode.FileName;
							file.Fps = 25;
							file.HasAudio = false;
							file.HasVideo =  true;
							file.Length = 0;
							file.VideoHeight = 576;
							file.VideoWidth = 720;				
							file.AudioCodec = "";
							file.VideoCodec = "";
						}
						newPLNode.MediaFile = file;
						newPL.Add(newPLNode);
					}				          
					newPL.Save();										
				}
				SendEvent(DONE);
			}
		}
		
		public void SendEvent (string message){
			Console.WriteLine(message);
			if (ConversionProgressEvent != null)					
						Application.Invoke(delegate {ConversionProgressEvent(message);});
		}
	}
}
