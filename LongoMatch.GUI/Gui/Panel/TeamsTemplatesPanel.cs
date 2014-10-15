//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Gui.Helpers;
using Mono.Unix;
using Pango;
using LongoMatch.Gui.Dialog;
using Image = LongoMatch.Core.Common.Image;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TeamsTemplatesPanel : Gtk.Bin, IPanel
	{
		public event BackEventHandle BackEvent;

		ListStore teams;
		TeamTemplate loadedTeam;
		Dictionary<string, TreeIter> itersDict;
		ITeamTemplatesProvider provider;

		public TeamsTemplatesPanel ()
		{
			this.Build ();
			provider = Config.TeamTemplatesProvider;
			teamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-team-header", 45, IconLookupFlags.ForceSvg);
			playerheaderimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-player-header", 45, IconLookupFlags.ForceSvg);
			newteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-team-add", 34, IconLookupFlags.ForceSvg);
			deleteteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-team-delete", 34, IconLookupFlags.ForceSvg);
			saveteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-team-save", 34, IconLookupFlags.ForceSvg);
			newplayerimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-player-add", 34, IconLookupFlags.ForceSvg);
			deleteplayerimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-player-delete", 34, IconLookupFlags.ForceSvg);
			vseparatorimage.Pixbuf = Helpers.Misc.LoadIcon ("vertical-separator", 34, IconLookupFlags.ForceSvg);

			newteambutton.Entered += HandleEnterTeamButton;
			newteambutton.Left += HandleLeftTeamButton;
			newteambutton.Clicked += HandleNewTeamClicked;
			deleteteambutton.Entered += HandleEnterTeamButton;
			deleteteambutton.Left += HandleLeftTeamButton;
			deleteteambutton.Clicked += HandleDeleteTeamClicked;
			saveteambutton.Entered += HandleEnterTeamButton;
			saveteambutton.Left += HandleLeftTeamButton;
			saveteambutton.Clicked += (s, e) => {
				SaveLoadedTeam ();};
			newplayerbutton1.Entered += HandleEnterPlayerButton;
			newplayerbutton1.Left += HandleLeftPlayerButton;
			newplayerbutton1.Clicked += (object sender, EventArgs e) => {
				teamtemplateeditor1.AddPlayer (); };
			deleteplayerbutton.Entered += HandleEnterPlayerButton;
			deleteplayerbutton.Left += HandleLeftPlayerButton;
			deleteplayerbutton.Clicked += (object sender, EventArgs e) => {
				teamtemplateeditor1.DeleteSelectedPlayers (); };

			teams = new ListStore (typeof(Pixbuf), typeof(string), typeof(TeamTemplate));
			itersDict = new Dictionary<string, TreeIter> ();
			
			var cell = new CellRendererText ();
			cell.Editable = true;
			cell.Edited += HandleEdited;
			teamseditortreeview.Model = teams;
			teamseditortreeview.HeadersVisible = false;
			teamseditortreeview.AppendColumn ("Icon", new CellRendererPixbuf (), "pixbuf", 0); 
			teamseditortreeview.AppendColumn ("Text", cell, "text", 1); 
			teamseditortreeview.SearchColumn = 1;
			teamseditortreeview.TooltipColumn = 2;
			teamseditortreeview.EnableGridLines = TreeViewGridLines.None;
			teamseditortreeview.CursorChanged += HandleSelectionChanged;
			
			teamsvbox.WidthRequest = 280;
			
			teamtemplateeditor1.Visible = false;
			newteambutton.Visible = true;
			deleteteambutton.Visible = false;
			
			teamtemplateeditor1.VisibleButtons = false;
			teamtemplateeditor1.TemplateSaved += (s, e) => {
				SaveLoadedTeam ();};

			panelheader1.ApplyVisible = false;
			panelheader1.Title = "TEAMS MANAGER";
			panelheader1.BackClicked += (sender, o) => {
				PromptSave ();
				if (BackEvent != null)
					BackEvent ();
			};
			
			editteamslabel.ModifyFont (FontDescription.FromString (Config.Style.Font + " 8"));
			editplayerslabel.ModifyFont (FontDescription.FromString (Config.Style.Font + " 8"));

			Load (null);
		}

		public override void Destroy ()
		{
			teamtemplateeditor1.Destroy ();
			base.Destroy ();
		}

		void Load (string templateName)
		{
			TreeIter templateIter = TreeIter.Zero;
			bool first = true;
			
			teams.Clear ();
			itersDict.Clear ();
			foreach (TeamTemplate template in provider.Templates) {
				Pixbuf img;
				TreeIter iter;
				
				if (template.Shield != null) {
					img = template.Shield.Scale (StyleConf.TeamsShieldIconSize, StyleConf.TeamsShieldIconSize).Value;
				} else {
					img = Helpers.Misc.LoadIcon (Constants.LOGO_ICON,
					                             StyleConf.TeamsShieldIconSize,
					                             IconLookupFlags.ForceSvg);
				}
				iter = teams.AppendValues (img, template.Name, template);
				itersDict.Add (template.Name, iter);
				if (first || template.Name == templateName) {
					templateIter = iter;
				}
				first = false;
			}
			if (teams.IterIsValid (templateIter)) {
				teamseditortreeview.Selection.SelectIter (templateIter);
				HandleSelectionChanged (null, null);
			}
		}

		void HandleEnterTeamButton (object sender, EventArgs e)
		{
			if (sender == newteambutton) {
				editteamslabel.Markup = Catalog.GetString ("New team");
			} else if (sender == deleteteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Delete team");
			} else if (sender == saveteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Save team");
			}
		}

		void HandleLeftTeamButton (object sender, EventArgs e)
		{
			editteamslabel.Markup = Catalog.GetString ("Manage teams");
		}

		void HandleEnterPlayerButton (object sender, EventArgs e)
		{
			if (sender == newplayerbutton1) {
				editplayerslabel.Markup = Catalog.GetString ("New player");
			} else if (sender == deleteplayerbutton) {
				editplayerslabel.Markup = Catalog.GetString ("Delete player");
			}
		}

		void HandleLeftPlayerButton (object sender, EventArgs e)
		{
			editplayerslabel.Markup = Catalog.GetString ("Manage players");
		}

		void SaveLoadedTeam ()
		{
			if (loadedTeam == null)
				return;

			provider.Update (loadedTeam);
			/* The shield might have changed, update it just in case */
			if (loadedTeam.Shield != null) {
				teamseditortreeview.Model.SetValue (itersDict [loadedTeam.Name], 0,
				                                    loadedTeam.Shield.Scale (StyleConf.TeamsShieldIconSize, StyleConf.TeamsShieldIconSize).Value);
			}
			teamtemplateeditor1.Edited = false;
		}

		void PromptSave ()
		{
			if (loadedTeam != null) {
				if (teamtemplateeditor1.Edited) {
					string msg = Catalog.GetString ("Do you want to save the current template");
					if (Config.GUIToolkit.QuestionMessage (msg, null, this)) {
						SaveLoadedTeam ();
					}
				}
			}
		}

		void LoadTeam (TeamTemplate team)
		{
			PromptSave ();
			
			loadedTeam = team;
			teamtemplateeditor1.Team = loadedTeam;
		}

		void HandleSelectionChanged (object sender, EventArgs e)
		{
			TeamTemplate selected;
			TreeIter iter;

			teamseditortreeview.Selection.GetSelected (out iter);
			selected = teams.GetValue (iter, 2) as TeamTemplate;
			deleteteambutton.Visible = selected != null;
			teamtemplateeditor1.Visible = selected != null;
			if (selected != null) {
				LoadTeam (selected);
			}
		}

		void HandleDeleteTeamClicked (object sender, EventArgs e)
		{
			if (loadedTeam != null) {
				if (loadedTeam.Name == "default") {
					MessagesHelpers.ErrorMessage (this,
					                              Catalog.GetString ("The default team can't be deleted"));
				}
				string msg = Catalog.GetString ("Do you really want to delete the template: ") + loadedTeam.Name;
				if (MessagesHelpers.QuestionMessage (this, msg, null)) {
					provider.Delete (loadedTeam.Name);
				}
			}
			Load ("default");
		}

		void HandleNewTeamClicked (object sender, EventArgs e)
		{
			bool create = false;
			
			EntryDialog dialog = new EntryDialog ();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;
			dialog.ShowCount = true;
			dialog.Text = Catalog.GetString ("New team");
			dialog.AvailableTemplates = provider.TemplatesNames;
			
			while (dialog.Run() == (int)ResponseType.Ok) {
				if (dialog.Text == "") {
					MessagesHelpers.ErrorMessage (dialog, Catalog.GetString ("The template name is empty."));
					continue;
				} else if (dialog.Text == "default") {
					MessagesHelpers.ErrorMessage (dialog, Catalog.GetString ("The template can't be named 'default'."));
					continue;
				} else if (provider.Exists (dialog.Text)) {
					var msg = Catalog.GetString ("The template already exists. " +
						"Do you want to overwrite it ?");
					if (MessagesHelpers.QuestionMessage (this, msg)) {
						create = true;
						break;
					}
				} else {
					create = true;
					break;
				}
			}
			
			if (create) {
				if (dialog.SelectedTemplate != null) {
					provider.Copy (dialog.SelectedTemplate, dialog.Text);
				} else {
					TeamTemplate team;
					team = TeamTemplate.DefaultTemplate (dialog.Count);
					team.TeamName = dialog.Text;
					team.Name = dialog.Text;
					provider.Update (team);
				}
				Load (dialog.Text);
			}
			dialog.Destroy ();
		}

		void HandleEdited (object o, EditedArgs args)
		{
			Gtk.TreeIter iter;
			teams.GetIter (out iter, new Gtk.TreePath (args.Path));
 
			TeamTemplate team = (TeamTemplate)teams.GetValue (iter, 2);
			if (team.Name != args.NewText) {
				if (provider.TemplatesNames.Contains (args.NewText)) {
					Config.GUIToolkit.ErrorMessage (Catalog.GetString ("A team with the same name already exists"), this);
					args.RetVal = false;
				} else {
					provider.Delete (team.Name);
					team.Name = args.NewText;
					provider.Save (team);
					teams.SetValue (iter, 1, team.Name);
				}
			}
		}
	}
}

