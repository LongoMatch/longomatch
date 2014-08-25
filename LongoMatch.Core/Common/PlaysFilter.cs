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
		
		Dictionary<TaggerButton, List<Tag>> categoriesFilter;
		List<Player> playersFilter;
		Project project;
		
		public PlaysFilter (Project project)
		{
			this.project = project;
			categoriesFilter = new Dictionary<TaggerButton, List<Tag>>();
			playersFilter = new List<Player>(); 
			ClearAll();
			UpdateFilters();
		}
		
		public List<TaggerButton> VisibleCategories {
			get;
			protected set;
		}
		
		public List<Player> VisiblePlayers {
			get;
			protected set;
		}
		
		public List<Play> VisiblePlays {
			get;
			protected set;
		}
		
		public void ClearCategoriesFilter (bool update=true) {
			categoriesFilter.Clear();
			if (update)
				Update ();
		}
		
		public void ClearPlayersFilter (bool update=true) {
			playersFilter.Clear();
			if (update)
				Update ();
		}

		public void ClearAll (bool update=true) {
			ClearCategoriesFilter(false);
			ClearPlayersFilter(false);
			if (update)
				Update ();
		}
				
		public void FilterPlayer (Player player, bool visible) {
			if (visible) {
				if (!playersFilter.Contains(player))
					playersFilter.Add(player);
			} else {
				if (playersFilter.Contains(player))
					playersFilter.Remove(player);
			}
			Update();
		}
		
		public void FilterCategory (TaggerButton cat, bool visible) {
			if (visible) {
				if (!categoriesFilter.ContainsKey (cat))
					categoriesFilter[cat] = new List<Tag> ();
			} else {
				if (categoriesFilter.ContainsKey (cat))
					categoriesFilter.Remove (cat);
			}
			Update();
		}

		public void FilterCategoryTag (TaggerButton cat, Tag tag, bool visible) {
			List<Tag> tags;

			if (visible) {
				FilterCategory (cat, true);
				tags = categoriesFilter[cat];
				if (!tags.Contains (tag))
					tags.Add (tag);
			} else {
				if (categoriesFilter.ContainsKey(cat)) {
					tags = categoriesFilter[cat];
					if (tags.Contains (tag))
						tags.Remove (tag);
				}
			}
			Update();
		}
		
		public bool IsVisible(object o) {
			if (o is Player) {
				return VisiblePlayers.Contains(o as Player);
			} else if (o is Play) {
				return VisiblePlays.Contains (o as Play);
			}
			return true;
		}
		
		public void Update () {
			UpdateFilters();
			EmitFilterUpdated();
		}
		
		void UpdateFilters () {
			UpdateVisiblePlayers ();
			UpdateVisibleCategories ();
			UpdateVisiblePlays ();
		}
		
		void UpdateVisiblePlayers () {
			if (playersFilter.Count == 0) {
				VisiblePlayers = project.LocalTeamTemplate.List.Concat (project.VisitorTeamTemplate.List).ToList();
			} else {
				VisiblePlayers = playersFilter.ToList();
			}
		}
		
		void UpdateVisibleCategories () {
			if (categoriesFilter.Count == 0) {
				VisibleCategories = project.Categories.List;
			} else {
				VisibleCategories = categoriesFilter.Keys.ToList();
			}
		}
		
		void UpdateVisiblePlays () {
			bool cat_match=true, player_match=true;
			VisiblePlays = new List<Play>();
				
			foreach (Play play in project.Timeline) {
				cat_match = false;
				if (VisibleCategories.Contains (play.Category)) {
					cat_match = true;
					if (categoriesFilter.ContainsKey (play.Category)) {
						List<Tag> tags = categoriesFilter[play.Category];
						if (tags.Count == 0 || tags.Intersect (play.Tags).Count() > 0) {
							cat_match = true;
						} else {
							cat_match = false;
						}
					}
				}

				if (play.Players.Count == 0 && VisiblePlayers.Count == 
				    project.LocalTeamTemplate.List.Count + project.VisitorTeamTemplate.List.Count) {
					player_match = true;
				} else {
					player_match = VisiblePlayers.Intersect(play.Players).Count() != 0;
				}
				if (player_match && cat_match) {
					VisiblePlays.Add (play);
				}
			}
		}
		
		void EmitFilterUpdated () {
			if (FilterUpdated != null)
				FilterUpdated ();
		}
	}
}

