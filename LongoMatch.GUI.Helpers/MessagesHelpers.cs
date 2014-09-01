
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
using Mono.Unix;

namespace LongoMatch.Gui.Helpers
{

	public class MessagesHelpers
	{
	
		static public void InfoMessage (Widget parent, string message) {
			PopupMessage(parent, MessageType.Info, message);
		}
		
		static public void ErrorMessage(Widget parent, string message) {
			PopupMessage(parent, MessageType.Error, message);
		}
		
		static public void WarningMessage(Widget parent, string message) {
			PopupMessage(parent, Gtk.MessageType.Warning, message);
		}
		
		static public bool QuestionMessage(Widget parent, string question, string title=null) {
			Window toplevel;
			
			if(parent != null)
				toplevel = parent.Toplevel as Window;
			else
				toplevel = null;
				
			MessageDialog md = new MessageDialog(toplevel, DialogFlags.Modal,
			                                     MessageType.Question, ButtonsType.YesNo,
			                                     question);
				
			md.Icon =  Misc.LoadIcon("longomatch", IconSize.Button, 0);
			md.Title = title;
			var res = md.Run();
			md.Destroy();
			return (res == (int)ResponseType.Yes);
		}

		public static int PopupMessage(Widget sender,MessageType type, String errorMessage) {
			Window toplevel;
			int ret;
			
			if(sender != null)
				toplevel = (Window)sender.Toplevel;
			else
				toplevel = null;

			MessageDialog md = new MessageDialog(toplevel,
			                                     DialogFlags.Modal,
			                                     type,
			                                     ButtonsType.Ok,
			                                     errorMessage);
			md.Icon = Misc.LoadIcon("longomatch", Gtk.IconSize.Dialog, 0);
			ret = md.Run();
			md.Destroy();
			return ret;
		}
		
		static public string QueryMessage (Widget sender, string key, string title=null, string value="") {
			string ret = null;
			Window parent;
			
			if(sender != null)
				parent = (Window)sender.Toplevel;
			else
				parent = null;
				
			Label label = new Label(key);
			Entry entry = new Entry(value);
			Gtk.Dialog dialog = new Gtk.Dialog (title, parent, DialogFlags.DestroyWithParent);
			dialog.Modal = true;
            dialog.AddButton (Catalog.GetString("Add"), ResponseType.Ok);
			dialog.VBox.PackStart (label, false, false, 0);
			dialog.VBox.PackStart (entry, true, true, 0);
			dialog.Icon = Misc.LoadIcon ("longomatch", Gtk.IconSize.Dialog, 0);
			dialog.ShowAll ();
			if (dialog.Run () == (int) ResponseType.Ok) {
				ret = entry.Text;
			}
			dialog.Destroy ();
			return ret;
		}
	}
}
