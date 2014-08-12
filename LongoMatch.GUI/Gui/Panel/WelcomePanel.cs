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
			new WelcomeButton ("longomatch-new", Catalog.GetString ("New"),
			                   new Action (() => Config.EventsBroker.EmitNewProject (null))),
			new WelcomeButton ("longomatch-open", Catalog.GetString ("Open"),
			                   new Action (() => Config.EventsBroker.EmitOpenProject ())),
			new WelcomeButton ("longomatch-import", Catalog.GetString ("Import"),
			                   new Action (() => Config.EventsBroker.EmitImportProject ())),
			new WelcomeButton ("longomatch-project", Catalog.GetString ("Projects\nmanager"),
			                   new Action (() => Config.EventsBroker.EmitManageProjects ())),
			new WelcomeButton ("longomatch-sportconfig", Catalog.GetString ("Sport\ntemplates"),
			                   new Action (() => Config.EventsBroker.EmitManageCategories ())),
			new WelcomeButton ("longomatch-teamconfig", Catalog.GetString ("Team\ntemplates"),
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
			hbox1.BorderWidth = (uint) Config.Style.WelcomeBorder;
			Create ();
			Name = "WelcomePanel";
		}

		int MinWidth {
			get {
				return Math.Max (Config.Style.WelcomeLogoWidth,
				                 Config.Style.WelcomeIconSize * 3 +
				                 Config.Style.WelcomeIconsHSpacing * 2) +
					Config.Style.WelcomeMinWidthBorder;
			}
		}
		
		int MinHeight {
			get {
				return  HeaderHeight + Config.Style.WelcomeLogoHeight +
					(IconHeight + Config.Style.WelcomeIconsVSpacing) * NRows + 20;
			}
		}
		
		int IconHeight {
			get {
				return Config.Style.WelcomeLogoHeight + Config.Style.WelcomeIconsTextSpacing +
					Config.Style.WelcomeTextHeight;
			}
		}
		
		int HeaderHeight {
			get {
				return Config.Style.WelcomeBorder * 2 + Config.Style.WelcomeIconSize;
			}
		}
		int NRows {
			get {
				return (int)Math.Ceiling ((float)buttons.Length / Config.Style.WelcomeIconsPerRow);
			}
		}
		
		void Create ()
		{
			int padding;

			Gtk.Image prefImage = new Gtk.Image (
				IconTheme.Default.LoadIcon ("longomatch-preferences",
			                            Config.Style.WelcomeIconSize, 0));
			preferencesbutton.Add (prefImage);
			preferencesbutton.WidthRequest = Config.Style.WelcomeIconSize;
			preferencesbutton.HeightRequest = Config.Style.WelcomeIconSize;

			logoImage = new Gtk.Image ();
			logoImage.Pixbuf = Gdk.Pixbuf.LoadFromResource ("longomatch-dark-bg.svg");
			logoImage.WidthRequest = Config.Style.WelcomeLogoWidth;
			logoImage.HeightRequest = Config.Style.WelcomeLogoHeight;
			fixedwidget.Put (logoImage, 0, 0);

			padding = Config.Style.WelcomeLogoHeight + Config.Style.WelcomeIconsVSpacing;
			for (int i=0; i < buttons.Length; i++) {
				Widget b;
				int x, y;
				
				x = (Config.Style.WelcomeIconsHSpacing + Config.Style.WelcomeIconSize) *
					(i % Config.Style.WelcomeIconsPerRow);
				y = (Config.Style.WelcomeIconsVSpacing + Config.Style.WelcomeIconSize) *
					(i / Config.Style.WelcomeIconsPerRow);

				b = CreateButton (buttons[i]);
				fixedwidget.Put (b, x, y + padding);
				buttonWidgets.Add (b);
			}
			fixedwidget.HeightRequest = Config.Style.WelcomeLogoHeight +
					(IconHeight + Config.Style.WelcomeIconsVSpacing) * NRows; 
		}
		
		Widget CreateButton (WelcomeButton b) {
			Button button;
			VBox box;
			Gtk.Image image;
			Label label;
			
			image = new Gtk.Image (
				IconTheme.Default.LoadIcon (b.name, Config.Style.WelcomeIconSize, 0));

			button = new Button ();
			button.Clicked += (sender, e) => (b.func());
			button.HeightRequest = Config.Style.WelcomeIconSize;
			button.WidthRequest = Config.Style.WelcomeIconSize;
			button.Add (image);

			label = new Label (b.text);
			label.LineWrap = true;
			label.LineWrapMode = Pango.WrapMode.Word;
			label.Justify = Justification.Center;

			box = new VBox (false, Config.Style.WelcomeIconsTextSpacing);
			box.PackStart (button, false, false, 0);
			box.PackStart (label, false, false, 0);
			box.HeightRequest = IconHeight;
			box.ShowAll ();
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

