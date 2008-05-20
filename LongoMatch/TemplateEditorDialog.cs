// TemplateEditorDialog.cs created with MonoDevelop
// User: ando at 17:53 07/12/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace LongoMatch
{
	
	
	public partial class TemplateEditorDialog : Gtk.Dialog
	{
		
		public TemplateEditorDialog()
		{
			this.Build();
		}
		
		public Sections Sections{
			set{this.sectionspropertieswidget3.SetSections(value);}
			get{return this.sectionspropertieswidget3.GetSections();}
		}
	
	}
}
