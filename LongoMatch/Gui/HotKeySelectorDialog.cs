
using System;
using Gtk;
using Gdk;
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Dialog
{
	
	
	public partial class HotKeySelectorDialog : Gtk.Dialog
	{
		HotKey hotKey;
		
#region Constructors
		
		public HotKeySelectorDialog()
		{
			hotKey = new HotKey();
			this.Build();
		}
#endregion
		
#region Properties
		
		public HotKey HotKey{
			get{return this.hotKey;}
		}		
#endregion
		
#region Overrides
		
		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			Gdk.Key key = evnt.Key;
			ModifierType modifier = evnt.State;
			

			if ((modifier & (ModifierType.ControlMask | ModifierType.ShiftMask )) != 0
				&& key != Gdk.Key.Control_L && key  != Gdk.Key.Control_R 
			  	&& key != Gdk.Key.Shift_L && key != Gdk.Key.Shift_R){
				hotKey.Key = key;
				hotKey.Modifier = modifier & (ModifierType.ControlMask | ModifierType.ShiftMask );
				this.Respond (ResponseType.Ok);
			}
			
			return base.OnKeyPressEvent (evnt);
		}
#endregion

	}
}
