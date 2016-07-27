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
using System.Linq;
using LongoMatch.Common;
using LongoMatch.Core;
using LongoMatch.Interfaces;

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
	[Serializable]
	public class SubCategory<T>: List<T>, ISubCategory
	{

		public SubCategory ()
		{
			Name = "";
			AllowMultiple = true;
			FastTag = true;
		}

		public SubCategory (IEnumerable<T> list) : base (list)
		{
		}

		/// <summary>
		/// Name of the subcategory
		/// </summary>
		public String Name {
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

		protected string RenderDesc (string type, string values)
		{
			string str;
			
			str = String.Format ("{0}: {1} [{2}]\n", 
				Catalog.GetString ("Name"), Name, type);
			str += values;
			return str;
		}

		public virtual string ToMarkupString ()
		{
			return this.ToString ();
		}

		public List<string> ElementsDesc ()
		{
			return this.Select (e => e.ToString ()).ToList ();
		}
	}

	[Serializable]
	public class SubCategory
	{
		public SubCategory ()
		{
		}

		public String Name { get; set; }

		public List<string> Options { get; set; }

		public bool AllowMultiple { get; set; }
	}

	[Serializable]
	public class TagSubCategory: SubCategory<string>
	{
	
		public TagSubCategory ()
		{
		}

		public TagSubCategory (IEnumerable<string> tags) : base (tags)
		{
		}

		public override string ToMarkupString ()
		{
			string tags = "";
			
			foreach (string tag in this) {
				if (tags == "")
					tags += tag;
				else
					tags += " - " + tag;
			}
			return RenderDesc (Catalog.GetString ("Tags list"),
				Catalog.GetString ("Tags:" +
				String.Format (" <b>{0}</b>", tags)));
		}
		
	}

	/// <summary>
	/// SubCategory to tag Players
	/// Stores a list of teams to be shown in the options.
	/// The teams LOCAL, VISITOR will be then mapped to a list of players
	/// for this team, so that a change in the team's templates will not
	/// affect the list of available players.
	/// </summary>
	[Serializable]
	public class PlayerSubCategory: SubCategory<Team>
	{
	
		public bool PositionFilter { get; set; }

		public override string ToMarkupString ()
		{
			string teams = "";
			if (this.Contains (Team.LOCAL))
				teams += Catalog.GetString ("Local ");
			if (this.Contains (Team.VISITOR))
				teams += Catalog.GetString ("Visitor");
			
			return RenderDesc (Catalog.GetString ("List of players"),
				Catalog.GetString ("Teams:" +
				String.Format (" <b>{0}</b>", teams)));
		}
	}

	/// <summary>
	/// SubCategory to tag teams
	/// A list of options containing the teams to be shown in the options.
	/// The teams LOCAL, VISITOR and NONE are then mapped to real team names
	/// so that a change in the name doesn't affect the category.
	/// </summary>
	[Serializable]
	public class TeamSubCategory: SubCategory<Team>
	{
	
		public TeamSubCategory ()
		{
			Name = Catalog.GetString ("Team");
			AllowMultiple = true;
			FastTag = true;
			Add (Team.LOCAL);
			Add (Team.VISITOR);
		}

		public override string ToMarkupString ()
		{
			return RenderDesc (Catalog.GetString ("Team selection"), "");
		}
	}

	/// <summary>
	/// SubCategory to tag locations or trajectories in the game field
	/// </summary>
	[Serializable]
	public class CoordinatesSubCategory: SubCategory<Coordinates>
	{
	
		/* Use 1 for locations or 2 for trajectories */
		public int NumCoordinates {
			get;
			set;
		}

		public override string ToMarkupString ()
		{
			return RenderDesc (Catalog.GetString ("Coordinates"), "");
		}
	}
}