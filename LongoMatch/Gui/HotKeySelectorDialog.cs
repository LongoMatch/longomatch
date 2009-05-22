
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
			
			if ((modifier & (ModifierType.ControlMask | ModifierType.ShiftMask | ModifierType.SuperMask)) != 0){
				hotKey.Key = key;
				hotKey.Modifier = modifier;
				this.Respond (ResponseType.Ok);
			}
			
			return base.OnKeyPressEvent (evnt);
		}
#endregion

	}
}
