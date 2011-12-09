// 
//  Copyright (C) 2011 Andoni Morales Alastruey
// 
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
// 

using System;
using System.Collections.Generic;
using Gdk;
using Gtk;
using Mono.Unix;
using Stetic;

using Image = LongoMatch.Common.Image;
using LongoMatch.Common;
using LongoMatch.Gui.Base;
using LongoMatch.Gui.Dialog;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Gui.Component
{
	public class TeamTemplateEditorWidget: TemplatesEditorWidget<TeamTemplate, Player>
	{	
		PlayerPropertiesTreeView treeview;
		Entry teamentry;
		Gtk.Image shieldImage;
		VBox box;
		
		public TeamTemplateEditorWidget (ITemplateProvider<TeamTemplate, Player> provider): base(provider) {
			treeview = new PlayerPropertiesTreeView(); 
			treeview.PlayerClicked += this.OnPlayerClicked;
			treeview.PlayersSelected += this.OnPlayersSelected;
			FirstPageName = Catalog.GetString("Teams players");
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
				teamentry.Text = template.TeamName;
				if (template.Shield != null) {
					shieldImage.Pixbuf = template.Shield.Value;
				}
				box.Sensitive = true;
			}
		}
		
		private void AddTeamNamesWidget () {
			Gtk.Frame sframe, tframe;
			EventBox ebox;
			
			sframe = new Gtk.Frame("<b>" + Catalog.GetString("Shield") + "</b>");
			(sframe.LabelWidget as Label).UseMarkup = true;
			sframe.ShadowType = ShadowType.None;
			tframe = new Gtk.Frame("<b>" + Catalog.GetString("Team Name") + "</b>");
			(tframe.LabelWidget as Label).UseMarkup = true;
			tframe.ShadowType = ShadowType.None;
			
			ebox = new EventBox();
			ebox.ButtonPressEvent += OnImageClicked;
			
			shieldImage = new Gtk.Image();
			shieldImage.Pixbuf = IconLoader.LoadIcon(this, "gtk-execute", IconSize.Dialog);
			box = new VBox();
			
			teamentry = new Entry ();
			teamentry.Changed += delegate(object sender, EventArgs e) {
				Template.TeamName = teamentry.Text;
			};
			
			sframe.Add(ebox);
			ebox.Add(shieldImage);
			tframe.Add(teamentry);
			
			box.PackStart (sframe, false, false, 0);
			box.PackStart (tframe, false, false, 0);
			box.ShowAll();
			box.Sensitive = false;
			AddUpperWidget(box);
		}
		
		protected override void EditSelected() {
			LongoMatch.Gui.Dialog.EditPlayerDialog dialog = new LongoMatch.Gui.Dialog.EditPlayerDialog();
			dialog.Player=selected[0];
			dialog.TransientFor = (Gtk.Window) Toplevel;
			dialog.Run();
			dialog.Destroy();
			Edited = true;
		}
		
		protected virtual void OnImageClicked (object sender, EventArgs args)
		{
			Pixbuf shield;
			
			shield = Helpers.OpenImage((Gtk.Window)this.Toplevel);
			if (shield != null) {
				Template.Shield = new Image(shield);
				shieldImage.Pixbuf = shield;
			}
		}

		protected virtual void OnPlayerClicked(Player player)
		{
			selected = new List<Player>();
			selected.Add(player);
			EditSelected();
		}

		protected virtual void OnPlayersSelected(List<Player> players)
		{
			selected = players;
			
			if(selected.Count == 0) {
				ButtonsSensitive = false;
			} else if(selected.Count == 1) {
				ButtonsSensitive = true;
			} else {
				MultipleSelection();
			}
		}
		
		protected override void RemoveSelected (){
			if(Project != null) {
				MessageDialog dialog = new MessageDialog((Gtk.Window)this.Toplevel,DialogFlags.Modal,MessageType.Question,
				                                         ButtonsType.YesNo,true,
				                                         Catalog.GetString("You are about to delete a player and all " +
				                                         	"its tags. Do you want to proceed?"));
				if(dialog.Run() == (int)ResponseType.Yes) {
					try {
						foreach(var player in selected)
							Project.RemovePlayer (template, player);
					} catch {
						MessagePopup.PopupMessage(this,MessageType.Warning,
						                          Catalog.GetString("A template needs at least one category"));
					}
				}
				dialog.Destroy();
			} else {
				try {
					foreach(var player in selected)
					Template.Remove(player);
				} catch {
					MessagePopup.PopupMessage(this,MessageType.Warning,
					                          Catalog.GetString("A template needs at least one category"));
				}
			}
			base.RemoveSelected();
		}
	}
}
