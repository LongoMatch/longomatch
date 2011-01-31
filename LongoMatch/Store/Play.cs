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

namespace LongoMatch.Store
{

	/// <summary>
	/// Represents a Play in the game.
	/// </summary>

	[Serializable]
	public class  Play : PixbufTimeNode
	{

		#region Constructors
		public Play(){
			Drawings = new DrawingsList();
			Tags = new List<Tag>();
		}
		#endregion

		#region Properties
		
		/// <summary>
		/// Category in which this play is tagged 
		/// </summary>
		public Category Category {get; set;}
		
		/// <summary>
		/// A string with the play's notes
		/// </summary>
		public string Notes {get; set;}

		/// <summary>
		/// Video framerate in frames per second. This value is taken from the
		/// video file properties and used to translate from seconds
		/// to frames: second 100 is equivalent to frame 100*fps
		/// </summary>
		public uint Fps {get; set;}

		/// <summary>
		/// Start frame number
		/// </summary>
		public uint StartFrame {
			get {return (uint) (Start.MSeconds * Fps / 1000);}
			set {Start = new Time {MSeconds = (int)(1000 * value / Fps)};}
		}

		/// <summary>
		/// Stop frame number
		/// </summary>
		public uint StopFrame {
			get {return (uint) (Stop.MSeconds * Fps / 1000);}
			set {Stop = new Time {MSeconds = (int)(1000 * value / Fps)};}
		}

		/// <summary>
		/// Get the key frame number if this play as key frame drawing or 0
		/// </summary>
		public uint KeyFrame {
			get {
				if (HasDrawings)
					return (uint) KeyFrameDrawing.RenderTime * Fps / 1000;
				else return 0;
			}
		}

		/// <summary>
		/// Get/Set wheter this play is actually loaded. Used in  <see cref="LongoMatch.Gui.Component.TimeScale">
		/// </summary>
		public bool Selected {get; set;}
	
		/// <summary>
		/// List of drawings for this play
		/// </summary>
		public DrawingsList Drawings {get; set;}
		
		/* FIXME: Keep this until we support multiple drawings */
		public Drawing KeyFrameDrawing {
			get{
				if (Drawings.Count > 0)
					return Drawings.List[0];
				else
					return null;
			}
		}
		
		/// <summary>
		/// Get wether the play has at least a frame drawing
		/// </summary>
		public bool HasDrawings {
			get {
				return Drawings.Count > 0;
			}
		}
		
		/// <summary>
		/// Central frame number using (stopFrame-startFrame)/2
		/// </summary>
		public uint CentralFrame {
			get {return StopFrame-((TotalFrames)/2);}
		}

		/// <summary>
		/// Number of frames inside the play's boundaries
		/// </summary>
		public uint TotalFrames {
			get {return StopFrame-StartFrame;}
		}

		//// <summary>
		/// Play's tags 
		/// </summary>
		public List<Tag> Tags{get; set;}
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
		
		/// <summary>
		/// Return True if the play contains a similar tag 
		/// </summary>
		/// <param name="name">
		/// A <see cref="String"/> with the tag name
		/// </param>
		/// <param name="val">
		/// A <see cref="System.Object"/> with tag value
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool HasTag(String name, object val) {
			return  (from tag in Tags
					where (tag.Name == (string)name) && (tag.Value == val) 
					select tag).Count() > 0;
		}
		
		public override string ToString ()
		{
			String[] tags = new String[Tags.Count];
		
			for (int i=0; i<Tags.Count; i++)
				tags[i] = Tags[i].Value.ToString();
			
			return  "<b>"+Catalog.GetString("Name")+": </b>"+Name+"\n"+
					"<b>"+Catalog.GetString("Start")+": </b>"+Start.ToMSecondsString()+"\n"+
					"<b>"+Catalog.GetString("Stop")+": </b>"+Stop.ToMSecondsString()+"\n"+
					"<b>"+Catalog.GetString("Tags")+": </b>"+ String.Join(" ; ", tags);
		}
		#endregion
	}
}
