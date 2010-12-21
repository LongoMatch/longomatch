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
using Gdk;
using LongoMatch.Common;

namespace LongoMatch.Store
{
	/// <summary>
	/// I am a player from a team
	/// </summary>
	[Serializable]
	public class Player
	{
		private byte[] photo;

		#region Constructors
		public Player()
		{
		}
		#endregion

		#region Properties
		/// <value>
		/// My name
		/// </value>
		public string Name {
			get;
			set;
		}
		
		public Team Team{
			get;
			set;
		}

		/// <value>
		/// My position in the field
		/// </value>
		public string Position {
			get;
			set;
		}

		/// <value>
		/// My shirt number
		/// </value>
		public int Number {
			get;
			set;
		}

		/// <value>
		/// My photo
		/// </value>
		public Pixbuf Photo {
			get {
				if (photo != null)
					return new Pixbuf(photo);
				else
					return null;
			}
			set {
				if (value != null)
					photo=value.SaveToBuffer("png");
				else
					photo=null;
			}
		}
		
		/// <value>
		/// My birthdayt
		/// </value>
		public DateTime Birthday{
			get;
			set;
		}
		
		/// <value>
		/// My nationality
		/// </value>
		public String Nationality{
			get;
			set;
		}
		
		/// <value>
		/// My height
		/// </value>
		public float Height{
			get;
			set;
		}

		/// <value>
		/// My Weight
		/// </value>
		public int Weight{
			get;
			set;
		}
		
		public bool Playing{
			get;
			set;
		}
		
		/// <value>
		/// A team can have several players, but not all of them
		/// play in the same match,. This allow reusing the same
		/// template in a team, definning if this plays plays or not.
		/// </value>
		public bool Discarded{
			get;
			set;
		}
		#endregion
	}
}
