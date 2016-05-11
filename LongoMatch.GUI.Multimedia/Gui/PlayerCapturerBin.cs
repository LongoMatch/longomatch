// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VASUi = VAS.UI;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlayerCapturerBin : VAS.UI.PlayerCapturerBin
	{
		public PlayerCapturerBin () : base ()
		{
		}

		protected override void HandleElementLoadedEvent (object element, bool hasNext)
		{
			if (element == null) {
				if (mode == PlayerViewOperationMode.Analysis) {
					return;
				}
				livebox.Visible = replayhbox.Visible = false;
				Player.Pause ();
				ShowCapturer ();
			} else {
				if (element is TimelineEventLongoMatch && mode == PlayerViewOperationMode.LiveAnalysisReview) {
					ShowPlayer ();
					livebox.Visible = replayhbox.Visible = true;
				}
			}
		}
	}
}

