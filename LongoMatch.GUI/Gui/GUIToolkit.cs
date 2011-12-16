// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using Mono.Unix;

using Image = LongoMatch.Common.Image;
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Popup;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Video.Utils;

namespace LongoMatch.Gui
{
	public class GUIToolkit: IGUIToolkit
	{
		IMainWindow mainWindow;
		
		public GUIToolkit ()
		{
			mainWindow = new MainWindow(this);
			(mainWindow as MainWindow).Show();
		}
		
		public IMainWindow MainWindow{
			get {
				return mainWindow;
			}
		}
		
		public void InfoMessage(string message) {
			MessagePopup.PopupMessage(mainWindow as Gtk.Widget, Gtk.MessageType.Info, message);
		}
		
		public void ErrorMessage(string message) {
			MessagePopup.PopupMessage(mainWindow as Gtk.Widget, Gtk.MessageType.Error, message);
		}
		
		public void WarningMessage(string message) {
			MessagePopup.PopupMessage(mainWindow as Gtk.Widget, Gtk.MessageType.Warning, message);
		}
		
		public bool QuestionMessage(string question, string title) {
			MessageDialog md = new MessageDialog(mainWindow as Gtk.Window, DialogFlags.Modal,
				MessageType.Question, Gtk.ButtonsType.YesNo, question);
			md.Icon = Stetic.IconLoader.LoadIcon(mainWindow as Widget, "longomatch", IconSize.Button);
			var res = md.Run();
			md.Destroy();
			return (res == (int)ResponseType.Yes);
		}
		
		public string SaveFile(string title, string defaultName, string defaultFolder,
			string filterName, string extensionFilter)
		{
			return FileChooser(title, defaultName, defaultFolder, filterName,
				extensionFilter, FileChooserAction.Save);
		}
		
		public string SelectFolder(string title, string defaultName, string defaultFolder,
			string filterName, string extensionFilter)
		{
			return FileChooser(title, defaultName, defaultFolder, filterName,
				extensionFilter, FileChooserAction.SelectFolder);
		}
		
		public string OpenFile(string title, string defaultName, string defaultFolder,
			string filterName, string extensionFilter)
		{
			return FileChooser(title, defaultName, defaultFolder, filterName,
				extensionFilter, FileChooserAction.Open);
		}
		
		public Job ConfigureRenderingJob (IPlayList playlist)
		{
			VideoEditionProperties vep;
			Job job = null;
			int response;

			vep = new VideoEditionProperties();
			vep.TransientFor = mainWindow as Gtk.Window;
			response = vep.Run();
			while(response == (int)ResponseType.Ok && vep.EncodingSettings.OutputFile == "") {
				WarningMessage(Catalog.GetString("Please, select a video file."));
				response=vep.Run();
			}
			if(response ==(int)ResponseType.Ok)
				job = new Job(playlist, vep.EncodingSettings, vep.EnableAudio, vep.TitleOverlay);
			vep.Destroy();
			return job;
		}
		
		public void ExportFrameSeries(Project openedProject, Play play, string snapshotsDir) {
			SnapshotsDialog sd;
			uint interval;
			string seriesName;
			string outDir;


			sd= new SnapshotsDialog();
			sd.TransientFor= mainWindow as Gtk.Window;
			sd.Play = play.Name;

			if(sd.Run() == (int)ResponseType.Ok) {
				sd.Destroy();
				interval = sd.Interval;
				seriesName = sd.SeriesName;
				outDir = System.IO.Path.Combine(snapshotsDir, seriesName);
				var fsc = new FramesSeriesCapturer(openedProject.Description.File.FilePath,
				                               play.Start.MSeconds, play.Stop.MSeconds,
				                               interval, outDir);
				var fcpd = new FramesCaptureProgressDialog(fsc);
				fcpd.TransientFor = mainWindow as Gtk.Window;
				fcpd.Run();
				fcpd.Destroy();
			}
			else
				sd.Destroy();
		}
		
		public void TagPlay (Play play, TeamTemplate local, TeamTemplate visitor) {
			TaggerDialog tg = new TaggerDialog(play.Category, play.Tags, play.Players, play.Teams,
			                                   local, visitor);
			tg.TransientFor = mainWindow as Gtk.Window;
			tg.Run();
			tg.Destroy();
		}

		public void DrawingTool (Image image, Play play, int stopTime) {
			DrawingTool dialog = new DrawingTool();

			dialog.Image = image.Value;
			if (play != null)
				dialog.SetPlay(play, stopTime);
			dialog.TransientFor = mainWindow as Gtk.Window;
			image.Dispose();
			dialog.Run();
			dialog.Destroy();	
		}
		
		public ProjectDescription SelectProject(List<ProjectDescription> projects) {
			ProjectDescription project = null;
			OpenProjectDialog opd = new OpenProjectDialog();
			
			opd.Fill(projects);	
			opd.TransientFor = mainWindow as Gtk.Window;
			if(opd.Run() == (int)ResponseType.Ok)
				project = opd.SelectedProject;
			opd.Destroy();
			return project;
		}
		
