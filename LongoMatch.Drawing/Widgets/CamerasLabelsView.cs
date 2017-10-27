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
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.ViewModel;
using VAS.Drawing;
using VAS.Drawing.CanvasObjects.Timeline;

namespace LongoMatch.Drawing.Widgets
{
	public class CamerasLabelsView : Canvas, ICanvasView<MediaFileSetVM>
	{
		MediaFileSetVM fileSetVM;

		public CamerasLabelsView (IWidget widget) : base (widget)
		{
		}

		public CamerasLabelsView () : this (null)
		{
		}

		public double Scroll {
			set {
				foreach (var o in Objects) {
					LabelView cl = o as LabelView;
					cl.Scroll = value;
				}
			}
		}

		public MediaFileSetVM ViewModel {
			get {
				return fileSetVM;
			}
			set {
				fileSetVM = value;
				ClearObjects ();
				FillCanvas ();
				widget?.ReDraw ();
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (MediaFileSetVM)viewModel;
		}

		void AddLabel (LabelView label)
		{
			Objects.Add (label);
		}

		void AddCamera (MediaFileVM fileVM, int width, int height, ref int row)
		{
			var l = App.Current.ViewLocator.Retrieve ("CameraLabelView") as CameraLabelView;
			l.Width = width;
			l.Height = height;
			l.OffsetY = row * height;
			l.BackgroundColor = App.Current.Style.ThemeContrastDisabled;
			l.ViewModel = fileVM;
			AddLabel (l);
			row++;
		}

		void FillCanvas ()
		{
			int row = 0, w, h, height = 0;

			w = StyleConf.TimelineLabelsWidth * 2;
			h = StyleConf.TimelineCameraHeight;

			// Main camera
			AddCamera (fileSetVM.ViewModels [0], w, h, ref row);

			// Periods
			var l = new LabelView ();
			l.Width = w;
			l.Height = h;
			l.OffsetY = row * h;
			l.BackgroundColor = App.Current.Style.ThemeContrastDisabled;
			l.Name = Catalog.GetString ("Periods");
			AddLabel (l);
			row++;

			// Secondary cams
			for (int j = 1; j < fileSetVM.ViewModels.Count; j++) {
				AddCamera (fileSetVM.ViewModels [j], w, h, ref row);
			}

			double width = Objects.Max (la => (la as LabelView).RequiredWidth);
			foreach (LabelView label in Objects) {
				label.Width = width;
				height += h;
			}
			WidthRequest = (int)width;
			HeightRequest = (int)height;
		}
	}
}

