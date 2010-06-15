// TreeWidget.cs
//
//  Copyright(C) 20072009 Andoni Morales Alastruey
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
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using LongoMatch.DB;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Component
{
	public enum FilterType {
		OR = 0,
		AND = 1
	}


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TagsTreeWidget : Gtk.Bin
	{

		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlayListNodeAddedHandler PlayListNodeAdded;
		public event SnapshotSeriesHandler SnapshotSeriesEvent;

		private TreeModelFilter filter;
		private ListStore model;
		private List<Tag> filterTags;
		private Project project;
		private FilterType filterType;
		private const string orFilter =  "'OR' Filter";
		private const string andFilter = "'AND' Filter";


		public TagsTreeWidget()
		{
			this.Build();
			filterTags = new List<Tag>();
			model = new Gtk.ListStore(typeof(MediaTimeNode));			
			filter = new Gtk.TreeModelFilter(model, null);
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc(FilterTree);
			treeview.Model = filter;
			filtercombobox.InsertText ((int)FilterType.OR, Catalog.GetString(orFilter));
			filtercombobox.InsertText ((int)FilterType.AND, Catalog.GetString(andFilter));
			filtercombobox.Active = 0;
			filterType = FilterType.OR;
			
		}
		
		public void Clear(){
			model.Clear();
			filterTags.Clear();
			filter.Refilter();
		}

		public void DeletePlay(MediaTimeNode play) {
			if (project != null) {
				TreeIter iter;
				model.GetIterFirst(out iter);
				while (model.IterIsValid(iter)) {
					MediaTimeNode mtn = (MediaTimeNode) model.GetValue(iter,0);
					if (mtn == play) {
						model.Remove(ref iter);
						break;
					}
					TreeIter prev = iter;
					model.IterNext(ref iter);
					if (prev.Equals(iter))
						break;
				}
			}
		}

		public void AddPlay(MediaTimeNode play) {
			model.AppendValues(play);
			filter.Refilter();
		}
		
		public bool ProjectIsLive{
			set{
				treeview.ProjectIsLive = value;
			}
		}

		public Project Project {
			set {
				project = value;
				if (project != null) {
					model.Clear();
					foreach (List<MediaTimeNode> list in project.GetDataArray()){
						foreach (MediaTimeNode tNode in list)
							model.AppendValues(tNode);
					}
					UpdateTagsList();
				}
			}
		}

		public bool PlayListLoaded {
			set {
				treeview.PlayListLoaded=value;
			}
		}
		
		public void UpdateTagsList(){
			(tagscombobox.Model as ListStore).Clear();
			foreach (Tag tag in project.Tags)
				tagscombobox.AppendText(tag.Text);
		}
		
		private void AddFilterWidget(Tag tag){
			HBox box;
			Button b;
			Label l;
			
			box = new HBox();
			box.Name = tag.Text;
			b = new Button();
			b.Image =  new Image(Stetic.IconLoader.LoadIcon(this, "gtk-delete", Gtk.IconSize.Menu, 16));
			b.Clicked += OnDeleteClicked;
			l = new Label(tag.Text);
			l.Justify = Justification.Left;
			box.PackEnd(b,false,  false, 0);
			box.PackStart(l,true, true, 0);
			tagsvbox.PackEnd(box);
			box.ShowAll();
		}
		
		protected virtual void OnDeleteClicked (object o, System.EventArgs e){
			Widget parent = (o as Widget).Parent;
		    tagscombobox.AppendText(parent.Name);
			filterTags.Remove(new Tag(parent.Name));
			filter.Refilter();
			tagsvbox.Remove(parent);
		}
			
		protected virtual void OnAddFilter (object sender, System.EventArgs e)
		{
			string text = tagscombobox.ActiveText;
			if (text == null || text == "")
				return;
			
			Tag tag = new Tag(text);
			if (!filterTags.Contains(tag)){
				filterTags.Add(tag);
				tagscombobox.RemoveText(tagscombobox.Active);
				AddFilterWidget(tag);
				filter.Refilter();
			}
		}

		protected virtual void OnClearbuttonClicked (object sender, System.EventArgs e)
		{
			filterTags.Clear();
			filter.Refilter();
			foreach (Widget w in tagsvbox.Children)
				tagsvbox.Remove(w);
			UpdateTagsList();
		}
		
		private bool FilterTree(Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			MediaTimeNode tNode;
			
			if (filterTags.Count == 0)
				return true;
			
			tNode = model.GetValue(iter, 0) as MediaTimeNode;

			if (tNode == null)
				return true;

			if (filterType == FilterType.OR){
				foreach (Tag tag in filterTags){
					if (tNode.Tags.Contains(tag))
						return true;
				}
				return false;
			} else {
				foreach (Tag tag in filterTags){
					if (! tNode.Tags.Contains(tag))
						return false;
				}
				return true;
			}
		}
		
		protected virtual void OnTimeNodeChanged(TimeNode tNode,object val) {
			if (TimeNodeChanged != null)
				TimeNodeChanged(tNode,val);
		}

		protected virtual void OnTimeNodeSelected(MediaTimeNode tNode) {
			if (TimeNodeSelected != null)
				TimeNodeSelected(tNode);
		}

		protected virtual void OnPlayListNodeAdded(MediaTimeNode tNode)
		{
			if (PlayListNodeAdded != null)
				PlayListNodeAdded(tNode);
		}

		protected virtual void OnSnapshotSeriesEvent(LongoMatch.TimeNodes.MediaTimeNode tNode)
		{
			if (SnapshotSeriesEvent != null)
				SnapshotSeriesEvent(tNode);
		}
		
		protected virtual void OnFiltercomboboxChanged (object sender, System.EventArgs e)
		{
			filterType = (FilterType) filtercombobox.Active;
			filter.Refilter();
		}
		
		
	}
}