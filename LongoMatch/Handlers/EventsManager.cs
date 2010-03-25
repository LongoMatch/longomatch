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
using LongoMatch.Common;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.TimeNodes;
using LongoMatch.DB;
using LongoMatch.Video.Player;
using LongoMatch.Video.Handlers;
using LongoMatch.Video.Utils;
using LongoMatch.Video.Editor;
using LongoMatch.Video;
using LongoMatch.Handlers;
using LongoMatch.Gui;
using Gtk;
using Gdk;
using Mono.Unix;

namespace LongoMatch
{


	public class EventsManager
	{

		private PlaysListTreeWidget treewidget;
		private PlayersListTreeWidget localPlayersList,visitorPlayersList;
		private TagsTreeWidget tagsTreeWidget;
		private ButtonsWidget buttonswidget;
		private PlayListWidget playlist;
		private PlayerBin player;
		private CapturerBin capturer;
		private TimeLineWidget timeline;
		private ProgressBar videoprogressbar;
		private NotesWidget notes;
		private FramesSeriesCapturer fsc;
		private FramesCaptureProgressDialog fcpd;
		private VideoDrawingsManager drawingManager;

		// Current play loaded. null if no play is loaded
		private TimeNode selectedTimeNode=null;
		// current proyect in use
		private Project openedProject;
		private ProjectType projectType;

		public EventsManager(PlaysListTreeWidget treewidget, PlayersListTreeWidget localPlayersList, 
		                     PlayersListTreeWidget visitorPlayersList, TagsTreeWidget tagsTreeWidget,
		                     ButtonsWidget buttonswidget, PlayListWidget playlist, PlayerBin player, 
		                     TimeLineWidget timeline, ProgressBar videoprogressbar,NotesWidget notes)
		{
			this.treewidget = treewidget;
			this.localPlayersList = localPlayersList;
			this.visitorPlayersList = visitorPlayersList;
			this.tagsTreeWidget = tagsTreeWidget;
			this.buttonswidget = buttonswidget;
			this.playlist = playlist;
			this.player = player;
			this.timeline = timeline;
			this.videoprogressbar = videoprogressbar;
			this.notes = notes;
			this.drawingManager = new VideoDrawingsManager(player);

			ConnectSignals();
		}

		public  Project OpenedProject {
			set {
				openedProject = value;
			}
		}
		
		public ProjectType OpenedProjectType{
			set {
				projectType = value;
			}
		}
		
		public CapturerBin Capturer{
			set {
				capturer = value;
			}
		}

		private void ConnectSignals() {
			//Adding Handlers for each event

			buttonswidget.NewMarkEvent += OnNewMark;

			treewidget.TimeNodeChanged += OnTimeNodeChanged;
			localPlayersList.TimeNodeChanged += OnTimeNodeChanged;
			visitorPlayersList.TimeNodeChanged += OnTimeNodeChanged;
			tagsTreeWidget.TimeNodeChanged += OnTimeNodeChanged;
			timeline.TimeNodeChanged += OnTimeNodeChanged;
			notes.TimeNodeChanged += OnTimeNodeChanged;

			treewidget.TimeNodeDeleted += OnTimeNodeDeleted;
			timeline.TimeNodeDeleted += OnTimeNodeDeleted;

			treewidget.TimeNodeSelected += OnTimeNodeSelected;
			localPlayersList.TimeNodeSelected += OnTimeNodeSelected;
			visitorPlayersList.TimeNodeSelected += OnTimeNodeSelected;
			tagsTreeWidget.TimeNodeSelected += OnTimeNodeSelected;
			timeline.TimeNodeSelected += OnTimeNodeSelected;

			playlist.PlayListNodeSelected += OnPlayListNodeSelected;
			playlist.Progress += OnProgress;
			playlist.ApplyCurrentRate += OnApplyRate;

			treewidget.PlayListNodeAdded += OnPlayListNodeAdded;
			localPlayersList.PlayListNodeAdded += OnPlayListNodeAdded;
			visitorPlayersList.PlayListNodeAdded += OnPlayListNodeAdded;
			tagsTreeWidget.PlayListNodeAdded += OnPlayListNodeAdded;

			treewidget.PlayersTagged += OnPlayersTagged;
			treewidget.TagPlay += OnTagPlay;

			treewidget.SnapshotSeriesEvent += OnSnapshotSeries;
			localPlayersList.SnapshotSeriesEvent += OnSnapshotSeries;
			visitorPlayersList.SnapshotSeriesEvent += OnSnapshotSeries;
			tagsTreeWidget.SnapshotSeriesEvent += OnSnapshotSeries;;

			timeline.NewMarkEvent += OnNewMarkAtFrame;

			player.Prev += OnPrev;
			player.Next += OnNext;
			player.Tick += OnTick;
			player.SegmentClosedEvent += OnSegmentClosedEvent;
			player.DrawFrame += OnDrawFrame;
		}

