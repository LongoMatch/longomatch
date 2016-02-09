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
using System.IO;
using System.Threading.Tasks;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Helpers;
using LongoMatch.Gui.Panel;
using LongoMatch.Video.Utils;
using LongoMatch.Core;
using Image = LongoMatch.Core.Common.Image;

namespace LongoMatch.Gui
{
	public sealed class GUIToolkit: IGUIToolkit
	{
		static readonly GUIToolkit instance = new GUIToolkit ();

		public static GUIToolkit Instance {
			get {
				return instance;
			}
		}

		MainWindow mainWindow;
		Registry registry;

		private GUIToolkit ()
		{
			mainWindow = new MainWindow (this);
			mainWindow.Hide ();
			registry = new Registry ("GUI backend");
		}

		public IMainController MainController {
			get {
				return mainWindow;
			}
		}

		public IRenderingStateBar RenderingStateBar {
			get {
				return mainWindow.RenderingStateBar;
			}
		}

		public bool FullScreen {
			set {
				if (mainWindow != null) {
					if (value)
						mainWindow.GdkWindow.Fullscreen ();
					else
						mainWindow.GdkWindow.Unfullscreen ();
				}
			}
		}

		public void Register <I, C> (int priority)
		{
			registry.Register<I, C> (priority);
		}

		public IPlayerView GetPlayerView ()
		{
			return registry.Retrieve<IPlayerView> ();
		}

		public void InfoMessage (string message, object parent = null)
		{
			if (parent == null)
				parent = mainWindow as Widget;
			MessagesHelpers.InfoMessage (parent as Widget, message);
		}

		public void ErrorMessage (string message, object parent = null)
		{
			if (parent == null)
				parent = mainWindow as Widget;
			MessagesHelpers.ErrorMessage (parent as Widget, message);
		}

		public void WarningMessage (string message, object parent = null)
		{
			if (parent == null)
				parent = mainWindow as Widget;
			MessagesHelpers.WarningMessage (parent as Widget, message);
		}

		public Task<bool> QuestionMessage (string question, string title, object parent = null)
		{
			if (parent == null)
				parent = mainWindow as Widget;
			bool res = MessagesHelpers.QuestionMessage (parent as Widget, question, title);
			return Task.Factory.StartNew (() => res);
		}

		public Task<string> QueryMessage (string key, string title = null, string value = "", object parent = null)
		{
			if (parent == null)
				parent = mainWindow;
			string res = MessagesHelpers.QueryMessage (parent as Widget, key, title, value);
			return Task.Factory.StartNew (() => res);
		}

		public Task<bool> NewVersionAvailable (Version currentVersion, Version latestVersion,
		                                       string downloadURL, string changeLog, object parent = null)
		{
			if (parent == null)
				parent = mainWindow;
			bool res = MessagesHelpers.NewVersionAvailable (currentVersion, latestVersion, downloadURL,
				           changeLog, parent as Widget);
			return Task.Factory.StartNew (() => res);
		}

		public string SaveFile (string title, string defaultName, string defaultFolder,
		                        string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.SaveFile (mainWindow as Widget, title, defaultName,
				defaultFolder, filterName, extensionFilter);
		}

		public string SelectFolder (string title, string defaultName, string defaultFolder,
		                            string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.SelectFolder (mainWindow as Widget, title, defaultName,
				defaultFolder, filterName, extensionFilter);
		}

		public string OpenFile (string title, string defaultName, string defaultFolder,
		                        string filterName = null, string[] extensionFilter = null)
		{
			return FileChooserHelper.OpenFile (mainWindow as Widget, title, defaultName,
				defaultFolder, filterName, extensionFilter);
		}

		public List<string> OpenFiles (string title, string defaultName, string defaultFolder,
		                               string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.OpenFiles (mainWindow as Widget, title, defaultName,
				defaultFolder, filterName, extensionFilter);
		}

		public Task<object> ChooseOption (Dictionary<string, object> options, object parent = null)
		{
			object res = null;
			Window parentWindow;
			ChooseOptionDialog dialog; 

			if (parent != null) {
				parentWindow = (parent as Widget).Toplevel as Gtk.Window;
			} else {
				parentWindow = mainWindow as Gtk.Window;
			}

			dialog = new ChooseOptionDialog (parentWindow);
			dialog.Options = options;

			if (dialog.Run () == (int)ResponseType.Ok) {
				res = dialog.SelectedOption;
			}
			dialog.Destroy ();
			var task = Task.Factory.StartNew (() => res);
			return task;
		}

