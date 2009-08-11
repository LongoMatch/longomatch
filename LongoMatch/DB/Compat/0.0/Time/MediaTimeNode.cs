// MediaTimeNode.cs
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
using Gdk;

namespace LongoMatch.DB.Compat.v00.TimeNodes
{
	public enum Team{
		NONE = 0,
		LOCAL = 1,
		VISITOR = 2,
	}
	
	/* MediaTimeNode is the main object of the database for {@LongoMatch}. It' s used to
	       store the name of each reference point we want to remind with its start time
	       and its stop time, and the data type it belowns to. When we mark a moment in the
	       video, this object contains all the information we need to reproduce the
	       video sequence again.
	 */
	[Serializable]
	public class MediaTimeNode : PixbufTimeNode
	{
		
		//Stores the Data Section it belowns to, to allow its removal
		private int dataSection;
		private Team team;
		private uint fps;
		
		private bool selected;
		
		private uint startFrame;
		
		private uint stopFrame;
		
		private string notes;

		
		
		public MediaTimeNode(String name, Time start, Time stop, uint fps, int dataSection,string miniaturePath):base (name,start,stop,miniaturePath) {
			this.dataSection = dataSection;		
			this.team = Team.NONE;
			this.fps = fps;
			if (stop <= start )
				this.Stop = start+500;
			else
				this.Stop = stop;
			this.startFrame = (uint) this.Start.MSeconds*fps/1000;
			this.stopFrame = (uint) this.Stop.MSeconds*fps/1000;
		}
		
		public MediaTimeNode(String name, Time start, Time stop,string notes, uint fps, int dataSection,string miniaturePath):base (name,start,stop,miniaturePath) {
			this.notes = notes;
			this.dataSection = dataSection;		
			this.team = Team.NONE;
			this.fps = fps;
			this.startFrame = (uint) this.Start.MSeconds*fps/1000;
			this.stopFrame = (uint) this.Stop.MSeconds*fps/1000;
		}
		
		public string Notes {
			get{return notes;}
			set{notes = value;}
		}
		public int DataSection{
			get{return dataSection;}
		}	
		
		public Team Team{
			get{return this.team;}
			set{this.team = value;}				
		}
		
		public uint Fps{
			get{return this.fps;}
			set{this.fps = value;}
		}
		
		public uint CentralFrame{
			get{ return this.StopFrame-((this.TotalFrames)/2);}
		}
		
		public uint TotalFrames{
			get{return this.StopFrame-this.StartFrame;}
		}
		
		public uint StartFrame {
			get {return startFrame;}			
			set { 
				this.startFrame = value;
				this.Start = new Time((int)(1000*value/fps));
			}
		}
		
		public uint StopFrame {			
			get {return stopFrame;}
			set { 
				this.stopFrame = value;
				this.Stop = new Time((int)(1000*value/fps));
			}
		}
	
		public bool HasFrame(int frame){
			return (frame>=startFrame && frame<stopFrame);
		}
		
		public bool Selected {
			get {return selected;}
			set{this.selected = value;}
			
		}
		
	}
		
}
