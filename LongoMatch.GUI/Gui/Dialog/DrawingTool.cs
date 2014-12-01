//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using Gtk;
using Mono.Unix;
using LongoMatch.Core.Common;
using LongoMatch.Gui.Component;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Helpers;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Interfaces.Drawing;
using Misc = LongoMatch.Gui.Helpers.Misc;
using Color = LongoMatch.Core.Common.Color;
using Drawable = LongoMatch.Core.Store.Drawables.Drawable;
using Image = LongoMatch.Core.Common.Image;

namespace LongoMatch.Gui.Dialog
{
	public partial class DrawingTool : Gtk.Dialog
	{
		TimelineEvent play;
		Blackboard blackboard;
		FrameDrawing drawing;
		Drawable selectedDrawable;
		Gtk.Dialog playerDialog;
		Text playerText;
		Project project;
		double scaleFactor;

		public DrawingTool ()
		{
			this.Build ();
			savebutton.Clicked += OnSavebuttonClicked;
			savetoprojectbutton.Clicked += OnSavetoprojectbuttonClicked;
			blackboard = new Blackboard (new WidgetWrapper (drawingarea));
			blackboard.ConfigureObjectEvent += HandleConfigureObjectEvent;
			blackboard.ShowMenuEvent += HandleShowMenuEvent;
			blackboard.DrawableChangedEvent += HandleDrawableChangedEvent;
			
			selectbutton.Active = true;

			selectbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-select", 20, IconLookupFlags.ForceSvg);
			eraserbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-eraser", 20, IconLookupFlags.ForceSvg);
			penbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-pencil", 20, IconLookupFlags.ForceSvg);
			textbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-text", 20, IconLookupFlags.ForceSvg);
			linebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-arrow", 20, IconLookupFlags.ForceSvg);
			crossbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-mark", 20, IconLookupFlags.ForceSvg);
			rectanglebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-square", 20, IconLookupFlags.ForceSvg);
			ellipsebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-circle", 20, IconLookupFlags.ForceSvg);
			rectanglefilledbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-square-fill", 20, IconLookupFlags.ForceSvg);
			ellipsefilledbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-circle-fill", 20, IconLookupFlags.ForceSvg);
			playerbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-person", 20, IconLookupFlags.ForceSvg);
			numberbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-counter", 20, IconLookupFlags.ForceSvg);
			anglebuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-angle", 20, IconLookupFlags.ForceSvg);

			selectbutton.Toggled += HandleToolClicked;
			eraserbutton.Toggled += HandleToolClicked;
			penbutton.Toggled += HandleToolClicked;
			textbutton.Toggled += HandleToolClicked;
			linebutton.Toggled += HandleToolClicked;
			crossbutton.Toggled += HandleToolClicked;
			rectanglebutton.Toggled += HandleToolClicked;
			ellipsebutton.Toggled += HandleToolClicked;
			rectanglefilledbutton.Toggled += HandleToolClicked;
			ellipsefilledbutton.Toggled += HandleToolClicked;
			playerbutton.Toggled += HandleToolClicked;
			anglebutton.Toggled += HandleToolClicked;
			numberbutton.Toggled += HandleToolClicked;
			
			FillLineStyle ();
			FillLineType ();

			colorbutton.ColorSet += HandleColorSet;
			colorbutton.Color = Misc.ToGdkColor (Color.Red1);
			textcolorbutton.ColorSet += HandleTextColorSet;
			textcolorbutton.Color = Misc.ToGdkColor (Color.White); 
			backgroundcolorbutton.UseAlpha = true;
			backgroundcolorbutton.Alpha = (ushort) (ushort.MaxValue * 0.8);
			backgroundcolorbutton.ColorSet += HandleBackgroundColorSet;
			backgroundcolorbutton.Color = Misc.ToGdkColor (Color.Green1); 
			blackboard.Color = Color.Red1;
			blackboard.TextColor = Color.Grey2;
			blackboard.TextBackgroundColor = new Color (Color.Green1.R, Color.Green1.G,
			                                            Color.Green1.B, 0);
			textspinbutton.Value = 12;
			textspinbutton.ValueChanged += (sender, e) => {
				UpdateTextSize ();};
			linesizespinbutton.ValueChanged += (sender, e) => {
				UpdateLineWidth ();};
			linesizespinbutton.Value = 4;
			
			clearbutton.Clicked += HandleClearClicked;
		}

