// SectionsTimeNode.cs
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
using System.Runtime.Serialization;
using Gdk;

namespace LongoMatch.TimeNodes
{

	/// <summary>
	/// I am a tagging category for the analysis. I contain the default values to creates plays
	/// tagged in my category
	/// </summary>
	[Serializable]
	public class SectionsTimeNode:TimeNode, ISerializable
	{
		HotKey hotkey;
		Gdk.Color color;

		#region Constructors
		/// <summary>
		/// Creates a new category
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/> with the category's name
		/// </param>
		/// <param name="start">
		/// A <see cref="Time"/> with the default lead time
		/// </param>
		/// <param name="stop">
		/// A <see cref="Time"/> with the default lag time
		/// </param>
		/// <param name="hotkey">
		/// A <see cref="HotKey"/> with the hotkey to create new plays in my category
		/// </param>
		/// <param name="color">
		/// A <see cref="Color"/> that will be shared among plays tagged in my category
		/// </param>
		public SectionsTimeNode(String name,Time start, Time stop, HotKey hotkey, Color color):base(name,start,stop)
		{
			this.hotkey = hotkey;
			this.color = color;
		}
		
		// this constructor is automatically called during deserialization
		public SectionsTimeNode(SerializationInfo info, StreamingContext context) {
			Name = info.GetString("name");
			Start = (Time)info.GetValue("start", typeof(Time));
			Stop = (Time)info.GetValue("stop", typeof(Time));
			HotKey = (HotKey)info.GetValue("hotkey", typeof(HotKey));
			// read 'red', 'blue' and 'green' values and convert it to Gdk.Color
			Color = new Color((byte)info.GetValue("red", typeof(ushort)),
			                  (byte)info.GetValue("green", typeof(ushort)),
			                  (byte)info.GetValue("blue", typeof(ushort)));
			Console.WriteLine("Deserialize");
		}
		#endregion
		#region  Properties

		/// <value>
		/// A key combination to create plays in my category
		/// </value>
		public HotKey HotKey {
			get {
				return this.hotkey;
			}
			set {
				this.hotkey = value;
			}
		}

		/// <value>
		/// A color to draw plays from my category
		/// </value>
		public Color Color {
			get {
				return this.color;
			}
			set {
				this.color=value;
			}
		}
				
		// this method is automatically called during serialization
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			Console.WriteLine("Serialize");
			info.AddValue("name", Name);
			info.AddValue("start", Start);
			info.AddValue("stop", Stop);
			info.AddValue("hotkey", hotkey);
			info.AddValue("red", color.Red);
			info.AddValue("blue", color.Green);
			info.AddValue("green", color.Blue);
		}
		#endregion		
	}
}
