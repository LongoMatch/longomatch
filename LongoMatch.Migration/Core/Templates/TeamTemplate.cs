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
using LongoMatch.Core;
using Newtonsoft.Json;

using LongoMatch.Common;
using LongoMatch.Interfaces;
using VAS.Core;

namespace LongoMatch.Store.Templates
{
	[Serializable]
	[JsonObject]
	public class TeamTemplate: List<Player>, ITemplate<Player>
	{
		[JsonProperty ("ID")]
		private Guid _UUID;

		private byte[] thumbnailBuf;
		private const int MAX_WIDTH = 100;
		private const int MAX_HEIGHT = 100;
		Version version;

		public TeamTemplate ()
		{
			init (Guid.NewGuid ());
		}

		public TeamTemplate (Guid uuid)
		{
			init (uuid);
		}

		[JsonIgnore]
		public Guid UUID {
			get {
				return _UUID;
			}
			set {
				_UUID = value;
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

		public List<Player> List {
			get {
				return this.ToList ();
			}
		}

		public Image Shield {
			get {
				if (thumbnailBuf != null)
					return Image.Deserialize (thumbnailBuf);
				else
					return null;
			}
			set {
				if (value == null)
					thumbnailBuf = null;
				else
					thumbnailBuf = value.Serialize ();
			}
		}

		[JsonIgnore]
		public int PlayingPlayers {
			get;
			protected set;
		}

		public int[] Formation {
			get;
			set;
		}

		[JsonIgnore]
		public string FormationStr {
			set {
				string[] elements = value.Split ('-');
				int[] tactics = new int[elements.Length];
				int index = 0;
				foreach (string s in elements) {
					try {
						tactics [index] = int.Parse (s);
						index++;
					} catch {
						throw new FormatException ();
					}
				}
				PlayingPlayers = tactics.Sum ();
				Formation = tactics;
			}
			get {
				return String.Join ("-", Formation);
			}
		}

		[JsonIgnore]
		public List<Player> PlayingPlayersList {
			get {
				return this.Where (p => p.Playing).Select (p => p).ToList ();
			}
		}

		public void Save (string filePath)
		{
			SerializableObject.Save (this, filePath);
		}

		public Player AddDefaultItem (int i)
		{
			Player p = new Player {
				Name = "Player " + (i + 1).ToString (),
				Birthday = new DateTime (),
				Height = 1.80f,
				Weight = 80,
				Number = i + 1,
				Position = "",
				Photo = null,
				Playing = true,
			};
			Insert (i, p);
			return p;
		}

		public static TeamTemplate Load (string filePath)
		{
			TeamTemplate template = SerializableObject.LoadSafe<TeamTemplate> (filePath);
			if (template.Formation == null) {
				template.FormationStr = "1-4-3-3";
			}
			return template;
		}

		public static TeamTemplate DefaultTemplate (int playersCount)
		{
			TeamTemplate defaultTemplate = new TeamTemplate ();
			defaultTemplate.FillDefaultTemplate (playersCount);
			return defaultTemplate;
		}

		private void FillDefaultTemplate (int playersCount)
		{
			Clear ();
			for (int i = 1; i <= playersCount; i++)
				AddDefaultItem (i - 1);
		}

		void init (Guid uuid)
		{
			_UUID = uuid;
			TeamName = Catalog.GetString ("Team");
			if (Formation == null) {
				FormationStr = "1-4-3-3";
			}
		}
	}
}
