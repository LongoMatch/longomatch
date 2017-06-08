// ButtonsWidget.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Gtk;
using LongoMatch.Gui.Dialog;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using VAS.UI.Helpers;
using VAS.UI.Helpers.Bindings;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	[View ("DashboardView")]
	public partial class DashboardWidget : Gtk.Bin, IView<DashboardVM>
	{
		const int PROPERTIES_NOTEBOOK_PAGE_EMPTY = 0;
		const int PROPERTIES_NOTEBOOK_PAGE_TAGS = 1;
		const int PROPERTIES_NOTEBOOK_PAGE_LINKS = 2;

		public event NewEventHandler NewTagEvent;

		DashboardCanvas tagger;
		DashboardVM viewModel;
		bool internalButtons;
		BindingContext ctx;

		public DashboardWidget ()
		{
			this.Build ();

			applyimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-apply", IconSize.Button);

			tagger = new DashboardCanvas (new WidgetWrapper (drawingarea));
			tagger.ShowMenuEvent += HandleShowMenuEvent;
			tagger.NewTagEvent += HandleNewTagEvent;
			tagger.EditButtonTagsEvent += HandleEditEventSubcategories;
			tagger.ActionLinksSelectedEvent += HandleActionLinksSelectedEvent;
			tagger.ActionLinkCreatedEvent += HandleActionLinkCreatedEvent;
			drawingarea.CanFocus = true;
			fieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			hfieldeventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			goaleventbox.ButtonPressEvent += HandleFieldButtonPressEvent;
			applybutton.Clicked += HandleApplyClicked;

			positionsbox.NoShowAll = true;
			periodsbox.NoShowAll = true;
			hbuttonbox2.NoShowAll = true;

			ConfigureToolbar ();
			// Initialize to the empty notebook page.
			propertiesnotebook.Page = PROPERTIES_NOTEBOOK_PAGE_EMPTY;

			CodingDashboardMode = false;

			Bind ();

		}

		protected override void OnDestroyed ()
		{
			ctx.Dispose ();
			tagger.Dispose ();
			base.OnDestroyed ();
		}

		public DashboardVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				tagger.ViewModel = value;
				// Start with disabled widget until something get selected
				tagproperties.Tagger = null;
				propertiesnotebook.Page = PROPERTIES_NOTEBOOK_PAGE_EMPTY;
				tagproperties.Dashboard = value.Model;
				popupbutton.Active = !value.DisablePopupWindow;
				viewModel.FitMode = FitMode.Fit;
				fitbutton.Active = true;
				viewModel.PropertyChanged += HandleViewModelPropertyChanged;

				ctx.UpdateViewModel (viewModel);
				viewModel.Sync ();
			}
		}

		public bool CodingDashboardMode {
			set {
				positionsbox.Visible = !value;
				periodsbox.Visible = !value;
				internalButtons = value;
			}
		}

		public Time CurrentTime {
			set {
				ViewModel.CurrentTime = value;
			}
		}

		public bool LinksButtonVisible {
			set {
				if (!App.Current.SupportsActionLinks)
					linksbutton.Visible = false;
				else
					linksbutton.Visible = value;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (DashboardVM)viewModel;
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();

			ctx.Add (fieldimage.Bind (vm => ((DashboardVM)vm).FieldBackground, 50, 50));
			ctx.Add (hfieldimage.Bind (vm => ((DashboardVM)vm).HalfFieldBackground, 50, 50));
			ctx.Add (goalimage.Bind (vm => ((DashboardVM)vm).GoalBackground, 50, 50));

			ctx.Add (addcatbutton.BindWithIcon (App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-category", StyleConf.NotebookTabSize),
												vm => ((DashboardVM)vm).AddButton, "Category"));
			ctx.Add (addscorebutton.BindWithIcon (App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-score", StyleConf.NotebookTabSize),
												  vm => ((DashboardVM)vm).AddButton, "Score"));
			ctx.Add (addtimerbutton.BindWithIcon (App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-timer", StyleConf.NotebookTabSize),
												  vm => ((DashboardVM)vm).AddButton, "Timer"));
			ctx.Add (addtagbutton.BindWithIcon (App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-tag", StyleConf.NotebookTabSize),
												   vm => ((DashboardVM)vm).AddButton, "Tag"));
			ctx.Add (addcardbutton.BindWithIcon (App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-tag", StyleConf.NotebookTabSize),
												 vm => ((DashboardVM)vm).AddButton, "Card"));

			ctx.Add (resetfieldbutton.Bind (vm => ((DashboardVM)vm).ResetField, FieldPositionType.Field));
			ctx.Add (resethfieldbutton.Bind (vm => ((DashboardVM)vm).ResetField, FieldPositionType.HalfField));
			ctx.Add (resetgoalbutton.Bind (vm => ((DashboardVM)vm).ResetField, FieldPositionType.Goal));

			ctx.Add (editbutton.Bind (vm => ((DashboardVM)vm).ChangeDashboardMode, DashboardMode.Edit, DashboardMode.Code));
			ctx.Add (linksbutton.Bind (vm => ((DashboardVM)vm).ToggleActionLinks, true, false));
			ctx.Add (popupbutton.Bind (vm => ((DashboardVM)vm).TogglePopupWindow, true, false));
			ctx.Add (fitbutton.BindWithIcon (App.Current.ResourcesLocator.LoadIcon ("longomatch-dash-fit", 22),
											 vm => ((DashboardVM)vm).ChangeFitMode, FitMode.Fit));
			ctx.Add (fillbutton.BindWithIcon (App.Current.ResourcesLocator.LoadIcon ("longomatch-dash-fill", 22),
											  vm => ((DashboardVM)vm).ChangeFitMode, FitMode.Fill));
			ctx.Add (d11button.BindWithIcon (App.Current.ResourcesLocator.LoadIcon ("longomatch-dash-11", 22),
											 vm => ((DashboardVM)vm).ChangeFitMode, FitMode.Original));
		}

		void ConfigureToolbar ()
		{
			fitbutton.TooltipText = Catalog.GetString ("Fit dashboard");
			fillbutton.TooltipText = Catalog.GetString ("Fill dashboard");
			d11button.TooltipText = Catalog.GetString ("1:1 dashboard");
			ButtonHelper.LinkToggleButtons (fitbutton, fillbutton, d11button);
			editbutton.Active = true;
			linksbutton.Active = false;
		}

		void UpdateFitMode ()
		{
			if (ViewModel.FitMode == FitMode.Original) {
				dashscrolledwindow.HscrollbarPolicy = PolicyType.Automatic;
				dashscrolledwindow.VscrollbarPolicy = PolicyType.Automatic;
			} else {
				drawingarea.WidthRequest = -1;
				drawingarea.HeightRequest = -1;
				dashscrolledwindow.HscrollbarPolicy = PolicyType.Never;
				dashscrolledwindow.VscrollbarPolicy = PolicyType.Never;
			}
		}

		void UpdateSelection ()
		{
			if (viewModel.Selection.Count == 1) {
				tagproperties.Tagger = viewModel.Selection [0].Model;
				propertiesnotebook.Page = PROPERTIES_NOTEBOOK_PAGE_TAGS;
			} else {
				tagproperties.Tagger = null;
				propertiesnotebook.Page = PROPERTIES_NOTEBOOK_PAGE_EMPTY;
			}
		}

		void HandleActionLinksSelectedEvent (List<ActionLinkVM> actionLinks)
		{
			if (actionLinks.Count == 1) {
				linkproperties.Link = actionLinks [0].Model;
				propertiesnotebook.Page = PROPERTIES_NOTEBOOK_PAGE_LINKS;
			} else {
				propertiesnotebook.Page = PROPERTIES_NOTEBOOK_PAGE_EMPTY;
			}
		}

		void HandleNewTagEvent (EventType evntType, List<Player> players, ObservableCollection<Team> teams, List<Tag> tags,
								Time start, Time stop, Time eventTime, DashboardButton btn)
		{
			/* Forward event until we have players integrted in the dashboard layout */
			if (NewTagEvent != null) {
				NewTagEvent (evntType, players, teams, tags, start, stop, eventTime, btn);
			}
		}

		void HandleFieldButtonPressEvent (object sender, EventArgs e)
		{
			if (sender == fieldeventbox) {
				ViewModel.ChangeField.Execute (FieldPositionType.Field);
			} else if (sender == hfieldeventbox) {
				ViewModel.ChangeField.Execute (FieldPositionType.HalfField);
			} else if (sender == goaleventbox) {
				ViewModel.ChangeField.Execute (FieldPositionType.Goal);
			}
		}

		void HandleApplyClicked (object sender, EventArgs e)
		{
			try {
				ViewModel.GamePeriods = new ObservableCollection<string> (periodsentry.Text.Split ('-'));
			} catch {
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Could not parse game periods."));
			}
		}

		void HandleActionLinkCreatedEvent (ActionLinkVM actionLink)
		{
			//			if (template.HasCircularDependencies ()) {
			//				Config.GUIToolkit.ErrorMessage (Catalog.GetString (
			//					"This linking option is not valid: infinite loop."));
			//				RemoveLink (actionLink, true);
			//			}
			HandleActionLinksSelectedEvent (new List<ActionLinkVM> { actionLink });
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e, nameof (DashboardVM.GamePeriods))) {
				periodsentry.Text = string.Join ("-", viewModel.Model.GamePeriods);
			}
			if (ViewModel.NeedsSync (e, nameof (DashboardVM.FitMode))) {
				UpdateFitMode ();
			}
			if (ViewModel.NeedsSync (e, nameof (DashboardVM.Mode))) {
				// Add buttons for cards/tags/etc.. can be handled remotely.
				hbuttonbox2.Visible = ViewModel.Mode == DashboardMode.Edit && internalButtons;
				LinksButtonVisible = editbutton.Active = rightbox.Visible = ViewModel.Mode == DashboardMode.Edit;
				Helpers.Misc.SetFocus (this, ViewModel.Mode == DashboardMode.Edit);
			}
			if (ViewModel.NeedsSync (e, nameof (DashboardVM.ShowLinks))) {
				propertiesnotebook.Page = PROPERTIES_NOTEBOOK_PAGE_EMPTY;
			}
			if (ViewModel.NeedsSync (e, $"Collection_{nameof (DashboardVM.Selection)}")) {
				UpdateSelection ();
			}
		}

		void HandleShowMenuEvent (List<DashboardButtonVM> buttons, List<ActionLinkVM> links)
		{
			Menu menu;
			MenuItem delbut, dupbut;

			if (ViewModel.Mode != DashboardMode.Edit) {
				return;
			}

			if (buttons.Count == 0 && links.Count == 0) {
				return;
			}

			menu = new Menu ();
			foreach (DashboardButtonVM button in buttons) {
				delbut = ViewModel.DeleteButton.CreateMenuItem ($"{ViewModel.DeleteButton.Text}: {button.Name}",
																commandParam: button);
				menu.Add (delbut);
				dupbut = ViewModel.DuplicateButton.CreateMenuItem ($"{ViewModel.DuplicateButton.Text}: {button.Name}",
																   commandParam: button);
				menu.Add (dupbut);
			}

			foreach (ActionLinkVM link in links) {
				delbut = ViewModel.DeleteLink.CreateMenuItem ($"{ViewModel.DeleteLink.Text}: {link.Name}", commandParam: link);
				menu.Add (delbut);
			}
			menu.ShowAll ();
			menu.Popup ();
		}

		void HandleEditEventSubcategories (DashboardButton dashboardButton)
		{
			AnalysisEventButton button = (dashboardButton as AnalysisEventButton);
			AnalysisEventType evt = button.AnalysisEventType;
			EventTypeTagsEditor dialog = new EventTypeTagsEditor (this.Toplevel as Window);
			dialog.EventType = evt;
			dialog.Run ();
			dialog.Destroy ();
			ViewModel.Model.RemoveDeadLinks (button);
		}
	}
}
