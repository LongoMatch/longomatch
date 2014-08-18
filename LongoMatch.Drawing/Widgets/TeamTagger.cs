//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store.Drawables;
using LongoMatch.Store.Templates;
using LongoMatch.Drawing.CanvasObject;
using LongoMatch.Store;
using LongoMatch.Handlers;

namespace LongoMatch.Drawing.Widgets
{
	public class TeamTagger: SelectionCanvas
	{
	
		public event PlayersPropertiesHandler PlayersSelectionChangedEvent;
		public event PlayersPropertiesHandler ShowMenuEvent;

		PlayersTaggerObject tagger;
		Point offset;
		MultiSelectionMode prevMode;
		bool inSubs;

		public TeamTagger (IWidget widget): base (widget)
		{
			Accuracy = 0;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			widget.SizeChangedEvent += HandleSizeChangedEvent;
			tagger = new PlayersTaggerObject ();
			Objects.Add (tagger);
		}

		public void LoadTeams (TeamTemplate homeTeam, TeamTemplate awayTeam, Image background)
		{
			tagger.LoadTeams (homeTeam, awayTeam, background);
			widget.ReDraw ();
		}

		public void Reload ()
		{
		}

		public void Select (Player p) {
		}
		
		protected override void ShowMenu (Point coords)
		{
		}

		void HandleSizeChangedEvent ()
		{
			tagger.Width = widget.Width;
			tagger.Height = widget.Height;
		}
	}
}

