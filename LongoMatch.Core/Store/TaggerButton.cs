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
using LongoMatch.Common;
using Newtonsoft.Json;
using Mono.Unix;

namespace LongoMatch.Store
{
	[Serializable]
	public class TaggerButton
	{
		Color color;
	
		public TaggerButton () {
			Position = new Point (0, 0);
			Width = Constants.BUTTON_WIDTH;
			Height = Constants.BUTTON_HEIGHT;
			Color = Color.Red;
			TextColor = Color.Grey2;
			Start = new Time {Seconds = 10};
			Stop = new Time {Seconds = 10};
			HotKey = new HotKey();
			ID = new Guid();
		}
		
		public Guid ID {
			get;
			set;
		}
		
		public string Name {
			get;
			set;
		}
		
		public Point Position {
			get;
			set;
		}
		
		public int Width {
			get;
			set;
		}
		
		public int Height {
			get;
			set;
		}
		
		public Color Color {
			get {
				return color;
			}
			set {
				byte y;
				YCbCrColor c = YCbCrColor.YCbCrFromColor (value);
				y = c.Y;
				c.Y = (byte) (Math.Min (y + 50, 255));
				LightColor = YCbCrColor.ColorFromYCbCr (c);
				c.Y = (byte) (Math.Max (y - 50, 0));
				DarkColor = YCbCrColor.ColorFromYCbCr (c);
				color = value;
			}
		}
		
		public Color TextColor {
			get;
			set;
		}

		public TagMode TagMode {
			get;
			set;
		}
		
		public Time Start {
			get;
			set;
		}

		public Time Stop {
			get;
			set;
		}

		public HotKey HotKey {
			get;
			set;
		}

		public SortMethodType SortMethod {
			get;
			set;
		}

		
		[JsonIgnore]
		public Color LightColor {
			get;
			set;
		}
		
		[JsonIgnore]
		public Color DarkColor {
			get;
			set;
		}
		
		[JsonIgnore]
		public string SortMethodString {
			get {
				switch(SortMethod) {
				case SortMethodType.SortByName:
					return Catalog.GetString("Sort by name");
				case SortMethodType.SortByStartTime:
					return Catalog.GetString("Sort by start time");
				case SortMethodType.SortByStopTime:
					return Catalog.GetString("Sort by stop time");
				case SortMethodType.SortByDuration:
					return Catalog.GetString("Sort by duration");
				default:
					return Catalog.GetString("Sort by name");
				}
			}
			set {
				if(value == Catalog.GetString("Sort by start time"))
					SortMethod = SortMethodType.SortByStartTime;
				else if(value == Catalog.GetString("Sort by stop time"))
					SortMethod = SortMethodType.SortByStopTime;
				else if(value == Catalog.GetString("Sort by duration"))
					SortMethod = SortMethodType.SortByDuration;
				else
					SortMethod = SortMethodType.SortByName;
			}
		}
	}
	
	public class AnalysisCategory: TaggerButton {
		public bool TagGoalPosition {
			get;
			set;
		}
		
		public bool TagFieldPosition {
			get;
			set;
		}
		
		public bool TagHalfFieldPosition {
			get;
			set;
		}
		
		public bool FieldPositionIsDistance {
			get;
			set;
		}
		
		public bool HalfFieldPositionIsDistance {
			get;
			set;
		}
	}

	public class AnalysisEventCategory: AnalysisCategory {
		public Time EventTime {
			get;
			set;
		}
	}
	
}

