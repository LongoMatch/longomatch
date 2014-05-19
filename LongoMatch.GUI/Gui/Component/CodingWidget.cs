//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Common;
using System.Collections.Generic;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CodingWidget : Gtk.Bin
	{
		AnalysisComponent parent;
		VideoAnalysisMode analysisMode;
		Project project;
		
		public CodingWidget ()
		{
			this.Build ();
			buttonswidget.NewMarkEvent += (c) => parent.EmitNewTag (c);
			buttonswidget.NewMarkStartEvent += (c) => parent.EmitNewTagStart (c);
			buttonswidget.NewMarkStopEvent += (c) => parent.EmitNewTagStop (c);
			buttonswidget.NewMarkCancelEvent += (c) => parent.EmitNewTagCancel (c);
			timeline.NewTagAtPosEvent += (c, p) => parent.EmitNewTagAtPos (c, p);
			timeline.TimeNodeChanged += (t, v) => parent.EmitTimeNodeChanged (t, v);
			timeline.PlayListNodeAdded += (p) => parent.EmitPlayListNodeAdded (p);
			timeline.TagPlay += (p) => parent.EmitTagPlay (p);
			timeline.SnapshotSeries += (t) => parent.EmitSnapshotSeries (t);
			timeline.RenderPlaylist += (p) => parent.EmitRenderPlaylist (p);
			timeline.TimeNodeDeleted += (p) => parent.EmitPlaysDeleted (p);
			timeline.TimeNodeSelected += (p) => parent.EmitPlaySelected (p);

			autoTaggingMode.Toggled += HandleViewToggled;
			autoTaggingMode.Active = true;
		}
		
		public void SetProject (Project project, bool isLive, PlaysFilter filter,
		                        AnalysisComponent parent) {
			this.parent = parent;
			this.project = project;	
			autoTaggingMode.Active = true;
			timeline.Visible = false;
			buttonswidget.Visible = true;
			buttonswidget.Project = project;
			timeline.SetProject (project, filter);
		}
		
		public AnalysisComponent AnalysisComponentParent {
			set;
			protected get;
		}
		
		public void AddPlay(Play play) {
			timeline.AddPlay(play);
		}
		
		public void DeletePlays (List<Play> plays) {
			timeline.RemovePlays(plays);
		}
		
		public Play SelectedPlay {
			set {
				timeline.SelectedTimeNode = value;
			}
		}
		
		public void UpdateCategories () {
			buttonswidget.Project = project;
		}
		
		public VideoAnalysisMode AnalysisMode {
			set {
				buttonswidget.Visible = (value == VideoAnalysisMode.ManualTagging) ||
					(value == VideoAnalysisMode.PredefinedTagging);
				timeline.Visible = value == VideoAnalysisMode.Timeline;
				if(value == VideoAnalysisMode.ManualTagging)
					buttonswidget.Mode = TagMode.Free;
				else if (value == VideoAnalysisMode.ManualTagging)
					buttonswidget.Mode = TagMode.Predifined;
				analysisMode = value;
				timeline.Visible = true;
			}
			protected get {
				return analysisMode;
			}
		}
		
		public bool WidgetsVisible {
			set {
				timeline.Visible = value && AnalysisMode == VideoAnalysisMode.Timeline;
				buttonswidget.Visible = value && (AnalysisMode == VideoAnalysisMode.ManualTagging ||
				                                  AnalysisMode == VideoAnalysisMode.PredefinedTagging);

			}
		}
		
		void HandleViewToggled (object sender, EventArgs e)
		{
			buttonswidget.Visible = autoTaggingMode.Active;
			timeline.Visible = timelineMode.Active;
		}
	}

}

