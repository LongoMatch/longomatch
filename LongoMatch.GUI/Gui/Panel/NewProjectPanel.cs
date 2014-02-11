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
using System.Collections.Generic;
using Gtk;
using Gdk;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Store;
using LongoMatch.Common;
using LongoMatch.Store.Templates;
using LongoMatch.Multimedia.Utils;
using Misc = LongoMatch.Gui.Helpers.Misc;
using Mono.Unix;
using LongoMatch.Gui.Popup;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Helpers;
using LongoMatch.Video.Utils;
using LongoMatch.Utils;
using LongoMatch.Interfaces.GUI;

using Device = LongoMatch.Common.Device;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NewProjectPanel : Gtk.Bin
	{
		public event BackEventHandle CancelEvent;
		public event OpenNewProjectHandler OpenNewProjectEvent;
		
		const int PROJECT_TYPE = 0;
		const int PROJECT_DETAILS = 1;
		
		Project project;
		ProjectType projectType;
		ITemplatesService tps;
		List<Device> videoDevices;
		ListStore videoStandardList, encProfileList, qualList;
		CalendarPopup cp;
		Win32CalendarDialog win32CP;
		MediaFile mediaFile;
		IMultimediaToolkit mtoolkit;
		IGUIToolkit gtoolkit;
		Color red;
		
		public NewProjectPanel (ITemplatesService tps, IGUIToolkit gtoolkit,
		                        IMultimediaToolkit mtoolkit, Project project)
		{
			this.Build ();
			this.tps = tps;
			this.mtoolkit = mtoolkit;
			this.gtoolkit = gtoolkit;
			notebook1.ShowTabs = false;
			notebook1.ShowBorder = false;
			backgroundwidget.Background = Gdk.Pixbuf.LoadFromResource (Constants.BACKGROUND).RotateSimple (Gdk.PixbufRotation.Counterclockwise);
			backgroundwidget.WidthRequest = 200;
			nextbutton.Clicked += HandleNextClicked;
			backbutton.Clicked += HandleBackClicked;
			if (project == null) {
				notebook1.Page = 0;
				this.project = new Project {Description = new ProjectDescription ()};
				this.project.Description.MatchDate = DateTime.Now;
			} else {
				notebook1.Page = 1;
				this.project = project;
			}
			localteamplayersselection.TemplatesProvider = tps.TeamTemplateProvider;
			awayteamplayersselection.TemplatesProvider = tps.TeamTemplateProvider;
			ConnectSignals ();
			FillProjectDetails ();
			FillCategories ();
			FillFormats ();
			FillDevices (mtoolkit.VideoDevices);
			Color.Parse ("red", ref red);
			outputfilelabel.ModifyFg (StateType.Normal, red);
			Color.Parse ("red", ref red);
			urilabel.ModifyFg (StateType.Normal, red);
			Color.Parse ("red", ref red);
			filelabel.ModifyFg (StateType.Normal, red);
		}
		
		void ConnectSignals () {
			if(Environment.OSVersion.Platform != PlatformID.Win32NT) {
				cp = new CalendarPopup();
				cp.Hide();
				cp.DateSelectedEvent += (selectedDate) => {
					dateEntry.Text = selectedDate.ToShortDateString();};
			}
			
			calendarbutton.Clicked += HandleCalendarbuttonClicked; 
			openbutton.Clicked += HandleOpenbuttonClicked;
			savebutton.Clicked += HandleSavebuttonClicked;
			urientry.Changed += HandleEntryChanged;
			fileEntry.Changed += HandleEntryChanged;
			outfileEntry.Changed += HandleEntryChanged;
			createbutton.Clicked += HandleCreateProject;
		}

		void FillProjectDetails () {
			seasonentry.Text = project.Description.Season;
			competitionentry.Text = project.Description.Competition;
			dateEntry.Text = project.Description.MatchDate.ToShortDateString();
			localSpinButton.Value = project.Description.LocalGoals;
			visitorSpinButton.Value = project.Description.VisitorGoals;
		}
		
		void FillCategories() {
			int i=0;
			int index = 0;

			foreach(string template in tps.CategoriesTemplateProvider.TemplatesNames) {
				tagscombobox.AppendText(template);
				if(template == "default")
					index = i;
				i++;
			}
			tagscombobox.Active = index;
		}
		
		void SetProjectType ()
		{
			bool filemode = false, urimode = false, capturemode = false;
			
			if (fromfileradiobutton.Active) {
				projectType = ProjectType.FileProject;
				filemode = true;
			} else if (liveradiobutton.Active) {
				projectType = ProjectType.CaptureProject;
				capturemode = true;
			} else if (fakeliveradiobutton.Active) {
				projectType = ProjectType.FakeCaptureProject;
			} else if (uriliveradiobutton.Active) {
				projectType = ProjectType.URICaptureProject;
				urimode = true;
			}
			filetable.Visible = filemode;
			outputfiletable.Visible = capturemode || urimode;
			capturetable.Visible = capturemode || urimode;
			urientry.Visible = urimode;
			urilabel.Visible = urimode;
			device.Visible = capturemode;
			devicecombobox.Visible = capturemode;
		}
		
		void FillFormats() {
			videoStandardList = Misc.FillImageFormat (imagecombobox, Config.CaptureVideoStandard);
			encProfileList = Misc.FillEncodingFormat (encodingcombobox, Config.CaptureEncodingProfile);
			qualList = Misc.FillQuality (qualitycombobox, Config.CaptureEncodingQuality);
		}
		
		public void FillDevices(List<Device> devices) {
			videoDevices = devices;

			foreach(Device device in devices) {
				string deviceElement, deviceName;

				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					deviceElement = Catalog.GetString("DirectShow source");
				} else {
					if(device.DeviceType == CaptureSourceType.DV)
						deviceElement = Catalog.GetString(Catalog.GetString("DV source"));
					else if (device.DeviceType == CaptureSourceType.System) {
						deviceElement = Catalog.GetString(Catalog.GetString("System source"));
					} else {
						deviceElement = Catalog.GetString(Catalog.GetString("GConf source"));
					}
				}
				deviceName = (device.ID == "") ? Catalog.GetString("Unknown"): device.ID;
				devicecombobox.AppendText(deviceName + " ("+deviceElement+")");
				devicecombobox.Active = 0;
			}
		}

		void HandleCalendarbuttonClicked(object sender, System.EventArgs e)
		{
			if(Environment.OSVersion.Platform == PlatformID.Win32NT) {
				win32CP = new Win32CalendarDialog();
				win32CP.TransientFor = (Gtk.Window)this.Toplevel;
				win32CP.Run();
				dateEntry.Text = win32CP.getSelectedDate().ToShortDateString();
				win32CP.Destroy();
			}
			else {
				cp.TransientFor=(Gtk.Window)this.Toplevel;
				cp.Show();
			}
		}

		void HandleSavebuttonClicked(object sender, System.EventArgs e)
		{
			string filename;
				
			filename = FileChooserHelper.SaveFile (this, Catalog.GetString("Output file"),
			                                       "Capture.mp4", Config.VideosDir, "MP4",
			                                       new string[] {"*.mp4"});
			if (filename != null) {
				outfileEntry.Text = System.IO.Path.ChangeExtension(filename, "mp4");
			}
		}

		void HandleOpenbuttonClicked(object sender, System.EventArgs e)
		{
			mediaFile = Open.OpenFile (gtoolkit, mtoolkit, this);
			if (mediaFile != null) {
				fileEntry.Text = mediaFile.FilePath;
			}
		}
		
		void HandleEntryChanged (object sender, EventArgs e)
		{
			if (fileEntry.Text != "") {
				filelabel.ModifyFg (StateType.Normal);
			} else {
				filelabel.ModifyFg (StateType.Normal, red);
			}
			if (urientry.Text != "") {
				urilabel.ModifyFg (StateType.Normal);
			} else {
				urilabel.ModifyFg (StateType.Normal, red);
			}
			if (outfileEntry.Text != "") {
				outputfilelabel.ModifyFg (StateType.Normal);
			} else {
				outputfilelabel.ModifyFg (StateType.Normal, red);
			}
			QueueDraw ();
		}

		void HandleCreateProject (object sender, EventArgs e)
		{
			CaptureSettings captureSettings;
			EncodingSettings encSettings;
			TreeIter iter;
			Project p;
			
			if (projectType == ProjectType.FileProject) {
				if (fileEntry.Text == "") {
					gtoolkit.WarningMessage (Catalog.GetString ("No input video file"));
					return;
				}
			}
			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
				if (outfileEntry.Text == "") {
					gtoolkit.WarningMessage (Catalog.GetString ("No output video file"));
					return;
				}
			}
			if (projectType == ProjectType.URICaptureProject) {
				if (urientry.Text == "") {
					gtoolkit.WarningMessage (Catalog.GetString ("No input URI"));
					return;
				}
			}
			p = new Project ();
			p.Categories = tps.CategoriesTemplateProvider.Load(tagscombobox.ActiveText);
			p.LocalTeamTemplate = localteamplayersselection.Template;
			p.VisitorTeamTemplate = awayteamplayersselection.Template;
			p.Description = new ProjectDescription ();
			p.Description.Competition = competitionentry.Text;
			p.Description.File = mediaFile;
			p.Description.LocalGoals = (int) localSpinButton.Value;
			p.Description.VisitorGoals = (int) visitorSpinButton.Value;
			p.Description.MatchDate = DateTime.Parse (dateEntry.Text);
			p.Description.Season = seasonentry.Text;
			p.Description.LocalName = p.LocalTeamTemplate.TeamName;
			p.Description.VisitorName = p.VisitorTeamTemplate.TeamName;
			
			encSettings = new EncodingSettings();
			captureSettings = new CaptureSettings();
				
			encSettings.OutputFile = fileEntry.Text;
			if (projectType == ProjectType.CaptureProject) {
				captureSettings.CaptureSourceType = videoDevices[devicecombobox.Active].DeviceType;
				captureSettings.DeviceID = videoDevices[devicecombobox.Active].ID;
			} else if (projectType == ProjectType.URICaptureProject) {
				captureSettings.CaptureSourceType = CaptureSourceType.URI;
				captureSettings.DeviceID = urientry.Text;
			}
				
			/* Get quality info */
			qualitycombobox.GetActiveIter(out iter);
			encSettings.EncodingQuality = (EncodingQuality) qualList.GetValue(iter, 1);
			
			/* Get size info */
			imagecombobox.GetActiveIter(out iter);
			encSettings.VideoStandard = (VideoStandard) videoStandardList.GetValue(iter, 1);
			
			/* Get encoding profile info */
			encodingcombobox.GetActiveIter(out iter);
			encSettings.EncodingProfile = (EncodingProfile) encProfileList.GetValue(iter, 1);
			
			encSettings.Framerate_n = Config.FPS_N;
			encSettings.Framerate_d = Config.FPS_D;
			
			captureSettings.EncodingSettings = encSettings;
				
			if (OpenNewProjectEvent != null) {
				OpenNewProjectEvent (p, projectType, captureSettings);
			}
		}

		void HandleBackClicked (object sender, EventArgs e)
		{
			if (notebook1.Page == PROJECT_TYPE) {
				if (CancelEvent != null) {
					CancelEvent ();
				}
			} else {
				notebook1.Page --;
				nextbutton.Visible = true;
				createbutton.Visible = false;
			}
		}

		void HandleNextClicked (object sender, EventArgs e)
		{
			if (notebook1.Page == PROJECT_TYPE) {
				SetProjectType ();	
			}
			notebook1.Page ++;
			if (notebook1.Page == PROJECT_DETAILS) {
				nextbutton.Visible = false;
				createbutton.Visible = true;
			}
		}
	}
}

