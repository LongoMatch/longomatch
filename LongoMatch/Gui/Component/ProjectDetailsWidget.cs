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
using Mono.Unix;
using Gtk;
using LongoMatch.Common;
using LongoMatch.DB;
using LongoMatch.Handlers;
using LongoMatch.IO;
using LongoMatch.Gui.Popup;
using LongoMatch.Gui.Dialog;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Utils;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Common;

namespace LongoMatch.Gui.Component
{


	//TODO añadir eventos de cambios para realizar el cambio directamente sobre el file data abierto
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectDetailsWidget : Gtk.Bin
	{
		public event EventHandler EditedEvent;
		private Project project;
		private LongoMatch.Video.Utils.PreviewMediaFile mFile;
		private bool edited;
		private DateTime date;
		private CalendarPopup cp;
		private Win32CalendarDialog win32CP;
		private Sections actualSection;
		private TeamTemplate actualVisitorTeam;
		private TeamTemplate actualLocalTeam;
		private ProjectType useType;
		private List<Device> videoDevices;
		private const string PAL_FORMAT = "720x576 (4:3)";
		private const string PAL_3_4_FORMAT = "540x432 (4:3)";
		private const string PAL_1_2_FORMAT = "360x288 (4:3)";
		private const string DV_SOURCE = "DV Source";
		private const string GCONF_SOURCE = "GConf Source";
		
		
		public ProjectDetailsWidget()
		{
			this.Build();

			//HACK:The calendar dialog does not respond on win32
			if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
				cp = new CalendarPopup();
				cp.Hide();
				cp.DateSelectedEvent += new DateSelectedHandler(OnDateSelected);
			}
			
			FillSections();
			FillTeamsTemplate();
			FillFormats();
			
			videoDevices = new List<Device>();

			Use=ProjectType.FileProject;
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

		public string LocalName {
			get {
				return localTeamEntry.Text;
			}
			set {
				localTeamEntry.Text = value;
			}
		}

		public string VisitorName {
			get {
				return visitorTeamEntry.Text;
			}
			set {
				visitorTeamEntry.Text = value;
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

		public Sections Sections {
			get {
				return actualSection;
			}
			set {
				actualSection = value;
			}
		}

		public TeamTemplate LocalTeamTemplate {
			get {
				return actualLocalTeam;
			}
			set {
				actualLocalTeam = value;
			}
		}

		public TeamTemplate VisitorTeamTemplate {
			get {
				return actualVisitorTeam;
			}
			set {
				actualVisitorTeam = value;
			}
		}

		private string SectionsFile {
			get {
				return tagscombobox.ActiveText + ".sct";
			}
		}

		private string LocalTeamTemplateFile {
			get {
				return localcombobox.ActiveText + ".tem";
			}
		}

		private string VisitorTeamTemplateFile {
			get {
				return visitorcombobox.ActiveText + ".tem";
			}
		}
		
		public CapturePropertiesStruct CaptureProperties{
			get{
				CapturePropertiesStruct s = new CapturePropertiesStruct();
				s.OutputFile = fileEntry.Text;
				s.AudioBitrate = (uint)audiobitratespinbutton.Value;
				s.VideoBitrate = (uint)videobitratespinbutton.Value;
				if (videoDevices[devicecombobox.Active].DeviceType == DeviceType.DV){
					if (Environment.OSVersion.Platform == PlatformID.Win32NT)
						s.CaptureSourceType = CaptureSourceType.DShow;
					else
						s.CaptureSourceType = CaptureSourceType.DV;
				}
				else {
					s.CaptureSourceType = CaptureSourceType.Raw;
				}
				s.DeviceID = videoDevices[devicecombobox.Active].ID;
				/* Get size info */
				switch (sizecombobox.ActiveText){
					/* FIXME: Don't harcode size values */
					case PAL_FORMAT:
						s.Width = 720;
						s.Height = 576;
						break;
					case PAL_3_4_FORMAT:
						s.Width = 540;
						s.Height = 432;
						break;
					case PAL_1_2_FORMAT:
						s.Width = 360;
						s.Height = 288;
						break;
					default:
						s.Width = 0;
						s.Height = 0;
						break;
				}
				/* Get video compresion format info */
				switch (videoformatcombobox.ActiveText){
					case Constants.AVI:
						s.VideoEncoder = VideoEncoderType.Mpeg4;
						s.AudioEncoder = AudioEncoderType.Mp3;
						s.Muxer = VideoMuxerType.Avi;
						break;
					case Constants.MP4:
						s.VideoEncoder = VideoEncoderType.H264;
						s.AudioEncoder = AudioEncoderType.Aac;
						s.Muxer = VideoMuxerType.Mp4;
						break;
					case Constants.OGG:
						s.VideoEncoder = VideoEncoderType.Theora;
						s.AudioEncoder = AudioEncoderType.Vorbis;
						s.Muxer = VideoMuxerType.Ogg;
						break;
					case Constants.WEBM:
						s.VideoEncoder = VideoEncoderType.VP8;
						s.AudioEncoder = AudioEncoderType.Vorbis;
						s.Muxer = VideoMuxerType.WebM;
						break;
				}
				return s;
			}
		}
		
		public void SetProject(Project project) {
			this.project = project;
			mFile = project.File;
			Filename = mFile != null ? mFile.FilePath : "";
			LocalName = project.LocalName;
			VisitorName = project.VisitorName;
			LocalGoals = project.LocalGoals;
			VisitorGoals = project.VisitorGoals;
			Date = project.MatchDate;
			Season = project.Season;
			Competition = project.Competition;
			Sections = project.Sections;
			LocalTeamTemplate = project.LocalTeamTemplate;
			VisitorTeamTemplate = project.VisitorTeamTemplate;
			Edited = false;
		}

		public void UpdateProject() {
			project.File= mFile;			
			project.LocalName = localTeamEntry.Text;
			project.VisitorName = visitorTeamEntry.Text;
			project.LocalGoals = (int)localSpinButton.Value;
			project.VisitorGoals = (int)visitorSpinButton.Value;
			project.MatchDate = DateTime.Parse(dateEntry.Text);
			project.Competition = competitionentry.Text;
			project.Season = seasonentry.Text;
			project.Sections = Sections;
			project.LocalTeamTemplate = LocalTeamTemplate;
			project.VisitorTeamTemplate = VisitorTeamTemplate;
		}

		public Project GetProject() {
			if (useType != ProjectType.EditProject) {
				if (Filename == "" && useType != ProjectType.FakeCaptureProject)
					return null;
				else {
					if (useType == ProjectType.FakeCaptureProject){
						mFile = new PreviewMediaFile();
						mFile.FilePath = Constants.FAKE_PROJECT;
						mFile.Fps = 25;
					} else if  (useType == ProjectType.CaptureProject){
						mFile = new PreviewMediaFile();
						mFile.FilePath = fileEntry.Text;
						mFile.Fps = 25;
					}
					return new Project(mFile,
					                   LocalName,
					                   VisitorName,
					                   Season,
					                   Competition,
					                   LocalGoals,
					                   VisitorGoals,
					                   Date,
					                   Sections,
					                   LocalTeamTemplate,
					                   VisitorTeamTemplate);
				}				
			}
			else {
				// New imported project from a fake live analysis will have a null File
				// return null to force selecting a new file.
				if (mFile == null)
					return null;
				UpdateProject();
				return project;
			}
		}

		public void Clear() {
			LocalName = "";
			VisitorName = "";
			LocalGoals = 0;
			VisitorGoals = 0;
			Date = System.DateTime.Today;
			Filename = "";
			mFile = null;
			edited = false;
		}
		
		public void FillDevices(List<Device> devices){
			videoDevices = devices;
			
			foreach (Device device in devices){
				string deviceElement;
				string deviceName;
				if (Environment.OSVersion.Platform == PlatformID.Unix){
					if (device.DeviceType == DeviceType.DV)
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

		private void FillSections() {
			string[] allFiles;
			int i=0;
			int index = 0;

			allFiles = System.IO.Directory.GetFiles(MainClass.TemplatesDir(),"*.sct");
			foreach (string filePath in allFiles) {
				string fileName = System.IO	.Path.GetFileNameWithoutExtension(filePath);
				tagscombobox.AppendText(fileName);
				//Setting the selected value to the default template
				if (fileName == "default")
					index = i;
				i++;
			}
			tagscombobox.Active = index;
			SectionsReader reader = new SectionsReader(System.IO.Path.Combine(MainClass.TemplatesDir(),SectionsFile));
			Sections= reader.GetSections();
		}

		private void FillTeamsTemplate() {
			string[] allFiles;
			int i=0;
			int index = 0;

			allFiles = System.IO.Directory.GetFiles(MainClass.TemplatesDir(),"*.tem");
			foreach (string filePath in allFiles) {
				string fileName = System.IO	.Path.GetFileNameWithoutExtension(filePath);
				localcombobox.AppendText(fileName);
				visitorcombobox.AppendText(fileName);

				//Setting the selected value to the default template
				if (fileName == "default")
					index = i;
				i++;
			}
			localcombobox.Active = index;
			visitorcombobox.Active = index;
			LocalTeamTemplate = TeamTemplate.LoadFromFile(System.IO.Path.Combine(MainClass.TemplatesDir(),LocalTeamTemplateFile));
			VisitorTeamTemplate = TeamTemplate.LoadFromFile(System.IO.Path.Combine(MainClass.TemplatesDir(),VisitorTeamTemplateFile));
		}
		
		private void FillFormats(){
			sizecombobox.AppendText (Catalog.GetString("Keep original size"));
			sizecombobox.AppendText(PAL_FORMAT);
			sizecombobox.AppendText(PAL_3_4_FORMAT);
			sizecombobox.AppendText(PAL_1_2_FORMAT);
			sizecombobox.Active = 0;
		
			videoformatcombobox.AppendText(Constants.AVI);
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				videoformatcombobox.AppendText(Constants.WEBM);
			videoformatcombobox.AppendText(Constants.OGG);
			videoformatcombobox.AppendText(Constants.MP4);
			videoformatcombobox.Active = 0;
		}
		
		protected virtual void OnDateSelected(DateTime dateTime) {
			Date = dateTime;
		}

		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = null;

			if (useType == ProjectType.CaptureProject) {
				fChooser = new FileChooserDialog(Catalog.GetString("Output file"),
				                                 (Gtk.Window)this.Toplevel,
				                                 FileChooserAction.Save,
				                                 "gtk-cancel",ResponseType.Cancel,
				                                 "gtk-save",ResponseType.Accept);
				fChooser.SetCurrentFolder(MainClass.VideosDir());
				fChooser.DoOverwriteConfirmation = true;
				if (fChooser.Run() == (int)ResponseType.Accept)
					fileEntry.Text = fChooser.Filename;
				fChooser.Destroy();

			} else	{
				fChooser = new FileChooserDialog(Catalog.GetString("Open file..."),
				                                 (Gtk.Window)this.Toplevel,
				                                 FileChooserAction.Open,
				                                 "gtk-cancel",ResponseType.Cancel,
				                                 "gtk-open",ResponseType.Accept);

				fChooser.SetCurrentFolder(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal));

				if (fChooser.Run() == (int)ResponseType.Accept) {
					MessageDialog md=null;
					string filename = fChooser.Filename;
					fChooser.Destroy();
					try {
						md = new MessageDialog((Gtk.Window)this.Toplevel,
						                       DialogFlags.Modal,
						                       MessageType.Info,
						                       Gtk.ButtonsType.None,
						                       Catalog.GetString("Analyzing video file:")+"\n"+filename);
						md.Icon=Stetic.IconLoader.LoadIcon(this, "longomatch", Gtk.IconSize.Dialog, 48);
						md.Show();
						mFile = LongoMatch.Video.Utils.PreviewMediaFile.GetMediaFile(filename);
						if (!mFile.HasVideo || mFile.VideoCodec == "")
							throw new Exception(Catalog.GetString("This file doesn't contain a video stream."));
						if (mFile.HasVideo && mFile.Length == 0)
							throw new Exception(Catalog.GetString("This file contains a video stream but its length is 0."));
						
							
						fileEntry.Text = filename;
					}
					catch (Exception ex) {
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
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
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
			SectionsReader reader = new SectionsReader(System.IO.Path.Combine(MainClass.TemplatesDir(),SectionsFile));
			Sections= reader.GetSections();
		}

		protected virtual void OnVisitorcomboboxChanged(object sender, System.EventArgs e)
		{
			VisitorTeamTemplate = TeamTemplate.LoadFromFile(System.IO.Path.Combine(MainClass.TemplatesDir(), VisitorTeamTemplateFile));
		}


		protected virtual void OnLocalcomboboxChanged(object sender, System.EventArgs e)
		{
			LocalTeamTemplate = TeamTemplate.LoadFromFile(System.IO.Path.Combine(MainClass.TemplatesDir(), LocalTeamTemplateFile));
		}

		protected virtual void OnEditbuttonClicked(object sender, System.EventArgs e)
		{
			ProjectTemplateEditorDialog ted = new ProjectTemplateEditorDialog();
			ted.TransientFor = (Window)Toplevel;
			ted.Sections = Sections;
			ted.Project = project;
			ted.CanExport = Use == ProjectType.EditProject;
			if (ted.Run() == (int)ResponseType.Apply) {
				Sections = ted.Sections;
			}
			ted.Destroy();
			OnEdited(this,null);
		}

		protected virtual void OnLocaltemplatebuttonClicked(object sender, System.EventArgs e) {
			TeamTemplateEditor tted = new TeamTemplateEditor();
			tted.TransientFor = (Window)Toplevel;
			tted.Title=Catalog.GetString("Local Team Template");
			tted.SetTeamTemplate(LocalTeamTemplate);
			
			if (tted.Run() == (int)ResponseType.Apply) {
				LocalTeamTemplate = tted.GetTeamTemplate();
			}
			tted.Destroy();
			OnEdited(this,null);
		}

		protected virtual void OnVisitorbuttonClicked(object sender, System.EventArgs e) {
			TeamTemplateEditor tted = new TeamTemplateEditor();
			tted.TransientFor = (Window)Toplevel;
			tted.Title=Catalog.GetString("Visitor Team Template");
			tted.SetTeamTemplate(VisitorTeamTemplate);
			if (tted.Run() == (int)ResponseType.Apply) {
				VisitorTeamTemplate = tted.GetTeamTemplate();
			}
			tted.Destroy();
			OnEdited(this,null);
		}

		protected virtual void OnEdited(object sender, System.EventArgs e) {
			Edited = true;
			if (EditedEvent != null)
				EditedEvent(this,null);
		}
	}
}
