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
using Mono.Addins;
using LongoMatch.Addins.ExtensionPoints;
using VAS.Core;
using LongoMatch.Core.Store;

[assembly:Addin]
[assembly:AddinAuthor ("LongoMatch Project")]
[assembly:AddinName ("Stats")]
[assembly:AddinDescription ("Statistics plugin")]
[assembly:AddinDependency ("LongoMatch", "1.1")]
namespace LongoMatch.Plugins.Stats
{
	[Extension]
	public class StatsUIPlugin: IStatsUI
	{
		public void ShowStats (ProjectLongoMatch project)
		{
			StatsDialog statsui = new StatsDialog ();
			statsui.LoadStats (project);
			statsui.Run ();
			statsui.Destroy ();
		}

		public int Priority {
			get {
				return 0;
			}
		}

		public string Name {
			get {
				return Catalog.GetString ("Game statistics");
			}
		}

		public string Description {
			get {
				return Catalog.GetString ("Show the game statistics");
			}
		}
	}
}