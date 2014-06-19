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
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using Mono.Unix;
using System.IO;
using LongoMatch.Stats;

namespace LongoMatch.Services
{

	public class EventsManager
	{
		/* Current play loaded. null if no play is loaded */
		TimeNode loadedPlay=null;
		/* current project in use */
		Project openedProject;
		ProjectType projectType;
		PlaysFilter filter;
		Dictionary<Category, Time> catsTime;
		
		IGUIToolkit guiToolkit;
		IAnalysisWindow analysisWindow;
		IPlayerBin player;
		ICapturerBin capturer;
		IRenderingJobsManager renderer;

		public EventsManager(IGUIToolkit guiToolkit, IRenderingJobsManager renderer)
		{
			this.guiToolkit = guiToolkit;
			this.renderer = renderer;
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			catsTime = new Dictionary<Category, Time>();
			ConnectSignals ();
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 PlaysFilter filter, IAnalysisWindow analysisWindow)
		{
			this.openedProject = project;
			this.projectType = projectType;
			this.filter = filter;
			catsTime.Clear ();
			
			if (project == null)
				return;
				
			if (player != null) {
				player.Prev -= OnPrev;
				player.SegmentClosedEvent -= OnSegmentClosedEvent;
				player.DrawFrame -= OnDrawFrame;
				player.PlaybackRateChanged -= HandlePlaybackRateChanged;
			}

			this.analysisWindow = analysisWindow;
			player = analysisWindow.Player;
			capturer = analysisWindow.Capturer;
			
			if (player != null) {
				player.Prev += OnPrev;
				player.SegmentClosedEvent += OnSegmentClosedEvent;
				player.DrawFrame += OnDrawFrame;
				player.PlaybackRateChanged += HandlePlaybackRateChanged;
			}

		}

		void Save (Project project) {
			if (Config.AutoSave) {
				Config.DatabaseManager.ActiveDB.UpdateProject (project);
			}
		}
		
		private void ConnectSignals() {
			Config.EventsBroker.NewTagEvent += OnNewTag;
			Config.EventsBroker.NewTagStartEvent += OnNewPlayStart;
			Config.EventsBroker.NewTagStopEvent += OnNewPlayStop;
			Config.EventsBroker.NewTagCancelEvent += OnNewPlayCancel;
			Config.EventsBroker.NewTagAtPosEvent += OnNewTagAtPos;
			Config.EventsBroker.TimeNodeChanged += OnTimeNodeChanged;
			Config.EventsBroker.PlaysDeleted += OnPlaysDeleted;
			Config.EventsBroker.PlaySelected += OnPlaySelected;
			Config.EventsBroker.PlayCategoryChanged += OnPlayCategoryChanged;
			Config.EventsBroker.DuplicatePlay += OnDuplicatePlay;
			Config.EventsBroker.PlayListNodeSelectedEvent += (tn) => {loadedPlay = tn;};
			Config.EventsBroker.SnapshotSeries += OnSnapshotSeries;
			
			Config.EventsBroker.ShowProjectStatsEvent += HandleShowProjectStatsEvent;
			Config.EventsBroker.TagSubcategoriesChangedEvent += HandleTagSubcategoriesChangedEvent;
		}

		void HandleTagSubcategoriesChangedEvent (bool tagsubcategories)
		{
			Config.FastTagging = !tagsubcategories;
		}

		void HandleShowProjectStatsEvent (Project project)
		{
			guiToolkit.ShowProjectStats (project);
		}

		void RenderPlay (Project project, Play play, MediaFile file) {
			PlayList playlist;
			EncodingSettings settings;
			EditionJob job;
			string outputDir, outputFile;
			
			if (Config.AutoRenderDir == null ||
			    !Directory.Exists (Config.AutoRenderDir)) {
				outputDir = Config.VideosDir;
			} else {
				outputDir = Config.AutoRenderDir;
			}
			
			outputFile = String.Format ("{0}-{0}.mp4", play.Category.Name, play.Name);
			outputFile = Path.Combine (outputDir, project.Description.Title, outputFile);
			try {
				Directory.CreateDirectory (Path.GetDirectoryName (outputFile));
				settings = EncodingSettings.DefaultRenderingSettings (outputFile);
				playlist = new PlayList();
				playlist.Add (new PlayListPlay (play, file, true));
			
				job = new EditionJob (playlist, settings);
				renderer.AddJob (job);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
			
		}
		
		void LoadPlay (Play play, Time seekTime, bool playing) {
			player.LoadPlay (openedProject.Description.File.FilePath, play,
			                 seekTime, playing);
			loadedPlay = play;
			if (playing) {
				player.Play ();
			}
		}
		
		private void ProcessNewTag(Category category,Time pos) {
			Time length, startTime, stopTime, start, stop, fStart, fStop;

			if(player == null || openedProject == null)
				return;

			/* Get the default lead and lag time for the category */
			startTime = category.Start;
			stopTime = category.Stop;
			/* Calculate boundaries of the segment */
			start = pos - startTime;
			stop = pos + stopTime;
			fStart = (start < new Time {MSeconds =0}) ? new Time {MSeconds = 0} : start;

			if(projectType == ProjectType.FakeCaptureProject ||
			   projectType == ProjectType.CaptureProject ||
			   projectType == ProjectType.URICaptureProject) {
				fStop = stop;
			} else {
				length = player.StreamLength;
				fStop = (stop > length) ? length: stop;
			}
			AddNewPlay(fStart, fStop, category);
		}

		private void AddNewPlay(Time start, Time stop, Category category) {
			Image miniature;

			Log.Debug(String.Format("New play created start:{0} stop:{1} category:{2}",
									start, stop, category));
			/* Get the current frame and get a thumbnail from it */
			if(projectType == ProjectType.CaptureProject || projectType == ProjectType.URICaptureProject) {
				if(!capturer.Capturing) {
					guiToolkit.InfoMessage(Catalog.GetString("You can't create a new play if the capturer "+
						"is not recording."));
					return;
				}
				miniature = capturer.CurrentMiniatureFrame;
			}
			else if(projectType == ProjectType.FileProject) {
				miniature = player.CurrentMiniatureFrame;
				player.Pause();
			}
			else
				miniature = null;
			
			/* Add the new created play to the project and update the GUI*/
			var play = openedProject.AddPlay(category, start, stop,miniature);
			/* Tag subcategories of the new play */
			if (!Config.FastTagging)
				guiToolkit.TagPlay (play, openedProject);
			analysisWindow.AddPlay(play);
			filter.Update();
			if (projectType == ProjectType.FileProject) {
				player.Play();
			}
			Save (openedProject);
			
			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
			    if (Config.AutoRenderPlaysInLive) {
					RenderPlay (openedProject, play, openedProject.Description.File);
				}
			}
		}

