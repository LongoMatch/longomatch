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
using LongoMatch.Handlers;
using LongoMatch.Common;
using Mono.Unix;
using Gtk;

using Action = System.Action;
using System.Collections.Generic;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WelcomePanel : Gtk.Bin
	{
	
		static WelcomeButton [] buttons = {
			new WelcomeButton ("longomatch-project-new", Catalog.GetString ("New"),
			                   new Action (() => Config.EventsBroker.EmitNewProject (null))),
			new WelcomeButton ("longomatch-project-open", Catalog.GetString ("Open"),
			                   new Action (() => Config.EventsBroker.EmitOpenProject ())),
			new WelcomeButton ("longomatch-project-import", Catalog.GetString ("Import"),
			                   new Action (() => Config.EventsBroker.EmitImportProject ())),
			new WelcomeButton ("longomatch-project", Catalog.GetString ("Projects\nmanager"),
			                   new Action (() => Config.EventsBroker.EmitManageProjects ())),
			new WelcomeButton ("longomatch-template-config", Catalog.GetString ("Analysis\nmanager"),
			                   new Action (() => Config.EventsBroker.EmitManageCategories ())),
			new WelcomeButton ("longomatch-team-config", Catalog.GetString ("Team\nmanager"),
			                   new Action (() => Config.EventsBroker.EmitManageTeams ())),
			                   
		};
		List<Widget> buttonWidgets;
		Gtk.Image logoImage;
		
		public WelcomePanel ()
		{
			this.Build ();
			HeightRequest = MinHeight;
			WidthRequest = MinWidth;
			buttonWidgets = new List<Widget>();
			hbox1.BorderWidth = StyleConf.WelcomeBorder;
			preferencesbutton.Clicked += HandlePreferencesClicked;
			Create ();
			Name = "WelcomePanel";
		}

		int MinWidth {
			get {
				return Math.Max (StyleConf.WelcomeLogoWidth,
				                 StyleConf.WelcomeIconSize * 3 +
				                 StyleConf.WelcomeIconsHSpacing * 2) +
					StyleConf.WelcomeMinWidthBorder;
			}
		}
		
		int MinHeight {
			get {
				return  HeaderHeight + StyleConf.WelcomeLogoHeight +
					(IconHeight + StyleConf.WelcomeIconsVSpacing) * NRows + 20;
			}
		}
		
		int IconHeight {
			get {
				return StyleConf.WelcomeLogoHeight + StyleConf.WelcomeIconsTextSpacing +
					StyleConf.WelcomeTextHeight;
			}
		}
		
		int HeaderHeight {
			get {
				return StyleConf.WelcomeBorder * 2 + StyleConf.WelcomeIconSize;
			}
		}
		int NRows {
			get {
				return (int)Math.Ceiling ((float)buttons.Length / StyleConf.WelcomeIconsPerRow);
			}
		}
		
		void HandlePreferencesClicked (object sender, EventArgs e)
		{
			Config.EventsBroker.EmitEditPreferences ();
		}

		void Create ()
		{
			int padding;

			Gtk.Image prefImage = new Gtk.Image (
				IconTheme.Default.LoadIcon ("longomatch-preferences",
			                            StyleConf.WelcomeIconSize, 0));
			preferencesbutton.Add (prefImage);
			preferencesbutton.WidthRequest = StyleConf.WelcomeIconSize;
			preferencesbutton.HeightRequest = StyleConf.WelcomeIconSize;

			logoImage = new Gtk.Image ();
			logoImage.Pixbuf = Gdk.Pixbuf.LoadFromResource ("longomatch-dark-bg.svg");
			logoImage.WidthRequest = StyleConf.WelcomeLogoWidth;
			logoImage.HeightRequest = StyleConf.WelcomeLogoHeight;
			fixedwidget.Put (logoImage, 0, 0);

			padding = StyleConf.WelcomeLogoHeight + StyleConf.WelcomeIconsVSpacing;
			for (int i=0; i < buttons.Length; i++) {
				Widget b;
				int x, y;
				
				x = (StyleConf.WelcomeIconsHSpacing + StyleConf.WelcomeIconSize) *
					(i % StyleConf.WelcomeIconsPerRow);
				y = (StyleConf.WelcomeIconsVSpacing + StyleConf.WelcomeIconSize) *
					(i / StyleConf.WelcomeIconsPerRow);

				b = CreateButton (buttons[i]);
				fixedwidget.Put (b, x, y + padding);
				buttonWidgets.Add (b);
			}
			fixedwidget.HeightRequest = StyleConf.WelcomeLogoHeight +
					(IconHeight + StyleConf.WelcomeIconsVSpacing) * NRows; 
		}
		
		Widget CreateButton (WelcomeButton b) {
			Button button;
			VBox box;
			Gtk.Image image;
			Label label;
			
			image = new Gtk.Image (
				IconTheme.Default.LoadIcon (b.name, StyleConf.WelcomeIconImageSize, 0));

			button = new Button ();
			button.Clicked += (sender, e) => (b.func());
			button.HeightRequest = StyleConf.WelcomeIconSize;
			button.WidthRequest = StyleConf.WelcomeIconSize;
			button.Add (image);

			label = new Label (b.text);
			label.LineWrap = true;
			label.LineWrapMode = Pango.WrapMode.Word;
			label.Justify = Justification.Center;

			box = new VBox (false, StyleConf.WelcomeIconsTextSpacing);
			box.PackStart (button, false, false, 0);
			box.PackStart (label, false, false, 0);
			box.HeightRequest = IconHeight;
			box.ShowAll ();
			box.Name = b.name + "roundedbutton";
			return box;
		}
	}
	
	public struct WelcomeButton {
		public string name;
		public string text;
		public Action func;

		public WelcomeButton (string name, string text, Action func) {
			this.name = name;
			this.text = text;
			this.func = func;
		}
		
	}
}