		public override void Destroy ()
		{
			blackboard.Dispose ();
			base.Destroy ();
		}

		public void LoadPlay (TimelineEvent play, Image frame, FrameDrawing drawing, Project project)
		{
			this.play = play;
			this.drawing = drawing;
			this.project = project;
			scaleFactor = frame.Width / 500;
			blackboard.Background = frame;
			savetoprojectbutton.Visible = true;
			blackboard.Drawing = drawing;
		}

		public void LoadFrame (Image frame, Project project)
		{
			this.project = project;
			drawing = new FrameDrawing ();
			scaleFactor = frame.Width / 500;
			blackboard.Background = frame;
			blackboard.Drawing = drawing;
			savetoprojectbutton.Visible = false;
			UpdateLineWidth ();
			UpdateTextSize ();
		}

		int ScalledSize (int size)
		{
			return (int)(size * scaleFactor);
		}

		int OriginalSize (int size)
		{
			return (int)(size / scaleFactor);
		}

		void FillLineStyle ()
		{
			ListStore formatStore;
			CellRendererPixbuf renderer = new CellRendererPixbuf ();
			
			formatStore = new ListStore (typeof(Gdk.Pixbuf), typeof(LineStyle));
			formatStore.AppendValues (Gdk.Pixbuf.LoadFromResource (Constants.LINE_NORMAL),
			                          LineStyle.Normal);
			formatStore.AppendValues (Gdk.Pixbuf.LoadFromResource (Constants.LINE_DASHED),
			                          LineStyle.Dashed);
			stylecombobox.Clear ();
			stylecombobox.PackStart (renderer, true);
			stylecombobox.AddAttribute (renderer, "pixbuf", 0);
			stylecombobox.Model = formatStore;
			stylecombobox.Active = 0;
			stylecombobox.Changed += HandleLineStyleChanged;
		}

		void FillLineType ()
		{
			ListStore formatStore;
			CellRendererPixbuf renderer = new CellRendererPixbuf ();
			
			formatStore = new ListStore (typeof(Gdk.Pixbuf), typeof(LineStyle));
			formatStore.AppendValues (Gdk.Pixbuf.LoadFromResource (Constants.LINE_NORMAL),
			                          LineType.Simple);
			formatStore.AppendValues (Gdk.Pixbuf.LoadFromResource (Constants.LINE_ARROW),
			                          LineType.Arrow);
			formatStore.AppendValues (Gdk.Pixbuf.LoadFromResource (Constants.LINE_DOUBLE_ARROW),
			                          LineType.DoubleArrow);
			formatStore.AppendValues (Gdk.Pixbuf.LoadFromResource (Constants.LINE_DOT),
			                          LineType.Dot);
			formatStore.AppendValues (Gdk.Pixbuf.LoadFromResource (Constants.LINE_DOUBLE_DOT),
			                          LineType.DoubleDot);
			typecombobox.Clear ();
			typecombobox.PackStart (renderer, true);
			typecombobox.AddAttribute (renderer, "pixbuf", 0);
			typecombobox.Model = formatStore;
			typecombobox.Active = 0;
			typecombobox.Changed += HandleLineTypeChanged;
		}

		void UpdateTextSize ()
		{
			if (selectedDrawable is Text) {
				Text t = (selectedDrawable as Text);
				t.TextSize = ScalledSize (textspinbutton.ValueAsInt);
				QueueDraw (); 
			} else {
				blackboard.FontSize = ScalledSize (textspinbutton.ValueAsInt);
			}
			
		}

		void UpdateLineWidth ()
		{
			int width;
			
			width = ScalledSize (linesizespinbutton.ValueAsInt);
			if (selectedDrawable != null) {
				selectedDrawable.LineWidth = width;
				QueueDraw ();
			} else {
				blackboard.LineWidth = width;
			}
		}

		void EditText (Text text)
		{
			text.Value = MessagesHelpers.QueryMessage (this, Catalog.GetString ("Text"),
			                                           null, text.Value);
			QueueDraw ();
		}

