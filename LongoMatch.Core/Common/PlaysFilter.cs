// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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

using LongoMatch.Interfaces;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Common
{
	public class PlaysFilter
	{
		
		public event FilterUpdatedHandler FilterUpdated;
		
		bool playersFiltered, categoriesFiltered;
		Dictionary<Category, List<SubCategoryTags>> categoriesFilter;
		List<Player> playersFilter;
		Project project;
		
		public PlaysFilter (Project project)
		{
			this.project = project;
			categoriesFilter = new Dictionary<Category, List<SubCategoryTags>>();
			playersFilter = new List<Player>(); 
			ClearAll();
		}
		
		public bool PlayersFilterEnabled {
			get {
				return  playersFiltered;
			}
			set {
				playersFiltered = value;
				Update();
			}
		}
		
		public bool CategoriesFilterEnabled {
			get {
				return  categoriesFiltered;
			}
			set {
				categoriesFiltered = value;
				Update();
			}
		}
		
		public void Update () {
			EmitFilterUpdated();
		}
		
		public void ClearCategoriesFilter () {
			categoriesFilter.Clear();
			foreach (var cat in project.Categories) {
				List<SubCategoryTags> list = new List<SubCategoryTags>(); 
				categoriesFilter.Add(cat, list);
				foreach (var subcat in cat.SubCategories) {
					if (subcat is TagSubCategory) {
					SubCategoryTags subcatTags = new SubCategoryTags{SubCategory = subcat};
					list.Add(subcatTags);
					foreach (var option in subcat as TagSubCategory)
						subcatTags.Add (option);
					}
				}
			}
		}
		
		public void ClearPlayersFilter () {
			playersFilter.Clear();
			foreach (var player in project.LocalTeamTemplate)
				playersFilter.Add(player);
			foreach (var player in project.VisitorTeamTemplate)
				playersFilter.Add(player);
		}

		public void ClearAll () {
			ClearCategoriesFilter();
			ClearPlayersFilter();
		}
				
		public void FilterPlayer (Player player) {
			playersFilter.Remove(player);
		}
		
		public void UnFilterPlayer(Player player) {
			if (!playersFilter.Contains(player))
				playersFilter.Add(player);
		}
		
		public void FilterSubCategory (Category cat, ISubCategory subcat, string option, bool filtered) {
			SubCategoryTags tsub = categoriesFilter[cat].Find(s => s.SubCategory == subcat);
			if (filtered) {
				tsub.Add(option);
			} else {
				tsub.Remove(option);
			}
		}
		
		public List<Category> VisibleCategories {
			get {
				return categoriesFilter.Keys.ToList();
			}
		}
		
		public List<Player> VisiblePlayers {
			get {
				return playersFilter;
			}
		}
		
		public bool IsVisible(object o) {
			if (o is Player) {
				if (PlayersFilterEnabled)
					return VisiblePlayers.Contains(o as Player);
				else
					return true;
			} else if (o is Play) {
				bool cat_match=true, player_match=true;
				Play play = o as Play;
				
				if (CategoriesFilterEnabled) {
					cat_match = false;
					foreach (var subcat in categoriesFilter[play.Category]) {
						bool match = false;
						foreach (var option in subcat) {
							StringTag tag = new StringTag{SubCategory=subcat.SubCategory, Value=option};
							if (play.Tags.Contains(tag)) {
								match = true;
								break;
							}
						}
						/* A single match in a subcategory is not enough as we want to filter
						 * all plays that have Period=1 and Type=Stroke. So if there is a match
						 * for Period we want a match for Type too */
						if (!match && subcat.Count != 0) {
							cat_match = false;
							break;
						}
						if (match)
							cat_match = true;
					}
				}
				
				if (PlayersFilterEnabled)
					player_match = VisiblePlayers.Intersect(play.Players.GetTagsValues()).Count() != 0;
				
				return player_match && cat_match;
			}
			return true;
		}
		
		void EmitFilterUpdated () {
			if (FilterUpdated != null)
				FilterUpdated ();
		}
	}
	
	class SubCategoryTags: List<string> {
		public ISubCategory SubCategory {
			get;
			set;
		}
	}
}

