// CategoriesPropertiesWidget.cs
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;

using LongoMatch.Gui.Dialog;
using LongoMatch.Interfaces;
using LongoMatch.Store;


namespace LongoMatch.Gui.Base
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TemplatesEditorBase : Gtk.Bin
	{
		public TemplatesEditorBase()
		{
			this.Build();
		}
		
		public bool CanExport {
			get {
				return hseparator1.Visible;
			}
			set {
				hseparator1.Visible = value;
				exportbutton.Visible = value;
			}
		}
		
		public bool Edited {
			get;
			set;
		}
		
		public Project Project {
			get;
			set;
		}
		
		public int CurrentPage {
			set {
				notebook.CurrentPage = value;
			}
		}
		
		public string FirstPageName {
			set {
				(notebook.GetTabLabel(notebook.GetNthPage(0)) as Label).Text = value;
			}
		}
		
		protected void AddTreeView (Widget w) {
			scrolledwindow.Add(w);
			w.Show();
		}
		
		protected void AddUpperWidget (Widget w) {
			upbox.PackStart(w, true, false, 0);
		}
		
		protected void AddPage (Widget widget, string name) {
			Label label = new Label(name);
			widget.Show();
			label.Show();
			notebook.AppendPage(widget, label);
		}
		
		protected bool ButtonsSensitive {
			set {
				newprevbutton.Sensitive = value;
				newafterbutton.Sensitive = value;
				removebutton.Sensitive = value;
				editbutton.Sensitive = value;
			}
		}
		
		protected void MultipleSelection() {
			newprevbutton.Sensitive = false;
			newafterbutton.Sensitive = false;
			removebutton.Sensitive = true;
			editbutton.Sensitive = false;
		}

		protected virtual void OnNewAfter(object sender, EventArgs args) {}

		protected virtual void OnNewBefore(object sender, EventArgs args) {}

		protected virtual void OnRemove(object sender, EventArgs args) {}

		protected virtual void OnEdit(object sender, EventArgs args) {}

		protected virtual void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs args) {}
		
		protected virtual void OnExportbuttonClicked(object sender, System.EventArgs e) {}
	}
	
	public abstract class TemplatesEditorWidget<T, U> : TemplatesEditorBase, ITemplateWidget<T, U> where T: ITemplate<U>
	{
		protected T template;
		protected List<U> selected;
		protected ITemplateProvider<T, U> provider;
		
		public TemplatesEditorWidget (ITemplateProvider<T, U> provider): base()
		{
			this.provider = provider; 
		}
		
		public abstract T Template {get; set;}
		
		protected void UpdateModel() {
			Template = Template;
		}
		
		protected void AddItem(int item_index) {
			Template.AddDefaultItem(item_index);
			UpdateModel();
			Edited = true;
		}

		protected virtual void RemoveSelected() {
			UpdateModel();
			Edited = true;
			selected = null;
			ButtonsSensitive=false;
		}

		protected abstract void EditSelected();
		
		protected override void OnNewAfter(object sender, EventArgs args) {
			AddItem(template.IndexOf(selected[0])+1);
		}

		protected override void OnNewBefore(object sender, EventArgs args) {
			AddItem(template.IndexOf(selected[0]));
		}

		protected override void OnRemove(object sender, EventArgs args) {
			RemoveSelected();
		}

		protected override void OnEdit(object sender, EventArgs args) {
			EditSelected();
		}

		protected override  void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.Delete && selected != null)
				RemoveSelected();
		}

		protected override void OnExportbuttonClicked(object sender, System.EventArgs e)
		{
			EntryDialog dialog = new EntryDialog();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;
			dialog.ShowCount = false;
			dialog.Text = Catalog.GetString("New template");
			if(dialog.Run() == (int)ResponseType.Ok) {
				if(dialog.Text == "")
					MessagePopup.PopupMessage(dialog, MessageType.Error,
					                          Catalog.GetString("The template name is void."));
				else if(provider.Exists(dialog.Text)) {
					MessageDialog md = new MessageDialog(null,
					                                     DialogFlags.Modal,
					                                     MessageType.Question,
					                                     Gtk.ButtonsType.YesNo,
					                                     Catalog.GetString("The template already exists. " +
					                                                     "Do you want to overwrite it ?")
					                                    );
					if(md.Run() == (int)ResponseType.Yes){
						Template.Name = dialog.Text;
						provider.Update (Template);
					}
					md.Destroy();
				}
				else {
					Template.Name = dialog.Text;
					provider.Save (Template);
				}
			}
			dialog.Destroy();
		}
	}
	
}
