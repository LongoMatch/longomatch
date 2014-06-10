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
using Gdk;
using Gtk;

using LongoMatch.Common;
using LongoMatch.Gui.Component;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Dialog
{


	public partial class TaggerDialog : Gtk.Dialog
	{
		TeamTemplate localTeamTemplate;
		TeamTemplate visitorTeamTemplate;
		bool subcategoryAdded;
		bool firstExpose;
		
		public TaggerDialog(Play play,
		                    Categories categoriesTemplate,
		                    TeamTemplate localTeamTemplate,
		                    TeamTemplate visitorTeamTemplate,
		                    bool showAllSubcategories)
		{
			this.Build();
			
			firstExpose = false;
			tagsnotebook.Visible = false;
			
			this.localTeamTemplate = localTeamTemplate;
			this.visitorTeamTemplate = visitorTeamTemplate;
			
			taggerwidget1.SetData(categoriesTemplate, play,
			                      localTeamTemplate.TeamName,
			                      visitorTeamTemplate.TeamName);
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
			
			if (!play.Category.TagFieldPosition && !play.Category.TagHalfFieldPosition && !play.Category.TagGoalPosition) {
				coordstagger.Visible = false;
				(mainvbox[hbox] as Gtk.Box.BoxChild).Expand = true;
			} else {
				coordstagger.LoadBackgrounds (categoriesTemplate.FieldBackground,
				                              categoriesTemplate.HalfFieldBackground,
				                              categoriesTemplate.GoalBackground);
				coordstagger.LoadPlay (play);
			}
			
			if (subcategoryAdded || playersnotebook.Visible) {
				tagsnotebook.Visible = true;
			}
		}
		
		public void AddTagSubcategory (SubCategory subcat, TagsStore tags){
			/* the notebook starts invisible */
			taggerwidget1.AddSubCategory(subcat, tags);
			subcategoryAdded = true;
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			bool ret = base.OnExposeEvent (evnt);
			
			if (!firstExpose) {
				Screen screen = Display.Default.DefaultScreen;
				int width, height, newWidth, newHeight;
				
				width = newWidth = Requisition.Width;
				height = newHeight = Requisition.Height;
				
				if (width + 20 > screen.Width) {
					newWidth = screen.Width - 20;
				}
				if (height + 20 > screen.Height) {
					newHeight = screen.Height - 20;
				}
				
				if (newWidth != width || newHeight != height) {
					ScrolledWindow win = new ScrolledWindow();
					VBox.Remove(mainvbox);
					win.AddWithViewport (mainvbox);
					win.Show ();
					VBox.PackStart (win, true, true, 0);
					this.Resize (newWidth, newHeight);
					this.SetPosition (Gtk.WindowPosition.CenterOnParent);
				}
				firstExpose = true;
			}
			return ret;
		}
	}
}
