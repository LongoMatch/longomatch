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
using Mono.Unix;
using Newtonsoft.Json;

using LongoMatch.Common;
using LongoMatch.Interfaces;

namespace LongoMatch.Store.Templates
{
	[Serializable]
	[JsonObject]
	public class TeamTemplate: List<Player>, ITemplate<Player>
	{
		private byte[] thumbnailBuf;
		private const int MAX_WIDTH=100;
		private const int MAX_HEIGHT=100;
		Version version;
		
		public TeamTemplate() {
			TeamName = Catalog.GetString("Team");
			if (PlayingPlayers == 0) {
				PlayingPlayers = 11;
			}
			if (Formation == null) {
				Formation = new List<int>(new int[] {1, 4, 3, 3});
			}
		}

		public String Name {
			get;
			set;
		}

		public String TeamName {
			get;
			set;
		}
		
		public Version Version {
			get;
			set;
		}
		
		public Image Shield {
			get {
				if(thumbnailBuf != null)
					return Image.Deserialize(thumbnailBuf);
				else return null;
			} set {
				if (value == null)
					thumbnailBuf = null;
				else
					thumbnailBuf = value.Serialize();
			}
		}
		
		public int PlayingPlayers {
			get;
			set;
		} 
		
		public List<int> Formation {
			get;
			set;
		}
		
		public List<Player> PlayingPlayersList {
			get {
				return this.Where(p=>p.Playing).Select(p=>p).ToList();
			}
		}

		public void Save(string filePath) {
			SerializableObject.Save(this, filePath);
		}
		
		public Player AddDefaultItem (int i) {
			Player p = new Player {
				Name = "Player " + (i+1).ToString(),
				Birthday = new DateTime(),
				Height = 1.80f,
				Weight = 80,
				Number = i+1,
				Position = "",
				Photo = null,
				Playing = true,};
			Insert (i, p);
			return p;
		}

		public static TeamTemplate Load(string filePath) {
			return SerializableObject.LoadSafe<TeamTemplate>(filePath);
		}

		public static TeamTemplate DefaultTemplate(int playersCount) {
			TeamTemplate defaultTemplate = new TeamTemplate();
			defaultTemplate.FillDefaultTemplate(playersCount);
			return defaultTemplate;
		}

		private void FillDefaultTemplate(int playersCount) {
			Clear();
			for(int i=1; i<=playersCount; i++)
				AddDefaultItem(i-1);
			version = new Version (Constants.DB_MAYOR_VERSION, Constants.DB_MINOR_VERSION);
		}
	}
}
