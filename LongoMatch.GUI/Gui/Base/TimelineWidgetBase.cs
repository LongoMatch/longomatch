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

using LongoMatch.Interfaces;
using LongoMatch.Gui.Base;
using LongoMatch.Gui.Component;
using Gtk;

namespace LongoMatch.Gui.Base
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TimelineWidgetBase : Gtk.Bin
	{
		protected TimeReferenceWidget tr;
		protected CategoriesScale cs;
		protected uint currentFrame, pixelRatio, frames;
		protected bool loaded;
		
		public TimelineWidgetBase ()
		{
			this.Build ();
			
			tr = new TimeReferenceWidget();
			cs = new CategoriesScale();
			
			cs.WidthRequest = 100;
			toolsbox.HeightRequest = 50 - leftbox.Spacing;
			tr.HeightRequest = 50 - leftbox.Spacing;
			
			categoriesbox.PackStart(cs, false, false, 0);
			timelinebox.PackStart(tr,false,false,0);
			
			zoomscale.CanFocus = false;
			loaded = false;
			frames = 0;
			
			ScrolledWindow.Vadjustment.ValueChanged += HandleScrollEvent;
			ScrolledWindow.Hadjustment.ValueChanged += HandleScrollEvent;
			ScrolledWindow.HScrollbar.SizeAllocated += OnSizeAllocated;
		}
		
		public ScrolledWindow ScrolledWindow {
			get {
				return GtkScrolledWindow;
			}
		}
		
		public Box TimelineBox {
			get{
				return timelinebox;
			}
		}
		
		public Scale ZoomScale {
			get{
				return zoomscale;
			}
		}
		
		public Alignment Alignment {
			get{
				return categoriesalignment1;
			}
		}

		public void AdjustPostion(uint currentframe) {
			int visibleWidth;
			int realWidth;
			uint pos;
			int scrollbarWidth;
			if(Visible) {
				scrollbarWidth= ScrolledWindow.VScrollbar.Allocation.Width;
				visibleWidth = ScrolledWindow.Allocation.Width-scrollbarWidth;
				realWidth = TimelineBox.Allocation.Width;
				pos = currentframe/pixelRatio;
				if(pos+visibleWidth < realWidth) {
					ScrolledWindow.Hadjustment.Value = pos;
				}
				else {
					ScrolledWindow.Hadjustment.Value = realWidth-visibleWidth-20;
				}
			}
		}

		protected virtual void HandleScrollEvent(object sender, System.EventArgs args)
		{
			if(sender == ScrolledWindow.Vadjustment)
				cs.Scroll = ScrolledWindow.Vadjustment.Value;
			else if(sender == ScrolledWindow.Hadjustment)
				tr.Scroll = ScrolledWindow.Hadjustment.Value;
		}

		protected virtual void OnSizeAllocated(object sender, SizeAllocatedArgs e)
		{
			/* Align the categories list widget on top of the timeline's horizontal bar */
			if(sender == ScrolledWindow.HScrollbar)
				Alignment.BottomPadding = (uint) ScrolledWindow.HScrollbar.Allocation.Height;
		}
		
	}
	
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TimelineBase <W, Z> : TimelineWidgetBase where Z:ITimelineNode where W: TimeScaleBase<Z>
	{
		protected Dictionary<object, W> tsList;
		protected Z selected;
		
		public TimelineBase (): base()
		{
			SetPixelRatio(10);
			tsList = new Dictionary<object, W>();
		}

		public Z SelectedTimeNode {
			get {
				return selected;
			}
			set {
				if(!loaded)
					return;

				selected = value;
				foreach(W  ts in tsList.Values)
					ts.SelectedTimeNode = value;
				if(selected != null) {
					if(SelectedTimeNode.StartFrame/pixelRatio < ScrolledWindow.Hadjustment.Value ||
					                SelectedTimeNode.StartFrame/pixelRatio > ScrolledWindow.Hadjustment.Value +
					                ScrolledWindow.Allocation.Width - ScrolledWindow.VScrollbar.Allocation.Width)
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
				if(!loaded)
					return;

				currentFrame = value;
				foreach(W ts in tsList.Values)
					ts.CurrentFrame = value;
				tr.CurrentFrame = value;
				QueueDraw();
			}
		}

		protected void SetPixelRatio(uint pixelRatio) {
			if(!loaded)
				return;

			this.pixelRatio = pixelRatio;
			tr.PixelRatio = pixelRatio;
			foreach(W  ts in tsList.Values)
				ts.PixelRatio = pixelRatio;
			ZoomScale.Value=pixelRatio;
		}

		protected void ResetGui() {
			//Unrealize all children
			foreach(Widget w in TimelineBox.AllChildren) {
				TimelineBox.Remove(w);
				w.Destroy();
			}
		}

		protected virtual void OnFitbuttonClicked(object sender, System.EventArgs e)
		{
			AdjustPostion(currentFrame);
		}

		protected virtual void OnZoomscaleValueChanged(object sender, System.EventArgs e)
		{
			SetPixelRatio((uint)(ZoomScale.Value));
			QueueDraw();
			AdjustPostion(currentFrame);
		}
	}
}

