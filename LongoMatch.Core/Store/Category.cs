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
	public class Category:TaggerButton, IIDObject
	{

		#region Constructors
		public Category() {
			ID = System.Guid.NewGuid();
			Tags = new List<Tag>();
			TagGoalPosition = false;
			TagFieldPosition = true;
			Position = new Point (0, 0);
			ShowSubcategories = true;
			TagsPerRow = 2;
			Color = Color.Red;
			TextColor = Color.Grey2;
			TagMode = TagMode.Predefined;
			Width = 30;
			Height = 20;
		}
		#endregion

		#region  Properties

		/// <summary>
		/// Unique ID for this category
		/// </summary>
		public Guid ID {
			get;
			set;
		}

		/// <summary>
		/// A key combination to create plays in this category
		/// </summary>
		public HotKey HotKey {
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

		public List<Tag> Tags  {
			get;
			set;
		}
		
		public bool ShowSubcategories {
			get;
			set;
		}
		
		public int TagsPerRow {
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
		#endregion
		
	}
}
