﻿//
//  Copyright (C) 2015 fluendo
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
using System.Runtime.Serialization;

namespace LongoMatch.Core.Common
{
	[Serializable]
	public abstract class BaseImage<T>: IDisposable where T: IDisposable
	{

		public BaseImage (T image)
		{
			Value = image;
		}

		public BaseImage (string filename)
		{
			Value = LoadFromFile (filename);
		}

		public T Value {
			get;
			protected set;
		}

		public int Width {
			get {
				return GetWidth ();
			}
		}

		public int Height {
			get {
				return GetHeight ();
			}
		}

		public void Dispose ()
		{
			Value.Dispose ();
		}

		public void ScaleInplace ()
		{
			ScaleInplace (Constants.MAX_THUMBNAIL_SIZE, Constants.MAX_THUMBNAIL_SIZE);
		}

		public void ScaleFactor (int destWidth, int destHeight,
		                         out double scaleX, out double scaleY, out Point offset)
		{
			ScaleFactor (Width, Height, destWidth, destHeight, out scaleX, out scaleY, out offset);
		}

		public static void ScaleFactor (int imgWidth, int imgHeight, int destWidth, int destHeight,
		                                out double scaleX, out double scaleY, out Point offset)
		{
			int oWidth = 0;
			int oHeight = 0;

			ComputeScale (imgWidth, imgHeight, destWidth, destHeight, out oWidth, out oHeight);
			scaleX = (double)oWidth / imgWidth;
			scaleY = (double)oHeight / imgHeight;
			offset = new Point ((destWidth - oWidth) / 2, (destHeight - oHeight) / 2);
		}

		public static void ComputeScale (int inWidth, int inHeight, int maxOutWidth, int maxOutHeight,
		                                 out int outWidth, out int outHeight)
		{
			outWidth = maxOutWidth;
			outHeight = maxOutHeight;

			double par = (double)inWidth / (double)inHeight;
			double outPar = (double)maxOutWidth / (double)maxOutHeight;

			if (outPar > par) {
				outWidth = Math.Min (maxOutWidth, (int)(outHeight * par));
			} else {
				outHeight = Math.Min (maxOutHeight, (int)(outWidth / par));
			}
		}

		public void ScaleInplace (int maxWidth, int maxHeight)
		{
			T scalled;

			scalled = Scale (Value, maxWidth, maxHeight);
			Value.Dispose ();
			Value = scalled;
		}

		public abstract Image Scale (int maxWidth, int maxHeight);

		public abstract byte[] Serialize ();

		public abstract void Save (string filename);

		public abstract Image Composite (Image image2);

		protected abstract T LoadFromFile (string filename);

		protected abstract T Scale (T pix, int maxWidth, int maxHeight);

		protected abstract int GetWidth ();

		protected abstract int GetHeight ();
	}
}
