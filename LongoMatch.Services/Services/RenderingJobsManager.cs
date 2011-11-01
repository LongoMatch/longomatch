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
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Gui.Component;
using LongoMatch.Multimedia.Interfaces;
using LongoMatch.Video;
using LongoMatch.Store;
using LongoMatch.Video.Editor;
using LongoMatch.Gui;
using LongoMatch.Gui.Dialog;

namespace LongoMatch.Services
{
	public class RenderingJobsManager: IRenderingJobsManager
	{
		/* List of pending jobs */
		List<Job> jobs, pendingJobs;
		IVideoEditor videoEditor;
		MultimediaFactory factory;
		Job currentJob;
		RenderingStateBar stateBar;
		
		public RenderingJobsManager (RenderingStateBar stateBar)
		{
			jobs = new List<Job>();
			pendingJobs = new List<Job>();
			factory = new MultimediaFactory();
			this.stateBar = stateBar;
			stateBar.Cancel += (sender, e) => CancelCurrentJob();
			stateBar.ManageJobs += (sender, e) => ManageJobs();
		}
		
		public TreeStore Model {
			get {
				TreeStore model = new TreeStore(typeof(Job));
				foreach (Job job in jobs)
					model.AppendValues(job);
				return model;
			}
		}
		
		public void AddJob(Job job) {
			jobs.Add(job);
			pendingJobs.Add(job);
			UpdateJobsStatus();
			if (pendingJobs.Count == 1)
				StartNextJob();
		}
		
		public void RetryJobs(List<Job> retryJobs) {
			foreach (Job job in retryJobs) {
				if (!jobs.Contains(job))
					return;
				if (!pendingJobs.Contains(job)) {
					job.State = JobState.NotStarted;
					jobs.Remove(job);
					jobs.Add(job);
					pendingJobs.Add(job);
					UpdateJobsStatus();
				}
			}
		}
		
		public void DeleteJob(Job job) {
			job.State = JobState.Cancelled;
			CancelJob(job);
		}
		
		public void ClearDoneJobs() {
			jobs.RemoveAll(j => j.State == JobState.Finished);
		}
		
		public void CancelJobs(List<Job> cancelJobs) {
			foreach (Job job in cancelJobs){
				job.State = JobState.Cancelled;
				pendingJobs.Remove(job);
			}
			
			if (cancelJobs.Contains(currentJob))
				CancelCurrentJob();
		}
		
		public void CancelCurrentJob () {
			CancelJob(currentJob);
		}
		
		public void CancelJob(Job job) {
			if (currentJob != job) 
				return;
			
			videoEditor.Progress -= OnProgress;
			videoEditor.Cancel();
			job.State = JobState.Cancelled;
			RemoveCurrentFromPending();
			UpdateJobsStatus();
			StartNextJob();
		}
		
		public void CancelAllJobs() {
			foreach (Job job in pendingJobs)
				job.State = JobState.Cancelled;
			pendingJobs.Clear();
			CancelJob(currentJob);
		}
		
		protected void ManageJobs() {
			RenderingJobsDialog dialog = new RenderingJobsDialog(this);
			dialog.TransientFor = (stateBar.Toplevel as Gtk.Window);
			dialog.Run();
			dialog.Destroy();
		}
		
		public static Job ConfigureRenderingJob (PlayList playlist, Gtk.Widget parent)
		{
			VideoEditionProperties vep;
			Job job = null;
			int response;

			vep = new VideoEditionProperties();
			vep.TransientFor = (Gtk.Window)parent.Toplevel;
			response = vep.Run();
			while(response == (int)ResponseType.Ok && vep.EncodingSettings.OutputFile == "") {
				MessagePopup.PopupMessage(parent, MessageType.Warning,
				                          Catalog.GetString("Please, select a video file."));
				response=vep.Run();
			}
			if(response ==(int)ResponseType.Ok)
				job = new Job(playlist, vep.EncodingSettings, vep.EnableAudio, vep.TitleOverlay);
			vep.Destroy();
			return job;
		}
		
		private void LoadJob(Job job) {
			foreach(PlayListPlay segment in job.Playlist) {
				if(segment.Valid)
					videoEditor.AddSegment(segment.MediaFile.FilePath,
					                       segment.Start.MSeconds,
					                       segment.Duration.MSeconds,
					                       segment.Rate,
					                       segment.Name,
					                       segment.MediaFile.HasAudio);
			}
		}
		
		private void CloseAndNext() {
			RemoveCurrentFromPending();
			UpdateJobsStatus();
			StartNextJob();
		}
		
		private void ResetGui() {
			stateBar.ProgressText = "";
			stateBar.JobRunning = false;
		}
		
		private void StartNextJob() {
			if (pendingJobs.Count == 0) {
				ResetGui();
				return;
			}
			
			videoEditor = factory.getVideoEditor();
			videoEditor.Progress += OnProgress;
			currentJob = pendingJobs[0];
			LoadJob(currentJob);
			
			try {
				videoEditor.EncodingSettings = currentJob.EncodingSettings;
				videoEditor.EnableTitle = currentJob.OverlayTitle;
				videoEditor.EnableAudio = currentJob.EnableAudio;
				videoEditor.Start();
			}
			catch(Exception ex) {
				Log.Exception(ex);
				Log.Error("Error redering job: ", currentJob.Playlist.Filename);
				currentJob.State = JobState.Error;
			}
		}
		
		private void UpdateProgress(float progress) {
			stateBar.Fraction = progress;
			stateBar.ProgressText = String.Format("{0}... {1:0.0}%", Catalog.GetString("Rendering"),
			                              progress * 100);
		}
		
		private void UpdateJobsStatus() {
			stateBar.Text = String.Format("{0} ({1} {2})", Catalog.GetString("Rendering queue"),
			                              pendingJobs.Count, Catalog.GetString("Pending"));
		}
		
		private void RemoveCurrentFromPending () {
			try {
				pendingJobs.Remove(currentJob);
			} catch {}
		}
		
		private void MainLoopOnProgress (float progress) {
			if(progress > (float)EditorState.START && progress <= (float)EditorState.FINISHED
			   && progress > stateBar.Fraction) {
				UpdateProgress(progress);
			}

			if(progress == (float)EditorState.CANCELED) {
				Log.Debug ("Job was cancelled");
				currentJob.State = JobState.Cancelled;
				CloseAndNext();
			}

			else if(progress == (float)EditorState.START) {
				Log.Debug ("Job started");
				currentJob.State = JobState.Running;
				stateBar.JobRunning = true;
				UpdateProgress(progress);
			}

			else if(progress == (float)EditorState.FINISHED) {
				Log.Debug ("Job finished successfully");
				videoEditor.Progress -= OnProgress;
				UpdateProgress(progress);
				currentJob.State = JobState.Finished;
				CloseAndNext();
			}

			else if(progress == (float)EditorState.ERROR) {
				Log.Debug ("Job finished with errors");
				MessagePopup.PopupMessage(stateBar, MessageType.Error,
				                          Catalog.GetString("An error has occurred in the video editor.")
				                          +Catalog.GetString("Please, try again."));
				currentJob.State = JobState.Error;
				CloseAndNext();
			}
		}
		
		protected void OnProgress(float progress)
		{
			/* FIXME: Check if this is really sent from a gstreamer thread */
			Application.Invoke(delegate {
				MainLoopOnProgress (progress);
			});
		}
	}
}

