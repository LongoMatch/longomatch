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

namespace LongoMatch.Store
{


	public class DrawingsList
	{

		private SortedList<int,Drawing> drawingsList;

		public DrawingsList()
		{
			drawingsList = new SortedList<int,Drawing>();
		}
		
		/// <summary>
		/// Adds a new drawing to the list
		/// </summary>
		/// <param name="drawing">
		/// The <see cref="Drawing"/> to add 
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>: true if the frawing was added
		/// </returns>
		public bool Add(Drawing drawing) {
			int renderTime = drawing.RenderTime;
			if (!drawingsList.ContainsKey(renderTime)) {
				drawingsList.Add(renderTime,drawing);
				return true;
			}
			else return false;
		}

		/// <summary>
		/// Removes a drawing from the list
		/// </summary>
		/// <param name="drawing">
		/// A <see cref="Drawing"/> to remove
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>: true if was removed correctly
		/// </returns>
		public bool Remove(Drawing drawing) {
			int renderTime = drawing.RenderTime;
			return drawingsList.Remove(renderTime);
		}

		/// <summary>
		/// Clear the drawing list
		/// </summary>
		protected void Clear() {
			drawingsList.Clear();
		}

		/// <summary>
		/// The count of drawings
		/// </summary>
		public int Count {
			get {
				return drawingsList.Count;
			}
		}

		/// <summary>
		/// A list with all the render times
		/// </summary>
		public IList<int> RenderTime {
			get {
				return drawingsList.Keys;
			}
		}

		/// <summary>
		/// Gets the render time for a drawing at a position in the list
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/> with the index
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> with the render time
		/// </returns>
		public int GetRenderTime(int index) {
			return drawingsList.Keys[index];
		}

		/// <summary>
		/// Get the drawing for an the drawing at a position in the list
		/// </summary>
		/// <param name="index">
		/// A <see cref="System.Int32"/> with the index
		/// </param>
		/// <returns>
		/// A <see cref="Drawing"/> with the render time
		/// </returns>
		public Drawing GetRenderDrawing(int index) {
			return drawingsList.Values[index];
		}

		/// <summary>
		/// A list with all the drawings ordered by render time
		/// </summary>
		public SortedList<int,Drawing> List {
			get{
				return drawingsList;
			}
		}

	}
}
