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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace LongoMatch.Core.Store
{
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class DashboardButton: IChanged
	{
		ObservableCollection<ActionLink> actionLinks;

		public DashboardButton ()
		{
			Name = "";
			Position = new Point (0, 0);
			Position.IsChanged = false;
			Width = Constants.BUTTON_WIDTH;
			Height = Constants.BUTTON_HEIGHT;
			BackgroundColor = Color.Red.Copy (true);
			TextColor = Config.Style.PaletteBackgroundLight.Copy (true);
			HotKey = new HotKey { IsChanged = false };
			ActionLinks = new ObservableCollection <ActionLink> ();
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
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

		/// <summary>
		/// A list with all the outgoing links of this button
		/// </summary>
		public ObservableCollection<ActionLink> ActionLinks {
			get {
				return actionLinks;
			}
			set {
				if (actionLinks != null) {
					actionLinks.CollectionChanged -= ListChanged;
				}
				actionLinks = value;
				if (actionLinks != null) {
					actionLinks.CollectionChanged += ListChanged;
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Color LightColor {
			get {
				YCbCrColor c = YCbCrColor.YCbCrFromColor (BackgroundColor);
				byte y = c.Y;
				c.Y = (byte)(Math.Min (y + 50, 255));
				return c.RGBColor ();
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Color DarkColor {
			get {
				YCbCrColor c = YCbCrColor.YCbCrFromColor (BackgroundColor);
				byte y = c.Y;
				c.Y = (byte)(Math.Max (y - 50, 0));
				return c.RGBColor ();
			}
		}

		public void AddActionLink (ActionLink link)
		{
			link.SourceButton = this;
			ActionLinks.Add (link);
		}

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
	}

	[Serializable]
	public class TimedDashboardButton: DashboardButton
	{
		public TimedDashboardButton ()
		{
			TagMode = TagMode.Predefined;
			Start = new Time { TotalSeconds = 10 };
			Start.IsChanged = false;
			Stop = new Time { TotalSeconds = 10 };
			Stop.IsChanged = false;
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
			BackgroundColor = StyleConf.ButtonTagColor.Copy (true);
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
		TimeNode currentNode;

		public TimerButton ()
		{
			BackgroundColor = StyleConf.ButtonTimerColor.Copy (true);
			currentNode = null;
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

		public void Start (Time start, List<DashboardButton> from)
		{
			if (currentNode != null)
				return;

			if (Timer != null) {
				currentNode = Timer.Start (start);
				Config.EventsBroker.EmitTimeNodeStartedEvent (currentNode, this, from);
			}
		}

		public void Stop (Time stop, List<DashboardButton> from)
		{
			if (currentNode == null)
				return;

			if (Timer != null) {
				Timer.Stop (stop);
				Config.EventsBroker.EmitTimeNodeStoppedEvent (currentNode, this, from);
				currentNode = null;
			}
		}

		public void Cancel ()
		{
			if (currentNode == null)
				return;
			if (Timer != null) {
				Timer.CancelCurrent ();
				currentNode = null;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Time StartTime {
			get {
				return currentNode == null ? null : currentNode.Start;
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

		[OnDeserialized ()]
		internal void OnDeserializedMethod (StreamingContext context)
		{
			if (EventType != null) {
				EventType.IsChanged = false;
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
		[PropertyChanged.DoNotNotify]
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

		// We keep it for backwards compatibility, to be able to retrieve the PenaltyCard from this object
		[JsonProperty ("PenaltyCard")]
		[Obsolete ("Do not use, it's only kept here for migrations")]
		public PenaltyCard OldPenaltyCard {
			set;
			get;
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
		[PropertyChanged.DoNotNotify]
		public PenaltyCard PenaltyCard {
			get {
				return PenaltyCardEventType != null ? PenaltyCardEventType.PenaltyCard : null;
			}
			set {
				if (PenaltyCardEventType != null) {
					PenaltyCardEventType.PenaltyCard = value;
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
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

		// We keep it for backwards compatibility, to be able to retrieve the Score from this object
		[JsonProperty ("Score")]
		[Obsolete ("Do not use, it's only kept here for migrations")]
		public Score OldScore {
			set;
			get;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Score Score {
			get {
				return ScoreEventType != null ? ScoreEventType.Score : null;
			}
			set {
				if (ScoreEventType != null) {
					ScoreEventType.Score = value;
				}
			}
		}


		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public ScoreEventType ScoreEventType {
			get {
				return EventType as ScoreEventType;
			}
		}
	}
}

