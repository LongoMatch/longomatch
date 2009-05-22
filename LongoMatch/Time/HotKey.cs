
using System;
using Gtk;
using Gdk;
using Mono.Unix;

namespace LongoMatch.TimeNodes
{
	
	public class HotKey
	{
		int key;
		int modifier;
	
		#region Constructors
		public HotKey()
		{
			this.key = -1;
			this.modifier = -1;
		}
		#endregion
		
	
		#region Properties
		public Gdk.Key Key{
			get{return (Gdk.Key)key;}
			set{key = (int)value;}
		}
		
		public Gdk.ModifierType Modifier{
			get{return (Gdk.ModifierType)modifier;}
			set{modifier= (int)value; }
		}
		
		public Boolean Defined{
			get{return (key!=-1 && modifier != -1);}
		}
		#endregion	
		
#region Override
		public override bool Equals (object obj)
		{
			HotKey comp;
			
			if (obj is HotKey){
				comp = (HotKey)obj;
				return (comp.Key==Key && comp.Modifier==Modifier);
			}
			else 
				return false;
		}
		
		public override string ToString ()
		{
			string modifierS;
			if ((Modifier & ModifierType.ControlMask) != 0)
				modifierS=Catalog.GetString("Control");
			else if ((Modifier & ModifierType.ShiftMask) != 0)
				modifierS=Catalog.GetString("Shift");
			else if ((Modifier & ModifierType.SuperMask) != 0)
				modifierS=Catalog.GetString("Super");
			else return "";	
			return string.Format("<{0}> + {1}", modifierS,(Key.ToString()).ToLower());
		}

#endregion
	}
}
