// EventsManager.cs
//
//  Copyright (C2007-2009 Andoni Morales Alastruey
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
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using Mono.Unix;
using System.IO;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Store.Playlists;

namespace LongoMatch.Services
{
	public class EventsManager
	{
		/* Current play loaded. null if no play is loaded */
		Play loadedPlay = null;
		/* current project in use */
		Project openedProject;
		ProjectType projectType;
		PlaysFilter filter;
		IGUIToolkit guiToolkit;
		IAnalysisWindow analysisWindow;
		IPlayerBin player;
		ICapturerBin capturer;
		IFramesCapturer framesCapturer;
		IRenderingJobsManager renderer;

		public EventsManager (IGUIToolkit guiToolkit, IRenderingJobsManager renderer)
		{
			this.guiToolkit = guiToolkit;
			this.renderer = renderer;
			framesCapturer = Config.MultimediaToolkit.GetFramesCapturer ();
			ConnectSignals ();
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 PlaysFilter filter, IAnalysisWindow analysisWindow)
		{
			this.openedProject = project;
			this.projectType = projectType;
			this.filter = filter;
			
			if (project == null)
				return;
				
			if (projectType == ProjectType.FileProject) {
				framesCapturer.Open (openedProject.Description.File.FilePath);
			}
			this.analysisWindow = analysisWindow;
			player = analysisWindow.Player;
			capturer = analysisWindow.Capturer;
		}

		void Save (Project project)
		{
			if (Config.AutoSave) {
				Config.DatabaseManager.ActiveDB.UpdateProject (project);
			}
		}

		private void ConnectSignals ()
		{
			Config.EventsBroker.NewTagEvent += OnNewTag;
			Config.EventsBroker.NewPlayEvent += HandleNewPlay;
			Config.EventsBroker.PlaysDeleted += OnPlaysDeleted;
			Config.EventsBroker.PlayCategoryChanged += OnPlayCategoryChanged;
			Config.EventsBroker.DuplicatePlays += OnDuplicatePlays;
			Config.EventsBroker.SnapshotSeries += OnSnapshotSeries;
			Config.EventsBroker.PlayLoadedEvent += HandlePlayLoaded;
			
			Config.EventsBroker.ShowProjectStatsEvent += HandleShowProjectStatsEvent;
			Config.EventsBroker.TagSubcategoriesChangedEvent += HandleTagSubcategoriesChangedEvent;
			
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;

			Config.EventsBroker.DrawFrame += HandleDrawFrame;
			Config.EventsBroker.Detach += HandleDetach;
			
			Config.EventsBroker.ShowFullScreenEvent += HandleShowFullScreenEvent;
		}

		void HandleShowFullScreenEvent (bool fullscreen)
		{
			guiToolkit.FullScreen = fullscreen;
		}

		void HandlePlayLoaded (Play play)
		{
			loadedPlay = play;
		}

		void HandleDetach ()
		{
			analysisWindow.DetachPlayer ();
		}

		void HandleTagSubcategoriesChangedEvent (bool tagsubcategories)
		{
			Config.FastTagging = !tagsubcategories;
		}

		void HandleShowProjectStatsEvent (Project project)
		{
			guiToolkit.ShowProjectStats (project);
		}

		void HandleDrawFrame (Play play, int drawingIndex)
		{
			Image pixbuf;
			FrameDrawing drawing = null;

			player.Pause ();
			if (play == null) {
				play = loadedPlay as Play;
			}
			if (play != null) {
				if (drawingIndex == -1) {
					drawing = new FrameDrawing ();
					drawing.Render = player.CurrentTime;
				} else {
					drawing = play.Drawings [drawingIndex];
				}
				player.Seek (drawing.Render, true);
			}
			pixbuf = player.CurrentFrame;
			guiToolkit.DrawingTool (pixbuf, play, drawing);
		}

