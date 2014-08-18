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
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Store;
using LongoMatch.Common;
using LongoMatch.Store.Templates;
using Misc = LongoMatch.Gui.Helpers.Misc;
using Mono.Unix;
using LongoMatch.Gui.Helpers;
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
		
		const int PROJECT_TYPE = 0;
		const int PROJECT_DETAILS = 1;
		const int PROJECT_PERIODS = 2;
		
		Project project;
		ProjectType projectType;
		CaptureSettings captureSettings;
		EncodingSettings encSettings;
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
			logoimage.Pixbuf = IconTheme.Default.LoadIcon ("longomatch", StyleConf.NewHeaderHeight - 10,
			                                               IconLookupFlags.ForceSvg);
			notebook1.ShowTabs = false;
			notebook1.ShowBorder = false;
			nextroundedbutton.Clicked += HandleNextClicked;
			backrectbutton.Clicked += HandleBackClicked;
			ConnectSignals ();
			FillCategories ();
			FillFormats ();
			FillDevices (mtoolkit.VideoDevices);
			LoadTeams ();
			titlelabel.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundLight));
			if (project == null) {
				notebook1.Page = 0;
				datepicker1.Date = DateTime.Now;
			} else {
				notebook1.Page = 1;
				this.project = project;
				projectType = ProjectType.EditProject;
				FillProjectDetails ();
			}
			UpdateTitle();
			Color.Parse ("red", ref red);
			outputfilelabel.ModifyFg (StateType.Normal, red);
			Color.Parse ("red", ref red);
			urilabel.ModifyFg (StateType.Normal, red);
			Color.Parse ("red", ref red);
			ApplyStyle ();
		}
		
		protected override void OnDestroyed ()
		{
			projectperiods1.Destroy ();
			base.OnDestroyed ();
		}
		
		void ApplyStyle () {
			centerbox.WidthRequest = StyleConf.NewTeamsComboWidth * 2 + StyleConf.NewTeamsSpacing;
			newheaderbox.HeightRequest = StyleConf.NewHeaderHeight;
			notebook1.BorderWidth = StyleConf.NewHeaderSpacing;
			lefttable.RowSpacing = filetable.RowSpacing =
				outputfiletable.RowSpacing = righttable.RowSpacing = StyleConf.NewTableHSpacing;
			lefttable.ColumnSpacing = righttable.ColumnSpacing = StyleConf.NewTableHSpacing;
			filetable.ColumnSpacing = outputfiletable.ColumnSpacing = StyleConf.NewTeamsSpacing; 
			vsimage.WidthRequest = StyleConf.NewTeamsSpacing;
			hometeamscombobox.WidthRequest = awayteamscombobox.WidthRequest = StyleConf.NewTeamsComboWidth;
			hometeamscombobox.HeightRequest = awayteamscombobox.HeightRequest = StyleConf.NewTeamsComboHeight;
			titlelabel.ModifyBg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteBackgroundLight));
		}
		
		void LoadTeams () {
			List<TeamTemplate> teams;
			
			drawingarea.HeightRequest = 200;
			teamtagger = new TeamTagger (new WidgetWrapper (drawingarea));
			teams = Config.TeamTemplatesProvider.Templates;
			hometeamscombobox.Load (teams, false);
			hometeamscombobox.Changed += (sender, e) => {
				LoadTemplate (hometeamscombobox.ActiveTeam, Team.LOCAL);};
			awayteamscombobox.Load (teams, true);
			awayteamscombobox.Changed += (sender, e) => {
				LoadTemplate (awayteamscombobox.ActiveTeam, Team.VISITOR);};
			hometeamscombobox.Active = 0;
			awayteamscombobox.Active = 0;
		}
		
		void ConnectSignals () {
			savebutton.Clicked += HandleSavebuttonClicked;
			urientry.Changed += HandleEntryChanged;
			outfileEntry.Changed += HandleEntryChanged;
			createbutton.Clicked += HandleCreateProject;
			tagscombobox.Changed += HandleSportsTemplateChanged;
		}

		void FillProjectDetails () {
			seasonentry.Text = project.Description.Season;
			competitionentry.Text = project.Description.Competition;
			datelabel2.Text = project.Description.MatchDate.ToShortDateString();
			datepicker1.Date = project.Description.MatchDate;
			hometeamscombobox.Sensitive = false;
			awayteamscombobox.Sensitive = false;
			tagscombobox.Visible = false;
			analysislabel.Visible = false;
			filetable.Visible = true;
			analysisTemplate = project.Categories;
			LoadTemplate (project.LocalTeamTemplate, Team.LOCAL);
			LoadTemplate (project.VisitorTeamTemplate, Team.VISITOR);
		}
		
		void FillCategories() {
			int i=0;
			int index = 0;

			foreach (string template in Config.CategoriesTemplatesProvider.TemplatesNames) {
				tagscombobox.AppendText(template);
				if (template == Config.DefaultTemplate)
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
		
		
		void LoadTemplate (string name, Team team) {
			TeamTemplate template;
			if (name != null) {
				template = Config.TeamTemplatesProvider.Load (name);
				LoadTemplate (template, team);
			}
		}
		
		void LoadTemplate (TeamTemplate template, Team team) {
			if (team == Team.LOCAL) {
				hometemplate = template;
			} else {
				awaytemplate = template;
			}
			teamtagger.LoadTeams (hometemplate, awaytemplate,
			                      analysisTemplate.FieldBackground);
		}
		
		void UpdateTitle ()
		{
			if (notebook1.Page == 0) {
				titlelabel.Markup = Catalog.GetString ("<b>PROJECT TYPE</b>");
			} else if (notebook1.Page == 1) {
				titlelabel.Markup = Catalog.GetString ("<b>PROJECT PROPERTIES</b>");
			} else if (notebook1.Page == 2) {
				titlelabel.Markup = Catalog.GetString ("<b>PERIODS SYNCHRONIZATION</b>");
			}
		}
		
		bool CreateProject ()
		{
			TreeIter iter;
			
			if (projectType == ProjectType.FileProject ||
				projectType == ProjectType.EditProject) {
				if (mediafilechooser1.File == null) {
					gtoolkit.WarningMessage (Catalog.GetString ("No input video file"));
					return false;
				}
			}

			if (project != null) {
				project.Description.File = mediaFile;
				return true;
			}
			
			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
				if (outfileEntry.Text == "") {
					gtoolkit.WarningMessage (Catalog.GetString ("No output video file"));
					return false;
				}
			}
			if (projectType == ProjectType.URICaptureProject) {
				if (urientry.Text == "") {
					gtoolkit.WarningMessage (Catalog.GetString ("No input URI"));
					return false;
				}
			}
			project = new Project ();
			project.Categories = analysisTemplate;
			project.LocalTeamTemplate = hometemplate;
			project.VisitorTeamTemplate = awaytemplate;
			project.Description = new ProjectDescription ();
			project.Description.Competition = competitionentry.Text;
			project.Description.File = mediaFile;
			project.Description.MatchDate = datepicker1.Date;
			project.Description.Season = seasonentry.Text;
			project.Description.LocalName = project.LocalTeamTemplate.TeamName;
			project.Description.VisitorName = project.VisitorTeamTemplate.TeamName;
			
			encSettings = new EncodingSettings();
			captureSettings = new CaptureSettings();
				
			encSettings.OutputFile = outfileEntry.Text;
			
			if (project.Description.File == null) {
				project.Description.File = new MediaFile ();
				project.Description.File.Fps = (ushort) (Config.FPS_N / Config.FPS_D);
				project.Description.File.FilePath = outfileEntry.Text;
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
				project.Description.File.FilePath = Constants.FAKE_PROJECT;
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
			return true;
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

		void HandleEntryChanged (object sender, EventArgs e)
		{
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
			if (teamtagger != null) {
				teamtagger.LoadTeams (hometemplate, awaytemplate, analysisTemplate.FieldBackground);
			}
		}

		void HandleCreateProject (object sender, EventArgs e)
		{
			if (CreateProject ()) {
				if (projectType == ProjectType.EditProject) {
					projectType = ProjectType.FileProject;
					Config.EventsBroker.EmitCreateThumbnails (project);
				}
				Config.EventsBroker.EmitOpenNewProject (project, projectType, captureSettings);
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
				nextroundedbutton.Visible = true;
				createbutton.Visible = false;
			}
			UpdateTitle ();
		}

		void HandleNextClicked (object sender, EventArgs e)
		{
			if (notebook1.Page == PROJECT_TYPE) {
				SetProjectType ();	
			}
			if (notebook1.Page == PROJECT_DETAILS) {
				if (!CreateProject ()) {
					return;
				}
			}

			notebook1.Page ++;

			if (notebook1.Page == PROJECT_DETAILS) {
				switch (projectType) {
				case ProjectType.CaptureProject:
				case ProjectType.FakeCaptureProject:
				case ProjectType.URICaptureProject:
					nextroundedbutton.Visible = false;
					createbutton.Visible = true;
					break;
				}
			} else if (notebook1.Page == PROJECT_PERIODS) {
				projectperiods1.Project = project;
				nextroundedbutton.Visible = false;
				createbutton.Visible = true;
			}
			UpdateTitle ();
		}
	}
}

