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
using Color = Gdk.Color;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NewProjectPanel : Gtk.Bin, IPanel
	{
		public event BackEventHandle BackEvent;
		public event OpenNewProjectHandler OpenNewProjectEvent;
		
		const int PROJECT_TYPE = 0;
		const int PROJECT_DETAILS = 1;
		
		Project project;
		ProjectType projectType;
		List<Device> videoDevices;
		ListStore teams, videoStandardList, encProfileList, qualList;
		MediaFile mediaFile;
		IMultimediaToolkit mtoolkit;
		IGUIToolkit gtoolkit;
		Color red;
		TeamTemplate hometemplate, awaytemplate;
		Categories analysisTemplate;
		TeamTagger teamtagger;
		
		public NewProjectPanel (Project project)
		{
			this.Build ();
			this.mtoolkit = Config.MultimediaToolkit;
			this.gtoolkit = Config.GUIToolkit;
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
			
			ConnectSignals ();
			FillProjectDetails ();
			FillCategories ();
			FillFormats ();
			FillDevices (mtoolkit.VideoDevices);
			LoadTeams ();
			Color.Parse ("red", ref red);
			outputfilelabel.ModifyFg (StateType.Normal, red);
			Color.Parse ("red", ref red);
			urilabel.ModifyFg (StateType.Normal, red);
			Color.Parse ("red", ref red);
			filelabel.ModifyFg (StateType.Normal, red);
		}
		
		void LoadTeams () {
			drawingarea1.HeightRequest = 200;
			teamtagger = new TeamTagger (new WidgetWrapper (drawingarea1));
			teams = new ListStore (typeof(string));

			teamtagger.HomeColor = Constants.HOME_COLOR;
			teamtagger.AwayColor = Constants.AWAY_COLOR;

			foreach (string name in Config.TeamTemplatesProvider.TemplatesNames) {
				teams.AppendValues (name);
			}
			hometeamscombobox.Model = teams;
			hometeamscombobox.Changed += (sender, e) => {
				LoadTemplate (hometeamscombobox.ActiveText, Team.LOCAL);};
			awayteamscombobox.Model = teams;
			awayteamscombobox.Changed += (sender, e) => {
				LoadTemplate (awayteamscombobox.ActiveText, Team.VISITOR);};
			hometeamscombobox.Active = 0;
			awayteamscombobox.Active = 0;
			teamtagger.SubstitutionsMode = true;
		}
		
		void ConnectSignals () {
			calendarbutton.Clicked += HandleCalendarbuttonClicked; 
			openbutton.Clicked += HandleOpenbuttonClicked;
			savebutton.Clicked += HandleSavebuttonClicked;
			urientry.Changed += HandleEntryChanged;
			fileEntry.Changed += HandleEntryChanged;
			outfileEntry.Changed += HandleEntryChanged;
			createbutton.Clicked += HandleCreateProject;
			tagscombobox.Changed += HandleSportsTemplateChanged;
		}

		void FillProjectDetails () {
			seasonentry.Text = project.Description.Season;
			competitionentry.Text = project.Description.Competition;
			datelabel.Text = project.Description.MatchDate.ToShortDateString();
			localSpinButton.Value = project.Description.LocalGoals;
			visitorSpinButton.Value = project.Description.VisitorGoals;
		}
		
		void FillCategories() {
			int i=0;
			int index = 0;

			foreach (string template in Config.CategoriesTemplatesProvider.TemplatesNames) {
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
		
		
		public void LoadTemplate (string name, Team team) {
			TeamTemplate template;
			if (name != null) {
				template = Config.TeamTemplatesProvider.Load (name);
				if (team == Team.LOCAL) {
					if (template.Shield != null) {
						homeshieldimage.Pixbuf = template.Shield.Value;
					} else {
						homeshieldimage.Pixbuf = Gdk.Pixbuf.LoadFromResource ("logo.svg");						
					}
					homelabel.Text = template.TeamName;
					hometemplate = template;
				} else {
					if (template.Shield != null) {
						awayshieldimage.Pixbuf = template.Shield.Value;
					} else {
						awayshieldimage.Pixbuf = Gdk.Pixbuf.LoadFromResource ("logo.svg");						
					}
					awaylabel.Text = template.TeamName;
					awaytemplate = template;
				}
				teamtagger.LoadTeams (hometemplate, awaytemplate,
				                      analysisTemplate.FieldBackground);
			}
		}
		
		void HandleCalendarbuttonClicked(object sender, System.EventArgs e)
		{
			datelabel.Text = Config.GUIToolkit.SelectDate (project.Description.MatchDate, this).ToShortDateString ();
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
			mediaFile = Open.OpenFile (this);
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

		void HandleSportsTemplateChanged (object sender, EventArgs e)
		{
			analysisTemplate = Config.CategoriesTemplatesProvider.Load(tagscombobox.ActiveText);
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
			p.Categories = analysisTemplate;
			p.LocalTeamTemplate = hometemplate;
			p.VisitorTeamTemplate = awaytemplate;
			p.Description = new ProjectDescription ();
			p.Description.Competition = competitionentry.Text;
			p.Description.File = mediaFile;
			p.Description.LocalGoals = (int) localSpinButton.Value;
			p.Description.VisitorGoals = (int) visitorSpinButton.Value;
			p.Description.MatchDate = DateTime.Parse (datelabel.Text);
			p.Description.Season = seasonentry.Text;
			p.Description.LocalName = p.LocalTeamTemplate.TeamName;
			p.Description.VisitorName = p.VisitorTeamTemplate.TeamName;
			
			encSettings = new EncodingSettings();
			captureSettings = new CaptureSettings();
				
			encSettings.OutputFile = outfileEntry.Text;
			
			if (p.Description.File == null) {
				p.Description.File = new MediaFile ();
				p.Description.File.Fps = (ushort) (Config.FPS_N / Config.FPS_D);
				p.Description.File.FilePath = outfileEntry.Text;
			}
			if (projectType == ProjectType.CaptureProject) {
				Device device = videoDevices[devicecombobox.Active];
				captureSettings.CaptureSourceType = device.DeviceType;
				captureSettings.DeviceID = device.ID;
				captureSettings.SourceElement = device.SourceElement;
			} else if (projectType == ProjectType.URICaptureProject) {
				captureSettings.CaptureSourceType = CaptureSourceType.URI;
				captureSettings.DeviceID = urientry.Text;
			}else if (projectType == ProjectType.FakeCaptureProject) {
				captureSettings.CaptureSourceType = CaptureSourceType.None;
				p.Description.File.FilePath = Constants.FAKE_PROJECT;
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
				if (BackEvent != null) {
					BackEvent ();
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

