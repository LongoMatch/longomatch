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
using System.IO;
using Gdk;
using Cairo;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

using LongoMatch.Stats;
using LongoMatch.Common;
using Mono.Unix;
using Gtk;

namespace LongoMatch.Gui.Component.Stats
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class Plotter : Gtk.Bin
	{
		const double WIDTH = 800;
		const double HEIGHT = 300;
		GraphType graphType;
		SubCategoryStat stats;
		
		public Plotter ()
		{
			this.Build ();
			HeightRequest = (int) HEIGHT;
			WidthRequest = (int) WIDTH;
			pieradiobutton.Toggled += HandleToggled;
			historadiobutton.Toggled += HandleToggled;
		}

		public void LoadPie (SubCategoryStat stats) {
			graphType = GraphType.Pie;
			this.stats = stats;
			Reload ();
		}
		
		public void LoadHistogram (SubCategoryStat stats) {
			graphType = GraphType.Histogram;
			this.stats = stats;
			Reload ();
		}
		
		Pixbuf Load (PlotModel model, double width, double height) {
			MemoryStream stream = new MemoryStream();
            SvgExporter.Export (model, stream, width, height, false, new PangoTextMeasurer());
            stream.Seek (0, SeekOrigin.Begin);
            return new Pixbuf (stream);
		}
		
		PlotModel GetHistogram (SubCategoryStat stats) {
			PlotModel model = new PlotModel ();
            CategoryAxis categoryAxis;
            LinearAxis valueAxis;
            
			valueAxis = new LinearAxis(AxisPosition.Left) { MinimumPadding = 0, AbsoluteMinimum = 0,
				MinorStep = 1, MajorStep = 1, Minimum = 0};
            categoryAxis = new CategoryAxis () {ItemsSource = stats.OptionStats, LabelField = "Name",
				Angle = 20.0};
            
			model.Series.Add(new ColumnSeries { Title = Catalog.GetString ("Total"), ItemsSource = stats.OptionStats,
				ValueField = "TotalCount" });	
			model.Series.Add(new ColumnSeries { Title = Catalog.GetString ("Home"), ItemsSource = stats.OptionStats,
				ValueField = "LocalTeamCount" });	
			model.Series.Add(new ColumnSeries { Title = Catalog.GetString ("Away"), ItemsSource = stats.OptionStats,
				ValueField = "VisitorTeamCount" });	
            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);
            return model;
		}
		
		PlotModel GetPie (SubCategoryStat stats, Team team) {
			PlotModel model = new PlotModel ();
			PieSeries ps = new PieSeries();
			
			foreach (PercentualStat st in stats.OptionStats) {
				double count = GetCount (st, team);
				if (count == 0)
					continue;
				ps.Slices.Add(new PieSlice(st.Name, count));
			}
			ps.InnerDiameter = 0;
			ps.ExplodedDistance = 0.0;
			ps.Stroke = OxyColors.White;
			ps.StrokeThickness = 2.0;
			ps.InsideLabelPosition = 0.8;
			ps.AngleSpan = 360;
			ps.StartAngle = 0;
            model.Series.Add(ps);
            return model;
		}
		
		double GetCount (PercentualStat stats, Team team) {
			switch (team) {
			case Team.NONE:
			case Team.BOTH:
				return stats.TotalCount;
			case Team.LOCAL:
				return stats.LocalTeamCount;
			case Team.VISITOR:
				return stats.VisitorTeamCount;
			}
			return 0;
		}
		
		void Reload () {
			if (stats == null)
				return;
			
			switch (graphType) {
			case GraphType.Histogram:
				imageall.Pixbuf = Load (GetHistogram (stats), WIDTH, HEIGHT);
				imagehome.Visible = false;
				imageaway.Visible = false;
				break;
			case GraphType.Pie:
				imageall.Pixbuf = Load (GetPie (stats, Team.BOTH), WIDTH / 3, HEIGHT);
				imagehome.Pixbuf = Load (GetPie (stats, Team.LOCAL), WIDTH / 3, HEIGHT);
				imageaway.Pixbuf = Load (GetPie (stats, Team.VISITOR), WIDTH / 3, HEIGHT);
				imagehome.Visible = true;
				imageaway.Visible = true;
				break;
			}
		}
		
		void HandleToggled (object sender, EventArgs args) {
			RadioButton r = sender as RadioButton;
			
			if (r == pieradiobutton && r.Active) {
				graphType = GraphType.Pie;
				Reload ();
			} else if (r == historadiobutton && r.Active) {
				graphType = GraphType.Histogram;
				Reload ();
			}
		}
		
		protected enum GraphType {
			Histogram,
			Pie,
		}
	}
	
}

