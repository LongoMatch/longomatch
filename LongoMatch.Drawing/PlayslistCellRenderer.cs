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
using System.Collections.Generic;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Drawing.CanvasObjects.Teams;

namespace LongoMatch.Drawing
{
	public class PlayslistCellRenderer
	{
		protected const int VERTICAL_OFFSET = 5;
		protected const int RIGTH_OFFSET = 5;
		public static ISurface EyeSurface = null;
		public static ISurface ArrowRight = null;
		public static ISurface ArrowDown = null;
		static ISurface PlayIcon;
		static ISurface BtnNormalBackground;
		static ISurface BtnNormalBackgroundPrelight;
		static ISurface BtnNormalBackgroundActive;
		static ISurface BtnNormalBackgroundInsensitive;
		public static Image subsImage = null;

		protected static double offsetX, offsetY = 0;
		static bool playButtonPrelighted = false;

		//FIXME: this uses some resources and render methods that are similar or are the same as the EventCellRenderer in
		//RiftAnalyst, try to reuse the code.
		static PlayslistCellRenderer ()
		{
			PlayIcon = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.PlayButton, false);
			BtnNormalBackground = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalButtonNormalTheme, false);
			BtnNormalBackgroundPrelight = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalButtonPrelightTheme, false);
			BtnNormalBackgroundActive = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalButtonActiveTheme, false);
			BtnNormalBackgroundInsensitive = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.NormalButtonInsensititveTheme, false);
		}

		/// <summary>
		/// Returns the Area that should redraw based on X, Y positions
		/// </summary>
		/// <returns>The area to be redrawn, or null otherwise</returns>
		/// <param name="cellX">Cell x.</param>
		/// <param name="cellY">Cell y.</param>
		/// <param name="TotalY">Total y.</param>
		/// <param name="width">Width.</param>
		/// <param name="viewModel">View model.</param>
		public static Area ShouldRedraw (double cellX, double cellY, double TotalY, int width, IViewModel viewModel)
		{
			Point drawingImagePoint = null;
			double startY = VERTICAL_OFFSET + offsetY;
			double startX = width - offsetX - RIGTH_OFFSET - App.Current.Style.ButtonNormalWidth;
			double margin = cellY - startY;
			//Just to know if its inside PlayButton
			if (cellY > startY && cellY < startY + App.Current.Style.ButtonNormalHeight &&
			    cellX > startX && cellX < startX + App.Current.Style.ButtonNormalWidth) {

				drawingImagePoint = new Point (startX, TotalY - margin);
				playButtonPrelighted = true;
			} else if (playButtonPrelighted) {
				playButtonPrelighted = false;
				drawingImagePoint = new Point (startX, TotalY - margin);
			}
			if (drawingImagePoint == null) {
				return null;
			}
			return new Area (drawingImagePoint, App.Current.Style.ButtonNormalWidth, App.Current.Style.ButtonNormalHeight);
		}

		public static bool ClickedPlayButton (double cellX, double cellY, int width)
		{
			double startY = VERTICAL_OFFSET + offsetY;
			double startX = width - offsetX - RIGTH_OFFSET - App.Current.Style.ButtonNormalWidth;
			if (cellY > startY && cellY < startY + App.Current.Style.ButtonNormalHeight &&
			    cellX > startX && cellX < startX + App.Current.Style.ButtonNormalWidth) {
				return true;
			}
			return false;
		}

		public static void RenderSeparationLine (IDrawingToolkit tk, IContext context, Area backgroundArea)
		{

			double x1, x2, y;

			x1 = backgroundArea.Start.X;
			x2 = x1 + backgroundArea.Width;
			y = backgroundArea.Start.Y + backgroundArea.Height;
			tk.LineWidth = 1;
			tk.StrokeColor = App.Current.Style.PaletteBackgroundLight;
			tk.DrawLine (new Point (x1, y), new Point (x2, y));
		}

		static void RenderPlayer (IDrawingToolkit tk, LMPlayer p, Point imagePoint)
		{
			PlayerView po = App.Current.ViewLocator.Retrieve ("PlayerView") as PlayerView;
			// FIXME: Remove it with everything is ported to MVVM
			po.Player = new LMPlayerVM { Model = p };
			po.Position = imagePoint;
			po.Size = StyleConf.ListImageWidth - 2;
			po.Draw (tk, null);
			po.Dispose ();
		}

		static void RenderTeam (IDrawingToolkit tk, Team team, Point imagePoint)
		{
			tk.DrawImage (imagePoint, StyleConf.ListImageWidth, StyleConf.ListImageWidth, team.Shield,
				ScaleMode.AspectFit);
		}

		static void RenderCount (bool isExpanded, Color color, int count, IDrawingToolkit tk, Area backgroundArea, Area cellArea)
		{
			double countX1, countX2, countY, countYC;
			Point arrowY;
			ISurface arrow;

			countX1 = cellArea.Start.X + StyleConf.ListRowSeparator * 2 + StyleConf.ListCountRadio;
			countX2 = countX1 + StyleConf.ListCountWidth;
			countYC = backgroundArea.Start.Y + backgroundArea.Height / 2;
			countY = countYC - StyleConf.ListCountRadio;
			if (count > 0) {
				if (!isExpanded) {
					if (ArrowRight == null) {
						ArrowRight = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.ListArrowRightPath, false);
					}
					arrow = ArrowRight;
				} else {
					if (ArrowDown == null) {
						ArrowDown = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.ListArrowDownPath, false);
					}
					arrow = ArrowDown;
				}
				arrowY = new Point (cellArea.Start.X + 1, cellArea.Start.Y + cellArea.Height / 2 - arrow.Height / 2);
				tk.DrawSurface (arrowY, StyleConf.ListArrowRightWidth, StyleConf.ListArrowRightHeight, arrow, ScaleMode.AspectFit);
			}

			tk.LineWidth = 0;
			tk.FillColor = color;
			tk.DrawCircle (new Point (countX1, countYC), StyleConf.ListCountRadio);
			tk.DrawCircle (new Point (countX2, countYC), StyleConf.ListCountRadio);
			tk.DrawRectangle (new Point (countX1, countY), StyleConf.ListCountWidth, 2 * StyleConf.ListCountRadio);
			tk.StrokeColor = App.Current.Style.PaletteBackgroundDark;
			tk.FontAlignment = FontAlignment.Center;
			tk.FontWeight = FontWeight.Bold;
			tk.FontSize = 14;
			tk.DrawText (new Point (countX1, countY), StyleConf.ListCountWidth,
				2 * StyleConf.ListCountRadio, count.ToString ());
		}

		static void RenderPlayButton (IDrawingToolkit tk, Area cellArea, bool insensitive, CellState state)
		{
			Point p = new Point (cellArea.Right - App.Current.Style.ButtonNormalWidth - RIGTH_OFFSET,
								cellArea.Top + VERTICAL_OFFSET);
			ISurface background = BtnNormalBackground;
			if (insensitive) {
				background = BtnNormalBackgroundInsensitive;
			} else if (state.HasFlag (CellState.Prelit) && playButtonPrelighted) {
				background = BtnNormalBackgroundPrelight;
			}
			tk.DrawSurface (p, App.Current.Style.ButtonNormalWidth, App.Current.Style.ButtonNormalHeight, background, ScaleMode.AspectFit);
			tk.DrawSurface (p, App.Current.Style.IconLargeHeight, App.Current.Style.IconLargeHeight, PlayIcon, ScaleMode.AspectFit);
		}

		static void RenderBackgroundAndText (bool isExpanded, IDrawingToolkit tk, Area backgroundArea, Point textP, double textW, string text)
		{
			Color textColor, backgroundColor;

			/* Background */
			tk.LineWidth = 0;
			if (isExpanded) {
				backgroundColor = App.Current.Style.PaletteBackgroundLight;
				textColor = App.Current.Style.PaletteSelected;
			} else {
				backgroundColor = App.Current.Style.PaletteBackground;
				textColor = App.Current.Style.PaletteWidgets;
			}
			tk.FillColor = backgroundColor;
			tk.DrawRectangle (backgroundArea.Start, backgroundArea.Width, backgroundArea.Height);

			/* Text */
			tk.StrokeColor = textColor;
			tk.FontSize = StyleConf.ListTextFontSize;
			tk.FontWeight = FontWeight.Bold;
			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (textP, textW, backgroundArea.Height, text);
		}

		public static void RenderPlayer (LMPlayer player, int count, bool isExpanded, IDrawingToolkit tk,
										 IContext context, Area backgroundArea, Area cellArea)
		{
			Point image, text;
			double textWidth;

			image = new Point (StyleConf.ListTextOffset, cellArea.Start.Y);
			text = new Point (image.X + StyleConf.ListRowSeparator + StyleConf.ListImageWidth,
				cellArea.Start.Y);
			textWidth = cellArea.Start.X + cellArea.Width - text.X;

			tk.Context = context;
			tk.Begin ();
			RenderBackgroundAndText (isExpanded, tk, backgroundArea, text, textWidth, player.ToString ());
			/* Photo */
			RenderPlayer (tk, player, image);
			RenderCount (isExpanded, player.Color, count, tk, backgroundArea, cellArea);
			RenderSeparationLine (tk, context, backgroundArea);
			tk.End ();
		}

		public static void RenderPlaylist (Playlist playlist, int count, bool isExpanded, IDrawingToolkit tk,
										   IContext context, Area backgroundArea, Area cellArea)
		{
			Point textP = new Point (StyleConf.ListTextOffset, cellArea.Start.Y);
			tk.Context = context;
			tk.Begin ();
			RenderBackgroundAndText (isExpanded, tk, backgroundArea, textP, cellArea.Width - textP.X, playlist.Name);
			RenderCount (isExpanded, App.Current.Style.PaletteActive, count, tk, backgroundArea, cellArea);
			RenderSeparationLine (tk, context, backgroundArea);
			tk.End ();
		}

		public static void RenderAnalysisCategory (EventTypeTimelineVM vm, int count, bool isExpanded, IDrawingToolkit tk,
		                                           IContext context, Area backgroundArea, Area cellArea, CellState state)
		{
			Point textP = new Point (StyleConf.ListTextOffset, cellArea.Start.Y);
			tk.Context = context;
			tk.Begin ();
			RenderBackgroundAndText (isExpanded, tk, backgroundArea, textP, cellArea.Width - textP.X, vm.EventTypeVM.Name);
			RenderCount (isExpanded, vm.EventTypeVM.Color, count, tk, backgroundArea, cellArea);
			RenderSeparationLine (tk, context, backgroundArea);
			if (!(vm.Model is SubstitutionEventType)) {
				RenderPlayButton (tk, cellArea, vm.VisibleChildrenCount == 0, state);
			}
			tk.End ();
		}

		// FIXME: This method might be deleted when presentations is migrated to MVVMC
		public static void RenderAnalysisCategory (EventType cat, int count, bool isExpanded, IDrawingToolkit tk,
												   IContext context, Area backgroundArea, Area cellArea)
		{
			Point textP = new Point (StyleConf.ListTextOffset, cellArea.Start.Y);
			tk.Context = context;
			tk.Begin ();
			RenderBackgroundAndText (isExpanded, tk, backgroundArea, textP, cellArea.Width - textP.X, cat.Name);
			RenderCount (isExpanded, cat.Color, count, tk, backgroundArea, cellArea);
			RenderSeparationLine (tk, context, backgroundArea);
			tk.End ();
		}

		static void RenderTimelineEventBase (Color color, Image ss, bool selected, string desc, IDrawingToolkit tk,
											 IContext context, Area backgroundArea, Area cellArea, CellState state,
											 out Point selectPoint, out Point textPoint, out Point imagePoint,
											 out Point circlePoint, out double textWidth)
		{
			selectPoint = new Point (backgroundArea.Start.X, backgroundArea.Start.Y);
			textPoint = new Point (selectPoint.X + StyleConf.ListSelectedWidth + StyleConf.ListRowSeparator, selectPoint.Y);
			imagePoint = new Point (textPoint.X + StyleConf.ListTextWidth + StyleConf.ListRowSeparator, selectPoint.Y);
			textWidth = StyleConf.ListTextWidth;
			circlePoint = new Point (selectPoint.X + StyleConf.ListSelectedWidth / 2, selectPoint.Y + backgroundArea.Height / 2);

			tk.LineWidth = 0;
			if (state.HasFlag (CellState.Prelit)) {
				tk.FillColor = App.Current.Style.PaletteBackgroundDarkBright;
			} else {
				tk.FillColor = App.Current.Style.PaletteBackgroundDark;
			}
			tk.DrawRectangle (backgroundArea.Start, backgroundArea.Width, backgroundArea.Height);
			/* Selection rectangle */
			tk.LineWidth = 0;
			tk.FillColor = color;
			tk.DrawRectangle (selectPoint, StyleConf.ListSelectedWidth, backgroundArea.Height);
			tk.FillColor = App.Current.Style.PaletteBackgroundDark;
			tk.DrawCircle (circlePoint, (StyleConf.ListSelectedWidth / 2) - 1);
			if (state.HasFlag (CellState.Selected)) {
				tk.FillColor = App.Current.Style.PaletteBackground;
				tk.FillColor = App.Current.Style.PaletteActive;
				tk.DrawCircle (circlePoint, (StyleConf.ListSelectedWidth / 2) - 2);
			}

			if (desc != null) {
				tk.FontSize = 10;
				tk.FontWeight = FontWeight.Normal;
				tk.StrokeColor = App.Current.Style.PaletteSelected;
				tk.FontAlignment = FontAlignment.Left;
				tk.DrawText (textPoint, textWidth, cellArea.Height, desc);
			}
			if (selected) {
				if (EyeSurface == null) {
					EyeSurface = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.ListEyeIconPath, false);
				}
				tk.DrawSurface (new Point (imagePoint.X - EyeSurface.Width - StyleConf.ListEyeIconOffset, imagePoint.Y + backgroundArea.Height / 2 - EyeSurface.Height / 2), StyleConf.ListEyeIconWidth, StyleConf.ListEyeIconHeight, EyeSurface, ScaleMode.AspectFit);
			}
			if (ss != null) {
				tk.DrawImage (imagePoint, StyleConf.ListImageWidth, cellArea.Height, ss,
					ScaleMode.AspectFit);
			}
		}

		public static void RenderSubstitution (Color color, Time evt, LMPlayer playerIn, LMPlayer playerOut, bool selected,
											   bool isExpanded, IDrawingToolkit tk, IContext context, Area backgroundArea,
											   Area cellArea, CellState state)
		{
			Point selectPoint, textPoint, imagePoint, circlePoint;
			Point inPoint, imgPoint, outPoint, timePoint;
			double textWidth;

			if (subsImage == null) {
				subsImage = App.Current.ResourcesLocator.LoadImage (StyleConf.SubsIcon);
			}
			tk.Context = context;
			tk.Begin ();

			RenderTimelineEventBase (color, null, selected, null, tk, context, backgroundArea, cellArea, state,
				out selectPoint, out textPoint, out imagePoint, out circlePoint, out textWidth);
			inPoint = textPoint;
			imgPoint = new Point (textPoint.X + StyleConf.ListImageWidth + StyleConf.ListRowSeparator, textPoint.Y);
			outPoint = new Point (imgPoint.X + 20 + StyleConf.ListRowSeparator, imgPoint.Y);
			RenderPlayer (tk, playerIn, inPoint);
			tk.DrawImage (imgPoint, 20, cellArea.Height, subsImage, ScaleMode.AspectFit);
			RenderPlayer (tk, playerOut, outPoint);

			timePoint = new Point (outPoint.X + StyleConf.ListImageWidth + StyleConf.ListRowSeparator, textPoint.Y);
			tk.FontSize = 10;
			tk.FontWeight = FontWeight.Normal;
			tk.StrokeColor = App.Current.Style.PaletteSelected;
			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (timePoint, 100, cellArea.Height, evt.ToSecondsString ());
			RenderSeparationLine (tk, context, backgroundArea);
			tk.End ();
		}

		public static void RenderPlay (Color color, Image ss, IList<Player> players, IEnumerable<Team> teams,
									   bool selected, string desc, int count, bool isExpanded, IDrawingToolkit tk,
									   IContext context, Area backgroundArea, Area cellArea, CellState state)
		{
			Point selectPoint, textPoint, imagePoint, circlePoint;
			double textWidth;

			tk.Context = context;
			tk.Begin ();

			RenderTimelineEventBase (color, ss, selected, desc, tk, context, backgroundArea, cellArea, state,
				out selectPoint, out textPoint, out imagePoint, out circlePoint, out textWidth);

			imagePoint.X += StyleConf.ListImageWidth + StyleConf.ListRowSeparator;
			if (players != null && players.Count > 0) {
				foreach (LMPlayer p in players) {
					RenderPlayer (tk, p, imagePoint);
					imagePoint.X += StyleConf.ListImageWidth + StyleConf.ListRowSeparator;
				}
			}
			if (teams != null) {
				foreach (var team in teams) {
					RenderTeam (tk, team, imagePoint);
					imagePoint.X += StyleConf.ListImageWidth + StyleConf.ListRowSeparator;
				}
			}
			RenderSeparationLine (tk, context, backgroundArea);
			tk.End ();
		}

		public static void Render (object item, LMProject project, int count, bool isExpanded, IDrawingToolkit tk,
								   IContext context, Area backgroundArea, Area cellArea, CellState state)
		{
			//Get the offset to properly calulate if needs tooltip or redraw
			offsetX = backgroundArea.Right - cellArea.Right;
			offsetY = cellArea.Top - backgroundArea.Top;

			// HACK: to be remove when all treeviews are migrated to user VM's
			if (item is TimelineEventVM) {
				item = ((TimelineEventVM)item).Model;
			} else if (item is EventTypeTimelineVM) {
				var vm = item as EventTypeTimelineVM;
				RenderAnalysisCategory (vm, count, isExpanded, tk,
					context, backgroundArea, cellArea, state);
				return;
			} else if (item is PlaylistElementVM) {
				item = ((PlaylistElementVM)item).Model;
			} else if (item is PlaylistVM) {
				item = ((PlaylistVM)item).Model;
			} else if (item is PlayerVM) {
				item = ((PlayerVM)item).Model;
			} else if (item is PlayerTimelineVM) {
				item = ((PlayerTimelineVM)item).Model;
			}

			// FIXME: This first if case must be deleted when presentations is migrated to MVVMC
			if (item is EventType) {
				RenderAnalysisCategory (item as EventType, count, isExpanded, tk,
					context, backgroundArea, cellArea);
			} else if (item is SubstitutionEvent) {
				SubstitutionEvent s = item as SubstitutionEvent;
				RenderSubstitution (s.Color, s.EventTime, s.In, s.Out, s.Playing, isExpanded, tk, context,
					backgroundArea, cellArea, state);
			} else if (item is TimelineEvent) {
				LMTimelineEvent p = item as LMTimelineEvent;
				// always add local first.
				RenderPlay (p.Color, p.Miniature, p.Players, p.Teams, p.Playing, p.Description, count, isExpanded, tk,
					context, backgroundArea, cellArea, state);
			} else if (item is Player) {
				RenderPlayer (item as LMPlayer, count, isExpanded, tk, context, backgroundArea, cellArea);
			} else if (item is Playlist) {
				RenderPlaylist (item as Playlist, count, isExpanded, tk, context, backgroundArea, cellArea);
			} else if (item is PlaylistPlayElement) {
				PlaylistPlayElement p = item as PlaylistPlayElement;
				RenderPlay (p.Play.EventType.Color, p.Miniature, null, null, p.Playing, p.Description, count, isExpanded, tk,
					context, backgroundArea, cellArea, state);
			} else if (item is IPlaylistElement) {
				IPlaylistElement p = item as IPlaylistElement;
				RenderPlay (App.Current.Style.PaletteActive, p.Miniature, null, null, p.Playing, p.Description,
					count, isExpanded, tk, context, backgroundArea, cellArea, state);
			} else {
				Log.Error ("No renderer for type " + item.GetType ());
			}
		}
	}
}
