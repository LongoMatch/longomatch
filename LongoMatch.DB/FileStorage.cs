//
//  Copyright (C) 2016 Fluendo S.A.
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
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace LongoMatch.DB
{
	public class FileStorage : VAS.DB.FileStorage
	{
		protected override void MigrateStorable (IStorable storable)
		{
			if (storable is Project) {
				ProjectMigration.Migrate (storable as ProjectLongoMatch);
			} else if (storable is Team) {
				TeamMigration.Migrate (storable as SportsTeam);
			} else if (storable is DashboardLongoMatch) {
				DashboardMigration.Migrate (storable as DashboardLongoMatch);
			}
		}
	}
}
