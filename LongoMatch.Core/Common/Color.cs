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
		public Color (short r, short g, short b, short a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}
		
		public short R {
			get;
			set;
		}
		
		public short G {
			get;
			set;
		}
		
		public short B {
			get;
			set;
		}
		
		public short A {
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
	}

	public class ColorHelper
	{
	
		static public ushort ByteToShort (Byte val) {
			var ret = (ushort) (((float)val) / byte.MaxValue * ushort.MaxValue);
			return ret;
		}
		
		static public byte ShortToByte (ushort val) {
			return (byte) (((float)val) / ushort.MaxValue * byte.MaxValue);
		}
		
		static public double ShortToDouble (ushort val) {
			return (double) (val) / ushort.MaxValue;
		}
		
		static public double ByteToDouble (byte val) {
			return (double) (val) / byte.MaxValue;
		}
	}
}