		public List<EditionJob> ConfigureRenderingJob (Playlist playlist)
		{
			VideoEditionProperties vep;
			List<EditionJob> jobs = new List<EditionJob> ();
			int response;
			
			Log.Information ("Configure rendering job");
			if (playlist.Elements.Count == 0) {
				WarningMessage (Catalog.GetString ("The playlist you want to render is empty."));
				return null;
			}

			vep = new VideoEditionProperties (mainWindow as Gtk.Window);
			vep.Playlist = playlist;
			response = vep.Run ();
			while (response == (int)ResponseType.Ok) {
				if (!vep.SplitFiles && vep.EncodingSettings.OutputFile == "") {
					WarningMessage (Catalog.GetString ("Please, select a video file."));
					response = vep.Run ();
				} else if (vep.SplitFiles && vep.OutputDir == null) {
					WarningMessage (Catalog.GetString ("Please, select an output directory."));
					response = vep.Run ();
				} else {
					break;
				}
			}
			if (response == (int)ResponseType.Ok) {
				if (!vep.SplitFiles) {
					jobs.Add (new EditionJob (playlist, vep.EncodingSettings));
				} else {
					int i = 0;
					foreach (IPlaylistElement play in playlist.Elements) {
						EncodingSettings settings;
						Playlist pl;
						string name, ext, filename;

						settings = vep.EncodingSettings;
						pl = new Playlist ();
						if (play is PlaylistPlayElement) {
							name = (play as PlaylistPlayElement).Play.Name;
							ext = settings.EncodingProfile.Extension;
						} else {
							name = "image";
							ext = "png";
						}
						filename = String.Format ("{0}-{1}.{2}", i.ToString ("d4"), name, ext);
						
						pl.Elements.Add (play);
						settings.OutputFile = Path.Combine (vep.OutputDir, filename);
						jobs.Add (new EditionJob (pl, settings));
						i++;
					}
				}
			}
			vep.Destroy ();
			return jobs;
		}

		public void ExportFrameSeries (Project openedProject, TimelineEvent play, string snapshotsDir)
		{
			SnapshotsDialog sd;
			uint interval;
			string seriesName;
			string outDir;

			Log.Information ("Export frame series");
			sd = new SnapshotsDialog (mainWindow as Gtk.Window);
			sd.Play = play.Name;

			if (sd.Run () == (int)ResponseType.Ok) {
				interval = sd.Interval;
				seriesName = sd.SeriesName;
				sd.Destroy ();
				outDir = System.IO.Path.Combine (snapshotsDir, seriesName);
				var fsc = new FramesSeriesCapturer (openedProject.Description.FileSet, play, interval, outDir);
				var fcpd = new FramesCaptureProgressDialog (fsc, mainWindow as Gtk.Window);
				fcpd.Run ();
				fcpd.Destroy ();
			} else
				sd.Destroy ();
		}

		public void EditPlay (TimelineEvent play, Project project, bool editTags, bool editPos, bool editPlayers, bool editNotes)
		{
			if (play is StatEvent) {
				SubstitutionsEditor dialog = new SubstitutionsEditor (mainWindow as Gtk.Window);
				dialog.Load (project, play as StatEvent);
				if (dialog.Run () == (int)ResponseType.Ok) {
					dialog.SaveChanges ();
				}
				dialog.Destroy ();
			} else {
				PlayEditor dialog = new PlayEditor (mainWindow as Gtk.Window);
				dialog.LoadPlay (play, project, editTags, editPos, editPlayers, editNotes);
				dialog.Run ();
				dialog.Destroy ();
			}
		}

		public void DrawingTool (Image image, TimelineEvent play, FrameDrawing drawing,
		                         CameraConfig camConfig, Project project)
		{
			DrawingTool dialog = new DrawingTool (mainWindow);
			dialog.TransientFor = mainWindow;

			Log.Information ("Drawing tool");
			if (play == null) {
				dialog.LoadFrame (image, project);
			} else {
				dialog.LoadPlay (play, image, drawing, camConfig, project);
			}
			dialog.Show ();
			dialog.Run ();
			dialog.Destroy ();
		}

		public Project ChooseProject (List<Project> projects)
		{
			Log.Information ("Choosing project");
			Project project = null;
			ChooseProjectDialog dialog = new ChooseProjectDialog (mainWindow);
			dialog.Fill (projects);
			if (dialog.Run () == (int)ResponseType.Ok) {
				project = dialog.Project;
			}
			dialog.Destroy ();
			return project;
		}

		public void SelectProject (List<Project> projects)
		{
			Log.Information ("Select project");
			mainWindow.SelectProject (projects);
		}

		public void OpenCategoriesTemplatesManager ()
		{
			SportsTemplatesPanel panel = new SportsTemplatesPanel ();
			Log.Information ("Open sports templates manager");
			mainWindow.SetPanel (panel);
		}

		public void OpenTeamsTemplatesManager ()
		{
			TeamsTemplatesPanel panel = new TeamsTemplatesPanel ();
			Log.Information ("Open teams templates manager");
			mainWindow.SetPanel (panel);
		}

		public void OpenProjectsManager (Project openedProject)
		{
			ProjectsManagerPanel panel = new ProjectsManagerPanel (openedProject);
			Log.Information ("Open projects manager");
			mainWindow.SetPanel (panel);
		}

		public void OpenPreferencesEditor ()
		{
			PreferencesPanel panel = new PreferencesPanel ();
			Log.Information ("Open preferences");
			mainWindow.SetPanel (panel);
		}

