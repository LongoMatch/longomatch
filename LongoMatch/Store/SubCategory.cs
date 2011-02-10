//
//  Copyright (C) 2011 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Collections.Generic;
using LongoMatch.Common;

namespace LongoMatch.Store
{
	/// <summary>
	/// A sub category is used to extend the tags of a category.
	/// In a complex analysis scenario, a category is not enough to tag
	/// a play and we need to use subcategories. For example we might want to
	/// tag the type of goal, who scored, who did the passs and for which team.
	///   * Goal
	///     - Type: [Short Corner, Corner, Penalty, Penalty Corner, Field Goal]
	///     - Scorer: Players List
	///     - Assister: Players List
	///     - Team: [Local Team, Visitor Team]
	///
	/// A sub category with name Type and a list of options will be added to the
	/// Goal category to extends its tags.
	/// </summary>
	public class SubCategory
	{
		public SubCategory() {
			Options = new List<object>();
		}

		/// <summary>
		/// Name of the subcategory
		/// </summary>
		public String Name {
			get;
			set;
		}

		/// <summary>
		/// List of available options for the subcategory
		/// </summary>
		public List<object> Options {
			get;
			set;
		}

		/// <summary>
		/// Wheter this subcategory allow multiple options.
		/// eg: Team will only allow one option, because a goal can't be scored by 2 teams
		/// </summary>
		public bool AllowMultiple {
			get;
			set;
		}

		/// <summary>
		/// Whether this SubCategory should be added to the tagging widget shown after
		/// creating a new play.
		/// </summary>
		public bool FastTag {
			get;
			set;
		}
	}

	public class TagSubCategory: SubCategory
	{
		public TagSubCategory() {
			Options = new List<string>();
		}

		public new List<string> Options {
			get;
			set;
		}
	}

	/// <summary>
	/// SubCategory to tag Players
	/// </summary>
	public class PlayerSubCategory: SubCategory
	{
		public PlayerSubCategory() {
			Options = new List<Team>();
		}

		/// <summary>
		/// A list of options containing the teams to be shown in the options.
		/// The teams LOCAL, VISITOR will be then mapped to a list of players
		/// for this team, so that a change in the team's templates will not
		/// affect the list of available players.
		/// </summary>
		public new List<Team> Options {
			get;
			set;
		}
	}

	/// <summary>
	/// SubCategory to tag teams
	/// </summary>
	public class TeamSubCategory: SubCategory
	{
		public TeamSubCategory() {
			Options = new List<Team>();
		}

		/// <summary>
		/// A list of options containing the teams to be shown in the options.
		/// The teams LOCAL, VISITOR and NONE are then mapped to real team names
		/// so that a change in the name doesn't affect the category.
		/// </summary>
		public new List<Team> Options {
			get;
			set;
		}
	}
}

