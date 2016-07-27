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
using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using VAS.Core.Interfaces.GUI;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Dialog
{
	public class BusyDialog: Gtk.Dialog, IBusyDialog
	{
		VBox box;
		Label titleLabel;
		ProgressBar progressBar;
		uint timeout;
		object lockObject;

		public BusyDialog (Window parent)
		{
			TransientFor = parent;
			box = new VBox (false, 10);
			titleLabel = new Label ();
			progressBar = new ProgressBar ();
			box.PackStart (titleLabel, true, true, 0);
			box.PackStart (progressBar, true, true, 0);
			box.ShowAll ();
			VBox.PackStart (box);
			Icon = Helpers.Misc.LoadIcon ("longomatch", 28);
			TypeHint = Gdk.WindowTypeHint.Dialog;
			WindowPosition = WindowPosition.Center;
			Modal = true;
			Resizable = false;
			Gravity = Gdk.Gravity.Center; 
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			DefaultWidth = 300;
			DefaultHeight = 100;
			timeout = 0;
		}

		protected override void OnDestroyed ()
		{
			if (timeout != 0) {
				GLib.Source.Remove (timeout);
			}
			base.OnDestroyed ();
		}

		public string Message {
			set {
				titleLabel.Text = value;
			}
		}

		public void Pulse ()
		{
			progressBar.Pulse ();
		}

		public void ShowSync (System.Action action, uint pulseIntervalMS = 100)
		{
			Exception ex = null;
			object lockObject = this.lockObject = new object ();

			if (pulseIntervalMS == 0) {
				pulseIntervalMS = 100;
			}
			Task task = new Task (() => {

				try {
					action.Invoke ();
				} catch (Exception e) {
					ex = e;
				} finally {
					App.Current.GUIToolkit.Invoke (delegate {
						lock (lockObject) {
							Destroy ();
						}
					});
				}
			});
			Monitor.Enter (lockObject);
			task.Start ();
			timeout = GLib.Timeout.Add (pulseIntervalMS, OnTimeout);
			Run ();
			if (ex != null)
				throw ex;
		}

		public void Show (uint pulseIntervalMS = 0)
		{
			timeout = GLib.Timeout.Add (pulseIntervalMS, OnTimeout);
		}

		bool OnTimeout ()
		{
			if (lockObject != null) {
				Monitor.Exit (lockObject);
				lockObject = null;
			}
			Pulse ();
			return true;
		}
	}
}

