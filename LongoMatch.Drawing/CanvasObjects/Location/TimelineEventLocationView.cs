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
using System.ComponentModel;
using System.Linq;
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
					Points = EventPoints;
					viewModel.PropertyChanged += HandleViewModelPropertyChanged;
				}
			}
		}

		override protected Color Color {
			get {
				return ViewModel?.Color;
			}
			set {
			}
		}

		override protected Color OutlineColor {
			get {
				return ViewModel?.TeamColor;
			}
			set {
			}
		}

		public override string Description {
			get {
				return ViewModel?.Name;
			}
		}

		public override bool Visible {
			get {
				return ViewModel?.Visible ?? false;
			}
		}

		IList<Point> EventPoints { get => ViewModel.Model.CoordinatesInFieldPosition (FieldPosition)?.Points; }

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMTimelineEventVM)viewModel;
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Visible)) ||
				ViewModel.NeedsSync (e, nameof (ViewModel.Color)) ||
				ViewModel.NeedsSync (e, $"Collection_{nameof (ViewModel.FieldPosition.Points)}")) {
				// FIXME: Add positions to the VM
				var eventPoints = EventPoints;
				if (eventPoints?.Any () ?? false) {
					Points = eventPoints;
					ReDraw ();
				}
			}
		}
	}
}

