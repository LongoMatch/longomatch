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
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.MVVMC;

namespace LongoMatch.Core.Store
{
	[Serializable]
	public class Score: BindableBase
	{
		public Score ()
		{
		}

		public Score (string name, int points)
		{
			Name = name;
			Points = points;
			Color = Color.Red;
		}

		[Obsolete ("Color is now defined in the ScoreEventType")]
		public Color Color {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public int Points {
			get;
			set;
		}
	}
}
