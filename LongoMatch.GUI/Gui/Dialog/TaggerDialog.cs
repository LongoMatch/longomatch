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
		
		public TaggerDialog(Play play,
		                    Categories categoriesTemplate,
		                    TeamTemplate localTeamTemplate,
		                    TeamTemplate visitorTeamTemplate,
		                    bool showAllSubcategories)
		{
			this.Build();
			
			tagsnotebook.Visible = false;
			
			this.localTeamTemplate = localTeamTemplate;
			this.visitorTeamTemplate = visitorTeamTemplate;
			
			taggerwidget1.SetData(play, localTeamTemplate.TeamName, visitorTeamTemplate.TeamName);
			playersnotebook.Visible = false;
			
			/* Iterate over all subcategories, adding a widget only for the FastTag ones */
			foreach (var subcat in play.Category.SubCategories) {
				if (!subcat.FastTag && !showAllSubcategories)
					continue;
				if (subcat is TagSubCategory) {
					var tagcat = subcat as TagSubCategory;
					AddTagSubcategory(tagcat, play.Tags);
				} else if (subcat is PlayerSubCategory) {
					playersnotebook.Visible = false;
					hbox.SetChildPacking(tagsnotebook, false, false, 0, Gtk.PackType.Start);
					var tagcat = subcat as PlayerSubCategory;
					AddPlayerSubcategory(tagcat, play.Players);
				} else if (subcat is TeamSubCategory) {
					var tagcat = subcat as TeamSubCategory;
					AddTeamSubcategory(tagcat, play.Teams,
					                   localTeamTemplate.TeamName,
					                   visitorTeamTemplate.TeamName);
				}
			}
			
			if (!play.Category.TagFieldPosition && !play.Category.TagGoalPosition) {
				poshbox.Visible = false;
				(vbox2[hbox] as Gtk.Box.BoxChild).Expand = true;
			} else {
				if (play.Category.TagFieldPosition) {
					AddFieldPosTagger (categoriesTemplate, play);				
				}
				if (play.Category.TagGoalPosition) {
					AddGoalPosTagger (categoriesTemplate, play);
				}
			}
		}
		
		public void AddTeamSubcategory (TeamSubCategory subcat, TeamsTagStore tags,
		                                string localTeam, string visitorTeam){
			/* the notebook starts invisible */
			tagsnotebook.Visible = true;
			taggerwidget1.AddTeamSubCategory(subcat, tags, localTeam, visitorTeam);
		}
		
		public void AddTagSubcategory (TagSubCategory subcat, StringTagStore tags){
			/* the notebook starts invisible */
			tagsnotebook.Visible = true;
			taggerwidget1.AddSubCategory(subcat, tags);
		}
		
		public void AddPlayerSubcategory (PlayerSubCategory subcat, PlayersTagStore tags){
			TeamTemplate local=null, visitor=null;
			
			/* the notebook starts invisible */
			playersnotebook.Visible = true;
			if (subcat.Contains(Team.LOCAL))
				local = localTeamTemplate;
			if (subcat.Contains(Team.VISITOR))
				visitor = visitorTeamTemplate;
			
			PlayersTaggerWidget widget = new PlayersTaggerWidget(subcat, local, visitor, tags);
			widget.Show();
			playersbox.PackStart(widget, true, true, 0);
		}
		
		public void AddFieldPosTagger (Categories categoriesTemplate, Play play) {
			List<Coordinates> coords = new List<Coordinates>();
			fieldcoordinatestagger.Visible = true;
			if (categoriesTemplate.FieldBackgroundImage != null) {
				fieldcoordinatestagger.Background = categoriesTemplate.FieldBackgroundImage.Value;
			} else {
				fieldcoordinatestagger.Background = Gdk.Pixbuf.LoadFromResource (Constants.FIELD_BACKGROUND);
			}
			if (play.FieldPosition != null) {
				coords.Add (play.FieldPosition);
			} else {
				Coordinates c = new Coordinates ();
				c.Add (new Point(100, 100));
				if (play.Category.FieldPositionIsDistance) {
					c.Add (new Point (300, 300));
				}
				coords.Add (c);
				play.FieldPosition = c;
			}
			fieldcoordinatestagger.Coordinates = coords;
			fieldcoordinatestagger.Visible = true; 
		}
		
		public void AddGoalPosTagger (Categories categoriesTemplate, Play play) {
			List<Coordinates> coords = new List<Coordinates>();
			goalcoordinatestagger.Visible = true;
			if (categoriesTemplate.GoalBackgroundImage != null) {
				goalcoordinatestagger.Background = categoriesTemplate.GoalBackgroundImage.Value;
			} else {
				goalcoordinatestagger.Background = Gdk.Pixbuf.LoadFromResource (Constants.GOAL_BACKGROUND);
			}
			if (play.GoalPosition != null) {
				coords.Add (play.GoalPosition);
			} else {
				Coordinates c = new Coordinates ();
				c.Add (new Point(100, 100));
				coords.Add (c);
				play.GoalPosition = c;
			}
			goalcoordinatestagger.Coordinates = coords; 
			goalcoordinatestagger.Visible = true;
		}

	}
}
