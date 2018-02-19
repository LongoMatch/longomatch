//
//  Copyright (C) 2017 Fluendo S.A.
using System.Linq;
using Gdk;
using Gtk;
using LongoMatch.Core.ViewModel;
using VAS.Core;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;
using Constants = LongoMatch.Core.Common.Constants;
using Image = VAS.Core.Common.Image;
using Log = VAS.Core.Common.Log;
using Point = VAS.Core.Common.Point;
using ScaleMode = VAS.Core.Common.ScaleMode;
using Color = VAS.Core.Common.Color;
using VAS.Core.Resources.Styles;

namespace LongoMatch.Gui.Component
{
	/// <summary>
	/// Cell renderer for LongoMatch Projects
	/// </summary>
	public class LMProjectCellRenderer : CellRenderer, IView<LMProjectVM>
	{
		LMProjectVM projectVM;
		Rectangle cellArea;
		Rectangle backgroundArea;
		IDrawingToolkit tk;
		Point pos;
		CellRendererState flags;
		int remainingWidth;

		Image image;
		Image homeShield;
		Image awayShield;
		Color homeMaskColor;
		Color awayMaskColor;
		string description;


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
			ViewModel = null;
			Log.Verbose ($"Destroying {GetType ()}");
			base.OnDestroyed ();

			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		public LMProjectVM ViewModel {
			get {
				return projectVM;
			}
			set {
				projectVM = value;
				if (projectVM != null) {
					Update ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMProjectVM)viewModel;
		}

		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;

			width = Constants.PROJECT_CELL_WIDTH;
			height = Constants.PROJECT_CELL_HEIGHT;
		}

		protected override void Render (Drawable window, Widget widget, Rectangle backgroundArea, Rectangle cellArea, Rectangle exposeArea, CellRendererState flags)
		{
			tk = App.Current.DrawingToolkit;
			this.backgroundArea = backgroundArea;
			this.cellArea = cellArea;
			this.flags = flags;

			using (IContext context = new CairoContext (window)) {
				tk.Context = context;
				tk.Begin ();
				remainingWidth = cellArea.Width;
				pos = new Point (cellArea.X, cellArea.Y);
				DrawTeamShield (homeShield, homeMaskColor);
				DrawTeamShield (awayShield, awayMaskColor);
				DrawDescription ();
				tk.End ();
				tk.Context = null;
			}
		}

		void DrawTeamShield (Image shield, Color maskColor)
		{
			var point = new Point (pos.X, pos.Y + ((cellArea.Height - shield.Height) / 2));
			tk.FillColor = maskColor;
			tk.DrawImage (point, shield.Width, shield.Height, shield, ScaleMode.AspectFit, maskColor != null);
			pos = new Point (pos.X + shield.Width, pos.Y);
			remainingWidth -= shield.Width;
		}

		void DrawDescription ()
		{
			tk.StrokeColor = Color.White;
			tk.DrawText (pos, remainingWidth, cellArea.Height, description, false, true);
		}

		void Update ()
		{
			MediaFileVM file = ViewModel.FileSet.FirstOrDefault ();

			if (ViewModel.HomeTeamShield != null) {
				homeShield = ViewModel.HomeTeamShield.Scale (Constants.SHIELD_IMAGE_SIZE, Constants.SHIELD_IMAGE_SIZE);
				homeMaskColor = null;
			} else {
				homeShield = App.Current.ResourcesLocator.LoadIcon ("vas-default-shield", Constants.SHIELD_IMAGE_SIZE);
				homeMaskColor = Colors.DefaultShield;
			}
			if (ViewModel.AwayTeamShield != null) {
				awayShield = ViewModel.AwayTeamShield.Scale (Constants.SHIELD_IMAGE_SIZE, Constants.SHIELD_IMAGE_SIZE);
				awayMaskColor = null;
			} else {
				awayShield = App.Current.ResourcesLocator.LoadIcon ("vas-default-shield", Constants.SHIELD_IMAGE_SIZE);
				awayMaskColor = Colors.DefaultShield;
			}
			description = FormatDesc ();
		}

		string FormatDesc ()
		{
			string desc = $"{ViewModel.HomeTeamText}-{ViewModel.AwayTeamText} ({ViewModel.LocalScore}-{ViewModel.AwayScore})\n" +
				$"{Catalog.GetString ("Date")}: {ViewModel.MatchDate.ToShortDateString ()}\n" +
				$"{Catalog.GetString ("Competition")}: {ViewModel.Competition}\n" +
				$"{Catalog.GetString ("Season")}: {ViewModel.Season}";
			return desc;
		}
	}
}
