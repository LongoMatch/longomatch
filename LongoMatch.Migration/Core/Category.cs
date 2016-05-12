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
using System.Collections.Generic;
using System.Runtime.Serialization;
using LongoMatch.Common;
using LongoMatch.Core;

using LongoMatch.Interfaces;
using Newtonsoft.Json;
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

		[JsonProperty ("ID")]
		private Guid _UUID;

		#region Constructors

		#endregion

		public Category ()
		{
			_UUID = System.Guid.NewGuid ();
			SubCategories = new List<ISubCategory> ();
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
			set {
				_UUID = value;
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
		[JsonProperty ("Color")]
		public  Color LColor {
			get {
				return new Color (this.Color.R, this.Color.G,
					this.Color.B, this.Color.A);
			}
			set {
			}
		}

		[JsonIgnore]
		public System.Drawing.Color Color {
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

		public List<SubCategory> SubCategoriesList {
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
				switch (SortMethod) {
				case SortMethodType.SortByName:
					return Catalog.GetString ("Sort by name");
				case SortMethodType.SortByStartTime:
					return Catalog.GetString ("Sort by start time");
				case SortMethodType.SortByStopTime:
					return Catalog.GetString ("Sort by stop time");
				case SortMethodType.SortByDuration:
					return Catalog.GetString ("Sort by duration");
				default:
					return Catalog.GetString ("Sort by name");
				}
			}
			set {
				if (value == Catalog.GetString ("Sort by start time"))
					SortMethod = SortMethodType.SortByStartTime;
				else if (value == Catalog.GetString ("Sort by stop time"))
					SortMethod = SortMethodType.SortByStopTime;
				else if (value == Catalog.GetString ("Sort by duration"))
					SortMethod = SortMethodType.SortByDuration;
				else
					SortMethod = SortMethodType.SortByName;
			}
		}

		// this constructor is automatically called during deserialization
		public Category (SerializationInfo info, StreamingContext context)
		{
			_UUID = (Guid)info.GetValue ("uuid", typeof(Guid));
			Name = (string)info.GetValue ("name", typeof(string));
			Start = (Time)info.GetValue ("start", typeof(Time));
			Stop = (Time)info.GetValue ("stop", typeof(Time));
			HotKey = (HotKey)info.GetValue ("hotkey", typeof(HotKey));
			SubCategories = (List<ISubCategory>)info.GetValue ("subcategories", typeof(List<ISubCategory>));
			Position = (Int32)info.GetValue ("position", typeof(Int32));
			SortMethod = (SortMethodType)info.GetValue ("sort_method", typeof(SortMethodType));
			Color = System.Drawing.Color.FromArgb (
				LongoMatch.Common.Color.UShortToByte ((ushort)info.GetValue ("red", typeof(ushort))),
				LongoMatch.Common.Color.UShortToByte ((ushort)info.GetValue ("green", typeof(ushort))),
				LongoMatch.Common.Color.UShortToByte ((ushort)info.GetValue ("blue", typeof(ushort))));
			LColor = LongoMatch.Common.Color.ColorFromUShort ((ushort)info.GetValue ("red", typeof(ushort)),
				(ushort)info.GetValue ("green", typeof(ushort)),
				(ushort)info.GetValue ("blue", typeof(ushort)));
			try {
				TagFieldPosition = (bool)info.GetValue ("tagfieldpos", typeof(bool));
			} catch {
				TagFieldPosition = true;
			}
			try {
				TagHalfFieldPosition = (bool)info.GetValue ("taghalffieldpos", typeof(bool));
			} catch {
				TagHalfFieldPosition = false;
			}
			try {
				TagGoalPosition = (bool)info.GetValue ("taggoalpos", typeof(bool));
			} catch {
				TagGoalPosition = false;
			}
			try {
				FieldPositionIsDistance = (bool)info.GetValue ("fieldposisdist", typeof(bool));
			} catch {
				FieldPositionIsDistance = false;
			}
			try {
				HalfFieldPositionIsDistance = (bool)info.GetValue ("halffieldposisdist", typeof(bool));
			} catch {
				HalfFieldPositionIsDistance = false;
			}
		}

		// this method is automatically called during serialization
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("ID", UUID);
			info.AddValue ("Name", Name);
			info.AddValue ("Start", Start);
			info.AddValue ("Stop", Stop);
			info.AddValue ("Hotkey", HotKey);
			info.AddValue ("Position", Position);
			info.AddValue ("SubCategories", SubCategoriesList);
			/* Convert to ushort for backward compatibility */
			info.AddValue ("Color", LColor);
			info.AddValue ("red", ByteToUShort (Color.R));
			info.AddValue ("green", ByteToUShort (Color.G));
			info.AddValue ("blue", ByteToUShort (Color.B));
			info.AddValue ("SortMethod", SortMethod);
			info.AddValue ("TagFieldPosition", TagFieldPosition);
			info.AddValue ("TagHalfFieldPosition", TagHalfFieldPosition);
			info.AddValue ("TagGoalPosistion", TagGoalPosition);
			info.AddValue ("FieldPositionIsDistance", FieldPositionIsDistance);
			info.AddValue ("HalfFieldPositionIsDistance", HalfFieldPositionIsDistance);
		}

		ushort ByteToUShort (Byte val)
		{
			var ret = (ushort)(((float)val) / byte.MaxValue * ushort.MaxValue);
			return ret;
		}

		#endregion
		
	}
}
