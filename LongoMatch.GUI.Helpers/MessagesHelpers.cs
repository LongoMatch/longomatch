
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using Dialog = Gtk.Dialog;
using LongoMatch.Core;
using LongoMatch.Core.Common;

namespace LongoMatch.Gui.Helpers
{

	public class MessagesHelpers
	{
		public delegate void CustomizeDialog (MessageDialog Dialog);
	
		static public void InfoMessage (Widget parent, string message)
		{
			PopupMessage (parent, MessageType.Info, message);
		}

		static public void ErrorMessage (Widget parent, string message)
		{
			PopupMessage (parent, MessageType.Error, message);
		}

		static public void WarningMessage (Widget parent, string message)
		{
			PopupMessage (parent, Gtk.MessageType.Warning, message);
		}

		static public bool QuestionMessage (Widget parent, string question, string title = null)
		{
			Window toplevel;
			
			if (parent != null)
				toplevel = parent.Toplevel as Window;
			else
				toplevel = null;
				
			MessageDialog md = new MessageDialog (toplevel, DialogFlags.Modal,
				                   MessageType.Question, ButtonsType.YesNo,
				                   question);
				
			md.Icon = Misc.LoadIcon ("longomatch", IconSize.Button, 0);
			md.Title = title;
			var res = md.Run ();
			md.Destroy ();
			return (res == (int)ResponseType.Yes);
		}

		public static int PopupMessage (Widget sender, MessageType type, String errorMessage,
			CustomizeDialog customize = null)
		{
			Window toplevel;
			int ret;
			
			if (sender != null)
				toplevel = sender.Toplevel as Window;
			else
				toplevel = null;

			MessageDialog md = new MessageDialog (toplevel,
				                   DialogFlags.Modal,
				                   type,
				                   ButtonsType.Ok,
				                   errorMessage);
			md.Icon = Misc.LoadIcon ("longomatch", Gtk.IconSize.Dialog, 0);
			try {
				var vbox = md.MessageDialogGetMessageArea ();
				var label = (Label)vbox.Children [0];

				label.SetLinkHandler (url => {
					try {
						System.Diagnostics.Process.Start (url);
					} catch (Exception ex) {
						Log.WarningFormat ("Could not spawn process for url {0}\n{1}",
							url, ex);
					}
				});
			} catch (Exception ex) {
				Log.WarningFormat ("Could not set link handler for Message Dialog\n{0}",
					ex);
			}

			if (customize != null)
				customize (md);

			ret = md.Run ();
			md.Destroy ();
			return ret;
		}

		static public string QueryMessage (Widget sender, string key, string title = null, string value = "")
		{
			string ret = null;
			Window parent;
			
			if (sender != null)
				parent = sender.Toplevel as Window;
			else
				parent = null;
				
			Label label = new Label (key);
			Entry entry = new Entry (value);
			Gtk.Dialog dialog = new Gtk.Dialog (title, parent, DialogFlags.DestroyWithParent);
			dialog.Modal = true;
			dialog.WidthRequest = 300;
			dialog.AddButton (Catalog.GetString ("Add"), ResponseType.Ok);
			dialog.VBox.PackStart (label, false, false, 0);
			dialog.VBox.PackStart (entry, true, true, 0);
			dialog.Icon = Misc.LoadIcon ("longomatch", Gtk.IconSize.Dialog, 0);
			dialog.ShowAll ();
			if (dialog.Run () == (int)ResponseType.Ok) {
				ret = entry.Text;
			}
			dialog.Destroy ();
			return ret;
		}

		static public bool NewVersionAvailable (Version currentVersion, Version latestVersion,
			string downloadURL, string changeLog, Widget parent = null)
		{
			string message = string.Format (
				Catalog.GetString("Version {0} is available!\n" +
					"(You are using version {1})\n" +
					"<a href=\"{2}\">Click here to get it.</a>"),
				latestVersion, currentVersion, downloadURL);

			bool checkState = false;

			PopupMessage (parent, MessageType.Info, message, (dialog) =>
				{
					dialog.Title = Catalog.GetString ("New version available");
					VBox vbox = dialog.MessageDialogGetMessageArea ();

					var expander = new Gtk.Expander (Catalog.GetString ("Changes:"));
					expander.Add (new Label (changeLog));
					vbox.Add (expander);

					var check = new CheckButton (Catalog.GetString ("Do not notify me again until next version"));
					check.Toggled += delegate {
						checkState = check.Active;
					};
					vbox.Add (check);

					vbox.ShowAll ();
				});

			return checkState;
		}
	}
}
