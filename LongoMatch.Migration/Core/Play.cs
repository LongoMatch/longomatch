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
using LongoMatch.Core;
using LongoMatch.Common;
using LongoMatch.Interfaces;
using Newtonsoft.Json;

namespace LongoMatch.Store
{

	/// <summary>
	/// Represents a Play in the game.
	/// </summary>

	[Serializable]
	public class  Play : PixbufTimeNode, ITimelineNode
	{

		#region Constructors
		public Play() {
			Drawings = new List<Drawing>();
			Tags = new StringTagStore();
			Players = new PlayersTagStore(); 
			Teams = new TeamsTagStore();
			Fps = 25;
			PlaybackRate = 1.0;
		}
		#endregion

		#region Properties
		
		public Guid ID {
			get;
			set;
		}

		/// <summary>
		/// Category in which this play is tagged
		/// </summary>
		public Category Category {
			get;
			set;
		}

		/// <summary>
		/// A string with the play's notes
		/// </summary>
		public string Notes {
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
		[JsonIgnore]
		public uint StartFrame {
			get {
				return (uint)(Start.MSeconds * Fps / 1000);
			}
			set {
				Start = new Time {MSeconds = (int)(1000 * value / Fps)};
			}
		}

		/// <summary>
		/// Stop frame number
		/// </summary>
		[JsonIgnore]
		public uint StopFrame {
			get {
				return (uint)(Stop.MSeconds * Fps / 1000);
			}
			set {
				Stop = new Time {MSeconds = (int)(1000 * value / Fps)};
			}
		}

		/// <summary>
		/// Get the key frame number if this play as key frame drawing or 0
		/// </summary>
		[JsonIgnore]
		public uint KeyFrame {
			get {
				if(HasDrawings)
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
		/// List of drawings for this play
		/// </summary>
		public List<Drawing> Drawings {
			get;
			set;
		}

		/* FIXME: Keep this until we support multiple drawings */
		[JsonIgnore]
		public Drawing KeyFrameDrawing {
			get {
				if(Drawings.Count > 0)
					return Drawings.First();
				else
					return null;
			} set {
				if (Drawings.Count == 0)
					Drawings.Add (value);
				else
					Drawings[0] = value;
			}
		}

		/// <summary>
		/// Get wether the play has at least a frame drawing
		/// </summary>
		[JsonIgnore]
		public bool HasDrawings {
			get {
				return Drawings.Count > 0;
			}
		}

		/// <summary>
		/// Central frame number using (stopFrame-startFrame)/2
		/// </summary>
		[JsonIgnore]
		public uint CentralFrame {
			get {
				return StopFrame-((TotalFrames)/2);
			}
		}

		/// <summary>
		/// Number of frames inside the play's boundaries
		/// </summary>
		[JsonIgnore]
		public uint TotalFrames {
			get {
				return StopFrame-StartFrame;
			}
		}
		
		[JsonIgnore]
		public Team Team {
			get;
			set;
		}
		
		[JsonProperty ("Players")]
		public List<Player> PlayersList {
			get {
				return Players.GetTagsValues();
			}
		}

		public string GamePeriod {
			get;
			set;
		}
		
		[JsonProperty ("Tags")]
		public List<Tag> TagsList {
			get;
			set;
		}
		
		[JsonIgnore]
		public StringTagStore Tags {
			get;
			set;
		}
		
		[JsonIgnore]
		public PlayersTagStore Players {
			get;
			set;
		}
		
		[JsonIgnore]
		public TeamsTagStore Teams {
			get;
			set;
		}
		
		public Coordinates FieldPosition {
			get;
			set;
		}

		public Coordinates HalfFieldPosition {
			get;
			set;
		}
		
		public Coordinates GoalPosition {
			get;
			set;
		}
		
		public double PlaybackRate {
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
		
		public override string ToString()
		{
			return Name + "\n" + Start.ToMSecondsString() + " - " + Stop.ToMSecondsString();
		}
		#endregion
	}
}
