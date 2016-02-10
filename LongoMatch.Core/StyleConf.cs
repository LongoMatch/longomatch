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
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Common
{
	public class StyleConf
	{
		public const string IMAGE_EXT = Constants.IMAGE_EXT;

		public const int WelcomeBorder = 30;
		public const int WelcomeIconSize = 80;
		public const int WelcomeIconImageSize = 36;
		public const int WelcomeLogoWidth = 450;
		public const int WelcomeLogoHeight = 99;
		public const int WelcomeIconsHSpacing = 105;
		public const int WelcomeIconsVSpacing = 55;
		public const int WelcomeIconsTextSpacing = 5;
		public const int WelcomeIconsTextHeight = 20;
		public const int WelcomeIconsTotalRows = 2;
		public const int WelcomeIconsFirstRow = 3;
		public const int WelcomeTextHeight = 20;
		public const int WelcomeMinWidthBorder = 30;

		public const int ProjectTypeIconSize = 80;

		public const int HeaderFontSize = 20;
		public const int HeaderHeight = 60;

		public const int NewHeaderSpacing = 10;
		public const int NewEntryWidth = 150;
		public const int NewEntryHeight = 30;
		public const int NewTableHSpacing = 5;
		public const int NewTableVSpacing = 5;
		public const int NewTeamsComboWidth = 245;
		public const int NewTeamsComboHeight = 60;
		public const int NewTeamsIconSize = 55;
		public const int NewTeamsFontSize = 16;
		public static Color NewTeamsFontColor = Color.White;
		public const int NewTeamsSpacing = 60;
		public const int NewTaggerSpacing = 35;

		public const int ListSelectedWidth = 16;
		public const int ListRowSeparator = 10;
		public const int ListTextWidth = 180;
		public const int ListImageWidth = 50;
		public const int ListCategoryHeight = 50;
		public const int ListCountRadio = 10;
		public const int ListCountWidth = 20;
		public const int ListTextOffset = ListRowSeparator * 2 + StyleConf.ListCountRadio * 2 + StyleConf.ListCountWidth;

		public const int ListEyeIconOffset = 10;
		public const string ListEyeIconPath = "icons/hicolor/scalable/actions/longomatch-eye" + IMAGE_EXT;
		public const string ListArrowRightPath = "icons/hicolor/scalable/actions/longomatch-arrow-right" + IMAGE_EXT;
		public const string ListArrowDownPath = "icons/hicolor/scalable/actions/longomatch-arrow-down" + IMAGE_EXT;

		public const int TeamsShieldIconSize = 45;
		
		public const string TimelineNeedleResource = "icons/hicolor/scalable/actions/longomatch-timeline-needle-big" + IMAGE_EXT;
		public const string TimelineNeedleUP = "icons/hicolor/scalable/actions/longomatch-timeline-needle-up" + IMAGE_EXT;
		public const int TimelineCategoryHeight = 20;
		public const int TimelineCameraHeight = 30;
		public const int TimelineCameraMaxLines = 8;
		public const int TimelineCameraFontSize = 14;
		public const int TimelineLabelsWidth = 200;
		public const int TimelineLabelHSpacing = 10;
		public const int TimelineLabelVSpacing = 2;
		public const int TimelineLineSize = 6;
		public const int TimelineFontSize = 16;
		public const int TimelineRuleFontSize = 12;
		public const int TimelineBackgroundLineSize = 4;
		public const int TimelinePadding = 30;
		public const string TimelineSelectionLeft = "icons/hicolor/scalable/actions/longomatch-timeline-select-left" + IMAGE_EXT;
		public const string TimelineSelectionRight = "icons/hicolor/scalable/actions/longomatch-timeline-select-right" + IMAGE_EXT;

		public const string PlayerArrowOut = "images/player/arrow-out" + IMAGE_EXT;
		public const string PlayerArrowIn = "images/player/arrow-in" + IMAGE_EXT;
		public const string PlayerPhoto = "images/player/photo" + IMAGE_EXT;
		public const int PlayerLineWidth = 2;
		public const int PlayerSize = 60;
		public const int PlayerNumberSize = 20;
		public const int PlayerArrowSize = PlayerNumberSize;
		public const int PlayerNumberX = 0;
		public const int PlayerNumberY = 60 - PlayerLineWidth - PlayerNumberSize + 1;
		public const int PlayerArrowX = PlayerNumberX;
		public const int PlayerArrowY = PlayerNumberY - PlayerArrowSize + 1;
		
		public const string SubsLock = "icons/hicolor/scalable/actions/longomatch-player-swap-lock" + IMAGE_EXT;
		public const string SubsUnlock = "icons/hicolor/scalable/actions/longomatch-player-swap-unlock" + IMAGE_EXT;
		public const string SubsIcon = "icons/hicolor/scalable/actions/longomatch-subs-arrow" + IMAGE_EXT;
		public const string DefaultShield = "icons/hicolor/scalable/actions/longomatch-default-shield" + IMAGE_EXT;

		public const string EditButton = "icons/hicolor/scalable/actions/longomatch-pencil" + IMAGE_EXT;
		public const string ApplyButton = "icons/hicolor/scalable/actions/longomatch-apply-button" + IMAGE_EXT;
		public const string CancelButton = "icons/hicolor/scalable/actions/longomatch-mark" + IMAGE_EXT;
		public const string RecordButton = "icons/hicolor/scalable/actions/longomatch-control-record" + IMAGE_EXT;

		public const int NotebookTabIconSize = 18;
		public const int NotebookTabSize = NotebookTabIconSize + 14;

		public const int TopBarButtonIconSize = 28;
		public const int TopBarToggleButtonIconSize = 18;

		public const int PresentationManagerHeaderIconSize = 54;
		public const int PresentationManagerButtonIconSize = 34;

		public const int ButtonHeaderHeight = 22;
		public const int ButtonHeaderWidth = 5 + 34 + 5;
		public const int ButtonRecWidth = 40;
		public const int ButtonLineWidth = 2;
		public const int ButtonHeaderFontSize = 14;
		public const int ButtonNameFontSize = 18;
		public const int ButtonTimerFontSize = 24;
		public const int ButtonButtonsFontSize = 10;
		public const int ButtonMinWidth = 100;
		public const string ButtonTimerIcon = "images/dashboard/longomatch-timer" + IMAGE_EXT;
		public const string ButtonTagIcon = "images/dashboard/longomatch-tag" + IMAGE_EXT;
		public const string ButtonScoreIcon = "images/dashboard/longomatch-score" + IMAGE_EXT;
		public const string ButtonEventIcon = "images/dashboard/longomatch-event" + IMAGE_EXT;
		public static Color ButtonTagColor = Color.Parse ("#d8ffc7");
		public static Color ButtonTimerColor = Color.Parse ("#bebbff");
		public static Color ButtonScoreColor = Color.Parse ("#d8ffc7");
		public static Color ButtonPenaltyColor = Color.Parse ("#ffc7f0");
		public static Color ButtonEventColor = Color.Parse ("#c7e9ff");
		
		public static int PlayerCapturerIconSize = 20;
		public static int PlayerCapturerControlsHeight = 30;
		
		public int BenchLineWidth = 2;
		public int TeamTaggerBenchBorder = 10;

		public static Color ActionLinkNormal = Color.Parse ("#808080");
		public static Color ActionLinkPrelight = Color.Parse ("#B3B3B3");
		public static Color ActionLinkSelected = Color.Parse ("#ABD05C");
		public const string LinkIn = "icons/hicolor/scalable/actions/longomatch-link-in" + IMAGE_EXT;
		public const string LinkInPrelight = "icons/hicolor/scalable/actions/longomatch-link-in-prelight" + IMAGE_EXT;
		public const string LinkOut = "icons/hicolor/scalable/actions/longomatch-link-out" + IMAGE_EXT;
		public const string LinkOutPrelight = "icons/hicolor/scalable/actions/longomatch-link-out-prelight" + IMAGE_EXT;

		public const string ButtonAlert = "icons/hicolor/scalable/actions/longomatch-alert" + IMAGE_EXT;

		public const int FilterTreeViewToogleWidth = 30;
		public const int FilterTreeViewOnlyRightOffset = 10;
		public const int FilterTreeViewOnlyTopOffset = 2;

		public string Font = "Ubuntu";

		public Color HomeTeamColor { get; set; }

		public Color AwayTeamColor { get; set; }

		public Color PaletteBackground { get; set; }

		public Color PaletteBackgroundSemiLight { get; set; }

		public Color PaletteBackgroundLight { get; set; }

		public Color PaletteBackgroundDark { get; set; }

		public Color PaletteBackgroundDarkBright { get; set; }

		public Color PaletteWidgets { get; set; }

		public Color PaletteSelected { get; set; }

		public Color PaletteActive { get; set; }

		public Color PaletteTool { get; set; }

		public Color PaletteText { get; set; }

		public StyleConf ()
		{
			HomeTeamColor = Color.Red;
			AwayTeamColor = Color.Blue;
			PaletteBackground = Color.Black;
			PaletteBackgroundLight = Color.Black;
			PaletteBackgroundSemiLight = Color.Black;
			PaletteBackgroundDark = Color.Black;
			PaletteBackgroundDarkBright = Color.Black;
			PaletteWidgets = Color.Black;
			PaletteSelected = Color.Black;
			PaletteActive = Color.Black;
			PaletteTool = Color.Black;
			PaletteText = Color.Black;
		}

		public static StyleConf Load (string filename)
		{
			return Serializer.Instance.Load <StyleConf> (filename);
		}
		
	}
}
