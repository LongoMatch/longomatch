//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.ComponentModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Core.ViewModel.Statistics;
using VAS.Drawing;
using VAS.Drawing.Cairo;
using VAS.Drawing.CanvasObjects.Blackboard;
using VAS.Drawing.CanvasObjects.Statistics;
using VAS.UI.Helpers;
using VAS.UI.Helpers.Bindings;
using Constants = VAS.Core.Common.Constants;

namespace LongoMatch.Gui.Component
{
	/// <summary>
	/// LongoMatch widget for count limitations.
	/// It shows a "progress bar" with the number of remaining/current elements limited.
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LMLimitationWidget : Gtk.Bin, IView<CountLimitationBarChartVM>
	{
		const int UPGRADE_BUTTON_WIDTH = 95;
		const int UPGRADE_BUTTON_HEIGHT = 50;

		CountLimitationBarChartVM viewModel;
		BindingContext ctx;
		BarChartView barView;
		Canvas barCanvas;

		public LMLimitationWidget ()
		{
			this.Build ();
			countLabel.UseMarkup = true;
			countLabel.ModifyFont (Pango.FontDescription.FromString (App.Current.Style.Font + " 16px"));

			// FIXME: This color is bg_dark_color from gtkrc, it should be set in the color scheme, styleconf, whatever...
			backgroundBox.ModifyBg (Gtk.StateType.Normal, Misc.ToGdkColor (Color.Parse ("#151a20")));
			barCanvas = new Canvas (new WidgetWrapper (barDrawingArea));
			barView = new BarChartView ();
			barCanvas.AddObject (barView);

			upgradeButton.ApplyStyle (StyleConf.ButtonRegular, UPGRADE_BUTTON_WIDTH, UPGRADE_BUTTON_HEIGHT);

			Bind ();
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected override void OnDestroyed ()
		{
			Log.Verbose ($"Destroying {GetType ()}");
			ctx?.Dispose ();
			ctx = null;
			ViewModel = null;

			base.OnDestroyed ();

			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		/// <summary>
		/// Gets or sets the view model.
		/// </summary>
		/// <value>The view model.</value>
		public CountLimitationBarChartVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChangedEventHandler;
				}
				viewModel = value;
				Visible = viewModel != null && viewModel.Limitation.Enabled;
				ctx?.UpdateViewModel (viewModel);
				barView.SetViewModel (viewModel?.BarChart);
				if (viewModel != null) {
					viewModel.PropertyChanged += HandlePropertyChangedEventHandler;
					viewModel.Sync ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (CountLimitationBarChartVM)viewModel;
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (upgradeButton.Bind (vm => ((CountLimitationBarChartVM)vm).Limitation.UpgradeCommand));
		}

		void HandlePropertyChangedEventHandler (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.Limitation.Count))) {
				SetBarViewModel ();
			}
		}

		void SetBarViewModel ()
		{
			if (ViewModel.Limitation.Remaining == 0) {
				ViewModel.BarChart.RightSerie.Color = Color.Red;
				countLabel.Markup = $"Oops! <b>No {ViewModel.Limitation.RegisterName.ToLower ()}</b> left in your plan!";
			} else {
				ViewModel.BarChart.RightSerie.Color = Color.Transparent;
				countLabel.Markup = $"Only <b>{ViewModel.Limitation.Remaining} {ViewModel.Limitation.RegisterName.ToLower ()}</b> left in your plan!";
			}
		}
	}
}
