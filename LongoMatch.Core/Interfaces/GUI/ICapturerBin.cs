// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using LongoMatch.Core.Interfaces.Multimedia;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Core.Interfaces.GUI
{
	public interface ICapturerBin
	{
		Time CurrentCaptureTime { get; }

		bool Capturing { get; }

		Image CurrentCaptureFrame { get; }

		CaptureSettings CaptureSettings { get; }

		List<string> PeriodsNames { set; }

		List<Period> Periods { get; set; }

		ICapturer Capturer { get; }

		void Run (CaptureSettings settings, MediaFile outputFile);

		void StartPeriod ();

		void PausePeriod ();

		void ResumePeriod ();

		void StopPeriod ();

		void Close ();
	}
}

