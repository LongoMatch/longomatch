
using System;

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

	}
}
