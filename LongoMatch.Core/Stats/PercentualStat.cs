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

namespace LongoMatch.Core.Stats
{
	public class PercentualStat: Stat
	{
		int parentTotal;

		public PercentualStat (string name, int totalCount, int localTeamCount,
		                       int visitorTeamCount, int parentTotal) : base (name, totalCount, localTeamCount, visitorTeamCount)
		{
			this.parentTotal = parentTotal;
		}

		public int TotalPercent {
			get {
				return (int)(((float)TotalCount) / parentTotal * 100);
			}
		}
	}
}

