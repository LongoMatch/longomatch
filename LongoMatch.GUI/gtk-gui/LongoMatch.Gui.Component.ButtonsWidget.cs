
// This file has been generated by the GUI designer. Do not modify.
namespace LongoMatch.Gui.Component
{
	public partial class ButtonsWidget
	{
		private global::Gtk.HBox hbox2;
		private global::Gtk.VBox vbox2;
		private global::Gtk.HButtonBox hbuttonbox2;
		private global::Gtk.Button addcatbutton;
		private global::Gtk.Button addtimerbutton;
		private global::Gtk.Button addscorebutton;
		private global::Gtk.Button addcardbutton;
		private global::Gtk.Button addtagbutton;
		private global::Gtk.ScrolledWindow scrolledwindow4;
		private global::Gtk.DrawingArea drawingarea;
		private global::Gtk.VBox rightbox;
		private global::Gtk.Frame propertiesframe;
		private global::Gtk.Alignment propertiesalignment;
		private global::Gtk.VBox vbox10;
		private global::Gtk.HBox positionsbox;
		private global::Gtk.VBox fieldvbox;
		private global::Gtk.Frame fieldframe;
		private global::Gtk.Alignment fieldalignment;
		private global::Gtk.EventBox fieldeventbox;
		private global::Gtk.VBox vbox12;
		private global::Gtk.Image fieldimage;
		private global::Gtk.Label fieldlabel1;
		private global::Gtk.Label fieldlabel2;
		private global::Gtk.Button resetfieldbutton;
		private global::Gtk.VBox vbox13;
		private global::Gtk.Frame hfieldframe;
		private global::Gtk.Alignment halffieldalignment;
		private global::Gtk.EventBox hfieldeventbox;
		private global::Gtk.VBox vbox14;
		private global::Gtk.Image hfieldimage;
		private global::Gtk.Label hfieldlabel1;
		private global::Gtk.Label hfieldlabel2;
		private global::Gtk.Button resethfieldbutton;
		private global::Gtk.VBox goalvbox;
		private global::Gtk.Frame goalframe;
		private global::Gtk.Alignment goalalignment;
		private global::Gtk.EventBox goaleventbox;
		private global::Gtk.VBox vbox16;
		private global::Gtk.Image goalimage;
		private global::Gtk.Label goallabel1;
		private global::Gtk.Label goallabel2;
		private global::Gtk.Button resetgoalbutton;
		private global::LongoMatch.Gui.Component.CategoryProperties tagproperties;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget LongoMatch.Gui.Component.ButtonsWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "LongoMatch.Gui.Component.ButtonsWidget";
			// Container child LongoMatch.Gui.Component.ButtonsWidget.Gtk.Container+ContainerChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 12;
			// Container child hbox2.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.CanFocus = true;
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbuttonbox2 = new global::Gtk.HButtonBox ();
			this.hbuttonbox2.Name = "hbuttonbox2";
			this.hbuttonbox2.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(1));
			// Container child hbuttonbox2.Gtk.ButtonBox+ButtonBoxChild
			this.addcatbutton = new global::Gtk.Button ();
			this.addcatbutton.CanFocus = true;
			this.addcatbutton.Name = "addcatbutton";
			this.addcatbutton.UseUnderline = true;
			// Container child addcatbutton.Gtk.Container+ContainerChild
			global::Gtk.Alignment w1 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w2 = new global::Gtk.HBox ();
			w2.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w3 = new global::Gtk.Image ();
			w3.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-add", global::Gtk.IconSize.Menu);
			w2.Add (w3);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w5 = new global::Gtk.Label ();
			w5.LabelProp = global::Mono.Unix.Catalog.GetString ("Add category");
			w5.UseUnderline = true;
			w2.Add (w5);
			w1.Add (w2);
			this.addcatbutton.Add (w1);
			this.hbuttonbox2.Add (this.addcatbutton);
			global::Gtk.ButtonBox.ButtonBoxChild w9 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox2 [this.addcatbutton]));
			w9.Expand = false;
			w9.Fill = false;
			// Container child hbuttonbox2.Gtk.ButtonBox+ButtonBoxChild
			this.addtimerbutton = new global::Gtk.Button ();
			this.addtimerbutton.CanFocus = true;
			this.addtimerbutton.Name = "addtimerbutton";
			this.addtimerbutton.UseUnderline = true;
			// Container child addtimerbutton.Gtk.Container+ContainerChild
			global::Gtk.Alignment w10 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w11 = new global::Gtk.HBox ();
			w11.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w12 = new global::Gtk.Image ();
			w12.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "stock_alarm", global::Gtk.IconSize.Menu);
			w11.Add (w12);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w14 = new global::Gtk.Label ();
			w14.LabelProp = global::Mono.Unix.Catalog.GetString ("Add timer");
			w14.UseUnderline = true;
			w11.Add (w14);
			w10.Add (w11);
			this.addtimerbutton.Add (w10);
			this.hbuttonbox2.Add (this.addtimerbutton);
			global::Gtk.ButtonBox.ButtonBoxChild w18 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox2 [this.addtimerbutton]));
			w18.Position = 1;
			w18.Expand = false;
			w18.Fill = false;
			// Container child hbuttonbox2.Gtk.ButtonBox+ButtonBoxChild
			this.addscorebutton = new global::Gtk.Button ();
			this.addscorebutton.CanFocus = true;
			this.addscorebutton.Name = "addscorebutton";
			this.addscorebutton.UseUnderline = true;
			// Container child addscorebutton.Gtk.Container+ContainerChild
			global::Gtk.Alignment w19 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w20 = new global::Gtk.HBox ();
			w20.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w21 = new global::Gtk.Image ();
			w21.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-zoom-100", global::Gtk.IconSize.Menu);
			w20.Add (w21);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w23 = new global::Gtk.Label ();
			w23.LabelProp = global::Mono.Unix.Catalog.GetString ("Add score");
			w23.UseUnderline = true;
			w20.Add (w23);
			w19.Add (w20);
			this.addscorebutton.Add (w19);
			this.hbuttonbox2.Add (this.addscorebutton);
			global::Gtk.ButtonBox.ButtonBoxChild w27 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox2 [this.addscorebutton]));
			w27.Position = 2;
			w27.Expand = false;
			w27.Fill = false;
			// Container child hbuttonbox2.Gtk.ButtonBox+ButtonBoxChild
			this.addcardbutton = new global::Gtk.Button ();
			this.addcardbutton.CanFocus = true;
			this.addcardbutton.Name = "addcardbutton";
			this.addcardbutton.UseUnderline = true;
			// Container child addcardbutton.Gtk.Container+ContainerChild
			global::Gtk.Alignment w28 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w29 = new global::Gtk.HBox ();
			w29.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w30 = new global::Gtk.Image ();
			w30.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "stock_media-stop", global::Gtk.IconSize.Menu);
			w29.Add (w30);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w32 = new global::Gtk.Label ();
			w32.LabelProp = global::Mono.Unix.Catalog.GetString ("Add penalty card");
			w32.UseUnderline = true;
			w29.Add (w32);
			w28.Add (w29);
			this.addcardbutton.Add (w28);
			this.hbuttonbox2.Add (this.addcardbutton);
			global::Gtk.ButtonBox.ButtonBoxChild w36 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox2 [this.addcardbutton]));
			w36.Position = 3;
			w36.Expand = false;
			w36.Fill = false;
			// Container child hbuttonbox2.Gtk.ButtonBox+ButtonBoxChild
			this.addtagbutton = new global::Gtk.Button ();
			this.addtagbutton.CanFocus = true;
			this.addtagbutton.Name = "addtagbutton";
			this.addtagbutton.UseUnderline = true;
			// Container child addtagbutton.Gtk.Container+ContainerChild
			global::Gtk.Alignment w37 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w38 = new global::Gtk.HBox ();
			w38.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w39 = new global::Gtk.Image ();
			w39.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "stock_zoom-in", global::Gtk.IconSize.Menu);
			w38.Add (w39);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w41 = new global::Gtk.Label ();
			w41.LabelProp = global::Mono.Unix.Catalog.GetString ("Add tag");
			w41.UseUnderline = true;
			w38.Add (w41);
			w37.Add (w38);
			this.addtagbutton.Add (w37);
			this.hbuttonbox2.Add (this.addtagbutton);
			global::Gtk.ButtonBox.ButtonBoxChild w45 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox2 [this.addtagbutton]));
			w45.Position = 4;
			w45.Expand = false;
			w45.Fill = false;
			this.vbox2.Add (this.hbuttonbox2);
			global::Gtk.Box.BoxChild w46 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbuttonbox2]));
			w46.Position = 0;
			w46.Expand = false;
			w46.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.scrolledwindow4 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow4.CanFocus = true;
			this.scrolledwindow4.Name = "scrolledwindow4";
			// Container child scrolledwindow4.Gtk.Container+ContainerChild
			global::Gtk.Viewport w47 = new global::Gtk.Viewport ();
			w47.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.drawingarea = new global::Gtk.DrawingArea ();
			this.drawingarea.CanFocus = true;
			this.drawingarea.Name = "drawingarea";
			w47.Add (this.drawingarea);
			this.scrolledwindow4.Add (w47);
			this.vbox2.Add (this.scrolledwindow4);
			global::Gtk.Box.BoxChild w50 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.scrolledwindow4]));
			w50.Position = 1;
			this.hbox2.Add (this.vbox2);
			global::Gtk.Box.BoxChild w51 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.vbox2]));
			w51.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.rightbox = new global::Gtk.VBox ();
			this.rightbox.Name = "rightbox";
			this.rightbox.Spacing = 6;
			// Container child rightbox.Gtk.Box+BoxChild
			this.propertiesframe = new global::Gtk.Frame ();
			this.propertiesframe.Name = "propertiesframe";
			this.propertiesframe.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child propertiesframe.Gtk.Container+ContainerChild
			this.propertiesalignment = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.propertiesalignment.Name = "propertiesalignment";
			this.propertiesalignment.LeftPadding = ((uint)(6));
			this.propertiesalignment.TopPadding = ((uint)(6));
			this.propertiesalignment.RightPadding = ((uint)(6));
			// Container child propertiesalignment.Gtk.Container+ContainerChild
			this.vbox10 = new global::Gtk.VBox ();
			this.vbox10.Name = "vbox10";
			this.vbox10.Spacing = 6;
			// Container child vbox10.Gtk.Box+BoxChild
			this.positionsbox = new global::Gtk.HBox ();
			this.positionsbox.Name = "positionsbox";
			this.positionsbox.Spacing = 6;
			// Container child positionsbox.Gtk.Box+BoxChild
			this.fieldvbox = new global::Gtk.VBox ();
			this.fieldvbox.Name = "fieldvbox";
			this.fieldvbox.Spacing = 6;
			// Container child fieldvbox.Gtk.Box+BoxChild
			this.fieldframe = new global::Gtk.Frame ();
			this.fieldframe.Name = "fieldframe";
			this.fieldframe.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child fieldframe.Gtk.Container+ContainerChild
			this.fieldalignment = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.fieldalignment.Name = "fieldalignment";
			this.fieldalignment.LeftPadding = ((uint)(6));
			this.fieldalignment.RightPadding = ((uint)(6));
			// Container child fieldalignment.Gtk.Container+ContainerChild
			this.fieldeventbox = new global::Gtk.EventBox ();
			this.fieldeventbox.Name = "fieldeventbox";
			// Container child fieldeventbox.Gtk.Container+ContainerChild
			this.vbox12 = new global::Gtk.VBox ();
			this.vbox12.Name = "vbox12";
			this.vbox12.Spacing = 6;
			// Container child vbox12.Gtk.Box+BoxChild
			this.fieldimage = new global::Gtk.Image ();
			this.fieldimage.WidthRequest = 71;
			this.fieldimage.HeightRequest = 50;
			this.fieldimage.Name = "fieldimage";
			this.vbox12.Add (this.fieldimage);
			global::Gtk.Box.BoxChild w52 = ((global::Gtk.Box.BoxChild)(this.vbox12 [this.fieldimage]));
			w52.Position = 0;
			w52.Expand = false;
			w52.Fill = false;
			// Container child vbox12.Gtk.Box+BoxChild
			this.fieldlabel1 = new global::Gtk.Label ();
			this.fieldlabel1.Name = "fieldlabel1";
			this.fieldlabel1.LabelProp = global::Mono.Unix.Catalog.GetString ("Field");
			this.fieldlabel1.UseMarkup = true;
			this.fieldlabel1.Wrap = true;
			this.fieldlabel1.Justify = ((global::Gtk.Justification)(2));
			this.vbox12.Add (this.fieldlabel1);
			global::Gtk.Box.BoxChild w53 = ((global::Gtk.Box.BoxChild)(this.vbox12 [this.fieldlabel1]));
			w53.Position = 1;
			w53.Expand = false;
			w53.Fill = false;
			// Container child vbox12.Gtk.Box+BoxChild
			this.fieldlabel2 = new global::Gtk.Label ();
			this.fieldlabel2.Name = "fieldlabel2";
			this.fieldlabel2.LabelProp = global::Mono.Unix.Catalog.GetString ("click to add...");
			this.vbox12.Add (this.fieldlabel2);
			global::Gtk.Box.BoxChild w54 = ((global::Gtk.Box.BoxChild)(this.vbox12 [this.fieldlabel2]));
			w54.Position = 2;
			w54.Expand = false;
			w54.Fill = false;
			this.fieldeventbox.Add (this.vbox12);
			this.fieldalignment.Add (this.fieldeventbox);
			this.fieldframe.Add (this.fieldalignment);
			this.fieldvbox.Add (this.fieldframe);
			global::Gtk.Box.BoxChild w58 = ((global::Gtk.Box.BoxChild)(this.fieldvbox [this.fieldframe]));
			w58.Position = 0;
			w58.Expand = false;
			w58.Fill = false;
			// Container child fieldvbox.Gtk.Box+BoxChild
			this.resetfieldbutton = new global::Gtk.Button ();
			this.resetfieldbutton.CanFocus = true;
			this.resetfieldbutton.Name = "resetfieldbutton";
			this.resetfieldbutton.UseUnderline = true;
			// Container child resetfieldbutton.Gtk.Container+ContainerChild
			global::Gtk.Alignment w59 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w60 = new global::Gtk.HBox ();
			w60.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w61 = new global::Gtk.Image ();
			w61.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-refresh", global::Gtk.IconSize.Menu);
			w60.Add (w61);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w63 = new global::Gtk.Label ();
			w60.Add (w63);
			w59.Add (w60);
			this.resetfieldbutton.Add (w59);
			this.fieldvbox.Add (this.resetfieldbutton);
			global::Gtk.Box.BoxChild w67 = ((global::Gtk.Box.BoxChild)(this.fieldvbox [this.resetfieldbutton]));
			w67.Position = 1;
			w67.Expand = false;
			w67.Fill = false;
			this.positionsbox.Add (this.fieldvbox);
			global::Gtk.Box.BoxChild w68 = ((global::Gtk.Box.BoxChild)(this.positionsbox [this.fieldvbox]));
			w68.Position = 0;
			w68.Expand = false;
			w68.Fill = false;
			// Container child positionsbox.Gtk.Box+BoxChild
			this.vbox13 = new global::Gtk.VBox ();
			this.vbox13.Name = "vbox13";
			this.vbox13.Spacing = 6;
			// Container child vbox13.Gtk.Box+BoxChild
			this.hfieldframe = new global::Gtk.Frame ();
			this.hfieldframe.Name = "hfieldframe";
			this.hfieldframe.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child hfieldframe.Gtk.Container+ContainerChild
			this.halffieldalignment = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.halffieldalignment.Name = "halffieldalignment";
			this.halffieldalignment.LeftPadding = ((uint)(6));
			this.halffieldalignment.RightPadding = ((uint)(6));
			// Container child halffieldalignment.Gtk.Container+ContainerChild
			this.hfieldeventbox = new global::Gtk.EventBox ();
			this.hfieldeventbox.Name = "hfieldeventbox";
			// Container child hfieldeventbox.Gtk.Container+ContainerChild
			this.vbox14 = new global::Gtk.VBox ();
			this.vbox14.Name = "vbox14";
			this.vbox14.Spacing = 6;
			// Container child vbox14.Gtk.Box+BoxChild
			this.hfieldimage = new global::Gtk.Image ();
			this.hfieldimage.WidthRequest = 71;
			this.hfieldimage.HeightRequest = 50;
			this.hfieldimage.Name = "hfieldimage";
			this.vbox14.Add (this.hfieldimage);
			global::Gtk.Box.BoxChild w69 = ((global::Gtk.Box.BoxChild)(this.vbox14 [this.hfieldimage]));
			w69.Position = 0;
			w69.Expand = false;
			w69.Fill = false;
			// Container child vbox14.Gtk.Box+BoxChild
			this.hfieldlabel1 = new global::Gtk.Label ();
			this.hfieldlabel1.Name = "hfieldlabel1";
			this.hfieldlabel1.LabelProp = global::Mono.Unix.Catalog.GetString ("Half field");
			this.vbox14.Add (this.hfieldlabel1);
			global::Gtk.Box.BoxChild w70 = ((global::Gtk.Box.BoxChild)(this.vbox14 [this.hfieldlabel1]));
			w70.Position = 1;
			w70.Expand = false;
			w70.Fill = false;
			// Container child vbox14.Gtk.Box+BoxChild
			this.hfieldlabel2 = new global::Gtk.Label ();
			this.hfieldlabel2.Name = "hfieldlabel2";
			this.hfieldlabel2.LabelProp = global::Mono.Unix.Catalog.GetString ("click to add...");
			this.vbox14.Add (this.hfieldlabel2);
			global::Gtk.Box.BoxChild w71 = ((global::Gtk.Box.BoxChild)(this.vbox14 [this.hfieldlabel2]));
			w71.Position = 2;
			w71.Expand = false;
			w71.Fill = false;
			this.hfieldeventbox.Add (this.vbox14);
			this.halffieldalignment.Add (this.hfieldeventbox);
			this.hfieldframe.Add (this.halffieldalignment);
			this.vbox13.Add (this.hfieldframe);
			global::Gtk.Box.BoxChild w75 = ((global::Gtk.Box.BoxChild)(this.vbox13 [this.hfieldframe]));
			w75.Position = 0;
			w75.Expand = false;
			w75.Fill = false;
			// Container child vbox13.Gtk.Box+BoxChild
			this.resethfieldbutton = new global::Gtk.Button ();
			this.resethfieldbutton.CanFocus = true;
			this.resethfieldbutton.Name = "resethfieldbutton";
			this.resethfieldbutton.UseUnderline = true;
			// Container child resethfieldbutton.Gtk.Container+ContainerChild
			global::Gtk.Alignment w76 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w77 = new global::Gtk.HBox ();
			w77.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w78 = new global::Gtk.Image ();
			w78.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-refresh", global::Gtk.IconSize.Menu);
			w77.Add (w78);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w80 = new global::Gtk.Label ();
			w77.Add (w80);
			w76.Add (w77);
			this.resethfieldbutton.Add (w76);
			this.vbox13.Add (this.resethfieldbutton);
			global::Gtk.Box.BoxChild w84 = ((global::Gtk.Box.BoxChild)(this.vbox13 [this.resethfieldbutton]));
			w84.Position = 1;
			w84.Expand = false;
			w84.Fill = false;
			this.positionsbox.Add (this.vbox13);
			global::Gtk.Box.BoxChild w85 = ((global::Gtk.Box.BoxChild)(this.positionsbox [this.vbox13]));
			w85.Position = 1;
			w85.Expand = false;
			w85.Fill = false;
			// Container child positionsbox.Gtk.Box+BoxChild
			this.goalvbox = new global::Gtk.VBox ();
			this.goalvbox.Name = "goalvbox";
			this.goalvbox.Spacing = 6;
			// Container child goalvbox.Gtk.Box+BoxChild
			this.goalframe = new global::Gtk.Frame ();
			this.goalframe.Name = "goalframe";
			this.goalframe.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child goalframe.Gtk.Container+ContainerChild
			this.goalalignment = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.goalalignment.Name = "goalalignment";
			this.goalalignment.LeftPadding = ((uint)(6));
			this.goalalignment.RightPadding = ((uint)(6));
			// Container child goalalignment.Gtk.Container+ContainerChild
			this.goaleventbox = new global::Gtk.EventBox ();
			this.goaleventbox.Name = "goaleventbox";
			// Container child goaleventbox.Gtk.Container+ContainerChild
			this.vbox16 = new global::Gtk.VBox ();
			this.vbox16.Name = "vbox16";
			this.vbox16.Spacing = 6;
			// Container child vbox16.Gtk.Box+BoxChild
			this.goalimage = new global::Gtk.Image ();
			this.goalimage.WidthRequest = 71;
			this.goalimage.HeightRequest = 50;
			this.goalimage.Name = "goalimage";
			this.vbox16.Add (this.goalimage);
			global::Gtk.Box.BoxChild w86 = ((global::Gtk.Box.BoxChild)(this.vbox16 [this.goalimage]));
			w86.Position = 0;
			w86.Expand = false;
			w86.Fill = false;
			// Container child vbox16.Gtk.Box+BoxChild
			this.goallabel1 = new global::Gtk.Label ();
			this.goallabel1.Name = "goallabel1";
			this.goallabel1.LabelProp = global::Mono.Unix.Catalog.GetString ("Goal");
			this.vbox16.Add (this.goallabel1);
			global::Gtk.Box.BoxChild w87 = ((global::Gtk.Box.BoxChild)(this.vbox16 [this.goallabel1]));
			w87.Position = 1;
			w87.Expand = false;
			w87.Fill = false;
			// Container child vbox16.Gtk.Box+BoxChild
			this.goallabel2 = new global::Gtk.Label ();
			this.goallabel2.Name = "goallabel2";
			this.goallabel2.LabelProp = global::Mono.Unix.Catalog.GetString ("click to add...");
			this.vbox16.Add (this.goallabel2);
			global::Gtk.Box.BoxChild w88 = ((global::Gtk.Box.BoxChild)(this.vbox16 [this.goallabel2]));
			w88.Position = 2;
			w88.Expand = false;
			w88.Fill = false;
			this.goaleventbox.Add (this.vbox16);
			this.goalalignment.Add (this.goaleventbox);
			this.goalframe.Add (this.goalalignment);
			this.goalvbox.Add (this.goalframe);
			global::Gtk.Box.BoxChild w92 = ((global::Gtk.Box.BoxChild)(this.goalvbox [this.goalframe]));
			w92.Position = 0;
			w92.Expand = false;
			w92.Fill = false;
			// Container child goalvbox.Gtk.Box+BoxChild
			this.resetgoalbutton = new global::Gtk.Button ();
			this.resetgoalbutton.CanFocus = true;
			this.resetgoalbutton.Name = "resetgoalbutton";
			this.resetgoalbutton.UseUnderline = true;
			// Container child resetgoalbutton.Gtk.Container+ContainerChild
			global::Gtk.Alignment w93 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w94 = new global::Gtk.HBox ();
			w94.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w95 = new global::Gtk.Image ();
			w95.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-refresh", global::Gtk.IconSize.Menu);
			w94.Add (w95);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w97 = new global::Gtk.Label ();
			w94.Add (w97);
			w93.Add (w94);
			this.resetgoalbutton.Add (w93);
			this.goalvbox.Add (this.resetgoalbutton);
			global::Gtk.Box.BoxChild w101 = ((global::Gtk.Box.BoxChild)(this.goalvbox [this.resetgoalbutton]));
			w101.Position = 1;
			w101.Expand = false;
			w101.Fill = false;
			this.positionsbox.Add (this.goalvbox);
			global::Gtk.Box.BoxChild w102 = ((global::Gtk.Box.BoxChild)(this.positionsbox [this.goalvbox]));
			w102.Position = 2;
			w102.Expand = false;
			w102.Fill = false;
			this.vbox10.Add (this.positionsbox);
			global::Gtk.Box.BoxChild w103 = ((global::Gtk.Box.BoxChild)(this.vbox10 [this.positionsbox]));
			w103.Position = 0;
			w103.Fill = false;
			// Container child vbox10.Gtk.Box+BoxChild
			this.tagproperties = new global::LongoMatch.Gui.Component.CategoryProperties ();
			this.tagproperties.Events = ((global::Gdk.EventMask)(256));
			this.tagproperties.Name = "tagproperties";
			this.tagproperties.Edited = false;
			this.vbox10.Add (this.tagproperties);
			global::Gtk.Box.BoxChild w104 = ((global::Gtk.Box.BoxChild)(this.vbox10 [this.tagproperties]));
			w104.Position = 1;
			this.propertiesalignment.Add (this.vbox10);
			this.propertiesframe.Add (this.propertiesalignment);
			this.rightbox.Add (this.propertiesframe);
			global::Gtk.Box.BoxChild w107 = ((global::Gtk.Box.BoxChild)(this.rightbox [this.propertiesframe]));
			w107.Position = 0;
			w107.Expand = false;
			w107.Fill = false;
			this.hbox2.Add (this.rightbox);
			global::Gtk.Box.BoxChild w108 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.rightbox]));
			w108.Position = 1;
			w108.Expand = false;
			w108.Fill = false;
			this.Add (this.hbox2);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Show ();
		}
	}
}
