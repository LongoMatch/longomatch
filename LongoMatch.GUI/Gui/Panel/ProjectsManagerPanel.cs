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
using System.Linq;
using Gtk;
using LongoMatch.Core.ViewModel;
using LongoMatch.Gui.Component;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using Pango;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.UI.Helpers.Bindings;
using VAS.UI.Bindings;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (ProjectsManagerState.NAME)]
	public partial class ProjectsManagerPanel : Gtk.Bin, IPanel<SportsProjectsManagerVM>
	{
		SportsProjectsManagerVM viewModel;
		List<VideoFileInfo> videoFileInfos;
		BindingContext ctx;
		BindingContext detailCtx;

		public ProjectsManagerPanel ()
		{
			this.Build ();

			this.videoFileInfos = new List<VideoFileInfo> ();

			savebuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-save", 34);
			exportbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("lm-export", 34);
			resyncbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("lm-project-resync", 34);
			deletebuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-delete", 34);
			openbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-open", 34);

			// Force tooltips to be translatable as there seems to be a bug in stetic 
			// code generation for translatable tooltips.
			savebutton.TooltipMarkup = Catalog.GetString ("Save");
			exportbutton.TooltipMarkup = Catalog.GetString ("Export");
			openbutton.TooltipMarkup = Catalog.GetString ("Open");
			deletebutton.TooltipMarkup = Catalog.GetString ("Delete");

			panelheader1.Title = Title;
			panelheader1.ApplyVisible = false;
			panelheader1.BackClicked += HandleBackClicked;

			resyncbutton.Visible = true;

			SetStyle ();
			Bind ();
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected override void OnDestroyed ()
		{
			Log.Verbose ($"Destroying {GetType ()}");

			ViewModel = null;
			ctx.Dispose ();
			ctx = null;

			base.OnDestroyed ();

			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		public string Title {
			get {
				return Catalog.GetString ("PROJECTS MANAGER");
			}
		}

		public SportsProjectsManagerVM ViewModel {
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
					viewModel.ViewModels.CollectionChanged -= HandleViewModelsCollectionChanged;
				}
				viewModel = value;
				projectlistwidget1.SetViewModel (viewModel);
				ctx.UpdateViewModel (viewModel);
				if (viewModel != null) {
					viewModel.PropertyChanged += HandleViewModelPropertyChanged;
					viewModel.ViewModels.CollectionChanged += HandleViewModelsCollectionChanged;
				}
			}
			get {
				return viewModel;
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
			ViewModel = (SportsProjectsManagerVM)viewModel;
		}

		void SetStyle ()
		{
			FontDescription desc = FontDescription.FromString (App.Current.Style.Font + " 18");
			infoeventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.ThemeBase));
			infolabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextBase));
			infolabel.ModifyFont (desc);
			videoseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.ThemeBase));
			videoslabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.TextBase));
			videoslabel.ModifyFont (desc);
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (ViewModel.LoadedProject) && sender == ViewModel) {
				LoadProject (ViewModel.LoadedProject);
			}
		}

		void HandleViewModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			rbox.Visible = ViewModel.ViewModels.Any ();
		}

		void LoadProject (LMProjectVM project)
		{
			gamedescriptionheader1.ViewModel = project;
			detailCtx.UpdateViewModel (project);

			foreach (VideoFileInfo vfi in videoFileInfos) {
				videofileinfo_vbox.Remove (vfi);
			}
			videoFileInfos.Clear ();

			foreach (var mf in project.FileSet) {
				VideoFileInfo vfi = new VideoFileInfo ();

				vfi.SetMediaFileSet (project.FileSet, mf);
				vfi.Changed += HandleChanged;

				vfi.ShowAll ();

				videoFileInfos.Add (vfi);

				videofileinfo_vbox.PackStart (vfi, true, true, 0);
			}

			rbox.Visible = true;
		}

		void HandleBackClicked (object sender, EventArgs e)
		{
			App.Current.StateController.MoveBack ();
		}

		void HandleChanged (object sender, EventArgs e)
		{
			if (ViewModel.LoadedProject == null) {
				return;
			}

			ViewModel.SaveCommand.EmitCanExecuteChanged ();
		}


		void Bind ()
		{
			ctx = this.GetBindingContext ();
			detailCtx = new BindingContext ();
			ctx.Add (deletebutton.Bind (vm => ((SportsProjectsManagerVM)vm).DeleteCommand));
			ctx.Add (openbutton.Bind (vm => ((SportsProjectsManagerVM)vm).OpenCommand));
			ctx.Add (exportbutton.Bind (vm => ((SportsProjectsManagerVM)vm).ExportCommand));
			ctx.Add (savebutton.Bind (vm => ((SportsProjectsManagerVM)vm).SaveCommand));
			ctx.Add (resyncbutton.Bind (vm => ((SportsProjectsManagerVM)vm).ResyncCommand));
			detailCtx.Add (seasonentry.Bind (vm => ((LMProjectVM)vm).Season));
			detailCtx.Add (competitionentry.Bind (vm => ((LMProjectVM)vm).Competition));
			detailCtx.Add (datepicker.Bind (vm => ((LMProjectVM)vm).MatchDate));
			detailCtx.Add (templatelabel.Bind (vm => ((LMProjectVM)vm).DashboardText));
			detailCtx.Add (desctextview.Bind (vm => ((LMProjectVM)vm).Description));
		}
	}
}
