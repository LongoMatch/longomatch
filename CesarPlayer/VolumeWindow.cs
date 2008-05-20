// VolumeWindow.cs created with MonoDevelop
// User: ando at 3:33 25/11/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace CesarPlayer
{
	
	
	public partial class VolumeWindow : Gtk.Window
	{

	
		public delegate void VolumeChangedHandler (int level);
		public event         VolumeChangedHandler VolumeChanged;
		
		
		public VolumeWindow() : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
		}
		
		public void SetLevel(int level){
			volumescale.Value = level ;
		}

		protected virtual void OnLessbuttonClicked(object sender, System.EventArgs e)
		{
			volumescale.Value = volumescale.Value - 5;
		}

		protected virtual void OnMorebuttonClicked(object sender, System.EventArgs e)
		{
			volumescale.Value = volumescale.Value + 5;
		}

		protected virtual void OnVolumescaleValueChanged(object sender, System.EventArgs e)
		{
			VolumeChanged((int)volumescale.Value);
		}

		protected virtual void OnFocusOutEvent (object o, Gtk.FocusOutEventArgs args)
		{
			this.Hide();
		}


		
	}
}
