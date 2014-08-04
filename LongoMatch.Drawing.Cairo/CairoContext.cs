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
using Gdk;
using Cairo;
using LongoMatch.Interfaces.Drawing;

namespace LongoMatch.Drawing.Cairo
{
	public class CairoContext: IContext
	{
		public CairoContext (Window window)
		{
			Value = CairoHelper.Create (window);
		}

		public CairoContext (global::Cairo.Surface surface)
		{
			Value = new Context (surface);
		}

		public CairoContext (Context context)
		{
			Value = context;
		}

		public object Value {
			get;
			protected set;
		}

		public void Dispose ()
		{
			(Value as Context).Dispose ();
		}
	}
}