		void EditPlayer (Text text)
		{
			playerText = text;
			if (playerDialog == null) {
				Gtk.Dialog d = new Gtk.Dialog (Catalog.GetString ("Select player"),
				                               this, DialogFlags.Modal | DialogFlags.DestroyWithParent,
				                               Gtk.Stock.Cancel, ResponseType.Cancel);
				d.WidthRequest = 600;
				d.HeightRequest = 400;
				
				DrawingArea da = new DrawingArea ();
				TeamTagger tagger = new TeamTagger (new WidgetWrapper (da));
				tagger.ShowSubstitutionButtons = false;
				tagger.LoadTeams (project.LocalTeamTemplate, project.VisitorTeamTemplate,
				                  project.Dashboard.FieldBackground);
				tagger.PlayersSelectionChangedEvent += players => {
					if (players.Count == 1) {
						Player p = players [0];
						playerText.Value = p.ToString ();
						d.Respond (ResponseType.Ok);
					}
					tagger.ResetSelection ();
				};
				d.VBox.PackStart (da, true, true, 0);
				d.ShowAll ();
				playerDialog = d;
			}
			if (playerDialog.Run () != (int)ResponseType.Ok) {
				text.Value = null;
			}
			playerDialog.Hide ();
		}

		void HandleLineStyleChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			LineStyle style;
				
			stylecombobox.GetActiveIter (out iter);
			style = (LineStyle)stylecombobox.Model.GetValue (iter, 1);
			if (selectedDrawable != null) {
				selectedDrawable.Style = style;
				QueueDraw ();
			} else {
				blackboard.LineStyle = style;
			}
		}

		void HandleLineTypeChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			LineType type;
				
