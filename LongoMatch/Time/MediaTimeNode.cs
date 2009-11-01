// MediaTimeNode.cs
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
using Gdk;

namespace LongoMatch.TimeNodes
{
	public enum Team{
		NONE = 0,
		LOCAL = 1,
		VISITOR = 2,
	}
	
	/* Plays are represented and stored in the database using {@MediaTimeNode} objects. 
	       It stores the name and the start and stop {@Time} of a play and
	       it's used to replay the video segment of the play. A play can have
	       tagged several players storing the {@Player} number in a list.
	 */
	[Serializable]
	public class  MediaTimeNode : PixbufTimeNode
	{
		
		
		private Team team;
		
		private uint fps;
		
		private bool selected;
		
		private uint startFrame;
		
		private uint stopFrame;
		
		private string notes;
		
		private List<int> localPlayersList; //Used for multitagging: one play and several players
											// We use the int index of the player in the template,		
		private List<int> visitorPlayersList;// because it's the only unmutable variable
		
		private Drawing keyFrame;
		

		
		#region Constructors	
		public MediaTimeNode(String name, Time start, Time stop,string notes, uint fps,Pixbuf thumbnail):base (name,start,stop,thumbnail) {
			this.notes = notes;
			this.team = Team.NONE;
			this.fps = fps;
			this.startFrame = (uint) this.Start.MSeconds*fps/1000;
			this.stopFrame = (uint) this.Stop.MSeconds*fps/1000;
			localPlayersList = new List<int>();
			visitorPlayersList = new List<int>();
		}
		#endregion
		
		#region Properties 		
		/**
		 * Get/Set the notes of a play  
		 * 
		 * @returns Stop {@Time}
		 */	
		public string Notes {
			get{return notes;}
			set{notes = value;}
		}
		
		/**
		 * Get/Set the {@Team}  
		 * 
		 * @returns Stop {@Time}
		 */				
		public Team Team{
			get{return this.team;}
			set{this.team = value;}				
		}
		
		/**
		 * Get/Set the frames per second. This value is taken from the 
		 * video file properties and used to translate from seconds
		 * to frames, for instance second 100 is equivalent to frame
		 * 100*fps
		 * 
		 * @returns Stop {@Time}
		 */	
		public uint Fps{
			get{return this.fps;}
			set{this.fps = value;}
		}
		
		/**
		 * Get the central frame (stop-start)/2
		 * 
		 * @returns Stop {@Time}
		 */
		public uint CentralFrame{
			get{ return this.StopFrame-((this.TotalFrames)/2);}
		}
		
		/**
		 * Get the numbers of frames between the start and stop time
		 * 
		 * @returns Stop {@Time}
		 */
		public uint TotalFrames{
			get{return this.StopFrame-this.StartFrame;}
		}
		
		/**
		 * Get/Set the start frame
		 * 
		 * @returns Stop {@Time}
		 */
		public uint StartFrame {
			get {return startFrame;}			
			set { 
				this.startFrame = value;
				this.Start = new Time((int)(1000*value/fps));
			}
		}
		
		/**
		 * Get/Set the stop frame
		 * 
		 * @returns Stop {@Time}
		 */
		public uint StopFrame {			
			get {return stopFrame;}
			set { 
				this.stopFrame = value;
				this.Stop = new Time((int)(1000*value/fps));
			}
		}
		
		/**
		 * Get the key frame number if this play as key frame drawing or 0
		 * 
		 * @returns Stop {@Time}
		 */
		public uint KeyFrame{
			get {
				if (HasKeyFrame)
					return (uint) KeyFrameDrawing.StopTime*fps/1000;
				else return 0;
			}
		}
	
		/**
		 * Get/Set wheter a this play is actually loaded
		 * 
		 * @returns Stop {@Time}
		 */
		public bool Selected {
			get {return selected;}
			set{this.selected = value;}			
		}
		
		/**
		 * Get/Set 
		 * 
		 * @returns Stop {@Time}
		 */
		public List<int> LocalPlayers{
			set {localPlayersList = value;}
			get{return localPlayersList;}
		}
		
		public List<int> VisitorPlayers{
			set {visitorPlayersList = value;}
			get{return visitorPlayersList;}
		}
		
		public Drawing KeyFrameDrawing{
			set{keyFrame = value;}
			get{return keyFrame;}
		}
		
		public bool HasKeyFrame{
			get{return keyFrame != null;}
		}			
		#endregion
		
		#region Public methods
		/**
		 * Returns true is the frame number is in the play span
		 * 
		 * @returns frame {@boo}
		 */
		public bool HasFrame(int frame){
			return (frame>=startFrame && frame<stopFrame);
		}
		
		/**
		 * Adds a local player to the local team player's list 
		 * 
		 * @param index {@Player} number
		 */
		public void AddLocalPlayer(int index){
			localPlayersList.Add(index);
		}
	
		/**
		 * Adds a visitor player to the visitor team player's list
		 * 
		 * @param index {@Player} number
		 */
		public void AddVisitorPlayer(int index){
			visitorPlayersList.Add(index);			
		}
		
		/**
		 * Removes a local player from the local team player's list
		 * 
		 * @param index {@Player} number
		 */
		public void RemoveLocalPlayer(int index){
			localPlayersList.Remove(index);
		}
	
		/**
		 * Removes a visitor player from the visitor team player's list
		 * 
		 * @param index {@Player} number
		 */
		public void RemoveVisitorPlayer(int index){
			visitorPlayersList.Remove(index);			
		}
		#endregion
	}		
}
