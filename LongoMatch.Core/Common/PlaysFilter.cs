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
		Dictionary<Category, List<Tag>> categoriesFilter;
		List<Player> playersFilter;
		Project project;
		List<Category> visibleCategories;
		List<Play> visiblePlays;
		List<Player> visiblePlayers;
		
		public PlaysFilter (Project project)
		{
			this.project = project;
			categoriesFilter = new Dictionary<Category, List<Tag>>();
			playersFilter = new List<Player>(); 
			ClearAll();
			UpdateFilters();
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
			UpdateFilters();
			EmitFilterUpdated();
		}
		
		public void ClearCategoriesFilter () {
			categoriesFilter.Clear();
			foreach (Category cat in project.Categories.List) {
			}
		}
		
		public void ClearPlayersFilter () {
			playersFilter.Clear();
			foreach (var player in project.LocalTeamTemplate.List)
				playersFilter.Add(player);
			foreach (var player in project.VisitorTeamTemplate.List)
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
		
		public List<Category> VisibleCategories {
			get {
				return visibleCategories;
			}
		}
		
		public List<Player> VisiblePlayers {
			get {
				return visiblePlayers;
			}
		}
		
		public bool IsVisible(object o) {
			if (o is Player) {
				return visiblePlayers.Contains(o as Player);
			} else if (o is Play) {
				return visiblePlays.Contains (o as Play);
			}
			return true;
		}
		
		void UpdateFilters () {
			UpdateVisiblePlayers ();
			UpdateVisibleCategories ();
			UpdateVisiblePlays ();
		}
		
		void UpdateVisiblePlayers () {
			visiblePlayers = new List<Player>();
			foreach (Player p in project.LocalTeamTemplate.List.Concat (project.VisitorTeamTemplate.List)) {
				if (PlayersFilterEnabled && ! playersFilter.Contains (p)) {
					continue;
				}
				visiblePlayers.Add (p);
			}
		}
		
		void UpdateVisibleCategories () {
			visibleCategories = new List<Category>();
			foreach (var c in categoriesFilter.Keys) {
				bool visible = false;
				if (!CategoriesFilterEnabled) {
					visible = true;
				} else {
					foreach (var subcat in categoriesFilter[c]) {
					}
				}
				if (visible)
					visibleCategories.Add (c);
			}
		}
		
		void UpdateVisiblePlays () {
			bool cat_match=true, player_match=true;
			visiblePlays = new List<Play>();
				
			foreach (Play play in project.Timeline) {
				if (CategoriesFilterEnabled) {
					cat_match = false;
				}
				
				if (PlayersFilterEnabled)
					player_match = VisiblePlayers.Intersect(play.Players).Count() != 0;
				
				if (player_match && cat_match) {
					visiblePlays.Add (play);
				}
			}
		}
		
		void EmitFilterUpdated () {
			if (FilterUpdated != null)
				FilterUpdated ();
		}
	}
}

