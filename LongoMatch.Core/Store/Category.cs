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
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Interfaces;

namespace LongoMatch.Store
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
		public Category() {
			_UUID = System.Guid.NewGuid();
			SubCategories = new List<ISubCategory>();
		}

		#region  Properties

		/// <summary>
		/// Unique ID for this category
		/// </summary>
		public Guid UUID {
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
		public  Color Color {
			get;
			set;
		}

		//// <summary>
		/// Sort method used to sort plays for this category
		/// </summary>
		public SortMethodType SortMethod {
			get;
			set;
		}

		/// <summary>
		/// Position of the category in the list of categories
		/// </summary>
		public int Position {
			get;
			set;
		}

		public List<ISubCategory> SubCategories {
			get;
			set;
		}

		/// <summary>
		/// Sort method string used for the UI
		/// </summary>
		public string SortMethodString {
			get {
				switch(SortMethod) {
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
			set {
				if(value == Catalog.GetString("Sort by start time"))
					SortMethod = SortMethodType.SortByStartTime;
				else if(value == Catalog.GetString("Sort by stop time"))
					SortMethod = SortMethodType.SortByStopTime;
				else if(value == Catalog.GetString("Sort by duration"))
					SortMethod = SortMethodType.SortByDuration;
				else
					SortMethod = SortMethodType.SortByName;
			}
		}

		// this constructor is automatically called during deserialization
		public Category(SerializationInfo info, StreamingContext context) {
			_UUID = (Guid)info.GetValue("uuid", typeof(Guid));
			Name = info.GetString("name");
			Start = (Time)info.GetValue("start", typeof(Time));
			Stop = (Time)info.GetValue("stop", typeof(Time));
			HotKey = (HotKey)info.GetValue("hotkey", typeof(HotKey));
			SubCategories = (List<ISubCategory>)info.GetValue("subcategories", typeof(List<ISubCategory>));
			Position = info.GetInt32("position");
			SortMethod = (SortMethodType)info.GetValue("sort_method", typeof(SortMethodType));
			// read 'red', 'blue' and 'green' values and convert it to Gdk.Color
			Color = Color.FromArgb(
				(ushort)info.GetValue("red", typeof(ushort)),
				(ushort)info.GetValue("green", typeof(ushort)),
				(ushort)info.GetValue("blue", typeof(ushort)));
		}

		// this method is automatically called during serialization
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("uuid", UUID);
			info.AddValue("name", Name);
			info.AddValue("start", Start);
			info.AddValue("stop", Stop);
			info.AddValue("hotkey", HotKey);
			info.AddValue("position", Position);
			info.AddValue("subcategories", SubCategories);
			info.AddValue("red", Color.R);
			info.AddValue("green", Color.G);
			info.AddValue("blue", Color.B);
			info.AddValue("sort_method", SortMethod);
		}
		#endregion
	}
}