			typecombobox.GetActiveIter (out iter);
			type = (LineType)typecombobox.Model.GetValue (iter, 1);
			if (selectedDrawable != null && selectedDrawable is Line) {
				(selectedDrawable as Line).Type = type;
				QueueDraw ();
			} else {
				blackboard.LineType = type;
			}
		}

		void HandleClearClicked (object sender, EventArgs e)
		{
			string msg = Catalog.GetString ("Do you want to clear the drawing?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				blackboard.Clear ();
			}
		}

		void HandleBackgroundColorSet (object sender, EventArgs e)
		{
			Color c;
			
			c = Misc.ToLgmColor (backgroundcolorbutton.Color,
			                     backgroundcolorbutton.Alpha);
			if (selectedDrawable is Text) {
				Text t = (selectedDrawable as Text);
				t.FillColor = t.StrokeColor = c;
				QueueDraw (); 
			} else {
				blackboard.TextBackgroundColor = c;
			}
		}

		void HandleTextColorSet (object sender, EventArgs e)
		{
			if (selectedDrawable is Text) {
				(selectedDrawable as Text).TextColor = Misc.ToLgmColor (textcolorbutton.Color); 
				QueueDraw ();
			} else {
				blackboard.TextColor = Misc.ToLgmColor (textcolorbutton.Color);
			}			
		}

		void HandleColorSet (object sender, EventArgs e)
		{
			if (selectedDrawable != null) {
				selectedDrawable.StrokeColor = Misc.ToLgmColor (colorbutton.Color);
				if (selectedDrawable.FillColor != null) {
					Color c = Misc.ToLgmColor (colorbutton.Color);
					c.A = selectedDrawable.FillColor.A;
					selectedDrawable.FillColor = c;
				}
				QueueDraw ();
			} else {
				blackboard.Color = Misc.ToLgmColor (colorbutton.Color);
			}
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Delete) {
				blackboard.DeleteSelection ();
			}
			return base.OnKeyPressEvent (evnt);
		}

		void HandleToolClicked (object sender, EventArgs e)
		{
			if (!(sender as RadioButton).Active) {
				return;
			}
			
			if (sender == selectbutton) {
				blackboard.Tool = DrawTool.Selection;
			} else if (sender == eraserbutton) {
				blackboard.Tool = DrawTool.Eraser;
			} else if (sender == penbutton) {
				blackboard.Tool = DrawTool.Pen;
			} else if (sender == textbutton) {
				blackboard.Tool = DrawTool.Text;
			} else if (sender == linebutton) {
				blackboard.Tool = DrawTool.Line;
			} else if (sender == crossbutton) {
				blackboard.Tool = DrawTool.Cross;
			} else if (sender == rectanglebutton) {
				blackboard.Tool = DrawTool.Rectangle;
			} else if (sender == ellipsebutton) {
				blackboard.Tool = DrawTool.Ellipse;
			} else if (sender == rectanglefilledbutton) {
				blackboard.Tool = DrawTool.RectangleArea;
			} else if (sender == ellipsefilledbutton) {
				blackboard.Tool = DrawTool.CircleArea;
			} else if (sender == numberbutton) {
				blackboard.Tool = DrawTool.Counter;
			} else if (sender == anglebutton) {
				blackboard.Tool = DrawTool.Angle;
			} else if (sender == playerbutton) {
				blackboard.Tool = DrawTool.Player;
			}
		}

		void OnSavebuttonClicked (object sender, System.EventArgs e)
		{
			string filename;
			
			filename = FileChooserHelper.SaveFile (this, Catalog.GetString ("Save File as..."),
			                                       null, Config.SnapshotsDir, "PNG Images", new string[] { "*.png" });
			if (filename != null) {
				System.IO.Path.ChangeExtension (filename, ".png");
				blackboard.Save (filename);
				drawing = null;
				Respond (ResponseType.Accept);
			}
		}

		void OnSavetoprojectbuttonClicked (object sender, System.EventArgs e)
		{
			if (!play.Drawings.Contains (drawing)) {
				play.Drawings.Add (drawing);
			}
			drawing.Miniature = blackboard.Save ();
			drawing.Miniature.ScaleInplace (Constants.MAX_THUMBNAIL_SIZE,
			                                Constants.MAX_THUMBNAIL_SIZE);
			play.UpdateMiniature ();
			drawing = null;
			Respond (ResponseType.Accept);
		}

		void HandleConfigureObjectEvent (IBlackboardObject drawable, DrawTool tool)
		{
			if (drawable is Text) {
				if (tool == DrawTool.Text) {
					EditText (drawable as Text);
				} else if (tool == DrawTool.Player) {
					EditPlayer (drawable as Text);
				}
			}
		}

		void HandleDrawableChangedEvent (IBlackboardObject drawable)
		{
			selectedDrawable = drawable as Drawable;
			
			colorbutton.Sensitive = !(drawable is Text);

			if (selectedDrawable == null) {
				colorbutton.Color = Misc.ToGdkColor (blackboard.Color);
				textcolorbutton.Color = Misc.ToGdkColor (blackboard.TextColor);
				backgroundcolorbutton.Color = Misc.ToGdkColor (blackboard.TextBackgroundColor);
				backgroundcolorbutton.Alpha = Color.ByteToUShort (blackboard.TextBackgroundColor.A);
				linesizespinbutton.Value = OriginalSize (blackboard.LineWidth);
				if (blackboard.LineStyle == LineStyle.Normal) {
					stylecombobox.Active = 0;
				} else {
					stylecombobox.Active = 1;
				}
				typecombobox.Active = (int)blackboard.LineType;
			} else {
				if (drawable is Text) {
					textcolorbutton.Color = Misc.ToGdkColor ((selectedDrawable as Text).TextColor);
					backgroundcolorbutton.Color = Misc.ToGdkColor (selectedDrawable.FillColor);
					backgroundcolorbutton.Alpha = Color.ByteToUShort (selectedDrawable.FillColor.A);
					textspinbutton.Value = OriginalSize ((selectedDrawable as Text).TextSize);
				} else {
					colorbutton.Color = Misc.ToGdkColor (selectedDrawable.StrokeColor);
				}
				if (drawable is Line) {
					typecombobox.Active = (int)(drawable as Line).Type; 
				}
				linesizespinbutton.Value = OriginalSize (selectedDrawable.LineWidth);
				if (selectedDrawable.Style == LineStyle.Normal) {
					stylecombobox.Active = 0;
				} else {
					stylecombobox.Active = 1;
				}
			}
		}

		void HandleShowMenuEvent (IBlackboardObject drawable)
		{
			Menu m = new Menu ();
			MenuItem item = new MenuItem (Catalog.GetString ("Delete"));
			item.Activated += (sender, e) => {
				blackboard.DeleteSelection ();};
			m.Add (item);
			if (drawable is Text) {
				MenuItem edit = new MenuItem (Catalog.GetString ("Edit"));
				edit.Activated += (sender, e) => {
					EditText (drawable as Text);};
				m.Add (edit);
			}
			m.ShowAll ();
			m.Popup ();
		}

		void HandleDeleteEvent (object o, DeleteEventArgs args)
		{
			string msg = Catalog.GetString ("Do you want to close the current drawing?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				args.RetVal = false;
			} else {
				args.RetVal = true;
			}
		}
	}
}
