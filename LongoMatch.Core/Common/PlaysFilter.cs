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

using LongoMatch.Handlers;
using LongoMatch.Store;

namespace LongoMatch.Common
{
	public class PlaysFilter
	{
		
		public event FilterUpdatedHandler FilterUpdated;
		
		List<Category> categoriesFilter;
		List<Player> playersFilter;
		Project project;
		
		public PlaysFilter (Project project)
		{
			this.project = project;
			categoriesFilter = new List<Category>();
			playersFilter = new List<Player>(); 
			ClearAll();
		}
		
		public void Update () {
			EmitFilterUpdated();
		}
		
		public void ClearPlayersFilter () {
			categoriesFilter.Clear();
			foreach (var cat in project.Categories)
				categoriesFilter.Add(cat);
		}
		
		public void ClearCategoriesFilter () {
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
		
		public void FilterCategory (Category category) {
			categoriesFilter.Remove(category);
		}
		
		public void UnFilterCategory(Category category) {
			if (!categoriesFilter.Contains(category))
				categoriesFilter.Add(category);
		}
		
		public List<Category> VisibleCategories {
			get {
				return categoriesFilter;
			}
		}
		
		public List<Player> VisiblePlayers {
			get {
				return playersFilter;
			}
		}
		
		public bool IsVisible(object o) {
			if (o is Player) {
				return VisiblePlayers.Contains(o as Player);
			} else if (o is Category) {
				return VisibleCategories.Contains(o as Category);
			} else if (o is Play) {
				bool cat_match, player_match;
				Play play = o as Play;
				
				cat_match = VisibleCategories.Contains(play.Category);
				if (!cat_match)
					return false;
				
				if (play.Players.Tags.Count == 0)
					player_match = true;
				else
					player_match = VisiblePlayers.Intersect(play.Players.GetTagsValues()).Count() != 0;
				
				return player_match && cat_match;
			} else {
				return false;
			}
		}
		
		void EmitFilterUpdated () {
			if (FilterUpdated != null)
				FilterUpdated ();
		}
	}
}

