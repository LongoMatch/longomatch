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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using VAS.Core.Store;

namespace LongoMatch.Core.Stats
{
	public class PlayerStats
	{
		LMProject project;
		EventsFilter filter;

		public PlayerStats (LMProject project, EventsFilter filter, LMPlayer player)
		{
			this.project = project;
			this.filter = filter;
			this.Player = player;
			PlayerEventStats = new List<PlayerEventTypeStats> ();
			foreach (EventType evtType in project.EventTypes) {
				if (!(evtType is SubstitutionEventType)) {
					PlayerEventStats.Add (new PlayerEventTypeStats (project, filter, player, evtType));
				}
			}
			UpdateTimePlayed ();
		}

		public LMPlayer Player {
			get;
			set;
		}

		public Time TimePlayed {
			get;
			set;
		}

		public List<PlayerEventTypeStats> PlayerEventStats {
			get;
			set;
		}

		public void Update ()
		{
			foreach (PlayerEventTypeStats stats in PlayerEventStats) {
				stats.Update ();
			}
		}

		void UpdateTimePlayed ()
		{
			LineupEvent lineup = project.Lineup;
			List<SubstitutionEvent> subs;
			Time start;
			List<TimeNode> timenodes, playingTimeNodes;
			TimeNode last;
			
			subs = project.EventsByType (project.SubstitutionsEventType).
				Where (s => !(s is LineupEvent) && ((s as SubstitutionEvent).In == Player ||
			(s as SubstitutionEvent).Out == Player))
				.OrderBy (e => e.EventTime).Select (e => e as SubstitutionEvent).ToList ();

			if (lineup.AwayStartingPlayers.Contains (Player) ||
			    lineup.HomeStartingPlayers.Contains (Player)) {
				start = lineup.EventTime;
			} else {
				SubstitutionEvent sub = subs.Where (s => s.In == Player).FirstOrDefault ();
				if (sub == null) {
					TimePlayed = new Time (0);
					return;
				} else {
					start = sub.EventTime;
				}
			}

			timenodes = new List<TimeNode> ();
			/* Find the sequences of playing time */
			last = new TimeNode { Start = start };
			timenodes.Add (last);
			if (subs.Count == 0) {
				last.Stop = project.Description.FileSet.Duration;
			} else {
				foreach (SubstitutionEvent sub in subs) {
					if (last.Stop == null) {
						if (sub.Out == Player) {
							last.Stop = sub.EventTime;
						}
					} else {
						if (sub.In == Player) {
							last = new TimeNode { Start = sub.EventTime };
							timenodes.Add (last);
						}
					}
				}
			}

			/* If the last substitution was Player IN */
			if (last.Stop == null) {
				last.Stop = project.Description.FileSet.Duration;
			}

			playingTimeNodes = new List<TimeNode> ();
			/* Get the real playing time intersecting with the periods */
			foreach (TimeNode timenode in timenodes) {
				foreach (Period p in project.Periods) {
					if (p.PeriodNode.Intersect (timenode) != null) {
						foreach (TimeNode ptn in p.Nodes) {
							TimeNode res = ptn.Intersect (timenode);
							if (res != null) {
								playingTimeNodes.Add (res);
							}
						}
					}
				}
			}
			
			TimePlayed = new Time (playingTimeNodes.Sum (t => t.Duration.MSeconds));
		}
	}
}

