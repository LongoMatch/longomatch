//
//  Copyright (C) 2015 Fluendo S.A.
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
using LongoMatch.Core.Common;
using Misc = LongoMatch.Gui.Helpers.Misc;
using Pixbuf = Gdk.Pixbuf;

namespace LongoMatch.Gui.Helpers
{
	
	public class IconNotebookHelper
	{

		public IconNotebookHelper (Notebook notebook)
		{
			Notebook = notebook;
			TabIcons = new Dictionary<Widget, Tuple<Pixbuf, Pixbuf>> (notebook.NPages);
			TabToolTips = new Dictionary<Widget, string> (notebook.NPages);
			CurrentPage = notebook.CurrentPage;

			notebook.ShowBorder = false;
			notebook.SwitchPage += HandleSwitchPage;
		}

		Notebook Notebook {
			get;
			set;
		}

		Dictionary<Widget, Tuple<Pixbuf, Pixbuf>> TabIcons {
			get;
			set;
		}

		Dictionary<Widget, string> TabToolTips {
			get;
			set;
		}

		int CurrentPage {
			get;
			set;
		}

		public void SetTabIcon (Widget widget, string icon, string activeIcon, string tooltiptext)
		{
			var pixIcon = Misc.LoadIcon (icon, StyleConf.NotebookTabIconSize, IconLookupFlags.ForceSvg);
			var pixActiveIcon = Misc.LoadIcon (activeIcon, StyleConf.NotebookTabIconSize, IconLookupFlags.ForceSvg);
			SetTabIcon (widget, pixIcon, pixActiveIcon, tooltiptext);
		}

		public void SetTabIcon (Widget widget, Pixbuf pixIcon, Pixbuf pixActiveIcon, string tooltiptext)
		{
			TabIcons.Add (widget, new Tuple<Pixbuf, Pixbuf> (pixIcon, pixActiveIcon));
			TabToolTips.Add (widget, tooltiptext);
		}

		public void SetTabIcon (int tabIndex, string icon, string activeIcon, string tooltiptext)
		{
			SetTabIcon (Notebook.GetNthPage (tabIndex), icon, activeIcon, tooltiptext);
		}

		public void UpdateTabs ()
		{
			for (int i = 0; i < Notebook.NPages; i++) {
				SetTabProps (Notebook.GetNthPage (i), i == Notebook.CurrentPage);
			}
		}

		void HandleSwitchPage (object o, SwitchPageArgs args)
		{
			SetTabProps (Notebook.GetNthPage (CurrentPage), false);
			SetTabProps (Notebook.GetNthPage ((int)args.PageNum), true);
			CurrentPage = Notebook.CurrentPage;
		}

		void SetTabProps (Widget widget, bool active)
		{
			if (widget == null) {
				return;
			}

			Gtk.Image img;

			img = Notebook.GetTabLabel (widget) as Gtk.Image;
			if (img == null) {
				img = new Gtk.Image ();
				img.WidthRequest = StyleConf.NotebookTabSize;
				img.HeightRequest = StyleConf.NotebookTabSize;
				Notebook.SetTabLabel (widget, img);
			}

			try {
				var tuple = TabIcons [widget];
				img.Pixbuf = active ? tuple.Item2 : tuple.Item1;
				img.TooltipText = TabToolTips [widget];
			} catch (KeyNotFoundException ex) {
				Log.Warning ("No icon set for tab number <" + Notebook.PageNum (widget) + "> with child <" + widget + ">");
			}

		}
	}
}

