// TimeScale.cs
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
using Cairo;
using Gdk;
using Gtk;
using Pango;
using Mono.Unix;
using LongoMatch.Common;
using LongoMatch.Gui.Base;
using LongoMatch.Handlers;
using LongoMatch.Store;


namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class GameUnitTimeScale : TimeScaleBase<TimelineNode>
	{
		
		public event UnitChangeHandler UnitChanged;
		public event UnitSelectedHandler UnitSelected;
		public event UnitsDeletedHandler UnitDeleted;
		public event UnitAddedHandler UnitAdded;

		GameUnit gameUnit;
		
		public GameUnitTimeScale(GameUnit gameUnit, uint frames): base(gameUnit, frames)
		{
			this.gameUnit = gameUnit;
			elementName = Catalog.GetString("play");
		}

		public void AddUnit(TimelineNode unit) {
			AddTimeNode(unit);
		}

		public void RemoveUnit(TimelineNode unit) {
			RemoveTimeNode(unit);
		}

		override protected void HandleTimeNodeChanged(TimelineNode unit, Time time) {
			if (UnitChanged != null)
				UnitChanged(unit, time);
		}
		
		override protected void HandleTimeNodeSelected(TimelineNode unit) {
			if (UnitSelected != null)
				UnitSelected(unit);
		}
		
		override protected void HandleTimeNodeDeleted(List<TimelineNode> units) {
			if (UnitDeleted != null)
				UnitDeleted(units);
		}
		
		override protected void AddNewTimeNode() {
			if (UnitAdded != null)
				UnitAdded(gameUnit, cursorFrame);
		}
		
	}
}