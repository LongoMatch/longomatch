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
using System.Linq;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Drawing.Cairo;
using VAS.Services.State;
using Color = VAS.Core.Common.Color;
using Constants = LongoMatch.Core.Common.Constants;
using Device = VAS.Core.Common.Device;
using Helpers = VAS.UI.Helpers;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (NewProjectState.NAME)]
	public partial class NewProjectPanel : Gtk.Bin, IPanel<NewProjectPanelVM>
	{
		const int PROJECT_TYPE = 0;
		const int PROJECT_DETAILS = 1;
		int firstPage;
		LMProject project;
		ProjectType projectType;
		CaptureSettings captureSettings;
		EncodingSettings encSettings;
		List<Device> videoDevices;
		ListStore videoStandardList, encProfileList, qualList, dashboardsList;
		IMultimediaToolkit mtoolkit;
		IDialogs dialogs;
		Gdk.Color red;
		LMTeam hometemplate, awaytemplate;
		LMDashboard analysisTemplate;
		LMTeamTaggerView teamtagger;
		SizeGroup sg;
		CameraSynchronizationEditorState cameraSynchronizationState;
		NewProjectPanelVM viewModel;
		bool resyncEvents;

		public NewProjectPanel ()
		{
			this.Build ();
			this.mtoolkit = App.Current.MultimediaToolkit;
			dialogs = App.Current.Dialogs;
			capturemediafilechooser.FileChooserMode = FileChooserMode.File;
			capturemediafilechooser.ProposedFileName = String.Format ("Live-LongoMatch-{0}.mp4",
				DateTime.Now.ToShortDateString ());
			notebook1.ShowTabs = false;
			notebook1.ShowBorder = false;

			LoadIcons ();
			GroupLabels ();
			ConnectSignals ();
			FillDahsboards ();
			FillFormats ();
			Gdk.Color.Parse ("red", ref red);
			outputfilelabel.ModifyFg (StateType.Normal, red);
			urilabel.ModifyFg (StateType.Normal, red);

			ApplyStyle ();
		}

		protected override void OnDestroyed ()
		{
			OnUnload ();
			teamtagger?.Dispose ();
			base.OnDestroyed ();
		}

		public override void Dispose ()
		{
			Destroy ();
			base.Dispose ();
		}

		public string Title {
			get {
				return Catalog.GetString ("New project");
			}
		}

		public NewProjectPanelVM ViewModel {
			set {
				viewModel = value;
				project = viewModel.Model;
				LoadTeams (project);
				if (project == null) {
					notebook1.Page = firstPage = 0;
					datepicker1.Date = DateTime.Now;
					mediafilesetselection1.FileSet = new MediaFileSet ();
				} else {
					notebook1.Page = firstPage = 1;
					projectType = ProjectType.EditProject;
					resyncEvents = true;
					SetProjectType ();
					FillProjectDetails ();
				}
				UpdateTitle ();
				viewModel.TeamTagger.Background = analysisTemplate.FieldBackground;
				teamtagger.ViewModel = viewModel.TeamTagger;
			}
			get {
				return viewModel;
			}
		}

		public void OnLoad ()
		{

		}

		public void OnUnload ()
		{

		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (NewProjectPanelVM)viewModel;
		}

		public void FillDevices (List<Device> devices)
		{
			videoDevices = devices;
			bool includeSourceName;

			includeSourceName = devices.GroupBy (d => d.SourceElement).Count () > 1;

			foreach (Device device in devices) {
				string deviceName;

				if (device.Formats.Count == 0)
					continue;

				deviceName = (device.ID == "") ? Catalog.GetString ("Unknown") : device.ID;
				if (includeSourceName) {
					deviceName += String.Format (" ({0})", device.SourceElement);
				}
				devicecombobox.AppendText (deviceName);
				devicecombobox.Active = 0;
			}
		}

		void ApplyStyle ()
		{
			/* Keep the central box aligned in the center of the widget */
			SizeGroup grp = new SizeGroup (SizeGroupMode.Horizontal);
			grp.AddWidget (lefttable);
			grp.AddWidget (righttable);

			centerbox.WidthRequest = StyleConf.NewTeamsComboWidth * 2 + StyleConf.NewTeamsSpacing;
			lefttable.RowSpacing = outputfiletable.RowSpacing =
				righttable.RowSpacing = StyleConf.NewTableHSpacing;
			lefttable.ColumnSpacing = righttable.ColumnSpacing = StyleConf.NewTableHSpacing;
			vsimage.WidthRequest = StyleConf.NewTeamsSpacing;
			hometeamscombobox.WidthRequest = awayteamscombobox.WidthRequest = StyleConf.NewTeamsComboWidth;
			hometeamscombobox.HeightRequest = awayteamscombobox.HeightRequest = StyleConf.NewTeamsComboHeight;
			hometeamscombobox.WrapWidth = awayteamscombobox.WrapWidth = 1;
			homealignment.Xscale = awayalignment.Xscale = 0;
			homealignment.Xalign = 0.8f;
			awayalignment.Xalign = 0.2f;
		}

		void LoadIcons ()
		{
			int s = StyleConf.ProjectTypeIconSize;
			IconLookupFlags f = IconLookupFlags.ForceSvg;

			fileimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-video-file", s, f);
			captureimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-video-device", s, f);
			fakeimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-video-device-fake", s, f);
			ipimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-video-device-ip", s, f);
			vsimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-vs", 50, f);

			filebutton.Clicked += HandleProjectTypeSet;
			capturebutton.Clicked += HandleProjectTypeSet;
			fakebutton.Clicked += HandleProjectTypeSet;
			ipbutton.Clicked += HandleProjectTypeSet;
		}

		void GroupLabels ()
		{
			sg = new SizeGroup (SizeGroupMode.Horizontal);
			sg.AddWidget (urilabel);
			sg.AddWidget (outputfilelabel);
			sg.AddWidget (device);
			sg.AddWidget (userlabel);
			sg.AddWidget (videoformatlabel);
			sg.AddWidget (deviceformatlabel);
			sg.AddWidget (outputsizelabel);
		}

		void LoadTeams (LMProject project)
		{
			List<Team> teams;
			bool hasLocalTeam = false;
			bool hasAwayTeam = false;

			drawingarea.HeightRequest = 200;
			teamtagger = new LMTeamTaggerView (new WidgetWrapper (drawingarea));
			teamtagger.ShowMenuEvent += HandleShowMenuEvent;
			teams = App.Current.TeamTemplatesProvider.Templates;

			// Fill the combobox with project values or the templates ones
			if (project != null) {
				if (project.LocalTeamTemplate != null)
					hasLocalTeam = true;
				if (project.VisitorTeamTemplate != null)
					hasAwayTeam = true;
			}

			// Update the combobox
			if (hasAwayTeam) {
				awayteamscombobox.Load (new List<LMTeam> { project.VisitorTeamTemplate });
			} else {
				awayteamscombobox.Load (teams.OfType<LMTeam> ().ToList ());
				awayteamscombobox.Changed += (sender, e) => {
					LoadTemplate (awayteamscombobox.ActiveTeam.Clone (), TeamType.VISITOR, false);
				};
			}

			if (hasLocalTeam) {
				hometeamscombobox.Load (new List<LMTeam> { project.LocalTeamTemplate });
			} else {
				hometeamscombobox.Load (teams.OfType<LMTeam> ().ToList ());
				hometeamscombobox.Changed += (sender, e) => {
					LoadTemplate (hometeamscombobox.ActiveTeam.Clone (), TeamType.LOCAL, false);
				};

			}

			hometeamscombobox.Active = 0;
			awayteamscombobox.Active = teams.Count == 1 ? 0 : 1;
		}

		void ConnectSignals ()
		{
			homecolor1button.Clicked += HandleColorClicked;
			homecolor2button.Clicked += HandleColorClicked;
			awaycolor1button.Clicked += HandleColorClicked;
			awaycolor2button.Clicked += HandleColorClicked;
			hometacticsbutton.Clicked += HandleTacticsChanged;
			awaytacticsbutton.Clicked += HandleTacticsChanged;
			panelheader1.ApplyClicked += HandleNextClicked;
			panelheader1.BackClicked += HandleBackClicked;
			urientry.Changed += HandleEntryChanged;
			capturemediafilechooser.ChangedEvent += HandleEntryChanged;
			tagscombobox.Changed += HandleSportsTemplateChanged;
			devicecombobox.Changed += HandleDeviceChanged;
			notebook1.SwitchPage += HandleSwitchPage;
		}

		void FillProjectDetails ()
		{
			seasonentry.Text = project.Description.Season;
			competitionentry.Text = project.Description.Competition;
			datepicker1.Date = project.Description.MatchDate;
			desctextview.Buffer.Clear ();
			desctextview.Buffer.InsertAtCursor (project.Description.Description ?? "");

			// In case the project provides a dashboard, use it, otherwise, enable the combobox
			if (project.Dashboard != null) {
				tagscombobox.Visible = false;
				analysislabel.Visible = false;
				analysisTemplate = project.Dashboard as LMDashboard;
			} else {
				project.Dashboard = analysisTemplate;
			}

			ViewModel.TeamTagger.Background = analysisTemplate.FieldBackground;

			// In case the project does have a team, do not allow a modification
			// otherwise set the loaded template
			if (project.LocalTeamTemplate != null) {
				hometeamscombobox.Sensitive = false;
				hometeamscombobox.Load (new List<LMTeam> { project.LocalTeamTemplate });
				hometeamscombobox.Active = 0;
				LoadTemplate (project.LocalTeamTemplate, TeamType.LOCAL, true);
			} else {
				project.LocalTeamTemplate = hometemplate;
			}

			if (project.VisitorTeamTemplate != null) {
				awayteamscombobox.Sensitive = false;
				awayteamscombobox.Load (new List<LMTeam> { project.VisitorTeamTemplate });
				awayteamscombobox.Active = 0;
				LoadTemplate (project.VisitorTeamTemplate, TeamType.VISITOR, true);
			} else {
				project.VisitorTeamTemplate = awaytemplate;
			}

			mediafilesetselection1.Visible = true;
			mediafilesetselection1.FileSet = project.Description.FileSet;
		}

		void FillDahsboards ()
		{
			int i = 0;
			int index = 0;

			dashboardsList = new ListStore (typeof (string), typeof (Dashboard));
			foreach (var dashboard in App.Current.CategoriesTemplatesProvider.Templates) {
				dashboardsList.AppendValues (dashboard.Name, dashboard);
				if (dashboard.Name == App.Current.Config.DefaultTemplate)
					index = i;
				i++;
			}
			tagscombobox.Model = dashboardsList;
			tagscombobox.Clear ();
			var cell = new CellRendererText ();
			tagscombobox.PackStart (cell, true);
			tagscombobox.AddAttribute (cell, "text", 0);
			tagscombobox.Active = index;
		}

		void SetProjectType ()
		{
			bool filemode = false, urimode = false, capturemode = false;

			if (projectType == ProjectType.FileProject ||
				projectType == ProjectType.EditProject) {
				filemode = true;
			} else if (projectType == ProjectType.CaptureProject) {
				capturemode = true;
			} else if (projectType == ProjectType.URICaptureProject) {
				urimode = true;
			}
			mediafilesetselection1.Visible = filemode;
			capturebox.Visible = capturemode || urimode;
			urltable.Visible = urimode;
			credentialtable.Visible = urimode;
			devicetable.Visible = capturemode;
		}

		void FillFormats ()
		{
			videoStandardList = Misc.FillImageFormat (imagecombobox, VideoStandards.Capture,
				App.Current.Config.CaptureVideoStandard);
			encProfileList = Misc.FillEncodingFormat (encodingcombobox, App.Current.Config.CaptureEncodingProfile);
			qualList = Misc.FillQuality (qualitycombobox, App.Current.Config.CaptureEncodingQuality);
		}

		void SetButtonColor (DrawingArea area, Color color)
		{
			Gdk.Color gcolor = Misc.ToGdkColor (color);
			area.ModifyBg (StateType.Normal, gcolor);
			area.ModifyBg (StateType.Active, gcolor);
			area.ModifyBg (StateType.Insensitive, gcolor);
			area.ModifyBg (StateType.Prelight, gcolor);
			area.ModifyBg (StateType.Selected, gcolor);
		}

		void LoadTemplate (LMTeam template, TeamType team, bool forceColor)
		{
			if (team == TeamType.LOCAL) {
				hometemplate = template;
				hometacticsentry.Text = hometemplate.FormationStr;
				SetButtonColor (homecolor1, hometemplate.Colors [0]);
				SetButtonColor (homecolor2, hometemplate.Colors [1]);
				homecolor1button.Active = homecolor2button.Active = false;
				if ((forceColor && template.ActiveColor == 1) ||
					(awaytemplate != null && awaytemplate.Color.Equals (hometemplate.Color))) {
					homecolor2button.Click ();
				} else {
					homecolor1button.Click ();
				}
				ViewModel.TeamTagger.HomeTeam.Model = hometemplate;
			} else {
				awaytemplate = template;
				awaytacticsentry.Text = awaytemplate.FormationStr;
				SetButtonColor (awaycolor1, awaytemplate.Colors [0]);
				SetButtonColor (awaycolor2, awaytemplate.Colors [1]);
				awaycolor1button.Active = awaycolor2button.Active = false;
				if ((forceColor && template.ActiveColor == 1) ||
					(hometemplate != null && hometemplate.Color.Equals (awaytemplate.Color))) {
					awaycolor2button.Click ();
				} else {
					awaycolor1button.Click ();
				}
				ViewModel.TeamTagger.AwayTeam.Model = awaytemplate;
			}
		}

		void UpdateTitle ()
		{
			if (notebook1.Page == 0) {
				panelheader1.Title = Catalog.GetString ("PROJECT TYPE");
			} else if (notebook1.Page == 1) {
				panelheader1.Title = Catalog.GetString ("PROJECT PROPERTIES");
			}
		}

		void FillProject ()
		{
			project.Dashboard = analysisTemplate;
			project.LocalTeamTemplate = hometemplate;
			project.VisitorTeamTemplate = awaytemplate;
			project.Description.Competition = competitionentry.Text;
			project.Description.MatchDate = datepicker1.Date;
			project.Description.Description = desctextview.Buffer.GetText (desctextview.Buffer.StartIter,
				desctextview.Buffer.EndIter, true);
			project.Description.Season = seasonentry.Text;
			project.Description.LocalName = project.LocalTeamTemplate.TeamName;
			project.Description.VisitorName = project.VisitorTeamTemplate.TeamName;
			project.Description.FileSet = mediafilesetselection1.FileSet;
			project.UpdateEventTypesAndTimers ();
			project.ConsolidateDescription ();
			project.ProjectType = projectType;
		}

		bool CreateProject ()
		{
			TreeIter iter;
			MediaFile file;

			if (projectType == ProjectType.FileProject ||
				projectType == ProjectType.EditProject) {
				if (!mediafilesetselection1.FileSet.CheckFiles ()) {
					dialogs.WarningMessage (Catalog.GetString ("You need at least 1 video file for the main angle"));
					return false;
				}
			}

			if (project != null) {
				FillProject ();
				return true;
			}

			if (projectType == ProjectType.CaptureProject ||
				projectType == ProjectType.URICaptureProject) {
				if (String.IsNullOrEmpty (capturemediafilechooser.CurrentPath)) {
					dialogs.WarningMessage (Catalog.GetString ("No output video file"));
					return false;
				}
			}
			if (projectType == ProjectType.URICaptureProject) {
				if (urientry.Text == "") {
					dialogs.WarningMessage (Catalog.GetString ("No input URI"));
					return false;
				}
			}

			project = new LMProject ();
			project.Description = new ProjectDescription ();
			FillProject ();
			ViewModel.Model = project;


			encSettings = new EncodingSettings ();
			captureSettings = new CaptureSettings ();

			encSettings.OutputFile = capturemediafilechooser.CurrentPath;

			/* Get quality info */
			qualitycombobox.GetActiveIter (out iter);
			encSettings.EncodingQuality = (EncodingQuality)qualList.GetValue (iter, 1);

			/* Get size info */
			imagecombobox.GetActiveIter (out iter);
			encSettings.VideoStandard = (VideoStandard)videoStandardList.GetValue (iter, 1);

			/* Get encoding profile info */
			encodingcombobox.GetActiveIter (out iter);
			encSettings.EncodingProfile = (EncodingProfile)encProfileList.GetValue (iter, 1);

			encSettings.Framerate_n = App.Current.Config.FPS_N;
			encSettings.Framerate_d = App.Current.Config.FPS_D;

			captureSettings.EncodingSettings = encSettings;

			file = project.Description.FileSet.FirstOrDefault ();
			if (file == null) {
				file = new MediaFile () { Name = Catalog.GetString ("Main camera angle") };
				file.FilePath = capturemediafilechooser.CurrentPath;
				file.Fps = (ushort)(App.Current.Config.FPS_N / App.Current.Config.FPS_D);
				file.Par = 1;
				project.Description.FileSet.Add (file);
			}

			if (projectType == ProjectType.CaptureProject) {
				captureSettings.Device = videoDevices [devicecombobox.Active];
				captureSettings.Format = captureSettings.Device.Formats [deviceformatcombobox.Active];
				file.VideoHeight = encSettings.VideoStandard.Height;
				file.VideoWidth = encSettings.VideoStandard.Width;
			} else if (projectType == ProjectType.URICaptureProject) {
				string uri = urientry.Text;
				if (!String.IsNullOrEmpty (userentry.Text) || !String.IsNullOrEmpty (passwordentry.Text)) {
					int index = uri.IndexOf ("://", StringComparison.Ordinal);
					if (index != -1) {
						uri = uri.Insert (index + 3, string.Format ("{0}:{1}@", userentry.Text, passwordentry.Text));
					}
				}
				captureSettings.Device = new Device {
					DeviceType = CaptureSourceType.URI,
					ID = uri
				};
				file.VideoHeight = encSettings.VideoStandard.Height;
				file.VideoWidth = encSettings.VideoStandard.Width;
			} else if (projectType == ProjectType.FakeCaptureProject) {
				file.FilePath = Constants.FAKE_PROJECT;
			}
			return true;
		}

		void StartProject ()
		{
			if (projectType == ProjectType.EditProject) {
				projectType = ProjectType.FileProject;
				ViewModel.ProjectType = projectType;
			} else {
				project.CreateLineupEvent ();
			}
			LMStateHelper.OpenProject (ViewModel, captureSettings);
		}

		void HandleEntryChanged (object sender, EventArgs e)
		{
			if (urientry.Text != "") {
				urilabel.ModifyFg (StateType.Normal);
			} else {
				urilabel.ModifyFg (StateType.Normal, red);
			}
			if (!String.IsNullOrEmpty (capturemediafilechooser.CurrentPath)) {
				outputfilelabel.ModifyFg (StateType.Normal);
			} else {
				outputfilelabel.ModifyFg (StateType.Normal, red);
			}
			QueueDraw ();
		}

		void HandleSportsTemplateChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			tagscombobox.GetActiveIter (out iter);
			analysisTemplate = tagscombobox.Model.GetValue (iter, 1) as LMDashboard;
			if (ViewModel != null) {
				ViewModel.TeamTagger.Background = analysisTemplate.FieldBackground;
			}
		}

		void HandleProjectTypeSet (object sender, EventArgs e)
		{
			if (sender == filebutton) {
				projectType = ProjectType.FileProject;
			} else if (sender == capturebutton) {
				if (videoDevices == null || videoDevices.Count == 0) {
					FillDevices (mtoolkit.VideoDevices);
				}

				if (videoDevices == null || videoDevices.Count == 0) {
					App.Current.Dialogs.ErrorMessage (Catalog.GetString ("No capture devices found in the system"),
						this);
					return;
				}
				projectType = ProjectType.CaptureProject;
			} else if (sender == fakebutton) {
				projectType = ProjectType.FakeCaptureProject;
			} else if (sender == ipbutton) {
				projectType = ProjectType.URICaptureProject;
			}
			HandleNextClicked (this, e);
		}

		void HandleBackClicked (object sender, EventArgs e)
		{
			if (notebook1.Page == firstPage) {
				App.Current.StateController.MoveBack ();
			} else {
				notebook1.Page--;
			}
			if (notebook1.Page == PROJECT_TYPE) {
				project = null;
			}
			UpdateTitle ();
		}

		void HandleNextClicked (object sender, EventArgs e)
		{
			if (notebook1.Page == PROJECT_TYPE) {
				SetProjectType ();
				notebook1.Page++;
				UpdateTitle ();
			} else if (notebook1.Page == PROJECT_DETAILS) {
				if (!CreateProject ()) {
					return;
				}
				if (ViewModel.IsLive) {
					StartProject ();
					return;
				}
				App.Current.StateController.MoveTo (CameraSynchronizationState.NAME, viewModel);
			}
		}

		void HandleShowMenuEvent (List<LMPlayer> players)
		{
			Menu menu = new Menu ();
			MenuItem item;

			if (players.Count > 0) {
				item = new MenuItem ("Remove for this match");
				item.Activated += (sender, e) => {
					hometemplate.RemovePlayers (players, false);
					awaytemplate.RemovePlayers (players, false);
					App.Current.EventsBroker.Publish (new UpdateLineup ());
				};
			} else {
				item = new MenuItem ("Reset players");
				item.Activated += (sender, e) => {
					hometemplate.ResetPlayers ();
					awaytemplate.ResetPlayers ();
				};
			}
			menu.Add (item);
			menu.ShowAll ();
			menu.Popup ();
		}

		void HandleTacticsChanged (object sender, EventArgs e)
		{
			LMTeam team;
			Entry entry;

			if (sender == hometacticsbutton) {
				team = hometemplate;
				entry = hometacticsentry;
			} else {
				team = awaytemplate;
				entry = awaytacticsentry;
			}

			try {
				team.FormationStr = entry.Text;
			} catch {
				App.Current.Dialogs.ErrorMessage (
					Catalog.GetString ("Could not parse tactics string"));
			}
			entry.Text = team.FormationStr;
		}

		void HandleColorClicked (object sender, EventArgs e)
		{
			ToggleButton button = sender as ToggleButton;
			if (!button.Active) {
				return;
			}
			if (button == homecolor1button) {
				homecolor2button.Active = false;
				hometemplate.ActiveColor = 0;
				hometemplate.UpdateColors ();
			} else if (button == homecolor2button) {
				homecolor1button.Active = false;
				hometemplate.ActiveColor = 1;
				hometemplate.UpdateColors ();
			} else if (button == awaycolor1button) {
				awaycolor2button.Active = false;
				awaytemplate.ActiveColor = 0;
				awaytemplate.UpdateColors ();
			} else if (button == awaycolor2button) {
				awaycolor1button.Active = false;
				awaytemplate.ActiveColor = 1;
				awaytemplate.UpdateColors ();
			}
			drawingarea.QueueDraw ();
		}

		void HandleDeviceChanged (object sender, EventArgs e)
		{
			Device device = videoDevices [devicecombobox.Active];
			ListStore store = new ListStore (typeof (string));
			deviceformatcombobox.Model = store;
			foreach (DeviceVideoFormat format in device.Formats) {
				deviceformatcombobox.AppendText (format.ToString ());
			}
			deviceformatcombobox.Active = 0;
		}

		void HandleSwitchPage (object o, SwitchPageArgs args)
		{
			panelheader1.ApplyVisible = notebook1.Page != PROJECT_TYPE;
		}
	}
}

