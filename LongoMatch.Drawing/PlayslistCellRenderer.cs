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
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Drawing.CanvasObjects.Teams;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Drawing
{
	public class PlayslistCellRenderer
	{
	
		public static ISurface EyeSurface = null;
		public static ISurface ArrowRight = null;
		public static ISurface ArrowDown = null;
		public static Image subsImage = null;

		public static void RenderSeparationLine (IDrawingToolkit tk, IContext context, Area backgroundArea)
		{
			
			double x1, x2, y;
			
			x1 = backgroundArea.Start.X;
			x2 = x1 + backgroundArea.Width;
			y = backgroundArea.Start.Y + backgroundArea.Height; 
			tk.LineWidth = 1;
			tk.StrokeColor = Config.Style.PaletteBackgroundLight;
			tk.DrawLine (new Point (x1, y), new Point (x2, y));
		}

		static void RenderPlayer (IDrawingToolkit tk, Player p, Point imagePoint)
		{
			PlayerObject po = new PlayerObject (p);
			po.Position = new Point (imagePoint.X + StyleConf.ListImageWidth / 2, imagePoint.Y + StyleConf.ListImageWidth / 2);
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
						Image img = Resources.LoadImage (StyleConf.ListArrowRightPath);
						ArrowRight = Config.DrawingToolkit.CreateSurface (img.Width, img.Height, img, false);
					}
					arrow = ArrowRight;
				} else {
					if (ArrowDown == null) {
						Image img = Resources.LoadImage (StyleConf.ListArrowDownPath);
						ArrowDown = Config.DrawingToolkit.CreateSurface (img.Width, img.Height, img, false);
					}
					arrow = ArrowDown;
				}
				arrowY = new Point (cellArea.Start.X + 1, cellArea.Start.Y + cellArea.Height / 2 - arrow.Height / 2);
				tk.DrawSurface (arrow, arrowY);
			}

			tk.LineWidth = 0;
			tk.FillColor = color;
			tk.DrawCircle (new Point (countX1, countYC), StyleConf.ListCountRadio);
			tk.DrawCircle (new Point (countX2, countYC), StyleConf.ListCountRadio);
			tk.DrawRectangle (new Point (countX1, countY), StyleConf.ListCountWidth, 2 * StyleConf.ListCountRadio);
			tk.StrokeColor = Config.Style.PaletteBackgroundDark;
			tk.FontAlignment = FontAlignment.Center;
			tk.FontWeight = FontWeight.Bold;
			tk.FontSize = 14;
			tk.DrawText (new Point (countX1, countY), StyleConf.ListCountWidth,
				2 * StyleConf.ListCountRadio, count.ToString ());
		}

		static void RenderBackgroundAndText (bool isExpanded, IDrawingToolkit tk, Area backgroundArea, Point textP, double textW, string text)
		{
			Color textColor, backgroundColor;

			/* Background */
			tk.LineWidth = 0;
			if (isExpanded) {
				backgroundColor = Config.Style.PaletteBackgroundLight;
				textColor = Config.Style.PaletteSelected;
			} else {
				backgroundColor = Config.Style.PaletteBackground;
				textColor = Config.Style.PaletteWidgets;
			}
			tk.FillColor = backgroundColor;
			tk.DrawRectangle (backgroundArea.Start, backgroundArea.Width, backgroundArea.Height);

			/* Text */
			tk.StrokeColor = textColor;
			tk.FontSize = 14;
			tk.FontWeight = FontWeight.Bold;
			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (textP, textW, backgroundArea.Height, text);
		}

		public static void RenderPlayer (Player player, int count, bool isExpanded, IDrawingToolkit tk,
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
			RenderCount (isExpanded, Config.Style.PaletteActive, count, tk, backgroundArea, cellArea);
			RenderSeparationLine (tk, context, backgroundArea);
			tk.End ();
		}

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
				tk.FillColor = Config.Style.PaletteBackgroundDarkBright;
			} else {
				tk.FillColor = Config.Style.PaletteBackgroundDark;
			}
			tk.DrawRectangle (backgroundArea.Start, backgroundArea.Width, backgroundArea.Height);
			/* Selection rectangle */
			tk.LineWidth = 0;
			tk.FillColor = color;
			tk.DrawRectangle (selectPoint, StyleConf.ListSelectedWidth, backgroundArea.Height);
			tk.FillColor = Config.Style.PaletteBackgroundDark;
			tk.DrawCircle (circlePoint, (StyleConf.ListSelectedWidth / 2) - 1);
			if (state.HasFlag (CellState.Selected)) {
				tk.FillColor = Config.Style.PaletteBackground;
				tk.FillColor = Config.Style.PaletteActive;
				tk.DrawCircle (circlePoint, (StyleConf.ListSelectedWidth / 2) - 2);
			}
			
			if (desc != null) {
				tk.FontSize = 10;
				tk.FontWeight = FontWeight.Normal;
				tk.StrokeColor = Config.Style.PaletteSelected;
				tk.FontAlignment = FontAlignment.Left;
				tk.DrawText (textPoint, textWidth, cellArea.Height, desc);
			}
			if (selected) {
				if (EyeSurface == null) {
					Image img = Resources.LoadImage (StyleConf.ListEyeIconPath);
					EyeSurface = Config.DrawingToolkit.CreateSurface (img.Width, img.Height, img, false);
				}
				tk.DrawSurface (EyeSurface, new Point (imagePoint.X - EyeSurface.Width - StyleConf.ListEyeIconOffset, imagePoint.Y + backgroundArea.Height / 2 - EyeSurface.Height / 2));
			}
			if (ss != null) {
				tk.DrawImage (imagePoint, StyleConf.ListImageWidth, cellArea.Height, ss,
					ScaleMode.AspectFit);
			}
		}

		public static void RenderSubstitution (Color color, Time evt, Player playerIn, Player playerOut, bool selected,
		                                       bool isExpanded, IDrawingToolkit tk, IContext context, Area backgroundArea,
		                                       Area cellArea, CellState state)
		{
			Point selectPoint, textPoint, imagePoint, circlePoint;
			Point inPoint, imgPoint, outPoint, timePoint;
			double textWidth;
			
			if (subsImage == null) {
				subsImage = Resources.LoadImage (StyleConf.SubsIcon);
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
			tk.StrokeColor = Config.Style.PaletteSelected;
			tk.FontAlignment = FontAlignment.Left;
			tk.DrawText (timePoint, 100, cellArea.Height, evt.ToSecondsString ());
			RenderSeparationLine (tk, context, backgroundArea);
			tk.End ();
		}

		public static void RenderPlay (Color color, Image ss, IList<Player> players, IEnumerable<Team> teams, bool selected, string desc,
		                               int count, bool isExpanded, IDrawingToolkit tk,
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
				foreach (Player p in players) {
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

		public static void Render (object item, Project project, int count, bool isExpanded, IDrawingToolkit tk,
		                           IContext context, Area backgroundArea, Area cellArea, CellState state)
		{
			if (item is EventType) {
				RenderAnalysisCategory (item as EventType, count, isExpanded, tk,
					context, backgroundArea, cellArea);
			} else if (item is SubstitutionEvent) {
				SubstitutionEvent s = item as SubstitutionEvent;
				RenderSubstitution (s.Color, s.EventTime, s.In, s.Out, s.Selected, isExpanded, tk, context,
					backgroundArea, cellArea, state);
			} else if (item is TimelineEvent) {
				TimelineEvent p = item as TimelineEvent;
				// always add local first.
				RenderPlay (p.Color, p.Miniature, p.Players, p.Teams, p.Selected, p.Description, count, isExpanded, tk,
					context, backgroundArea, cellArea, state);
			} else if (item is Player) {
				RenderPlayer (item as Player, count, isExpanded, tk, context, backgroundArea, cellArea);
			} else if (item is Playlist) {
				RenderPlaylist (item as Playlist, count, isExpanded, tk, context, backgroundArea, cellArea);
			} else if (item is PlaylistPlayElement) {
				PlaylistPlayElement p = item as PlaylistPlayElement;
				RenderPlay (p.Play.EventType.Color, p.Miniature, null, null, p.Selected, p.Description, count, isExpanded, tk,
					context, backgroundArea, cellArea, state);
			} else if (item is IPlaylistElement) {
				IPlaylistElement p = item as IPlaylistElement;
				RenderPlay (Config.Style.PaletteActive, p.Miniature, null, null, p.Selected, p.Description,
					count, isExpanded, tk, context, backgroundArea, cellArea, state);
			} else {
				Log.Error ("No renderer for type " + item.GetType ());
			}
		}
	}
}
