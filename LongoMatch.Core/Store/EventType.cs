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
using LongoMatch.Core.Common;
using Mono.Unix;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	[Serializable]
	public class EventType
	{

		public EventType ()
		{
			ID = Guid.NewGuid ();
			Color = Color.Red;
		}

		public Guid ID {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public Color Color {
			get;
			set;
		}

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

		public SortMethodType SortMethod {
			get;
			set;
		}

		[JsonIgnore]
		public string SortMethodString {
			get {
				switch (SortMethod) {
				case SortMethodType.SortByName:
					return Catalog.GetString ("Sort by name");
				case SortMethodType.SortByStartTime:
					return Catalog.GetString ("Sort by start time");
				case SortMethodType.SortByStopTime:
					return Catalog.GetString ("Sort by stop time");
				case SortMethodType.SortByDuration:
					return Catalog.GetString ("Sort by duration");
				default:
					return Catalog.GetString ("Sort by name");
				}
			}
			set {
				if (value == Catalog.GetString ("Sort by start time"))
					SortMethod = SortMethodType.SortByStartTime;
				else if (value == Catalog.GetString ("Sort by stop time"))
					SortMethod = SortMethodType.SortByStopTime;
				else if (value == Catalog.GetString ("Sort by duration"))
					SortMethod = SortMethodType.SortByDuration;
				else
					SortMethod = SortMethodType.SortByName;
			}
		}
	}
	
	[Serializable]
	public class AnalysisEventType: EventType
	{
		public AnalysisEventType ()
		{
			Tags = new List<Tag> ();
		}

		public List<Tag> Tags {
			get;
			set;
		}
	}
	
	[Serializable]
	public class PenaltyCardEventType: EventType
	{
		public PenaltyCardEventType ()
		{
			ID = Constants.PenaltyCardID;
			Name = Catalog.GetString ("Penalty card");
		}

		public override bool Equals (object obj)
		{
			PenaltyCardEventType pc = obj as PenaltyCardEventType;
			if (pc == null)
				return false;
			return pc.ID == ID;
		}
		
		public override int GetHashCode ()
		{
			return ID.GetHashCode ();
		}
	}
	
	[Serializable]
	public class ScoreEventType: EventType
	{
		public ScoreEventType ()
		{
			ID = Constants.ScoreID;
			Name = Catalog.GetString ("Score");
		}

		public override bool Equals (object obj)
		{
			ScoreEventType sc = obj as ScoreEventType;
			if (sc == null)
				return false;
			return sc.ID == ID;
		}

		public override int GetHashCode ()
		{
			return ID.GetHashCode ();
		}
	}
}

