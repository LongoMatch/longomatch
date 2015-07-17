//
//  Copyright (C) 2013 Andoni Morales Alastruey
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

namespace LongoMatch.Core.Common
{
	
	[Serializable]
	public class EncodingQuality
	{
		public string Name;
		public uint AudioQuality;
		public uint VideoQuality;

		public EncodingQuality ()
		{
		}

		public EncodingQuality (string name, uint videoQuality, uint audioQuality)
		{
			Name = name;
			VideoQuality = videoQuality;
			AudioQuality = audioQuality;
		}

		public override bool Equals (object obj)
		{
			EncodingQuality q;
			if (!(obj is EncodingQuality))
				return false;
			q = (EncodingQuality)obj;	
			return q.Name == Name &&
			q.AudioQuality == AudioQuality &&
			q.VideoQuality == VideoQuality;
		}

		public override int GetHashCode ()
		{
			return String.Format ("{0}-{1}-{2}", Name, AudioQuality, VideoQuality).GetHashCode ();
		}

	}

	public class EncodingQualities
	{
		public static EncodingQuality Lowest = new EncodingQuality ("Lowest (500 kbps)", 500, 128);
		public static EncodingQuality Low = new EncodingQuality ("Low (1000 kbps)", 1000, 128);
		public static EncodingQuality Medium = new EncodingQuality ("Medium (2000 kbps)", 2000, 128);
		public static EncodingQuality High = new EncodingQuality ("High (4000 kbps)", 4000, 128);
		public static EncodingQuality Highest = new EncodingQuality ("Highest (6000 kbps)", 6000, 128);

		public static List<EncodingQuality> All {
			get {
				List<EncodingQuality> list = new List<EncodingQuality> ();
				list.Add (Lowest);
				list.Add (Low);
				list.Add (Medium);
				list.Add (High);
				list.Add (Highest);
				return list;
			}
		}

		public static EncodingQuality[] Transcode {
			get {
				return new EncodingQuality[] { Highest, High, Medium };
			}
		}
	}
}