		private void ProcessNewMarkEvent(int section,Time pos) {
			Time length, startTime, stopTime, start, stop, fStart, fStop;	
			
			if (player == null || openedProject == null)
				return;
						
			//Get the default lead and lag time for the section
			startTime = openedProject.Sections.GetStartTime(section);
			stopTime = openedProject.Sections.GetStopTime(section);
			// Calculating borders of the segment depnding
			start = pos - startTime;
			stop = pos + stopTime;
			fStart = (start < new Time(0)) ? new Time(0) : start;
			
			if (projectType == ProjectType.NewFakeCaptureProject || 
			    projectType == ProjectType.NewCaptureProject){
				fStop = stop;					
			}
			else {
				length = new Time((int)player.StreamLength);
				fStop = (stop > length) ? length: stop;
			}	
			AddNewPlay(fStart, fStop, section);
		}
		
		private void AddNewPlay(Time start, Time stop, int section){
			Pixbuf miniature;
			MediaTimeNode tn;
		
			miniature = projectType == ProjectType.NewFakeCaptureProject ?
				null : player.CurrentMiniatureFrame;
			tn = openedProject.AddTimeNode(section,fStart, fStop,miniature);
			treewidget.AddPlay(tn,section);
			tagsTreeWidget.AddPlay(tn);
			timeline.QueueDraw();
		}

		protected virtual void OnProgress(float progress) {

			if (progress > (float)EditorState.START && progress <= (float)EditorState.FINISHED && progress > videoprogressbar.Fraction) {
				videoprogressbar.Fraction = progress;
			}

			if (progress == (float)EditorState.CANCELED) {
				videoprogressbar.Hide();
			}

			else if (progress == (float)EditorState.START) {
				videoprogressbar.Show();
				videoprogressbar.Fraction = 0;
				videoprogressbar.Text = "Creating new video";
			}

			else if (progress == (float)EditorState.FINISHED) {
				MessagePopup.PopupMessage(player, MessageType.Info,  Catalog.GetString("The video edition has finished successfully."));
				videoprogressbar.Hide();
			}

			else if (progress == (float)EditorState.ERROR) {
				MessagePopup.PopupMessage(player, MessageType.Error,
				                          Catalog.GetString("An error has occurred in the video editor.")
				                          +Catalog.GetString("Please, try again."));
				videoprogressbar.Hide();
			}
		}

		protected virtual void OnNewMarkAtFrame(int section, int frame) {

			Time pos = new Time(frame*1000/openedProject.File.Fps);
			ProcessNewMarkEvent(section,pos);
		}

		public virtual void OnNewMark(int i) {
			Time pos;
			
			if (projectType == ProjectType.NewFakeCaptureProject || 
			    projectType == ProjectType.NewCaptureProject)
				pos =  new Time((int)capturer.CurrentTime);
			else 
				pos = new Time((int)player.CurrentTime);
			ProcessNewMarkEvent(i,pos);
		}

		protected virtual void OnTimeNodeSelected(MediaTimeNode tNode)
		{
			selectedTimeNode = tNode;
			timeline.SelectedTimeNode = tNode;
			player.SetStartStop(tNode.Start.MSeconds,tNode.Stop.MSeconds);
			notes.Visible = true;
			notes.Play= tNode;
			drawingManager.Play=tNode;
		}

		protected virtual void OnTimeNodeChanged(TimeNode tNode, object val)
		{
			//Si hemos modificado el valor de un nodo de tiempo a través del
			//widget de ajuste de tiempo posicionamos el reproductor en el punto
			//
			if (tNode is MediaTimeNode && val is Time) {
				if (tNode != selectedTimeNode)
					OnTimeNodeSelected((MediaTimeNode)tNode);
				Time pos = (Time)val;
				if (pos == tNode.Start) {
					player.UpdateSegmentStartTime(pos.MSeconds);
				}
				else {
					player.UpdateSegmentStopTime(pos.MSeconds);
				}
			}
			else if (tNode is SectionsTimeNode) {
				buttonswidget.Sections = openedProject.Sections;
			}
		}

		protected virtual void OnTimeNodeDeleted(MediaTimeNode tNode,int section)
		{
			treewidget.DeletePlay(tNode,section);
			foreach (int player in tNode.LocalPlayers)
				localPlayersList.DeleteTimeNode(tNode,player);
			foreach (int player in tNode.VisitorPlayers)
				visitorPlayersList.DeleteTimeNode(tNode,player);
			openedProject.DeleteTimeNode(tNode,section);
			if (projectType == ProjectType.NewFileProject){
				this.player.CloseActualSegment();
				MainClass.DB.UpdateProject(openedProject);
			}
			timeline.QueueDraw();			
		}

		protected virtual void OnPlayListNodeAdded(MediaTimeNode tNode)
		{
			playlist.Add(new PlayListTimeNode(openedProject.File,tNode));
		}

