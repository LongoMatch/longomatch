// HotKeysManager.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using System;

#if HAVE_GTK
using Gdk;
using Gtk;

#endif
namespace LongoMatch.Services
{
	public class HotKeysManager
	{
		Dictionary<HotKey, DashboardButton> dic;
		IAnalysisWindow analysisWindow;
		bool ignoreKeys;

		public HotKeysManager ()
		{
			dic = new Dictionary<HotKey,DashboardButton> ();
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			Config.EventsBroker.KeyPressed += KeyListener;
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 EventsFilter filter, IAnalysisWindow analysisWindow)
		{
			this.analysisWindow = analysisWindow;
			if (project == null) {
				ignoreKeys = true;
				return;
			}
			
			dic.Clear ();
			ignoreKeys = false;
			foreach (DashboardButton cat in project.Dashboard.List) {
				if (cat.HotKey.Defined &&
					!dic.ContainsKey (cat.HotKey))
					dic.Add (cat.HotKey, cat);
			}
		}

		public void KeyListener (object sender, HotKey key)
		{
			KeyAction action;

			try {
				action = Config.Hotkeys.ActionsHotkeys.GetKeyByValue (key);
			} catch (Exception ex) {
				/* The dictionary contains 2 equal values for different keys */
				Log.Exception (ex);
				return;
			}
			
			if (action != KeyAction.None && analysisWindow != null) {
				switch (action) {
				case KeyAction.ZoomIn:
					analysisWindow.ZoomIn ();
					return;
				case KeyAction.ZoomOut:
					analysisWindow.ZoomOut ();
					return;
				case KeyAction.ShowDashboard:
					analysisWindow.ShowDashboard ();
					return;
				case KeyAction.ShowTimeline:
					analysisWindow.ShowTimeline ();
					return;
				case KeyAction.ShowPositions:
					analysisWindow.ShowZonalTags ();
					return;
				case KeyAction.FitTimeline:
					analysisWindow.FitTimeline ();
					return;
				}
			}
			if (ignoreKeys)
				return;
		}
	}
}
