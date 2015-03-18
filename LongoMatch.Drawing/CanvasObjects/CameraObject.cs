﻿//
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
using LongoMatch.Core.Store;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Common;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class CameraObject: TimeNodeObject
	{
		MediaFile mediaFile;

		public CameraObject (MediaFile mf) : 
			base (new TimeNode () { Start = mf.Offset, Stop = mf.Duration + mf.Offset, Name = mf.Name })
		{
			mediaFile = mf;
			SelectionMode = NodeSelectionMode.Segment;
		}

		public override string Description {
			get {
				return mediaFile.Name;
			}
		}
	}
}

