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
using System.Linq;
using LongoMatch.Core.Common;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	[Serializable]
	public class EventType: StorableBase
	{

		public EventType ()
		{
			ID = Guid.NewGuid ();
			Color = Color.Red.Copy (true);
		}

		public virtual string Name {
			get;
			set;
		}

		public virtual Color Color {
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
		[PropertyChanged.DoNotNotify]
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
		ObservableCollection<Tag> tags;

		public AnalysisEventType ()
		{
			Tags = new ObservableCollection<Tag> ();
		}

		public ObservableCollection<Tag> Tags {
			get {
				return tags;
			}
			set {
				if (tags != null) {
					tags.CollectionChanged -= ListChanged;
				}
				tags = value;
				if (tags != null) {
					tags.CollectionChanged += ListChanged;
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Dictionary<string, List<Tag>> TagsByGroup {
			get {
				return Tags.GroupBy (t => t.Group).ToDictionary (g => g.Key, g => g.ToList ());
			}
		}

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
	}

	[Serializable]
	public class PenaltyCardEventType: EventType
	{
		public override Color Color {
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

		public PenaltyCard PenaltyCard {
			get;
			set;
		}
	}

	[Serializable]
	public class ScoreEventType: EventType
	{
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

		public Score Score {
			get;
			set;
		}
	}

	[Serializable]
	public class SubstitutionEventType: EventType
	{
		public SubstitutionEventType ()
		{
			ID = Constants.SubsID;
			Name = Catalog.GetString ("Substitution");
		}

		public override bool Equals (object obj)
		{
			SubstitutionEventType sc = obj as SubstitutionEventType;
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

