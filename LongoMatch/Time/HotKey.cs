
using System;
using Gtk;
using Gdk;
using Mono.Unix;

namespace LongoMatch.TimeNodes
{
	
	public class HotKey
	{
		Gdk.Key key;
		Gdk.ModifierType modifier;
	
		#region Constructors
		public HotKey()
		{
		}
		#endregion
		
		#region Properties
		public Gdk.Key Key{
			get{return this.key;}
			set{this.key = value;}
		}
		
		public Gdk.ModifierType Modifier{
			get{return this.modifier;}
			set{this.modifier= value; }
		}
		#endregion	
		
#region Override
		public override bool Equals (object obj)
		{
			HotKey comp;
			
			if (obj is HotKey){
				comp = (HotKey)obj;
				return (comp.Key==this.Key && comp.Modifier==this.Modifier);
			}
			else 
				return false;
		}
		
		public override string ToString ()
		{
			string modifierS;
			if ((modifier & ModifierType.ControlMask) != 0)
				modifierS=Catalog.GetString("Control");
			else if ((modifier & ModifierType.ShiftMask) != 0)
				modifierS=Catalog.GetString("Shift");
			else if ((modifier & ModifierType.SuperMask) != 0)
				modifierS=Catalog.GetString("Super");
			else return "";	
			return string.Format("<{0}> + {1}", modifierS,(Key.ToString()).ToLower());
		}

#endregion
	}
}
