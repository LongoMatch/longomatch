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
using System.Globalization;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;

namespace LongoMatch.Core.Common
{
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class Color: IChanged
	{
		public Color (byte r, byte g, byte b, byte a = byte.MaxValue)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
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

		public Color Copy (bool resetChanged = false)
		{
			Color c = new Color (R, G, B, A);
			if (resetChanged) {
				c.IsChanged = false;
			}
			return c;
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
			return (Int32)R << 24 | (Int32)G << 16 | (Int32)B << 8 | (Int32)A;
		}

		public string ToRGBString (bool with_alpha)
		{
			if (with_alpha) {
				return string.Format ("#{0:X}{1:X}{2:X}{3:X}", R, G, B, A);
			} else {
				return string.Format ("#{0:X}{1:X}{2:X}", R, G, B);
			}
		}

		public override string ToString ()
		{
			return ToRGBString (true);
		}

		static public byte UShortToByte (ushort val)
		{
			return (byte)(((float)val) / ushort.MaxValue * byte.MaxValue);
		}

		static public ushort ByteToUShort (byte val)
		{
			return (ushort)((float)val / byte.MaxValue * ushort.MaxValue);
		}

		static public Color ColorFromUShort (ushort r, ushort g, ushort b, ushort a = ushort.MaxValue)
		{
			return new Color (UShortToByte (r), UShortToByte (g),
				UShortToByte (b), UShortToByte (a));
		}

		static public Color Parse (string colorHex)
		{
			byte r, g, b, a = Byte.MaxValue;
			
			if (!colorHex.StartsWith ("#")) {
				return null;
			}
			if (colorHex.Length != 7 && colorHex.Length != 9) {
				return null;
			}
			
			try {
				r = Byte.Parse (colorHex.Substring (1, 2), NumberStyles.HexNumber);
				g = Byte.Parse (colorHex.Substring (3, 2), NumberStyles.HexNumber);
				b = Byte.Parse (colorHex.Substring (5, 2), NumberStyles.HexNumber);
				if (colorHex.Length == 9) {
					a = Byte.Parse (colorHex.Substring (7, 2), NumberStyles.HexNumber);
				}
			} catch (FormatException ex) {
				Log.Exception (ex);
				return null;
			}
			return new Color (r, g, b, a);
		}

		static public Color Black = new Color (0, 0, 0);
		static public Color White = new Color (255, 255, 255);
		static public Color Red = new Color (255, 0, 0);
		static public Color Green = new Color (0, 255, 0);
		static public Color Blue = new Color (0, 0, 255);
		static public Color Grey1 = new Color (190, 190, 190);
		static public Color Grey2 = new Color (32, 32, 32);
		static public Color Green1 = new Color (99, 192, 56);
		static public Color Red1 = new Color (255, 51, 0);
		static public Color Blue1 = new Color (0, 153, 255);
		static public Color Yellow = new Color (255, 255, 0);
		static public Color Transparent = new Color (0, 0, 0, 0);

	}


	public class YCbCrColor
	{

		public YCbCrColor (byte y, byte cb, byte cr)
		{
			Y = y;
			Cb = cb;
			Cr = cr;
		}

		public byte Y {
			get;
			set;
		}

		public byte Cb {
			get;
			set;
		}

		public byte Cr {
			get;
			set;
		}

		public static YCbCrColor YCbCrFromColor (Color c)
		{
			byte Y, Cb, Cr;

			Y = (byte)(16 + 0.257 * c.R + 0.504 * c.G + 0.098 * c.B);
			Cb = (byte)(128 - 0.148 * c.R - 0.291 * c.G + 0.439 * c.B);
			Cr = (byte)(128 + 0.439 * c.R - 0.396 * c.G - 0.071 * c.B);
			return new YCbCrColor (Y, Cb, Cr);
		}

		public Color RGBColor ()
		{
			double r, g, b;

			r = (1.164 * (Y - 16) + 1.596 * (Cr - 128));
			g = (1.164 * (Y - 16) - 0.392 * (Cb - 128) - 0.813 * (Cr - 128));
			b = (1.164 * (Y - 16) + 2.017 * (Cb - 128));

			return new Color (
				(byte)Math.Max (0, Math.Min (r, 255)),
				(byte)Math.Max (0, Math.Min (g, 255)),
				(byte)Math.Max (0, Math.Min (b, 255)));
		}
	}
}
