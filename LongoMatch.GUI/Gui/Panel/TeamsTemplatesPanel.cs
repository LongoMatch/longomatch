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
using System.Collections.Specialized;
using System.ComponentModel;
using Gtk;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.States;
using LongoMatch.Services.ViewModel;
using Pango;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (TeamsManagerState.NAME)]
	public partial class TeamsTemplatesPanel : Gtk.Bin, IPanel
	{
		const int COL_TEAM = 0;
		const int COL_EDITABLE = 1;

		ListStore teamsStore;
		TeamsManagerVM viewModel;

		public TeamsTemplatesPanel ()
		{
			this.Build ();

			panelheader1.ApplyVisible = false;
			panelheader1.Title = Title;
			panelheader1.BackClicked += (sender, e) => App.Current.StateController.MoveBack ();

			teamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-team-header", StyleConf.TemplatesHeaderIconSize);
			playerheaderimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-player-header", StyleConf.TemplatesHeaderIconSize);
			newteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-add", StyleConf.TemplatesIconSize);
			importteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-import", StyleConf.TemplatesIconSize);
			exportteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-export", StyleConf.TemplatesIconSize);
			deleteteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-delete", StyleConf.TemplatesIconSize);
			saveteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-save", StyleConf.TemplatesIconSize);
			newplayerimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-add", StyleConf.TemplatesIconSize);
			deleteplayerimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-delete", StyleConf.TemplatesIconSize);
			vseparatorimage.Pixbuf = Helpers.Misc.LoadIcon ("vertical-separator", StyleConf.TemplatesIconSize);

			newteambutton.Entered += HandleEnterTeamButton;
			newteambutton.Left += HandleLeftTeamButton;
			newteambutton.Clicked += HandleNewTeamClicked;
			importteambutton.Entered += HandleEnterTeamButton;
			importteambutton.Left += HandleLeftTeamButton;
			importteambutton.Clicked += HandleImportTeamClicked;
			exportteambutton.Entered += HandleEnterTeamButton;
			exportteambutton.Left += HandleLeftTeamButton;
			exportteambutton.Clicked += HandleExportTeamClicked;
			deleteteambutton.Entered += HandleEnterTeamButton;
			deleteteambutton.Left += HandleLeftTeamButton;
			deleteteambutton.Clicked += HandleDeleteTeamClicked;
			saveteambutton.Entered += HandleEnterTeamButton;
			saveteambutton.Left += HandleLeftTeamButton;
			saveteambutton.Clicked += HandleSaveTeamClicked;

			newplayerbutton1.Entered += HandleEnterPlayerButton;
			newplayerbutton1.Left += HandleLeftPlayerButton;
			newplayerbutton1.Clicked += (object sender, EventArgs e) => {
				teamtemplateeditor1.AddPlayer ();
			};
			deleteplayerbutton.Entered += HandleEnterPlayerButton;
			deleteplayerbutton.Left += HandleLeftPlayerButton;
			deleteplayerbutton.Clicked += (object sender, EventArgs e) => {
				teamtemplateeditor1.DeleteSelectedPlayers ();
			};

			teamsStore = new ListStore (typeof (LMTeamVM));

			var cell = new CellRendererText ();
			cell.Editable = true;
			cell.Edited += HandleEdited;
			teamseditortreeview.Model = teamsStore;
			teamseditortreeview.HeadersVisible = false;
			teamseditortreeview.AppendColumn ("Icon", new CellRendererPixbuf (), RenderIcon);
			teamseditortreeview.AppendColumn ("Text", cell, RenderTemplateName);
			teamseditortreeview.SearchColumn = COL_TEAM;
			teamseditortreeview.EnableGridLines = TreeViewGridLines.None;
			teamseditortreeview.CursorChanged += HandleSelectionChanged;

			teamsvbox.WidthRequest = 280;

			deleteteambutton.Sensitive = false;
			exportteambutton.Sensitive = false;
			saveteambutton.Sensitive = false;
			teamtemplateeditor1.VisibleButtons = false;

			editteamslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));
			editplayerslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));
		}

		public override void Destroy ()
		{
			teamtemplateeditor1.Destroy ();
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
				viewModel = value;
				foreach (LMTeamVM team in viewModel.ViewModels) {
					Add (team);
				}
				viewModel.ViewModels.CollectionChanged += HandleCollectionChanged;
				viewModel.LoadedTemplate.PropertyChanged += HandleLoadedTemplateChanged;
				viewModel.PropertyChanged += HandleViewModelChanged;
				deleteteambutton.Sensitive = viewModel.DeleteSensitive;
				saveteambutton.Sensitive = viewModel.SaveSensitive;
				exportteambutton.Sensitive = viewModel.ExportSensitive;
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

		void RenderIcon (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			LMTeamVM teamVM = (LMTeamVM)model.GetValue (iter, COL_TEAM);
			(cell as CellRendererPixbuf).Pixbuf = teamVM.Icon.Value;
		}

		void RenderTemplateName (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			LMTeamVM teamVM = (LMTeamVM)model.GetValue (iter, COL_TEAM);
			(cell as CellRendererText).Text = teamVM.Name;
		}

		void Add (LMTeamVM teamVM)
		{
			teamsStore.AppendValues (teamVM, teamVM.Editable);
		}

		void Remove (LMTeamVM teamVM)
		{
			TreeIter iter;
			teamsStore.GetIterFirst (out iter);
			while (teamsStore.IterIsValid (iter)) {
				if (teamsStore.GetValue (iter, COL_TEAM) == teamVM) {
					teamsStore.Remove (ref iter);
					break;
				}
				teamsStore.IterNext (ref iter);
			}
		}

		void Select (LMTeamVM teamVM)
		{
			TreeIter iter;
			teamsStore.GetIterFirst (out iter);
			while (teamsStore.IterIsValid (iter)) {
				if ((teamsStore.GetValue (iter, COL_TEAM) as LMTeamVM).Model.Equals (teamVM.Model)) {
					teamseditortreeview.Selection.SelectIter (iter);
					break;
				}
				teamsStore.IterNext (ref iter);
			}
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
				// FIXME: Remove this when the DashboardWidget is ported to the new MVVMC model
				teamtemplateeditor1.Team = ViewModel.LoadedTemplate.Model;
				teamtemplateeditor1.Sensitive = true;
				Select (ViewModel.LoadedTemplate);
			}
		}

		void HandleViewModelChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SaveSensitive") {
				saveteambutton.Sensitive = ViewModel.SaveSensitive;
			} else if (e.PropertyName == "ExportSensitive") {
				exportteambutton.Sensitive = ViewModel.ExportSensitive;
			} else if (e.PropertyName == "DeleteSensitive") {
				deleteteambutton.Sensitive = ViewModel.DeleteSensitive;
			}
		}

		void HandleSaveTeamClicked (object sender, EventArgs e)
		{
			ViewModel.Save (false);
		}

		void HandleDeleteTeamClicked (object sender, EventArgs e)
		{
			ViewModel.DeleteCommand.Execute ();
		}

		void HandleImportTeamClicked (object sender, EventArgs e)
		{
			ViewModel.Import ();
		}

		void HandleExportTeamClicked (object sender, EventArgs e)
		{
			ViewModel.Export ();
		}

		void HandleNewTeamClicked (object sender, EventArgs e)
		{
			ViewModel.NewCommand.Execute ();
		}

		void HandleEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			teamsStore.GetIter (out iter, new TreePath (args.Path));
			var teamVM = teamsStore.GetValue (iter, COL_TEAM) as LMTeamVM;
			ViewModel.ChangeName (teamVM, args.NewText);
			QueueDraw ();
		}
	}
}
