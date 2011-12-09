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

namespace LongoMatch.Services
{


	public class EventsManager
	{

		private VideoDrawingsManager drawingManager;

		/* Current play loaded. null if no play is loaded */
		TimeNode selectedTimeNode=null;
		/* current project in use */
		Project openedProject;
		ProjectType projectType;
		Time startTime;
		
		IGUIToolkit guiToolkit;
		IMainWindow mainWindow;
		IPlayer player;
		ICapturer capturer;

		public EventsManager(IGUIToolkit guiToolkit)
		{
			this.guiToolkit = guiToolkit;
			mainWindow = guiToolkit.MainWindow;
			player = mainWindow.Player;
			capturer = mainWindow.Capturer;
			drawingManager = new VideoDrawingsManager(player);
			ConnectSignals();
		}

		public  Project OpenedProject {
			set {
				openedProject = value;
			}
		}

		public ProjectType OpenedProjectType {
			set {
				projectType = value;
			}
		}

		private void ConnectSignals() {
			/* Adding Handlers for each event */

			/* Connect tagging related events */
			mainWindow.NewTagEvent += OnNewTag;
			mainWindow.NewTagStartEvent += OnNewPlayStart;
			mainWindow.NewTagStopEvent += OnNewPlayStop;
			mainWindow.NewTagAtFrameEvent += OnNewTagAtFrame;
			mainWindow.TimeNodeChanged += OnTimeNodeChanged;
			mainWindow.PlaysDeletedEvent += OnPlaysDeleted;
			mainWindow.PlaySelectedEvent += OnPlaySelected;

			/* Connect playlist events */
			mainWindow.PlayListNodeSelectedEvent += (tn) => {selectedTimeNode = tn;};
			/* Connect tags events */
			mainWindow.TagPlayEvent += OnTagPlay;

			/* Connect SnapshotSeries events */
			mainWindow.SnapshotSeriesEvent += OnSnapshotSeries;
			
			/* Connect player events */
			player.Prev += OnPrev;
			player.SegmentClosedEvent += OnSegmentClosedEvent;
			player.DrawFrame += OnDrawFrame;
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
			   projectType == ProjectType.CaptureProject) {
				fStop = stop;
			} else {
				length = new Time {MSeconds = (int)player.StreamLength};
				fStop = (stop > length) ? length: stop;
			}
			AddNewPlay(fStart, fStop, category);
		}

		private void AddNewPlay(Time start, Time stop, Category category) {
			Image miniature;

			Log.Debug(String.Format("New play created start:{0} stop:{1} category:{2}",
									start, stop, category));
			/* Get the current frame and get a thumbnail from it */
			if(projectType == ProjectType.CaptureProject) {
				if(!capturer.Capturing) {
					guiToolkit.InfoMessage(Catalog.GetString("You can't create a new play if the capturer "+
						"is not recording."));
					return;
				}
				miniature = capturer.CurrentMiniatureFrame;
			}
			else if(projectType == ProjectType.FileProject)
				miniature = player.CurrentMiniatureFrame;
			else
				miniature = null;
			
			/* Add the new created play to the project and update the GUI*/
			var play = openedProject.AddPlay(category, start, stop,miniature);
			mainWindow.AddPlay(play);
			/* Tag subcategories of the new play */
			LaunchPlayTagger(play);
		}

		protected virtual void OnNewTagAtFrame(Category category, int frame) {
			Time pos = new Time { MSeconds = frame*1000/openedProject.Description.File.Fps};
			player.CloseActualSegment();
			player.SeekTo((long)pos.MSeconds, true);
			ProcessNewTag(category,pos);
		}

		public virtual void OnNewTag(Category category) {
			Time pos;

			if(projectType == ProjectType.FakeCaptureProject ||
			   projectType == ProjectType.CaptureProject) {
				pos =  new Time { MSeconds = (int)capturer.CurrentTime};
			} else {
				pos = new Time {MSeconds = (int)player.CurrentTime};
			}
			ProcessNewTag(category,pos);
		}

		public virtual void OnNewPlayStart() {
			startTime = new Time {MSeconds = (int)player.CurrentTime};
			Log.Debug("New play start time: " + startTime);
		}

		public virtual void OnNewPlayStop(Category category) {
			int diff;
			Time stopTime = new Time {MSeconds = (int)player.CurrentTime};

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

		private void LaunchPlayTagger(Play play) {
			guiToolkit.TagPlay(play, openedProject.LocalTeamTemplate, openedProject.VisitorTeamTemplate);
		}

		protected virtual void OnPlaySelected(Play play)
		{
			Log.Debug("Play selected: " + play);
			selectedTimeNode = play;
			player.SetStartStop(play.Start.MSeconds,play.Stop.MSeconds);
			drawingManager.Play=play;
			mainWindow.UpdateSelectedPlay(play);
		}

		protected virtual void OnTimeNodeChanged(TimeNode tNode, object val)
		{
			/* FIXME: Tricky, create a new handler for categories */
			if(tNode is Play && val is Time) {
				if(tNode != selectedTimeNode)
					OnPlaySelected((Play)tNode);
				Time pos = (Time)val;
				if(pos == tNode.Start) {
					player.UpdateSegmentStartTime(pos.MSeconds);
				}
				else {
					player.UpdateSegmentStopTime(pos.MSeconds);
				}
			}
			else if(tNode is Category) {
				mainWindow.UpdateCategories(openedProject.Categories);
			}
		}

		protected virtual void OnPlaysDeleted(List<Play> plays)
		{
			Log.Debug(plays.Count + " plays deleted");
			mainWindow.DeletePlays(plays);
			openedProject.RemovePlays(plays);

			if(projectType == ProjectType.FileProject) {
				player.CloseActualSegment();
				Core.DB.UpdateProject(openedProject);
			}
		}

		protected virtual void OnSegmentClosedEvent()
		{
			selectedTimeNode = null;
		}

		protected virtual void OnSnapshotSeries(Play play) {
			player.Pause();
			guiToolkit.ExportFrameSeries(openedProject, play, Config.SnapshotsDir());
		}
		
		protected virtual void OnPrev()
		{
			if(selectedTimeNode is Play)
				player.SeekInSegment(selectedTimeNode.Start.MSeconds);
			else if(selectedTimeNode == null)
				player.SeekTo(0,false);
		}

		protected virtual void OnTimeline2PositionChanged(Time pos)
		{
			player.SeekInSegment(pos.MSeconds);
		}

		protected virtual void OnDrawFrame(int time) {
			Image pixbuf = null;
			player.Pause();
			pixbuf = player.CurrentFrame;
			guiToolkit.DrawingTool(pixbuf, selectedTimeNode as Play, time);
		}

		protected virtual void OnTagPlay(Play play) {
			LaunchPlayTagger(play);
		}
	}
}
