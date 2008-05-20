// NewSectionsTemplatesFiles.cs created with MonoDevelop
// User: ando at 16:23 25/12/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace LongoMatch
{
	
	
	public partial class NewSectionsTemplatesFiles : Gtk.Dialog
	{
		
		public NewSectionsTemplatesFiles()
		{
			this.Build();
		}
		
		public string GetName(){
			return this.entry1.Text;;			
		}
	}
}
