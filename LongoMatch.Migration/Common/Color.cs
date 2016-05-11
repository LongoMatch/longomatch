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
using Newtonsoft.Json;

namespace LongoMatch.Common
{

	[JsonConverter (typeof (VASConverter))]
	public class Color
	{
		public Color (byte r, byte g, byte b, byte a=byte.MaxValue)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}
		
		public byte R {
			get;
			set;
		}
		
		public byte G {
			get;
			set;
		}
		
		public byte B {
			get;
			set;
		}
		
		public byte A {
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
		
		static public byte UShortToByte (ushort val) {
			return (byte) (((float)val) / ushort.MaxValue * byte.MaxValue);
		}
		
		static public Color ColorFromUShort (ushort r, ushort g, ushort b, ushort a = ushort.MaxValue) {
			return new Color (UShortToByte (r), UShortToByte (g),
			                  UShortToByte (b), UShortToByte (a));
		}
		
		static public Color Black = new Color (0, 0, 0);
		static public Color White = new Color (255, 255, 255);
		static public Color Red = new Color (255, 0, 0);
		static public Color Green = new Color (0, 255, 0);
		static public Color Blue = new Color (0, 0, 255);
		static public Color Grey1 = new Color (190, 190, 190);
		static public Color Grey2 = new Color (32, 32, 32);
		static public Color Green1 = new Color (99,192,56);
		static public Color Red1 = new Color (255, 51, 0);
		static public Color Blue1 = new Color (0, 153, 255);
	}
}

