// 
//  Copyright (C) 2011 Andoni Morales Alastruey
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
// 
using System;
using Newtonsoft.Json;
using LongoMatch.Interfaces;

namespace LongoMatch.Store
{
    /* FIXME: Code duplicated from Play, unfortunately we can't
      * modify the class hierachy */
	[Serializable]
	public class TimelineNode: TimeNode, ITimelineNode
	{
		public TimelineNode ()
		{
		}
		
		/// <summary>
		/// Video framerate in frames per second. This value is taken from the
		/// video file properties and used to translate from seconds
		/// to frames: second 100 is equivalent to frame 100*fps
		/// </summary>
		[JsonIgnore]
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
		/// Get/Set wheter this play is actually loaded. Used in  <see cref="LongoMatch.Gui.Component.TimeScale">
		/// </summary>
		[JsonIgnore]
		public bool Selected {
			get;
			set;
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
		
		/// <summary>
		/// Get the key frame number if this play as key frame drawing or 0
		/// </summary>
		public uint KeyFrame {
			get {
				return 0;
			}
		}
		
		public bool HasDrawings {
			get;
			set;
		}

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
	}
}

