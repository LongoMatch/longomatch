//
//  Copyright (C) 2015 FLUENDO S.A.
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
using NUnit.Framework;
using Moq;
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tests.Services
{
	[TestFixture ()]
	public class TestRenderingJobsManager
	{
		[Test ()]
		public void TestRenderedCamera ()
		{
			Project p = Utils.CreateProject ();

			try {
				TimelineEvent evt = p.Timeline [0];
				evt.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (0) };
				PlaylistPlayElement element = new PlaylistPlayElement (evt);

				// Playlist with one event
				var playlist = new Playlist ();
				playlist.Elements.Add (element);

				// Create a job
				const string outputFile = "path";
				EncodingSettings settings = new EncodingSettings (VideoStandards.P720, EncodingProfiles.MP4, EncodingQualities.Medium,
					                            25, 1, outputFile, true, true, 20);
				EditionJob job = new EditionJob (playlist, settings);

				// Mock IMultimediaToolkit and video editor
				var mtk = Mock.Of<IMultimediaToolkit> (m => m.GetVideoEditor () == Mock.Of<IVideoEditor> ());
				// and guitoolkit
				var gtk = Mock.Of<IGUIToolkit> (g => g.RenderingStateBar == Mock.Of<IRenderingStateBar> ());
				// and a video editor
				Mock<IVideoEditor> mock = Mock.Get<IVideoEditor> (mtk.GetVideoEditor ());
				// And eventbroker
				Config.EventsBroker = Mock.Of<EventsBroker> ();
				Config.GUIToolkit = gtk;
				Config.MultimediaToolkit = mtk;

				// Create a rendering object with mocked interfaces
				RenderingJobsManager renderer = new RenderingJobsManager ();
				// Start service
				renderer.Start ();

				renderer.AddJob (job);

				// Check that AddSegment is called with the right video file.
				mock.Verify (m => m.AddSegment (p.Description.FileSet [0].FilePath,
					evt.Start.MSeconds, evt.Stop.MSeconds, evt.Rate, evt.Name, true, new Area ()), Times.Once ()); 

				/* Test with a camera index bigger than the total cameras */
				renderer.CancelAllJobs ();
				mock.ResetCalls ();
				evt = p.Timeline [1];
				evt.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (1) };
				element = new PlaylistPlayElement (evt);
				playlist.Elements [0] = element; 
				job = new EditionJob (playlist, settings);
				renderer.AddJob (job);
				mock.Verify (m => m.AddSegment (p.Description.FileSet [1].FilePath,
					evt.Start.MSeconds, evt.Stop.MSeconds, evt.Rate, evt.Name, true, new Area ()), Times.Once ()); 

				/* Test with the secondary camera */
				renderer.CancelAllJobs ();
				mock.ResetCalls ();
				evt = p.Timeline [1];
				evt.CamerasConfig = new ObservableCollection<CameraConfig> { new CameraConfig (2) };
				element = new PlaylistPlayElement (evt);
				playlist.Elements [0] = element; 
				job = new EditionJob (playlist, settings);
				renderer.AddJob (job);
				mock.Verify (m => m.AddSegment (p.Description.FileSet [0].FilePath,
					evt.Start.MSeconds, evt.Stop.MSeconds, evt.Rate, evt.Name, true, new Area ()), Times.Once ()); 
			} finally {
				Utils.DeleteProject (p);
			}
		}
	}
}

