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
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Gui.Component;
using LongoMatch.Services.State;
using VAS.Core;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Services.ViewModel;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Panel
{
	//FIXME: This Panel should dissapear in favour of PreferencesPanel in VAS.UI, but only when all panels inside this
	//preferences panel are migrated to have a VM of type IPreferencesVM
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (PreferencesState.NAME)]
	public partial class PreferencesPanel : Gtk.Bin, IPanel<PreferencesPanelVM>
	{
		Widget selectedPanel;
		ListStore prefsStore;
		PreferencesPanelVM viewModel;

		public PreferencesPanel ()
		{
			this.Build ();
			prefsStore = new ListStore (typeof (Pixbuf), typeof (string), typeof (Widget));
			treeview.AppendColumn ("Icon", new CellRendererPixbuf (), "pixbuf", 0);
			treeview.AppendColumn ("Desc", new CellRendererText (), "text", 1);
			treeview.Selection.Changed += HandleSelectionChanged;
			treeview.Model = prefsStore;
			treeview.HeadersVisible = false;
			treeview.EnableGridLines = TreeViewGridLines.None;
			treeview.EnableTreeLines = false;
			panelheader1.ApplyVisible = false;
			panelheader1.Title = Title;
			panelheader1.BackClicked += (sender, e) => {
				App.Current.StateController.MoveBack ();
			};
		}

		protected override void OnDestroyed ()
		{
			OnUnload ();
			base.OnDestroyed ();
		}

		public override void Dispose ()
		{
			Destroy ();
			base.Dispose ();
		}

		public string Title {
			get {
				return Catalog.GetString ("PREFERENCES");
			}
		}

		public PreferencesPanelVM ViewModel {
			get {
				return viewModel;
			}

			set {
				if (viewModel != null) {
					RemovePanels ();
				}
				viewModel = value;
				if (viewModel != null) {
					AddPanels ();
					//Select First Panel
					treeview.Selection.SelectPath (new TreePath ("0"));
				}
			}
		}

		public void OnLoad ()
		{

		}

		public void OnUnload ()
		{

		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (PreferencesPanelVM)viewModel;
		}

		void AddPanels ()
		{
			AddPanel (Catalog.GetString ("General"),
				Helpers.Misc.LoadIcon ("lm-preferences", IconSize.Dialog, 0),
				new GeneralPreferencesPanel ());
			//FIXME: this is a hack, when all preferences panel are migrated to MVVM we should use the PreferencesPanel from VAS
			//Now as we know that in this position there should be the HokteysConfiguration we add it
			AddPanel (ViewModel.ViewModels.Where (p => p is HotkeysConfigurationVM).FirstOrDefault ());
			AddPanel (Catalog.GetString ("Video"),
				Helpers.Misc.LoadIcon ("vas-record", IconSize.Dialog, 0),
				new VideoPreferencesPanel ());
			AddPanel (Catalog.GetString ("Live analysis"),
				Helpers.Misc.LoadIcon ("vas-video-device", IconSize.Dialog, 0),
				new LiveAnalysisPreferences ());
			AddPanel (Catalog.GetString ("Plugins"),
				Helpers.Misc.LoadIcon ("vas-plugin", IconSize.Dialog, 0),
				new PluginsPreferences ());
		}

		/// <summary>
		/// Adds the specified panel.
		/// </summary>
		/// <param name="desc">Desc.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="pane">Pane.</param>
		void AddPanel (string desc, Pixbuf icon, Widget pane)
		{
			prefsStore.AppendValues (icon, desc, pane);
		}

		/// <summary>
		/// Adds a preference panel, by passing a IPreferencesVM ViewModel
		/// </summary>
		/// <param name="prefViewModel">Preference view model.</param>
		void AddPanel (IPreferencesVM prefViewModel)
		{
			IView view = App.Current.ViewLocator.Retrieve (prefViewModel.View);
			view.SetViewModel (prefViewModel);
			var icon = App.Current.ResourcesLocator.LoadIcon (prefViewModel.Icon).Value;
			prefsStore.AppendValues (icon, prefViewModel.Name,
									 view as Widget);
		}

		void RemovePanels ()
		{
			prefsStore.Foreach ((model, path, iter) => prefsStore.Remove (ref iter));
		}

		void HandleSelectionChanged (object sender, EventArgs e)
		{
			if (viewModel == null) {
				return;
			}

			Widget newPanel;
			TreeIter iter;

			if (selectedPanel != null)
				propsvbox.Remove (selectedPanel);

			treeview.Selection.GetSelected (out iter);
			newPanel = prefsStore.GetValue (iter, 2) as Widget;
			newPanel.Visible = true;
			propsvbox.PackStart (newPanel, true, true, 0);
			selectedPanel = newPanel;
		}
	}
}

