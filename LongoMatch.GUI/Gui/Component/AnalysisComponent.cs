//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using LongoMatch.Interfaces.GUI;
using LongoMatch.Handlers;
using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Interfaces;
using LongoMatch.Store.Templates;
using System.Collections.Generic;
using Gdk;
using Gtk;
using LongoMatch.Gui.Helpers;
using Mono.Unix;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnalysisComponent : Gtk.Bin, IAnalysisWindow
	{
		/* Error handling */
		public event CloseOpenendProjectHandler CloseOpenedProjectEvent;
		
		/* Tags */
		public event NewTagHandler NewTagEvent;
		public event NewTagStartHandler NewTagStartEvent;
		public event NewTagStopHandler NewTagStopEvent;
		public event NewTagCancelHandler NewTagCancelEvent;
		public event PlaySelectedHandler PlaySelectedEvent;
		public event NewTagAtPosHandler NewTagAtPosEvent;
		public event TagPlayHandler TagPlayEvent;
		public event PlaysDeletedHandler PlaysDeletedEvent;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlayCategoryChangedHandler PlayCategoryChanged;
		public event DuplicatePlayHandler DuplicatePlay;
		
		/* Playlist */
		public event RenderPlaylistHandler RenderPlaylistEvent;
		public event PlayListNodeAddedHandler PlayListNodeAddedEvent;
		public event PlayListNodeSelectedHandler PlayListNodeSelectedEvent;
		public event OpenPlaylistHandler OpenPlaylistEvent;
		public event NewPlaylistHandler NewPlaylistEvent;
		public event SavePlaylistHandler SavePlaylistEvent; 
		
		/* Snapshots */
		public event SnapshotSeriesHandler SnapshotSeriesEvent;
		
		/* Game Units events */
		public event GameUnitHandler GameUnitEvent;
		public event UnitChangedHandler UnitChanged;
		public event UnitSelectedHandler UnitSelected;
		public event UnitsDeletedHandler UnitDeleted;
		public event UnitAddedHandler UnitAdded;
		
		public event KeyHandler KeyPressed;

		static Project openedProject;
		ProjectType projectType;
		bool detachedPlayer;
		Gtk.Window playerWindow;
		VideoAnalysisMode analysisMode;
		
		public AnalysisComponent ()
		{
			this.Build ();
			projectType = ProjectType.None;
			playsSelection.Visible = true;

			playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Player;
			playercapturer.Tick += OnTick;
			playercapturer.Detach += DetachPlayer;
			playercapturer.Logo = System.IO.Path.Combine(Config.ImagesDir,"background.png");
			playercapturer.CaptureFinished += (sender, e) => {
				EmitCloseOpenedProject();
			};
			
			ConnectSignals();
			AnalysisMode = VideoAnalysisMode.PredefinedTagging;
			
			postagger.SetMode (false);
		}
		
		public IPlayerBin Player{
			get {
				return playercapturer;
			}
		}
		
		public ICapturerBin Capturer{
			get {
				return playercapturer;
			}
		}
		
		public IPlaylistWidget Playlist{
			get {
				return null;
			}
		}
		
		public bool Fullscreen {
			set {
				playercapturer.FullScreen = value;
			}
		}
		
		public VideoAnalysisMode AnalysisMode {
			set {
				codingwidget.AnalysisMode = value;
			}
		}
		
		public void AddPlay(Play play) {
			playsSelection.AddPlay(play);
			codingwidget.AddPlay (play);
		}
		
		public void UpdateSelectedPlay (Play play) {
			codingwidget.SelectedPlay = play;
			postagger.LoadPlay (play, false);
			notes.Play= play;
		}

		public void UpdateCategories () {
			codingwidget.UpdateCategories ();
		}
		
		public void DeletePlays (List<Play> plays) {
			playsSelection.RemovePlays(plays);
			codingwidget.DeletePlays (plays);
		}
		
		private void ConnectSignals() {
			/* Adding Handlers for each event */

			notes.TimeNodeChanged += EmitTimeNodeChanged;

			playsSelection.PlaysDeleted += EmitPlaysDeleted;
			playsSelection.PlaySelected += EmitPlaySelected;
			playsSelection.TimeNodeChanged += EmitTimeNodeChanged;
			playsSelection.PlayCategoryChanged += EmitPlayCategoryChanged;
			playsSelection.PlayListNodeAdded += EmitPlayListNodeAdded;
			playsSelection.DuplicatePlay += EmitDuplicatePlay;
			playsSelection.TagPlay += EmitTagPlay;
			playsSelection.SnapshotSeries += EmitSnapshotSeries;
			playsSelection.RenderPlaylist += EmitRenderPlaylist;

			/* Connect playlist events */
//			playlist.PlayListNodeSelected += EmitPlayListNodeSelected;
//			playlist.NewPlaylistEvent += EmitNewPlaylist;
//			playlist.OpenPlaylistEvent += EmitOpenPlaylist;
//			playlist.SavePlaylistEvent += EmitSavePlaylist;

			playercapturer.Error += OnMultimediaError;
			playercapturer.SegmentClosedEvent += OnSegmentClosedEvent;
			
			KeyPressEvent += (o, args) => (EmitKeyPressed(o, (int)args.Event.Key, (int)args.Event.State));
 		}
 		
		void DetachPlayer (bool detach) {
			if (detach == detachedPlayer)
				return;
				
			detachedPlayer = detach;
			
			if (detach) {
				EventBox box;
				Log.Debug("Detaching player");
				
				playerWindow = new Gtk.Window(Constants.SOFTWARE_NAME);
				playerWindow.Icon = Stetic.IconLoader.LoadIcon(this, "longomatch", IconSize.Button);
				playerWindow.DeleteEvent += (o, args) => DetachPlayer(false);
				box = new EventBox();
				
				box.KeyPressEvent += (o, args) => OnKeyPressEvent(args.Event);
				playerWindow.Add(box);
				
				box.Show();
				playerWindow.Show();
				
				playercapturer.Reparent(box);
				videowidgetsbox.Visible = false;
			} else {
				Log.Debug("Attaching player again");
				videowidgetsbox.Visible = true;
				playercapturer.Reparent(this.videowidgetsbox);
				playerWindow.Destroy();
				
				AnalysisMode = analysisMode;
			}
			playercapturer.Detached = detach;
		}
		
		public void CloseOpenedProject () {
			openedProject = null;
			projectType = ProjectType.None;
			ResetGUI ();
			return;
		}
		
		public void SetProject(Project project, ProjectType projectType, CaptureSettings props, PlaysFilter filter)
		{
			bool isLive = false;
			
			if(projectType == ProjectType.FileProject) {
				playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Player;
			} else {
				isLive = true;
				if(projectType == ProjectType.FakeCaptureProject) {
					playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Capturer;
				} else {
					playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.PreviewCapturer;
				}
			}
			
// FIXME
//			if(projectType == ProjectType.FakeCaptureProject) {
//#if OS_TYPE_LINUX
//				/* This deadlocks in Windows and OS X */
//				(downbox[videowidgetsbox] as Box.BoxChild).Expand = false;
//#endif
//				(downbox[buttonswidget] as Box.BoxChild).Expand = true;
//			}
			
			openedProject = project;
			this.projectType = projectType;
			
			codingwidget.SetProject (project, isLive, filter, this);
			playsSelection.SetProject (project, isLive, filter);
			postagger.LoadBackgrounds (openedProject.Categories.FieldBackground,
			                           openedProject.Categories.HalfFieldBackground,
			                           openedProject.Categories.GoalBackground);
		}
		
		public void EmitNewTagAtPos(Category category, Time pos) {
			if (NewTagAtPosEvent != null)
				NewTagAtPosEvent(category, pos);
		}

		public void EmitNewTag(Category category) {
			if (NewTagEvent != null)
				NewTagEvent(category);
		}

		public void EmitNewTagStart(Category category) {
			if (NewTagStartEvent != null)
				NewTagStartEvent (category);
		}

		public void EmitNewTagStop(Category category) {
			if (NewTagStopEvent != null)
				NewTagStopEvent (category);
		}
		
		public void EmitNewTagCancel(Category category) {
			if (NewTagCancelEvent != null)
				NewTagCancelEvent (category);
		}
		
		public void EmitTimeNodeChanged(TimeNode tNode, object val)
		{
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode, val);
		}

		public void EmitPlayListNodeAdded(List<Play> plays)
		{
			if (PlayListNodeAddedEvent != null)
				PlayListNodeAddedEvent(plays);
		}
		
		public void EmitTagPlay(Play play) {
			if (TagPlayEvent != null)
				TagPlayEvent (play);
		}
		
		public void EmitSnapshotSeries(Play play) {
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent (play);
		}

		public void EmitRenderPlaylist(IPlayList playlist) {
			if (RenderPlaylistEvent != null)
				RenderPlaylistEvent(playlist);
		}
		
		public void EmitPlaySelected(Play play)
		{
			if (PlaySelectedEvent != null)
				PlaySelectedEvent(play);
		}

		public void EmitPlaysDeleted(List<Play> plays)
		{
			if (PlaysDeletedEvent != null)
				PlaysDeletedEvent(plays);
		}
		
		
		void ResetGUI() {
			playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Player;
			ClearWidgets();
			if (detachedPlayer)
				DetachPlayer(false);
		}
		
		void ClearWidgets() {
			playsSelection.Clear();
		}
		
		protected override bool OnKeyPressEvent(EventKey evnt)
		{
			Gdk.Key key = evnt.Key;
			Gdk.ModifierType modifier = evnt.State;
			bool ret;

			ret = base.OnKeyPressEvent(evnt);

			if(openedProject == null && !playercapturer.Opened)
				return ret;

			if(projectType != ProjectType.CaptureProject &&
			   projectType != ProjectType.URICaptureProject &&
			   projectType != ProjectType.FakeCaptureProject) {
				switch(key) {
				case Constants.SEEK_FORWARD:
					if(modifier == Constants.STEP)
						playercapturer.StepForward();
					else
						playercapturer.SeekToNextFrame();
					break;
				case Constants.SEEK_BACKWARD:
					if(modifier == Constants.STEP)
						playercapturer.StepBackward();
					else
						playercapturer.SeekToPreviousFrame();
					break;
				case Constants.FRAMERATE_UP:
					playercapturer.FramerateUp();
					break;
				case Constants.FRAMERATE_DOWN:
					playercapturer.FramerateDown();
					break;
				case Constants.TOGGLE_PLAY:
					playercapturer.TogglePlay();
					break;
				}
			} else {
				switch(key) {
				case Constants.TOGGLE_PLAY:
					playercapturer.TogglePause();
					break;
				}
			}
			return ret;
		}

		void OnSegmentClosedEvent()
		{
			codingwidget.SelectedPlay = null;
		}
		
		void OnTick (Time currentTime, Time streamLength,
			double currentPosition)
		{
			if (currentTime.MSeconds != 0 && codingwidget != null && openedProject != null) {
				codingwidget.CurrentTime = currentTime;
			}
		}
		
		void OnMultimediaError(string message)
		{
			MessagesHelpers.ErrorMessage (this,
				Catalog.GetString("The following error happened and" +
				" the current project will be closed:")+"\n" + message);
			EmitCloseOpenedProject ();
		}
		
		void EmitCloseOpenedProject () {
			if (CloseOpenedProjectEvent != null)
				CloseOpenedProjectEvent ();
		}
		
		void EmitPlayCategoryChanged(Play play, Category cat)
		{
			if(PlayCategoryChanged != null)
				PlayCategoryChanged(play, cat);
		}

		void EmitPlayListNodeSelected(PlayListPlay plNode)
		{
			if (PlayListNodeSelectedEvent != null)
				PlayListNodeSelectedEvent(plNode);
		}

		void EmitNewPlaylist() {
			if (NewPlaylistEvent != null)
				NewPlaylistEvent();
		}
		
		void EmitOpenPlaylist() {
			if (OpenPlaylistEvent != null)
				OpenPlaylistEvent();
		}
		
		void EmitSavePlaylist() {
			if (SavePlaylistEvent != null)
				SavePlaylistEvent();
		}
		
		void EmitKeyPressed(object sender, int key, int modifier) {
			if (KeyPressed != null)
				KeyPressed(sender, key, modifier);
		}

		void EmitDuplicatePlay (Play play)
		{
			if (DuplicatePlay != null)
				DuplicatePlay (play);
		}
	}
}

