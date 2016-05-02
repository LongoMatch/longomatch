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
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Services;
using LMCommon = LongoMatch.Core.Common;
using Timer = System.Threading.Timer;

namespace LongoMatch.Services
{
	public class PlaylistManager: PlaylistManagerBase
	{
		EventsFilter filter;

		public PlaylistManager () : base ()
		{
		}

		public override IPlayerController Player {
			get;
			set;
		}

		public override Project OpenedProject {
			get;
			set;
		}

		public override ProjectType OpenedProjectType {
			get;
			set;
		}

		protected override void HandlePlayChanged (TimeNode tNode, Time time)
		{
			if (tNode is TimelineEventLongoMatch) {
				LoadPlay (tNode as TimelineEventLongoMatch, time, false);
				if (filter != null) {
					filter.Update ();
				}
			}
		}

		protected override void HandleLoadPlayEvent (TimelineEvent play)
		{
			if (OpenedProject == null || OpenedProjectType == ProjectType.FakeCaptureProject) {
				return;
			}

			if (play is SubstitutionEvent || play is LineupEvent) {
				//FIXME: This switch bugs me, it's the only one here...
				Player.Switch (null, null, null);
				((LMCommon.EventsBroker)Config.EventsBroker).EmitEventLoaded (null);
				Player.Seek (play.EventTime, true);
				Player.Play ();
			} else {
				if (play != null) {
					LoadPlay (play as TimelineEventLongoMatch, new Time (0), true);
				} else if (Player != null) {
					Player.UnloadCurrentEvent ();
				}
				((LMCommon.EventsBroker)Config.EventsBroker).EmitEventLoaded (play as TimelineEventLongoMatch);
			}
		}

		protected override void HandleKeyPressed (object sender, HotKey key)
		{
			if (OpenedProject == null && Player?.LoadedPlaylist == null)
				return;

			if ((OpenedProjectType != ProjectType.CaptureProject &&
			    OpenedProjectType != ProjectType.URICaptureProject &&
			    OpenedProjectType != ProjectType.FakeCaptureProject) || Player.LoadedPlaylist != null) {
				KeyAction action;
				if (Player == null)
					return;

				try {
					action = Config.Hotkeys.ActionsHotkeys.GetKeyByValue (key);
				} catch (Exception ex) {
					/* The dictionary contains 2 equal values for different keys */
					Log.Exception (ex);
					return;
				}
				
				if (action == KeyAction.None) {
					return;
				}

				switch (action) {
				case KeyAction.FrameUp:
					Player.SeekToNextFrame ();
					return;
				case KeyAction.FrameDown:
					Player.SeekToPreviousFrame ();
					return;
				case KeyAction.JumpUp:
					Player.StepForward ();
					return;
				case KeyAction.JumpDown:
					Player.StepBackward ();
					return;
				case KeyAction.DrawFrame:
					Player.DrawFrame ();
					return;
				case KeyAction.TogglePlay:
					Player.TogglePlay ();
					return;
				case KeyAction.SpeedUp:
					Player.FramerateUp ();
					((LMCommon.EventsBroker)Config.EventsBroker).EmitPlaybackRateChanged ((float)Player.Rate);
					return;
				case KeyAction.SpeedDown:
					Player.FramerateDown ();
					((LMCommon.EventsBroker)Config.EventsBroker).EmitPlaybackRateChanged ((float)Player.Rate);
					return;
				case KeyAction.CloseEvent:
					((LMCommon.EventsBroker)Config.EventsBroker).EmitLoadEvent (null);
					return;
				case KeyAction.Prev:
					HandlePrev (null);
					return;
				case KeyAction.Next:
					HandleNext (null);
					return;
				}
			} else {
				//if (Capturer == null)
				//	return;
			}
		}

		#region IService

		public override int Level {
			get {
				return 80;
			}
		}

		public  override string Name {
			get {
				return "Playlists";
			}
		}

		#endregion
	}
}
