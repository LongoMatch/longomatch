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
using System.Linq;
using Gtk;
using LongoMatch.Core.Events;
using LongoMatch.Services.State;
using LongoMatch.Services.States;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.UI.Helpers;
using Action = System.Action;
using Helpers = VAS.UI.Helpers;
using Image = VAS.Core.Common.Image;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	[ViewAttribute (HomeState.NAME)]
	public partial class WelcomePanel : Gtk.Bin, IPanel
	{
		static WelcomeButton [] default_buttons = {
			new WelcomeButton ("lm-project-new", Catalog.GetString ("New"),
							   new Action (() => App.Current.StateController.MoveTo (NewProjectState.NAME, null))),
			new WelcomeButton ("vas-open", Catalog.GetString ("Open"),
							   new Action (() => (App.Current.StateController.MoveTo (OpenProjectState.NAME, null)))),
			new WelcomeButton ("vas-import", Catalog.GetString ("Import"),
							   new Action (() => (App.Current.EventsBroker.Publish (new ImportProjectEvent ())))),
			new WelcomeButton ("lm-project", Catalog.GetString ("Projects"),
							   new Action (() => (App.Current.StateController.MoveTo (ProjectsManagerState.NAME, null)))),
			new WelcomeButton ("lm-team-config", Catalog.GetString ("Teams"),
							   new Action (() => (App.Current.StateController.MoveTo (TeamsManagerState.NAME, null)))),
			new WelcomeButton ("lm-template-config", Catalog.GetString ("Analysis Dashboards"),
							   new Action (() => (App.Current.StateController.MoveTo (DashboardsManagerState.NAME, null)))),
		};

		List<WelcomeButton> buttons;
		List<Widget> buttonWidgets;
		ImageView logoImage;
		SizeGroup sizegroup;

		public WelcomePanel ()
		{
			this.Build ();

			buttonWidgets = new List<Widget> ();
			buttons = new List<WelcomeButton> (default_buttons);

			hbox1.BorderWidth = StyleConf.WelcomeBorder;
			vbox2.Spacing = StyleConf.WelcomeIconsVSpacing;
			label3.ModifyFont (Pango.FontDescription.FromString ("Ubuntu 12"));
			preferencesbutton.Clicked += HandlePreferencesClicked;

			Create ();
		}

		uint NRows {
			get {
				return (uint)StyleConf.WelcomeIconsTotalRows;
			}
		}

		public string Title {
			get {
				return App.Current.SoftwareName;
			}
		}

		void HandlePreferencesClicked (object sender, EventArgs e)
		{
			App.Current.StateController.MoveTo (PreferencesState.NAME, null);
		}

		void Populate ()
		{
			// Query for tools
			List<ITool> tools = new List<ITool> ();

			App.Current.EventsBroker.Publish<QueryToolsEvent> (
				new QueryToolsEvent {
					Tools = tools
				}
			);

			foreach (ITool tool in tools) {
				if (tool.WelcomePanelIconList.Any () && tool.WelcomePanelIconList.FirstOrDefault ().Icon != null) {
					foreach (IWelcomeButton wpb in tool.WelcomePanelIconList) {
						buttons.Add (new WelcomeButton (wpb.Icon, wpb.Title, new Action (() => { wpb.Activate (); })));
					}
				}
			}
		}

		void Create ()
		{
			// Check if some additional tools are available that should be added to our buttons list
			Populate ();

			sizegroup = new SizeGroup (SizeGroupMode.Horizontal);

			Gtk.Image prefImage = new Gtk.Image (
									  Helpers.Misc.LoadIcon ("lm-preferences",
										  StyleConf.WelcomeIconSize, 0));
			preferencesbutton.Add (prefImage);

			preferencesbutton.WidthRequest = StyleConf.WelcomeIconSize;
			preferencesbutton.HeightRequest = StyleConf.WelcomeIconSize;

			// Our logo
			logoImage = new ImageView (App.Current.Background);
			logoImage.SetSize (StyleConf.WelcomeLogoWidth, StyleConf.WelcomeLogoHeight);

			//Adding the title
			vbox2.Add (logoImage);

			//Create necessary Hboxes for all icons
			List<HBox> hboxList = new List<HBox> ();

			for (int i = 0; i < NRows; i++) {
				Alignment al = new Alignment (0.5F, 0.5F, 0, 0);
				hboxList.Add (new HBox (true, StyleConf.WelcomeIconsHSpacing));
				al.Add (hboxList [i]);
				vbox2.Add (al);
			}

			int hboxRow = 0;
			for (uint i = 0; i < buttons.Count; i++) {
				Widget b;
				if (i >= StyleConf.WelcomeIconsFirstRow && hboxRow == 0) {
					hboxRow++;
				}
				b = CreateButton (buttons [(int)i]);
				hboxList [hboxRow].Add (b);
			}

			ShowAll ();
		}

		Widget CreateButton (WelcomeButton b)
		{
			Button button;
			VBox box;
			ImageView image;
			Gtk.Alignment alignment;
			Label label;

			if (b.Icon == null) {
				image = new ImageView (App.Current.ResourcesLocator.LoadIcon (b.Name, StyleConf.WelcomeIconImageSize));
			} else {
				image = new ImageView (b.Icon);
			}
			image.SetSize (StyleConf.WelcomeIconImageSize, StyleConf.WelcomeIconImageSize);

			button = new Button ();
			button.Clicked += (sender, e) => (b.Func ());
			button.HeightRequest = StyleConf.WelcomeIconSize;
			button.WidthRequest = StyleConf.WelcomeIconSize;
			button.Add (image);
			if (buttonWidgets.Count == 0) {
				button.Realized += (sender, e) => button.GrabFocus ();
			}

			alignment = new Alignment (0.5f, 0.5f, 0.0f, 0.0f);
			alignment.Add (button);

			label = new Label (b.Text);
			label.ModifyFont (Pango.FontDescription.FromString ("Ubuntu 12"));
			label.LineWrap = true;
			label.LineWrapMode = Pango.WrapMode.Word;
			label.Justify = Justification.Center;
			sizegroup.AddWidget (label);

			box = new VBox (false, StyleConf.WelcomeIconsTextSpacing);
			box.PackStart (alignment, false, false, 0);
			box.PackStart (label, false, false, 0);

			box.Name = b.Name + "roundedbutton";

			return box;
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public void SetViewModel (object viewModel)
		{
		}

		public KeyContext GetKeyContext ()
		{
			return null;
		}
	}

	public struct WelcomeButton
	{
		public string Name;
		public string Text;
		public Image Icon;
		public Action Func;

		public WelcomeButton (string name, string text, Action func)
		{
			Name = name;
			Text = text;
			Func = func;
			Icon = null;
		}

		public WelcomeButton (Image icon, string text, Action func)
		{
			Icon = icon;
			Text = text;
			Func = func;
			Name = null;
		}

	}
}