		protected virtual void OnPlayListNodeSelected(PlayListTimeNode plNode, bool hasNext)
		{
			if (openedProject == null) {
				if (plNode.Valid) {
					player.SetPlayListElement(plNode.MediaFile.FilePath,plNode.Start.MSeconds,plNode.Stop.MSeconds,plNode.Rate,hasNext);
					selectedTimeNode = plNode;
				}
			}
			else {
				MessagePopup.PopupMessage(playlist, MessageType.Error,
				                          Catalog.GetString("Please, close the opened project to play the playlist."));
				playlist.Stop();
			}
		}

		protected virtual void OnPlayListSegmentDone()
		{
			playlist.Next();
		}

		protected virtual void OnSegmentClosedEvent()
		{
			selectedTimeNode = null;
			timeline.SelectedTimeNode = null;
			notes.Visible = false;
		}

		protected virtual void OnSnapshotSeries(MediaTimeNode tNode) {
			SnapshotsDialog sd;
			uint interval;
			string seriesName;
			string outDir;

			player.Pause();

			sd= new SnapshotsDialog();
			sd.TransientFor= (Gtk.Window) treewidget.Toplevel;
			sd.Play = tNode.Name;

			if (sd.Run() == (int)ResponseType.Ok) {
				sd.Destroy();
				interval = sd.Interval;
				seriesName = sd.SeriesName;
				outDir = System.IO.Path.Combine(MainClass.SnapshotsDir(),seriesName);
				fsc = new FramesSeriesCapturer(openedProject.File.FilePath,tNode.Start.MSeconds,tNode.Stop.MSeconds,interval,outDir);
				fcpd = new FramesCaptureProgressDialog(fsc);
				fcpd.TransientFor=(Gtk.Window) treewidget.Toplevel;
				fcpd.Run();
				fcpd.Destroy();
			}
			else
				sd.Destroy();
		}

		protected virtual void OnNext()
		{
			playlist.Next();
		}

		protected virtual void OnPrev()
		{
			if (selectedTimeNode is MediaTimeNode)
				player.SeekInSegment(selectedTimeNode.Start.MSeconds);
			else if (selectedTimeNode is PlayListTimeNode)
				playlist.Prev();
			else if (selectedTimeNode == null)
				player.SeekTo(0,false);
		}

		protected virtual void OnTick(object o, LongoMatch.Video.Handlers.TickArgs args)
		{
			if (args.CurrentTime != 0 && timeline != null && openedProject != null)
				timeline.CurrentFrame=(uint)(args.CurrentTime * openedProject.File.Fps / 1000);
		}

		protected virtual void OnTimeline2PositionChanged(Time pos)
		{
			player.SeekInSegment(pos.MSeconds);
		}

		protected virtual void OnApplyRate(PlayListTimeNode plNode) {
			plNode.Rate = player.Rate;
		}

		protected virtual void OnDrawFrame(int time) {
			Pixbuf pixbuf=null;
			DrawingTool dialog = new DrawingTool();

			player.SeekTo(time,true);
			while (pixbuf == null)
				pixbuf = player.CurrentFrame;

			dialog.Image = pixbuf;
			dialog.TransientFor = (Gtk.Window)player.Toplevel;
			if (selectedTimeNode != null)
				dialog.SetPlay((selectedTimeNode as MediaTimeNode),
				               time);
			player.Pause();
			pixbuf.Dispose();
			dialog.Run();
			dialog.Destroy();
		}
		
		protected virtual void OnTagPlay(MediaTimeNode tNode){
			TaggerDialog tagger = new TaggerDialog();
			tagger.ProjectTags = openedProject.Tags;
			tagger.Tags = tNode.Tags;
			tagger.TransientFor = (Gtk.Window)player.Toplevel;
			tagger.Run();
			tNode.Tags = tagger.Tags;
			foreach (Tag tag in tagger.Tags){
				openedProject.Tags.AddTag(tag);
			}
			tagsTreeWidget.UpdateTagsList();
			tagger.Destroy();
		}

		protected virtual void OnPlayersTagged(MediaTimeNode tNode, Team team) {
			PlayersSelectionDialog dialog = new PlayersSelectionDialog();
			if (team == Team.LOCAL) {
				dialog.SetPlayersInfo(openedProject.LocalTeamTemplate);
				dialog.PlayersChecked = tNode.LocalPlayers;
				if (dialog.Run() == (int) ResponseType.Ok) {
					tNode.LocalPlayers = dialog.PlayersChecked;
					localPlayersList.UpdatePlaysList(openedProject.GetLocalTeamModel());
				}
			}

			else if (team == Team.VISITOR) {
				dialog.SetPlayersInfo(openedProject.VisitorTeamTemplate);
				dialog.PlayersChecked = tNode.VisitorPlayers;
				if (dialog.Run() == (int) ResponseType.Ok) {
					tNode.VisitorPlayers = dialog.PlayersChecked;
					visitorPlayersList.UpdatePlaysList(openedProject.GetVisitorTeamModel());
				}
			}
			dialog.Destroy();
		}
	}
}
