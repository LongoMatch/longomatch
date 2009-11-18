//
//  Copyright (C) 2009 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

using System;
using System.Collections.Generic;

namespace LongoMatch.TimeNodes
{


	public class DrawingsList
	{
		private SortedList<int,Drawing> drawingsList;

		public DrawingsList()
		{
			drawingsList = new SortedList<int,Drawing>();
		}

		public bool Add(Drawing drawing) {
			int stopTime = drawing.StopTime;
			if (!drawingsList.ContainsKey(stopTime)) {
				drawingsList.Add(stopTime,drawing);
				return true;
			}
			else return false;
		}

		public bool Remove(Drawing drawing) {
			int stopTime = drawing.StopTime;
			return drawingsList.Remove(stopTime);
		}

		protected void Clear() {
			drawingsList.Clear();
		}

		public int Count {
			get {
				return drawingsList.Count;
			}
		}

		public IList<int> StopTimes {
			get {
				return drawingsList.Keys;
			}
		}

		public int GetStopTime(int index) {
			return drawingsList.Keys[index];
		}

		public Drawing GetStopDrawing(int index) {
			return drawingsList.Values[index];
		}

		public SortedList<int,Drawing> List {
			get {
				return drawingsList;
			}
		}

	}
}
