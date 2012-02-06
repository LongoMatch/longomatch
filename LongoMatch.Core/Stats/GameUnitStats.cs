// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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

using LongoMatch.Store;

namespace LongoMatch.Stats
{
	public class GameUnitStatsNode : List<GameUnitStatsNode>
	{
		public GameUnitStatsNode (TimelineNode  node)
		{
			Node = node;
		}
		
		public GameUnitStatsNode (TimelineNode  node, List<GameUnitStatsNode> list): base (list)
		{
			Node = node;
		}
		
		public TimelineNode Node {
			get;
			set;
		}
		
		public string Name {
			get {
				return Node.Name;
			}
		}
		
		public int Duration {
			get {
				return Node.Duration.MSeconds;
			}
		}
		
		public int PlayingTime {
			get {
				if (this.Count == 0) {
					return Duration;
				}
				return this.Sum(n => n.PlayingTime);
			}
		}
		
		public int PausedTime {
			get {
				if (this.Count == 0)
					return 0;
				return Duration - PlayingTime;
			}
		}
		
		public double AverageDuration {
			get {
				if (this.Count == 0)
					return 0;
				return this.Average(n => n.Duration);
			}
		}
		
		public double AveragePlayingTime {
			get {
				if (this.Count == 0)
					return 0;
				return this.Average(n => n.PlayingTime);
			}
		}
		
		public double AveragePausedTime {
			get {
				if (this.Count == 0)
					return 0;
				return this.Average(n => n.PausedTime);
			}
		}
		
		public double DurationTimeStdDeviation {
			get {
				if (this.Count == 0)
					return 0;
				return Math.Sqrt(this.Average(n=>Math.Pow(n.Duration - AverageDuration,2)));	
			}
		}
		
		public double PlayingTimeStdDeviation {
			get {
				if (this.Count == 0)
					return 0;
				return Math.Sqrt(this.Average(n=>Math.Pow(n.PlayingTime - AveragePlayingTime,2)));	
			}
		}
		
		public double PausedTimeStdDeviation {
			get {
				if (this.Count == 0)
					return 0;
				return Math.Sqrt(this.Average(n=>Math.Pow(n.PausedTime - AveragePausedTime,2)));	
			}
		}
	}
}

