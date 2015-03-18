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
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Stats
{
	public class TimerStats
	{
		Timer timer;
		Project project;

		public TimerStats (Project project, Timer timer)
		{
			this.timer = timer;
			Name = timer.Name;
			if (timer.Team == TeamType.LOCAL) {
				TeamImage = project.LocalTeamTemplate.Shield;
			} else if (timer.Team == TeamType.VISITOR) {
				TeamImage = project.VisitorTeamTemplate.Shield;
			}
		}

		public string Name {
			get;
			set;
		}

		public Image TeamImage {
			get;
			set;
		}

		public int Count {
			get;
			set;
		}

		public Time TotalDuration {
			get;
			set;
		}

		public Time AverageDuration {
			get;
			set;
		}

		public Time MinDuration {
			get;
			set;
		}

		public Time MaxDuration {
			get;
			set;
		}

		public void Update ()
		{
			Count = timer.Nodes.Count;
			if (Count > 0) {
				TotalDuration = new Time (timer.Nodes.Sum (n => n.Duration.MSeconds));
				AverageDuration = new Time ((int)timer.Nodes.Average (n => n.Duration.MSeconds));
				MinDuration = timer.Nodes.Min (n => n.Duration);
				MaxDuration = timer.Nodes.Max (n => n.Duration);
			} else {
				TotalDuration = new Time (0);
				AverageDuration = new Time (0);
				MinDuration = new Time (0);
				MaxDuration = new Time (0);
			}
		}
	}
}

