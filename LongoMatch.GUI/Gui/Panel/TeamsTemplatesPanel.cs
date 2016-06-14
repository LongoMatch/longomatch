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
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Gui.Dialog;
using Pango;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Serialization;
using Constants = LongoMatch.Core.Common.Constants;
using Helpers = VAS.UI.Helpers;
using Image = VAS.Core.Common.Image;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TeamsTemplatesPanel : Gtk.Bin, IPanel
	{
		public event BackEventHandle BackEvent;

		const int COL_PIXBUF = 0;
		const int COL_NAME = 1;
		const int COL_TEAM = 2;

		ListStore teamsStore;
		SportsTeam loadedTeam;
		ITeamTemplatesProvider provider;
		TreeIter selectedIter;
		List<SportsTeam> teams;

		public TeamsTemplatesPanel ()
		{
			this.Build ();
			provider = App.Current.TeamTemplatesProvider;
			teamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-team-header", StyleConf.TemplatesHeaderIconSize);
			playerheaderimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-player-header", StyleConf.TemplatesHeaderIconSize);
			newteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-add", StyleConf.TemplatesIconSize);
			importteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-import", StyleConf.TemplatesIconSize);
			exportteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-export", StyleConf.TemplatesIconSize);
			deleteteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-delete", StyleConf.TemplatesIconSize);
			saveteamimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-save", StyleConf.TemplatesIconSize);
			newplayerimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-add", StyleConf.TemplatesIconSize);
			deleteplayerimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-delete", StyleConf.TemplatesIconSize);
			vseparatorimage.Pixbuf = Helpers.Misc.LoadIcon ("vertical-separator", StyleConf.TemplatesIconSize);

			newteambutton.Entered += HandleEnterTeamButton;
			newteambutton.Left += HandleLeftTeamButton;
			newteambutton.Clicked += HandleNewTeamClicked;
			importteambutton.Entered += HandleEnterTeamButton;
			importteambutton.Left += HandleLeftTeamButton;
			importteambutton.Clicked += HandleImportTeamClicked;
			exportteambutton.Entered += HandleEnterTeamButton;
			exportteambutton.Left += HandleLeftTeamButton;
			exportteambutton.Clicked += HandleExportTeamClicked;
			deleteteambutton.Entered += HandleEnterTeamButton;
			deleteteambutton.Left += HandleLeftTeamButton;
			deleteteambutton.Clicked += HandleDeleteTeamClicked;
			saveteambutton.Entered += HandleEnterTeamButton;
			saveteambutton.Left += HandleLeftTeamButton;
			saveteambutton.Clicked += (s, e) => {
				PromptSave (false);
			};
			newplayerbutton1.Entered += HandleEnterPlayerButton;
			newplayerbutton1.Left += HandleLeftPlayerButton;
			newplayerbutton1.Clicked += (object sender, EventArgs e) => {
				teamtemplateeditor1.AddPlayer ();
			};
			deleteplayerbutton.Entered += HandleEnterPlayerButton;
			deleteplayerbutton.Left += HandleLeftPlayerButton;
			deleteplayerbutton.Clicked += (object sender, EventArgs e) => {
				teamtemplateeditor1.DeleteSelectedPlayers ();
			};

			teamsStore = new ListStore (typeof(Pixbuf), typeof(string), typeof(SportsTeam));
			
			var cell = new CellRendererText ();
			cell.Editable = true;
			cell.Edited += HandleEdited;
			teamseditortreeview.Model = teamsStore;
			teamseditortreeview.HeadersVisible = false;
			teamseditortreeview.AppendColumn ("Icon", new CellRendererPixbuf (), "pixbuf", COL_PIXBUF);
			teamseditortreeview.AppendColumn ("Text", cell, "text", COL_NAME);
			teamseditortreeview.SearchColumn = COL_NAME;
			teamseditortreeview.EnableGridLines = TreeViewGridLines.None;
			teamseditortreeview.CursorChanged += HandleSelectionChanged;
			
			teamsvbox.WidthRequest = 280;
			
			teamtemplateeditor1.Visible = false;
			newteambutton.Visible = true;
			deleteteambutton.Visible = false;
			
			teamtemplateeditor1.VisibleButtons = false;

			panelheader1.ApplyVisible = false;
			panelheader1.Title = Catalog.GetString ("TEAMS MANAGER");
			panelheader1.BackClicked += (sender, o) => {
				PromptSave (true);
				if (BackEvent != null)
					BackEvent ();
			};
			
			editteamslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));
			editplayerslabel.ModifyFont (FontDescription.FromString (App.Current.Style.Font + " 9"));

			Load (null);
		}

		public override void Destroy ()
		{
			teamtemplateeditor1.Destroy ();
			base.Destroy ();
		}

		public void OnLoaded ()
		{

		}

		public void OnUnloaded ()
		{

		}

		void Load (string templateName)
		{
			TreeIter templateIter = TreeIter.Zero;
			bool first = true;

			teams = new List<SportsTeam> ();
			teamsStore.Clear ();
			foreach (SportsTeam team in provider.Templates) {
				Pixbuf img;
				TreeIter iter;
				string name = team.Name;

				if (team.Shield != null) {
					img = team.Shield.Scale (StyleConf.TeamsShieldIconSize,
						StyleConf.TeamsShieldIconSize).Value;
				} else {
					img = Helpers.Misc.LoadIcon ("longomatch-default-shield",
						StyleConf.TeamsShieldIconSize);
				}
				if (team.Static) {
					name += " (" + Catalog.GetString ("System") + ")";
				} else {
					teams.Add (team);
				}
				iter = teamsStore.AppendValues (img, team.Name, team);
				if (first || team.Name == templateName) {
					templateIter = iter;
				}
				first = false;
			}
			if (teamsStore.IterIsValid (templateIter)) {
				teamseditortreeview.Selection.SelectIter (templateIter);
				HandleSelectionChanged (null, null);
			}
		}

		bool SaveTemplate (SportsTeam template)
		{
			try {
				provider.Save (template);
				return true;
			} catch (InvalidTemplateFilenameException ex) {
				App.Current.GUIToolkit.ErrorMessage (ex.Message, this);
				return false;
			}
		}

		void HandleEnterTeamButton (object sender, EventArgs e)
		{
			if (sender == newteambutton) {
				editteamslabel.Markup = Catalog.GetString ("New team");
			} else if (sender == exportteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Export team");
			} else if (sender == deleteteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Delete team");
			} else if (sender == saveteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Save team");
			} else if (sender == importteambutton) {
				editteamslabel.Markup = Catalog.GetString ("Import team");
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

			if (!SaveTemplate (loadedTeam)) {
				return;
			}
			/* Update the shield and the team in our model */
			Pixbuf shield = loadedTeam.Shield.Scale (StyleConf.TeamsShieldIconSize,
				                StyleConf.TeamsShieldIconSize).Value;
			teamseditortreeview.Model.SetValue (selectedIter, COL_PIXBUF, shield);
			teamseditortreeview.Model.SetValue (selectedIter, COL_TEAM, loadedTeam);
			teamtemplateeditor1.Edited = false;
			//Replace the old team with the new one with the new attributes
			teams.RemoveAll (t => t.ID == loadedTeam.ID);
			teams.Add (loadedTeam);
		}

		void SaveStatic ()
		{
			string msg = Catalog.GetString ("System teams can't be edited, do you want to create a copy?");
			if (App.Current.GUIToolkit.QuestionMessage (msg, null, this).Result) {
				string newName;
				while (true) {
					newName = App.Current.GUIToolkit.QueryMessage (Catalog.GetString ("Name:"), null,
						loadedTeam.Name + "_copy", this).Result;
					if (newName == null)
						break;
					if (teams.Any (t => t.Name == newName)) {
						msg = Catalog.GetString ("A team with the same name already exists"); 
						App.Current.GUIToolkit.ErrorMessage (msg, this);
					} else {
						break;
					}
				}
				if (newName == null) {
					return;
				}
				SportsTeam newTeam = loadedTeam.Clone ();
				newTeam.ID = Guid.NewGuid ();
				newTeam.Name = newName;
				newTeam.Static = false;
				if (SaveTemplate (newTeam)) {
					Load (newTeam.Name);
				}
			}
		}

		void PromptSave (bool prompt)
		{
			if (loadedTeam != null && teamtemplateeditor1.Edited) {
				if (loadedTeam.Static) {
					if (!prompt) {
						SaveStatic ();
					}
				} else if (prompt) {
					string msg = Catalog.GetString ("Do you want to save the current template");
					if (App.Current.GUIToolkit.QuestionMessage (msg, null, this).Result) {
						SaveLoadedTeam ();
					}
				} else {
					SaveLoadedTeam ();
				}
			}
		}

		void LoadTeam (SportsTeam team, TreeIter selectedIter)
		{
			PromptSave (true);

			this.selectedIter = selectedIter;
			loadedTeam = team;
			team.TemplateEditorMode = true;
			teamtemplateeditor1.Team = loadedTeam;
		}

		void HandleSelectionChanged (object sender, EventArgs e)
		{
			SportsTeam selected;
			TreeIter iter;

			teamseditortreeview.Selection.GetSelected (out iter);
			try {
				SportsTeam team = teamsStore.GetValue (iter, COL_TEAM) as SportsTeam;
				team.Load ();
				selected = team.Clone ();
			} catch (Exception ex) {
				Log.Exception (ex);
				App.Current.GUIToolkit.ErrorMessage (Catalog.GetString ("Could not load team"));
				return;
			}
			deleteteambutton.Visible = selected != null;
			teamtemplateeditor1.Visible = selected != null;
			if (selected != null) {
				LoadTeam (selected, iter);
			}
		}

		void HandleImportTeamClicked (object sender, EventArgs e)
		{
			string fileName, filterName;
			string[] extensions;

			Log.Debug ("Importing team");
			filterName = Catalog.GetString ("Team files");
			extensions = new [] { "*" + Constants.TEAMS_TEMPLATE_EXT };
			/* Show a file chooser dialog to select the file to import */
			fileName = App.Current.GUIToolkit.OpenFile (Catalog.GetString ("Import team"), null, App.Current.HomeDir,
				filterName, extensions);

			if (fileName == null)
				return;

			try {
				SportsTeam newTeam = provider.LoadFile (fileName);

				if (newTeam != null) {
					bool abort = false;

					while (provider.Exists (newTeam.Name) && !abort) {
						string name = App.Current.GUIToolkit.QueryMessage (Catalog.GetString ("Team name:"),
							              Catalog.GetString ("Name conflict"), newTeam.Name + "#").Result;
						if (name == null) {
							abort = true;
						} else {
							newTeam.Name = name;
						}
					}

					if (!abort) {
						Pixbuf img;

						provider.Save (newTeam);
						if (newTeam.Shield != null) {
							img = newTeam.Shield.Value;
						} else {
							img = Helpers.Misc.LoadIcon ("longomatch-default-shield", StyleConf.TeamsShieldIconSize);
						}

						teamsStore.AppendValues (img, newTeam.Name, newTeam);
						Load (newTeam.Name);
					}
				}
			} catch (Exception ex) {
				App.Current.GUIToolkit.ErrorMessage (Catalog.GetString ("Error importing team:") +
				"\n" + ex.Message);
				Log.Exception (ex);
				return;
			}
		}

		void HandleDeleteTeamClicked (object sender, EventArgs e)
		{
			if (loadedTeam != null) {
				if (loadedTeam.Static) {
					string msg = Catalog.GetString ("System teams can't be deleted");
					Helpers.MessagesHelpers.WarningMessage (this, msg);
					return;
				} else {
					string msg = Catalog.GetString ("Do you really want to delete the template: ") + loadedTeam.Name;
					if (Helpers.MessagesHelpers.QuestionMessage (this, msg, null)) {
						provider.Delete (loadedTeam);
						teamsStore.Remove (ref selectedIter);
						teams.Remove (loadedTeam);
						selectedIter = TreeIter.Zero;
						teamseditortreeview.Selection.SelectPath (new TreePath ("0"));
						HandleSelectionChanged (null, null);
					}
				}
			}
		}

		void HandleNewTeamClicked (object sender, EventArgs e)
		{
			bool create = false;
			SportsTeam auxdelete = null;
			
			EntryDialog dialog = new EntryDialog (Toplevel as Gtk.Window);
			dialog.ShowCount = true;
			dialog.Title = dialog.Text = Catalog.GetString ("New team");
			dialog.SelectText ();
			dialog.AvailableTemplates = teams.Select (t => t.Name).ToList ();
			
			while (dialog.Run () == (int)ResponseType.Ok) {
				if (dialog.Text == "") {
					Helpers.MessagesHelpers.ErrorMessage (dialog, Catalog.GetString ("The template name is empty."));
					continue;
				} else if (dialog.Text == "default") {
					Helpers.MessagesHelpers.ErrorMessage (dialog, Catalog.GetString ("The template can't be named 'default'."));
					continue;
				} else if (provider.Exists (dialog.Text)) {
					var msg = Catalog.GetString ("The template already exists. Do you want to overwrite it?");
					if (Helpers.MessagesHelpers.QuestionMessage (this, msg)) {
						create = true;
						auxdelete = teams.FirstOrDefault (t => t.Name == dialog.Text);
						break;
					}
				} else {
					create = true;
					break;
				}
			}
			
			if (create) {
				if (dialog.SelectedTemplate != null) {
					provider.Copy (teams.FirstOrDefault (t => t.Name == dialog.SelectedTemplate), dialog.Text);
				} else {
					SportsTeam team;
					team = SportsTeam.DefaultTemplate (dialog.Count);
					team.TeamName = dialog.Text;
					team.Name = dialog.Text;
					if (!SaveTemplate (team)) {
						dialog.Destroy ();
						return;
					}
				}
				if (auxdelete != null) {
					provider.Delete (auxdelete);
				}
				Load (dialog.Text);
			}
			dialog.Destroy ();
		}

		void HandleExportTeamClicked (object sender, EventArgs e)
		{
			string fileName, filterName;
			string[] extensions;

			Log.Debug ("Exporting team");
			filterName = Catalog.GetString ("Team files");
			extensions = new [] { "*" + Constants.TEAMS_TEMPLATE_EXT };
			/* Show a file chooser dialog to select the file to export */
			fileName = App.Current.GUIToolkit.SaveFile (Catalog.GetString ("Export team"),
				System.IO.Path.ChangeExtension (loadedTeam.Name, Constants.TEAMS_TEMPLATE_EXT), App.Current.HomeDir,
				filterName, extensions);

			if (fileName != null) {
				bool succeeded = true;
				fileName = System.IO.Path.ChangeExtension (fileName, Constants.TEAMS_TEMPLATE_EXT);
				if (System.IO.File.Exists (fileName)) {
					string msg = Catalog.GetString ("A file with the same name already exists, do you want to overwrite it?");
					succeeded = App.Current.GUIToolkit.QuestionMessage (msg, null).Result;
				}

				if (succeeded) {
					Serializer.Instance.Save (loadedTeam, fileName);
					string msg = Catalog.GetString ("Team exported correctly");
					App.Current.GUIToolkit.InfoMessage (msg);
				}
			}

		}

		void HandleEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			teamsStore.GetIter (out iter, new TreePath (args.Path));
 
			SportsTeam team = teamsStore.GetValue (iter, COL_TEAM) as SportsTeam;
			if (team.Name != args.NewText) {
				if (teams.Any (t => t.Name == args.NewText)) {
					App.Current.GUIToolkit.ErrorMessage (
						Catalog.GetString ("A team with the same name already exists"), this);
					args.RetVal = false;
				} else {
					try {
						team.Name = args.NewText;
						provider.Save (team);
						teamsStore.SetValue (iter, COL_NAME, team.Name);
					} catch (Exception ex) {
						App.Current.GUIToolkit.ErrorMessage (ex.Message);
					}
				}
			}
		}
	}
}

