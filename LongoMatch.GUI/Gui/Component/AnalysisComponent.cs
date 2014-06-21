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
		static Project openedProject;
		ProjectType projectType;
		bool detachedPlayer;
		Gtk.Window playerWindow;
		
		public AnalysisComponent ()
		{
			this.Build ();
			projectType = ProjectType.None;
			playsSelection.Visible = true;

			playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Player;
			ConnectSignals();
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
		
		public void AddPlay(Play play) {
			playsSelection.AddPlay(play);
			codingwidget.AddPlay (play);
		}
		
		public void UpdateCategories () {
			codingwidget.UpdateCategories ();
		}
		
		public void DeletePlays (List<Play> plays) {
			playsSelection.RemovePlays(plays);
			codingwidget.DeletePlays (plays);
		}
		
		private void ConnectSignals() {
			playercapturer.Detach += DetachPlayer;
			KeyPressEvent += (o, args) => (
				Config.EventsBroker.EmitKeyPressed(o, (int)args.Event.Key, (int)args.Event.State));
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
			}
			playercapturer.Detached = detach;
		}
		
		public void CloseOpenedProject () {
			openedProject = null;
			projectType = ProjectType.None;
			if (detachedPlayer)
				DetachPlayer(false);
			ClearWidgets();
		}
		
		public void SetProject(Project project, ProjectType projectType, CaptureSettings props, PlaysFilter filter)
		{
			openedProject = project;
			this.projectType = projectType;
			
			if(projectType == ProjectType.FakeCaptureProject) {
				CreateCodingUI ();
			} else {
				CreatePreviewUI ();
			}
			
			codingwidget.SetProject (project, projectType, filter);
			playsSelection.SetProject (project, filter);
		}
		
		void CreateCodingUI () {
			HPaned centralpane, rightpane;
			VBox vbox;
			PeriodsRecoder periodsrecorder;
			
			ClearWidgets ();

			centralpane = new HPaned();
			rightpane = new HPaned ();
			vbox = new VBox ();
			
			playsSelection = new PlaysSelectionWidget ();
			codingwidget = new CodingWidget();
			periodsrecorder = new PeriodsRecoder ();
			playercapturer = null;
			
			centralpane.Show ();
			rightpane.Show ();
			vbox.Show();
			playsSelection.Show ();
			codingwidget.Show ();
			periodsrecorder.Show ();
			
			centralpane.Pack1 (playsSelection, true, true);
			centralpane.Pack2 (rightpane, true, true);
			rightpane.Pack1 (vbox, true, true);
			vbox.PackStart (periodsrecorder, false, true, 0);
			vbox.PackEnd (codingwidget, true, true, 0);
			Add (centralpane);
		}

		void CreatePreviewUI () {
			VPaned centralpane;
			HPaned uppane, rightpane;
			
			ClearWidgets ();

			centralpane = new VPaned();
			uppane = new HPaned ();
			rightpane = new HPaned();
			
			playsSelection = new PlaysSelectionWidget ();
			codingwidget = new CodingWidget();
			playercapturer = new PlayerCapturerBin ();
			if(projectType == ProjectType.FileProject) {
				playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Player;
			} else {
				playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.PreviewCapturer;
				playercapturer.PeriodsNames = openedProject.Categories.GamePeriods;
				playercapturer.PeriodsTimers = openedProject.Periods;
			}
			
			centralpane.Show ();
			uppane.Show ();
			rightpane.Show ();
			playsSelection.Show ();
			codingwidget.Show ();
			playercapturer.Show ();
			
			centralpane.Pack1 (uppane, true, true);
			centralpane.Pack2 (codingwidget, true, true);
			uppane.Pack1 (playsSelection, true, true);
			uppane.Pack2 (rightpane, true, true);
			rightpane.Pack1 (playercapturer, true, true);
			Add (centralpane);
		}
		
		void ClearWidgets() {
			if (Children.Length == 1) 
				Children[0].Destroy();
			if (playsSelection != null)
				playsSelection.Destroy();
			if (codingwidget != null)
				codingwidget.Destroy();
			if (playercapturer != null) {
				playercapturer.Destroy();
			}
		}
		
	}
}

