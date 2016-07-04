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
using Gtk;
using LongoMatch.Core.Events;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.GUI;
using Action = System.Action;
using Helpers = VAS.UI.Helpers;
using Image = VAS.Core.Common.Image;
using LMCommon = LongoMatch.Core.Common;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WelcomePanel : Gtk.Bin
	{
		static WelcomeButton[] default_buttons = {
			new WelcomeButton ("longomatch-project-new", Catalog.GetString ("New"),
				new Action (() => 
					(App.Current.EventsBroker.Publish<NewProjectEvent> (new NewProjectEvent { Project = null })))),
			new WelcomeButton ("longomatch-open", Catalog.GetString ("Open"),
				new Action (() => (App.Current.EventsBroker.Publish<OpenProjectEvent> (new OpenProjectEvent ())))),
			new WelcomeButton ("longomatch-import", Catalog.GetString ("Import"),
				new Action (() => (App.Current.EventsBroker.Publish<ImportProjectEvent> (new ImportProjectEvent ())))),			
			new WelcomeButton ("longomatch-project", Catalog.GetString ("Projects"),
				new Action (() => (App.Current.EventsBroker.Publish<ManageProjectsEvent> (new ManageProjectsEvent ())))),
			new WelcomeButton ("longomatch-team-config", Catalog.GetString ("Teams"),
				new Action (() => (App.Current.EventsBroker.Publish<ManageTeamsEvent> (new ManageTeamsEvent ())))),
			new WelcomeButton ("longomatch-template-config", Catalog.GetString ("Analysis Dashboards"),
				new Action (() => (App.Current.EventsBroker.Publish<ManageCategoriesEvent> (new ManageCategoriesEvent ())))),
		};

		List<WelcomeButton> buttons;
		List<Widget> buttonWidgets;
		Gtk.Image logoImage;
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

			Name = "WelcomePanel";
		}

		uint NRows {
			get {
				return (uint)StyleConf.WelcomeIconsTotalRows;
			}
		}

		void HandlePreferencesClicked (object sender, EventArgs e)
		{
			App.Current.EventsBroker.Publish<EditPreferencesEvent> (new EditPreferencesEvent ());
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
				if (tool.WelcomePanelIcon != null) {
					buttons.Add (new WelcomeButton (tool.WelcomePanelIcon, tool.Name,
						new Action (() => tool.Load (App.Current.GUIToolkit))));
				}
			}
		}

		void Create ()
		{
			// Check if some additional tools are available that should be added to our buttons list
			Populate ();

			sizegroup = new SizeGroup (SizeGroupMode.Horizontal);

			Gtk.Image prefImage = new Gtk.Image (
				                      Helpers.Misc.LoadIcon ("longomatch-preferences",
					                      StyleConf.WelcomeIconSize, 0));
			preferencesbutton.Add (prefImage);
			preferencesbutton.WidthRequest = StyleConf.WelcomeIconSize;
			preferencesbutton.HeightRequest = StyleConf.WelcomeIconSize;

			// Our logo
			logoImage = new Gtk.Image ();
			logoImage.Pixbuf = App.Current.Background.Scale (StyleConf.WelcomeLogoWidth,
				StyleConf.WelcomeLogoHeight).Value;
			logoImage.WidthRequest = StyleConf.WelcomeLogoWidth;
			logoImage.HeightRequest = StyleConf.WelcomeLogoHeight;

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
			Gtk.Image image;
			Gtk.Alignment alignment;
			Label label;

			if (b.Icon == null) {
				image = new Gtk.Image (
					Helpers.Misc.LoadIcon (b.Name, StyleConf.WelcomeIconImageSize, 0));
			} else {
				image = new Gtk.Image (b.Icon.Value);
			}

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

