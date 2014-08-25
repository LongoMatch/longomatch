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
	public class Category: AnalysisCategory, IIDObject
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
		
		#endregion
		
	}
}
