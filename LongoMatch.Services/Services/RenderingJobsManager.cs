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
		IVideoConverter videoConverter;
		IFramesCapturer capturer;
		Job currentJob;
		IRenderingStateBar stateBar;
		IMultimediaToolkit multimediaToolkit;
		IGUIToolkit guiToolkit;
		
		public RenderingJobsManager (IMultimediaToolkit multimediaToolkit, IGUIToolkit guiToolkit)
		{
			this.guiToolkit = guiToolkit;
			this.multimediaToolkit = multimediaToolkit; 
			this.stateBar = guiToolkit.RenderingStateBar;
			capturer = multimediaToolkit.GetFramesCapturer();
			jobs = new List<Job>();
			pendingJobs = new List<Job>();
			stateBar.Cancel += (sender, e) => CancelCurrentJob();
			stateBar.ManageJobs += (sender, e) => ManageJobs();
			Config.EventsBroker.ConvertVideoFilesEvent += (inputFiles, encSettings) => {
				ConversionJob job = new ConversionJob(inputFiles, encSettings);
				AddJob (job);
			};; 
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
			
			if (job is EditionJob) {
				videoEditor.Progress -= OnProgress;
				videoEditor.Error -= OnError;
				videoEditor.Cancel();
			} else {
				videoConverter.Progress -= OnProgress;
				videoConverter.Error -= OnError;
				videoConverter.Cancel();
			}
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
			guiToolkit.ManageJobs ();
		}
		
		private void LoadConversionJob (ConversionJob job) {
			videoConverter = multimediaToolkit.GetVideoConverter(job.EncodingSettings.OutputFile);
			videoConverter.Progress += OnProgress;
			videoConverter.EncodingSettings = job.EncodingSettings;
			videoConverter.Error += OnError;
			
			foreach(MediaFile file in job.InputFiles) {
				videoConverter.AddFile (file.FilePath, file.Duration.MSeconds);
			}
			
			try {
				videoConverter.Start();
			} catch(Exception ex) {
				videoConverter.Cancel();
				job.State = JobState.Error;
				Log.Exception(ex);
				Log.Error("Error rendering job: ", job.Name);
				guiToolkit.ErrorMessage (Catalog.GetString("Error rendering job: ") + ex.Message);
			}
		}

		private void LoadEditionJob(EditionJob job) {
			videoEditor = multimediaToolkit.GetVideoEditor();
			videoEditor.EncodingSettings = job.EncodingSettings;
			videoEditor.Progress += OnProgress;
			videoEditor.Error += OnError;
			
			foreach(PlayListPlay segment in job.Playlist) {
				if (!ProcessSegment(segment))
					continue;
			}
			
			try {
				videoEditor.Start();
			}
			catch(Exception ex) {
				videoEditor.Cancel();
				job.State = JobState.Error;
				Log.Exception(ex);
				Log.Error("Error rendering job: ", job.Name);
				guiToolkit.ErrorMessage (Catalog.GetString("Error rendering job: ") + ex.Message);
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
				                       drawing.Render.MSeconds - segment.Start.MSeconds,
				                       segment.Rate,
				                       segment.Name,
				                       segment.MediaFile.HasAudio);
				videoEditor.AddImageSegment(image_path,
				                            drawing.Render.MSeconds,
				                            drawing.Pause.MSeconds,
				                            segment.Name);
				videoEditor.AddSegment(segment.MediaFile.FilePath,
				                       drawing.Render.MSeconds,
				                       segment.Stop.MSeconds - drawing.Render.MSeconds,
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
			string path = System.IO.Path.GetTempFileName().Replace(@"\", @"\\");
			
			capturer.Open(filename);
			capturer.Seek (drawing.Render, true);
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
			
			currentJob = pendingJobs[0];
			if (currentJob is EditionJob) {
				LoadEditionJob(currentJob as EditionJob);
			} else {
				LoadConversionJob(currentJob as ConversionJob);
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
		
		void HandleError () {
			Log.Debug ("Job finished with errors");
			guiToolkit.ErrorMessage(Catalog.GetString("An error has occurred in the video editor.")
			                        +Catalog.GetString("Please, try again."));
			currentJob.State = JobState.Error;
			CloseAndNext();
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
				if (currentJob is EditionJob) {
					videoEditor.Progress -= OnProgress;
				} else {
					videoConverter.Progress -= OnProgress;
				}
				UpdateProgress(progress);
				currentJob.State = JobState.Finished;
				CloseAndNext();
			}

			else if(progress == (float)EditorState.ERROR) {
				HandleError ();
			}
		}
		
		protected void OnError (string message)
		{
			HandleError ();
		}
		
		protected void OnProgress(float progress)
		{
			MainLoopOnProgress (progress);
		}
	}
}

