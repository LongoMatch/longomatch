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
using Gtk;
using LongoMatch.Core.Resources;
using LongoMatch.Core.Resources.Styles;
using LongoMatch.Gui.Component;
using LongoMatch.Services.State;
using VAS.Core;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Services.ViewModel;
using VAS.UI.Component;
using Image = VAS.Core.Common.Image;

namespace LongoMatch.Gui.Panel
{
	//FIXME: This Panel should dissapear in favour of PreferencesPanel in VAS.UI, but only when all panels inside this
	//preferences panel are migrated to have a VM of type IPreferencesVM
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (PreferencesState.NAME, 1)]
	public partial class PreferencesPanel : Gtk.Bin, IPanel<PreferencesPanelVM>
	{
		Widget selectedPanel;
		ListStore prefsStore;
		PreferencesPanelVM viewModel;

		public PreferencesPanel ()
		{
			this.Build ();
			prefsStore = new ListStore (typeof (Image), typeof (string), typeof (Widget));
			var imageRenderer = new CellRendererImage ();
			imageRenderer.Width = Sizes.PreferencesIconSize;
			imageRenderer.Height = Sizes.PreferencesIconSize;
			treeview.AppendColumn ("Icon", imageRenderer, "Image", 0);
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
			AddPanel (Catalog.GetString ("General"), App.Current.ResourcesLocator.LoadIcon (Icons.Preferences),
				new GeneralPreferencesPanel ());
			//FIXME: this is a hack, when all preferences panel are migrated to MVVM we should use the PreferencesPanel from VAS
			//Now as we know that in this position there should be the HokteysConfiguration we add it
			AddPanel (ViewModel.ViewModels.Where (p => p is HotkeysConfigurationVM).FirstOrDefault ());
			AddPanel (Catalog.GetString ("Video"), App.Current.ResourcesLocator.LoadIcon (Icons.RecordButton),
				new VideoPreferencesPanel ());
			AddPanel (Catalog.GetString ("Live analysis"), App.Current.ResourcesLocator.LoadIcon (Icons.VideoDevice),
				new LiveAnalysisPreferences ());
			AddPanel (Catalog.GetString ("Plugins"), App.Current.ResourcesLocator.LoadIcon (Icons.Plugin),
				new PluginsPreferences ());
		}

		/// <summary>
		/// Adds the specified panel.
		/// </summary>
		/// <param name="desc">Desc.</param>
		/// <param name="icon">Icon.</param>
		/// <param name="pane">Pane.</param>
		void AddPanel (string desc, Image icon, Widget pane)
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
			var icon = App.Current.ResourcesLocator.LoadIcon (prefViewModel.Icon);
			prefsStore.AppendValues (icon, prefViewModel.Name, view as Widget);
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

