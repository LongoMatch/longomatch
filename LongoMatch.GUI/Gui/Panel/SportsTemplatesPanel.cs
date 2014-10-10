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

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SportsTemplatesPanel : Gtk.Bin, IPanel
	{
		public event BackEventHandle BackEvent;

		ListStore templates;
		Dashboard loadedTemplate;
		ICategoriesTemplatesProvider provider;

		public SportsTemplatesPanel ()
		{
			this.Build ();
			provider = Config.CategoriesTemplatesProvider;

			// Assign images
			panelheader1.ApplyVisible = false;
			panelheader1.Title = "ANALYSIS DASHBOARDS MANAGER";
			panelheader1.BackClicked += (sender, o) => {
				if (BackEvent != null)
					BackEvent ();
			};

			templateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-template-header", 54, IconLookupFlags.ForceSvg);
			categoryheaderimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-category-header", 47, IconLookupFlags.ForceSvg);
			newtemplateimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-template-add", 36, IconLookupFlags.ForceSvg);
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
			deletetemplatebutton.Entered += HandleEnterTemplateButton;
			deletetemplatebutton.Left += HandleLeftTemplateButton;
			savetemplatebutton.Entered += HandleEnterTemplateButton;
			savetemplatebutton.Left += HandleLeftTemplateButton;

			addcategorybutton.Entered += HandleEnterTagButton;
			addcategorybutton.Left += HandleLeftTagButton;
			addcategorybutton.Clicked += (object sender, EventArgs e) => {
				buttonswidget.AddButton ("Category"); };
			addtagbutton1.Entered += HandleEnterTagButton;
			addtagbutton1.Left += HandleLeftTagButton;
			addtagbutton1.Clicked += (object sender, EventArgs e) => {
				buttonswidget.AddButton ("Tag"); };
			scorebutton.Entered += HandleEnterTagButton;
			scorebutton.Left += HandleLeftTagButton;
			scorebutton.Clicked += (object sender, EventArgs e) => {
				buttonswidget.AddButton ("Score"); };
			cardbutton.Entered += HandleEnterTagButton;
			cardbutton.Left += HandleLeftTagButton;
			cardbutton.Clicked += (object sender, EventArgs e) => {
				buttonswidget.AddButton ("Card"); };
			timerbutton.Entered += HandleEnterTagButton;
			timerbutton.Left += HandleLeftTagButton;
			timerbutton.Clicked += (object sender, EventArgs e) => {
				buttonswidget.AddButton ("Timer"); };

			templates = new ListStore (typeof(Pixbuf), typeof(string), typeof(Dashboard));

			// Connect treeview with Model and configure
			dashboardseditortreeview.Model = templates;
			dashboardseditortreeview.HeadersVisible = false;
			var cell = new CellRendererText { SizePoints = 14.0 };
			cell.Editable = true;
			cell.Edited += HandleEdited;
			dashboardseditortreeview.AppendColumn ("Text", cell, "text", 1); 
			dashboardseditortreeview.SearchColumn = 0;
			dashboardseditortreeview.EnableGridLines = TreeViewGridLines.None;
			dashboardseditortreeview.CursorChanged += HandleSelectionChanged;
			
			templatesvbox.WidthRequest = 160;
			
			buttonswidget.Sensitive = false;
			buttonswidget.ButtonsVisible = false;
			buttonswidget.Mode = TagMode.Edit;
			newtemplatebutton.Visible = true;
			deletetemplatebutton.Visible = false;
			
			newtemplatebutton.Clicked += HandleNewTeamClicked;
			deletetemplatebutton.Clicked += HandleDeleteTeamClicked;
			savetemplatebutton.Clicked += (sender, e) => Save (false);
			
			editdashboardslabel.ModifyFont (FontDescription.FromString (Config.Style.Font + " 8"));
			editbuttonslabel.ModifyFont (FontDescription.FromString (Config.Style.Font + " 8"));

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
			
			templates.Clear ();
			foreach (Dashboard template in provider.Templates) {
				Pixbuf img;
				TreeIter iter;
				
				if (template.Image != null)
					img = template.Image.Value;
				else
					img = Helpers.Misc.LoadIcon ("longomatch", 20, IconLookupFlags.ForceSvg);
					
				iter = templates.AppendValues (img, template.Name, template);
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

		void Save (bool prompt)
		{
			if (loadedTemplate != null && buttonswidget.Edited) {
				string msg = Catalog.GetString ("Do you want to save the current template");
				if (!prompt || Config.GUIToolkit.QuestionMessage (msg, null, this)) {
					provider.Update (loadedTemplate);
					buttonswidget.Edited = false;
				}
			}
		}

		void HandleEnterTemplateButton (object sender, EventArgs e)
		{
			if (sender == newtemplatebutton) {
				editdashboardslabel.Markup = Catalog.GetString ("New dashboard");
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
			TreeIter iter;
			Dashboard selected;
			
			dashboardseditortreeview.Selection.GetSelected (out iter);
			selected = templates.GetValue (iter, 2) as Dashboard;
			deletetemplatebutton.Visible = selected != null;
			buttonswidget.Sensitive = selected != null;
			loadedTemplate = selected;
			if (selected != null) {
				Save (true);
				buttonswidget.Template = selected;
			}
		}

		void HandleDeleteTeamClicked (object sender, EventArgs e)
		{
			if (loadedTemplate != null) {
				if (loadedTemplate.Name == "default") {
					MessagesHelpers.ErrorMessage (this, Catalog.GetString ("The default template can't be deleted"));
				} else {
					string msg = Catalog.GetString ("Do you really want to delete the template: ") + loadedTemplate.Name;
					if (MessagesHelpers.QuestionMessage (this, msg, null)) {
						provider.Delete (loadedTemplate.Name);
					}
				}
			}
			Load ("default");
		}

		void HandleNewTeamClicked (object sender, EventArgs e)
		{
			bool create = false;
			bool force = false;
			
			EntryDialog dialog = new EntryDialog ();
			dialog.TransientFor = (Gtk.Window)this.Toplevel;
			dialog.ShowCount = true;
			dialog.Text = Catalog.GetString ("New dasboard");
			dialog.CountText = Catalog.GetString ("Event types:");
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
					provider.Copy (dialog.SelectedTemplate, dialog.Text);
				} else {
					Dashboard template;
					template = Dashboard.DefaultTemplate (dialog.Count);
					template.Name = dialog.Text;
					provider.Save (template);
				}
				Load (dialog.Text);
			}
			dialog.Destroy ();
		}

		void HandleEdited (object o, EditedArgs args)
		{
			Gtk.TreeIter iter;
			templates.GetIter (out iter, new Gtk.TreePath (args.Path));
 
			Dashboard template = (Dashboard)templates.GetValue (iter, 2);
			if (template.Name != args.NewText) {
				if (provider.TemplatesNames.Contains (args.NewText)) {
					Config.GUIToolkit.ErrorMessage (Catalog.GetString ("A template with the same name already exists"), this);
					args.RetVal = false;
				} else {
					provider.Delete (template.Name);
					template.Name = args.NewText;
					provider.Save (template);
					templates.SetValue (iter, 1, template.Name);
				}
			}
		}
	}
}


