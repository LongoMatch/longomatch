//
//  Copyright (C) 2009 Andoni Morales Alastruey
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
using System.Linq;
using System.Collections.Generic;

using LongoMatch.Common;
using LongoMatch.Gui.Component;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Dialog
{


	public partial class TaggerDialog : Gtk.Dialog
	{
		private TeamTemplate localTeamTemplate;
		private TeamTemplate visitorTeamTemplate;

		public TaggerDialog(Category cat, StringTagStore tags, PlayersTagStore players,
		                    TeamTemplate localTeamTemplate, TeamTemplate visitorTeamTemplate)
		{
			this.Build();
			
			tagsnotebook.Visible = false;
			
			this.localTeamTemplate = localTeamTemplate;
			this.visitorTeamTemplate = visitorTeamTemplate;
			
			/* Iterate over all subcategories, adding a widget only for the FastTag ones */
			foreach (var subcat in cat.SubCategories) {
				if (subcat is TagSubCategory) {
					var tagcat = subcat as TagSubCategory;
					if (tagcat.FastTag)
						AddTagSubcategory(tagcat, tags.GetTags(tagcat));
				} else if (subcat is PlayerSubCategory) {
					var tagcat = subcat as PlayerSubCategory;
					if (tagcat.FastTag)
						AddPlayerSubcategory(tagcat, players.GetTags(tagcat));
				}
			}
		}
		
		public void AddTagSubcategory (TagSubCategory subcat, List<StringTag> tags){
			/* the notebook starts invisible */
			tagsnotebook.Visible = true;
			taggerwidget1.AddSubCategory(subcat, tags);
		}
		
		public void AddPlayerSubcategory (PlayerSubCategory subcat, List<PlayerTag> tags){
			TeamTemplate template;
			
			/* the notebook starts invisible */
			playersnotebook.Visible = true;
			if (subcat.Contains(Team.LOCAL))
				template = localTeamTemplate;
			/* FIXME: Add support for subcategories with both teams */
			//else if (subcat.Contains(Team.VISITOR))
			else
				template = visitorTeamTemplate;
			
			PlayersTaggerWidget widget = new PlayersTaggerWidget(subcat.Name, subcat.AllowMultiple,
			                                                     template, tags);
			widget.Show();
			playersbox.PackStart(widget, false, true, 0);
		}

	}
}