		void OnNewTagAtPos (Category category, Time pos) {
			if (openedProject == null)
				return;

			player.CloseSegment();
			player.Seek (pos, true);
			ProcessNewTag(category,pos);
		}

		public void OnNewTag(Category category) {
			Time pos;
			
			if (openedProject == null)
				return;

			if(projectType == ProjectType.FakeCaptureProject ||
			   projectType == ProjectType.CaptureProject ||
			   projectType == ProjectType.URICaptureProject) {
				pos =  capturer.CurrentTime;
			} else {
				pos = player.CurrentTime;
			}
			ProcessNewTag(category,pos);
		}

		void OnNewPlayStart (Category category) {
			Time startTime = player.CurrentTime;
			catsTime.Add (category, startTime);
			Log.Debug("New play start time: " + startTime);
		}
		
		void OnNewPlayStop(Category category) {
			int diff;
			Time startTime, stopTime;
			
			if (!catsTime.ContainsKey (category)) {
				Log.Error ("Can't add new play, no start time for this play");
				return;
			}
			startTime = catsTime[category];
			catsTime.Remove (category);
			stopTime = player.CurrentTime;

			Log.Debug("New play stop time: " + stopTime);
			diff = stopTime.MSeconds - startTime.MSeconds;

			if(diff < 0) {
				guiToolkit.WarningMessage(Catalog.GetString("The stop time is smaller than the start time. "+
					"The play will not be added."));
				return;
			}
			if(diff < 500) {
				int correction = 500 - diff;
				if(startTime.MSeconds - correction > 0)
					startTime = startTime - correction;
				else
					stopTime = stopTime + correction;
			}
			AddNewPlay(startTime, stopTime, category);
		}
		
		void OnNewPlayCancel (Category category) {
			try {
				catsTime.Remove (category);
			} catch {
			}
		}

		void HandlePlaybackRateChanged (float rate)
		{
			if (loadedPlay != null) {
				loadedPlay.Rate = rate;
			}
		}

		void OnPlaySelected(Play play)
		{
			LoadPlay (play, play.Start, true);
		}

		protected virtual void OnTimeNodeChanged(TimeNode tNode, object val)
		{
			/* FIXME: Tricky, create a new handler for categories */
			if(tNode is Play && val is Time) {
				LoadPlay (tNode as Play, val as Time, false);
			}
			else if(tNode is Category) {
				analysisWindow.UpdateCategories();
			}
			filter.Update();
		}

		protected virtual void OnPlaysDeleted(List<Play> plays)
		{
			Log.Debug(plays.Count + " plays deleted");
			analysisWindow.DeletePlays(plays);
			openedProject.RemovePlays(plays);

			if(projectType == ProjectType.FileProject) {
				player.CloseSegment ();
				Save (openedProject);
			}
			filter.Update();
		}

		void OnDuplicatePlay (Play play)
		{
			Play copy = Cloner.Clone (play);
			copy.ID = Guid.NewGuid();
			/* The category is also serialized and desarialized */
			copy.Category = play.Category;
			openedProject.AddPlay (copy);
			analysisWindow.AddPlay (copy);
			filter.Update();
		}

		protected virtual void OnSegmentClosedEvent()
		{
			loadedPlay = null;
		}

		protected virtual void OnSnapshotSeries(Play play) {
			player.Pause();
			guiToolkit.ExportFrameSeries(openedProject, play, Config.SnapshotsDir);
		}
		
		protected virtual void OnPrev()
		{
		}

		protected virtual void OnTimeline2PositionChanged(Time pos)
		{
			player.Seek (pos, false);
		}

		protected virtual void OnDrawFrame (Time time) {
			Image pixbuf = null;
			player.Pause();
			pixbuf = player.CurrentFrame;
			guiToolkit.DrawingTool (pixbuf, loadedPlay as Play, time);
		}

		protected virtual void OnPlayCategoryChanged(Play play, Category cat)
		{
			List<Play> plays = new List<Play>();
			plays.Add(play);
			OnPlaysDeleted(plays);
			var newplay = openedProject.AddPlay(cat, play.Start, play.Stop, play.Miniature);
			newplay.Name = play.Name;
			newplay.Notes = play.Notes;
			newplay.Drawings = play.Drawings;
			analysisWindow.AddPlay(newplay);
			Save (openedProject);
		}
		
	}
}
