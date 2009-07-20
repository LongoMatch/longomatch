// 
//  Copyright (C) 2009 andoni
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;
using Gtk;

namespace LongoMatch.Gui
{
	
	public class MessagePopup
	{
		
		
		public MessagePopup()
		{
		}
		
		public static void PopupMessage(Widget sender,MessageType type, String errorMessage){
			Window toplevel;
			if (sender != null)
				toplevel = (Window)sender.Toplevel;
			else 
				toplevel = null;
			
			MessageDialog md = new MessageDialog(toplevel,
			                                     DialogFlags.Modal,
			                                     type,
			                                     ButtonsType.Ok,
			                                     errorMessage);
			md.Run();
			md.Destroy();
		}	
	}
}
