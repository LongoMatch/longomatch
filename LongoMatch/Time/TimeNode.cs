// TimeNode.cs
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

namespace LongoMatch.TimeNodes
{
	
	[Serializable]
	public class TimeNode
	{
		//Stores the name of the refenrence point
		private string name;

		//Stores the start time
		private Time start;

		//Stores the stop time
		private Time stop;
		
		
		
		public TimeNode(){
		}
		
		public TimeNode(String name,Time start, Time stop)
		{
			this.name = name;
			this.start = start;
			this.stop = stop;				
		}
		
		/**
		 * Returns a String object that represents the name of the reference point
		 * 
		 * @returns name Name of the reference point
		 */
		public string Name {
			get{
			return this.name;
			}
			set{
			this.name=value;

			}
		}



		/**
		 * Returns a Time object representing the start time of the video sequence
		 * 
		 * @returns Start time
		 */
		public Time Start{
			get{
			return this.start;
			}
			
			set{ 
				this.start=value;
			}
			
		}

		/**
		 * Returns a Time object representing the stop time of the video sequence
		 * 
		 * @returns Stop time
		 */
		public Time Stop {
			get{
			return stop;
			}
			set{ 
				this.stop = value;
			}
		}
		
		public Time Duration {
			get {return Stop-Start;}
		}
		
		

		/**
		 * Returns a String object that represents the name of the reference point
		 * 
		 * @returns name Name of the reference point
		 */
		public string toString() {
			return name;
		}
		
		public void changeStartStop(Time start, Time stop) {
			this.start = start;
			this.stop = stop;
		}
		
		
		
	}
}
