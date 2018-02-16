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
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Resources;
using VAS.Core.Serialization;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace LongoMatch.Core.Store.Templates
{
	[Serializable]
	public class LMTeam : Team
	{
		public const int VERSION = 2;
		const int MAX_WIDTH = 100;
		const int MAX_HEIGHT = 100;

		public LMTeam ()
		{
			TeamName = Catalog.GetString ("Team");
			FormationStr = "1-4-3-3";
			try {
				Shield = App.Current.ResourcesLocator.LoadIcon (Icons.DefaultShield);
			} catch {
				/* Ignore for unit tests */
			}
			ActiveColor = 0;
			Colors = new Color [2];
			Colors [0] = Color.Blue1;
			Colors [1] = Color.Red1;
			Version = VERSION;
		}

		[PropertyIndex (1)]
		[PropertyPreload]
		/// <summary>
		/// The display name used for a team (eg: FC Barcelona)
		/// </summary>
		/// <value>The display name of the team.</value>
		public String TeamName {
			get;
			set;
		}

		public int ActiveColor {
			get;
			set;
		}

		public Color [] Colors {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override Color Color {
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

		public int [] Formation {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public string FormationStr {
			set {
				string [] elements = value.Split ('-');
				int [] tactics = new int [elements.Length];
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
		public List<LMPlayer> Players {
			get {
				return List.OfType<LMPlayer> ().ToList ();
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<LMPlayer> CalledPlayersList {
			get {
				if (TemplateEditorMode) {
					return Players.ToList ();
				} else {
					return Players.Where (p => p.Playing).Select (p => p).ToList ();
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<LMPlayer> StartingPlayersList {
			get {
				List<LMPlayer> playingPlayers = CalledPlayersList;
				int count = Math.Min (StartingPlayers, playingPlayers.Count);
				return playingPlayers.GetRange (0, count);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<LMPlayer> BenchPlayersList {
			get {
				List<LMPlayer> playingPlayers = CalledPlayersList;
				int starting = StartingPlayers;
				if (playingPlayers.Count > starting) {
					return playingPlayers.GetRange (starting, playingPlayers.Count - starting);
				} else {
					return new List<LMPlayer> ();
				}
			}
		}

		public void RemovePlayers (List<LMPlayer> players, bool delete)
		{
			List<LMPlayer> bench, starters;

			bench = BenchPlayersList;
			starters = StartingPlayersList;

			foreach (LMPlayer p in players) {
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
			foreach (LMPlayer p in List) {
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
			var p = new LMPlayer {
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

		public static LMTeam DefaultTemplate (int playersCount)
		{
			LMTeam defaultTemplate = new LMTeam ();
			defaultTemplate.FillDefaultTemplate (playersCount);
			defaultTemplate.Preview = App.Current.PreviewService.CreatePreview(defaultTemplate);
			return defaultTemplate;
		}


		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			// FIXME: Remove this when Players have their own ViewModel and the Tagged property is moved there
			if (sender is Player && e.PropertyName == "Tagged") {
				return;
			}
			base.ForwardPropertyChanged (sender, e);
		}

		void FillDefaultTemplate (int playersCount)
		{
			List.Clear ();
			for (int i = 1; i <= playersCount; i++)
				AddDefaultItem (i - 1);
		}

	}

	/* Keep this for backwards compatibility importing old project files */
	[Obsolete ("Use Team instead of TeamTeamplate in new code")]
	[Serializable]
	public class TeamTemplate : LMTeam
	{
	}
}
