//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;

namespace LongoMatch.Services.ViewModel
{
	/// <summary>
	/// ViewModel to edit a team.
	/// </summary>
	public class LMTeamEditorVM : ViewModelBase
	{
		public LMTeamEditorVM ()
		{
			NewPlayerCommand = new AsyncCommand (CreatePlayer, () => Team.Model != null);
			DeletePlayersCommand = new AsyncCommand (DeletePlayers, () => Team.Selection.Any ());
			NewPlayerCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("longomatch-add", StyleConf.TemplatesIconSize);
			DeletePlayersCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("longomatch-delete", StyleConf.TemplatesIconSize);
		}

		public LMTeamVM Team {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the new player command.
		/// </summary>
		/// <value>The new player command.</value>
		[PropertyChanged.DoNotNotify]
		public Command NewPlayerCommand {
			get;
			protected set;
		}

		/// <summary>
		/// Gets or sets the delete players command.
		/// </summary>
		/// <value>The delete players command.</value>
		[PropertyChanged.DoNotNotify]
		public Command DeletePlayersCommand {
			get;
			protected set;
		}

		async Task CreatePlayer ()
		{
			await App.Current.EventsBroker.Publish (new CreateEvent<LMPlayer> ());
		}

		async Task DeletePlayers ()
		{
			await App.Current.EventsBroker.Publish (new DeleteEvent<LMPlayer> ());
		}
	}
}
