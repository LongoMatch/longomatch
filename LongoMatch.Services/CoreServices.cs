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
using LongoMatch.Core.Hotkeys;
using LongoMatch.Core.Interfaces;
using LongoMatch.Services.States;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.MVVMC;
using VAS.Services;
using VAS.Services.ViewModel;


namespace LongoMatch.Services
{
	public class CoreServices
	{
		public static IProjectsImporter ProjectsImporter;
		internal static ToolsManager toolsManager;
		static JobsManagerVM jobsManagerVM;

		public static void Init ()
		{
			CoreTool tool = new CoreTool ();
			tool.Enable ();
			Scanner.ScanAll ();
			VASServicesInit.Init ();
			RegisterServices ();
			App.Current.EventsBroker.Subscribe<QuitApplicationEvent> (HandleQuitApplicationEvent);
		}

		public static void RegisterServices ()
		{
			if (App.Current.LicenseLimitationsService != null) {
				App.Current.RegisterService (App.Current.LicenseLimitationsService);
			}

			/* Register DB services */
			App.Current.RegisterService (new DataBaseManager ());

			App.Current.RegisterService (new TemplatesService ());

			/* Start the rendering jobs manager */
			jobsManagerVM = new JobsManagerVM {
				Model = new RangeObservableCollection<Job> ()
			};
			App.Current.JobsManager = jobsManagerVM;
			RenderingJobsController jobsController = new RenderingJobsController (jobsManagerVM);
			App.Current.RegisterService (jobsController);

			/* State the tools manager */
			toolsManager = new ToolsManager ();
			App.Current.RegisterService (toolsManager);
			ProjectsImporter = toolsManager;

			/* Register the hotkeys manager */
			App.Current.RegisterService (new HotKeysManager ());
			App.Current.HotkeysService = new HotkeysService ();
			App.Current.RegisterService (App.Current.HotkeysService);

			GeneralUIHotkeys.RegisterDefaultHotkeys ();
			PlaybackHotkeys.RegisterDefaultHotkeys ();
			DrawingToolHotkeys.RegisterDefaultHotkeys ();
			LMGeneralUIHotkeys.RegisterDefaultHotkeys ();
		}

		static void HandleQuitApplicationEvent (QuitApplicationEvent e)
		{
			if (jobsManagerVM.IsBusy) {
				string msg = Catalog.GetString ("A rendering job is running in the background. Do you really want to quit?");
				if (!App.Current.Dialogs.QuestionMessage (msg, null).Result) {
					return;
				}
				jobsManagerVM.CancelAll ();
			}
			App.Current.GUIToolkit.Quit ();
		}
	}
}
