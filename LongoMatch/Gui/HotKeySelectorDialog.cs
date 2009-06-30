
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
			Console.WriteLine(modifier);

			if ((modifier & (ModifierType.Mod1Mask | ModifierType.Mod5Mask | ModifierType.ShiftMask)) != 0
				&& key != Gdk.Key.Shift_L && key != Gdk.Key.Shift_R
			    && key != Gdk.Key.Alt_L &&  key != Gdk.Key.Alt_R ){
				hotKey.Key = key;
				hotKey.Modifier = modifier & (ModifierType.Mod1Mask | ModifierType.Mod5Mask | ModifierType.ShiftMask);
				this.Respond (ResponseType.Ok);
			}
			
			return base.OnKeyPressEvent (evnt);
		}
#endregion

	}
}
