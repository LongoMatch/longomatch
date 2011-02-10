//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using LongoMatch.Common;

namespace LongoMatch.Store
{

	[Serializable]
	public class Tag
	{
		public Tag() {
		}

		public string Name {
			get;
			set;
		}

		public object Value {
			get;
			set;
		}
	}

	[Serializable]
	public class StringTag
	{
		public StringTag() {
		}

		public string Name {
			get;
			set;
		}

		public string Value {
			get;
			set;
		}
	}

	[Serializable]
	public class PlayerTag:Tag
	{
		public PlayerTag() {
		}

		public new Player Value {
			get;
			set;
		}
	}

	[Serializable]
	public class TeamTag:Tag
	{
		public TeamTag() {
		}

		public new Team Value {
			get;
			set;
		}
	}
}
