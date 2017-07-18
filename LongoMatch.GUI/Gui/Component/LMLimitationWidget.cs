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
	public partial class LMLimitationWidget : Gtk.Bin, IView<CountLimitationVM>
	{
		const int UPGRADE_BUTTON_WIDTH = 95;
		const int UPGRADE_BUTTON_HEIGHT = 50;

		CountLimitationVM viewModel;
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
		public CountLimitationVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChangedEventHandler;
				}
				viewModel = value;
				Visible = viewModel != null && viewModel.Enabled;
				ctx?.UpdateViewModel (viewModel);
				if (viewModel != null) {
					viewModel.PropertyChanged += HandlePropertyChangedEventHandler;
					viewModel.Sync ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (CountLimitationVM)viewModel;
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (upgradeButton.Bind (vm => ((CountLimitationVM)vm).UpgradeCommand));
		}

		void HandlePropertyChangedEventHandler (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.Count))) {
				SetBarViewModel ();
			}
		}

		void SetBarViewModel ()
		{
			SeriesVM currentSeries = new SeriesVM ("Current", ViewModel.Count, Color.Transparent);
			if (ViewModel.Remaining == 0) {
				currentSeries.Color = Color.Red;
				countLabel.Markup = $"Oops! <b>No {ViewModel.RegisterName.ToLower ()}</b> left in your plan!";
			} else {
				countLabel.Markup = $"Only <b>{ViewModel.Remaining} {ViewModel.RegisterName.ToLower ()}</b> left in your plan!";

			}

			barView.SetViewModel (new BarChartVM {
				Height = 10,
				Series = new SeriesCollectionVM {
					ViewModels = {
						new SeriesVM("Remaining", ViewModel.Remaining, Color.Green1),
						currentSeries
					}
				},
				Background = new ImageCanvasObject {
					Image = App.Current.ResourcesLocator.LoadImage ("images/lm-widget-full-bar" + Constants.IMAGE_EXT),
					Mode = ScaleMode.Fill
				}
			});
		}
	}
}
