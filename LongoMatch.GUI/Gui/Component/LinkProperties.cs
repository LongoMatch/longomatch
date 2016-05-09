//
//  Copyright (C) 2015 Fluendo S.A.
//
using System;
using LongoMatch.Core.Common;
using VAS.Core.Store;
using LongoMatch.Core.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LinkProperties : Gtk.Bin
	{
		ActionLinkLongoMatch link;

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
			checkbuttonkeepgenerictags.Toggled += (sender, e) => {
				link.KeepGenericTags = checkbuttonkeepgenerictags.Active;
				Edited = true;
			};
		}

		public bool Edited { get; set; }

		public ActionLinkLongoMatch Link {
			set {
				link = value;
				UpdateUI ();
			}
			get {
				return link;
			}
		}

		void UpdateUI ()
		{
			labelfromdata.Text = Link.SourceButton.Name;
			labelfromtagsdata.Text = String.Join (", ", Link.SourceTags);
			labelfromtagsdata.Visible = (Link.SourceTags.Count != 0);
			// This wonrderful hack is required for the label to re-wrap its content
			labelfromtagsdata.WidthRequest = labelfromtagsdata.Allocation.Width;

			labeltodata.Text = Link.DestinationButton.Name;
			labeltotagsdata.Text = String.Join (", ", Link.DestinationTags);
			labeltotagsdata.Visible = (Link.DestinationTags.Count != 0);
			// This wonrderful hack is required for the label to re-wrap its content
			labeltotagsdata.WidthRequest = labeltotagsdata.Allocation.Width;

			if (Link.SourceButton is TimerButton && Link.DestinationButton is TimerButton) {
				comboboxaction.Visible = labelaction.Visible = true;
				comboboxteamaction.Visible = labelteamaction.Visible = false;
				checkbuttonkeepgenerictags.Visible = labelkeepgenerictags.Visible = false;
				checkbuttonkeepplayertags.Visible = labelkeepplayertags.Visible = false;

				comboboxaction.Active = (int)link.Action;
			} else {
				comboboxaction.Visible = labelaction.Visible = false;
				comboboxteamaction.Visible = labelteamaction.Visible = true;
				checkbuttonkeepgenerictags.Visible = labelkeepgenerictags.Visible = true;
				checkbuttonkeepplayertags.Visible = labelkeepplayertags.Visible = true;

				comboboxteamaction.Active = (int)link.TeamAction;
				checkbuttonkeepplayertags.Active = link.KeepPlayerTags;
				checkbuttonkeepgenerictags.Active = link.KeepGenericTags;
			}
		}
	}
}
