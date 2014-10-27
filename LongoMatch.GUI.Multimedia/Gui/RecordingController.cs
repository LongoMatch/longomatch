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
using System.Linq;
using System.Collections.Generic;
using LongoMatch.Core.Store;
using LongoMatch.Core.Interfaces.Multimedia;
using Mono.Unix;
using LongoMatch.Gui.Helpers;
using LongoMatch.Core.Common;
using Gtk;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RecordingController : Gtk.Bin
	{
		Period currentPeriod;
		uint timeoutID;
		TimeNode currentTimeNode;
		Time accumTime;
		DateTime currentPeriodStart;
		List<string> gamePeriods;

		public RecordingController ()
		{
			this.Build ();
			recbutton.Clicked += (sender, e) => StartPeriod ();
			stopbutton.Clicked += (sender, e) => StopPeriod ();
			pausebutton.Clicked += (sender, e) => PausePeriod ();
			resumebutton.Clicked += (sender, e) => ResumePeriod ();
			savebutton.Clicked += HandleSaveClicked;
			cancelbutton.Clicked += HandleCloseClicked;
			recimage.Pixbuf = Misc.LoadIcon ("longomatch-record",
			                                 StyleConf.PlayerCapturerIconSize);
			stopimage.Pixbuf = Misc.LoadIcon ("longomatch-stop",
			                                  StyleConf.PlayerCapturerIconSize);
			pauseimage.Pixbuf = Misc.LoadIcon ("longomatch-pause-clock",
			                                   StyleConf.PlayerCapturerIconSize);
			saveimage.Pixbuf = Misc.LoadIcon ("longomatch-save",
			                                  StyleConf.PlayerCapturerIconSize);
			resumeimage.Pixbuf = Misc.LoadIcon ("longomatch-pause-clock",
			                                    StyleConf.PlayerCapturerIconSize);
			cancelimage.Pixbuf = Misc.LoadIcon ("longomatch-cancel-rec",
			                                    StyleConf.PlayerCapturerIconSize);
			Periods = new List<Period>();
			hourseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			hourlabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			hourlabel.ModifyFont (Pango.FontDescription.FromString ("Ubuntu 24px"));
			hourseventbox.WidthRequest = 40;
			minuteseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			minuteslabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			minuteslabel.ModifyFont (Pango.FontDescription.FromString ("Ubuntu 24px"));
			minuteseventbox.WidthRequest = 40;
			secondseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundDark));
			secondslabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			secondslabel.ModifyFont (Pango.FontDescription.FromString ("Ubuntu 24px"));
			secondseventbox.WidthRequest = 40;
			label1.ModifyFont (Pango.FontDescription.FromString ("Ubuntu 24px"));
			label1.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			label2.ModifyFont (Pango.FontDescription.FromString ("Ubuntu 24px"));
			label2.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			periodlabel.ModifyFont (Pango.FontDescription.FromString ("Ubuntu 24px"));
			periodlabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
			Reset ();
		}
		
		protected override void OnDestroyed ()
		{
			if (timeoutID != 0) {
				GLib.Source.Remove (timeoutID);
			}
			base.OnDestroyed ();
		}

		public bool Capturing {
			get;
			set;
		}

		public List<string> GamePeriods {
			set {
				gamePeriods = value;
				if (gamePeriods != null && gamePeriods.Count > 0) {
					periodlabel.Markup = gamePeriods [0];
				} else {
					periodlabel.Markup = "1";
				}
			}
			get {
				return gamePeriods;
			}
		}

		public ICapturer Capturer {
			get;
			set;
		}
		
		public List<Period> Periods {
			set;
			get;
		}
		
		public void Reset () {
			currentPeriod = null;
			currentTimeNode = null;
			currentPeriodStart = DateTime.UtcNow;
			accumTime = new Time (0);
			Capturing = false;
			recbutton.Visible = true;
			stopbutton.Visible = false;
			pausebutton.Visible = false;
			savebutton.Visible = false;
			cancelbutton.Visible = true;
			resumebutton.Visible = false;
		}

		public void StartPeriod ()
		{
			string periodName;
			
			recbutton.Visible = false;
			pausebutton.Visible = savebutton.Visible = stopbutton.Visible = true;
			
			if (GamePeriods != null && GamePeriods.Count > Periods.Count) {
				periodName = GamePeriods [Periods.Count];
			} else {
				periodName = (Periods.Count + 1).ToString ();
			}
			currentPeriod = new Period { Name = periodName };
			
			currentTimeNode = currentPeriod.StartTimer (accumTime, periodName);
			currentTimeNode.Stop = currentTimeNode.Start;
			currentPeriodStart = DateTime.UtcNow;
			timeoutID = GLib.Timeout.Add (20, UpdateTime);
			if (Capturer != null) {
				if (Periods.Count == 0) {
					Capturer.Start ();
				} else {
					Capturer.TogglePause ();
				}
			}
			periodlabel.Markup = currentPeriod.Name;
			Capturing = true;
			Periods.Add (currentPeriod);
			Log.Debug ("Start new period start=", currentTimeNode.Start.ToMSecondsString());
		}

		public void StopPeriod ()
		{
			GLib.Source.Remove (timeoutID);
			if (currentPeriod != null) {
				currentPeriod.StopTimer (CurrentTime);
				accumTime = CurrentTime;
				Log.Debug ("Stop period stop=", accumTime.ToMSecondsString());
			}
			currentTimeNode = null;
			currentPeriod = null;
			
			recbutton.Visible = true;
			pausebutton.Visible = resumebutton.Visible = stopbutton.Visible = false;
			if (Capturer != null && Capturing) {
				Capturer.TogglePause ();
			}
			Capturing = false;
		}

		public void PausePeriod ()
		{
			if (currentPeriod != null) {
				Log.Debug ("Pause period at currentTime=", CurrentTime.ToMSecondsString());
				currentPeriod.PauseTimer (CurrentTime);
			}
			currentTimeNode = null;
			pausebutton.Visible = false;
			resumebutton.Visible = true;
			Capturing = false;
		}

		public void ResumePeriod ()
		{
			Log.Debug ("Resume period at currentTime=", CurrentTime.ToMSecondsString());
			currentTimeNode = currentPeriod.Resume (CurrentTime);
			pausebutton.Visible = true;
			resumebutton.Visible = false;
			Capturing = true;
		}

		Time CurrentTime {
			get {
				int timeDiff;
				
				timeDiff = (int)(DateTime.UtcNow - currentPeriodStart).TotalMilliseconds; 
				return (new Time (accumTime.MSeconds + timeDiff));
			}
		}

		public Time EllapsedTime {
			get {
				if (currentPeriod != null) {
					return currentPeriod.TotalTime;
				} else {
					return new Time (0);
				}
				
			}
		}
		
		bool UpdateTime () {
			if (currentTimeNode != null) {
				currentTimeNode.Stop = CurrentTime;
			}
			hourlabel.Markup = EllapsedTime.Hours.ToString ("d2");
			minuteslabel.Markup = EllapsedTime.Minutes.ToString ("d2");
			secondslabel.Markup = EllapsedTime.Seconds.ToString ("d2");
			Config.EventsBroker.EmitCapturerTick (new Time (Periods.Sum (p => p.TotalTime.MSeconds)));
			return true;
		}
		
		void HandleSaveClicked (object sender, EventArgs e)
		{
			string msg = Catalog.GetString ("Do you want to finish the current capture?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				Config.EventsBroker.EmitCaptureFinished (false);
			}
		}

		void HandleCloseClicked (object sender, EventArgs e)
		{
			string msg = Catalog.GetString ("Do you want to close and cancell the current capture?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				Config.EventsBroker.EmitCaptureFinished (true);
			}
		}
	}
}