// Sections.cs
//
//  Copyright (C) 2007 [name of author]
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
	
	
	public class Sections
	{
		private TimeNode[] timeNodesArray;
		private int visibleSections;
		private int totalSections;
		
		
		public Sections(int sections)
		{
			this.timeNodesArray = new TimeNode[sections];
			this.totalSections = sections;
			this.visibleSections = sections;
			
		}

		public int VisibleSections {
			
			set{
				if (value > this.totalSections ||value < 0 )
					return;
				else
					this.visibleSections = value;
			}
			get{
				return this.visibleSections;
			}
					
		}
		
		
		public void SetTimeNodes(string[] names, int[] startTimes, int[] stopTimes){
			for (int i=0;i<20;i++){
				timeNodesArray[i] = new TimeNode(names[i],startTimes[i],stopTimes[i]);
			}
		}
		
		public void SetTimeNodes(TimeNode[] timeNodesArray){
			this.timeNodesArray = timeNodesArray;
		}
		
		public string[] GetSectionsNames(){
			string[] names = new string[totalSections];
			TimeNode tNode;
			for (int i=0; i<totalSections; i++){
				tNode = timeNodesArray[i];
				names[i]=tNode.Name;
			}
			return names;
		
		}
		
		public int[] GetSectionsStartTimes(){
			int[] startTimes = new int[totalSections];
			TimeNode tNode;
			for (int i=0; i<totalSections; i++){
				tNode = timeNodesArray[i];
				startTimes[i]=(int)tNode.Start;
			}
			return startTimes;
		}
		
		public int[] GetSectionsStopTimes(){
			int[] stopTimes = new int[totalSections];
			TimeNode tNode;
			for (int i=0; i<totalSections; i++){
				tNode = timeNodesArray[i];
				stopTimes[i]=(int)tNode.Start;
			}
			return stopTimes;
			
		}
		
		public TimeNode GetTimeNode (int section){
			return timeNodesArray[section];
		}
		
		public string GetName ( int section){
			return timeNodesArray[section].Name;
		}
		
		public int GetStartTime ( int section){
			return (int)(this.timeNodesArray[section].Start);
		}
		
		public int GetStopTime ( int section){
			return (int)(this.timeNodesArray[section].Stop);
		}
	}
}
