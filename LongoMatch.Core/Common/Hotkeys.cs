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
using Newtonsoft.Json;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Core.Common
{
	
	public class Hotkeys
	{
		public Hotkeys ()
		{
			ActionsDescriptions = new Dictionary<KeyAction, string> ();
			ActionsHotkeys = new Dictionary<KeyAction, HotKey> ();
			FillActionsDescriptions ();
			FillDefaultMappings ();
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Dictionary<KeyAction, string> ActionsDescriptions {
			get;
			set;
		}

		public Dictionary<KeyAction, HotKey> ActionsHotkeys {
			get;
			set;
		}

		void UpdateMapping (KeyAction action, string name)
		{
			HotKey key = Keyboard.ParseName (name);
			ActionsHotkeys [action] = key;
		}

		public void FillActionsDescriptions ()
		{
			ActionsDescriptions [KeyAction.DeleteEvent] = Catalog.GetString ("Delete selected event");
			ActionsDescriptions [KeyAction.DrawFrame] = Catalog.GetString ("Draw frame");
			ActionsDescriptions [KeyAction.EditEvent] = Catalog.GetString ("Edit selected event");
			ActionsDescriptions [KeyAction.FitTimeline] = Catalog.GetString ("Adjust timeline to current position");
			ActionsDescriptions [KeyAction.FrameDown] = Catalog.GetString ("Frame step backward");
			ActionsDescriptions [KeyAction.FrameUp] = Catalog.GetString ("Frame step forward");
			ActionsDescriptions [KeyAction.JumpDown] = Catalog.GetString ("Jump backward");
			ActionsDescriptions [KeyAction.JumpUp] = Catalog.GetString ("Jump forward");
			ActionsDescriptions [KeyAction.CloseEvent] = Catalog.GetString ("Close loaded event");
			ActionsDescriptions [KeyAction.LocalPlayer] = Catalog.GetString ("Start tagging home player");
			ActionsDescriptions [KeyAction.VisitorPlayer] = Catalog.GetString ("Start tagging away player");
			ActionsDescriptions [KeyAction.Next] = Catalog.GetString ("Jump to next event");
			ActionsDescriptions [KeyAction.Prev] = Catalog.GetString ("Jump to prev event");
			ActionsDescriptions [KeyAction.ShowDashboard] = Catalog.GetString ("Show dashboard");
			ActionsDescriptions [KeyAction.ShowPositions] = Catalog.GetString ("Show zonal tags");
			ActionsDescriptions [KeyAction.ShowTimeline] = Catalog.GetString ("Show timeline");
			ActionsDescriptions [KeyAction.LocalPlayer] = Catalog.GetString ("Start tagging home player");
			ActionsDescriptions [KeyAction.VisitorPlayer] = Catalog.GetString ("Start tagging away player");
			ActionsDescriptions [KeyAction.SpeedDown] = Catalog.GetString ("Increase playback speed");
			ActionsDescriptions [KeyAction.SpeedUp] = Catalog.GetString ("Decrease playback speed");
			ActionsDescriptions [KeyAction.PauseClock] = Catalog.GetString ("Pause/Resume capture clock");
			ActionsDescriptions [KeyAction.StartPeriod] = Catalog.GetString ("Start recording period");
			ActionsDescriptions [KeyAction.StopPeriod] = Catalog.GetString ("Stop recording period");
			ActionsDescriptions [KeyAction.Substitution] = Catalog.GetString ("Toggle substitutions mode");
			ActionsDescriptions [KeyAction.TogglePlay] = Catalog.GetString ("Toggle playback");
			ActionsDescriptions [KeyAction.ZoomIn] = Catalog.GetString ("Zoom timeline in");
			ActionsDescriptions [KeyAction.ZoomOut] = Catalog.GetString ("Zoom timeline out");
			ActionsDescriptions [KeyAction.SpeedUpper] = Catalog.GetString ("Maximum playback speed");
			ActionsDescriptions [KeyAction.SpeedLower] = Catalog.GetString ("Default playback speed");
		}

		void FillDefaultMappings ()
		{
			UpdateMapping (KeyAction.DeleteEvent, "<Shift_L>+d");
			UpdateMapping (KeyAction.DrawFrame, "<Shift_L>+f");
			UpdateMapping (KeyAction.EditEvent, "<Shift_L>+e");
			UpdateMapping (KeyAction.FitTimeline, "<Shift_L>+t");
			UpdateMapping (KeyAction.FrameDown, "Left");
			UpdateMapping (KeyAction.FrameUp, "Right");
			UpdateMapping (KeyAction.JumpUp, "<Shift_L>+Right");
			UpdateMapping (KeyAction.JumpDown, "<Shift_L>+Left");
			UpdateMapping (KeyAction.CloseEvent, "<Shift_L>+a");
			UpdateMapping (KeyAction.LocalPlayer, "<Shift_L>+q");
			UpdateMapping (KeyAction.VisitorPlayer, "<Shift_L>+w");
			UpdateMapping (KeyAction.LocalPlayer, "<Shift_L>+n");
			UpdateMapping (KeyAction.VisitorPlayer, "<Shift_L>+b");
			UpdateMapping (KeyAction.PauseClock, "<Shift_L>+p");
			UpdateMapping (KeyAction.ShowDashboard, "<Shift_L>+z");
			UpdateMapping (KeyAction.ShowTimeline, "<Shift_L>+x");
			UpdateMapping (KeyAction.ShowPositions, "<Shift_L>+c");
			UpdateMapping (KeyAction.SpeedDown, "Down");
			UpdateMapping (KeyAction.SpeedUp, "Up");
			UpdateMapping (KeyAction.StartPeriod, "<Shift_L>+i");
			UpdateMapping (KeyAction.StopPeriod, "<Shift_L>+o");
			UpdateMapping (KeyAction.Substitution, "<Shift_L>+s");
			UpdateMapping (KeyAction.TogglePlay, "space");
			UpdateMapping (KeyAction.ZoomIn, "plus");
			UpdateMapping (KeyAction.ZoomOut, "minus");
			UpdateMapping (KeyAction.Next, "<Alt_L>+Right");
			UpdateMapping (KeyAction.Prev, "<Alt_L>+Left");
			UpdateMapping (KeyAction.SpeedUpper, "<Shift_L>+<Alt_L>+Up");
			UpdateMapping (KeyAction.SpeedLower, "<Shift_L>+<Alt_L>+Down");
		}
	}
}

