//
//  Copyright (C) 2015 Fluendo S.A.
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
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Core.Migration
{
	public static class TeamMigration
	{
		/// <summary>
		/// Migrate the specified team to the current version format.
		/// </summary>
		/// <param name="team">team.</param>
		public static void Migrate (LMTeam team)
		{
			/* Apply all the migration steps starting from the current version*/
			switch (team.Version) {
			case 0:
				Migrate0 (team);
				break;
			case 1:
				Migrate1 (team);
				break;
			default:
				return;
			}
			Migrate (team);
		}

		#pragma warning disable 0618
		public static void Migrate0 (LMTeam team, IDictionary<string, Guid> teamNameToID = null)
		{
			if (team.Version != 0) {
				return;
			}

			if (teamNameToID == null) {
				teamNameToID = new Dictionary<string, Guid> ();
			}

			if (team.ID == Guid.Empty) {
				Guid id;

				if (!teamNameToID.TryGetValue (team.Name, out id)) {
					teamNameToID [team.Name] = id = Guid.NewGuid ();
				}
				team.ID = id;
			}
			team.Version = 1;
		}
		#pragma warning restore 0618

		public static void Migrate1 (LMTeam team)
		{
			if (team.Version != 1) {
				return;
			}

			team.Preview = App.Current.PreviewService.CreatePreview (team);
			team.Version = 2;
		}
	}
}

