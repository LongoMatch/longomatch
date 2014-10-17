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
using System.Linq;
using LongoMatch.Core.Store.Templates;
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Drawing.CanvasObjects;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Drawing.Widgets
{
	public class DashboardCanvas: SelectionCanvas
	{
	
		public event ButtonsSelectedHandlers TaggersSelectedEvent;
		public event ButtonSelectedHandler EditButtonTagsEvent;
		public event ShowButtonsTaggerMenuHandler ShowMenuEvent;
		public event NewEventHandler NewTagEvent;

		Dashboard template;
		TagMode tagMode;
		Time currentTime;
		int templateWidth, templateHeight;
		FitMode fitMode;
		bool modeChanged;

		public DashboardCanvas (IWidget widget): base (widget)
		{
			Accuracy = 5;
			TagMode = TagMode.Edit;
			widget.SizeChangedEvent += SizeChanged;
			FitMode = FitMode.Fit;
			CurrentTime = new Time (0);
			AddTag = new Tag ("", "");
		}

		public Project Project {
			get;
			set;
		}

		public Dashboard Template {
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
				foreach (CategoryObject co in Objects.OfType<CategoryObject>()) {
					if (co.Button.TagMode == TagMode.Free) {
						co.CurrentTime = value;
					}
				}
			}
			get {
				return currentTime;
			}
		}

		public TagMode TagMode {
			set {
				modeChanged = true;
				tagMode = value;
				ObjectsCanMove = tagMode == TagMode.Edit;
				foreach (TaggerObject to in Objects) {
					to.Mode = value;
				}
				ClearSelection ();
			}
			get {
				return tagMode;
			}
		}

		public FitMode FitMode {
			set {
				fitMode = value;
				SizeChanged ();
				modeChanged = true;
			}
			get {
				return fitMode;
			}
		}

		public void RedrawButton (DashboardButton b)
		{
			TaggerObject co = Objects.OfType<TaggerObject>().FirstOrDefault (o => o.Tagger == b);
			if (co != null) {
				co.ReDraw ();
			}
		}

		public void Refresh (DashboardButton b = null)
		{
			TaggerObject to;
			
			if (Template == null) {
				return;
			}
			
			LoadTemplate ();
			to = (TaggerObject)Objects.FirstOrDefault (o => (o as TaggerObject).Tagger == b);
			if (to != null) {
				UpdateSelection (new Selection (to, SelectionPosition.All, 0));
			}
		}

		protected override void ShowMenu (Point coords)
		{
			Selection sel;
			if (ShowMenuEvent == null)
				return;
			
			sel = Selections.LastOrDefault ();
			if (sel != null) {
				TaggerObject to = sel.Drawable as TaggerObject;
				ShowMenuEvent (to.Tagger, null);
			}
		}

		protected override void SelectionMoved (Selection sel)
		{
			SizeChanged ();
			Edited = true;
			base.SelectionMoved (sel);
		}

		protected override void SelectionChanged (List<Selection> sel)
		{
			List<DashboardButton> taggers;
			
			taggers = sel.Select (s => (s.Drawable as TaggerObject).Tagger).ToList ();
			if (TagMode == TagMode.Edit) {
				if (TaggersSelectedEvent != null) {
					TaggersSelectedEvent (taggers);
				}
			}
			base.SelectionChanged (sel);
		}

		protected override void StopMove (bool moved)
		{
			Selection sel = Selections.FirstOrDefault ();
			
			if (sel != null && moved) {
				int i = Constants.CATEGORY_TPL_GRID;
				DashboardButton tb = (sel.Drawable as TaggerObject).Tagger;
				tb.Position.X = Utils.Round (tb.Position.X, i);
				tb.Position.Y = Utils.Round (tb.Position.Y, i);
				tb.Width = (int)Utils.Round (tb.Width, i);
				tb.Height = (int)Utils.Round (tb.Height, i);
				(sel.Drawable as TaggerObject).ResetDrawArea ();
				widget.ReDraw ();
			}

			base.StopMove (moved);
		}

		public override void Draw (IContext context, Area area)
		{
			tk.Context = context;
			tk.Begin ();
			tk.Clear (Config.Style.PaletteBackground);
			if (TagMode == TagMode.Edit) {
				tk.TranslateAndScale (translation, new Point (scaleX, scaleY));
				/* Draw grid */
				tk.LineWidth = 1;
				tk.StrokeColor = Color.Grey1;
				tk.FillColor = Color.Grey1;
				/* Vertical lines */
				for (int i = 0; i <= templateHeight; i += Constants.CATEGORY_TPL_GRID) {
					tk.DrawLine (new Point (0, i), new Point (templateWidth, i));
				}
				/* Horizontal lines */
				for (int i = 0; i < templateWidth; i += Constants.CATEGORY_TPL_GRID) {
					tk.DrawLine (new Point (i, 0), new Point (i, templateHeight));
				}
			}
			tk.End ();
			
			base.Draw (context, area);
		}

		void LoadTemplate ()
		{
			ClearObjects ();
			foreach (TagButton tag in template.List.OfType<TagButton>()) {
				TagObject to = new TagObject (tag);
				to.ClickedEvent += HandleTaggerClickedEvent;
				to.Mode = TagMode;
				AddObject (to);
			}
			
			foreach (AnalysisEventButton cat in template.List.OfType<AnalysisEventButton>()) {
				CategoryObject co = new CategoryObject (cat);
				co.ClickedEvent += HandleTaggerClickedEvent;
				co.EditButtonTagsEvent += (t) => EditButtonTagsEvent (t);
				co.Mode = TagMode;
				AddObject (co);
			}
			foreach (PenaltyCardButton c in template.List.OfType<PenaltyCardButton>()) {
				CardObject co = new CardObject (c);
				co.ClickedEvent += HandleTaggerClickedEvent;
				co.Mode = TagMode;
				AddObject (co);
			}
			foreach (ScoreButton s in template.List.OfType<ScoreButton>()) {
				ScoreObject co = new ScoreObject (s);
				co.ClickedEvent += HandleTaggerClickedEvent;
				co.Mode = TagMode;
				AddObject (co);
			}

			foreach (TimerButton t in template.List.OfType<TimerButton>()) {
				TimerObject to = new TimerObject (t);
				to.ClickedEvent += HandleTaggerClickedEvent;
				to.Mode = TagMode;
				if (Project != null && t.BackgroundImage == null) {
					if (t.Timer.Team == Team.LOCAL) {
						to.TeamImage = Project.LocalTeamTemplate.Shield;
					} else if (t.Timer.Team == Team.VISITOR) {
						to.TeamImage = Project.VisitorTeamTemplate.Shield;
					}
				}
				AddObject (to);
			}
			Edited = false;
			SizeChanged ();
		}

		void SizeChanged ()
		{
			if (Template == null) {
				return;
			}
			
			FitMode prevFitMode = FitMode;
			templateHeight = template.CanvasHeight + 10;
			templateWidth = template.CanvasWidth + 10;
			/* When going from Original to Fill or Fit, we can't know the new 
			 * size of the shrinked object until we have a resize */
			if (FitMode == FitMode.Original) {
				widget.Width = templateWidth;
				widget.Height = templateHeight;
				scaleX = scaleY = 1;
				translation = new Point (0, 0);
			} else if (FitMode == FitMode.Fill) {
				scaleX = (double)widget.Width / templateWidth;
				scaleY = (double)widget.Height / templateHeight;
				translation = new Point (0, 0);
			} else if (FitMode == FitMode.Fit) {
				Image.ScaleFactor (templateWidth, templateHeight,
				                   (int)widget.Width, (int)widget.Height,
				                   out scaleX, out scaleY, out translation);
			}
			if (modeChanged) {
				modeChanged = false;
				foreach (TaggerObject to in Objects) {
					to.ResetDrawArea ();
				}
			}
			widget.ReDraw ();
		}

		void HandleTaggerClickedEvent (ICanvasObject co)
		{
			TaggerObject tagger;
			EventButton button;
			Time start = null, stop = null, eventTime = null;
			List<Tag> tags = null;
			PenaltyCard card = null;
			Score score = null;
			
			tagger = co as TaggerObject;
			if (NewTagEvent == null || !(tagger.Tagger is EventButton)) {
				return;
			}

			button = tagger.Tagger as EventButton;
			
			if (TagMode == TagMode.Edit) {
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
			if (button is PenaltyCardButton) {
				card = (button as PenaltyCardButton).PenaltyCard;
			}
			if (button is ScoreButton) {
				score = (button as ScoreButton).Score;
			}
			
			NewTagEvent (button.EventType, null, Team.NONE, tags, start, stop, eventTime, score, card);
		}
	}
}

