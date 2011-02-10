//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using LongoMatch.Common;

namespace LongoMatch.Store.Templates
{
	[Serializable]

	public class TeamTemplate: List<Player>
	{
		public TeamTemplate() {}

		public String TeamName {
			get;
			set;
		}

		public List<Player> PlayingPlayersList {
			get {
				var players =
				        from player in this
				        where player.Playing == true
				        select player;
				return players.ToList() as List<Player>;
			}
		}

		public void Save(string filePath) {
			SerializableObject.Save(this, filePath);
		}

		public static TeamTemplate Load(string filePath) {
			return SerializableObject.Load<TeamTemplate>(filePath);
		}

		public static TeamTemplate DefaultTemplate(int playersCount) {
			TeamTemplate defaultTemplate = new TeamTemplate();
			defaultTemplate.FillDefaultTemplate(playersCount);
			return defaultTemplate;
		}

		private void FillDefaultTemplate(int playersCount) {
			Clear();
			for(int i=1; i<=playersCount; i++) {
				Add(new Player {
					Name = "Player " + i,
					Birthday = new DateTime(),
					Height = 1.80f,
					Weight = 80,
					Number = i,
					Position = "",
					Photo = null,
					Playing = true,
				});
			}
		}
	}
}
