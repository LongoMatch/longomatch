// TimeNode.cs
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
using System.Collections.Generic;
using LongoMatch.TimeNodes;

namespace LongoMatch.TimeNodes
{
	
	[Serializable]
	public class TimeNode
	{
		//Stores the name of the play
		private string name;

		//Stores the start time
		private Time start;

		//Stores the stop time
		private Time stop;
		
		#region Constructors
		public TimeNode(){
		}
		
		public TimeNode(String name,Time start, Time stop)
		{
			this.name = name;
			this.start = start;
			this.stop = stop;				
		}
		#endregion
		
		#region Properties
		/**
		 * Set/Get the name 
		 * 
		 * @returns name Name of the reference point
		 */
		public string Name {
			get{return this.name;}
			set{this.name=value;}
		}

		/**
		 * Set/Get the start {@Time}
		 * 
		 * @returns Start time
		 */
		public Time Start{
			get{return this.start;}			
			set{ this.start=value;}			
		}

		/**
		 * Set/Get the stop 
		 * 
		 * @returns Stop {@Time}
		 */
		public Time Stop {
			get{
			return stop;
			}
			set{ 
				this.stop = value;
			}
		}
		
		/**
		 * Get the duration defined like start {@Time} - stop {@Time} 
		 * 
		 * @returns Stop {@Time}
		 */		
		public Time Duration {
			get {return Stop-Start;}
		}		
		#endregion	

		#region Public methods
		/**
		 * Change the Start and Stop {@Time} 
		 * 
		 * @returns name Name of the reference point
		 */		
		public void ChangeStartStop(Time start, Time stop) {
			this.start = start;
			this.stop = stop;
		}
		#endregion	
	}
}
