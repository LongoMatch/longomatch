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

namespace LongoMatch.TimeNodes
{
	/// <summary>
	/// I am a player from a team
	/// </summary>
	[Serializable]
	public class Player
	{
		private string name;
		private string position;
		private int number;
		private byte[] photo;
		/* Added in 0.16.1 */
		private float height;
		private int weight;
		private DateTime birthday;

		/// <summary>
		/// Creates a new player
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/> with my name
		/// </param>
		/// <param name="birthday">
		/// A <see cref="DateTime"/> with my name
		/// </param>
		/// <param name="height">
		/// A <see cref="System.Float"/> with my height
		/// </param>
		/// <param name="weight">
		/// A <see cref="System.Int32"/> with my weight
		/// </param>
		/// <param name="position">
		/// A <see cref="System.String"/> with my position in the field
		/// </param>
		/// <param name="number">
		/// A <see cref="System.Int32"/> with my number
		/// </param>
		/// <param name="photo">
		/// A <see cref="Pixbuf"/> with my photo
		/// </param>
		#region Constructors
		public Player(string name, DateTime birthday, float height, int weight,
		              string position, int number, Pixbuf photo)
		{
			this.name = name;
			this.birthday = birthday;
			this.height = height;
			this.weight = weight;
			this.position = position;
			this.number = number;
			Photo = photo;
		}
		#endregion

		#region Properties
		/// <value>
		/// My name
		/// </value>
		public string Name {
			get {
				return name;
			}
			set {
				name=value;
			}
		}

		/// <value>
		/// My position in the field
		/// </value>
		public string Position {
			get {
				return position;
			}
			set {
				position=value;
			}
		}

		/// <value>
		/// My shirt number
		/// </value>
		public int Number {
			get {
				return number;
			}
			set {
				number=value;
			}
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
			get {
				return birthday;
			}
			set {
				birthday = value;
			}
		}
		
		/// <value>
		/// My height
		/// </value>
		public float Height{
			get {
				return height;
			}
			set {
				height = value;
			}
		}

		/// <value>
		/// My Weight
		/// </value>
		public int Weight{
			get {
				return weight;
			}
			set {
				weight = value;
			}
		}
		#endregion
	}
}