		public void OpenDatabasesManager ()
		{
			DatabasesManager dm = new DatabasesManager (mainWindow);
			Log.Information ("Open db manager");
			dm.Run ();
			dm.Destroy ();
		}

		public void ManageJobs ()
		{
			RenderingJobsDialog dialog = new RenderingJobsDialog (mainWindow as Gtk.Window);
			Log.Information ("Manage jobs");
			dialog.Run ();
			dialog.Destroy ();
		}

		public IBusyDialog BusyDialog (string message, object parent = null)
		{
			BusyDialog dialog;
			Window parentWindow;

			if (parent != null) {
				parentWindow = (parent as Widget).Toplevel as Gtk.Window;
			} else {
				parentWindow = mainWindow as Gtk.Window;
			}
			dialog = new BusyDialog (parentWindow);
			dialog.Message = message; 
			return dialog;
		}

		public void Welcome ()
		{
			mainWindow.Show ();
			mainWindow.Welcome ();
		}

		public void LoadPanel (IPanel panel)
		{
			mainWindow.SetPanel ((Widget)panel);
		}

		public void CreateNewProject (Project project = null)
		{
			mainWindow.CreateNewProject (project);
		}

		public void ShowProjectStats (Project project)
		{
			Log.Information ("Show project stats");
			Addins.AddinsManager.ShowStats (project);
			System.GC.Collect ();
		}

		public string RemuxFile (string inputFile, string outputFile, VideoMuxerType muxer)
		{
			Log.Information ("Remux file");
			Remuxer remuxer = new Remuxer (Config.MultimediaToolkit.DiscoverFile (inputFile),
				                  outputFile, muxer);
			return remuxer.Remux (mainWindow as Gtk.Window);
		}

		public void OpenProject (Project project, ProjectType projectType, 
		                         CaptureSettings props, EventsFilter filter,
		                         out IAnalysisWindow analysisWindow)
		{
			Log.Information ("Open project");
			analysisWindow = mainWindow.SetProject (project, projectType, props, filter);
		}

		public void CloseProject ()
		{
			Log.Information ("Close project");
			mainWindow.CloseProject ();
		}

		public Task<DateTime> SelectDate (DateTime date, object widget)
		{
			CalendarDialog dialog = new CalendarDialog (date);
			dialog.TransientFor = (widget as Widget).Toplevel as Gtk.Window;
			dialog.Run ();
			date = dialog.Date;
			dialog.Destroy ();
			var task = Task.Factory.StartNew (() => date);
			return task;
		}

		public EndCaptureResponse EndCapture (bool isCapturing)
		{
			int res;
			EndCaptureDialog dialog = new EndCaptureDialog (mainWindow, isCapturing);
			res = dialog.Run ();
			dialog.Destroy ();
			return (EndCaptureResponse)res;
		}

		public bool SelectMediaFiles (MediaFileSet fileSet)
		{
			bool ret = false;
			MediaFileSetSelection fileselector = new MediaFileSetSelection (false);
			Gtk.Dialog d = new Gtk.Dialog (Catalog.GetString ("Select video files"),
				               mainWindow.Toplevel as Gtk.Window,
				               DialogFlags.Modal | DialogFlags.DestroyWithParent,
				               Gtk.Stock.Cancel, ResponseType.Cancel,
				               Gtk.Stock.Ok, ResponseType.Ok);
			fileselector.Show ();
			fileselector.FileSet = fileSet.Clone ();
			d.VBox.Add (fileselector);
			WarningMessage (Catalog.GetString ("Some video files are missing for this project"));
			while (d.Run () == (int)ResponseType.Ok) {
				if (!fileselector.FileSet.CheckFiles ()) {
					WarningMessage (Catalog.GetString ("Some video files are still missing for this project."), d);
					continue;
				}
				if (fileselector.FileSet.Count == 0) {
					WarningMessage (Catalog.GetString ("You need at least 1 video file for the main angle"));
					continue;
				}
				ret = true;
				break;
			}
			if (ret) {
				// We need to update the fileset as it might have changed. Indeed if multi camera is not supported
				// widget will propose only one media file selector and will return a smaller fileset than the 
				// one provided originally.
				fileSet.Clear ();
				for (int i = 0; i < fileselector.FileSet.Count; i++) {
					fileSet.Add (fileselector.FileSet [i]);
				}
			}
			d.Destroy ();
			return ret;
		}

		public void Quit ()
		{
			Log.Information ("Quit application");
			Gtk.Application.Quit ();
		}

		public HotKey SelectHotkey (HotKey hotkey, object parent = null)
		{
			HotKeySelectorDialog dialog;
			Window w;
			
			w = parent != null ? (parent as Widget).Toplevel as Window : mainWindow;
			dialog = new HotKeySelectorDialog (w);
			if (dialog.Run () == (int)ResponseType.Ok) {
				hotkey = dialog.HotKey;
			} else {
				hotkey = null;
			}
			dialog.Destroy ();
			return hotkey;
		}

		public void Invoke (EventHandler handler)
		{
			Gtk.Application.Invoke (handler);
		}
	}
}

