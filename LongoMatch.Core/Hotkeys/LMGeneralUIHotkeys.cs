//
//  Copyright (C) 2017 FLUENDO S.A.
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
using VAS.Core;
using VAS.Core.Hotkeys;

namespace LongoMatch.Core.Hotkeys
{
	public static class LMGeneralUIHotkeys
	{
		static List<KeyConfig> hotkeys;
		public const string CATEGORY = "General Interface";

		// Keep this sorted alphabetically
		public const string EDIT_SELECTED_EVENT = "EDIT_SELECTED_EVENT";
		public const string SHOW_DASHBOARD = "SHOW_DASHBOARD";
		public const string SHOW_TIMELINE = "SHOW_TIMELINE";
		public const string SHOW_ZONAL_TAGS = "SHOW_ZONAL_TAGS";
		public const string START_HOMETEAM_TAGGING = "START_HOMETEAM_TAGGING";
		public const string START_AWAYTEAM_TAGGING = "START_AWAYTEAM_TAGGING";
		public const string START_RECORDING_PERIOD = "START_RECORDING_PERIOD";
		public const string STOP_RECORDING_PERIOD = "STOP_RECORDING_PERIOD";
		public const string TOGGLE_CAPTURE_CLOCK = "TOGGLE_CAPTURE_CLOCK";
		public const string TOGGLE_SUBSTITUTION_MODE = "TOGGLE_SUBSTITUTION_MODE";

		static LMGeneralUIHotkeys ()
		{
			hotkeys = new List<KeyConfig> {
				new KeyConfig {
					Name = SHOW_DASHBOARD,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+z"),
					Category = CATEGORY,
					Description = Catalog.GetString("Show dashboard")
				},
				new KeyConfig {
					Name = SHOW_TIMELINE,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+x"),
					Category = CATEGORY,
					Description = Catalog.GetString("Show timeline")
				},
				new KeyConfig {
					Name = SHOW_ZONAL_TAGS,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+c"),
					Category = CATEGORY,
					Description = Catalog.GetString("Show zonal tags")
				},
				new KeyConfig {
					Name = START_HOMETEAM_TAGGING,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+n"),
					Category = CATEGORY,
					Description = Catalog.GetString("Start tagging home player")
				},
				new KeyConfig {
					Name = START_AWAYTEAM_TAGGING,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+b"),
					Category = CATEGORY,
					Description = Catalog.GetString("Start tagging away player")
				},
				new KeyConfig {
					Name = TOGGLE_SUBSTITUTION_MODE,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+s"),
					Category = CATEGORY,
					Description = Catalog.GetString("Toggle substitutions mode")
				},
				new KeyConfig {
					Name = START_RECORDING_PERIOD,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+i"),
					Category = CATEGORY,
					Description = Catalog.GetString("Start recording period")
				},
				new KeyConfig {
					Name = STOP_RECORDING_PERIOD,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+o"),
					Category = CATEGORY,
					Description = Catalog.GetString("Stop recording period")
				},
				new KeyConfig {
					Name = TOGGLE_CAPTURE_CLOCK,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+p"),
					Category = CATEGORY,
					Description = Catalog.GetString("Pause/Resume capture clock")
				},
				new KeyConfig {
					Name = EDIT_SELECTED_EVENT,
					Key = App.Current.Keyboard.ParseName ("<Shift_L>+e"),
					Category = CATEGORY,
					Description = Catalog.GetString("Edit selected event")
				},
			};
		}

		/// <summary>
		/// Registers the default UI hotkeys
		/// </summary>
		public static void RegisterDefaultHotkeys ()
		{
			App.Current.HotkeysService.Register (hotkeys);
		}
	}
}
