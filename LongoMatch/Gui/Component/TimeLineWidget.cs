// TimeLineWidget.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using Gtk;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component {

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TimeLineWidget : Gtk.Bin
	{

		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeDeletedHandler TimeNodeDeleted;
		public event NewMarkAtFrameEventHandler NewMarkEvent;
		//public event PlayListNodeAddedHandler PlayListNodeAdded;

		private Dictionary<Category,TimeScale> tsList;
		private Categories categories;
		private TimeReferenceWidget tr;
		CategoriesScale cs;
		private uint frames;
		private uint pixelRatio;
		private Play selected;
		private uint currentFrame;
		private bool hasProject;


		public TimeLineWidget()
		{
			this.Build();
			SetPixelRatio(10);
			zoomscale.CanFocus = false;

			GtkScrolledWindow.Vadjustment.ValueChanged += HandleScrollEvent;
			GtkScrolledWindow.Hadjustment.ValueChanged += HandleScrollEvent;

			GtkScrolledWindow.HScrollbar.SizeAllocated += OnSizeAllocated;

			cs = new CategoriesScale();
			cs.WidthRequest = 100;
			categoriesbox.PackStart(cs, false, false, 0);

			tr = new TimeReferenceWidget();
			timescalebox.PackStart(tr,false,false,0);

			tr.HeightRequest = 50 - leftbox.Spacing;
			toolsbox.HeightRequest = 50 - leftbox.Spacing;
		}

		public Play SelectedTimeNode {
			get {
				return selected;
			}
			set {
				if(!hasProject)
					return;

				selected = value;
				foreach(TimeScale  ts in tsList.Values)
					ts.SelectedTimeNode = value;
				if(selected != null) {
					if(SelectedTimeNode.StartFrame/pixelRatio < GtkScrolledWindow.Hadjustment.Value ||
					                SelectedTimeNode.StartFrame/pixelRatio > GtkScrolledWindow.Hadjustment.Value +
					                GtkScrolledWindow.Allocation.Width - GtkScrolledWindow.VScrollbar.Allocation.Width)
						AdjustPostion(SelectedTimeNode.StartFrame);
				}
				QueueDraw();
			}
		}

		public uint CurrentFrame {
			get {
				return currentFrame;
			}
			set {
				if(!hasProject)
					return;

				currentFrame = value;
				foreach(TimeScale  ts in tsList.Values)
					ts.CurrentFrame = value;
				tr.CurrentFrame = value;
				QueueDraw();
			}
		}

		public void AdjustPostion(uint currentframe) {
			int visibleWidth;
			int realWidth;
			uint pos;
			int scrollbarWidth;
			if(Visible) {
				scrollbarWidth= GtkScrolledWindow.VScrollbar.Allocation.Width;
				visibleWidth = GtkScrolledWindow.Allocation.Width-scrollbarWidth;
				realWidth = vbox1.Allocation.Width;
				pos = currentframe/pixelRatio;
				if(pos+visibleWidth < realWidth) {
					GtkScrolledWindow.Hadjustment.Value = pos;
				}
				else {
					GtkScrolledWindow.Hadjustment.Value = realWidth-visibleWidth-20;
				}
			}
		}

		private void SetPixelRatio(uint pixelRatio) {
			if(!hasProject)
				return;

			this.pixelRatio = pixelRatio;
			tr.PixelRatio = pixelRatio;
			foreach(TimeScale  ts in tsList.Values)
				ts.PixelRatio = pixelRatio;
			zoomscale.Value=pixelRatio;
		}

		public Project Project {
			set {
				ResetGui();

				if(value == null) {
					categories = null;
					tsList.Clear();
					tsList=null;
					hasProject = false;
					return;
				}
				hasProject = true;
				categories = value.Categories;
				tsList = new Dictionary<Category, TimeScale>();
				frames = value.Description.File.GetFrames();

				cs.Categories = categories;
				cs.Show();

				tr.Frames = frames;
				tr.FrameRate = value.Description.File.Fps;
				tr.Show();

				foreach(Category cat in  categories) {
					List<Play> playsList = value.PlaysInCategory(cat);
					TimeScale ts = new TimeScale(cat, playsList,frames);
					tsList[cat] = ts;
					ts.TimeNodeChanged += new TimeNodeChangedHandler(OnTimeNodeChanged);
					ts.TimeNodeSelected += new TimeNodeSelectedHandler(OnTimeNodeSelected);
					ts.TimeNodeDeleted += new TimeNodeDeletedHandler(OnTimeNodeDeleted);
					ts.NewMarkAtFrameEvent += new NewMarkAtFrameEventHandler(OnNewMark);
					vbox1.PackStart(ts,true,true,0);
					ts.Show();
				}
				SetPixelRatio(3);
			}
		}

		public void AddPlay(Play play) {
			TimeScale ts;
			if(tsList.TryGetValue(play.Category, out ts))
				ts.AddPlay(play);
		}

		public void RemovePlays(List<Play> plays) {
			TimeScale ts;
			foreach(Play play in plays) {
				if(tsList.TryGetValue(play.Category, out ts))
					ts.RemovePlay(play);
			}

		}
		private void ResetGui() {
			//Unrealize all children
			foreach(Widget w in vbox1.AllChildren) {
				vbox1.Remove(w);
				w.Destroy();
			}
		}

		protected virtual void OnNewMark(Category category, int frame) {
			if(NewMarkEvent != null)
				NewMarkEvent(category,frame);
		}

		protected virtual void OnTimeNodeChanged(TimeNode tn, object val) {
			if(TimeNodeChanged != null)
				TimeNodeChanged(tn,val);
		}

		protected virtual void OnTimeNodeSelected(Play tn) {
			if(TimeNodeSelected != null)
				TimeNodeSelected(tn);
		}
		protected virtual void OnTimeNodeDeleted(List<Play> plays) {
			if(TimeNodeDeleted != null)
				TimeNodeDeleted(plays);
		}

		protected virtual void OnFitbuttonClicked(object sender, System.EventArgs e)
		{
			AdjustPostion(currentFrame);
		}

		protected virtual void OnZoomscaleValueChanged(object sender, System.EventArgs e)
		{
			SetPixelRatio((uint)(zoomscale.Value));
			QueueDraw();
			AdjustPostion(currentFrame);
		}

		protected virtual void HandleScrollEvent(object sender, System.EventArgs args)
		{
			if(sender == GtkScrolledWindow.Vadjustment)
				cs.Scroll = GtkScrolledWindow.Vadjustment.Value;
			else if(sender == GtkScrolledWindow.Hadjustment)
				tr.Scroll = GtkScrolledWindow.Hadjustment.Value;
		}

		protected virtual void OnSizeAllocated(object sender, SizeAllocatedArgs e)
		{
			/* Align the categories list widget on top of the timeline's horizontal bar */
			if(sender == GtkScrolledWindow.HScrollbar)
				categoriesalignment1.BottomPadding = (uint) GtkScrolledWindow.HScrollbar.Allocation.Height;
		}
	}
}
