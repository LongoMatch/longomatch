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
using LongoMatch.Services.ViewModel;
using Pango;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using Constants = LongoMatch.Core.Common.Constants;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute ("DashboardsManager")]
	public partial class SportsTemplatesPanel : Gtk.Bin, IPanel, IView
	{
		public event BackEventHandle BackEvent;

		const int COL_DASHBOARD = 0;
		const int COL_EDITABLE = 1;

		ListStore dashboardsStore;
		DashboardsManagerVM viewModel;

		public SportsTemplatesPanel ()
		{
			this.Build ();

			// Assign images
			panelheader1.ApplyVisible = false;
			panelheader1.Title = Catalog.GetString ("ANALYSIS DASHBOARDS MANAGER");
			panelheader1.BackClicked += (sender, o) => {
				ViewModel.Save (false);
				if (BackEvent != null)
					BackEvent ();
			};

			templateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-template-header", StyleConf.TemplatesHeaderIconSize);
			categoryheaderimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-category-header", StyleConf.TemplatesHeaderIconSize);
			newtemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-add", StyleConf.TemplatesIconSize);
			importtemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-import", StyleConf.TemplatesIconSize);
			exporttemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-export", StyleConf.TemplatesIconSize);
			deletetemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-delete", StyleConf.TemplatesIconSize);
			savetemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-save", StyleConf.TemplatesIconSize);
			addcategoryimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-category", StyleConf.TemplatesIconSize);
			addtagimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-tag", StyleConf.TemplatesIconSize);
			scoreimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-score", StyleConf.TemplatesIconSize);
			cardimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-card", StyleConf.TemplatesIconSize);
			timerimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-timer", StyleConf.TemplatesIconSize);
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
			addcategorybutton.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Category");
			addtagbutton1.Entered += HandleEnterTagButton;
			addtagbutton1.Left += HandleLeftTagButton;
			addtagbutton1.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Tag");
			scorebutton.Entered += HandleEnterTagButton;
			scorebutton.Left += HandleLeftTagButton;
			scorebutton.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Score");
			cardbutton.Entered += HandleEnterTagButton;
			cardbutton.Left += HandleLeftTagButton;
			cardbutton.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Card");
			timerbutton.Entered += HandleEnterTagButton;
			timerbutton.Left += HandleLeftTagButton;
			timerbutton.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Timer");

			dashboardsStore = new ListStore (typeof(DashboardVM), typeof(bool));

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
			
			buttonswidget.Sensitive = false;
			buttonswidget.ButtonsVisible = false;
			buttonswidget.Mode = DashboardMode.Edit;
			newtemplatebutton.Visible = true;
			savetemplatebutton.Sensitive = false;
			deletetemplatebutton.Sensitive = false;
			exporttemplatebutton.Sensitive = false;

			newtemplatebutton.Clicked += HandleNewTemplateClicked;
			importtemplatebutton.Clicked += HandleImportTemplateClicked;
			exporttemplatebutton.Clicked += HandleExportTemplateClicked;
			deletetemplatebutton.Clicked += HandleDeleteTemplateClicked;
			savetemplatebutton.Clicked += (sender, e) => ViewModel.Save (true);


			editdashboardslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));
			editbuttonslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));
		}

		public string PanelName {
			get {
				return null;
			}
			set {
			}
		}

		protected override void OnDestroyed ()
		{
			buttonswidget.Destroy ();
			base.OnDestroyed ();
		}

		public DashboardsManagerVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				foreach (DashboardVM dashboard in viewModel.ViewModels) {
					Add (dashboard);
				}
				viewModel.ViewModels.CollectionChanged += HandleCollectionChanged;
				viewModel.LoadedTemplate.PropertyChanged += HandleLoadedTemplateChanged;
				viewModel.PropertyChanged += HandleViewModelChanged;
			}
		}

		public void OnLoad ()
		{

		}

		public void OnUnload ()
		{

		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (DashboardsManagerVM)viewModel;
		}

		void RenderTemplateName (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			string name;

			DashboardVM dashboardVM = (DashboardVM)model.GetValue (iter, COL_DASHBOARD);
			name = dashboardVM.Name;
			if (!dashboardVM.Editable) {
				name += " (" + Catalog.GetString ("System") + ")";
			}
			(cell as CellRendererText).Text = name;
		}

		void Add (DashboardVM dashboardVM)
		{
			dashboardsStore.AppendValues (dashboardVM, dashboardVM.Editable);
		}

		void Remove (DashboardVM dashboardVM)
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

		void Select (DashboardVM dashboardVM)
		{
			TreeIter iter;
			dashboardsStore.GetIterFirst (out iter);
			while (dashboardsStore.IterIsValid (iter)) {
				if ((dashboardsStore.GetValue (iter, COL_DASHBOARD) as DashboardVM).Model.Equals (dashboardVM.Model)) {
					dashboardseditortreeview.Selection.SelectIter (iter);
					break;
				}
				dashboardsStore.IterNext (ref iter);
			}
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
				foreach (DashboardVM dashboardVM in e.NewItems) {
					Add (dashboardVM);
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (DashboardVM dashboardVM in e.OldItems) {
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
			ViewModel.Select (dashboardsStore.GetValue (iter, COL_DASHBOARD) as DashboardVM);
		}

		void HandleLoadedTemplateChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Model") {
				// FIXME: Remove this when the DashboardWidget is ported to the new MVVMC model
				buttonswidget.Template = ViewModel.LoadedTemplate.Model;
				buttonswidget.Sensitive = true;
				Select (ViewModel.LoadedTemplate);
			}
		}

		void HandleViewModelChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SaveSensitive") {
				savetemplatebutton.Sensitive = ViewModel.SaveSensitive;
			} else if (e.PropertyName == "ExportSensitive") {
				exporttemplatebutton.Sensitive = ViewModel.ExportSensitive;
			} else if (e.PropertyName == "DeleteSensitive") {
				deletetemplatebutton.Sensitive = ViewModel.DeleteSensitive;
			}	
		}

		void HandleDeleteTemplateClicked (object sender, EventArgs e)
		{
			ViewModel.Delete ();
		}

		void HandleImportTemplateClicked (object sender, EventArgs e)
		{
			ViewModel.Import ();
		}

		void HandleExportTemplateClicked (object sender, EventArgs e)
		{
			ViewModel.Export ();
		}

		void HandleNewTemplateClicked (object sender, EventArgs e)
		{
			ViewModel.New ();
		}

		void HandleEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			dashboardsStore.GetIter (out iter, new TreePath (args.Path));
			var dashboardVM = dashboardsStore.GetValue (iter, COL_DASHBOARD) as DashboardVM;
			ViewModel.ChangeName (dashboardVM, args.NewText);
			QueueDraw ();
		}
	}
}
