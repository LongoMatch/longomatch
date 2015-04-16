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
using System.Linq;
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

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class SportsTemplatesPanel : Gtk.Bin, IPanel
	{
		public event BackEventHandle BackEvent;

		ListStore templates;
		Dashboard loadedTemplate;
		ICategoriesTemplatesProvider provider;
		TreeIter selectedIter;
		List<string> templatesNames;

		public SportsTemplatesPanel ()
		{
			this.Build ();
			provider = Config.CategoriesTemplatesProvider;

			// Assign images
			panelheader1.ApplyVisible = false;
			panelheader1.Title = Catalog.GetString ("ANALYSIS DASHBOARDS MANAGER");
			panelheader1.BackClicked += (sender, o) => {
				Save (true);
				if (BackEvent != null)
					BackEvent ();
			};

			templateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-template-header", 54, IconLookupFlags.ForceSvg);
			categoryheaderimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-category-header", 47, IconLookupFlags.ForceSvg);
			newtemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-template-add", 36, IconLookupFlags.ForceSvg);
			importtemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-template-import", 36, IconLookupFlags.ForceSvg);
			deletetemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-template-delete", 36, IconLookupFlags.ForceSvg);
			savetemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-template-save", 36, IconLookupFlags.ForceSvg);
			addcategoryimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-category", 36, IconLookupFlags.ForceSvg);
			addtagimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-tag", 36, IconLookupFlags.ForceSvg);
			scoreimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-score", 36, IconLookupFlags.ForceSvg);
			cardimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-card", 36, IconLookupFlags.ForceSvg);
			timerimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-tag-timer", 36, IconLookupFlags.ForceSvg);
			vseparatorimage.Pixbuf = Helpers.Misc.LoadIcon ("vertical-separator", 54, IconLookupFlags.ForceSvg);

			// Connect buttons from the bar
			newtemplatebutton.Entered += HandleEnterTemplateButton;
			newtemplatebutton.Left += HandleLeftTemplateButton;
			importtemplatebutton.Entered += HandleEnterTemplateButton;
			importtemplatebutton.Left += HandleLeftTemplateButton;
			deletetemplatebutton.Entered += HandleEnterTemplateButton;
			deletetemplatebutton.Left += HandleLeftTemplateButton;
			savetemplatebutton.Entered += HandleEnterTemplateButton;
			savetemplatebutton.Left += HandleLeftTemplateButton;

			addcategorybutton.Entered += HandleEnterTagButton;
			addcategorybutton.Left += HandleLeftTagButton;
			addcategorybutton.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Category");
			addtagbutton1.Entered += HandleEnterTagButton;
			addtagbutton1.Left += HandleLeftTagButton;
			addtagbutton1.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Tag");
			scorebutton.Entered += HandleEnterTagButton;
			scorebutton.Left += HandleLeftTagButton;
			scorebutton.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Score");
			cardbutton.Entered += HandleEnterTagButton;
			cardbutton.Left += HandleLeftTagButton;
			cardbutton.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Card");
			timerbutton.Entered += HandleEnterTagButton;
			timerbutton.Left += HandleLeftTagButton;
			timerbutton.Clicked += (object sender, EventArgs e) =>
				buttonswidget.AddButton ("Timer");

			templates = new ListStore (typeof(Pixbuf), typeof(string), typeof(string), typeof(bool));

			// Connect treeview with Model and configure
			dashboardseditortreeview.Model = templates;
			dashboardseditortreeview.HeadersVisible = false;
			var cell = new CellRendererText { SizePoints = 14.0 };
			//cell.Editable = true;
			cell.Edited += HandleEdited;
			var col = dashboardseditortreeview.AppendColumn ("Text", cell, "text", 1); 
			col.AddAttribute (cell, "editable", 3);
			dashboardseditortreeview.SearchColumn = 0;
			dashboardseditortreeview.EnableGridLines = TreeViewGridLines.None;
			dashboardseditortreeview.CursorChanged += HandleSelectionChanged;
			
			templatesvbox.WidthRequest = 160;
			
			buttonswidget.Sensitive = false;
			buttonswidget.ButtonsVisible = false;
			buttonswidget.Mode = DashboardMode.Edit;
			newtemplatebutton.Visible = true;
			deletetemplatebutton.Visible = false;
			
			newtemplatebutton.Clicked += HandleNewTemplateClicked;
			importtemplatebutton.Clicked += HandleImportTemplateClicked;
			deletetemplatebutton.Clicked += HandleDeleteTemplateClicked;
			savetemplatebutton.Clicked += (sender, e) => Save (false);
			
			editdashboardslabel.ModifyFont (FontDescription.FromString (Config.Style.Font + " 9"));
			editbuttonslabel.ModifyFont (FontDescription.FromString (Config.Style.Font + " 9"));

			Load (null);
		}

		protected override void OnDestroyed ()
		{
			buttonswidget.Destroy ();
			base.OnDestroyed ();
		}

		void Load (string templateName)
		{
			TreeIter templateIter = TreeIter.Zero;
			bool first = true;

			templatesNames = new List<string> ();
			templates.Clear ();
			foreach (Dashboard template in provider.Templates) {
				Pixbuf img;
				TreeIter iter;
				string name;
				
				if (template.Image != null)
					img = template.Image.Value;
				else
					img = Helpers.Misc.LoadIcon ("longomatch", 20);
				
				name = template.Name;
				if (template.Static) {
					name += " (" + Catalog.GetString ("System") + ")";
				} else {
					templatesNames.Add (name);
				}
				iter = templates.AppendValues (img, name, template.Name, !template.Static);
				if (first || template.Name == templateName) {
					templateIter = iter;
				}
				first = false;
			}
			if (templates.IterIsValid (templateIter)) {
				dashboardseditortreeview.Selection.SelectIter (templateIter);
				HandleSelectionChanged (null, null);
			}
		}

		bool SaveTemplate (Dashboard dashboard)
		{
			try {
				provider.Update (dashboard);
				return true;
			} catch (InvalidTemplateFilenameException ex) {
				Config.GUIToolkit.ErrorMessage (ex.Message, this);
				return false;
			}
		}

		void SaveStatic ()
		{
			string msg = Catalog.GetString ("System dashboards can't be edited, do you want to create a copy?");
			if (Config.GUIToolkit.QuestionMessage (msg, null, this)) {
				string newName;
				while (true) {
					newName = Config.GUIToolkit.QueryMessage (Catalog.GetString ("Name:"), null,
						loadedTemplate.Name + "_copy", this);
					if (newName == null)
						break;
					if (templatesNames.Contains (newName)) {
						msg = Catalog.GetString ("A dashboard with the same name already exists"); 
						Config.GUIToolkit.ErrorMessage (msg, this);
					} else {
						break;
					}
				}
				if (newName == null) {
					return;
				}
				Dashboard newtemplate = loadedTemplate.Clone ();
				newtemplate.Name = newName;
				newtemplate.Static = false;
				if (SaveTemplate (newtemplate)) {
					Load (newtemplate.Name);
				}
			}
		}

		void Save (bool prompt)
		{
			if (loadedTemplate != null && buttonswidget.Edited) {
				if (loadedTemplate.Static) {
					/* prompt=false when we click the save button */
					if (!prompt) {
						SaveStatic ();
					}
				} else {
					string msg = Catalog.GetString ("Do you want to save the current dashboard");
					if (!prompt || Config.GUIToolkit.QuestionMessage (msg, null, this)) {
						if (SaveTemplate (loadedTemplate)) {
							buttonswidget.Edited = false;
						}
					}
				}
			}
		}

		void HandleEnterTemplateButton (object sender, EventArgs e)
		{
			if (sender == newtemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("New dashboard");
			} else if (sender == importtemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("Import dashboard");
			} else if (sender == deletetemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("Delete dashboard");
			} else if (sender == savetemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("Save dashboard");
			}
		}

		void HandleLeftTemplateButton (object sender, EventArgs e)
		{
			editdashboardslabel.Markup = Catalog.GetString ("Manage dashboards");
		}

		void HandleEnterTagButton (object sender, EventArgs e)
		{
			if (sender == addcategorybutton) {
				editbuttonslabel.Markup = Catalog.GetString ("Add category button");
			} else if (sender == addtagbutton1) {
				editbuttonslabel.Markup = Catalog.GetString ("Add tag button");
			} else if (sender == scorebutton) {
				editbuttonslabel.Markup = Catalog.GetString ("Add score button");
			} else if (sender == timerbutton) {
				editbuttonslabel.Markup = Catalog.GetString ("Add timer button");
			} else if (sender == cardbutton) {
				editbuttonslabel.Markup = Catalog.GetString ("Add card button");
			}
		}

		void HandleLeftTagButton (object sender, EventArgs e)
		{
			editbuttonslabel.Markup = Catalog.GetString ("Manage dashboard buttons");
		}

		void HandleSelectionChanged (object sender, EventArgs e)
		{
			Dashboard selected;
			
			dashboardseditortreeview.Selection.GetSelected (out selectedIter);

			try {
				// Load using the template real name and not the display name
				selected = provider.Load (templates.GetValue (selectedIter, 2) as string);
			} catch (Exception ex) {
				Config.GUIToolkit.ErrorMessage (Catalog.GetString ("Could not load dashboard"));
				return;
			}
			deletetemplatebutton.Visible = selected != null;
			buttonswidget.Sensitive = selected != null;
			if (selected != null) {
				Save (true);
				deletetemplatebutton.Sensitive = !selected.Static;
				buttonswidget.Template = selected;
			}
			loadedTemplate = selected;
		}

		void HandleDeleteTemplateClicked (object sender, EventArgs e)
		{
			if (loadedTemplate != null) {
				string msg = Catalog.GetString ("Do you really want to delete the dashboard: ") + loadedTemplate.Name;
				if (MessagesHelpers.QuestionMessage (this, msg, null)) {
					provider.Delete (loadedTemplate.Name);
					templates.Remove (ref selectedIter);
					templatesNames.Remove (loadedTemplate.Name);
					selectedIter = TreeIter.Zero;
					dashboardseditortreeview.Selection.SelectPath (new TreePath ("0"));
					HandleSelectionChanged (null, null);
				}
			}
		}

		void HandleImportTemplateClicked (object sender, EventArgs e)
		{
			string fileName, filterName;
			string[] extensions;

			Log.Debug ("Importing dashboard");
			filterName = Catalog.GetString ("Dashboard files");
			extensions = new [] { "*" + Constants.CAT_TEMPLATE_EXT };
			/* Show a file chooser dialog to select the file to import */
			fileName = Config.GUIToolkit.OpenFile (Catalog.GetString ("Import dashboard"), null, Config.HomeDir,
				filterName, extensions);

			if (fileName == null)
				return;

			try {
				Dashboard new_dashboard = provider.LoadFile (fileName);

				if (new_dashboard != null) {
					bool abort = false;

					while (provider.Exists (new_dashboard.Name) && !abort) {
						string name = Config.GUIToolkit.QueryMessage (Catalog.GetString ("Dashboard name:"),
							              Catalog.GetString ("Name conflict"), new_dashboard.Name + "#");
						if (name == null) {
							abort = true;
						} else {
							new_dashboard.Name = name;
						}
					}

					if (!abort) {
						Pixbuf img;

						provider.Save (new_dashboard);
						if (new_dashboard.Image != null)
							img = new_dashboard.Image.Value;
						else
							img = Helpers.Misc.LoadIcon ("longomatch", 20);

						string name = new_dashboard.Name;
						templates.AppendValues (img, name, name, !new_dashboard.Static);
						Load (new_dashboard.Name);
					}
				}
			} catch (Exception ex) {
				Config.GUIToolkit.ErrorMessage (Catalog.GetString ("Error importing template:") +
				"\n" + ex.Message);
				Log.Exception (ex);
				return;
			}
		}

		void HandleNewTemplateClicked (object sender, EventArgs e)
		{
			bool create = false;
			bool force = false;
			
			EntryDialog dialog = new EntryDialog ();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;
			dialog.ShowCount = true;
			dialog.Text = Catalog.GetString ("New dasboard");
			dialog.CountText = Catalog.GetString ("Event types:");
			dialog.AvailableTemplates = templatesNames;
			
			while (dialog.Run () == (int)ResponseType.Ok) {
				if (dialog.Text == "") {
					MessagesHelpers.ErrorMessage (dialog, Catalog.GetString ("The dashboard name is empty."));
					continue;
				} else if (provider.Exists (dialog.Text)) {
					var msg = Catalog.GetString ("The dashboard already exists. " +
					          "Do you want to overwrite it?");
					if (MessagesHelpers.QuestionMessage (this, msg)) {
						create = true;
						force = true;
						break;
					}
				} else {
					create = true;
					break;
				}
			}
			
			if (create) {
				if (force) {
					try {
						provider.Delete (dialog.Text);
					} catch {
					}
				}
				if (dialog.SelectedTemplate != null) {
					try {
						provider.Copy (dialog.SelectedTemplate, dialog.Text);
					} catch (InvalidTemplateFilenameException ex) {
						Config.GUIToolkit.ErrorMessage (ex.Message, this);
						dialog.Destroy ();
						return;
					}
				} else {
					Dashboard template;
					template = Dashboard.DefaultTemplate (dialog.Count);
					template.Name = dialog.Text;
					if (!SaveTemplate (template)) {
						dialog.Destroy ();
						return;
					}
				}
				Load (dialog.Text);
			}
			dialog.Destroy ();
		}

		void HandleEdited (object o, EditedArgs args)
		{
			Gtk.TreeIter iter;
			templates.GetIter (out iter, new Gtk.TreePath (args.Path));
 
			string name = (string)templates.GetValue (iter, 2);
			if (name != args.NewText) {
				if (templatesNames.Contains (args.NewText)) {
					Config.GUIToolkit.ErrorMessage (Catalog.GetString ("A dashboard with the same name already exists"), this);
					args.RetVal = false;
				} else {
					try {
						Dashboard dashboard = provider.Load (name);
						dashboard.Name = args.NewText;
						provider.Save (dashboard);
						provider.Delete (name);
						templatesNames.Remove (name);
						templatesNames.Add (dashboard.Name);
						// Change displayed name and template name in our store
						templates.SetValue (iter, 1, args.NewText);
						templates.SetValue (iter, 2, args.NewText);
					} catch (Exception ex) {
						Config.GUIToolkit.ErrorMessage (ex.Message);
					}
				}
			}
		}
	}
}


