//
//  Copyright (C) 2013 Andoni Morales Alastruey
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

namespace LongoMatch.Services.Services
{
		public class ProjectOptionsManager
		{
				public ProjectOptionsManager ()
				{
				}
				
		protected void OnTagSubcategoriesActionToggled (object sender, System.EventArgs e)
		{
			if (TagSubcategoriesChangedEvent != null)
				TagSubcategoriesChangedEvent (!TagSubcategoriesAction.Active);
			Config.FastTagging = !TagSubcategoriesAction.Active;
		}

		protected virtual void OnFullScreenActionToggled(object sender, System.EventArgs e)
		{
			playercapturer.FullScreen = (sender as Gtk.ToggleAction).Active;
		}

		protected virtual void OnPlaylistActionToggled(object sender, System.EventArgs e)
		{
			bool visible = (sender as Gtk.ToggleAction).Active;
			SetPlaylistVisibility (visible);
			playsSelection.PlayListLoaded=visible;
		}

		protected virtual void OnHideAllWidgetsActionToggled(object sender, System.EventArgs e)
		{
			ToggleAction action = sender as ToggleAction;
			
			if(openedProject == null)
				return;
			
			leftbox.Visible = !action.Active;
			timeline.Visible = !action.Active && TimelineViewAction.Active;
			buttonswidget.Visible = !action.Active &&
				(TaggingViewAction.Active || ManualTaggingViewAction.Active);
			if (Config.UseGameUnits) {
				guTimeline.Visible = !action.Visible && GameUnitsViewAction.Active;
				gameunitstaggerwidget1.Visible = !action.Active && (GameUnitsViewAction.Active || 
					TaggingViewAction.Active || ManualTaggingViewAction.Active);
			}
			if(action.Active) {
				SetTagsBoxVisibility (false);
			} else {
				if (selectedTimeNode != null)
					SetTagsBoxVisibility (true);
			}
		}

		protected virtual void OnViewToggled(object sender, System.EventArgs e)
		{
			ToggleAction action = sender as Gtk.ToggleAction;
			
			if (!action.Active)
				return;
			
			buttonswidget.Visible = action == ManualTaggingViewAction || sender == TaggingViewAction;
			timeline.Visible = action == TimelineViewAction;
			if (Config.UseGameUnits) {
				guTimeline.Visible = action == GameUnitsViewAction;
				gameunitstaggerwidget1.Visible = buttonswidget.Visible || guTimeline.Visible;
			}
			if(action == ManualTaggingViewAction)
				buttonswidget.Mode = TagMode.Free;
			else
				buttonswidget.Mode = TagMode.Predifined;
		}
		}
}

