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
using Mono.Unix;
using LongoMatch.Common;

namespace LongoMatch.TimeNodes
{

	/// <summary>
	/// Tag category for the analysis. Contains the default values to creates plays
	/// tagged in this category
	/// </summary>
	[Serializable]
	public class Category:TimeNode, ISerializable
	{
		
		private Guid _UUID;
		
		#region Constructors
		#endregion
		public Category (){
			_UUID = System.Guid.NewGuid();
		}
		#region  Properties
		
		/// <summary>
		/// Unique ID for this category 
		/// </summary>
		public Guid UUID{
			get {
				return _UUID;
			}
		}

		/// <summary>
		/// A key combination to create plays in this category
		/// </summary>
		public HotKey HotKey {
			get;
			set;
		}

		/// <summary>
		/// A color to identify plays in this category
		/// </summary>
		public Color Color {
			get;
			set;
		}
		
		//// <summary>
		/// Sort method used to sort plays for this category 
		/// </summary>
		public SortMethodType SortMethod{
			get;
			set;
		}
		
		/// <summary>
		/// Position of the category in the list of categories 
		/// </summary>
		public int Position{
			get;
			set;
		}
		
		/// <summary>
		/// Sort method string used for the UI
		/// </summary>
		public string SortMethodString{
			get{
				switch (SortMethod){
					case SortMethodType.SortByName:
						return Catalog.GetString("Sort by name");
					case SortMethodType.SortByStartTime:
						return Catalog.GetString("Sort by start time");
					case SortMethodType.SortByStopTime:
						return Catalog.GetString("Sort by stop time");
					case SortMethodType.SortByDuration:
						return Catalog.GetString("Sort by duration");
					default:
						return Catalog.GetString("Sort by name");
				}						
			}
			set{			
				if (value == Catalog.GetString("Sort by start time"))
					SortMethod = SortMethodType.SortByStartTime;
				else if (value == Catalog.GetString("Sort by stop time"))
					SortMethod = SortMethodType.SortByStopTime;
				else if (value == Catalog.GetString("Sort by duration"))
					SortMethod = SortMethodType.SortByDuration;
				else
					SortMethod = SortMethodType.SortByName;
			}
		}
		
		// this method is automatically called during serialization
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("uuid", UUID);
			info.AddValue("name", Name);
			info.AddValue("name", Name);
			info.AddValue("start", Start);
			info.AddValue("stop", Stop);
			info.AddValue("hotkey", HotKey);
			info.AddValue("position", Position);
			info.AddValue("red", Color.Red);
			info.AddValue("green", Color.Green);
			info.AddValue("blue", Color.Blue);
		}
		#endregion	
	}
}
