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
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.ToolboxItem(true)]
	public partial class TaggerWidget : Gtk.Bin
	{
		public TaggerWidget()
		{
			this.Build();
			table1.NColumns = 1;
			table1.NRows = 1;
		}
		
		public void AddSubCategory(TagSubCategory subcat, StringTagStore tags){
			StringTaggerWidget tagger = new StringTaggerWidget(subcat, tags);
			table1.Attach(tagger,0, 1, table1.NRows-1, table1.NRows);
			table1.NRows ++;
			tagger.Show();
		}
		
		public void AddTeamSubCategory(TeamSubCategory subcat, TeamsTagStore tags,
		                               string localTeam, string visitorTeam){
			TeamTaggerWidget tagger = new TeamTaggerWidget(subcat, tags,
			                                               localTeam, visitorTeam);
			table1.Attach(tagger,0, 1, table1.NRows-1, table1.NRows);
			table1.NRows ++;
			tagger.Show();
		}
	}
}