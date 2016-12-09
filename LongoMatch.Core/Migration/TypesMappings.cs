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
using System;
using System.Collections.Generic;

namespace LongoMatch.Core.Migration
{
	public static class Mappings
	{
		/// <summary>
		/// A dictionary the old namespace string to replace as a key and a tuple with the new namespace and its assembly.
		/// For example, if we moved LongoMatch.Core.Foo1 and LongoMatch.Core.Foo2 to VAS.Core.Foo1 and VAS.Core.Foo2 in
		/// the assembly VAS.Core we use "LongoMatch.Core": ("VAS.Core", "VAS.Core")
		/// </summary>
		public static Dictionary<string, Tuple<string, string>> NamespacesReplacements = new Dictionary<string, Tuple<string, string>> {
			{ "LongoMatch.Core", new Tuple<string, string> ("VAS.Core", "VAS.Core") },
		};

		/// <summary>
		/// A dictionary to convert an old type by string to a <see cref="Type"/>.
		/// This is only needed when we can't replace the namespace prefix using <see cref="NamespacesReplacements"/>
		/// </summary>
		public static Dictionary<string, Type> TypesMappings = new Dictionary<string, Type> {
			{ "LongoMatch.Core.Store.Templates.TeamTemplate", typeof(LongoMatch.Core.Store.Templates.LMTeam) },
			{ "LongoMatch.Core.Store.Templates.Team", typeof(LongoMatch.Core.Store.Templates.LMTeam) },
			{ "LongoMatch.Core.Store.Templates.Dashboard", typeof(LongoMatch.Core.Store.Templates.LMDashboard) },
			{ "LongoMatch.Core.Store.Player", typeof(LongoMatch.Core.Store.LMPlayer) },
			{ "LongoMatch.Core.Store.Project", typeof(LongoMatch.Core.Store.LMProject) },
			{ "LongoMatch.Core.Store.TimelineEvent", typeof(LongoMatch.Core.Store.LMTimelineEvent) },
			{ "LongoMatch.Core.Store.Timer", typeof(LongoMatch.Core.Store.LMTimer) },
		};
	}
}

