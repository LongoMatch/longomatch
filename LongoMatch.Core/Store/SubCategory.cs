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
using Mono.Unix;
using Newtonsoft.Json;

using LongoMatch.Common;
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
	public class SubCategory
	{

		public SubCategory() {
			Name = "";
			AllowMultiple = true;
			Options = new List<string>();
		}

		/// <summary>
		/// Name of the subcategory
		/// </summary>
		public String Name {
			get;
			set;
		}
		
		/// <summary>
		/// Gets or sets the list of available options.
		/// </summary>
		/// <value>The options.</value>
		public List<string> Options {
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

		protected string RenderDesc(string type, string values) {
			string str;
			
			str = String.Format("{0}: {1} [{2}]\n", 
			                    Catalog.GetString("Name"), Name, type);
			str += values;
			return str;
		}
		
		public virtual string ToMarkupString(){
			string tags;
			
			tags = String.Join (" - ", Options);
			return RenderDesc (Catalog.GetString("Tags list"),
			                  Catalog.GetString("Tags:" + 
			                  String.Format(" <b>{0}</b>", tags)));
		}
	}
}