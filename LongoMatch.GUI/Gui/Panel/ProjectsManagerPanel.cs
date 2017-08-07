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

			seasonentry.Changed += HandleChanged;
			competitionentry.Changed += HandleChanged;

			datepicker.ValueChanged += HandleDateChanged;
			desctextview.Buffer.Changed += HandleChanged;

			panelheader1.Title = Title;
			panelheader1.ApplyVisible = false;
			panelheader1.BackClicked += HandleBackClicked;

			// Only visible when multi camera is supported. Indeed periods can be edited in the timeline of the project.
			resyncbutton.Visible = App.Current.SupportsMultiCamera;

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
			infoeventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.PaletteBackgroundDark));
			infolabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.PaletteText));
			infolabel.ModifyFont (desc);
			videoseventbox.ModifyBg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.PaletteBackgroundDark));
			videoslabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.PaletteText));
			videoslabel.ModifyFont (desc);
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (ViewModel.LoadedProject.Model) && sender == ViewModel.LoadedProject) {
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
			seasonentry.Text = project.Season;
			competitionentry.Text = project.Competition;
			datepicker.Date = project.MatchDate;
			templatelabel.Text = project.DashboardText;
			desctextview.Buffer.Clear ();
			desctextview.Buffer.InsertAtCursor (project.Description ?? "");

			foreach (VideoFileInfo vfi in videoFileInfos) {
				videofileinfo_vbox.Remove (vfi);
			}
			videoFileInfos.Clear ();

			resyncbutton.Sensitive = project.FileSet.Count () > 1;

			int max = project.FileSet.Count ();
			// Cap to one media file for non multi camera version
			if (!App.Current.SupportsMultiCamera) {
				max = Math.Min (max, 1);
			}

			foreach (var mf in project.FileSet.Take (max)) {
				VideoFileInfo vfi = new VideoFileInfo ();

				vfi.SetMediaFileSet (project.FileSet.Model, mf.Model);
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
			if (ViewModel.LoadedProject == null)
				return;

			if (sender == competitionentry) {
				ViewModel.LoadedProject.Competition = (sender as Entry).Text;
			} else if (sender == seasonentry) {
				ViewModel.LoadedProject.Season = (sender as Entry).Text;
			} else if (sender == desctextview.Buffer) {
				ViewModel.LoadedProject.Description =
					desctextview.Buffer.GetText (desctextview.Buffer.StartIter,
					desctextview.Buffer.EndIter, true);
			}
			ViewModel.SaveCommand.EmitCanExecuteChanged ();
		}

		void HandleDateChanged (object sender, EventArgs e)
		{
			if (ViewModel.LoadedProject == null)
				return;

			ViewModel.LoadedProject.MatchDate = datepicker.Date;
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (deletebutton.Bind (vm => ((SportsProjectsManagerVM)vm).DeleteCommand));
			ctx.Add (openbutton.Bind (vm => ((SportsProjectsManagerVM)vm).OpenCommand));
			ctx.Add (exportbutton.Bind (vm => ((SportsProjectsManagerVM)vm).ExportCommand));
			ctx.Add (savebutton.Bind (vm => ((SportsProjectsManagerVM)vm).SaveCommand));
			ctx.Add (resyncbutton.Bind (vm => ((SportsProjectsManagerVM)vm).ResyncCommand));
		}
	}
}
