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
using System.ComponentModel;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;

namespace LongoMatch.Drawing.CanvasObjects.Location
{
	/// <summary>
	/// A view to render a <see cref="TimelineEvent"/> as a position in a map.
	/// </summary>
	public class TimelineEventLocationView : LocationView, ICanvasObjectView<LMTimelineEventVM>
	{
		LMTimelineEventVM viewModel;

		protected override void DisposeManagedResources ()
		{
			ViewModel = null;
			base.DisposeManagedResources ();
		}

		override protected Color Color {
			get {
				return viewModel.Color;
			}
			set {
			}
		}

		override protected Color OutlineColor {
			get {
				return viewModel.TeamColor;
			}
			set {
			}
		}

		public LMTimelineEventVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					UpdatePoints ();
					viewModel.PropertyChanged += HandleViewModelPropertyChanged;
				}
			}
		}

		public override string Description {
			get {
				return ViewModel?.Name;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMTimelineEventVM)viewModel;
		}

		void UpdatePoints ()
		{
			// FIXME: Add postions to the VM
			Points = viewModel.Model.CoordinatesInFieldPosition (FieldPosition)?.Points;
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			// FIXME: Add positions to the VM
			if (FieldPosition == FieldPositionType.Field ||
				FieldPosition == FieldPositionType.HalfField ||
				FieldPosition == FieldPositionType.Goal) {
				UpdatePoints ();
				ReDraw ();
			}
		}
	}
}

