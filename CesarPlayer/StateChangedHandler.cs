// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace LongoMatch.Video.Handlers {

	using System;

	public delegate void StateChangedHandler(object o, StateChangedArgs args);

	public class StateChangedArgs : GLib.SignalArgs {
		public bool Playing{
			get {
				return (bool) Args[0];
			}
			
		}

	}
}
