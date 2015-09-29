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

using Mono.Unix;
using Gtk;

using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.GUI;

using Action = System.Action;
using System.Collections.Generic;
using Image = LongoMatch.Core.Common.Image;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class WelcomePanel : Gtk.Bin
	{
		static WelcomeButton[] default_buttons = {
			new WelcomeButton ("longomatch-project-new", Catalog.GetString ("New"),
				new Action (() => Config.EventsBroker.EmitNewProject (null))),
			new WelcomeButton ("longomatch-project-open", Catalog.GetString ("Open"),
				new Action (() => Config.EventsBroker.EmitOpenProject ())),
			new WelcomeButton ("longomatch-project-import", Catalog.GetString ("Import"),
				new Action (() => Config.EventsBroker.EmitImportProject ())),
			new WelcomeButton ("longomatch-project", Catalog.GetString ("Projects\nmanager"),
				new Action (() => Config.EventsBroker.EmitManageProjects ())),
			new WelcomeButton ("longomatch-template-config", Catalog.GetString ("Analysis\nDashboards\nmanager"),
				new Action (() => Config.EventsBroker.EmitManageCategories ())),
			new WelcomeButton ("longomatch-team-config", Catalog.GetString ("Teams\nmanager"),
				new Action (() => Config.EventsBroker.EmitManageTeams ())),

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

			preferencesbutton.Clicked += HandlePreferencesClicked;

			Create ();

			Name = "WelcomePanel";
		}

		uint NRows {
			get {
				return (uint)(buttons.Count / StyleConf.WelcomeIconsPerRow);
			}
		}

		void HandlePreferencesClicked (object sender, EventArgs e)
		{
			Config.EventsBroker.EmitEditPreferences ();
		}

		void Populate ()
		{
			// Query for tools
			List<ITool> tools = new List<ITool> ();

			Config.EventsBroker.EmitQueryTools (tools);

			foreach (ITool tool in tools) {
				if (tool.WelcomePanelIcon != null) {
					buttons.Add (new WelcomeButton (tool.WelcomePanelIcon, tool.Name,
						new Action (() => tool.Load (Config.GUIToolkit))));
				}
			}
		}

		void Create ()
		{
			// Check if some additional tools are available that should be added to our buttons list
			Populate ();

			// One extra row for our logo
			tablewidget.NRows = (uint)NRows + 1;
			tablewidget.NColumns = StyleConf.WelcomeIconsPerRow;
			sizegroup = new SizeGroup (SizeGroupMode.Horizontal);

			Gtk.Image prefImage = new Gtk.Image (
				                      Helpers.Misc.LoadIcon ("longomatch-preferences",
					                      StyleConf.WelcomeIconSize, 0));
			preferencesbutton.Add (prefImage);
			preferencesbutton.WidthRequest = StyleConf.WelcomeIconSize;
			preferencesbutton.HeightRequest = StyleConf.WelcomeIconSize;

			// Our logo
			logoImage = new Gtk.Image ();
			logoImage.Pixbuf = Config.Background.Scale (StyleConf.WelcomeLogoWidth,
				StyleConf.WelcomeLogoHeight).Value;
			logoImage.WidthRequest = StyleConf.WelcomeLogoWidth;
			logoImage.HeightRequest = StyleConf.WelcomeLogoHeight;
			tablewidget.Attach (logoImage, 0, StyleConf.WelcomeIconsPerRow, 0, 1,
				AttachOptions.Expand | AttachOptions.Fill,
				AttachOptions.Expand | AttachOptions.Fill,
				0, StyleConf.WelcomeIconsVSpacing / 2);

			for (uint i = 0; i < buttons.Count; i++) {
				Widget b;
				uint c, l;

				c = i % StyleConf.WelcomeIconsPerRow;
				l = i / StyleConf.WelcomeIconsPerRow + 1;

				b = CreateButton (buttons [(int)i]);
				tablewidget.Attach (b, c, c + 1, l, l + 1,
					AttachOptions.Expand | AttachOptions.Fill,
					AttachOptions.Expand | AttachOptions.Fill,
					0, StyleConf.WelcomeIconsVSpacing / 2);
				buttonWidgets.Add (b);
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

