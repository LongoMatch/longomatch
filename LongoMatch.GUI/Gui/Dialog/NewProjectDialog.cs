// NewProjectDialog.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System.Collections.Generic;
using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Utils;
using LongoMatch.Interfaces;

namespace LongoMatch.Gui.Dialog
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class NewProjectDialog : Gtk.Dialog
	{

		public NewProjectDialog()
		{
			this.Build();
			fdwidget.Clear();
		}

		public ProjectType Use {
			set {
				fdwidget.Use = value;
			}
		}

		public Project Project {
			get {
				return fdwidget.GetProject();
			}
			set {
				fdwidget.SetProject(value);
			}
		}

		public List<Device> Devices {
			set {
				fdwidget.FillDevices(value);
			}
		}

		public CaptureSettings CaptureSettings {
			get {
				return fdwidget.CaptureSettings;
			}
		}
		
		public ITemplatesService TemplatesService {
			set {
				fdwidget.TemplatesService = value;
			}
		}
	}
}
