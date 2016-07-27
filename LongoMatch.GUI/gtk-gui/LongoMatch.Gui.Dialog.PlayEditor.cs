
// This file has been generated by the GUI designer. Do not modify.
namespace LongoMatch.Gui.Dialog
{
	public partial class PlayEditor
	{
		private global::Gtk.ScrolledWindow scrolledwindow2;
		
		private global::Gtk.VBox vbox3;
		
		private global::Gtk.HBox hbox4;
		
		private global::Gtk.Frame nameframe;
		
		private global::Gtk.Alignment GtkAlignment3;
		
		private global::Gtk.HBox hbox3;
		
		private global::Gtk.Entry nameentry;
		
		private global::Gtk.Label GtkLabel3;
		
		private global::Gtk.Frame notesframe;
		
		private global::Gtk.Alignment GtkAlignment;
		
		private global::LongoMatch.Gui.Component.NotesWidget notes;
		
		private global::Gtk.Label GtkLabel1;
		
		private global::Gtk.VBox tagsvbox;
		
		private global::LongoMatch.Gui.Component.PlaysCoordinatesTagger tagger;
		
		private global::Gtk.DrawingArea drawingarea3;
		
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget LongoMatch.Gui.Dialog.PlayEditor
			this.Name = "LongoMatch.Gui.Dialog.PlayEditor";
			this.Title = global::VAS.Core.Catalog.GetString ("Edit event details");
			this.Icon = global::Stetic.IconLoader.LoadIcon (this, "longomatch", global::Gtk.IconSize.Menu);
			this.TypeHint = ((global::Gdk.WindowTypeHint)(1));
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.Modal = true;
			this.Gravity = ((global::Gdk.Gravity)(5));
			this.SkipPagerHint = true;
			this.SkipTaskbarHint = true;
			// Internal child LongoMatch.Gui.Dialog.PlayEditor.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.scrolledwindow2 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow2.CanFocus = true;
			this.scrolledwindow2.Name = "scrolledwindow2";
			this.scrolledwindow2.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow2.Gtk.Container+ContainerChild
			global::Gtk.Viewport w2 = new global::Gtk.Viewport ();
			w2.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport1.Gtk.Container+ContainerChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox4 = new global::Gtk.HBox ();
			this.hbox4.HeightRequest = 80;
			this.hbox4.Name = "hbox4";
			this.hbox4.Spacing = 6;
			// Container child hbox4.Gtk.Box+BoxChild
			this.nameframe = new global::Gtk.Frame ();
			this.nameframe.WidthRequest = 200;
			this.nameframe.Name = "nameframe";
			this.nameframe.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child nameframe.Gtk.Container+ContainerChild
			this.GtkAlignment3 = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.GtkAlignment3.Name = "GtkAlignment3";
			this.GtkAlignment3.LeftPadding = ((uint)(12));
			// Container child GtkAlignment3.Gtk.Container+ContainerChild
			this.hbox3 = new global::Gtk.HBox ();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.nameentry = new global::Gtk.Entry ();
			this.nameentry.CanFocus = true;
			this.nameentry.Name = "nameentry";
			this.nameentry.IsEditable = true;
			this.nameentry.InvisibleChar = '•';
			this.hbox3.Add (this.nameentry);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.nameentry]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			this.GtkAlignment3.Add (this.hbox3);
			this.nameframe.Add (this.GtkAlignment3);
			this.GtkLabel3 = new global::Gtk.Label ();
			this.GtkLabel3.Name = "GtkLabel3";
			this.GtkLabel3.LabelProp = global::VAS.Core.Catalog.GetString ("<b>Name</b>");
			this.GtkLabel3.UseMarkup = true;
			this.nameframe.LabelWidget = this.GtkLabel3;
			this.hbox4.Add (this.nameframe);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.nameframe]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child hbox4.Gtk.Box+BoxChild
			this.notesframe = new global::Gtk.Frame ();
			this.notesframe.Name = "notesframe";
			this.notesframe.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child notesframe.Gtk.Container+ContainerChild
			this.GtkAlignment = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.GtkAlignment.Name = "GtkAlignment";
			this.GtkAlignment.LeftPadding = ((uint)(12));
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			this.notes = new global::LongoMatch.Gui.Component.NotesWidget ();
			this.notes.HeightRequest = 100;
			this.notes.Events = ((global::Gdk.EventMask)(256));
			this.notes.Name = "notes";
			this.GtkAlignment.Add (this.notes);
			this.notesframe.Add (this.GtkAlignment);
			this.GtkLabel1 = new global::Gtk.Label ();
			this.GtkLabel1.Name = "GtkLabel1";
			this.GtkLabel1.LabelProp = global::VAS.Core.Catalog.GetString ("<b>Notes</b>");
			this.GtkLabel1.UseMarkup = true;
			this.notesframe.LabelWidget = this.GtkLabel1;
			this.hbox4.Add (this.notesframe);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox4 [this.notesframe]));
			w9.Position = 1;
			this.vbox3.Add (this.hbox4);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.hbox4]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.tagsvbox = new global::Gtk.VBox ();
			this.tagsvbox.Name = "tagsvbox";
			this.tagsvbox.Spacing = 6;
			this.vbox3.Add (this.tagsvbox);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.tagsvbox]));
			w11.Position = 1;
			// Container child vbox3.Gtk.Box+BoxChild
			this.tagger = new global::LongoMatch.Gui.Component.PlaysCoordinatesTagger ();
			this.tagger.HeightRequest = 200;
			this.tagger.Events = ((global::Gdk.EventMask)(256));
			this.tagger.Name = "tagger";
			this.vbox3.Add (this.tagger);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.tagger]));
			w12.Position = 2;
			// Container child vbox3.Gtk.Box+BoxChild
			this.drawingarea3 = new global::Gtk.DrawingArea ();
			this.drawingarea3.HeightRequest = 200;
			this.drawingarea3.Name = "drawingarea3";
			this.vbox3.Add (this.drawingarea3);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.drawingarea3]));
			w13.Position = 3;
			w2.Add (this.vbox3);
			this.scrolledwindow2.Add (w2);
			w1.Add (this.scrolledwindow2);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(w1 [this.scrolledwindow2]));
			w16.Position = 0;
			// Internal child LongoMatch.Gui.Dialog.PlayEditor.ActionArea
			global::Gtk.HButtonBox w17 = this.ActionArea;
			w17.Name = "dialog1_ActionArea";
			w17.Spacing = 10;
			w17.BorderWidth = ((uint)(5));
			w17.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w18 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w17 [this.buttonOk]));
			w18.Expand = false;
			w18.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 1021;
			this.DefaultHeight = 671;
			this.Show ();
		}
	}
}
