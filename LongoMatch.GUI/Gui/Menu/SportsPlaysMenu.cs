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
using System.Linq;
using Gtk;
using LongoMatch.Core.Store;
using VAS;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Store;
using VAS.UI.Menus;

namespace LongoMatch.Gui.Menus
{
	public class SportsPlaysMenu : PlaysMenu
	{
		protected new void ShowMenu (Project project, IEnumerable<TimelineEvent> plays, EventType eventType, Time time,
										  IList<EventType> eventTypes, bool editableName)
		{
			PrepareMenu (project, plays, eventType, time, eventTypes, editableName);
			Popup ();
		}

		public override void ShowMenu (Project project, List<TimelineEvent> plays)
		{
			ShowMenu (project, plays, null, null, project.EventTypes, true);
		}

		protected override void PrepareMenu (Project project, IEnumerable<TimelineEvent> plays, EventType eventType, Time time,
										 IList<EventType> eventTypes, bool editableName)
		{
			bool isLineup = false, isSubstitution = false;

			this.plays = plays.ToList ();
			this.eventType = eventType;
			this.time = time;
			this.project = project;

			if (eventType != null) {
				string label = String.Format ("{0} in {1}", Catalog.GetString ("Add new event"), eventType.Name);
				newPlay.SetLabel (label);
				newPlay.Visible = true;
			} else {
				newPlay.Visible = false;
			}

			if (plays == null) {
				plays = new List<TimelineEvent> ();
			} else if (plays.Count () == 1) {
				isLineup = plays.FirstOrDefault () is LineupEvent;
				isSubstitution = plays.FirstOrDefault () is SubstitutionEvent;
			}

			MenuHelpers.FillExportToVideoFileMenu (render, project, plays, Catalog.GetString ("Export to video file"));

			if (isLineup || isSubstitution) {
				edit.Visible = true;
				del.Visible = isSubstitution;
				snapshot.Visible = moveCat.Visible = drawings.Visible =
					addPLN.Visible = render.Visible = duplicate.Visible = false;
			} else {
				edit.Visible = editableName && this.plays.Count == 1;
				snapshot.Visible = this.plays.Count == 1;
				drawings.Visible = this.plays.Count == 1 && this.plays.FirstOrDefault ().Drawings.Count > 0;
				moveCat.Visible = del.Visible = addPLN.Visible = duplicate.Visible = this.plays.Any ();
			}

			if (plays.Count () > 0) {
				string label = String.Format ("{0} ({1})", Catalog.GetString ("Delete"), plays.Count ());
				del.SetLabel (label);
				label = String.Format ("{0} ({1})", Catalog.GetString ("Duplicate "), plays.Count ());
				duplicate.SetLabel (label);
			}

			if (moveCat.Visible) {
				Menu catMenu = new Menu ();
				foreach (EventType c in eventTypes) {
					if (plays.Any (p => p.EventType == c))
						continue;
					var item = new MenuItem (c.Name);
					catMenu.Append (item);
					item.Activated += (sender, e) => {
						App.Current.EventsBroker.Publish<MoveToEventTypeEvent> (
							new MoveToEventTypeEvent {
								TimelineEvents = plays.ToList (),
								EventType = c
							}
						);
					};
				}
				catMenu.ShowAll ();
				moveCat.Submenu = catMenu;
			}

			if (drawings.Visible) {
				Menu drawingsMenu = new Menu ();
				for (int i = 0; i < plays.FirstOrDefault ().Drawings.Count; i++) {
					int index = i;
					MenuItem drawingItem = new MenuItem (Catalog.GetString ("Drawing ") + (i + 1));
					MenuItem editItem = new MenuItem (Catalog.GetString ("Edit"));
					MenuItem deleteItem = new MenuItem (Catalog.GetString ("Delete"));
					Menu drawingMenu = new Menu ();

					drawingsMenu.Append (drawingItem);
					drawingMenu.Append (editItem);
					drawingMenu.Append (deleteItem);
					editItem.Activated += (sender, e) => {
						var play = plays.FirstOrDefault ();
						App.Current.EventsBroker.Publish (
							new DrawFrameEvent {
								Play = play,
								DrawingIndex = index,
								CamConfig = play.Drawings [index].CameraConfig,
							}
						);
					};
					deleteItem.Activated += (sender, e) => {
						plays.FirstOrDefault ().Drawings.RemoveAt (index);
						plays.FirstOrDefault ().UpdateMiniature ();
					};
					drawingItem.Submenu = drawingMenu;
					drawingMenu.ShowAll ();
				}
				drawingsMenu.ShowAll ();
				drawings.Submenu = drawingsMenu;
			}

			if (!IsLineupEvent ()) {
				MenuHelpers.FillAddToPlaylistMenu (addPLN, project.Playlists, this.plays);
			}
		}

		protected override void CreateMenu ()
		{
			base.CreateMenu ();

			edit = new MenuItem (Catalog.GetString ("Edit properties"));
			edit.Activated += (sender, e) => {
				App.Current.EventsBroker.Publish (
				new EditEventEvent {
					TimelineEvent = plays.Single ()
				});
			};
			Add (edit);

			duplicate = new MenuItem ("");
			duplicate.Activated += (sender, e) => App.Current.EventsBroker.Publish<DuplicateEventsEvent> (
					new DuplicateEventsEvent {
						TimelineEvents = plays
					}
				);
			Add (duplicate);

			moveCat = new MenuItem (Catalog.GetString ("Move to"));
			Add (moveCat);

			drawings = new MenuItem (Catalog.GetString ("Drawings"));
			Add (drawings);

			addPLN = new MenuItem ("Add to playlist");
			Add (addPLN);

			render = new MenuItem ("");
			render.Activated += (sender, e) => MenuHelpers.EmitRenderPlaylist (plays);
			Add (render);

			snapshot = new MenuItem (Catalog.GetString ("Export to PNG images"));
			snapshot.Activated += (sender, e) => App.Current.EventsBroker.Publish<SnapshotSeriesEvent> (
				new SnapshotSeriesEvent {
					TimelineEvent = plays.FirstOrDefault ()
				}
			);
			Add (snapshot);

			ShowAll ();
		}

		bool IsLineupEvent ()
		{
			return plays.Any (p => p is LineupEvent || p is SubstitutionEvent);
		}
	}
}
