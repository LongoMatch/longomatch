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
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	[Serializable]
	public class Period: Timer
	{

		public Period ()
		{
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public TimeNode PeriodNode {
			get {
				Time start, stop;
				
				start = new Time (Nodes.Min (tn => tn.Start.MSeconds));
				stop = new Time (Nodes.Max (tn => tn.Stop != null ? tn.Stop.MSeconds : 0));
				if (stop.MSeconds == 0) {
					stop = null;
				}
				return new TimeNode {Name = Name,
					Start = start, Stop = stop
				};
			}
			set {
				foreach (TimeNode tn in Nodes) {
					if (tn.Start < value.Start) {
						tn.Start = value.Start;
					}
					if (tn.Stop > value.Stop) {
						tn.Stop = value.Stop;
					}
				}
			}
		}
	}
}

