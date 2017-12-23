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
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Gui.Component;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (OpenProjectState.NAME)]
	public partial class OpenProjectPanel : Gtk.Bin, IPanel<SportsProjectsManagerVM>
	{
		SportsProjectsManagerVM viewModel;

		public OpenProjectPanel ()
		{
			this.Build ();

			projectlistwidget.ProjectSelected += HandleProjectSelected;
			projectlistwidget.SelectionMode = SelectionMode.Single;
			projectlistwidget.ViewMode = ProjectListViewMode.Icons;
			panelheader1.ApplyVisible = false;
			panelheader1.BackClicked += HandleClicked;
			panelheader1.Title = Title;
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
			projectlistwidget.Dispose ();
			OnUnload ();
			base.OnDestroyed ();
			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		public SportsProjectsManagerVM ViewModel {
			set {
				viewModel = value;
				//FIXME: Change it to ViewModel structure in the future
				projectlistwidget.Fill (viewModel.ViewModels.Select (p => p.Model).ToList ());
			}
			get {
				return viewModel;
			}
		}

		public string Title {
			get {
				return Catalog.GetString ("OPEN PROJECT");
			}
		}

		public void OnLoad ()
		{

		}

		public void OnUnload ()
		{

		}

		//FIXME: add IPanel KeyContext using MMVMC pattern
		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (SportsProjectsManagerVM)viewModel;
		}

		void HandleClicked (object sender, EventArgs e)
		{
			App.Current.StateController.MoveBack ();
		}

		void HandleProjectSelected (LMProject project)
		{
			project.ProjectType = ProjectType.FileProject;
			LMStateHelper.OpenProject (new LMProjectVM { Model = project });
		}
	}
}
