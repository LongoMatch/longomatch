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
using LongoMatch.Playlist;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Utils;

namespace LongoMatch.DB.Compat
{
	
	
	public class PlayListMigrator
	{
		private string  oldPLFile;
		
		
		public PlayListMigrator(string  oldPLFile)
		{
			this.oldPLFile =  oldPLFile;
		}
		
		public void StartConversion(){
			v00.PlayList oldPL;
			PlayList newPL;
			MediaFile file;
			
			oldPL = new LongoMatch.DB.Compat.v00.PlayList(oldPLFile);
			System.IO.File.Move(oldPLFile,oldPLFile+".old");			
			newPL= new PlayList(oldPLFile);
			
			while (oldPL.HasNext()){
				v00.PlayListTimeNode oldPLNode = oldPL.Next();
				PlayListTimeNode newPLNode = new PlayListTimeNode();
				
				newPLNode.Name = oldPLNode.Name;
				newPLNode.Start = new Time(oldPLNode.Start.MSeconds);
				newPLNode.Stop = new Time (oldPLNode.Stop.MSeconds);
				newPLNode.Rate = 1;
				newPLNode.Valid = true;
				
				try{
					file = MediaFile.GetMediaFile(oldPLNode.FileName);
				}
				catch{
					file = new MediaFile();
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
				newPL.Add(newPLNode);
			}				          
			newPL.Save();	  
		}
	}
}
