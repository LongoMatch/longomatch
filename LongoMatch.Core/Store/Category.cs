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
using Newtonsoft.Json;

using LongoMatch.Common;
using LongoMatch.Interfaces;
using Image = LongoMatch.Common.Image;

namespace LongoMatch.Store
{

	/// <summary>
	/// Tag category for the analysis. Contains the default values to creates plays
	/// tagged in this category
	/// </summary>
	[Serializable]
	public class Category:TimeNode, ISerializable
	{

		[JsonProperty ("UUID")]
		private Guid _UUID;

		#region Constructors
		#endregion
		public Category() {
			_UUID = System.Guid.NewGuid();
			SubCategories = new List<ISubCategory>();
			TagGoalPosition = false;
			TagFieldPosition = true;
		}

		#region  Properties

		/// <summary>
		/// Unique ID for this category
		/// </summary>
		[JsonIgnore]
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
		public  System.Drawing.Color Color {
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
		
		public bool TagGoalPosition {
			get;
			set;
		}
		
		public bool TagFieldPosition {
			get;
			set;
		}
		
		public bool TagHalfFieldPosition {
			get;
			set;
		}
		
		public bool FieldPositionIsDistance {
			get;
			set;
		}
		
		public bool HalfFieldPositionIsDistance {
			get;
			set;
		}
		
		/// <summary>
		/// Sort method string used for the UI
		/// </summary>
		[JsonIgnore]
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
			Name = (string) info.GetValue("name", typeof(string));
			Start = (Time)info.GetValue("start", typeof(Time));
			Stop = (Time)info.GetValue("stop", typeof(Time));
			HotKey = (HotKey)info.GetValue("hotkey", typeof(HotKey));
			SubCategories = (List<ISubCategory>)info.GetValue("subcategories", typeof(List<ISubCategory>));
			Position = (Int32) info.GetValue("position", typeof (Int32));
			SortMethod = (SortMethodType)info.GetValue("sort_method", typeof(SortMethodType));
			Color = Color.FromArgb(
				ColorHelper.ShortToByte((ushort)info.GetValue("red", typeof(ushort))),
				ColorHelper.ShortToByte((ushort)info.GetValue("green", typeof(ushort))),
				ColorHelper.ShortToByte((ushort)info.GetValue("blue", typeof(ushort))));
			try {
				TagFieldPosition = (bool) info.GetValue("tagfieldpos", typeof (bool));
			} catch {
				TagFieldPosition = true;
			}
			try {
				TagHalfFieldPosition =(bool) info.GetValue("taghalffieldpos", typeof (bool));
			} catch {
				TagHalfFieldPosition = false;
			}
			try {
				TagGoalPosition = (bool) info.GetValue("taggoalpos", typeof (bool));
			} catch {
				TagGoalPosition = false;
			}
			try {
				FieldPositionIsDistance =(bool) info.GetValue("fieldposisdist", typeof (bool));
			} catch {
				FieldPositionIsDistance = false;
			}
			try {
				HalfFieldPositionIsDistance =(bool) info.GetValue("halffieldposisdist", typeof (bool));
			} catch {
				HalfFieldPositionIsDistance = false;
			}
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
			/* Convert to ushort for backward compatibility */
			info.AddValue("red", ColorHelper.ByteToShort(Color.R));
			info.AddValue("green", ColorHelper.ByteToShort(Color.G));
			info.AddValue("blue", ColorHelper.ByteToShort(Color.B));
			info.AddValue("sort_method", SortMethod);
			info.AddValue("tagfieldpos", TagFieldPosition);
			info.AddValue("taghalffieldpos", TagHalfFieldPosition);
			info.AddValue("taggoalpos", TagGoalPosition);
			info.AddValue("fieldposisdist", FieldPositionIsDistance);
			info.AddValue("halffieldposisdist", HalfFieldPositionIsDistance);
		}
		#endregion
		
	}
}
