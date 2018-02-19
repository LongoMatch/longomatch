//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using LongoMatch.Core.Stats;
using LongoMatch.Core.Store;
using VAS.Core.Resources;
using VAS.Core.Store;
using Color = Cairo.Color;
using Helpers = VAS.UI.Helpers;

namespace LongoMatch.Plugins.Stats
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class GameViewer : Gtk.Bin
	{
		ProjectStats stats;
		LMProject project;
		int catsMaxSize, subcatsMaxSize;
		List<Widget> subcats, cats;

		public GameViewer ()
		{
			this.Build ();
		}

		public void LoadProject (Project project, ProjectStats stats)
		{
			this.stats = stats;
			this.project = project as LMProject;
			UpdateGui ();				
		}

		void UpdateSubcatsVisibility ()
		{
			bool visible = subcatscheckbutton.Active;
			
			if (subcats == null)
				return;
			foreach (Widget w in subcats) {
				w.Visible = visible;
			}
			foreach (StatsWidget w in cats) {
				if (visible) {
					w.MaxTextSize = subcatsMaxSize;
				} else {
					w.MaxTextSize = catsMaxSize;
				}
				w.QueueDraw ();
			}
		}

		void UpdateGui ()
		{
			homelabel.Markup = String.Format ("{0} <span font_desc=\"40\">{1}</span>",
				project.LocalTeamTemplate.TeamName,
				project.GetScore (project.LocalTeamTemplate));
			awaylabel.Markup = String.Format ("<span font_desc=\"40\">{0}</span> {1}",
				project.GetScore (project.VisitorTeamTemplate),
				project.VisitorTeamTemplate.TeamName);
			GetMaxSize (out catsMaxSize, out subcatsMaxSize);
			if (project.LocalTeamTemplate.Shield != null) {
				homeimage.Pixbuf = project.LocalTeamTemplate.Shield.Value;
			} else {
				homeimage.Pixbuf = App.Current.ResourcesLocator.LoadIcon (Icons.DefaultShield).Value;
			}
			if (project.VisitorTeamTemplate.Shield != null) {
				awayimage.Pixbuf = project.VisitorTeamTemplate.Shield.Value;
			} else {
				homeimage.Pixbuf = App.Current.ResourcesLocator.LoadIcon (Icons.DefaultShield).Value;
			}
			
			subcats = new List<Widget> ();
			cats = new List<Widget> ();
			foreach (EventTypeStats cstats in stats.EventTypeStats) {
				AddCategory (cstats);
			}			
			mainbox.ShowAll ();
			UpdateSubcatsVisibility ();
		}

		void AddCategory (EventTypeStats cstats)
		{
			Widget w = new StatsWidget (cstats, null, null, catsMaxSize);
			cats.Add (w);
			cstatsbox.PackStart (w, false, true, 0);
			              
			foreach (SubCategoryStat stats in cstats.SubcategoriesStats) {
				AddSubcategory (stats, cstats);
			}
			cstatsbox.PackStart (new HSeparator (), false, false, 0);
		}

		void AddSubcategory (SubCategoryStat sstats, EventTypeStats parent)
		{
			foreach (PercentualStat ostats in sstats.OptionStats) {
				StatsWidget w = new StatsWidget (ostats, parent, sstats, subcatsMaxSize);
				subcats.Add (w);
				cstatsbox.PackStart (w, false, true, 0);
			}
		}

		void GetMaxSize (out int normal, out int full)
		{
			Pango.Layout layout = new Pango.Layout (Gdk.PangoHelper.ContextGet ());
			
			normal = full = 0;
			
			foreach (EventTypeStats cstat in stats.EventTypeStats) {
				int width, height;
				layout.SetMarkup (String.Format ("<b>{0}</b>", GLib.Markup.EscapeText (cstat.Name)));
				layout.GetPixelSize (out width, out height);
				if (width > normal) {
					normal = width;
				}
				foreach (SubCategoryStat sstat in cstat.SubcategoriesStats) {
					foreach (PercentualStat spstat in sstat.OptionStats) {
						layout.SetMarkup (GLib.Markup.EscapeText (String.Format ("{0}: {1}", sstat.Name, spstat.Name)));
						layout.GetPixelSize (out width, out height);
						if (width > full) {
							full = width;
						}
					}
				}
			}
			if (full < normal) {
				full = normal;
			}
#if !OSTYPE_LINUX
			normal = (int)(normal * 1.3);
			full = (int)(full * 1.3);
#endif
		}

		protected void OnSubcatscheckbuttonClicked (object sender, EventArgs e)
		{
			UpdateSubcatsVisibility ();
		}
	}

	class StatsWidget: DrawingArea
	{
		Stat stat, category;
		Pango.Layout layout;
		const double WIDTH_PERCENT = 0.8;
		const int COUNT_WIDTH = 120;
		int textSize;
		string name_tpl, count_tpl;

		public StatsWidget (Stat stat, Stat category, SubCategoryStat subcat, int textSize)
		{
			/* For subcategories, parent is the parent Category */
			this.stat = stat;
			this.category = category;
			HomeColor = CairoUtils.ColorFromRGB (0xFF, 0x33, 0);
			AwayColor = CairoUtils.ColorFromRGB (0, 0x99, 0xFF);
			layout = new Pango.Layout (PangoContext);
			layout.Wrap = Pango.WrapMode.Char;
			layout.Alignment = Pango.Alignment.Center;
			ModifyText (StateType.Normal, Helpers.Misc.ToGdkColor (App.Current.Style.TextBase));
			this.textSize = textSize;
			name_tpl = "{0}";
			count_tpl = "{0} ({1}%)";
			if (category == null) {
				name_tpl = "<b>" + name_tpl + "</b>";
				count_tpl = "<b>" + count_tpl + "</b>";
				HeightRequest = 25;
			} else {
				if (subcat != null) {
					name_tpl = GLib.Markup.EscapeText (subcat.Name);
					name_tpl += name_tpl == "" ? "{0}" : ": {0}";
				}
				HeightRequest = 18;
			}
		}

		Cairo.Color HomeColor {
			get;
			set;
		}

		Cairo.Color AwayColor {
			get;
			set;
		}

		public int MaxTextSize {
			set {
				textSize = value;
			}
		}

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			int width, height, center, lCenter, vCenter, totalCount;
			double localPercent, visitorPercent;
			
			this.GdkWindow.Clear ();
			
			width = Allocation.Width;
			center = width / 2;
			lCenter = center - textSize / 2;
			vCenter = center + textSize / 2;
			width = width - textSize - 10;
			
			height = Allocation.Height;
			
			if (category != null) {
				totalCount = category.TotalCount;
			} else {
				totalCount = stat.TotalCount;
			}
			if (totalCount != 0) {
				localPercent = (double)stat.LocalTeamCount / totalCount;
				visitorPercent = (double)stat.VisitorTeamCount / totalCount;
			} else {
				localPercent = 0;
				visitorPercent = 0;
			}
			
			using (Cairo.Context g = Gdk.CairoHelper.Create (this.GdkWindow)) {
				int localW, visitorW;
				
				localW = (int)(width / 2 * localPercent);
				visitorW = (int)(width / 2 * visitorPercent); 
				
				/* Home bar */
				CairoUtils.DrawRoundedRectangle (g, lCenter - localW, 0, localW, height, 0,
					HomeColor, HomeColor);
				/* Away bar  */
				CairoUtils.DrawRoundedRectangle (g, vCenter, 0, visitorW, height, 0,
					AwayColor, AwayColor);
				                                 
				/* Category name */
				layout.Width = Pango.Units.FromPixels (textSize);
				layout.Alignment = Pango.Alignment.Center;
				layout.SetMarkup (String.Format (name_tpl, GLib.Markup.EscapeText (stat.Name)));
				GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), center - textSize / 2, 0, layout);
				
				/* Home count */	
				layout.Width = Pango.Units.FromPixels (COUNT_WIDTH);
				layout.Alignment = Pango.Alignment.Right;
				layout.SetMarkup (String.Format (count_tpl, stat.LocalTeamCount, (localPercent * 100).ToString ("f2")));
				GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), lCenter - (COUNT_WIDTH + 3), 0, layout);
				
				/* Away count */	
				layout.Width = Pango.Units.FromPixels (COUNT_WIDTH);
				layout.Alignment = Pango.Alignment.Left;
				layout.SetMarkup (String.Format (count_tpl, stat.VisitorTeamCount, (visitorPercent * 100).ToString ("f2")));
				GdkWindow.DrawLayout (Style.TextGC (StateType.Normal), vCenter + 3, 0, layout);
			}
			
			return true;
		}
	}
}

