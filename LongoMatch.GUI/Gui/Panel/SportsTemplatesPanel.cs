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
using VAS.UI.Helpers.Bindings;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (DashboardsManagerState.NAME)]
	public partial class SportsTemplatesPanel : Gtk.Bin, IPanel
	{
		const int COL_DASHBOARD = 0;
		const int COL_EDITABLE = 1;

		ListStore dashboardsStore;
		DashboardsManagerVM viewModel;
		BindingContext ctx;

		public SportsTemplatesPanel ()
		{
			this.Build ();

			Bind ();

			// Assign images
			panelheader1.ApplyVisible = false;
			panelheader1.Title = Title;
			panelheader1.BackClicked += (sender, e) => App.Current.StateController.MoveBack ();

			templateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-template-header", StyleConf.TemplatesHeaderIconSize);
			categoryheaderimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-category-header", StyleConf.TemplatesHeaderIconSize);
			vseparatorimage.Pixbuf = Helpers.Misc.LoadIcon ("vertical-separator", StyleConf.TemplatesIconSize);

			// Connect buttons from the bar
			newtemplatebutton.Entered += HandleEnterTemplateButton;
			newtemplatebutton.Left += HandleLeftTemplateButton;
			importtemplatebutton.Entered += HandleEnterTemplateButton;
			importtemplatebutton.Left += HandleLeftTemplateButton;
			exporttemplatebutton.Entered += HandleEnterTemplateButton;
			exporttemplatebutton.Left += HandleLeftTemplateButton;
			deletetemplatebutton.Entered += HandleEnterTemplateButton;
			deletetemplatebutton.Left += HandleLeftTemplateButton;
			savetemplatebutton.Entered += HandleEnterTemplateButton;
			savetemplatebutton.Left += HandleLeftTemplateButton;

			addcategorybutton.Entered += HandleEnterTagButton;
			addcategorybutton.Left += HandleLeftTagButton;
			addtagbutton1.Entered += HandleEnterTagButton;
			addtagbutton1.Left += HandleLeftTagButton;
			scorebutton.Entered += HandleEnterTagButton;
			scorebutton.Left += HandleLeftTagButton;
			cardbutton.Entered += HandleEnterTagButton;
			cardbutton.Left += HandleLeftTagButton;
			timerbutton.Entered += HandleEnterTagButton;
			timerbutton.Left += HandleLeftTagButton;

			dashboardsStore = new ListStore (typeof (LMDashboardVM), typeof (bool));

			// Connect treeview with Model and configure
			dashboardseditortreeview.Model = dashboardsStore;
			dashboardseditortreeview.HeadersVisible = false;
			var cell = new CellRendererText { SizePoints = 14.0 };
			//cell.Editable = true;
			cell.Edited += HandleEdited;
			var col = dashboardseditortreeview.AppendColumn ("Text", cell, RenderTemplateName);
			col.AddAttribute (cell, "editable", COL_EDITABLE);
			dashboardseditortreeview.SearchColumn = COL_DASHBOARD;
			dashboardseditortreeview.EnableGridLines = TreeViewGridLines.None;
			dashboardseditortreeview.CursorChanged += HandleSelectionChanged;

			templatesvbox.WidthRequest = 160;

			dashboardwidget.Sensitive = false;
			dashboardwidget.CodingDashboardMode = false;
			newtemplatebutton.Visible = true;
			savetemplatebutton.Sensitive = false;
			deletetemplatebutton.Sensitive = false;
			exporttemplatebutton.Sensitive = false;

			editdashboardslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));
			editbuttonslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));
		}

		protected override void OnDestroyed ()
		{
			ctx.Dispose ();
			OnUnload ();
			dashboardwidget.Destroy ();
			base.OnDestroyed ();
		}

		public override void Dispose ()
		{
			Destroy ();
			base.Dispose ();
		}

		public string Title {
			get {
				return Catalog.GetString ("ANALYSIS DASHBOARDS MANAGER");
			}
		}

		public DashboardsManagerVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.ViewModels.CollectionChanged -= HandleCollectionChanged;
					viewModel.LoadedTemplate.PropertyChanged -= HandleLoadedTemplateChanged;
				}
				viewModel = value;
				ctx.UpdateViewModel (viewModel);
				if (viewModel != null) {
					foreach (LMDashboardVM dashboard in viewModel.ViewModels) {
						Add (dashboard);
					}
					viewModel.ViewModels.CollectionChanged += HandleCollectionChanged;
					viewModel.LoadedTemplate.PropertyChanged += HandleLoadedTemplateChanged;
					viewModel.LoadedTemplate.Mode = DashboardMode.Edit;
					dashboardwidget.ViewModel = viewModel.LoadedTemplate;
					UpdateLoadedTemplate ();
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
			ViewModel = (DashboardsManagerVM)viewModel;
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (addcategorybutton.BindWithIcon (
				App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-category", StyleConf.TemplatesIconSize),
				vm => ((DashboardsManagerVM)vm).AddButton, "Category"));
			ctx.Add (scorebutton.BindWithIcon (
				App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-score", StyleConf.TemplatesIconSize),
				vm => ((DashboardsManagerVM)vm).AddButton, "Score"));
			ctx.Add (timerbutton.BindWithIcon (
				App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-timer", StyleConf.TemplatesIconSize),
				vm => ((DashboardsManagerVM)vm).AddButton, "Timer"));
			ctx.Add (addtagbutton1.BindWithIcon (
				App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-tag", StyleConf.TemplatesIconSize),
				vm => ((DashboardsManagerVM)vm).AddButton, "Tag"));
			ctx.Add (cardbutton.BindWithIcon (
				App.Current.ResourcesLocator.LoadIcon ("longomatch-tag-tag", StyleConf.TemplatesIconSize),
				vm => ((DashboardsManagerVM)vm).AddButton, "Card"));

			ctx.Add (deletetemplatebutton.Bind (vm => ((DashboardsManagerVM)vm).DeleteCommand));
			ctx.Add (newtemplatebutton.Bind (vm => ((DashboardsManagerVM)vm).NewCommand));
			ctx.Add (importtemplatebutton.Bind (vm => ((DashboardsManagerVM)vm).ImportCommand));
			ctx.Add (exporttemplatebutton.Bind (vm => ((DashboardsManagerVM)vm).ExportCommand));
			ctx.Add (savetemplatebutton.Bind (vm => ((DashboardsManagerVM)vm).SaveCommand, true));
		}

		void RenderTemplateName (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			string name;

			LMDashboardVM dashboardVM = (LMDashboardVM)model.GetValue (iter, COL_DASHBOARD);
			name = dashboardVM.Name;
			if (!dashboardVM.Editable) {
				name += " (" + Catalog.GetString ("System") + ")";
			}
			(cell as CellRendererText).Text = name;
		}

		void Add (LMDashboardVM dashboardVM)
		{
			dashboardsStore.AppendValues (dashboardVM, dashboardVM.Editable);
		}

		void Remove (LMDashboardVM dashboardVM)
		{
			TreeIter iter;
			dashboardsStore.GetIterFirst (out iter);
			while (dashboardsStore.IterIsValid (iter)) {
				if (dashboardsStore.GetValue (iter, COL_DASHBOARD) == dashboardVM) {
					dashboardsStore.Remove (ref iter);
					break;
				}
				dashboardsStore.IterNext (ref iter);
			}
		}

		void Select (LMDashboardVM dashboardVM)
		{
			TreeIter iter;
			dashboardsStore.GetIterFirst (out iter);
			while (dashboardsStore.IterIsValid (iter)) {
				if ((dashboardsStore.GetValue (iter, COL_DASHBOARD) as LMDashboardVM).Model.Equals (dashboardVM.Model)) {
					dashboardseditortreeview.Selection.SelectIter (iter);
					break;
				}
				dashboardsStore.IterNext (ref iter);
			}
		}

		void UpdateLoadedTemplate ()
		{
			dashboardwidget.Sensitive = true;
			Select (ViewModel.LoadedTemplate);
		}

		void HandleEnterTemplateButton (object sender, EventArgs e)
		{
			if (sender == newtemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("New dashboard");
			} else if (sender == importtemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("Import dashboard");
			} else if (sender == exporttemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("Export dashboard");
			} else if (sender == deletetemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("Delete dashboard");
			} else if (sender == savetemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("Save dashboard");
			}
		}

		void HandleLeftTemplateButton (object sender, EventArgs e)
		{
			editdashboardslabel.Markup = Catalog.GetString ("Manage dashboards");
		}

		void HandleEnterTagButton (object sender, EventArgs e)
		{
			if (sender == addcategorybutton) {
				editbuttonslabel.Markup = Catalog.GetString ("Add category button");
			} else if (sender == addtagbutton1) {
				editbuttonslabel.Markup = Catalog.GetString ("Add tag button");
			} else if (sender == scorebutton) {
				editbuttonslabel.Markup = Catalog.GetString ("Add score button");
			} else if (sender == timerbutton) {
				editbuttonslabel.Markup = Catalog.GetString ("Add timer button");
			} else if (sender == cardbutton) {
				editbuttonslabel.Markup = Catalog.GetString ("Add card button");
			}
		}

		void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach (LMDashboardVM dashboardVM in e.NewItems) {
					Add (dashboardVM);
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (LMDashboardVM dashboardVM in e.OldItems) {
					Remove (dashboardVM);
				}
				break;
			case NotifyCollectionChangedAction.Replace:
				QueueDraw ();
				break;
			}
		}

		void HandleLeftTagButton (object sender, EventArgs e)
		{
			editbuttonslabel.Markup = Catalog.GetString ("Manage dashboard buttons");
		}

		void HandleSelectionChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			dashboardseditortreeview.Selection.GetSelected (out iter);
			ViewModel.Select (dashboardsStore.GetValue (iter, COL_DASHBOARD) as LMDashboardVM);
		}

		void HandleLoadedTemplateChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Model") {
				UpdateLoadedTemplate ();
			}
		}

		void HandleEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			dashboardsStore.GetIter (out iter, new TreePath (args.Path));
			var dashboardVM = dashboardsStore.GetValue (iter, COL_DASHBOARD) as LMDashboardVM;
			ViewModel.ChangeName (dashboardVM, args.NewText);
			QueueDraw ();
		}
	}
}
