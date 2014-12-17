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
using Gtk;
using Gdk;
using System.Runtime.InteropServices;

namespace LongoMatch
{
	public class GtkGlue
	{
		
		[DllImport("libgtk-2.0.dll") /* willfully unmapped */ ]
		static extern void gtk_menu_item_set_label (IntPtr menu, IntPtr label);
		[DllImport ("libcesarplayer.dll")]
		static extern void lgm_gtk_glue_gdk_event_button_set_button (IntPtr evt, uint button);
		
		public static void MenuItemSetLabel (MenuItem menu, string label) {
			gtk_menu_item_set_label (menu.Handle, GLib.Marshaller.StringToFilenamePtr (label));
		}


		public static void EventButtonSetButton (EventButton ev, uint button) {
			lgm_gtk_glue_gdk_event_button_set_button (ev.Handle, button);
		}

		/// <summary>
		/// Sets the link handler for a given GtkLabel.
		/// </summary>
		/// <param name="label">GtkLabel to set the handler for.</param>
		/// <param name="urlHandler">URL handler.</param>
		public static void SetLinkHandler (Gtk.Label label, Action<string> urlHandler)
		{
			new UrlHandlerClosure (urlHandler).ConnectTo (label);
		}

		class UrlHandlerClosure
		{
			Action<string> urlHandler;

			public UrlHandlerClosure (Action<string> urlHandler)
			{
				this.urlHandler = urlHandler;
			}

			[GLib.ConnectBefore]
			void HandleLink (object sender, ActivateLinkEventArgs args)
			{
				urlHandler (args.Url);
				args.RetVal = true;
			}

			public void ConnectTo (Gtk.Label label)
			{
				var signal = GLib.Signal.Lookup (label, "activate-link", typeof(ActivateLinkEventArgs));
				signal.AddDelegate (new EventHandler<ActivateLinkEventArgs> (HandleLink));
			}

			class ActivateLinkEventArgs : GLib.SignalArgs
			{
				public string Url { get { return (string)base.Args [0]; } }
			}
		}
	}
}

