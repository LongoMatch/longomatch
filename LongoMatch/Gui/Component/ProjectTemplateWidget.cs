// SectionsPropertiesWidget.cs
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
using System.IO;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using Gdk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using LongoMatch.Gui.Dialog;
using LongoMatch.IO;


namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectTemplateWidget : Gtk.Bin
	{
		private List<HotKey> hkList;
		private Project project;
		private Sections sections;
		private SectionsTimeNode selectedSection;
		private bool edited = false;

		public ProjectTemplateWidget()
		{
			this.Build();
			hkList = new List<HotKey>();
		}

		public void SetProject(Project project) {
			this.project = project;
			if (project != null)
				Sections=project.Sections;
		}

		public Sections Sections {
			get {
				return sections;
			}
			set {
				this.sections = value;
				edited = false;
				Gtk.TreeStore sectionsListStore = new Gtk.TreeStore(typeof(SectionsTimeNode));
				hkList.Clear();
				for (int i=0;i<sections.Count;i++) {
					sectionsListStore.AppendValues(sections.GetSection(i));
					try {
						hkList.Add(sections.GetSection(i).HotKey);
					} catch {}; //Do not add duplicated hotkeys
				}
				sectionstreeview1.Model = sectionsListStore;
				ButtonsSensitive = false;
			}
		}

		public bool CanExport{
			set{
				hseparator1.Visible = value;
				exportbutton.Visible = value;
			}
		}
		public bool Edited {
			get {
				return edited;
			}
			set {
				edited=value;
			}
		}

		private void UpdateModel() {
			Sections = Sections;
		}

		private void AddSection(int index) {
			SectionsTimeNode tn;
			HotKey hkey = new HotKey();

			Time start = new Time(10*Time.SECONDS_TO_TIME);
			Time stop = new Time(10*Time.SECONDS_TO_TIME);

			tn  = new SectionsTimeNode("New Section",start,stop,hkey,new Color(Byte.MaxValue,Byte.MinValue,Byte.MinValue));

			if (project != null) {
				project.AddSectionAtPos(tn,index);
			}
			else {
				sections.AddSectionAtPos(tn,index);
			}
			UpdateModel();
			edited = true;
		}

		private void RemoveSection(int index) {
			if (project!= null) {
				MessageDialog dialog = new MessageDialog((Gtk.Window)this.Toplevel,DialogFlags.Modal,MessageType.Question,
				                ButtonsType.YesNo,true,
				                Catalog.GetString("You are about to delete a category and all the plays added to this category. Do you want to proceed?"));
				if (dialog.Run() == (int)ResponseType.Yes)
					try {
						project.DeleteSection(index);
					} catch {
						MessagePopup.PopupMessage(this,MessageType.Warning,
						                          Catalog.GetString("You can't delete the last section"));
						dialog.Destroy();
						return;
					}
				dialog.Destroy();
				sections=project.Sections;
			} else {
				sections.RemoveSection(index);
			}
			UpdateModel();
			edited = true;
			selectedSection = null;
			ButtonsSensitive=false;
		}

		private bool ButtonsSensitive {
			set {
				newprevbutton.Sensitive = value;
				newafterbutton.Sensitive = value;
				removebutton.Sensitive = value;
				editbutton.Sensitive = value;
			}
		}

		private void EditSelectedSection() {
			EditCategoryDialog dialog = new EditCategoryDialog();
			dialog.Section=selectedSection;
			dialog.HotKeysList = hkList;
			dialog.TransientFor = (Gtk.Window) Toplevel;
			dialog.Run();
			dialog.Destroy();
			edited = true;
		}
		
		private void SaveTemplate(string templateName){
			SectionsWriter.UpdateTemplate(templateName+".sct", Sections);
		}

		protected virtual void OnNewAfter(object sender, EventArgs args) {
			AddSection(sections.SectionsTimeNodes.IndexOf(selectedSection)+1);
		}

		protected virtual void OnNewBefore(object sender, EventArgs args) {
			AddSection(sections.SectionsTimeNodes.IndexOf(selectedSection));
		}

		protected virtual void OnRemove(object sender, EventArgs args) {
			RemoveSection(sections.SectionsTimeNodes.IndexOf(selectedSection));
		}

		protected virtual void OnEdit(object sender, EventArgs args) {
			EditSelectedSection();
		}

		protected virtual void OnSectionstreeview1SectionClicked(LongoMatch.TimeNodes.SectionsTimeNode tNode)
		{
			EditSelectedSection();
		}

		protected virtual void OnSectionstreeview1SectionSelected(LongoMatch.TimeNodes.SectionsTimeNode tNode)
		{
			selectedSection = tNode;
			ButtonsSensitive = selectedSection != null;
		}

		protected virtual void OnKeyPressEvent(object o, Gtk.KeyPressEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.Delete && selectedSection != null)
				RemoveSection(sections.SectionsTimeNodes.IndexOf(selectedSection));
		}

		protected virtual void OnExportbuttonClicked (object sender, System.EventArgs e)
		{
			EntryDialog dialog = new EntryDialog();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;
			dialog.ShowCount = false;
			dialog.Text = Catalog.GetString("New template");
			if (dialog.Run() == (int)ResponseType.Ok){
				if (dialog.Text == "")
					MessagePopup.PopupMessage(dialog, MessageType.Error,
					                          Catalog.GetString("The template name is void."));
				else if (File.Exists(System.IO.Path.Combine(MainClass.TemplatesDir(),dialog.Text+".sct"))){
					MessageDialog md = new MessageDialog(null,
					                                     DialogFlags.Modal,
					                                     MessageType.Question,
					                                     Gtk.ButtonsType.YesNo,
					                                     Catalog.GetString("The template already exists.Do you want to overwrite it ?")
					                                   );
					if (md.Run() == (int)ResponseType.Yes)
						SaveTemplate(dialog.Text);
					md.Destroy();
				}					
				else SaveTemplate(dialog.Text);
			}	
			dialog.Destroy();
		}
	}
}
