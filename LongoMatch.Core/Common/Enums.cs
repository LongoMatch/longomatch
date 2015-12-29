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

using System;

namespace LongoMatch.Core.Common
{

	public enum SerializationType
	{
		Binary,
		Xml,
		Json
	}


	public enum ProjectType
	{
		CaptureProject,
		URICaptureProject,
		FakeCaptureProject,
		FileProject,
		EditProject,
		None,
	}

	public enum CapturerType
	{
		Fake,
		Live,
	}

	public enum EndCaptureResponse
	{
		Return = 234,
		Quit = 235,
		Save = 236
	}

	public enum TagMode
	{
		Predefined,
		Free,
		Edit
	}

	public enum SortMethodType
	{
		SortByName = 0,
		SortByStartTime = 1,
		SortByStopTime = 2,
		SortByDuration = 3
	}

	public enum ProjectSortType
	{
		SortByName = 0,
		SortByDate = 1,
		SortByModificationDate = 2,
		SortBySeason = 3,
		SortByCompetition = 4
	}

	public enum TeamType
	{
		NONE = 0,
		LOCAL = 1,
		VISITOR = 2,
		BOTH = 3,
	}

	public enum JobState
	{
		NotStarted,
		Running,
		Finished,
		Cancelled,
		Error,
	}

	public enum VideoEncoderType
	{
		Mpeg4,
		Xvid,
		Theora,
		H264,
		Mpeg2,
		VP8,
	}

	public enum AudioEncoderType
	{
		Mp3,
		Aac,
		Vorbis,
	}

	public enum VideoMuxerType
	{
		Avi,
		Mp4,
		Matroska,
		Ogg,
		MpegPS,
		WebM,
	}

	public enum DrawTool
	{
		None,
		Pen,
		Line,
		Ellipse,
		Rectangle,
		Angle,
		Cross,
		Eraser,
		Selection,
		RectangleArea,
		CircleArea,
		Player,
		Text,
		Counter,
		Zoom,
		CanMove,
		Move,
	}

	public enum CaptureSourceType
	{
		None,
		DV,
		System,
		URI,
	}

	public enum GameUnitEventType
	{
		Start,
		Stop,
		Cancel
	}

	public enum EditorState
	{
		START = 0,
		FINISHED = 1,
		CANCELED = -1,
		ERROR = -2
	}

	public enum JobType
	{
		VideoEdition,
		VideoConversion
	}

	public enum VideoAnalysisMode
	{
		PredefinedTagging,
		ManualTagging,
		Timeline,
		GameUnits,
	}

	/// <summary>
	/// Node selection mode.
	/// </summary>
	public enum NodeSelectionMode
	{
		/// <summary>
		/// The node is not selectable at all.
		/// </summary>
		None,
		/// <summary>
		/// Only borders of the node can be selected.
		/// </summary>
		Borders,
		/// <summary>
		/// Only the inner segment of the node can be selected.
		/// </summary>
		Segment,
		/// <summary>
		/// Both borders and inner segment can be selected.
		/// </summary>
		All,
	}

	/// <summary>
	/// Node border clipping mode.
	/// </summary>
	public enum NodeClippingMode
	{
		/// <summary>
		/// The node can expand freely.
		/// </summary>
		None,
		/// <summary>
		/// The node can expand freely, without disappearing completely.
		/// </summary>
		NoStrict,
		/// <summary>
		/// The node can expand freely to the right, but not to the left.
		/// </summary>
		LeftStrict,
		/// <summary>
		/// The node can expand freely to the left, but not to the right.
		/// </summary>
		RightStrict,
		/// <summary>
		/// The node cannot expand beyond its limits.
		/// </summary>
		Strict,
	}

	/// <summary>
	/// Node dragging mode.
	/// </summary>
	public enum NodeDraggingMode
	{
		/// <summary>
		/// The node is not draggable at all.
		/// </summary>
		None,
		/// <summary>
		/// Only borders of the node can be dragged.
		/// </summary>
		Borders,
		/// <summary>
		/// Only the inner segment of the node can be dragged.
		/// </summary>
		Segment,
		/// <summary>
		/// Both borders and inner segment can be dragged.
		/// </summary>
		All,
	}

	public enum SelectionPosition
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		Left,
		Right,
		Top,
		Bottom,
		LineStart,
		LineStop,
		AngleStart,
		AngleStop,
		AngleCenter,
		CircleBorder,
		All,
	}

	public enum LineStyle
	{
		Normal,
		Dashed,
		Pointed
	}

	public enum LineType
	{
		Simple,
		Arrow,
		DoubleArrow,
		Dot,
		DoubleDot
	}

	public enum FontSlant
	{
		Italic,
		Normal,
		Oblique,
	}

	public enum FontWeight
	{
		Light,
		Normal,
		Bold
	}

	public enum FontAlignment
	{
		Left,
		Right,
		Center,
	}

	public enum ButtonType
	{
		None,
		Left,
		Center,
		Right
	}

	public enum ButtonModifier
	{
		None,
		Shift,
		Control,
		Meta
	}

	public enum ButtonRepetition
	{
		Single,
		Double,
		Triple
	}

	public enum CursorType
	{
		Arrow,
		DoubleArrow,
		Selection,
		Cross,
	}

	public enum MultiSelectionMode
	{
		Single,
		Multiple,
		MultipleWithModifier,
	}

	public enum PlayersIconSize
	{
		Smallest = 20,
		Small = 30,
		Medium = 40,
		Large = 50,
		ExtraLarge = 60
	}

	public enum FieldPositionType
	{
		Field,
		HalfField,
		Goal
	}

	public enum CardShape
	{
		Rectangle,
		Triangle,
		Circle
	}

	public enum FitMode
	{
		Fill,
		Fit,
		Original
	}

	[Flags]
	public enum CellState
	{
		Selected = 1,
		Prelit = 2,
		Insensitive = 4,
		Sorted = 8,
		Focused = 16
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

	public enum FileChooserMode
	{
		MediaFile,
		File,
		Directory,
	}

	[Obsolete]
	public enum MediaFileAngle
	{
		Angle1,
		Angle2,
		Angle3,
		Angle4,
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

	public enum SeekType
	{
		Keyframe,
		Accurate,
		StepUp,
		StepDown,
		None
	}

	public enum ProjectSortMethod
	{
		Name,
		Date,
		ModificationDate,
		Season,
		Competition
	}

	public enum PlayerViewOperationMode
	{
		Synchronization,
		LiveAnalysisReview,
		Analysis,
		Presentation,
	}

	/* The values must be kept in sync with the combobox in
	 * LongoMatch.GUI/Gui/Component/LinkProperties.cs */
	public enum LinkAction
	{
		Toggle = 0,
		Replicate = 1,
	}

	/* The values must be kept in sync with the combobox in
	 * LongoMatch.GUI/Gui/Component/LinkProperties.cs */
	public enum TeamLinkAction
	{
		Clear = 0,
		Keep = 1,
		Invert = 2
	}

	public enum DashboardMode
	{
		Code,
		Edit,
	}

	public enum QueryOperator
	{
		None,
		And,
		Or,
	}

	/// <summary>
	/// Defines how images are scalled.
	/// </summary>
	public enum ScaleMode
	{
		/// <summary>
		/// The image is scalled to fill the defined area without keeping DAR.
		/// </summary>
		Fill,
		/// <summary>
		/// The image is scalled keeping DAR to fit in the defined area, adding black borders if needed.
		/// </summary>
		AspectFill,
		/// <summary>
		/// The image is scalled keeping DAR to fill the defined area, clipping the image if needed.
		/// </summary>
		AspectFit,
	}
}
