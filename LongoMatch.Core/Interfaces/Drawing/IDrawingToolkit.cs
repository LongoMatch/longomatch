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
using LongoMatch.Common;
using System.Collections.Generic;

namespace LongoMatch.Interfaces
{
	public interface IDrawingToolkit
	{
		object Context {set;}
		int LineWidth {set;}
		Color StrokeColor {set;}
		Color FillColor {set;}
		string FontFamily {set;}
		FontSlant FontSlant {set;}
		FontWeight FontWeight {set;}
		int FontSize {set;}
		
		void Begin();
		void End();
		void TranslateAndScale (Point translation, Point scale);
		void DrawLine (Point start, Point stop);
		void DrawTriangle (Point corner, double width, double height,
		                   SelectionPosition orientation);
		void DrawRectangle (Point start, double width, double height);
		void DrawRoundedRectangle (Point start, double width, double height, double radius);
		void DrawArea (List<Point> vertices);
		void DrawPoint (Point point);
		void DrawCircle (Point center, double radius);
		void DrawEllipse (Point center, double axisX, double axisY);
		void DrawText (Point point, double width, double height, string text);
		void DrawImage (Image image); 
		void DrawImage (Point start, double width, double height, Image image, bool scale); 
	}
}

