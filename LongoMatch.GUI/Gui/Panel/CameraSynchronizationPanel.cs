//
//  Copyright (C) 2017 Fluendo S.A.
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
using LongoMatch.Core;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Services.State;
using VAS.Services.ViewModel;

namespace LongoMatch.Gui.Panel
{
	/// <summary>
	/// A panel to synchronize cameras in newly created projects.
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	[View (CameraSynchronizationState.NAME)]
	public partial class CameraSynchronizationPanel : Gtk.Bin, IPanel<CameraSynchronizationVM>
	{
		CameraSynchronizationVM viewModel;

		public CameraSynchronizationPanel ()
		{
			this.Build ();
			Header = panelheader1;
			panelheader1.Title = Title;
			panelheader1.ApplyVisible = true;
			panelheader1.BackClicked += HandleBackClicked;
			panelheader1.ApplyClicked += HandleApplyClicked;
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
				return Catalog.GetString ("PERIODS SYNCHRONIZATION");
			}
		}

		public CameraSynchronizationVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				synchronizationwidget1.ViewModel = viewModel;
			}
		}

		protected PanelHeader Header {
			get;
			set;
		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (CameraSynchronizationVM)viewModel;
		}

		protected virtual void HandleBackClicked (object sender, System.EventArgs e)
		{
			App.Current.StateController.MoveBack ();
		}

		protected virtual void HandleApplyClicked (object sender, System.EventArgs e)
		{
			ViewModel.Save.Execute ();
		}
	}

	/// <summary>
	/// A panel to synchronize cameras in existing projects.
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	[View (CameraSynchronizationEditorState.NAME)]
	public class CameraSynchronizationEditorPanel : CameraSynchronizationPanel
	{
		public CameraSynchronizationEditorPanel ()
		{
			Header.ApplyVisible = false;
		}
	}
}
