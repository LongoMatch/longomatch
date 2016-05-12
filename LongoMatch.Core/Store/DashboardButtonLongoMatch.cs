//
//  Copyright (C) 2016 dfernandez
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
using VAS.Core.Common;
using VAS.Core.Store;
using System.Collections.Generic;

// TODO: Rename file to SportDashboardButtons.cs // in plural!

namespace LongoMatch.Core.Store
{
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
				return PenaltyCard?.Color;
			}
			set {
				if (PenaltyCard != null) {
					PenaltyCard.Color = value;
				}
			}
		}

		public override string Name {
			get {
				return PenaltyCard?.Name;
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
				return PenaltyCardEventType?.PenaltyCard;
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
				return ScoreEventType?.Score;
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


	[Serializable]
	public class TimerButtonLongoMatch: TimerButton
	{
		public TimerButtonLongoMatch () : base ()
		{
		}

		public override void Start (Time start, List<DashboardButton> from)
		{
			if (currentNode != null)
				return;

			if (Timer != null) {
				currentNode = Timer.Start (start);
				Config.EventsBroker.EmitTimeNodeStartedEvent (currentNode, this, from);
			}
		}

		public override void Stop (Time stop, List<DashboardButton> from)
		{
			if (currentNode == null)
				return;

			if (Timer != null) {
				Timer.Stop (stop);
				Config.EventsBroker.EmitTimeNodeStoppedEvent (currentNode, this, from);
				currentNode = null;
			}
		}

		public override Timer Timer {
			get;
			set;
		}
	}
}

