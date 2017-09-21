//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.Specialized;
using System.ComponentModel;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.States;
using LongoMatch.Services.ViewModel;
using Pango;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.UI.Component;
using VAS.UI.Helpers.Bindings;
using Helpers = VAS.UI.Helpers;
using Image = VAS.Core.Common.Image;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (TeamsManagerState.NAME)]
	public partial class TeamsTemplatesPanel : Gtk.Bin, IPanel<TeamsManagerVM>
	{
		const int COL_TEAM = 0;
		const int COL_EDITABLE = 1;
		const int SHIELD_SIZE = 50;
		ListStore teamsStore;
		TeamsManagerVM viewModel;
		BindingContext ctx;
		List<TeamVM> teams;

		public TeamsTemplatesPanel ()
		{
			this.Build ();

			teams = new List<TeamVM> ();
			panelheader1.ApplyVisible = false;
			panelheader1.Title = Title;
			panelheader1.BackClicked += (sender, e) => App.Current.StateController.MoveBack ();

			teamimage.Image = App.Current.ResourcesLocator.LoadIcon ("lm-team-header", StyleConf.TemplatesHeaderIconSize);
			playerheaderimage.Image = App.Current.ResourcesLocator.LoadIcon ("lm-player-header", StyleConf.TemplatesHeaderIconSize);
			// FIXME: "vertical-separator" has a png extension
			vseparatorimage.Image = new Image (Helpers.Misc.LoadIcon ("lm-vertical-separator", StyleConf.TemplatesIconSize));


			newteambutton.Entered += HandleEnterTeamButton;
			newteambutton.Left += HandleLeftTeamButton;
			importteambutton.Entered += HandleEnterTeamButton;
			importteambutton.Left += HandleLeftTeamButton;
			exportteambutton.Entered += HandleEnterTeamButton;
			exportteambutton.Left += HandleLeftTeamButton;
			deleteteambutton.Entered += HandleEnterTeamButton;
			deleteteambutton.Left += HandleLeftTeamButton;
			saveteambutton.Entered += HandleEnterTeamButton;
			saveteambutton.Left += HandleLeftTeamButton;

			newplayerbutton1.Entered += HandleEnterPlayerButton;
			newplayerbutton1.Left += HandleLeftPlayerButton;

			deleteplayerbutton.Entered += HandleEnterPlayerButton;
			deleteplayerbutton.Left += HandleLeftPlayerButton;

			teamsStore = new ListStore (typeof (LMTeamVM), typeof (bool));

			var cell = new CellRendererText ();
			cell.Editable = true;
			cell.Edited += HandleEdited;
			teamseditortreeview.Model = teamsStore;
			teamseditortreeview.HeadersVisible = false;
			teamseditortreeview.AppendColumn ("Icon", new CellRendererImage (), RenderIcon);
			var col = teamseditortreeview.AppendColumn ("Text", cell, RenderTemplateName);
			col.AddAttribute (cell, "editable", COL_EDITABLE);
			teamseditortreeview.SearchColumn = COL_TEAM;
			teamseditortreeview.EnableGridLines = TreeViewGridLines.None;
			teamseditortreeview.CursorChanged += HandleSelectionChanged;

			teamtemplateeditor1.VisibleButtons = false;

			editteamslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));
			editplayerslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));

			Bind ();
		}

		public override void Destroy ()
		{
			teamtemplateeditor1.Dispose ();
			base.Destroy ();
		}

		public override void Dispose ()
		{
			Destroy ();
			base.Dispose ();
		}

		public string Title {
			get {
				return Catalog.GetString ("TEAMS MANAGER");
			}
		}

		public void OnLoad ()
		{

		}

		public void OnUnload ()
		{

		}

		public TeamsManagerVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.ViewModels.CollectionChanged -= HandleCollectionChanged;
					viewModel.LoadedTemplate.PropertyChanged -= HandleLoadedTemplateChanged;
					foreach (LMTeamVM team in viewModel.ViewModels) {
						Remove (team);
					}
				}
				viewModel = value;
				ctx.UpdateViewModel (viewModel);
				limitationWidget.SetViewModel (viewModel.LimitationChart);
				if (viewModel != null) {
					foreach (LMTeamVM team in viewModel.ViewModels) {
						Add (team);
					}
					viewModel.ViewModels.CollectionChanged += HandleCollectionChanged;
					viewModel.LoadedTemplate.PropertyChanged += HandleLoadedTemplateChanged;
					teamtemplateeditor1.ViewModel = viewModel.TeamEditor;
					teamtemplateeditor1.TeamTagger = viewModel.TeamTagger;
					Select (ViewModel.LoadedTemplate);
				}
			}
		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (TeamsManagerVM)viewModel;
		}

		protected override void OnDestroyed ()
		{
			OnUnload ();
			base.OnDestroyed ();
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (deleteteambutton.Bind (vm => ((TeamsManagerVM)vm).DeleteCommand));
			ctx.Add (newteambutton.Bind (vm => ((TeamsManagerVM)vm).NewCommand));
			ctx.Add (importteambutton.Bind (vm => ((TeamsManagerVM)vm).ImportCommand));
			ctx.Add (exportteambutton.Bind (vm => ((TeamsManagerVM)vm).ExportCommand));
			ctx.Add (saveteambutton.Bind (vm => ((TeamsManagerVM)vm).SaveCommand, true));
			ctx.Add (newplayerbutton1.Bind (vm => ((TeamsManagerVM)vm).TeamEditor.NewPlayerCommand));
			ctx.Add (deleteplayerbutton.Bind (vm => ((TeamsManagerVM)vm).TeamEditor.DeletePlayersCommand));
		}

		void RenderIcon (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			LMTeamVM teamVM = (LMTeamVM)model.GetValue (iter, COL_TEAM);
			CellRendererImage renderer = (cell as CellRendererImage);
			renderer.Width = SHIELD_SIZE;
			renderer.Height = SHIELD_SIZE;
			renderer.Image = teamVM.Icon;
		}

		void RenderTemplateName (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			LMTeamVM teamVM = (LMTeamVM)model.GetValue (iter, COL_TEAM);
			(cell as CellRendererText).Text = teamVM.Name;
		}

		void Add (TeamVM teamVM)
		{
			teamsStore.AppendValues (teamVM, teamVM.Editable);
			teamVM.PropertyChanged += HandleTeamPropertyChanged;
			teams.Add (teamVM);
		}

		void Remove (TeamVM teamVM)
		{
			TreeIter iter;
			if (GetIterFromTeam (teamVM, out iter)) {
				teamsStore.Remove (ref iter);
				teamVM.PropertyChanged -= HandleTeamPropertyChanged;
				teams.Remove (teamVM);
			}
		}

		void Reset ()
		{
			foreach (LMTeamVM teamVM in teams) {
				teamVM.PropertyChanged -= HandleTeamPropertyChanged;
			}
			teamsStore.Clear ();
			foreach (LMTeamVM teamVM in viewModel.ViewModels) {
				Add (teamVM);
			}
		}

		void Select (TeamVM teamVM)
		{
			TreeIter iter;
			if (GetIterFromTeam (teamVM, out iter)) {
				teamseditortreeview.Selection.SelectIter (iter);
			}
		}

		bool GetIterFromTeam (TeamVM team, out TreeIter iter)
		{
			teamsStore.GetIterFirst (out iter);
			while (teamsStore.IterIsValid (iter)) {
				if ((teamsStore.GetValue (iter, COL_TEAM) as TeamVM).Model.Equals (team.Model)) {
					return true;
				}
				teamsStore.IterNext (ref iter);
			}
			return false;
		}

		void HandleEnterTeamButton (object sender, EventArgs e)
		{
			if (sender == newteambutton) {
				editteamslabel.Markup = Catalog.GetString ("New team");
			} else if (sender == exportteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Export team");
			} else if (sender == deleteteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Delete team");
			} else if (sender == saveteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Save team");
			} else if (sender == importteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Import team");
			}
		}

		void HandleLeftTeamButton (object sender, EventArgs e)
		{
			editteamslabel.Markup = Catalog.GetString ("Manage teams");
		}

		void HandleEnterPlayerButton (object sender, EventArgs e)
		{
			if (sender == newplayerbutton1) {
				editplayerslabel.Markup = Catalog.GetString ("New player");
			} else if (sender == deleteplayerbutton) {
				editplayerslabel.Markup = Catalog.GetString ("Delete player");
			}
		}

		void HandleLeftPlayerButton (object sender, EventArgs e)
		{
			editplayerslabel.Markup = Catalog.GetString ("Manage players");
		}

		void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (LMTeamVM teamVM in e.NewItems) {
					Add (teamVM);
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (LMTeamVM teamVM in e.OldItems) {
					Remove (teamVM);
				}
				break;
			case NotifyCollectionChangedAction.Reset:
				Reset (); 
				break;
			case NotifyCollectionChangedAction.Replace:
				QueueDraw ();
				break;
			}
		}

		void HandleSelectionChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			teamseditortreeview.Selection.GetSelected (out iter);
			ViewModel.Select (teamsStore.GetValue (iter, COL_TEAM) as LMTeamVM);
		}

		void HandleLoadedTemplateChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Model") {
				Select (ViewModel.LoadedTemplate);
			}
		}

		void HandleEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			teamsStore.GetIter (out iter, new TreePath (args.Path));
			var teamVM = teamsStore.GetValue (iter, COL_TEAM) as LMTeamVM;
			ViewModel.ChangeName (teamVM, args.NewText);
			QueueDraw ();
		}

		void HandleTeamPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			var teamVM = sender as TeamVM;
			if (teamVM != null && teamVM.NeedsSync (e.PropertyName, "Model")) {
				TreeIter iter;
				if (GetIterFromTeam (teamVM, out iter)) {
					teamsStore.EmitRowChanged (teamsStore.GetPath (iter), iter);
				}
			}
		}
	}
}
