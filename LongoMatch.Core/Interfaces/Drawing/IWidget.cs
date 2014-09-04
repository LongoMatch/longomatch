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
using LongoMatch.Core.Handlers.Drawing;
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Interfaces.Drawing
{
	public interface IWidget: IDisposable
	{
		event DrawingHandler DrawEvent;
		event ButtonPressedHandler ButtonPressEvent;
		event ButtonReleasedHandler ButtonReleasedEvent;
		event MotionHandler MotionEvent;
		event SizeChangedHandler SizeChangedEvent;
		event ShowTooltipHandler ShowTooltipEvent;
		
		double Width {get;set;}
		double Height {get;set;}
		void ReDraw (Area area = null);
		void ReDraw (IMovableObject drawable);
		void SetCursor (CursorType type);
		void SetCursorForTool (DrawTool tool);
		void ShowTooltip (string text);
	}
}

