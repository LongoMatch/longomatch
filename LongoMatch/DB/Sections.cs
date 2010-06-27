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
using Gdk;
using LongoMatch.TimeNodes;

namespace LongoMatch.DB
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
	public class Sections
	{
		private List<SectionsTimeNode> sectionsList;

		//These fields are not used but must be kept for DataBase compatiblity
#pragma warning disable 0169
		private Color[] colorsArray;
		private SectionsTimeNode[] timeNodesArray;
#pragma warning restore 0169

		/// <summary>
		/// Creates a new template
		/// </summary>
		public Sections()
		{
			this.sectionsList = new List<SectionsTimeNode>();
		}

		/// <summary>
		/// Clear the template
		/// </summary>
		public void Clear() {
			sectionsList.Clear();
		}

		/// <summary>
		/// Adds a new analysis category to the template
		/// </summary>
		/// <param name="tn">
		/// A <see cref="SectionsTimeNode"/>: category to add
		/// </param>
		public void AddSection(SectionsTimeNode tn) {
			sectionsList.Add(tn);
		}

		/// <summary>
		/// Adds a new category to the template at a given position
		/// </summary>
		/// <param name="tn">
		/// A <see cref="SectionsTimeNode"/>: category to add
		/// </param>
		/// <param name="index">
		/// A <see cref="System.Int32"/>: position
		/// </param>
		public void AddSectionAtPos(SectionsTimeNode tn, int index) {
			sectionsList.Insert(index,tn);
		}

		/// <summary>
		/// Delete a category from the templates using the it's index
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/>: position of the category to delete
		/// </param>
		public void RemoveSection(int index) {
			sectionsList.RemoveAt(index);
		}

		//// <value>
		/// Number of categories
		/// </value>
		public int Count {
			get {
				return sectionsList.Count;
			}
		}

		//// <value>
		/// Ordered list with all the categories
		/// </value>
		public List<SectionsTimeNode> SectionsTimeNodes {
			set {
				sectionsList.Clear();
				sectionsList = value;
			}
			get {
				return sectionsList;
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
		public SectionsTimeNode GetSection(int section) {
			return sectionsList[section];
		}

		/// <summary>
		/// Returns an array if strings with the categories names
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string[] GetSectionsNames() {
			int count = sectionsList.Count;
			string[] names = new string[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++) {
				tNode = sectionsList[i];
				names[i]=tNode.Name;
			}
			return names;
		}

		/// <summary>
		/// Returns an array of the categories' color
		/// </summary>
		/// <returns>
		/// A <see cref="Color"/>
		/// </returns>
		public Color[] GetColors() {
			int count = sectionsList.Count;
			Color[] colors = new Color[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++) {
				tNode = sectionsList[i];
				colors[i]=tNode.Color;
			}
			return colors;
		}

		/// <summary>
		/// Return an array of the hotkeys for this template
		/// </summary>
		/// <returns>
		/// A <see cref="HotKey"/>
		/// </returns>
		public HotKey[] GetHotKeys() {
			int count = sectionsList.Count;
			HotKey[] hotkeys = new HotKey[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++) {
				tNode = sectionsList[i];
				hotkeys[i]=tNode.HotKey;
			}
			return hotkeys;
		}

		/// <summary>
		/// Returns an array with the default start times
		/// </summary>
		/// <returns>
		/// A <see cref="Time"/>
		/// </returns>
		public Time[] GetSectionsStartTimes() {
			int count = sectionsList.Count;
			Time[] startTimes = new Time[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++) {
				tNode = sectionsList[i];
				startTimes[i]=tNode.Start;
			}
			return startTimes;
		}

		/// <summary>
		/// Returns an array with the defaul stop times
		/// </summary>
		/// <returns>
		/// A <see cref="Time"/>
		/// </returns>
		public Time[] GetSectionsStopTimes() {
			int count = sectionsList.Count;
			Time[] stopTimes = new Time[count];
			SectionsTimeNode tNode;
			for (int i=0; i<count; i++) {
				tNode = sectionsList[i];
				stopTimes[i]=tNode.Start;
			}
			return stopTimes;
		}

		/// <summary>
		/// Returns a category at a given position
		/// </summary>
		/// <param name="section">
		/// A <see cref="System.Int32"/>: position in the list
		/// </param>
		/// <returns>
		/// A <see cref="SectionsTimeNode"/>
		/// </returns>
		public SectionsTimeNode GetTimeNode(int section) {
			return sectionsList [section];
		}

		/// <summary>
		/// Returns the name for a category at a given position
		/// </summary>
		/// <param name="section">
		/// A <see cref="System.Int32"/>: position in the list
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>: name of the category
		/// </returns>
		public string GetName(int section) {
			return sectionsList[section].Name;
		}

		/// <summary>
		/// Returns the start time for a category at a given position
		/// </summary>
		/// <param name="section">
		/// A <see cref="System.Int32"/>: position in the list
		/// </param>
		/// <returns>
		/// A <see cref="Time"/>: start time
		/// </returns>
		public Time GetStartTime(int section) {
			return sectionsList[section].Start;
		}

		/// <summary>
		/// Returns the stop time for a category at a given position
		/// </summary>
		/// <param name="section">
		/// A <see cref="System.Int32"/>: position in the list
		/// </param>
		/// <returns>
		/// A <see cref="Time"/>: stop time
		/// </returns>
		public Time GetStopTime(int section) {
			return sectionsList[section].Stop;
		}

		/// <summary>
		/// Return the color for a category at a given position
		/// </summary>
		/// <param name="section">
		/// A <see cref="System.Int32"/>: position in the list
		/// </param>
		/// <returns>
		/// A <see cref="Color"/>: color
		/// </returns>
		public Color GetColor(int section) {
			return sectionsList[section].Color;
		}

		/// <summary>
		/// Returns the hotckey for a category at a given position
		/// </summary>
		/// <param name="section">
		/// A <see cref="System.Int32"/>: position in the list
		/// </param>
		/// <returns>
		/// A <see cref="HotKey"/>: hotkey for this category
		/// </returns>
		public HotKey GetHotKey(int section) {
			return sectionsList[section].HotKey;
		}
	}
}
