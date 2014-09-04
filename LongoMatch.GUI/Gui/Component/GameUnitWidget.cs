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
using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Core.Handlers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GameUnitWidget : Gtk.Frame
	{
		public event GameUnitHandler GameUnitEvent;
		
		GameUnit gameUnit;
		Button startButton, stopButton, cancelButton;
		Label label;
		Time start;
		Time current;
		
		public GameUnitWidget (GameUnit gameUnit){
			AddGameUnitButton();
			GameUnit = gameUnit;
			CurrentTime = new Time {MSeconds = 0};
		}
		
		public GameUnit GameUnit{
			set {
				gameUnit = value;
				Label = gameUnit.Name;
			}
			get {
				return gameUnit;
			}
		}
		
		public Time CurrentTime {
			set {
				current = value;
				if (start != null) {
					Time ellapsed = value-start;
					stopButton.Visible = ellapsed.MSeconds > 0;
					label.Text = Catalog.GetString("Time" + ": " + (value-start).ToSecondsString());
				} else {
					label.Text = "";
				}
			}
		}
		
		private void AddGameUnitButton() {
			HBox box = new HBox();
			startButton = new Button("gtk-media-record");
			label = new Label("");
			stopButton = new Button("gtk-media-stop");
			cancelButton = new Button("gtk-cancel");
			
			startButton.Clicked += OnButtonClicked;
			stopButton.Clicked += OnButtonClicked;
			cancelButton.Clicked += OnButtonClicked; 
			
			box.PackStart(startButton, false, true, 0);
			box.PackStart(label, false, true, 0);
			box.PackStart(stopButton, false, true, 0);
			box.PackStart(cancelButton, false, true, 0);
			Add(box);
			
			startButton.Show();
			stopButton.Show();
			cancelButton.Show();
			label.Show();
			box.Show();
			
			SetMode(true);
		}
		
		public void SetMode(bool tagging) {
			startButton.Visible = tagging;
			stopButton.Visible = !tagging;
			cancelButton.Visible = !tagging;
			label.Visible = !tagging;
		}
		
		void EmitGameUnitEvent (GameUnitEventType eType) {
			Log.Debug("Emitting  GameUnitEvent of type: " + eType);
			if (GameUnitEvent != null)
				GameUnitEvent(GameUnit, eType);
		}

		void OnButtonClicked (object sender, EventArgs args)
		{
			GameUnitEventType eType;
			
			SetMode(sender != startButton);
			if (sender == startButton) {
				start = current;
				eType = GameUnitEventType.Start;
			}
			else if (sender == stopButton) {
				eType = GameUnitEventType.Stop;
				start = null;
			}
			else
				eType = GameUnitEventType.Cancel;
			
			EmitGameUnitEvent(eType);
		}
	}
}

