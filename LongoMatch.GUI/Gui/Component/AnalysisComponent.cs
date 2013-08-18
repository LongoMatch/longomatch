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
using Mono.Posix;

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
		public event NewTagAtFrameHandler NewTagAtFrameEvent;
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
		TimeNode selectedTimeNode;		
		TimeLineWidget timeline;
		bool gameUnitsActionVisible, detachedPlayer;
		GameUnitsTimelineWidget guTimeline;
		IGUIToolkit guiToolKit;
		Gtk.Window playerWindow;
		VideoAnalysisMode analysisMode;
		
		public AnalysisComponent ()
		{
			this.Build ();
			projectType = ProjectType.None;

			timeline = new TimeLineWidget();
			downbox.PackStart(timeline, true, true, 0);
			
			guTimeline = new GameUnitsTimelineWidget ();
			downbox.PackStart(guTimeline, true, true, 0);
			
			playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Player;
			playercapturer.SetLogo(System.IO.Path.Combine(Config.ImagesDir,"background.png"));
			playercapturer.LogoMode = true;
			playercapturer.Tick += OnTick;
			playercapturer.Detach += DetachPlayer;
			playercapturer.Logo = System.IO.Path.Combine(Config.ImagesDir,"background.png");
			playercapturer.CaptureFinished += (sender, e) => {
				EmitCloseOpenedProject();
				(downbox[videowidgetsbox] as Box.BoxChild).Expand = true;
				(downbox[buttonswidget] as Box.BoxChild).Expand = false;
			};
			
			buttonswidget.Mode = TagMode.Predifined;
			ConnectSignals();
			AnalysisMode = VideoAnalysisMode.PredefinedTagging;
			
			postagger.SetMode (false);
		}
		
		public IPlayer Player{
			get {
				return playercapturer;
			}
		}
		
		public ICapturer Capturer{
			get {
				return playercapturer;
			}
		}
		
		public IPlaylistWidget Playlist{
			get {
				return playlist;
			}
		}
		
		public ITemplatesService TemplatesService {
			set {
				playsSelection.TemplatesService = value;
			}
		}
		
		public bool Fullscreen {
			set {
				playercapturer.FullScreen = value;
			}
		}
		
		public bool WidgetsVisible {
			set {
				if(openedProject == null)
					return;
			
				leftbox.Visible = value;
				timeline.Visible = value && AnalysisMode == VideoAnalysisMode.Timeline;
				buttonswidget.Visible = value && (AnalysisMode == VideoAnalysisMode.ManualTagging ||
				                                  AnalysisMode == VideoAnalysisMode.PredefinedTagging);
				if (Config.UseGameUnits) {
					guTimeline.Visible = value && AnalysisMode == VideoAnalysisMode.GameUnits;
					gameunitstaggerwidget1.Visible = value && (
						AnalysisMode == VideoAnalysisMode.GameUnits ||
						AnalysisMode == VideoAnalysisMode.PredefinedTagging ||
						AnalysisMode == VideoAnalysisMode.ManualTagging);
				}
				if(value) {
					SetTagsBoxVisibility (false);
				} else {
					if (selectedTimeNode != null)
						SetTagsBoxVisibility (true);
				}
			}
		}
		
		public bool PlaylistVisible {
			set {
				SetPlaylistVisibility (value);
				playsSelection.PlayListLoaded = value;
			}
		}
		
		public VideoAnalysisMode AnalysisMode {
			set {
				buttonswidget.Visible = (value == VideoAnalysisMode.ManualTagging) ||
					(value == VideoAnalysisMode.PredefinedTagging);
				timeline.Visible = value == VideoAnalysisMode.Timeline;
				if (Config.UseGameUnits) {
					guTimeline.Visible = value == VideoAnalysisMode.GameUnits;
					gameunitstaggerwidget1.Visible = buttonswidget.Visible || guTimeline.Visible;
				}
				if(value == VideoAnalysisMode.ManualTagging)
					buttonswidget.Mode = TagMode.Free;
				else if (value == VideoAnalysisMode.ManualTagging)
					buttonswidget.Mode = TagMode.Predifined;
				analysisMode = value;
				
			}
			protected get {
				return analysisMode;
			}
		}
		
		public void AddPlay(Play play) {
			playsSelection.AddPlay(play);
			timeline.AddPlay(play);
			timeline.QueueDraw();
		}
		
		public void UpdateSelectedPlay (Play play) {
			selectedTimeNode = play;
			timeline.SelectedTimeNode = play;
			postagger.LoadPlay (play, false);
			SetTagsBoxVisibility (true);
			notes.Play= play;
		}

		public void UpdateCategories (Categories categories) {
			buttonswidget.Categories = openedProject.Categories;
		}
		
		public void DeletePlays (List<Play> plays) {
			playsSelection.RemovePlays(plays);
			timeline.RemovePlays(plays);
			timeline.QueueDraw();
		}
		
		public void UpdateGameUnits (GameUnitsList gameUnits) {
			gameUnitsActionVisible = gameUnits != null && gameUnits.Count > 0;
			//GameUnitsViewAction.Sensitive = gameUnitsActionVisible;
			if (gameUnits == null) {
				gameunitstaggerwidget1.Visible = false;
				return;
			}
			gameunitstaggerwidget1.Visible = true;
			gameunitstaggerwidget1.GameUnits = gameUnits;
		}
		
		private void ConnectSignals() {
			/* Adding Handlers for each event */

			/* Connect new mark event */
			buttonswidget.NewMarkEvent += EmitNewTag;;
			buttonswidget.NewMarkStartEvent += EmitNewTagStart;
			
			buttonswidget.NewMarkStopEvent += EmitNewTagStop;
			buttonswidget.NewMarkCancelEvent += EmitNewTagCancel;
			timeline.NewMarkEvent += EmitNewTagAtFrame;

			/* Connect TimeNodeChanged events */
			timeline.TimeNodeChanged += EmitTimeNodeChanged;
			notes.TimeNodeChanged += EmitTimeNodeChanged;

			/* Connect TimeNodeDeleted events */
			playsSelection.PlaysDeleted += EmitPlaysDeleted;
			timeline.TimeNodeDeleted += EmitPlaysDeleted;

			/* Connect TimeNodeSelected events */
			playsSelection.PlaySelected += OnTimeNodeSelected;
			timeline.TimeNodeSelected += OnTimeNodeSelected;
			
			/* Connect TimeNodeChangedEvent */
			playsSelection.TimeNodeChanged += EmitTimeNodeChanged;

			/* Connect PlayCategoryChanged events */
			playsSelection.PlayCategoryChanged += EmitPlayCategoryChanged;

			/* Connect playlist events */
			playlist.PlayListNodeSelected += EmitPlayListNodeSelected;
			playlist.NewPlaylistEvent += EmitNewPlaylist;
			playlist.OpenPlaylistEvent += EmitOpenPlaylist;
			playlist.SavePlaylistEvent += EmitSavePlaylist;

			/* Connect PlayListNodeAdded events */
			playsSelection.PlayListNodeAdded += OnPlayListNodeAdded;
			timeline.PlayListNodeAdded += OnPlayListNodeAdded;

			/* Connect duplicate plays */
			playsSelection.DuplicatePlay += EmitDuplicatePlay;

			/* Connect tags events */
			playsSelection.TagPlay += EmitTagPlay;
			timeline.TagPlay += EmitTagPlay;

			/* Connect SnapshotSeries events */
			playsSelection.SnapshotSeries += EmitSnapshotSeries;
			timeline.SnapshotSeries += EmitSnapshotSeries;

			playlist.RenderPlaylistEvent += EmitRenderPlaylist;
			playsSelection.RenderPlaylist += EmitRenderPlaylist;
			timeline.RenderPlaylist += EmitRenderPlaylist;
			
			/* Game Units event */
			gameunitstaggerwidget1.GameUnitEvent += EmitGameUnitEvent;
			guTimeline.UnitAdded += EmitUnitAdded;;
			guTimeline.UnitDeleted += EmitUnitDeleted;
			guTimeline.UnitSelected += EmitUnitSelected;
			guTimeline.UnitChanged += EmitUnitChanged;
			
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
				if (openedProject != null) {
					buttonswidget.Visible = true;
					timeline.Visible = true;
					if (Config.UseGameUnits) {
						guTimeline.Visible = true;
						gameunitstaggerwidget1.Visible = true;
					}
				}
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
			
			/* Update tabs labels */
			var desc = project.Description;
			
			if(projectType == ProjectType.FileProject) {
				playercapturer.LogoMode = false;
				timeline.SetProject (project, filter);
				guTimeline.Project = project;

			} else {
				isLive = true;
				if(projectType == ProjectType.FakeCaptureProject) {
					playercapturer.Type = CapturerType.Fake;
					playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Capturer;
				} else {
					playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.PreviewCapturer;
				}
			}
			
			if(projectType == ProjectType.FakeCaptureProject) {
#if OS_TYPE_LINUX
				/* This deadlocks in Windows and OS X */
				(downbox[videowidgetsbox] as Box.BoxChild).Expand = false;
#endif
				(downbox[buttonswidget] as Box.BoxChild).Expand = true;
			}
			
			openedProject = project;
			this.projectType = projectType;
			
			playsSelection.SetProject(project, isLive, filter);
			buttonswidget.Categories = project.Categories;
			ShowWidgets ();
			postagger.LoadBackgrounds (openedProject.Categories.FieldBackground,
			                           openedProject.Categories.HalfFieldBackground,
			                           openedProject.Categories.GoalBackground);
			ShowWidgets();
		}
		
		void SetPlaylistVisibility (bool visible) {
			if (visible) {
				righthbox.Visible = true;
				playlist.Visible = true;
			} else {
				playlist.Visible = false;
				if (!tagsvbox.Visible)
					righthbox.Visible = false;
			}
		}
		
		void SetTagsBoxVisibility (bool visible) {
			if (visible) {
				righthbox.Visible = true;
				tagsvbox.Visible = true;
			} else {
				tagsvbox.Visible = false;
				if (!playlist.Visible)
					righthbox.Visible = false;
			}
		}
		
		private void ResetGUI() {
			bool playlistVisible = playlist.Visible;
			playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Player;
			playercapturer.LogoMode = true;
			ClearWidgets();
			HideWidgets();
			SetPlaylistVisibility (playlistVisible);
			SetTagsBoxVisibility (false);
			selectedTimeNode = null;
			if (detachedPlayer)
				DetachPlayer(false);
		}
		
		private void ShowWidgets() {
			leftbox.Show();
			if(analysisMode == VideoAnalysisMode.ManualTagging ||
			   analysisMode == VideoAnalysisMode.PredefinedTagging) {
				buttonswidget.Show();
				gameunitstaggerwidget1.Show();
			} else if (analysisMode == VideoAnalysisMode.Timeline) {
				timeline.Show();
			} else if (analysisMode == VideoAnalysisMode.GameUnits) {
				gameunitstaggerwidget1.Show();
				guTimeline.Show();
			}
		}

		private void HideWidgets() {
			leftbox.Hide();
			SetTagsBoxVisibility (false);
			buttonswidget.Hide();
			timeline.Hide();
			gameunitstaggerwidget1.Hide();
			guTimeline.Hide();
		}

		private void ClearWidgets() {
			buttonswidget.Categories = null;
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
						playercapturer.SeekToNextFrame(selectedTimeNode != null);
					break;
				case Constants.SEEK_BACKWARD:
					if(modifier == Constants.STEP)
						playercapturer.StepBackward();
					else
						playercapturer.SeekToPreviousFrame(selectedTimeNode != null);
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

		protected virtual void OnTimeNodeSelected(Play play)
		{
			SetTagsBoxVisibility (true);
			if (PlaySelectedEvent != null)
				PlaySelectedEvent(play);
		}

		protected virtual void OnSegmentClosedEvent()
		{
			SetTagsBoxVisibility (false);
			timeline.SelectedTimeNode = null;
			selectedTimeNode = null;
		}
		
		protected virtual void OnTick(object o, long currentTime, long streamLength,
			float currentPosition, bool seekable)
		{
			if(currentTime != 0 && timeline != null && openedProject != null) {
				uint frame = (uint)(currentTime * openedProject.Description.File.Fps / 1000);
				timeline.CurrentFrame = frame;
				guTimeline.CurrentFrame = frame;
			}
			gameunitstaggerwidget1.CurrentTime = new Time{MSeconds = (int)currentTime};
		}
		
		protected virtual void OnMultimediaError(object o, string message)
		{
			MessagesHelpers.ErrorMessage (this,
				Catalog.GetString("The following error happened and" +
				" the current project will be closed:")+"\n" + message);
			EmitCloseOpenedProject ();
		}
		
		private void EmitCloseOpenedProject () {
			if (CloseOpenedProjectEvent != null)
				CloseOpenedProjectEvent ();
		}
		
		private void EmitPlaySelected(Play play)
		{
			if (PlaySelectedEvent != null)
				PlaySelectedEvent(play);
		}

		private void EmitTimeNodeChanged(TimeNode tNode, object val)
		{
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode, val);
		}

		private void EmitPlaysDeleted(List<Play> plays)
		{
			if (PlaysDeletedEvent != null)
				PlaysDeletedEvent(plays);
		}
		
		protected virtual void EmitPlayCategoryChanged(Play play, Category cat)
		{
			if(PlayCategoryChanged != null)
				PlayCategoryChanged(play, cat);
		}

		private void OnPlayListNodeAdded(List<Play> plays)
		{
			if (PlayListNodeAddedEvent != null)
				PlayListNodeAddedEvent(plays);
		}

		private void EmitPlayListNodeSelected(PlayListPlay plNode)
		{
			if (PlayListNodeSelectedEvent != null)
				PlayListNodeSelectedEvent(plNode);
		}

		private void EmitSnapshotSeries(Play play) {
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent (play);
		}

		private void EmitNewTagAtFrame(Category category, int frame) {
			if (NewTagAtFrameEvent != null)
				NewTagAtFrameEvent(category, frame);
		}

		private void EmitNewTag(Category category) {
			if (NewTagEvent != null)
				NewTagEvent(category);
		}

		private void EmitNewTagStart(Category category) {
			if (NewTagStartEvent != null)
				NewTagStartEvent (category);
		}

		private void EmitNewTagStop(Category category) {
			if (NewTagStopEvent != null)
				NewTagStopEvent (category);
		}
		
		private void EmitNewTagCancel(Category category) {
			if (NewTagCancelEvent != null)
				NewTagCancelEvent (category);
		}
		
		private void EmitRenderPlaylist(IPlayList playlist) {
			if (RenderPlaylistEvent != null)
				RenderPlaylistEvent(playlist);
		}
		
		private void EmitTagPlay(Play play) {
			if (TagPlayEvent != null)
				TagPlayEvent (play);
		}
		
		private void EmitNewPlaylist() {
			if (NewPlaylistEvent != null)
				NewPlaylistEvent();
		}
		
		private void EmitOpenPlaylist() {
			if (OpenPlaylistEvent != null)
				OpenPlaylistEvent();
		}
		
		private void EmitSavePlaylist() {
			if (SavePlaylistEvent != null)
				SavePlaylistEvent();
		}
		
		private void EmitGameUnitEvent(GameUnit gameUnit, GameUnitEventType eType) {
			if (GameUnitEvent != null)
				GameUnitEvent(gameUnit, eType);
		}
		
		private void EmitUnitAdded(GameUnit gameUnit, int frame) {
			if (UnitAdded != null)
				UnitAdded(gameUnit, frame);
		}
		
		private void EmitUnitDeleted(GameUnit gameUnit, List<TimelineNode> units) {
			if (UnitDeleted != null)
				UnitDeleted(gameUnit, units);
		}
		
		private void EmitUnitSelected(GameUnit gameUnit, TimelineNode unit) {
			if (UnitSelected != null)
				UnitSelected(gameUnit, unit);
		}
		
		private void EmitUnitChanged(GameUnit gameUnit, TimelineNode unit, Time time) {
			if (UnitChanged != null)
				UnitChanged(gameUnit, unit, time);
		}
		
		private void EmitKeyPressed(object sender, int key, int modifier) {
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

