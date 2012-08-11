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
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Store;

namespace LongoMatch.Services
{
	public class RenderingJobsManager: IRenderingJobsManager
	{
		/* List of pending jobs */
		List<Job> jobs, pendingJobs;
		IVideoEditor videoEditor;
		IFramesCapturer capturer;
		Job currentJob;
		IRenderingStateBar stateBar;
		IMultimediaToolkit multimediaToolkit;
		IGUIToolkit guiToolkit;
		
		public RenderingJobsManager (IMultimediaToolkit multimediaToolkit, IGUIToolkit guiToolkit)
		{
			this.guiToolkit = guiToolkit;
			this.multimediaToolkit = multimediaToolkit; 
			this.stateBar = guiToolkit.MainWindow.RenderingStateBar;
			capturer = multimediaToolkit.GetFramesCapturer();
			jobs = new List<Job>();
			pendingJobs = new List<Job>();
			stateBar.Cancel += (sender, e) => CancelCurrentJob();
			stateBar.ManageJobs += (sender, e) => ManageJobs();
		}
		
		public List<Job> Jobs {
			get {
				return jobs;
			}
		}
		
		public void AddJob(Job job) {
			if (job == null)
				return;
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
			guiToolkit.ManageJobs(this);
		}
		
		private void LoadJob(Job job) {
			foreach(PlayListPlay segment in job.Playlist) {
				if (!ProcessSegment(segment))
					continue;
			}
		}
		
		private bool ProcessSegment(PlayListPlay segment) {
			if(!segment.Valid)
				return false;
			
			Log.Debug(String.Format("Adding segment with {0} drawings", segment.Drawings.Count));
			if (segment.Drawings.Count >= 1) {
				Drawing drawing = segment.Drawings[0];
				string image_path = CreateStillImage(segment.MediaFile.FilePath, drawing);
				
				videoEditor.AddSegment(segment.MediaFile.FilePath,
				                       segment.Start.MSeconds,
				                       drawing.RenderTime - segment.Start.MSeconds,
				                       segment.Rate,
				                       segment.Name,
				                       segment.MediaFile.HasAudio);
				videoEditor.AddImageSegment(image_path,
				                            drawing.RenderTime,
				                            drawing.PauseTime,
				                            segment.Name);
				videoEditor.AddSegment(segment.MediaFile.FilePath,
				                       drawing.RenderTime,
				                       segment.Stop.MSeconds - drawing.RenderTime,
				                       segment.Rate,
				                       segment.Name,
				                       segment.MediaFile.HasAudio);
			} else {
				videoEditor.AddSegment(segment.MediaFile.FilePath,
				                       segment.Start.MSeconds,
				                       segment.Duration.MSeconds,
				                       segment.Rate,
				                       segment.Name,
				                       segment.MediaFile.HasAudio);
			}
			return true;
		}
		
		private string CreateStillImage(string filename, Drawing drawing) {
			Image frame, final_image;
			string path = System.IO.Path.GetTempFileName();
			
			capturer.Open(filename);
			capturer.SeekTime(drawing.RenderTime, true);
			frame = capturer.GetCurrentFrame();
			final_image = Image.Composite(frame, drawing.Pixbuf);
			final_image.Save(path);
			return path;
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
			
			videoEditor = multimediaToolkit.GetVideoEditor();
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
				Log.Error("Error redering job: ", currentJob.Name);
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
				guiToolkit.ErrorMessage(Catalog.GetString("An error has occurred in the video editor.")
				                          +Catalog.GetString("Please, try again."));
				currentJob.State = JobState.Error;
				CloseAndNext();
			}
		}
		
		protected void OnProgress(float progress)
		{
			MainLoopOnProgress (progress);
		}
	}
}

