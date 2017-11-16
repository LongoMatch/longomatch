//
//  Copyright (C) 2017 FLUENDO.S.A.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
using System;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;

namespace Tests.State
{
	public class TestProjectAnalysisState
	{
		Mock<IMultimediaToolkit> mtkMock;
		Mock<IGUIToolkit> gtkMock;
		Mock<IVideoPlayer> playerMock;
		Mock<IMultiVideoPlayer> multiplayerMock;
		ProjectAnalysisState state;
		LMProject project;
		LMProjectVM projectVM;
		LMProjectAnalysisVM analysisVM;

		[OneTimeSetUp]
		public void SetupOnce ()
		{
			var hotkeysMock = new Mock<IHotkeysService> ();
			playerMock = new Mock<IVideoPlayer> ();
			multiplayerMock = new Mock<IMultiVideoPlayer> ();
			mtkMock = new Mock<IMultimediaToolkit> ();
			gtkMock = new Mock<IGUIToolkit> ();
			var capturerMock = new Mock<IFramesCapturer> ();
			mtkMock.Setup (m => m.GetFramesCapturer ()).Returns (capturerMock.Object);
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			mtkMock.Setup (m => m.DiscoverFile (It.IsAny<string> (), true)).Returns (new MediaFile ());
			gtkMock.SetupGet (o => o.DeviceScaleFactor).Returns (1.0f);
			App.Current.MultimediaToolkit = mtkMock.Object;
			App.Current.HotkeysService = hotkeysMock.Object;
			App.Current.GUIToolkit = gtkMock.Object;
		}

		[SetUp]
		public void Setup ()
		{
			state = new ProjectAnalysisState ();
			project = Utils.CreateProject ();
			projectVM = new LMProjectVM { Model = project };
			analysisVM = new LMProjectAnalysisVM { Project = projectVM };
			state.Panel = new Mock<IPanel> ().Object;
			gtkMock.Setup (g => g.SelectMediaFiles (It.IsAny<MediaFileSet> ())).Returns (true);
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				Utils.DeleteProject (project);
			} catch {
			}
		}

		[Test]
		public async Task LoadState_ProjectFileWithNullDuration_DiscoverFileOk ()
		{
			// Arrange
			Assert.Greater (project.Description.FileSet.Count, 0);
			foreach (var file in project.Description.FileSet) {
				file.Duration = null;
			}

			// Act
			bool ret = await state.LoadState (analysisVM);

			// Assert
			mtkMock.Verify (g => g.DiscoverFile (It.IsAny<string> (), true), Times.Exactly (project.Description.FileSet.Count));
			Assert.IsTrue (ret);
		}

		[Test]
		public async Task LoadState_AllGood_TransitionOK ()
		{
			// Act
			bool ret = await state.LoadState (analysisVM);

			// Assert
			Assert.IsTrue (ret);
			Assert.AreNotEqual (projectVM, state.ViewModel.Project);
			Assert.AreEqual (project, state.ViewModel.Project.Model);
		}

		[Test]
		public async Task LoadState_FilesMissingNotAdded_TransitionCancelled ()
		{
			// Arrange
			Utils.DeleteProject (project);
			gtkMock.Setup (g => g.SelectMediaFiles (It.IsAny<MediaFileSet> ())).Returns (false);

			// Act
			bool ret = await state.LoadState (analysisVM);

			// Assert
			Assert.IsFalse (ret);
		}

		[Test]
		public async Task LoadState_FilesMissingAdded_TransitionOK ()
		{
			// Arrange
			Utils.DeleteProject (project);
			gtkMock.Setup (g => g.SelectMediaFiles (It.IsAny<MediaFileSet> ())).Returns (true);

			// Act
			bool ret = await state.LoadState (analysisVM);

			// Assert
			Assert.IsTrue (ret);
		}

		[Test]
		public async Task LoadState_VideoPlayerException_TransitionCancelled ()
		{
			playerMock.Setup (p => p.Open (It.IsAny<MediaFile> ())).Throws<Exception> ();

			// Act
			bool ret = await state.LoadState (analysisVM);

			// Assert
			Assert.IsTrue (ret);
		}

		[Test]
		public async Task LoadState_SupportsMultipleCameras_NotLimited ()
		{
			mtkMock.Setup (m => m.GetMultiPlayer ()).Returns (multiplayerMock.Object);
			var total_filesets = analysisVM.Project.FileSet.ViewModels.Count;

			bool ret = await state.LoadState (analysisVM);

			Assert.IsTrue (ret);
			Assert.Greater (total_filesets, 1);
			Assert.AreEqual (total_filesets, analysisVM.Project.FileSet.ViewModels.Count);
			Assert.AreEqual (total_filesets, state.ViewModel.VideoPlayer.FileSet.ViewModels.Count);
			Assert.AreEqual (analysisVM.Project.FileSet.Model.ID, state.ViewModel.VideoPlayer.FileSet.Model.ID);
		}

		[Test]
		public async Task LoadState_MultipleCamerasSupportDisabled_MediaFileLimited ()
		{
			mtkMock.Setup (m => m.GetMultiPlayer ()).Throws (new Exception ());
			var total_filesets = analysisVM.Project.FileSet.ViewModels.Count;

			bool ret = await state.LoadState (analysisVM);

			Assert.IsTrue (ret);
			Assert.Greater (total_filesets, 1);
			Assert.AreEqual (total_filesets, analysisVM.Project.FileSet.ViewModels.Count);
			Assert.AreEqual (1, state.ViewModel.VideoPlayer.FileSet.ViewModels.Count);
			Assert.AreEqual (analysisVM.Project.FileSet.Model.ID, state.ViewModel.VideoPlayer.FileSet.Model.ID);
		}

	}
}
