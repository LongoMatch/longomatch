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
using LongoMatch.Core.Common;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	[Serializable]
	public class DashboardButton
	{

		public DashboardButton ()
		{
			Name = "";
			Position = new Point (0, 0);
			Width = Constants.BUTTON_WIDTH;
			Height = Constants.BUTTON_HEIGHT;
			BackgroundColor = Color.Red;
			TextColor = Config.Style.PaletteBackgroundLight;
			HotKey = new HotKey ();
		}

		public virtual string Name {
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

		public virtual Color BackgroundColor {
			get;
			set;
		}

		public Color TextColor {
			get;
			set;
		}

		public virtual HotKey HotKey {
			get;
			set;
		}

		public virtual Image BackgroundImage {
			get;
			set;
		}

		[JsonIgnore]
		public Color LightColor {
			get {
				YCbCrColor c = YCbCrColor.YCbCrFromColor (BackgroundColor);
				byte y = c.Y;
				c.Y = (byte)(Math.Min (y + 50, 255));
				return c.RGBColor ();
			}
		}

		[JsonIgnore]
		public Color DarkColor {
			get {
				YCbCrColor c = YCbCrColor.YCbCrFromColor (BackgroundColor);
				byte y = c.Y;
				c.Y = (byte)(Math.Max (y - 50, 0));
				return c.RGBColor ();
			}
		}
	}

	[Serializable]
	public class TimedDashboardButton: DashboardButton
	{
		public TimedDashboardButton ()
		{
			TagMode = TagMode.Predefined;
			Start = new Time { TotalSeconds = 10 };
			Stop = new Time { TotalSeconds = 10 };
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
	}

	[Serializable]
	public class TagButton: DashboardButton
	{
		public TagButton ()
		{
			BackgroundColor = StyleConf.ButtonTagColor;
		}

		public Tag Tag {
			get;
			set;
		}

		public override HotKey HotKey {
			get {
				return Tag != null ? Tag.HotKey : null;
			}
			set {
				if (Tag != null) {
					Tag.HotKey = value;
				}
			}
		}

		public override string Name {
			get {
				return Tag != null ? Tag.Value : null;
			}
			set {
				if (Tag != null) {
					Tag.Value = value;
				}
			}
		}
	}

	[Serializable]
	public class TimerButton: DashboardButton
	{
		public TimerButton ()
		{
			BackgroundColor = StyleConf.ButtonTimerColor;
		}

		public Timer Timer {
			get;
			set;
		}

		public override string Name {
			get {
				return Timer != null ? Timer.Name : null;
			}
			set {
				if (Timer != null) {
					Timer.Name = value;
				}
			}
		}
	}

	[Serializable]
	public class EventButton: TimedDashboardButton
	{
		public EventType EventType {
			get;
			set;
		}

		public override string Name {
			get {
				return EventType != null ? EventType.Name : null;
			}
			set {
				if (EventType != null) {
					EventType.Name = value;
				}
			}
		}

		public override Color BackgroundColor {
			get {
				return EventType != null ? EventType.Color : null;
			}
			set {
				if (EventType != null) {
					EventType.Color = value;
				}
			}
		}
	}

	[Serializable]
	public class AnalysisEventButton: EventButton
	{
		public AnalysisEventButton ()
		{
			TagsPerRow = 2;
			ShowSubcategories = true;
		}

		public bool ShowSubcategories {
			get;
			set;
		}

		public int TagsPerRow {
			get;
			set;
		}

		[JsonIgnore]
		public AnalysisEventType AnalysisEventType {
			get {
				return EventType as AnalysisEventType;
			}
		}
	}

	[Serializable]
	public class PenaltyCardButton: EventButton
	{
		public PenaltyCardButton ()
		{
			EventType = new PenaltyCardEventType ();
		}

		public PenaltyCard PenaltyCard {
			get;
			set;
		}

		public override Color BackgroundColor {
			get {
				return PenaltyCard != null ? PenaltyCard.Color : null;
			}
			set {
				if (PenaltyCard != null) {
					PenaltyCard.Color = value;
				}
			}
		}

		public override string Name {
			get {
				return PenaltyCard != null ? PenaltyCard.Name : null;
			}
			set {
				if (PenaltyCard != null) {
					PenaltyCard.Name = value;
				}
			}
		}

		[JsonIgnore]
		public PenaltyCardEventType PenaltyCardEventType {
			get {
				return EventType as PenaltyCardEventType;
			}
		}
	}

	[Serializable]
	public class ScoreButton: EventButton
	{

		public ScoreButton ()
		{
			EventType = new ScoreEventType ();
		}

		public Score Score {
			get;
			set;
		}

		public override string Name {
			get {
				return Score != null ? Score.Name : null;
			}
			set {
				if (Score != null) {
					Score.Name = value;
				}
			}
		}

		public override Color BackgroundColor {
			get {
				return Score != null ? Score.Color : null;
			}
			set {
				if (Score != null) {
					Score.Color = value;
				}
			}
		}


		[JsonIgnore]
		public ScoreEventType ScoreEventType {
			get {
				return EventType as ScoreEventType;
			}
		}
	}
}

