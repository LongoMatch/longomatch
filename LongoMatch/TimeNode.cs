// TimeNode.cs
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
		/* TimeNode is the main object of the database for {@LongoMatch}. It' s used to
	       store the name of each reference point we want to remind with its start time
	       and its stop time, and the data type it belowns to. When we mark a moment in the
	       video, this object contains all the information we need to reproduce the
	       video sequence again.
		*/
		[Serializable]
		public class TimeNode
		{
		
		//Stores the name of the refenrence point
		private string name;

		//Stores the start time
		private long start;

		//Stores the stop time
		private long stop;

		//Stores the Data Section it belowns to, to allow its removal
		 private int dataSection;
		
		//Determines if it's a void node used in the Tree Viev with no time values
		 private bool isRoot;

		public TimeNode (String name,long start, long stop){
			this.name = name;
			this.start = start;
			this.stop = stop;
			this.isRoot = true;
			
		}
		
		public TimeNode(String name, long start, long stop, int dataSection) {
			this.start = start;
			this.stop = stop;
			this.name = name;
			this.dataSection = dataSection;

		}
		

		
		public bool IsRoot(){
		
			return isRoot;
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
		public long Start{
			get{
			return this.start;
			}
			
			set{ this.start=value;}
			
		}

		/**
		 * Returns a Time object representing the stop time of the video sequence
		 * 
		 * @returns Stop time
		 */
		public long Stop {
			get{
			return stop;
			}
			set{ this.stop=value;}
		}

		/**
		 * Returns a String object that represents the name of the reference point
		 * 
		 * @returns name Name of the reference point
		 */
		public string toString() {
			return name;
		}

		
		public int DataSection{
			get{
			return dataSection;
			}
		}

		public void changeStartStop(long start, long stop) {
			this.start = start;
			this.stop = stop;
		}

		 
		 /*Este método igual sobra, probar sin él
		public  bool equals(Object o) {
		
			if (o is TimeNode) {
				TimeNode tn = (TimeNode) o ;
				return (tn.name == name && tn.Start == start	&& tn.Stop == stop && tn.dataNumber == dataNumber);
				
				
				
			} else {
				return false;
			}
		}*/

		
	}
		
	}
