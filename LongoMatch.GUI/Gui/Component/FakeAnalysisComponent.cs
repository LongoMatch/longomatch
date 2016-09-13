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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using LMFilters = LongoMatch.Core.Filters;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class FakeAnalysisComponent : Gtk.Bin,  IAnalysisWindow
	{

		public FakeAnalysisComponent ()
		{
			this.Build ();
			capturerbin.Mode = CapturerType.Fake;
			App.Current.EventsBroker.Subscribe<EventCreatedEvent> (HandleEventCreated);
			App.Current.EventsBroker.Subscribe<EventsDeletedEvent> (HandleEventsDeleted);
		}

		public void Dispose ()
		{
			Destroy ();
		}

		protected override void OnDestroyed ()
		{
			OnUnload ();
			App.Current.EventsBroker.Unsubscribe<EventCreatedEvent> (HandleEventCreated);
			App.Current.EventsBroker.Unsubscribe<EventsDeletedEvent> (HandleEventsDeleted);
		}

		#region IAnalysisWindow implementation

		public event VAS.Core.Handlers.BackEventHandle BackEvent;

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void SetViewModel (object viewModel)
		{
			throw new System.NotImplementedException ();
		}

		public string PanelName {
			get {
				return null;
			}
			set {
			}
		}

		public void SetProject (Project project, ProjectType projectType, CaptureSettings props, EventsFilter filter)
		{
			codingwidget1.SetProject ((LongoMatch.Core.Store.ProjectLongoMatch)project, projectType, 
				(LMFilters.EventsFilter)filter);
		}

		public void ReloadProject ()
		{
		}

		public void CloseOpenedProject ()
		{
		}

		public void UpdateCategories ()
		{
			codingwidget1.UpdateCategories ();
		}

		public void DetachPlayer ()
		{
		}

		public void ZoomIn ()
		{
		}

		public void ZoomOut ()
		{
		}

		public void FitTimeline ()
		{
		}

		public void ShowDashboard ()
		{
			codingwidget1.ShowDashboard ();
		}

		public void ShowTimeline ()
		{
			codingwidget1.ShowTimeline ();
		}

		public void ShowZonalTags ()
		{
			codingwidget1.ShowZonalTags ();
		}

		public void ClickButton (DashboardButton button, Tag tag = null)
		{
			codingwidget1.ClickButton (button, tag);
		}

		public void TagPlayer (Player player)
		{
			codingwidget1.TagPlayer ((PlayerLongoMatch)player);
		}

		public void TagTeam (TeamType team)
		{
			codingwidget1.TagTeam (team);
		}

		public IPlayerController Player {
			get {
				return null;
			}
		}

		public ICapturerBin Capturer {
			get {
				return capturerbin;
			}
		}

		#endregion

		void HandleEventCreated (EventCreatedEvent e)
		{
			codingwidget1.AddPlay ((TimelineEventLongoMatch)e.TimelineEvent);
		}

		void HandleEventsDeleted (EventsDeletedEvent e)
		{
			codingwidget1.DeletePlays (e.TimelineEvents.Cast<TimelineEventLongoMatch> ().ToList ());
		}
	}
}
