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
	public enum Team {
		NONE = 0,
		LOCAL = 1,
		VISITOR = 2,
	}

	/// <summary>
	/// I represent a Play in the game, that's why I'm probably the most
	/// important object of the database.
	/// I have a name to describe the play as well as a start and a stop {@LongoMatch.TimeNode.Time},
	/// which sets the play's position in the game's time line.
	/// I also stores a list a {@LongoMatch.TimeNode.Player} tagged to this play.
	/// </summary>

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


		/// <summary>
		/// Creates a new play
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/> with the play's name
		/// </param>
		/// <param name="start">
		/// A <see cref="Time"/> with the play's start time
		/// </param>
		/// <param name="stop">
		/// A <see cref="Time"/> with the play's stop time
		/// </param>
		/// <param name="notes">
		/// A <see cref="System.String"/> with the play's notes
		/// </param>
		/// <param name="fps">
		/// A <see cref="System.UInt32"/> with the frame rate in frames per second
		/// </param>
		/// <param name="thumbnail">
		/// A <see cref="Pixbuf"/> with the play's preview
		/// </param>
		#region Constructors
		public MediaTimeNode(String name, Time start, Time stop,string notes, uint fps,Pixbuf thumbnail):base(name,start,stop,thumbnail) {
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
		/// <value>
		/// Play's notes
		/// </value>
		public string Notes {
			get {
				return notes;
			}
			set {
				notes = value;
			}
		}

		/// <value>
		/// The <see cref="LongoMatch.TimeNode.Team"/> associated to this play
		/// </value>
		public Team Team {
			get {
				return this.team;
			}
			set {
				this.team = value;
			}
		}

		/// <value>
		/// Video frameratein frames per second. This value is taken from the
		/// video file properties and used to translate from seconds
		/// to frames: second 100 is equivalent to frame 100*fps
		/// </value>
		public uint Fps {
			get {
				return this.fps;
			}
			set {
				this.fps = value;
			}
		}

		/// <value>
		/// Central frame number using (stopFrame-startFrame)/2
		/// </value>
		public uint CentralFrame {
			get {
				return this.StopFrame-((this.TotalFrames)/2);
			}
		}

		/// <value>
		/// Number of frames inside the play's boundaries
		/// </value>
		public uint TotalFrames {
			get {
				return this.StopFrame-this.StartFrame;
			}
		}

		/// <value>
		/// Start frame number
		/// </value>
		public uint StartFrame {
			get {
				return startFrame;
			}
			set {
				this.startFrame = value;
				this.Start = new Time((int)(1000*value/fps));
			}
		}

		/// <value>
		/// Stop frame number
		/// </value>
		public uint StopFrame {
			get {
				return stopFrame;
			}
			set {
				this.stopFrame = value;
				this.Stop = new Time((int)(1000*value/fps));
			}
		}

		/// <value>
		/// Get the key frame number if this play as key frame drawing or 0
		/// </value>
		public uint KeyFrame {
			get {
				if (HasKeyFrame)
					return (uint) KeyFrameDrawing.StopTime*fps/1000;
				else return 0;
			}
		}

		/// <value>
		/// Get/Set wheter this play is actually loaded. Used in {@LongoMatch.Gui.Component.TimeScale}
		/// </value>
		public bool Selected {
			get {
				return selected;
			}
			set {
				this.selected = value;
			}
		}

		/// <value>
		/// Get/Set a list of local players tagged to this play
		/// </value>
		public List<int> LocalPlayers {
			set {
				localPlayersList = value;
			}
			get {
				return localPlayersList;
			}
		}

		/// <value>
		/// Get/Set a list of visitor players tagged to this play
		/// </value>
		public List<int> VisitorPlayers {
			set {
				visitorPlayersList = value;
			}
			get {
				return visitorPlayersList;
			}
		}

		/// <value>
		/// Get/Set the key frame's <see cref="LongoMatch.TimeNodes.Drawing"/>
		/// </value>
		public Drawing KeyFrameDrawing {
			set {
				keyFrame = value;
			}
			get {
				return keyFrame;
			}
		}

		/// <value>
		/// Get wether the play has as defined a key frame
		/// </value>
		public bool HasKeyFrame {
			get {
				return keyFrame != null;
			}
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Check the frame number is inside the play boundaries
		/// </summary>
		/// <param name="frame">
		/// A <see cref="System.Int32"/> with the frame number
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool HasFrame(int frame) {
			return (frame>=startFrame && frame<stopFrame);
		}

		/// <summary>
		/// Adds a player to the local team player's list
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/> with the <see cref="LongoMatch.TimeNode.Player"/> index
		/// </param>
		public void AddLocalPlayer(int index) {
			localPlayersList.Add(index);
		}

		/// <summary>
		/// Adds a player to the visitor team player's list
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/> with the <see cref="LongoMatch.TimeNode.Player"/> index
		/// </param>
		public void AddVisitorPlayer(int index) {
			visitorPlayersList.Add(index);
		}

		/// <summary>
		/// Removes a player from the local team player's list
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/> with the <see cref="LongoMatch.TimeNode.Player"/> index
		/// </param>
		public void RemoveLocalPlayer(int index) {
			localPlayersList.Remove(index);
		}

		/// <summary>
		/// Removes a player from the visitor team player's list
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/> with the <see cref="LongoMatch.TimeNode.Player"/> index
		/// </param>
		public void RemoveVisitorPlayer(int index) {
			visitorPlayersList.Remove(index);
		}
		#endregion
	}
}
