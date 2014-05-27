// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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

namespace LongoMatch.Common
{
	public class Color
	{
		public Color (ushort r, ushort g, ushort b, ushort a=ushort.MaxValue)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}
		
		public ushort R {
			get;
			set;
		}
		
		public ushort G {
			get;
			set;
		}
		
		public ushort B {
			get;
			set;
		}
		
		public ushort A {
			get;
			set;
		}
		
		public override bool Equals (object obj)
		{
			Color c = obj as Color;
			if (c == null) {
				return false;
			}
			return c.R == R && c.G == G && c.B == B && c.A == A;
		}
		
		public override int GetHashCode ()
		{
			return (Int32)R<<24 | (Int32)G<<16 | (Int32)B<<8 | (Int32)A;
		}
		
		static public ushort ByteToUShort (Byte val) {
			var ret = (ushort) (((float)val) / byte.MaxValue * ushort.MaxValue);
			return ret;
		}

		static Color ColorFromRGB (byte r, byte g, byte b) {
			return new Color (ByteToUShort (r), ByteToUShort (g), ByteToUShort (b));
		}
		
		
		static public Color Black = new Color (0, 0, 0);
		static public Color White = new Color (ushort.MaxValue, ushort.MaxValue, ushort.MaxValue);
		static public Color Red = new Color (ushort.MaxValue, 0, 0);
		static public Color Green = new Color (0, ushort.MaxValue, 0);
		static public Color Blue = new Color (0, 0, ushort.MaxValue);
		static public Color Grey1 = ColorFromRGB (190, 190, 190);
		static public Color Grey2 = ColorFromRGB (32, 32, 32);
		static public Color Green1 = ColorFromRGB (99,192,56);
	}
}

