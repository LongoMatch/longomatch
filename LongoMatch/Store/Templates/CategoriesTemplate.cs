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
using Gdk;
using LongoMatch.Common;

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
	public class Categories: SerializableObject
	{
		private List<Category> categoriesList;


		/// <summary>
		/// Creates a new template
		/// </summary>
		public Categories()
		{
			categoriesList = new List<Category>();
		}

		/// <summary>
		/// Clear the template
		/// </summary>
		public void Clear() {
			categoriesList.Clear();
		}

		/// <summary>
		/// Adds a new analysis category to the template
		/// </summary>
		/// <param name="tn">
		/// A <see cref="Category"/>: category to add
		/// </param>
		public void AddCategory(Category category) {
			categoriesList.Add(category);
		}
		
		public void AddCategoryAtPos(int pos, Category category) {
			categoriesList.Insert(pos, category);
		}

		/// <summary>
		/// Delete a category from the templates using the it's index
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/>: position of the category to delete
		/// </param>
		public void RemoveCategory(Category category) {
			categoriesList.Remove(category);
		}

		//// <value>
		/// Number of categories
		/// </value>
		public int Count {
			get {
				return categoriesList.Count;
			}
		}

		//// <value>
		/// Ordered list with all the categories
		/// </value>
		public List<Category> CategoriesList {
			set {
				categoriesList.Clear();
				categoriesList = value;
			}
			get {
				return categoriesList;
			}
		}

		/// <summary>
		/// Retrieves a category at a given index
		/// </summary>
		/// <param name="section">
		/// A <see cref="System.Int32"/>: position of the category to retrieve
		/// </param>
		/// <returns>
		/// A <see cref="SectionsTimeNode"/>: category retrieved
		/// </returns>
		public Category GetCategoryAtPos(int pos) {
			return categoriesList[pos];
		}

		/// <summary>
		/// Returns an array if strings with the categories names
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public List<string> GetSectionsNames() {
			return (from c in categoriesList
			        orderby c.Position
			        select c.Name).ToList();
		}

		/// <summary>
		/// Returns an array of the categories' color
		/// </summary>
		/// <returns>
		/// A <see cref="Color"/>
		/// </returns>
		public List<Color> GetColors() {
			return (from c in categoriesList
			        orderby c.Position
			        select c.Color).ToList();
		}

		/// <summary>
		/// Return an array of the hotkeys for this template
		/// </summary>
		/// <returns>
		/// A <see cref="HotKey"/>
		/// </returns>
		public List<HotKey> GetHotKeys() {
			return (from c in categoriesList
			        orderby c.Position
			        select c.HotKey).ToList();
		}

		/// <summary>
		/// Returns an array with the default start times
		/// </summary>
		/// <returns>
		/// A <see cref="Time"/>
		/// </returns>
		public List<Time> GetSectionsStartTimes() {
			return (from c in categoriesList
			        orderby c.Position
			        select c.Start).ToList();
		}

		/// <summary>
		/// Returns an array with the defaul stop times
		/// </summary>
		/// <returns>
		/// A <see cref="Time"/>
		/// </returns>
		public List<Time> GetSectionsStopTimes() {
			return (from c in categoriesList
			        orderby c.Position
			        select c.Stop).ToList();
		}
		
		public void Save(string filePath){
			Save(this, filePath);
		}
		
		public static Categories Load(string filePath) {
			return Load<Categories>(filePath);
		}
		
		public static Categories DefaultTemplate() {
			Categories defaultTemplate = new Categories();
			defaultTemplate.FillDefaultTemplate();
			return defaultTemplate;
		}

		private void FillDefaultTemplate() {
			Color c = new Color((Byte)255, (Byte)0, (Byte)0);
			HotKey h = new HotKey();
			
			for (int i=1; i<=20;i++) {
				AddCategory(new Category{
					Name = "Category " + i,
					Color = c, 
					Start = new Time{Seconds = 10},
					Stop = new Time {Seconds = 10},
					SortMethod = SortMethodType.SortByStartTime,
					HotKey = h,
					Position = i-1,
				});
			}
		}
	}
}
