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
using LongoMatch.Addins.ExtensionPoints;
using LongoMatch.Core.Store.Templates;
using System.Collections.Generic;
using Mono.Addins;
using Mono.Unix;

namespace LongoMatch.Plugins
{
	[Extension]
	public class SystemDashboards: IAnalsysDashboardsProvider
	{
		public string Name {
			get {
				return Catalog.GetString ("LongoMatch default dashboard");
			}
		}

		public string Description {
			get {
				return Catalog.GetString ("LongoMatch default dashboard");
			}
		}

		public List<Dashboard> Dashboards {
			get {
				Dashboard d = Dashboard.DefaultTemplate (14);
				d.Name = "Default";
				return new List<Dashboard> {d};	
			}
		}
	}
}