		void RenderPlay (Project project, Play play, MediaFile file)
		{
			Playlist playlist;
			EncodingSettings settings;
			EditionJob job;
			string outputDir, outputFile;
			
			if (Config.AutoRenderDir == null ||
				!Directory.Exists (Config.AutoRenderDir)) {
				outputDir = Config.VideosDir;
			} else {
				outputDir = Config.AutoRenderDir;
			}
			
			outputFile = String.Format ("{0}-{1}.mp4", play.Category.Name, play.Name);
			outputFile = Path.Combine (outputDir, project.Description.Title, outputFile);
			try {
				PlaylistPlayElement element;
				
				Directory.CreateDirectory (Path.GetDirectoryName (outputFile));
				settings = EncodingSettings.DefaultRenderingSettings (outputFile);
				playlist = new Playlist ();
				element = new PlaylistPlayElement (play);
				element.File = file;
				playlist.Elements.Add (element);
				job = new EditionJob (playlist, settings);
				renderer.AddJob (job);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
			
		}

		private Image CaptureFrame (Time tagtime)
		{
			Image frame = null;

			/* Get the current frame and get a thumbnail from it */
			if (projectType == ProjectType.CaptureProject ||
				projectType == ProjectType.URICaptureProject) {
				frame = capturer.CurrentMiniatureFrame;
			} else if (projectType == ProjectType.FileProject) {
				framesCapturer.Seek (tagtime, true);
				frame = player.CurrentMiniatureFrame;
			}
			return frame;
		}

		private void AddNewPlay (Play play)
		{
			/* Clip play boundaries */
			play.Start.MSeconds = Math.Max (0, play.Start.MSeconds);
			if (projectType == ProjectType.FileProject) {
				play.Stop.MSeconds = Math.Min (player.StreamLength.MSeconds, play.Stop.MSeconds);
			}

			analysisWindow.AddPlay (play);
			filter.Update ();
			if (projectType == ProjectType.FileProject) {
				player.Play ();
			}
			Save (openedProject);
			
			if (projectType == ProjectType.CaptureProject ||
				projectType == ProjectType.URICaptureProject) {
				if (Config.AutoRenderPlaysInLive) {
					RenderPlay (openedProject, play, openedProject.Description.File);
				}
			}
		}

		public void OnNewTag (TaggerButton tagger, List<Player> players, List<Tag> tags,
		                      Time start, Time stop)
		{
			Image frame;

			if (player == null || openedProject == null || !(tagger is AnalysisCategory))
				return;
			
			if (projectType == ProjectType.CaptureProject ||
				projectType == ProjectType.URICaptureProject) {
				if (!capturer.Capturing) {
					guiToolkit.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}
			frame = CaptureFrame (start);
			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
			                          start.ToMSecondsString(), stop.ToMSecondsString(),
			                          tagger.Name));
			/* Add the new created play to the project and update the GUI*/
			var play = openedProject.AddPlay (tagger as AnalysisCategory, start, stop, frame);
			if (players != null) {
				play.Players = players;
			}
			if (tags != null) {
				play.Tags = tags;
			}
			AddNewPlay (play);
		}

		public void HandleNewPlay (Play play)
		{
			if (player == null || openedProject == null)
				return;
			
			if (projectType == ProjectType.CaptureProject ||
				projectType == ProjectType.URICaptureProject) {
				if (!capturer.Capturing) {
					guiToolkit.WarningMessage (Catalog.GetString ("Video capture is stopped"));
					return;
				}
			}
			play.Miniature = CaptureFrame (play.Start);
			Log.Debug (String.Format ("New play created start:{0} stop:{1} category:{2}",
			                          play.Start.ToMSecondsString(), play.Stop.ToMSecondsString(),
			                          play.Category.Name));
			openedProject.Timeline.Add (play);
			AddNewPlay (play);
		}

		protected virtual void OnPlaysDeleted (List<Play> plays)
		{
			Log.Debug (plays.Count + " plays deleted");
			analysisWindow.DeletePlays (plays);
			openedProject.RemovePlays (plays);

			if (projectType == ProjectType.FileProject) {
				player.CloseSegment ();
				Save (openedProject);
			}
			filter.Update ();
		}

		void OnDuplicatePlays (List<Play> plays)
		{
			foreach (Play play in plays) {
				Play copy = Cloner.Clone (play);
				copy.ID = Guid.NewGuid ();
				/* The category is also serialized and desarialized */
				copy.Category = play.Category;
				openedProject.AddPlay (copy);
				analysisWindow.AddPlay (copy);
			}
			filter.Update ();
		}

		protected virtual void OnSnapshotSeries (Play play)
		{
			player.Pause ();
			guiToolkit.ExportFrameSeries (openedProject, play, Config.SnapshotsDir);
		}

		protected virtual void OnPlayCategoryChanged (Play play, Category cat)
		{
			List<Play> plays = new List<Play> ();
			plays.Add (play);
			OnPlaysDeleted (plays);
			var newplay = openedProject.AddPlay (cat, play.Start, play.Stop, play.Miniature);
			newplay.Name = play.Name;
			newplay.Notes = play.Notes;
			newplay.Drawings = play.Drawings;
			analysisWindow.AddPlay (newplay);
			Save (openedProject);
		}
	}
}
