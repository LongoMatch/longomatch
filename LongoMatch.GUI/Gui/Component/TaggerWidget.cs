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
using System.Collections.Generic;
using Gtk;

using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class TaggerWidget : Gtk.Bin
	{
		Play play;
		
		public TaggerWidget()
		{
			this.Build();
			table1.NColumns = 1;
			table1.NRows = 1;
		}
		
		public void SetData (Categories categories, Play play, string localTeam, string visitorTeam) {
			this.play = play;
			SetTeams(play, localTeam, visitorTeam);
			SetPeriod(categories.GamePeriods);
		}
		
		public void AddSubCategory(SubCategory subcat, TagsStore tags){
			if (subcat.Options.Count == 0)
				return;
			StringTaggerWidget tagger = new StringTaggerWidget(subcat, tags);
			table1.Attach(tagger,0, 1, table1.NRows-1, table1.NRows);
			table1.NRows ++;
			tagger.Show();
		}
		
		void SetTeams (Play play, string localTeam, string visitorTeam) {
			localcheckbutton.Label = localTeam;
			visitorcheckbutton.Label = visitorTeam;
			localcheckbutton.Active = play.Team == Team.LOCAL || play.Team == Team.BOTH;
			visitorcheckbutton.Active = play.Team == Team.VISITOR || play.Team == Team.BOTH;
			
			localcheckbutton.Toggled += OnCheckbuttonToggled;
			visitorcheckbutton.Toggled += OnCheckbuttonToggled;
		}
		
		void SetPeriod (List<string> gamePeriods) {
			RadioButton first = null, active = null;
			
			if (gamePeriods == null) {
				periodframe.Visible = false;
				return;
			}
			
			foreach (string period in gamePeriods) {
				RadioButton rb;
				
				if (first == null)
					first = rb = new RadioButton (period);
				else
					rb = new RadioButton (first, period);
					
				if (play.GamePeriod == period) {
					active = rb;
				}
				periodhbox.PackStart (rb);
				rb.Data["period"] = period;
				rb.Toggled += (sender, e) => {
					RadioButton b = sender as RadioButton;
					if (b.Active)
						play.GamePeriod = b.Data["period"] as String;
					};
			}
			if (active != null) {
				active.Active = true;
			} else {
				first.Active = true;
			}
			periodhbox.ShowAll();
		}
		
		protected void OnCheckbuttonToggled (object sender, System.EventArgs e)
		{
			if (visitorcheckbutton.Active && localcheckbutton.Active) {
				play.Team = Team.BOTH;
			} else {
				if (localcheckbutton.Active)
					play.Team = Team.LOCAL;
				else if (visitorcheckbutton.Active)
					play.Team = Team.VISITOR;
				else
					play.Team = Team.NONE;
			}
			Log.Debug("Team tagged: " + play.Team);
		}
	}
}