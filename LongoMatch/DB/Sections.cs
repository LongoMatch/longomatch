// Sections.cs
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
using System.Collections.Generic;
using Gdk;
using LongoMatch.TimeNodes;

namespace LongoMatch.DB
{
	
	
	public class Sections
	{
		private List<SectionsTimeNode> sectionsList;
		
		//These fields are not used but must be kept for DataBase compatiblity
		private Color[] colorsArray;
		private SectionsTimeNode[] timeNodesArray;
		
		public Sections()
		{
			this.sectionsList = new List<SectionsTimeNode>();			
		}
		
		public void AddSection(SectionsTimeNode tn){
			sectionsList.Add(tn);
		}	
		
		public void AddSectionAtPos(SectionsTimeNode tn, int index){
			sectionsList.Insert(index,tn);
		}		
		
		public void RemoveSection(int index){
			sectionsList.RemoveAt(index);
		}
		
		public int Count{
			get{return sectionsList.Count;}
		}
		
		public List<SectionsTimeNode> SectionsTimeNodes{
			set{
				sectionsList.Clear();
				sectionsList = value;
			}			
			get{return sectionsList;}
		}
		
		public SectionsTimeNode GetSection(int section){
			return sectionsList[section];
		}
		
		public string[] GetSectionsNames(){
			int count = sectionsList.Count;
			string[] names = new string[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++){
				tNode = sectionsList[i];
				names[i]=tNode.Name;
			}
			return names;		
		}
		
		public Color[] GetColors(){
			int count = sectionsList.Count;
			Color[] colors = new Color[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++){
				tNode = sectionsList[i];
				colors[i]=tNode.Color;
			}
			return colors;
		}
		
		public HotKey[] GetHotKeys(){
			int count = sectionsList.Count;
			HotKey[] hotkeys = new HotKey[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++){
				tNode = sectionsList[i];
				hotkeys[i]=tNode.HotKey;
			}
			return hotkeys;
		}
		
		public Time[] GetSectionsStartTimes(){
			int count = sectionsList.Count;
			Time[] startTimes = new Time[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++){
				tNode = sectionsList[i];
				startTimes[i]=tNode.Start;
			}
			return startTimes;
		}
		
		public Time[] GetSectionsStopTimes(){
			int count = sectionsList.Count;
			Time[] stopTimes = new Time[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++){
				tNode = sectionsList[i];
				stopTimes[i]=tNode.Start;
			}
			return stopTimes;			
		}			
		
		public SectionsTimeNode GetTimeNode (int section){
			return sectionsList [section];
		}
		
		public string GetName ( int section){
			return sectionsList[section].Name;
		}
		
		public Time GetStartTime ( int section){
			return sectionsList[section].Start;
		}
		
		public Time GetStopTime ( int section){
			return sectionsList[section].Stop;
		}
		
		public Color GetColor (int section){
			return sectionsList[section].Color;
		}
		
		public HotKey GetHotKey (int section){
			return sectionsList[section].HotKey;
		}
	}
}
