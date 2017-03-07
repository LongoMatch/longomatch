//
//  Copyright (C) 2016 Fluendo S.A.
using System;
using System.ComponentModel;
using LongoMatch.Core.ViewModel;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Events;
using VAS.Services.ViewModel;
using System.Threading.Tasks;

namespace LongoMatch.Services.ViewModel
{
	public class LMProjectAnalysisVM : ProjectAnalysisVM<LMProjectVM>
	{
		public LMProjectAnalysisVM ()
		{
			Project = new LMProjectVM ();
			SaveCommand = new Command (
				() => App.Current.EventsBroker.Publish (new UpdateEvent<LMProjectVM> { Object = Project }),
				() => Project.Edited);
			CloseCommand = new Command (Close);
		}

		public ICapturerBin Capturer {
			get;
			set;
		}

		public CaptureSettings CaptureSettings {
			get;
			set;
		}

		public async Task<bool> Close ()
		{
			return await App.Current.EventsBroker.PublishWithReturn (
			   new CloseEvent<LMProjectVM> { Object = Project });
		}
	}
}
