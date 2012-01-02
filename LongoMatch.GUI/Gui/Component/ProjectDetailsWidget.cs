// FileDescriptionWidget.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using Gtk;
using LongoMatch.Common;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Popup;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Video.Utils;
using Mono.Unix;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectDetailsWidget : Gtk.Bin
	{
		public event EventHandler EditedEvent;
		Project project;
		MediaFile mFile;
		bool edited;
		DateTime date;
		CalendarPopup cp;
		Win32CalendarDialog win32CP;
		Categories actualCategory;
		TeamTemplate actualVisitorTeam;
		TeamTemplate actualLocalTeam;
		
		ICategoriesTemplatesProvider tpc;
		ITeamTemplatesProvider tpt;
		ITemplateWidget<Categories, Category> twc;
		ITemplateWidget<TeamTemplate, Player> twt;
		ProjectType useType;
		List<Device> videoDevices;
		ListStore videoStandardList;
		ListStore encProfileList;
		private const string DV_SOURCE = "DV Source";
		private const string GCONF_SOURCE = "GConf Source";


		public ProjectDetailsWidget()
		{
			this.Build();

			//HACK:The calendar dialog does not respond on win32
			if(Environment.OSVersion.Platform != PlatformID.Win32NT) {
				cp = new CalendarPopup();
				cp.Hide();
				cp.DateSelectedEvent += new DateSelectedHandler(OnDateSelected);
			}
			
			FillFormats();
			videoDevices = new List<Device>();
			Use=ProjectType.FileProject;
		}
		
		public ITemplatesService TemplatesService {
			set {
				tpc = value.CategoriesTemplateProvider;
				tpt = value.TeamTemplateProvider;
				twc = new CategoriesTemplateEditorWidget(value);
				twt = new TeamTemplateEditorWidget(tpt);
				FillCategories();
				FillTeamsTemplate();
			}
		}
		
		public ProjectType Use {
			set {
				bool visible1 = value == ProjectType.CaptureProject;
				bool visible2 = value != ProjectType.FakeCaptureProject;
				bool visible3 = value != ProjectType.EditProject;

				filelabel.Visible = visible2;
				filehbox.Visible = visible2;

				tagscombobox.Visible = visible3;
				localcombobox.Visible = visible3;
				visitorcombobox.Visible = visible3;
				localteamlabel.Visible = !visible3;
				visitorteamlabel.Visible = !visible3;

				expander1.Visible = visible1;
				device.Visible = visible1;
				devicecombobox.Visible = visible1;

				useType = value;
			}
			get {
				return useType;
			}
		}

		public bool Edited {
			set {
				edited=value;
			}
			get {
				return edited;
			}
		}

		public string Season {
			get {
				return seasonentry.Text;
			}
			set {
				seasonentry.Text = value;
			}
		}

		public string Competition {
			get {
				return competitionentry.Text;
			}
			set {
				competitionentry.Text = value;
			}
		}

		public int LocalGoals {
			get {
				return (int)localSpinButton.Value;
			}
			set {
				localSpinButton.Value = value;
			}
		}

		public int VisitorGoals {
			get {
				return (int)visitorSpinButton.Value;
			}
			set {
				visitorSpinButton.Value = value;
			}
		}

		private string Filename {
			get {
				return fileEntry.Text;
			}
			set {
				fileEntry.Text = value;
			}
		}

		public DateTime Date {
			get {
				return date;
			}
			set {
				date = value;
				dateEntry.Text = value.ToShortDateString();
			}
		}

		public Categories Categories {
			get {
				return actualCategory;
			}
			set {
				actualCategory = value;
			}
		}

		public TeamTemplate LocalTeamTemplate {
			get {
				return actualLocalTeam;
			}
			set {
				localteamlabel.Text = value.TeamName;
				actualLocalTeam = value;
			}
		}

		public TeamTemplate VisitorTeamTemplate {
			get {
				return actualVisitorTeam;
			}
			set {
				visitorteamlabel.Text = value.TeamName;
				actualVisitorTeam = value;
			}
		}

		private string SelectedCategory {
			get {
				return tagscombobox.ActiveText;
			}
		}

		private string LocalTeamTemplateFile {
			get {
				return localcombobox.ActiveText;
			}
		}

		private string VisitorTeamTemplateFile {
			get {
				return visitorcombobox.ActiveText;
			}
		}

		public CaptureSettings CaptureSettings {
			get {
				TreeIter iter;
				EncodingSettings encSettings = new EncodingSettings();
				CaptureSettings s = new CaptureSettings();
				
				encSettings.OutputFile = fileEntry.Text;
				encSettings.AudioBitrate = (uint)audiobitratespinbutton.Value;
				encSettings.VideoBitrate = (uint)videobitratespinbutton.Value;
				if(videoDevices[devicecombobox.Active].DeviceType == DeviceType.DV) {
					if(Environment.OSVersion.Platform == PlatformID.Win32NT)
						s.CaptureSourceType = CaptureSourceType.DShow;
					else
						s.CaptureSourceType = CaptureSourceType.DV;
				}
				else {
					s.CaptureSourceType = CaptureSourceType.Raw;
				}
				s.DeviceID = videoDevices[devicecombobox.Active].ID;
				
				/* Get size info */
				sizecombobox.GetActiveIter(out iter);
				encSettings.VideoStandard = (VideoStandard) videoStandardList.GetValue(iter, 1);
			
				/* Get encoding profile info */
				videoformatcombobox.GetActiveIter(out iter);
				encSettings.EncodingProfile = (EncodingProfile) encProfileList.GetValue(iter, 1);
				
				/* FIXME: Configure with the UI */
				encSettings.Framerate_n = 25;
				encSettings.Framerate_d = 1;
				
				s.EncodingSettings = encSettings;
				return s;
			}
		}

		public void SetProject(Project project) {
			this.project = project;
			if (project == null)
				return;
			var desc = project.Description;
			mFile = desc.File;
			Filename = mFile != null ? mFile.FilePath : "";
			LocalGoals = desc.LocalGoals;
			VisitorGoals = desc.VisitorGoals;
			Date = desc.MatchDate;
			Season = desc.Season;
			Competition = desc.Competition;
			Categories = project.Categories;
			LocalTeamTemplate = project.LocalTeamTemplate;
			VisitorTeamTemplate = project.VisitorTeamTemplate;
			Edited = false;
		}

		public void UpdateProject() {
			var desc = project.Description;
			desc.File= mFile;
			desc.LocalGoals = (int)localSpinButton.Value;
			desc.VisitorGoals = (int)visitorSpinButton.Value;
			desc.MatchDate = DateTime.Parse(dateEntry.Text);
			desc.Competition = competitionentry.Text;
			desc.Season = seasonentry.Text;
			project.Categories = Categories;
			project.LocalTeamTemplate = LocalTeamTemplate;
			project.VisitorTeamTemplate = VisitorTeamTemplate;
		}

		public Project GetProject() {
			if(useType != ProjectType.EditProject) {
				if(Filename == "" && useType != ProjectType.FakeCaptureProject)
					return null;
				else {
					if(useType == ProjectType.FakeCaptureProject) {
						mFile = new MediaFile();
						mFile.FilePath = Constants.FAKE_PROJECT;
						mFile.Fps = 25;
					} else if(useType == ProjectType.CaptureProject) {
						mFile = new MediaFile();
						mFile.FilePath = fileEntry.Text;
						mFile.Fps = 25;
					}
					var desc = new ProjectDescription {
						File = mFile,
						VisitorName = VisitorTeamTemplate.TeamName,
						LocalName = LocalTeamTemplate.TeamName,
						Season = Season,
						Competition = Competition,
						LocalGoals = LocalGoals,
						MatchDate = Date
					};

					return new Project {
						Description = desc,
						Categories = Categories,
						LocalTeamTemplate = LocalTeamTemplate,
						VisitorTeamTemplate = VisitorTeamTemplate
					};
				}
			}
			else {
				// New imported project from a fake live analysis will have a null File
				// return null to force selecting a new file.
				if(mFile == null)
					return null;
				UpdateProject();
				return project;
			}
		}

		public void Clear() {
			LocalGoals = 0;
			VisitorGoals = 0;
			Date = System.DateTime.Today;
			localteamlabel.Text = "";
			visitorteamlabel.Text = "";
			Filename = "";
			mFile = null;
			edited = false;
		}

		public void FillDevices(List<Device> devices) {
			videoDevices = devices;

			foreach(Device device in devices) {
				string deviceElement;
				string deviceName;
				if(Environment.OSVersion.Platform == PlatformID.Unix) {
					if(device.DeviceType == DeviceType.DV)
						deviceElement = Catalog.GetString(DV_SOURCE);
					else
						deviceElement = Catalog.GetString(GCONF_SOURCE);
				} else
					deviceElement = Catalog.GetString("DirectShow Source");
				deviceName = (device.ID == "") ? Catalog.GetString("Unknown"): device.ID;
				devicecombobox.AppendText(deviceName + " ("+deviceElement+")");
				devicecombobox.Active = 0;
			}
		}

		private void FillCategories() {
			int i=0;
			int index = 0;

			foreach(string template in  tpc.TemplatesNames) {
				tagscombobox.AppendText(template);
				//Setting the selected value to the default template
				if(template == "default")
					index = i;
				i++;
			}
			tagscombobox.Active = index;
			if (Categories == null)
				Categories = tpc.Load(SelectedCategory);
		}

		private void FillTeamsTemplate() {
			int i=0;
			int index = 0;

			foreach(string template in tpt.TemplatesNames) {
				localcombobox.AppendText(template);
				visitorcombobox.AppendText(template);

				//Setting the selected value to the default template
				if(template == "default")
					index = i;
				i++;
			}
			localcombobox.Active = index;
			visitorcombobox.Active = index;
			if (LocalTeamTemplate == null) {
				LocalTeamTemplate = tpt.Load(LocalTeamTemplateFile);
				VisitorTeamTemplate = tpt.Load(VisitorTeamTemplateFile);
			}
		}

		private void FillFormats() {
			videoStandardList = new ListStore(typeof(string), typeof (VideoStandard));
			videoStandardList.AppendValues(VideoStandards.Original.Name, VideoStandards.Original);
			videoStandardList.AppendValues(VideoStandards.P240_4_3.Name, VideoStandards.P240_4_3);
			videoStandardList.AppendValues(VideoStandards.P240_16_9.Name, VideoStandards.P240_16_9);
			videoStandardList.AppendValues(VideoStandards.P480_4_3.Name, VideoStandards.P480_4_3);
			videoStandardList.AppendValues(VideoStandards.P480_16_9.Name, VideoStandards.P480_16_9);
			videoStandardList.AppendValues(VideoStandards.P720_4_3.Name, VideoStandards.P720_4_3);
			videoStandardList.AppendValues(VideoStandards.P720_16_9.Name, VideoStandards.P720_16_9);
			videoStandardList.AppendValues(VideoStandards.P1080_4_3.Name, VideoStandards.P1080_4_3);
			videoStandardList.AppendValues(VideoStandards.P1080_16_9.Name, VideoStandards.P1080_16_9);
			sizecombobox.Model = videoStandardList;
			sizecombobox.Active = 0;

			encProfileList = new ListStore(typeof(string), typeof (EncodingProfile));
			encProfileList.AppendValues(EncodingProfiles.MP4.Name, EncodingProfiles.MP4);
			encProfileList.AppendValues(EncodingProfiles.Avi.Name, EncodingProfiles.Avi);
			if(Environment.OSVersion.Platform != PlatformID.Win32NT)
				encProfileList.AppendValues(EncodingProfiles.WebM.Name, EncodingProfiles.WebM);
			videoformatcombobox.Model = encProfileList;
			videoformatcombobox.Active = 0;
		}
		
		private void StartEditor(TemplateEditorDialog editor) {
			editor.TransientFor = (Window)Toplevel;
			editor.Run();
			editor.Destroy();
			OnEdited(this,null);
		}

		protected virtual void OnDateSelected(DateTime dateTime) {
			Date = dateTime;
		}

		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = null;

			if(useType == ProjectType.CaptureProject) {
				fChooser = new FileChooserDialog(Catalog.GetString("Output file"),
				                                 (Gtk.Window)this.Toplevel,
				                                 FileChooserAction.Save,
				                                 "gtk-cancel",ResponseType.Cancel,
				                                 "gtk-save",ResponseType.Accept);
				fChooser.SetCurrentFolder(Config.VideosDir());
				fChooser.DoOverwriteConfirmation = true;
				if(fChooser.Run() == (int)ResponseType.Accept)
					fileEntry.Text = fChooser.Filename;
				fChooser.Destroy();

			} else	{
				fChooser = new FileChooserDialog(Catalog.GetString("Open file..."),
				                                 (Gtk.Window)this.Toplevel,
				                                 FileChooserAction.Open,
				                                 "gtk-cancel",ResponseType.Cancel,
				                                 "gtk-open",ResponseType.Accept);

				fChooser.SetCurrentFolder(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal));

				if(fChooser.Run() == (int)ResponseType.Accept) {
					MessageDialog md=null;
					string filename = fChooser.Filename;
					fChooser.Destroy();
					
					if (MpegRemuxer.FileIsMpeg(filename) &&
					    MpegRemuxer.AskForConversion(this.Toplevel as Gtk.Window)) {
						var remux = new MpegRemuxer(filename);
						var newFilename = remux.Remux(this.Toplevel as Gtk.Window);
						if (newFilename != null)
							filename = newFilename;
					}
					
					try {
						md = new MessageDialog((Gtk.Window)this.Toplevel,
						                       DialogFlags.Modal,
						                       MessageType.Info,
						                       Gtk.ButtonsType.None,
						                       Catalog.GetString("Analyzing video file:")+"\n"+filename);
						md.Icon=Stetic.IconLoader.LoadIcon(this, "longomatch", Gtk.IconSize.Dialog);
						md.Show();
						mFile = PreviewMediaFile.DiscoverFile(filename);
						if(!mFile.HasVideo || mFile.VideoCodec == "")
							throw new Exception(Catalog.GetString("This file doesn't contain a video stream."));
						if(mFile.HasVideo && mFile.Length == 0)
							throw new Exception(Catalog.GetString("This file contains a video stream but its length is 0."));


						fileEntry.Text = filename;
					}
					catch(Exception ex) {
						MessagePopup.PopupMessage(this, MessageType.Error,
						                          ex.Message);
					}
					finally {
						md.Destroy();
					}
				}
				fChooser.Destroy();
			}
		}


		protected virtual void OnCalendarbuttonClicked(object sender, System.EventArgs e)
		{
			if(Environment.OSVersion.Platform == PlatformID.Win32NT) {
				win32CP = new Win32CalendarDialog();
				win32CP.TransientFor = (Gtk.Window)this.Toplevel;
				win32CP.Run();
				Date = win32CP.getSelectedDate();
				win32CP.Destroy();
			}
			else {
				cp.TransientFor=(Gtk.Window)this.Toplevel;
				cp.Show();
			}
		}

		protected virtual void OnCombobox1Changed(object sender, System.EventArgs e)
		{
			Categories = tpc.Load(SelectedCategory);
		}

		protected virtual void OnVisitorcomboboxChanged(object sender, System.EventArgs e)
		{
			VisitorTeamTemplate = tpt.Load(VisitorTeamTemplateFile);
		}

		protected virtual void OnLocalcomboboxChanged(object sender, System.EventArgs e)
		{
			LocalTeamTemplate = tpt.Load(LocalTeamTemplateFile);
		}

		protected virtual void OnEditbuttonClicked(object sender, System.EventArgs e)
		{
			var editor = new TemplateEditorDialog<Categories, Category>(twc);
			editor.Template = Categories;
			if (Use == ProjectType.EditProject) {
				editor.Project = project;
				editor.CanExport = true;
			}
			StartEditor(editor);
		}

		protected virtual void OnLocaltemplatebuttonClicked(object sender, System.EventArgs e) {
			var editor = new TemplateEditorDialog<TeamTemplate, Player>(twt);
			editor.Template = LocalTeamTemplate;
			if (Use == ProjectType.EditProject) {
				editor.Project = project;
				editor.CanExport = true;
			}
			StartEditor(editor);
		}

		protected virtual void OnVisitorbuttonClicked(object sender, System.EventArgs e) {
			var editor = new TemplateEditorDialog<TeamTemplate, Player>(twt);
			editor.Template = VisitorTeamTemplate;
			if (Use == ProjectType.EditProject) {
				editor.Project = project;
				editor.CanExport = true;
			}
			StartEditor(editor);
		}

		protected virtual void OnEdited(object sender, System.EventArgs e) {
			Edited = true;
			if(EditedEvent != null)
				EditedEvent(this,null);
		}
	}
}
