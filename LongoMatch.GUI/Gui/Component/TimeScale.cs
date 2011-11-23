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
	public class TimeScale : TimeScaleBase<Play>
	{
		private Category category;
		
		public event NewTagAtFrameHandler NewMarkAtFrameEvent;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlaySelectedHandler TimeNodeSelected;
		public event PlaysDeletedHandler TimeNodeDeleted;


		public TimeScale(Category category, List<Play> list, uint frames): base(list, frames)
		{
			this.category = category;
			elementName = Catalog.GetString("play");
		}

		public void AddPlay(Play play) {
			AddTimeNode(play);
		}

		public void RemovePlay(Play play) {
			RemoveTimeNode(play);
		}

		override protected void HandleTimeNodeChanged(Play play, Time time) {
			if (TimeNodeChanged != null)
				TimeNodeChanged(play, time);
		}
		
		override protected void HandleTimeNodeSelected(Play play) {
			if (TimeNodeSelected != null)
				TimeNodeSelected(play);
		}
		
		override protected void HandleTimeNodeDeleted(List<Play> plays) {
			if (TimeNodeDeleted != null)
				TimeNodeDeleted(plays);
		}
		
		override protected void AddNewTimeNode() {
			if (NewMarkAtFrameEvent != null)
				NewMarkAtFrameEvent(category, cursorFrame);
		}
		
	}
}
