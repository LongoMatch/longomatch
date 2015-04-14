//
//  Copyright (C) 2015 Fluendo S.A.
//

using System;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LinkProperties : Gtk.Bin
	{
		ActionLink link;

		public LinkProperties ()
		{
			this.Build ();

			Edited = false;

			comboboxaction.Changed += (sender, e) => {
				link.Action = (LinkAction)comboboxaction.Active;
				Edited = true;
			};
			comboboxteamaction.Changed += (sender, e) => {
				link.TeamAction = (TeamLinkAction)comboboxteamaction.Active;
				Edited = true;
			};
			checkbuttonkeepplayertags.Toggled += (sender, e) => {
				link.KeepPlayerTags = checkbuttonkeepplayertags.Active;
				Edited = true;
			};
			checkbuttonkeepcommontags.Toggled += (sender, e) => {
				link.KeepCommonTags = checkbuttonkeepcommontags.Active;
				Edited = true;
			};
		}

		public bool Edited { get; set; }

		public ActionLink Link {
			set {
				link = value;
				UpdateUI ();
			}
			get {
				return link;
			}
		}

		void UpdateUI () {
			entryfrom.Text = Link.SourceButton.Name;
			entryfromtags.Text = String.Join (", ", Link.SourceTags);
			entryto.Text = Link.DestinationButton.Name;
			entrytotags.Text = String.Join (", ", Link.DestinationTags);
			comboboxaction.Active = (int)link.Action;
			comboboxteamaction.Active = (int)link.TeamAction;
			checkbuttonkeepplayertags.Active = link.KeepPlayerTags;
			checkbuttonkeepcommontags.Active = link.KeepCommonTags;
		}
	}
}

