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
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core;
using LongoMatch.Core.Common;

using LongoMatch.Core.Stats;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace LongoMatch.Plugins.Stats
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class Plotter : Gtk.Bin
	{
		const double WIDTH = 700;
		const double HEIGHT = 300;
		const double NO_TEAMS_WIDTH = 500;
		GraphType graphType;
		SubCategoryStat stats;
		bool showTeams;
		double graphWidth;

		public Plotter ()
		{
			this.Build ();
			HeightRequest = (int)HEIGHT + 20;
			WidthRequest = (int)WIDTH;
			pieradiobutton.Toggled += HandleToggled;
			historadiobutton.Toggled += HandleToggled;
			HomeName = Catalog.GetString ("Home");
			AwayName = Catalog.GetString ("Away");
			ShowTeams = true;
		}

		public bool ShowTeams {
			protected get {
				return showTeams;
			}
			set {
				showTeams = value;
				graphWidth = value ? WIDTH : NO_TEAMS_WIDTH;
				WidthRequest = (int)graphWidth;
			}
		}

		public string HomeName {
			get;
			set;
		}

		public string AwayName {
			get;
			set;
		}

		public void LoadPie (SubCategoryStat stats)
		{
			graphType = GraphType.Pie;
			this.stats = stats;
			Reload ();
		}

		public void LoadHistogram (SubCategoryStat stats)
		{
			graphType = GraphType.Histogram;
			this.stats = stats;
			Reload ();
		}

		Pixbuf Load (PlotModel model, double width, double height)
		{
			MemoryStream stream = new MemoryStream ();
			SvgExporter.Export (model, stream, width, height, false);
			stream.Seek (0, SeekOrigin.Begin);
			return new Pixbuf (stream);
		}

		PlotModel GetHistogram (SubCategoryStat stats)
		{
			PlotModel model = new PlotModel ();
			CategoryAxis categoryAxis;
			LinearAxis valueAxis;
			int maxCount;

			valueAxis = new LinearAxis {
				Position = AxisPosition.Left, MinimumPadding = 0, AbsoluteMinimum = 0,
				MinorStep = 1, MajorStep = 1, Minimum = 0
			};
			categoryAxis = new CategoryAxis () {
				ItemsSource = stats.OptionStats, LabelField = "Name",
				Angle = 20.0
			};

			model.Series.Add (new ColumnSeries {
				Title = Catalog.GetString ("Total"), ItemsSource = stats.OptionStats,
				ValueField = "TotalCount"
			});
			if (ShowTeams) {
				model.Series.Add (new ColumnSeries {
					Title = HomeName, ItemsSource = stats.OptionStats,
					ValueField = "LocalTeamCount", FillColor = OxyColor.FromArgb (0xFF, 0xFF, 0x33, 0x0),
				});
				model.Series.Add (new ColumnSeries {
					Title = AwayName, ItemsSource = stats.OptionStats,
					ValueField = "VisitorTeamCount", FillColor = OxyColor.FromArgb (0xFF, 0, 0x99, 0xFF)
				});
			}
			model.Axes.Add (categoryAxis);
			model.Axes.Add (valueAxis);

			if (stats.OptionStats.Count != 0) {
				maxCount = stats.OptionStats.Max (o => o.TotalCount);
				if (maxCount > 10 && maxCount <= 50) {
					valueAxis.MinorStep = 5;
					valueAxis.MajorStep = 10;
				} else if (maxCount > 50 && maxCount <= 100) {
					valueAxis.MinorStep = 10;
					valueAxis.MajorStep = 20;
				} else if (maxCount > 100) {
					valueAxis.MinorStep = 10;
					valueAxis.MajorStep = 50;
				}
			}
			OxyColor text_color = OxyColor.FromArgb (LongoMatch.App.Current.Style.PaletteText.A,
									  LongoMatch.App.Current.Style.PaletteText.R,
									  LongoMatch.App.Current.Style.PaletteText.G,
									  LongoMatch.App.Current.Style.PaletteText.B);
			model.TextColor = text_color;
			model.TitleColor = text_color;
			model.SubtitleColor = text_color;

			return model;
		}

		PlotModel GetPie (SubCategoryStat stats, TeamType team)
		{
			PlotModel model = new PlotModel ();
			PieSeries ps = new PieSeries ();

			foreach (PercentualStat st in stats.OptionStats) {
				double count = GetCount (st, team);
				if (count == 0)
					continue;
				ps.Slices.Add (new PieSlice (st.Name, count));
			}
			ps.InnerDiameter = 0;
			ps.ExplodedDistance = 0.0;
			ps.Stroke = OxyColors.White;
			ps.StrokeThickness = 2.0;
			ps.InsideLabelPosition = 0.8;
			ps.AngleSpan = 360;
			ps.StartAngle = 0;
			if (team == TeamType.LOCAL) {
				ps.Title = HomeName;
			} else if (team == TeamType.VISITOR) {
				ps.Title = AwayName;
			}
			OxyColor text_color = OxyColor.FromArgb (LongoMatch.App.Current.Style.PaletteText.A,
									  LongoMatch.App.Current.Style.PaletteText.R,
									  LongoMatch.App.Current.Style.PaletteText.G,
									  LongoMatch.App.Current.Style.PaletteText.B);
			model.TextColor = text_color;
			model.TitleColor = text_color;
			model.SubtitleColor = text_color;
			model.Series.Add (ps);
			return model;
		}

		double GetCount (PercentualStat stats, TeamType team)
		{
			switch (team) {
			case TeamType.NONE:
			case TeamType.BOTH:
				return stats.TotalCount;
			case TeamType.LOCAL:
				return stats.LocalTeamCount;
			case TeamType.VISITOR:
				return stats.VisitorTeamCount;
			}
			return 0;
		}

		void Reload ()
		{
			if (stats == null)
				return;

			switch (graphType) {
			case GraphType.Histogram:
				imageall.Pixbuf = Load (GetHistogram (stats), graphWidth, HEIGHT);
				imagehome.Visible = false;
				imageaway.Visible = false;
				break;
			case GraphType.Pie:
				if (ShowTeams) {
					imageall.Pixbuf = Load (GetPie (stats, TeamType.BOTH), graphWidth / 3, HEIGHT);
					imagehome.Pixbuf = Load (GetPie (stats, TeamType.LOCAL), graphWidth / 3, HEIGHT);
					imageaway.Pixbuf = Load (GetPie (stats, TeamType.VISITOR), graphWidth / 3, HEIGHT);
				} else {
					imageall.Pixbuf = Load (GetPie (stats, TeamType.BOTH), graphWidth, HEIGHT);
				}
				imagehome.Visible = ShowTeams;
				imageaway.Visible = ShowTeams;
				break;
			}
		}

		void HandleToggled (object sender, EventArgs args)
		{
			RadioButton r = sender as RadioButton;

			if (r == pieradiobutton && r.Active) {
				graphType = GraphType.Pie;
				Reload ();
			} else if (r == historadiobutton && r.Active) {
				graphType = GraphType.Histogram;
				Reload ();
			}
		}

		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			if (imageall != null)
				imageall.Destroy ();
			if (imageaway != null)
				imageaway.Destroy ();
			if (imagehome != null)
				imagehome.Destroy ();
		}

		protected enum GraphType
		{
			Histogram,
			Pie,
		}
	}

}

