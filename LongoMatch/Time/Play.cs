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
using System.Linq;
using Mono.Unix;
using Gdk;
using LongoMatch.Common;

namespace LongoMatch.TimeNodes
{

	/// <summary>
	/// Represents a Play in the game.
	/// </summary>

	[Serializable]
	public class  Play : PixbufTimeNode
	{

		#region Constructors
		public Play(){
			Players = new List<Player>();
			Tags = new List<Tag>();
		}
		#endregion

		#region Properties
		
		/// <summary>
		/// Category in which this play is tagged 
		/// </summary>
		public Category Category {
			get;
			set;
		}
		
		/// <summary>
		/// A strng with the play's notes
		/// </summary>
		public string Notes {
			get;
			set;
		}

		/// <summary>
		/// The <see cref="LongoMatch.TimeNode.Team"/> associated to this play
		/// </summary>
		public Team Team {
			get;
			set;
		}

		/// <summary>
		/// Video framerate in frames per second. This value is taken from the
		/// video file properties and used to translate from seconds
		/// to frames: second 100 is equivalent to frame 100*fps
		/// </summary>
		public uint Fps {
			get;
			set;
		}

		/// <summary>
		/// Start frame number
		/// </summary>
		public uint StartFrame {
			get {
				return (uint) (Start.MSeconds * Fps / 1000);
			}
			set {
				Start = new Time {MSeconds = (int)(1000 * value / Fps)};
			}
		}

		/// <summary>
		/// Stop frame number
		/// </summary>
		public uint StopFrame {
			get {
				return (uint) (Stop.MSeconds * Fps / 1000);
			}
			set {
				Stop = new Time {MSeconds = (int)(1000 * value / Fps)};
			}
		}

		/// <summary>
		/// Get the key frame number if this play as key frame drawing or 0
		/// </summary>
		public uint KeyFrame {
			get {
				if (HasKeyFrame)
					return (uint) KeyFrameDrawing.RenderTime * Fps / 1000;
				else return 0;
			}
		}

		/// <summary>
		/// Get/Set wheter this play is actually loaded. Used in  <see cref="LongoMatch.Gui.Component.TimeScale">
		/// </summary>
		public bool Selected {
			get;
			set;
		}

		/// <summary>
		/// Get/Set a list of the players tagged to this play
		/// </summary>
		public List<Player> Players {
			get;
			set;
		}
	
		/// <summary>
		/// Gets a list of players for the local team 
		/// </summary>
		public List<Player> LocalPlayers {
			get{
				return 
					(from player in Players
					 where player.Team == Team.LOCAL
					 select player
					 ).ToList();
			} 
			/* FIXME: should an array for each team */
			set {
				var l = VisitorPlayers;
				l.AddRange(value);
				Players = l;
			}
		}
	
		/// <summary>
		/// Gets a list of players for the visitor team 
		/// </summary>
		public List<Player> VisitorPlayers {
			get{
				return 
					(from player in Players
					 where player.Team == Team.VISITOR
					 select player
					 ).ToList();
			}
			
			set {
				var l = LocalPlayers;
				l.AddRange(value);
				Players = l;
			}
		}

		/// <summary>
		/// Get/Set the key frame's <see cref="LongoMatch.TimeNodes.Drawing"/>
		/// </summary>
		public Drawing KeyFrameDrawing {
			get;
			set;
		}

		/// <summary>
		/// Get wether the play has defined a key frame
		/// </summary>
		public bool HasKeyFrame {
			get {
				return KeyFrameDrawing != null;
			}
		}
		
		/// <summary>
		/// Central frame number using (stopFrame-startFrame)/2
		/// </summary>
		public uint CentralFrame {
			get {
				return StopFrame-((TotalFrames)/2);
			}
		}

		/// <summary>
		/// Number of frames inside the play's boundaries
		/// </summary>
		public uint TotalFrames {
			get {
				return StopFrame-StartFrame;
			}
		}

		//// <summary>
		/// Play's tags 
		/// </summary>
		public List<Tag> Tags{
			get;
			set;
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Check if the frame number is inside the play boundaries
		/// </summary>
		/// <param name="frame">
		/// A <see cref="System.Int32"/> with the frame number
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool HasFrame(int frame) {
			return (frame>=StartFrame && frame<StopFrame);
		}

		public bool HasPlayer(Player player){
			return Players.Contains(player);
		}
		
		/// <summary>
		/// Add a player to the player's list
		/// </summary>
		/// <param name="player">
		/// The <see cref="LongoMatch.TimeNode.Player"/> to add
		/// </param>
		public void AddPlayer(Player player) {
			Players.Add(player);
		}

		/// <summary>
		/// Removes a player from the player's list
		/// </summary>
		/// <param name="player">
		/// The <see cref="LongoMatch.TimeNode.Player"/> to remove
		/// </param>
		public void RemovePlayer(Player Player) {
			Players.Remove(Player);
		}
		
		/// <summary>
		/// Adds a new tag to the play 
		/// </summary>
		/// <param name="tag">
		/// A <see cref="Tag"/>: the tag to add
		/// </param>
		public void AddTag(Tag tag){
			if (!Tags.Contains(tag))
				Tags.Add(tag);
		}
		
		/// <summary>
		/// Removes a tag to the play
		/// </summary>
		/// <param name="tag">
		/// A <see cref="Tag"/>: the tag to remove
		/// </param>
		public void RemoveTag(Tag tag){
			if (Tags.Contains(tag))
				Tags.Remove(tag);
		}
		
		public string ToString (string team)
		{
			String[] tags = new String[Tags.Count];
			
			for (int i=0; i<Tags.Count; i++)
				tags[i] = Tags[i].Value;
			
			return  "<b>"+Catalog.GetString("Name")+": </b>"+Name+"\n"+
				    "<b>"+Catalog.GetString("Team")+": </b>"+team+"\n"+
					"<b>"+Catalog.GetString("Start")+": </b>"+Start.ToMSecondsString()+"\n"+
					"<b>"+Catalog.GetString("Stop")+": </b>"+Stop.ToMSecondsString()+"\n"+
					"<b>"+Catalog.GetString("Tags")+": </b>"+ String.Join(" ; ", tags);
		}
		
		public override string ToString(){
			return ToString(Team.ToString());
		}

		#endregion
	}
}