		public void OpenCategoriesTemplatesManager(ICategoriesTemplatesProvider tp)
		{
			var tManager = new TemplatesManager<Categories, Category> (tp, new CategoriesTemplateEditorWidget (tp));
			tManager.TransientFor = mainWindow as Gtk.Window;
			tManager.Show();
		}

		public void OpenTeamsTemplatesManager(ITeamTemplatesProvider tp)
		{
			var tManager = new TemplatesManager<TeamTemplate, Player>(tp, new TeamTemplateEditorWidget (tp));
			tManager.TransientFor = mainWindow as Gtk.Window;
			tManager.Show();
		}
		
		public void OpenProjectsManager(Project openedProject, IDatabase db, ITemplatesService ts)
		{
			Gui.Dialog.ProjectsManager pm = new Gui.Dialog.ProjectsManager(openedProject, db, ts);
			pm.TransientFor = mainWindow as Gtk.Window;
			pm.Show();
		}
		
		public void ManageJobs(IRenderingJobsManager manager) {
			RenderingJobsDialog dialog = new RenderingJobsDialog(manager);
			dialog.TransientFor = mainWindow as Gtk.Window;
			dialog.Run();
			dialog.Destroy();
		}
		
		public ProjectType SelectNewProjectType () {
			ProjectSelectionDialog psd;
			int response;

			psd = new ProjectSelectionDialog();
			psd.TransientFor = mainWindow as Gtk.Window;;
			response = psd.Run();
			psd.Destroy();
			if(response != (int)ResponseType.Ok)
				return ProjectType.None;
			return psd.ProjectType;
		}
		
		public Project NewCaptureProject(IDatabase db, ITemplatesService ts,
			List<LongoMatch.Common.Device> devices, out CaptureSettings captureSettings)
		{
			return NewProject(db, null, ProjectType.CaptureProject, ts, devices, out captureSettings);
		}
		
		public Project NewFakeProject(IDatabase db, ITemplatesService ts) {
			CaptureSettings captureSettings = new CaptureSettings();
			return NewProject(db, null, ProjectType.FakeCaptureProject, ts, null, out captureSettings);
		}
		
		public Project NewFileProject(IDatabase db, ITemplatesService ts) {
			CaptureSettings captureSettings = new CaptureSettings();
			return NewProject(db, null, ProjectType.FileProject, ts, null, out captureSettings);
		}
		
		public Project EditFakeProject(IDatabase db, Project project, ITemplatesService ts) {
			CaptureSettings captureSettings = new CaptureSettings();
			return NewProject(db, project, ProjectType.EditProject, ts, null, out captureSettings);
		}
		
		public IBusyDialog BusyDialog(string message) {
			BusyDialog dialog;

			dialog = new BusyDialog();
			dialog.TransientFor = mainWindow as Gtk.Window;
			dialog.Message = message; 
			return dialog;
		}
		
		Project NewProject(IDatabase db, Project project, ProjectType type,
			ITemplatesService tps, List<LongoMatch.Common.Device> devices, out CaptureSettings captureSettings)
		{
			NewProjectDialog npd = new NewProjectDialog();
			
			npd.TransientFor = mainWindow as Gtk.Window;
			npd.Use = type;
			npd.TemplatesService = tps;
			npd.Project = project;
			if(type == ProjectType.CaptureProject)
				npd.Devices = devices;
			int response = npd.Run();
			while(true) {
				if(response != (int)ResponseType.Ok) {
					break;
				} else if(npd.Project == null) {
					InfoMessage(Catalog.GetString("Please, select a video file."));
					response=npd.Run();
				} else {
					project = npd.Project;
					break;
				}
			}	
			if (type == ProjectType.CaptureProject)
				captureSettings = npd.CaptureSettings;
			npd.Destroy();
			return project;
		}
		
		string  FileChooser(string title, string defaultName,
			string defaultFolder, string filterName, string extensionFilter,
			FileChooserAction action)
		{
			FileChooserDialog fChooser;
			FileFilter filter;
			string button, path;
			
			if (action == FileChooserAction.Save)
				button = "gtk-save";
			else
				button = "gtk-open";
			
			fChooser = new FileChooserDialog(title, mainWindow as Gtk.Window, action,
				"gtk-cancel",ResponseType.Cancel, button, ResponseType.Accept);
			
			if (defaultFolder != null)
				fChooser.SetCurrentFolder(defaultFolder);
			if (defaultName != null)
				fChooser.SetFilename(defaultName);
			if (filterName != null) {
				filter = new FileFilter();
				filter.Name = filterName;
				filter.AddPattern(extensionFilter);
				fChooser.AddFilter(filter);	
			}
			
			if (fChooser.Run() != (int)ResponseType.Accept) 
				path = null;
			else
				path = fChooser.Filename;
			
			fChooser.Destroy();
			return path;
		}
	}
}

