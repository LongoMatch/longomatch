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
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;

namespace Tests.State
{
	public class TestProjectAnalysisState
	{
		[Test]
		public async void LoadState_ProjectFileWithNullDuration_DiscoverFileOk ()
		{
			// Arrange
			ProjectAnalysisState state = new ProjectAnalysisState ();
			LMProject project = Utils.CreateProject ();
			LMProjectVM projectVM = new LMProjectVM { Model = project };
			LMProjectAnalysisVM analysisVM = new LMProjectAnalysisVM { Project = projectVM };

			Assert.Greater (project.Description.FileSet.Count, 0);
			foreach (var file in project.Description.FileSet) {
				file.Duration = null;
			}

			var playerMock = new Mock<IVideoPlayer> ();
			var mtkMock = new Mock<IMultimediaToolkit> ();
			var capturerMock = new Mock<IFramesCapturer> ();
			mtkMock.Setup (m => m.GetFramesCapturer ()).Returns (capturerMock.Object);
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			App.Current.MultimediaToolkit = mtkMock.Object;
			App.Current.HotkeysService = new Mock<IHotkeysService> ().Object;

			state.Panel = new Mock<IPanel> ().Object;

			// Act
			await state.LoadState (analysisVM);

			// Assert
			mtkMock.Verify (g => g.DiscoverFile (It.IsAny<string> (), true), Times.Exactly (project.Description.FileSet.Count));
		}


	}
}
