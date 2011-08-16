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
using System.IO;
using Gdk;
using Gtk;

using LongoMatch.Gui.Dialog;
using LongoMatch.Interfaces;
using LongoMatch.IO;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using Mono.Unix;


namespace LongoMatch.Gui.Component
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
		
		public bool InProject {
			get;
			set;
		}
		
		protected void AddTreeView (Widget w) {
			scrolledwindow2.Add(w);
			w.Show();
		}
		
		protected void AddUpperWidget (Widget w) {
			upbox.PackStart(w, true, false, 0);
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
		
		public TemplatesEditorWidget (): base()
		{
			provider = MainClass.ts.GetTemplateProvider<T, U>();
		}
		
		public abstract T Template {get; set;}
		
		protected void UpdateModel() {
			Template = Template;
		}

		protected virtual void RemoveSelected() {
			UpdateModel();
			Edited = true;
			selected = null;
			ButtonsSensitive=false;
		}

		protected abstract void EditSelected();
		
		protected abstract void AddItem(int item);

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
	
	public class CategoriesTemplateEditorWidget: TemplatesEditorWidget<Categories, Category> 
	{
		private CategoriesTreeView categoriestreeview;
		private List<HotKey> hkList;

		public CategoriesTemplateEditorWidget (): base()
		{
			hkList = new List<HotKey>();
			categoriestreeview = new CategoriesTreeView();
			categoriestreeview.CategoryClicked += this.OnCategoryClicked;
			categoriestreeview.CategoriesSelected += this.OnCategoriesSelected;
			AddTreeView(categoriestreeview);
		}
		
		public override Categories Template {
			get {
				return template;
			}
			set {
				template = value;
				Edited = false;
				Gtk.TreeStore categoriesListStore = new Gtk.TreeStore(typeof(Category));
				hkList.Clear();

				foreach(var cat in template) {
					categoriesListStore.AppendValues(cat);
					try {
						hkList.Add(cat.HotKey);
					} catch {}; //Do not add duplicated hotkeys
				}
				categoriestreeview.Model = categoriesListStore;
				ButtonsSensitive = false;
			}
		}
		
		protected override void AddItem(int index) {
			UpdateModel();
			Edited = true;
		}


		protected override void RemoveSelected (){
			if(InProject) {
				MessageDialog dialog = new MessageDialog((Gtk.Window)this.Toplevel,DialogFlags.Modal,MessageType.Question,
				                                         ButtonsType.YesNo,true,
				                                         Catalog.GetString("You are about to delete a category and all the plays added to this category. Do you want to proceed?"));
				if(dialog.Run() == (int)ResponseType.Yes) {
					try {
						foreach(var item in selected)
							template.Remove(item);
					} catch {
						MessagePopup.PopupMessage(this,MessageType.Warning,
						                          Catalog.GetString("A template needs at least one category"));
					}
				}
				dialog.Destroy();
			} else {
				foreach(Category cat in selected) {
					if(template.Count == 1) {
						MessagePopup.PopupMessage(this,MessageType.Warning,
						                          Catalog.GetString("A template needs at least one category"));
					} else
						template.Remove(cat);
				}
			}	
			base.RemoveSelected();
		}
		
		protected override void EditSelected() {
			EditCategoryDialog dialog = new EditCategoryDialog();
			dialog.Category = selected[0];
			dialog.HotKeysList = hkList;
			dialog.TransientFor = (Gtk.Window) Toplevel;
			dialog.Run();
			dialog.Destroy();
			Edited = true;
		}
		private void OnCategoryClicked(Category cat)
		{
			selected = new List<Category> ();
			selected.Add (cat);
			EditSelected();
		}

		private void OnCategoriesSelected(List<Category> catList)
		{
			selected = catList;
			if(catList.Count == 0)
				ButtonsSensitive = false;
			else if(catList.Count == 1) {
				ButtonsSensitive = true;
			}
			else {
				MultipleSelection();
			}
		}
	}
	
	
	public class TeamTemplateEditorWidget: TemplatesEditorWidget<TeamTemplate, Player>
	{	
		private PlayerPropertiesTreeView treeview;
		Entry teamentry;
		
		public TeamTemplateEditorWidget () {
			treeview = new PlayerPropertiesTreeView(); 
			treeview.PlayerClicked += this.OnPlayerClicked;
			treeview.PlayerSelected += this.OnPlayersSelected;
			AddTreeView(treeview);
			AddTeamNamesWidget();
			
		}
		
		public override  TeamTemplate Template {
			get {
				return template;
			}
			set {
				template= value;
				Edited = false;
				Gtk.TreeStore playersListStore = new Gtk.TreeStore(typeof(Player));
				foreach(Player player in template)
					playersListStore.AppendValues(player);
				treeview.Model=playersListStore;
				teamentry.Text = template.Name;
			}
		}
		
		private void AddTeamNamesWidget () {
			HBox box = new HBox();
			Label label = new Label(Catalog.GetString("Team name")+":");
			teamentry = new Entry ();
			teamentry.Changed += delegate(object sender, EventArgs e) {
				Template.Name = teamentry.Text;
			};
		
			box.PackStart (label, false, false, 0);
			box.PackEnd (teamentry, false, false, 0);
			box.ShowAll();
			AddUpperWidget(box);
		}
		
		protected override void AddItem(int index) {
			UpdateModel();
			Edited = true;
		}

		protected override void EditSelected() {
			LongoMatch.Gui.Dialog.EditPlayerDialog dialog = new LongoMatch.Gui.Dialog.EditPlayerDialog();
			dialog.Player=selected[0];
			dialog.TransientFor = (Gtk.Window) Toplevel;
			dialog.Run();
			dialog.Destroy();
			Edited = true;
		}

		protected virtual void OnPlayerClicked(LongoMatch.Store.Player player)
		{
			selected = new List<Player>();
			selected.Add(player);
			EditSelected();
		}

		protected virtual void OnPlayersSelected(LongoMatch.Store.Player player)
		{
			selected = new List<Player>();
			selected.Add(player);
		}
	}
}
