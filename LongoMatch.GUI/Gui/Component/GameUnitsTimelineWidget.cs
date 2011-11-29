// TimeLineWidget.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;

using LongoMatch.Gui.Base;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component {

	public class GameUnitsTimelineWidget : TimelineBase<GameUnitTimeScale, TimelineNode> 
	{

		public event UnitChangedHandler UnitChanged;
		public event UnitSelectedHandler UnitSelected;
		public event UnitsDeletedHandler UnitDeleted;
		public event UnitAddedHandler UnitAdded;

		GameUnitsList gameUnits;

		public GameUnitsTimelineWidget(): base()
		{
		}

		public Project Project {
			set {
				ResetGui();

				if(value == null) {
					gameUnits = null;
					tsList.Clear();
					loaded = false;
					return;
				}
				loaded = true;
				gameUnits = value.GameUnits;
				tsList.Clear(); 
				frames = value.Description.File.GetFrames();

				cs.Labels = gameUnits.Select(g => g.Name).ToList(); 
				cs.Show();

				tr.Frames = frames;
				tr.FrameRate = value.Description.File.Fps;
				tr.Show();

				foreach(GameUnit gameUnit in gameUnits) {
					GameUnitTimeScale ts = new GameUnitTimeScale(gameUnit, frames);
					tsList[gameUnit] = ts;
					ts.UnitAdded += HandleUnitAdded;
					ts.UnitDeleted += HandleUnitDeleted;
					ts.UnitChanged += HandleUnitChanged;
					ts.UnitSelected += HandleUnitSelected;
					TimelineBox.PackStart(ts,false,true,0);
					ts.Show();
				}
				SetPixelRatio(3);
			}
		}

		public void AddUnit(GameUnit gameUnit, TimelineNode unit) {
			GameUnitTimeScale ts;
			if(tsList.TryGetValue(gameUnit, out ts))
				ts.AddUnit(unit);
		}

		public void RemoveUnit(GameUnit gameUnit, List<TimelineNode> units) {
			GameUnitTimeScale ts;
			foreach(TimelineNode unit in units) {
				if(tsList.TryGetValue(gameUnit, out ts))
					ts.RemoveUnit(unit);
			}
		}
		
		void HandleUnitSelected (GameUnit gameUnit, TimelineNode unit)
		{
			if (UnitSelected != null)
				UnitSelected(gameUnit, unit);
		}

		void HandleUnitChanged (GameUnit gameUnit, TimelineNode unit, Time time)
		{
			if (UnitChanged != null)
				UnitChanged(gameUnit, unit, time);
		}

		void HandleUnitDeleted (GameUnit gameUnit, List<TimelineNode> unit)
		{
			if (UnitDeleted != null)
				UnitDeleted(gameUnit, unit);
		}

		void HandleUnitAdded (GameUnit gameUnit, int frame)
		{
			if (UnitAdded != null)
				UnitAdded(gameUnit, frame);
		}
	}
}
