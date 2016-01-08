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
using Gtk;
using LongoMatch.Core;
using LongoMatch.Core.Handlers;
using LongoMatch.Gui;
using LongoMatch.Core.Store;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Gui.Component;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OpenProjectPanel : Gtk.Bin, IPanel
	{
		public event BackEventHandle BackEvent;

		public OpenProjectPanel ()
		{
			this.Build ();
			
			projectlistwidget.ProjectSelected += HandleProjectSelected;
			projectlistwidget.SelectionMode = SelectionMode.Single;
			projectlistwidget.ViewMode = ProjectListViewMode.Icons;
			panelheader1.ApplyVisible = false;
			panelheader1.BackClicked += HandleClicked;
			panelheader1.Title = Catalog.GetString ("OPEN PROJECT");
		}

		public List<Project> Projects {
			set {
				projectlistwidget.Fill (value);
			}
		}

		public void OnLoaded ()
		{

		}

		public void OnUnloaded ()
		{

		}
		void HandleClicked (object sender, EventArgs e)
		{
			if (BackEvent != null)
				BackEvent ();
		}

		void HandleProjectSelected (Project project)
		{
			Config.EventsBroker.EmitOpenProjectID (project.ID, project);
		}
	}
}

