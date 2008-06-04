// PlayListNode.cs 
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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

namespace LongoMatch
{
	
	
	public class PlayListNode
	{
		private string fileName;
		private string name;
		private Time startTime;
		private Time stopTime;
		
		public PlayListNode(string fileName, string name, Time startTime, Time stopTime)
		{
			this.fileName = fileName;
			this.name = name;
			this.stopTime = stopTime;
			this.startTime = startTime;
		}
		
		public PlayListNode(string fileName, TimeNode tNode){
			this.fileName = fileName;
			this.name = tNode.Name;
			this.stopTime = tNode.Stop;
			this.startTime = tNode.Start;
		}
		public string FileName{
			set{ this.fileName = value;}
			get{ return this.fileName;}
		}
		
		public string Name{
			set{ this.name = value;}
			get{ return this.name;}
		}
		
		public Time StartTime{
			set{ this.startTime = value;}
			get{ return this.startTime;}
		}
		
		public Time StopTime{
			set{ this.stopTime = value;}
			get{ return this.stopTime;}
		}
		
	}
}
