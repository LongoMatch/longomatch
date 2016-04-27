// Sections.cs
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
using Newtonsoft.Json;

using LongoMatch.Core;
using LongoMatch.Common;
using LongoMatch.Interfaces;

using Image = LongoMatch.Common.Image;
using VAS.Core;

namespace LongoMatch.Store.Templates
{

	/// <summary>
	/// I am a template for the analysis categories used in a project.
	/// I describe each one of the categories and provide the default values
	/// to use to create plys in a specific category.
	/// The position of the category in the index is very important and should
	/// respect the same index used in the plays list inside a project.
	/// The <see cref="LongoMatch.DB.Project"/> must handle all the changes
	/// </summary>
	[Serializable]
	[JsonObject]
	public class Categories: List<Category>, ITemplate, ITemplate<Category>
	{
		/* Database additions */
		GameUnitsList gameUnits;
		Version version;
		byte[] fieldImage, halfFieldImage, goalImage;

		/// <summary>
		/// Creates a new template
		/// </summary>
		public Categories ()
		{
		}

		public Guid ID {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		[JsonIgnore]
		public Version Version {
			get;
			set;
		}

		[JsonIgnore]
		public GameUnitsList GameUnits {
			set {
				gameUnits = value;
			}
			get {
				if (gameUnits == null) {
					gameUnits = new GameUnitsList ();
				}
				return gameUnits;
			}
		}

		public List<string> GamePeriods {
			get;
			set;
		}

		public Image Image {
			get;
			set;
		}

		public List<Category> List {
			get {
				return this.ToList ();
			}
		}
		

		
		/* Keep this for backwards compatiblity with 0.18.11 */
		[JsonIgnore]
		[Obsolete]
		public Image FieldBackgroundImage { get;	set; }

		[JsonIgnore]
		[Obsolete]
		public Image HalfFieldBackgroundImage { get; set; }

		[JsonIgnore]
		[Obsolete]
		public Image GoalBackgroundImage { get; set; }

		public Image FieldBackground {
			get {
				if (fieldImage != null)
					return Image.Deserialize (fieldImage);
				else
					return null;
			}
			set {
				if (value != null) {
					if (value.Width > 500) {
						value.Scale (500, 500);
					}
					fieldImage = value.Serialize ();
				} else
					fieldImage = null;
			}
		}

		public Image HalfFieldBackground {
			get {
				if (halfFieldImage != null)
					return Image.Deserialize (halfFieldImage);
				else
					return null;
			}
			set {
				if (value != null) {
					if (value.Width > 500) {
						value.Scale (500, 500);
					}
					halfFieldImage = value.Serialize ();
				} else
					halfFieldImage = null;
			}
		}

		public Image GoalBackground {
			get {
				if (goalImage != null)
					return Image.Deserialize (goalImage);
				else
					return null;
			}
			set {
				if (value != null) {
					if (value.Width > 500) {
						value.Scale (500, 500);
					}
					goalImage = value.Serialize ();
				} else
					goalImage = null;
			}
		}

		public void Save (string filePath)
		{
			SerializableObject.Save (this, filePath);
		}

		public void AddDefaultSubcategories (Category cat)
		{
			TagSubCategory resultsubcat;
			
			resultsubcat = new TagSubCategory {
				Name = Catalog.GetString ("Outcome"),
				AllowMultiple = false,
			};
			resultsubcat.Add (Catalog.GetString ("Success"));
			resultsubcat.Add (Catalog.GetString ("Failure"));
			cat.SubCategories.Add (resultsubcat);
		}

		public Category AddDefaultItem (int index)
		{
			Color c = Color.Red;
			HotKey h = new HotKey ();
			
			
			Category cat = new Category {
				Name = "Category " + index,
				LColor = c,
				Start = new Time{ Seconds = 10 },
				Stop = new Time { Seconds = 10 },
				SortMethod = SortMethodType.SortByStartTime,
				HotKey = h,
				Position = index - 1,
			};
			AddDefaultSubcategories (cat);
			Insert (index, cat);
			return cat;
		}

		public static Categories Load (string filePath)
		{
			Categories cat = SerializableObject.LoadSafe<Categories> (filePath);
			if (cat.GamePeriods == null) {
				cat.GamePeriods = new List<string> ();
				cat.GamePeriods.Add ("1");
				cat.GamePeriods.Add ("2");
			}
			return cat;
		}

		public static Categories DefaultTemplate (int count)
		{
			List<string> periods = new List<string> ();
			Categories defaultTemplate = new Categories ();
			
			defaultTemplate.FillDefaultTemplate (count);
			periods.Add ("1");
			periods.Add ("2");
			defaultTemplate.GamePeriods = periods; 
			defaultTemplate.Version = new Version (Constants.DB_MAYOR_VERSION, Constants.DB_MINOR_VERSION);
			return defaultTemplate;
		}

		private void FillDefaultTemplate (int count)
		{
			for (int i = 1; i <= count; i++)
				AddDefaultItem (i - 1);
		}
	}
}
