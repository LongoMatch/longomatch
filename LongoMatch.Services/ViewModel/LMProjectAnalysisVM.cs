//
//  Copyright (C) 2016 Fluendo S.A.
using LongoMatch.Core.ViewModel;
using VAS.Core.MVVMC;
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
				() => App.Current.EventsBroker.Publish (new SaveEvent<LMProjectVM> { Object = Project }),
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

		async Task<bool> Close ()
		{
			return await App.Current.EventsBroker.PublishWithReturn (
			   new CloseEvent<LMProjectVM> { Object = Project });
		}
	}
}
