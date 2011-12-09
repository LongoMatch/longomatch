// HotKey.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//


using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Mono.Unix;

namespace LongoMatch.Store
{

	/// <summary>
	/// A key combination used to tag plays using the keyboard. <see cref="LongoMatch.Store.SectionsTimeNodes"/>
	/// It can only be used with the Shith and Alt modifiers to avoid interfering with ohter shortcuts.
	/// 'key' and 'modifier' are set to -1 when it's initialized
	/// </summary>
	[Serializable]
	public class HotKey : IEquatable<HotKey>
	{
		private int key;
		private int modifier;

		#region Constructors
		/// <summary>
		/// Creates a new undefined HotKey
		/// </summary>
		public HotKey()
		{
			this.key = -1;
			this.modifier = -1;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gdk Key
		/// </summary>
		public int Key {
			get {
				return key;
			}
			set {
				key = value;
			}
		}

		/// <summary>
		/// Key modifier. Only Alt and Shift can be used
		/// </summary>
		public int Modifier {
			get {
				return modifier;
			}
			set {
				modifier = value;
			}
		}

		/// <summary>
		/// Get whether the hotkey is defined or not
		/// </summary>
		public Boolean Defined {
			get {
				return (key!=-1 && modifier != -1);
			}
		}
		#endregion

		#region Public Methods
		public bool Equals(HotKey hotkeyComp) {
			return (this.Key == hotkeyComp.Key && this.Modifier == hotkeyComp.Modifier);
		}
		#endregion

		#region Operators
		static public bool operator == (HotKey hk1, HotKey hk2) {
			return hk1.Equals(hk2);
		}

		static public bool operator != (HotKey hk1, HotKey hk2) {
			return !hk1.Equals(hk2);
		}
		#endregion

		#region Overrides
		public override bool Equals(object obj)
		{
			if(obj is HotKey) {
				HotKey hotkey= obj as HotKey;
				return Equals(hotkey);
			}
			else
				return false;
		}

		public override int GetHashCode()
		{
			return key ^ modifier;
		}

		public override string ToString()
		{
			string modifierS;
			if(!Defined)
				return Catalog.GetString("Not defined");
			if(Modifier == 3)//ModifierType.Mod1Mask)
				modifierS = "<Alt>+";
			else if(Modifier == 0)// ModifierType.ShiftMask)
				modifierS = "<Shift>+";
			else
				modifierS = "";

			return string.Format("{0}{1}", modifierS,(Key.ToString()).ToLower());
		}
		#endregion
	}
}
