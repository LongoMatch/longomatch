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
using System.IO;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Serialization;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LongoMatch.Core.Store.Templates
{
	[Serializable]
	public class Team: StorableBase, ITemplate<Team>
	{
		const int MAX_WIDTH = 100;
		const int MAX_HEIGHT = 100;
		ObservableCollection<Player> list;

		public Team ()
		{
			TeamName = Catalog.GetString ("Team");
			FormationStr = "1-4-3-3";
			ID = Guid.NewGuid ();
			List = new ObservableCollection<Player> ();
			try {
				Shield = Resources.LoadImage (StyleConf.DefaultShield);
			} catch {
				/* Ignore for unit tests */
			}
			ActiveColor = 0;
			Colors = new Color [2];
			Colors [0] = Color.Blue1;
			Colors [1] = Color.Red1;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool Static {
			get;
			set;
		}

		public ObservableCollection<Player> List {
			get {
				return list;
			}
			set {
				if (list != null) {
					list.CollectionChanged -= PlayersChanged;
				}
				list = value;
				if (list != null) {
					list.CollectionChanged += PlayersChanged;
				}
			}
		}

		[LongoMatchPropertyIndex (0)]
		[LongoMatchPropertyPreload]
		public String Name {
			get;
			set;
		}

		[LongoMatchPropertyIndex (1)]
		[LongoMatchPropertyPreload]
		public String TeamName {
			get;
			set;
		}

		[LongoMatchPropertyPreload]
		public Image Shield {
			get;
			set;
		}

		public int ActiveColor {
			get;
			set;
		}

		public Color[] Colors {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Color Color {
			get {
				if (ActiveColor > 0 && ActiveColor <= Colors.Length) {
					return Colors [ActiveColor];
				} else {
					ActiveColor = 0;
					return Colors [0];
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public int StartingPlayers {
			get {
				return Formation.Sum ();
			}
		}

		public int[] Formation {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
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
				Formation = tactics;
			}
			get {
				if (Formation != null) {
					return String.Join ("-", Formation);
				} else {
					return "";
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool TemplateEditorMode {
			set;
			get;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<Player> PlayingPlayersList {
			get {
				if (TemplateEditorMode) {
					return List.ToList ();
				} else {
					return List.Where (p => p.Playing).Select (p => p).ToList ();
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<Player> StartingPlayersList {
			get {
				List<Player> playingPlayers = PlayingPlayersList;
				int count = Math.Min (StartingPlayers, playingPlayers.Count);
				return playingPlayers.GetRange (0, count);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<Player> BenchPlayersList {
			get {
				List<Player> playingPlayers = PlayingPlayersList;
				int starting = StartingPlayers;
				if (playingPlayers.Count > starting) {
					return playingPlayers.GetRange (starting, playingPlayers.Count - starting);
				} else {
					return new List<Player> ();
				}
			}
		}

		/// <summary>
		/// Creates a deep copy of this team with new ID's for each player
		/// </summary>
		public Team Copy (string newName)
		{
			Team newTeam = this.Clone ();
			newTeam.ID = Guid.NewGuid ();
			newTeam.Name = newName;
			foreach (Player player in List) {
				player.ID = Guid.NewGuid ();
			}
			return newTeam;
		}

		public void RemovePlayers (List<Player> players, bool delete)
		{
			List<Player> bench, starters;
			
			bench = BenchPlayersList;
			starters = StartingPlayersList;

			foreach (Player p in players) {
				if (List.Contains (p)) {
					if (starters.Contains (p) && bench.Count > 0) {
						List.Swap (p, bench [0]);
					}
					List.Remove (p);
					if (!delete) {
						List.Add (p);
						p.Playing = false;
					}
				}
			}
		}

		public void ResetPlayers ()
		{
			foreach (Player p in List) {
				p.Playing = true;
			}
		}

		public void UpdateColors ()
		{
			foreach (Player p in List) {
				p.Color = Color;
			}
		}

		public Player AddDefaultItem (int i)
		{
			Player p = new Player {
				Name = "Player " + (i + 1).ToString (),
				Birthday = new DateTime (DateTime.Now.Year - 25, 6, 1),
				Height = 1.80f,
				Weight = 80,
				Number = i + 1,
				Position = "",
				Photo = null,
				Playing = true,
			};
			List.Insert (i, p);
			return p;
		}

		public static Team DefaultTemplate (int playersCount)
		{
			Team defaultTemplate = new Team ();
			defaultTemplate.FillDefaultTemplate (playersCount);
			return defaultTemplate;
		}

		void FillDefaultTemplate (int playersCount)
		{
			List.Clear ();
			for (int i = 1; i <= playersCount; i++)
				AddDefaultItem (i - 1);
		}

		void PlayersChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
	}

	/* Keep this for backwards compatibility importing old project files */
	[Obsolete ("Use Team instead of TeamTeamplate in new code")]
	[Serializable]
	public class TeamTemplate: Team
	{
	}
}
