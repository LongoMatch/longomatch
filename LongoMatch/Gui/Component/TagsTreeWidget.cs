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
using LongoMatch.Store;

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
			model = new Gtk.ListStore(typeof(Play));			
			filter = new Gtk.TreeModelFilter(model, null);
			filter.VisibleFunc = new Gtk.TreeModelFilterVisibleFunc(FilterTree);
			treeview.Model = filter;
			filtercombobox.InsertText ((int)FilterType.OR, Catalog.GetString(orFilter));
			filtercombobox.InsertText ((int)FilterType.AND, Catalog.GetString(andFilter));
			filtercombobox.Active = 0;
			filterType = FilterType.OR;
			treeview.TimeNodeChanged += OnTimeNodeChanged;
            treeview.TimeNodeSelected += OnTimeNodeSelected;
            treeview.PlayListNodeAdded += OnPlayListNodeAdded;
            treeview.SnapshotSeriesEvent += OnSnapshotSeriesEvent;
			
		}
		
		public void Clear(){
			model.Clear();
			filterTags.Clear();
			filter.Refilter();
		}

		public void RemovePlays(List<Play> plays) {
			TreeIter iter;
			List<TreeIter> removeIters;
			
			if (project == null)
				return;
			
			removeIters = new List<TreeIter>();
			model.GetIterFirst(out iter);
			
			do {
				Play play = (Play) model.GetValue(iter,0);
				if (plays.Contains(play)) {
					removeIters.Add(iter);
				}
			} while (model.IterNext(ref iter)); 
			
			for (int i=0; i < removeIters.Count; i++){
				iter = removeIters[i];
				model.Remove(ref iter);
			}
		}

		public void AddPlay(Play play) {
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
					foreach (Play play in value.AllPlays())
						model.AppendValues(play);
					
					UpdateTagsList();
					treeview.LocalTeam = project.Description.LocalName;
					treeview.VisitorTeam = project.Description.VisitorName;
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
				tagscombobox.AppendText(tag.Value.ToString());
		}
		
		private void AddFilterWidget(Tag tag){
			HBox box;
			Button b;
			Label l;
			
			box = new HBox();
			box.Name = tag.Value.ToString();
			b = new Button();
			b.Image =  new Image(Stetic.IconLoader.LoadIcon(this, "gtk-delete", Gtk.IconSize.Menu));
			b.Clicked += OnDeleteClicked;
			l = new Label(tag.Value.ToString());
			l.Justify = Justification.Left;
			box.PackEnd(b,false,  false, 0);
			box.PackStart(l,true, true, 0);
			tagsvbox.PackEnd(box);
			box.ShowAll();
		}
		
		protected virtual void OnDeleteClicked (object o, System.EventArgs e){
			Widget parent = (o as Widget).Parent;
		    tagscombobox.AppendText(parent.Name);
			filterTags.Remove(new Tag{Value = parent.Name});
			filter.Refilter();
			tagsvbox.Remove(parent);
		}
			
		protected virtual void OnAddFilter (object sender, System.EventArgs e)
		{
			string text = tagscombobox.ActiveText;
			if (text == null || text == "")
				return;
			
			Tag tag = new Tag{ Value = text};
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
			Play tNode;
			
			if (filterTags.Count == 0)
				return true;
			
			tNode = model.GetValue(iter, 0) as Play;

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

		protected virtual void OnTimeNodeSelected(Play tNode) {
			if (TimeNodeSelected != null)
				TimeNodeSelected(tNode);
		}

		protected virtual void OnPlayListNodeAdded(Play tNode)
		{
			if (PlayListNodeAdded != null)
				PlayListNodeAdded(tNode);
		}

		protected virtual void OnSnapshotSeriesEvent(LongoMatch.Store.Play tNode)
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