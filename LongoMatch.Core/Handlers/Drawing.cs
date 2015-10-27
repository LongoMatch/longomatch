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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;

namespace LongoMatch.Core.Handlers.Drawing
{
	public delegate void DrawingHandler (IContext context,Area area);
	public delegate void ButtonPressedHandler (Point coords,uint time,ButtonType type,ButtonModifier modifier,ButtonRepetition repetition);
	public delegate void ButtonReleasedHandler (Point coords,ButtonType type,ButtonModifier modifier);
	public delegate void MotionHandler (Point coords);
	public delegate void ShowTooltipHandler (Point coords);
	public delegate void SizeChangedHandler ();
	public delegate void CanvasHandler (ICanvasObject co);
	public delegate void RedrawHandler (ICanvasObject co,Area area);

}

