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

namespace LongoMatch.Interfaces.Drawing
{

	public interface IContext:IDisposable {
		object Value {get;}
	}
	
	public interface ISurface:IDisposable  {
		Image Copy ();
		object Value {get;}
		IContext Context {get;}
		int Width { get; }
		int Height { get; }
	}
	
	public interface IDrawingToolkit
	{
		IContext Context {set;}
		int LineWidth {set;}
		bool ClearOperation {set;}
		Color StrokeColor {set;}
		Color FillColor {set;}
		string FontFamily {set;}
		FontSlant FontSlant {set;}
		FontWeight FontWeight {set;}
		FontAlignment FontAlignment {set;}
		int FontSize {set;}
		LineStyle LineStyle {set;}
		
		ISurface CreateSurface (string filename);
		ISurface CreateSurface (int width, int height, Image image=null);
		void DrawSurface (ISurface surface, Point p = null);
		void Begin();
		void End();
		void Clear (Color color);
		void TranslateAndScale (Point translation, Point scale);
		void DrawLine (Point start, Point stop);
		void DrawTriangle (Point corner, double width, double height,
		                   SelectionPosition orientation);
		void DrawRectangle (Point start, double width, double height);
		void DrawRoundedRectangle (Point start, double width, double height, double radius);
		void DrawButton (Point start, double width, double height, double radius,
		                 Color startColor, Color stopColor);
		void DrawArea (params Point[] vertices);
		void DrawPoint (Point point);
		void DrawCircle (Point center, double radius);
		void DrawEllipse (Point center, double axisX, double axisY);
		void DrawText (Point point, double width, double height, string text);
		void DrawImage (Image image); 
		void DrawImage (Point start, double width, double height, Image image, bool scale); 
		void DrawArrow(Point start, Point stop, int lenght, double degrees, bool closed);
		void Save (ICanvas canvas, double width, double height, string filename);
		Image Copy (ICanvas canvas, double width, double height);
	}
}

