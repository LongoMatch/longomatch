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
using VAS.Drawing.CanvasObjects.Dashboard;

namespace LongoMatch.Drawing.Widgets
{
	public class SportDashboardCanvas: VAS.Drawing.Widgets.DashboardCanvas
	{
		public new event ButtonSelectedHandler EditButtonTagsEvent;
		public new event NewEventHandler NewTagEvent;

		public SportDashboardCanvas (IWidget widget) : base (widget)
		{
			Accuracy = 5;
			Mode = DashboardMode.Edit;
			FitMode = FitMode.Fit;
			CurrentTime = new Time (0);
			AddTag = new Tag ("", "");
			BackgroundColor = App.Current.Style.PaletteBackground;
		}

		public SportDashboardCanvas () : this (null)
		{
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
		}

		protected override void LoadTemplate ()
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

			foreach (TimerButton t in template.List.OfType<TimerButton>()) {
				TimerObject to = new TimerObject (t);
				to.ClickedEvent += HandleTaggerClickedEvent;
				to.Mode = Mode;
				if (Project != null && t.BackgroundImage == null) {
					TeamType team = (t.Timer as LMTimer).Team;
					if (team == TeamType.LOCAL) {
						if (Project is LMProject) {
							to.TeamImage = (Project as LMProject).LocalTeamTemplate.Shield;
						}
					} else if (team == TeamType.VISITOR) {
						if (Project is LMProject) {
							to.TeamImage = (Project as LMProject).VisitorTeamTemplate.Shield;
						}
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

		protected override void HandleTaggerClickedEvent (ICanvasObject co)
		{
			DashboardButtonObject tagger;
			EventButton button;
			Time start = null, stop = null, eventTime = null;
			List<Tag> tags = null;
			
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

