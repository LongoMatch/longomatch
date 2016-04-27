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
using System.Collections.Generic;
using Gtk;

using VAS.Core.Common;
using VAS.Core.Interfaces;


namespace LongoMatch.Gui.Dialog
{
	public partial class RenderingJobsDialog : Gtk.Dialog
	{
		IRenderingJobsManager manager;

		public RenderingJobsDialog (Window parent)
		{
			TransientFor = parent;
			this.Build ();
			this.manager = Config.RenderingJobsManger;
			UpdateModel ();
			cancelbutton.Clicked += OnCancelbuttonClicked;
			clearbutton.Clicked += OnClearbuttonClicked;
			retrybutton.Clicked += OnRetrybuttonClicked;
			renderingjobstreeview2.Selection.Changed += OnSelectionChanged;
		}

		private void UpdateModel ()
		{
			TreeStore model = new TreeStore (typeof(Job));
			
			foreach (Job job in manager.Jobs)
				model.AppendValues (job);
			renderingjobstreeview2.Model = model;
			QueueDraw ();
		}

		private void UpdateSelection ()
		{
			/* FIXME: Add support for multiple selection */
			Job job;
			List<Job> jobs = renderingjobstreeview2.SelectedJobs ();
			
			cancelbutton.Visible = false;
			retrybutton.Visible = false;
			
			if (jobs.Count == 0)
				return;
			
			job = jobs [0];
			
			if (job.State == JobState.NotStarted ||
			    job.State == JobState.Running)
				cancelbutton.Visible = true;
			
			if (job.State == JobState.Error || job.State == JobState.Cancelled)
				retrybutton.Visible = true;
		}

		protected virtual void OnClearbuttonClicked (object sender, System.EventArgs e)
		{
			manager.ClearDoneJobs ();
			UpdateModel ();
			UpdateSelection ();
		}

		protected virtual void OnCancelbuttonClicked (object sender, System.EventArgs e)
		{
			manager.CancelJobs (renderingjobstreeview2.SelectedJobs ());
			UpdateSelection ();
			QueueDraw ();
		}

		protected virtual void OnRetrybuttonClicked (object sender, System.EventArgs e)
		{
			manager.RetryJobs (renderingjobstreeview2.SelectedJobs ());
			UpdateModel ();
			UpdateSelection ();
		}

		protected virtual void OnSelectionChanged (object sender, System.EventArgs e)
		{
			UpdateSelection ();
		}
	}
}

