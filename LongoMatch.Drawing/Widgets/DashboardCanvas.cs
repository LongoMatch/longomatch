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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.CanvasObjects.Dashboard;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Drawing;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.Widgets
{
	public class DashboardCanvas: SelectionCanvas
	{
	
		public event ButtonsSelectedHandler ButtonsSelectedEvent;
		public event ButtonSelectedHandler EditButtonTagsEvent;
		public event ActionLinksSelectedHandler ActionLinksSelectedEvent;
		public event ActionLinkCreatedHandler ActionLinkCreatedEvent;
		public event ShowDashboardMenuHandler ShowMenuEvent;
		public event NewEventHandler NewTagEvent;

		DashboardLongoMatch template;
		DashboardMode mode;
		Time currentTime;
		int templateWidth, templateHeight;
		FitMode fitMode;
		bool modeChanged, showLinks;
		ActionLinkObject movingLink;
		LinkAnchorObject destAnchor;
		Dictionary<DashboardButton, DashboardButtonObject> buttonsDict;

		public DashboardCanvas (IWidget widget) : base (widget)
		{
			Accuracy = 5;
			Mode = DashboardMode.Edit;
			FitMode = FitMode.Fit;
			CurrentTime = new Time (0);
			AddTag = new Tag ("", "");
			BackgroundColor = Config.Style.PaletteBackground;
			buttonsDict = new Dictionary<DashboardButton, DashboardButtonObject> ();
		}

		public DashboardCanvas () : this (null)
		{
		}

		public ProjectLongoMatch Project {
			get;
			set;
		}

		public DashboardLongoMatch Template {
			set {
				template = value;
				LoadTemplate ();
			}
			get {
				return template;
			}
		}

		public Tag AddTag {
			get;
			set;
		}

		public bool Edited {
			get;
			set;
		}

		public Time CurrentTime {
			set {
				currentTime = value;
				foreach (TimerObject to in Objects.OfType<TimerObject>()) {
					to.CurrentTime = value;
				}
				foreach (TimedTaggerObject to in Objects.OfType<TimedTaggerObject>()) {
					if (to.TimedButton.TagMode == TagMode.Free) {
						to.CurrentTime = value;
					}
				}
			}
			get {
				return currentTime;
			}
		}

		public DashboardMode Mode {
			set {
				modeChanged = true;
				mode = value;
				ObjectsCanMove = mode == DashboardMode.Edit;
				foreach (DashboardButtonObject to in Objects.OfType<DashboardButtonObject> ()) {
					to.Mode = value;
				}
				ClearSelection ();
			}
			get {
				return mode;
			}
		}

		public bool ShowLinks {
			set {
				showLinks = value;
				foreach (DashboardButtonObject to in Objects.OfType<DashboardButtonObject> ()) {
					to.ShowLinks = showLinks;
					to.ResetDrawArea ();
				}
				foreach (ActionLinkObject ao in Objects.OfType<ActionLinkObject> ()) {
					ao.Visible = showLinks;
					ao.ResetDrawArea ();
				}
				ClearSelection ();
				widget?.ReDraw ();
			}
			get {
				return showLinks;
			}
		}

		public FitMode FitMode {
			set {
				fitMode = value;
				HandleSizeChangedEvent ();
				modeChanged = true;
			}
			get {
				return fitMode;
			}
		}

		public void Click (DashboardButton b, Tag tag = null)
		{
			DashboardButtonObject co = Objects.OfType<DashboardButtonObject> ().FirstOrDefault (o => o.Button == b);
			if (tag != null && co is CategoryObject) {
				(co as CategoryObject).ClickTag (tag);
			} else {
				co.Click ();
			}
		}

		public void RedrawButton (DashboardButton b)
		{
			DashboardButtonObject co = Objects.OfType<DashboardButtonObject> ().FirstOrDefault (o => o.Button == b);
			if (co != null) {
				co.ReDraw ();
			}
		}

		public void Refresh (DashboardButton b = null)
		{
			DashboardButtonObject to;
			
			if (Template == null) {
				return;
			}
			
			LoadTemplate ();
			to = Objects.OfType<DashboardButtonObject> ().
				FirstOrDefault (o => o.Button == b);
			if (to != null) {
				UpdateSelection (new Selection (to, SelectionPosition.All, 0));
			}
		}

		public override void SetWidget (IWidget newWidget)
		{
			base.SetWidget (newWidget);
			HandleSizeChangedEvent ();
		}

		protected override void ShowMenu (Point coords)
		{
			List<DashboardButton> buttons;
			List<ActionLink> links;

			if (ShowMenuEvent == null || Selections.Count == 0)
				return;

			buttons = Selections.Where (s => s.Drawable is DashboardButtonObject).
				Select (s => (s.Drawable as DashboardButtonObject).Button).ToList ();
			links = Selections.Where (s => s.Drawable is ActionLinkObject).
				Select (s => (s.Drawable as ActionLinkObject).Link).ToList ();
			ShowMenuEvent (buttons, links);
		}

		protected override Selection GetSelection (Point coords, bool inMotion = false, bool skipSelected = false)
		{
			Selection sel = null;
			Selection selected = null;

			/* Regular GetSelection */
			if (!ShowLinks)
				return base.GetSelection (coords, inMotion, skipSelected);

			/* With ShowLinks, only links and anchor can be selected */
			if (Selections.Count > 0) {
				selected = Selections.LastOrDefault ();
			}

			foreach (ICanvasSelectableObject co in Objects) {
				sel = co.GetSelection (coords, Accuracy, inMotion);
				if (sel == null || sel.Drawable is DashboardButtonObject)
					continue;
				if (skipSelected && selected != null && sel.Drawable == selected.Drawable)
					continue;
				break;
			}
			return sel;
		}

		protected override void SelectionMoved (Selection sel)
		{
			if (sel.Drawable is DashboardButtonObject) {
				HandleSizeChangedEvent ();
				Edited = true;
			} else if (sel.Drawable is ActionLinkObject) {
				ActionLinkObject link = sel.Drawable as ActionLinkObject;
				LinkAnchorObject anchor = null;
				Selection destSel;

				destSel = GetSelection (MoveStart, true, true);
				if (destSel != null && destSel.Drawable is LinkAnchorObject) { 
					anchor = destSel.Drawable as LinkAnchorObject;
				}
				/* Toggled highlited state */
				if (anchor != destAnchor) {
					if (destAnchor != null) {
						destAnchor.Highlighted = false;
					}
					/* Only highlight valid targets */
					if (link.CanLink (anchor)) {
						anchor.Highlighted = true;
						destAnchor = anchor;
					} else {
						destAnchor = null;
					}
				}
			}
			base.SelectionMoved (sel);
		}

		protected override void SelectionChanged (List<Selection> sel)
		{
			if (sel.Count == 0) {
				if (ButtonsSelectedEvent != null) {
					ButtonsSelectedEvent (new List<DashboardButton> ());
				}
				return;
			}

			if (sel [0].Drawable is DashboardButtonObject) {
				List<DashboardButton> buttons;

				buttons = sel.Select (s => (s.Drawable as DashboardButtonObject).Button).ToList ();
				if (Mode == DashboardMode.Edit) {
					if (ButtonsSelectedEvent != null) {
						ButtonsSelectedEvent (buttons);
					}
				}
			} else if (sel [0].Drawable is ActionLinkObject) {
				List<ActionLink> links;

				links = sel.Select (s => (s.Drawable as ActionLinkObject).Link).ToList ();
				if (Mode == DashboardMode.Edit) {
					if (ActionLinksSelectedEvent != null) {
						ActionLinksSelectedEvent (links);
					}
				}
			}
			base.SelectionChanged (sel);
		}

		protected override void StartMove (Selection sel)
		{
			if (sel != null && sel.Drawable is LinkAnchorObject) {
				LinkAnchorObject anchor = sel.Drawable as LinkAnchorObject;
				ActionLink link = new ActionLink {
					SourceButton = anchor.Button.Button,
					SourceTags = new ObservableCollection<Tag> (anchor.Tags)
				}; 
				movingLink = new ActionLinkObject (anchor, null, link);
				AddObject (movingLink);
				ClearSelection ();
				UpdateSelection (new Selection (movingLink, SelectionPosition.LineStop, 0), false);
			}
			base.StartMove (sel);
		}

		protected override void StopMove (bool moved)
		{
			Selection sel = Selections.FirstOrDefault ();

			if (movingLink != null) {
				if (destAnchor != null) {
					ActionLink link = movingLink.Link;
					link.DestinationButton = destAnchor.Button.Button;
					link.DestinationTags = new ObservableCollection<Tag> (destAnchor.Tags);
					link.SourceButton.ActionLinks.Add (link);
					movingLink.Destination = destAnchor;
					destAnchor.Highlighted = false;
					if (ActionLinkCreatedEvent != null) {
						ActionLinkCreatedEvent (link);
					}
					Edited = true;
				} else {
					RemoveObject (movingLink);
					widget.ReDraw ();
				}
				ClearSelection ();
				movingLink = null;
				destAnchor = null;
				return;
			}

			if (sel != null && moved) {
				if (sel.Drawable is DashboardButtonObject) {
					/* Round the position of the button to match a corner in the grid */
					int i = VASDrawing.Constants.CATEGORY_TPL_GRID;
					DashboardButton tb = (sel.Drawable as DashboardButtonObject).Button;
					tb.Position.X = VASDrawing.Utils.Round (tb.Position.X, i);
					tb.Position.Y = VASDrawing.Utils.Round (tb.Position.Y, i);
					tb.Width = (int)VASDrawing.Utils.Round (tb.Width, i);
					tb.Height = (int)VASDrawing.Utils.Round (tb.Height, i);
					(sel.Drawable as DashboardButtonObject).ResetDrawArea ();
					widget.ReDraw ();
				}
			}
			base.StopMove (moved);
		}

		public override void Draw (IContext context, Area area)
		{
			tk.Context = context;
			DrawBackground ();
			Begin (context);
			if (Mode != DashboardMode.Code) {
				/* Draw grid */
				tk.LineWidth = 1;
				tk.StrokeColor = Color.Grey1;
				tk.FillColor = Color.Grey1;
				/* Vertical lines */
				for (int i = 0; i <= templateHeight; i += VASDrawing.Constants.CATEGORY_TPL_GRID) {
					tk.DrawLine (new Point (0, i), new Point (templateWidth, i));
				}
				/* Horizontal lines */
				for (int i = 0; i < templateWidth; i += VASDrawing.Constants.CATEGORY_TPL_GRID) {
					tk.DrawLine (new Point (i, 0), new Point (i, templateHeight));
				}
			}
			DrawObjects (area);
			End ();
		}

		public void AddButton (DashboardButtonObject button)
		{
			button.ShowLinks = ShowLinks;
			AddObject (button);
			buttonsDict.Add (button.Button, button);
		}

		void LoadTemplate ()
		{
			ClearObjects ();
			buttonsDict.Clear ();

			foreach (TagButton tag in template.List.OfType<TagButton>()) {
				TagObject to = new TagObject (tag);
				to.ClickedEvent += HandleTaggerClickedEvent;
				to.Mode = Mode;
				AddButton (to);
			}
			
			foreach (AnalysisEventButton cat in template.List.OfType<AnalysisEventButton>()) {
				CategoryObject co = new CategoryObject (cat);
				co.ClickedEvent += HandleTaggerClickedEvent;
				co.EditButtonTagsEvent += (t) => {
					if (EditButtonTagsEvent != null)
						EditButtonTagsEvent (t);
				};
				co.Mode = Mode;
				AddButton (co);
			}
			foreach (PenaltyCardButton c in template.List.OfType<PenaltyCardButton>()) {
				CardObject co = new CardObject (c);
				co.ClickedEvent += HandleTaggerClickedEvent;
				co.Mode = Mode;
				AddButton (co);
			}
			foreach (ScoreButton s in template.List.OfType<ScoreButton>()) {
				ScoreObject co = new ScoreObject (s);
				co.ClickedEvent += HandleTaggerClickedEvent;
				co.Mode = Mode;
				AddButton (co);
			}

			foreach (TimerButtonLongoMatch t in template.List.OfType<TimerButtonLongoMatch>()) {
				TimerObject to = new TimerObject (t);
				to.ClickedEvent += HandleTaggerClickedEvent;
				to.Mode = Mode;
				if (Project != null && t.BackgroundImage == null) {
					TeamType team = (t.Timer as TimerLongoMatch).Team;
					if (team == TeamType.LOCAL) {
						to.TeamImage = Project.LocalTeamTemplate.Shield;
					} else if (team == TeamType.VISITOR) {
						to.TeamImage = Project.VisitorTeamTemplate.Shield;
					}
				}
				AddButton (to);
			}

			foreach (DashboardButtonObject buttonObject in buttonsDict.Values) {
				foreach (ActionLink link in buttonObject.Button.ActionLinks) {
					LinkAnchorObject sourceAnchor, destAnchor;
					ActionLinkObject linkObject;

					sourceAnchor = buttonObject.GetAnchor (link.SourceTags);
					try {
						destAnchor = buttonsDict [link.DestinationButton].GetAnchor (link.DestinationTags);
					} catch {
						Log.Error ("Skipping link with invalid destination tags");
						continue;
					}
					linkObject = new ActionLinkObject (sourceAnchor, destAnchor, link);
					link.SourceButton = buttonObject.Button;
					linkObject.Visible = ShowLinks;
					AddObject (linkObject);
				}
			}
			Edited = false;
			HandleSizeChangedEvent ();
		}

		protected override void HandleSizeChangedEvent ()
		{
			if (Template == null || widget == null) {
				return;
			}

			base.HandleSizeChangedEvent ();
			
			FitMode prevFitMode = FitMode;
			templateHeight = template.CanvasHeight + 10;
			templateWidth = template.CanvasWidth + 10;
			/* When going from Original to Fill or Fit, we can't know the new 
			 * size of the shrinked object until we have a resize */
			if (FitMode == FitMode.Original) {
				widget.Width = templateWidth;
				widget.Height = templateHeight;
				ScaleX = ScaleY = 1;
				Translation = new Point (0, 0);
			} else if (FitMode == FitMode.Fill) {
				ScaleX = (double)widget.Width / templateWidth;
				ScaleY = (double)widget.Height / templateHeight;
				Translation = new Point (0, 0);
			} else if (FitMode == FitMode.Fit) {
				double scaleX, scaleY;
				Point translation;
				Image.ScaleFactor (templateWidth, templateHeight,
					(int)widget.Width, (int)widget.Height, ScaleMode.AspectFit,
					out scaleX, out scaleY, out translation);
				ScaleX = scaleX;
				ScaleY = scaleY;
				Translation = translation;
			}
			if (modeChanged) {
				modeChanged = false;
			}

			widget.ReDraw ();
		}

		void HandleTaggerClickedEvent (ICanvasObject co)
		{
			DashboardButtonObject tagger;
			EventButton button;
			Time start = null, stop = null, eventTime = null;
			List<Tag> tags = null;
			PenaltyCard card = null;
			Score score = null;
			
			tagger = co as DashboardButtonObject;
			
			if (tagger is TagObject) {
				TagObject tag = tagger as TagObject;
				if (tag.Active) {
					/* All tag buttons from the same group that are active */
					foreach (TagObject to in Objects.OfType<TagObject>().
					         Where (t => t.TagButton.Tag.Group == tag.TagButton.Tag.Group &&
					       t.Active && t != tagger)) {
						to.Active = false;
					}
				}
				return;
			}

			if (NewTagEvent == null || !(tagger.Button is EventButton)) {
				return;
			}

			button = tagger.Button as EventButton;
			
			if (Mode == DashboardMode.Edit) {
				return;
			}
			
			if (button.TagMode == TagMode.Predefined) {
				stop = CurrentTime + button.Stop;
				start = CurrentTime - button.Start;
				eventTime = CurrentTime;
			} else {
				stop = CurrentTime;
				start = tagger.Start - button.Start;
				eventTime = tagger.Start;
			}
			
			tags = new List<Tag> ();
			if (tagger is CategoryObject) {
				tags.AddRange ((tagger as CategoryObject).SelectedTags);
			}
			foreach (TagObject to in Objects.OfType<TagObject>()) {
				if (to.Active) {
					tags.Add (to.TagButton.Tag);
				}
				to.Active = false;
			}
			NewTagEvent (button.EventType, null, null, tags, start, stop, eventTime, button);
		}
	}
}

