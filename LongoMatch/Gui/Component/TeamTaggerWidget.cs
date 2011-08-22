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
using Gtk;
using LongoMatch.Common;
using LongoMatch.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TeamTaggerWidget : Gtk.Bin
	{
		private Dictionary<TeamTag, CheckButton> dict;
		private TeamSubCategory subcat;
		private TeamsTagStore tags;
		private string subcategory, localTeam, visitorTeam;
		
		
		public TeamTaggerWidget (TeamSubCategory subcat, TeamsTagStore tags, 
		                         string localTeam, string visitorTeam)
		{
			this.Build ();
			this.subcat = subcat;
			this.tags = tags;
			this.localTeam = localTeam;
			this.visitorTeam = visitorTeam;
			Title = subcat.Name;
			dict = new Dictionary<TeamTag, CheckButton>();
			AddTagWidget(new TeamTag{Value=Team.LOCAL, SubCategory=subcat});
			AddTagWidget(new TeamTag{Value=Team.VISITOR, SubCategory=subcat});
			UpdateTags();
		}
		
		private void UpdateTags () {
			foreach (var tag in tags.GetTags(subcat)) {
				if (dict.ContainsKey(tag)) 	
					dict[tag].Active = true;
			}
		}
		
		private void AddTagWidget (TeamTag tag){
			CheckButton button = new CheckButton(tag.Value == Team.LOCAL ? localTeam : visitorTeam  );
			button.Toggled += delegate(object sender, EventArgs e) {
				if (button.Active) {
					tags.Add(tag);
				} else
					tags.Remove(tag);
			};
			dict.Add(tag, button);
			buttonsbox.PackStart(button, false, false, 0);
			button.ShowAll();
		} 
		
		private string Title {
			set {
				titlelabel.Markup = "<b>" + value + "</b>";
			}
		}
	}
}

