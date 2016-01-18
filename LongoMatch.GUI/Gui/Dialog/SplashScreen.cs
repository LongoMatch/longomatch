//
//  Copyright (C) 2015 Fluendo S.A.
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
using System;
using System.Collections.Generic;
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Gui.Dialog
{
	public partial class SplashScreen : Gtk.Window, IProgressReport
	{

		Dictionary<Guid, ProgressStatus> statusDict;
		IProgress<ProgressStatus> progress;

		public SplashScreen () : base (Gtk.WindowType.Toplevel)
		{
			Build ();

			LongoMatch.Core.Common.Image image = Resources.LoadImage (Constants.SPLASH);
			splashimage.WidthRequest = WidthRequest = image.Width;
			splashimage.HeightRequest = HeightRequest = image.Height;
			progressbar1.WidthRequest = WidthRequest * 60 / 100;
			progressbar1.HeightRequest = 20;

			Fixed.FixedChild w1 = (Fixed.FixedChild)(fixed1 [progressbar1]);
			w1.X = WidthRequest * 20 / 100;
			w1.Y = HeightRequest - 50;

			splashimage.Pixbuf = image.Value;
			Resizable = false;
			Decorated = false;
			SetPosition (Gtk.WindowPosition.CenterAlways);

			// HACK: Center window in OS X
			if (Utils.OS == OperatingSystemID.OSX) {
				Screen screen = Display.Default.DefaultScreen;
				int monitor = screen.GetMonitorAtWindow (this.GdkWindow);
				Rectangle monitor_geometry = screen.GetMonitorGeometry (monitor);
				Move (monitor_geometry.Width * 10 / 100, monitor_geometry.Height * 10 / 100);
			}

			statusDict = new Dictionary<Guid, ProgressStatus> ();
			progress = new Progress <ProgressStatus> (ProcessUpdate);
		}

		#region IProgressReport implementation

		public void Report (float percent, string message, Guid id = default(Guid))
		{
			progress.Report (new ProgressStatus (percent, message, id));
		}

		#endregion

		void ProcessUpdate (ProgressStatus status)
		{
			statusDict [status.ID] = status;
			progressbar1.Text = status.Message;
			progressbar1.Fraction = statusDict.Values.Sum (s => s.Percent) / statusDict.Count;
		}

		class ProgressStatus
		{
			public ProgressStatus (float percent, string message, Guid id)
			{
				Percent = percent;
				Message = message;
				ID = id;
			}

			public float Percent {
				get;
				set;
			}

			public string Message {
				get;
				set;
			}

			public Guid ID {
				get;
				set;
			}
		}
	}

}

