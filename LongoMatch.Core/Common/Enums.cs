//
//  Copyright (C) 2009 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

namespace LongoMatch.Core.Common
{
	public enum TeamType
	{
		NONE = 0,
		LOCAL = 1,
		VISITOR = 2,
		BOTH = 3,
	}

	public enum KeyAction
	{
		None,
		TogglePlay,
		FrameUp,
		FrameDown,
		SpeedUp,
		SpeedDown,
		JumpUp,
		JumpDown,
		Prev,
		Next,
		CloseEvent,
		DrawFrame,
		EditEvent,
		DeleteEvent,
		StartPeriod,
		StopPeriod,
		PauseClock,
		LocalPlayer,
		VisitorPlayer,
		Substitution,
		ShowDashboard,
		ShowTimeline,
		ShowPositions,
		ZoomIn,
		ZoomOut,
		FitTimeline,
		VideoZoomOriginal,
		VideoZoomIn,
		VideoZoomOut
	}

	public enum SubstitutionReason
	{
		PlayersSubstitution,
		PositionChange,
		BenchPositionChange,
		Injury,
		TemporalExclusion,
		Exclusion,
	}
}
