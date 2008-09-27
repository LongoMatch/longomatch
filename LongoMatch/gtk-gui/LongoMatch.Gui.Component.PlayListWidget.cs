// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace LongoMatch.Gui.Component {
    
    
    public partial class PlayListWidget {
        
        private Gtk.VBox vbox2;
        
        private Gtk.ScrolledWindow scrolledwindow1;
        
        private Gtk.VBox vbox1;
        
        private Gtk.Label label1;
        
        private LongoMatch.Gui.Component.PlayListTreeView playlisttreeview1;
        
        private Gtk.HBox hbox2;
        
        private Gtk.Button newbutton;
        
        private Gtk.Button openbutton;
        
        private Gtk.Button savebutton;
        
        private Gtk.Button newvideobutton;
        
        private Gtk.Button closebutton;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget LongoMatch.Gui.Component.PlayListWidget
            Stetic.BinContainer.Attach(this);
            this.WidthRequest = 100;
            this.Name = "LongoMatch.Gui.Component.PlayListWidget";
            // Container child LongoMatch.Gui.Component.PlayListWidget.Gtk.Container+ContainerChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            // Container child vbox2.Gtk.Box+BoxChild
            this.scrolledwindow1 = new Gtk.ScrolledWindow();
            this.scrolledwindow1.CanFocus = true;
            this.scrolledwindow1.Name = "scrolledwindow1";
            this.scrolledwindow1.ShadowType = ((Gtk.ShadowType)(1));
            // Container child scrolledwindow1.Gtk.Container+ContainerChild
            Gtk.Viewport w1 = new Gtk.Viewport();
            w1.ShadowType = ((Gtk.ShadowType)(0));
            // Container child GtkViewport.Gtk.Container+ContainerChild
            this.vbox1 = new Gtk.VBox();
            this.vbox1.Name = "vbox1";
            this.vbox1.Spacing = 6;
            // Container child vbox1.Gtk.Box+BoxChild
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("Load a playlist or create a new one.");
            this.vbox1.Add(this.label1);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.vbox1[this.label1]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.playlisttreeview1 = new LongoMatch.Gui.Component.PlayListTreeView();
            this.playlisttreeview1.Sensitive = false;
            this.playlisttreeview1.CanFocus = true;
            this.playlisttreeview1.Name = "playlisttreeview1";
            this.playlisttreeview1.Reorderable = true;
            this.playlisttreeview1.HeadersClickable = true;
            this.vbox1.Add(this.playlisttreeview1);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.vbox1[this.playlisttreeview1]));
            w3.Position = 1;
            w1.Add(this.vbox1);
            this.scrolledwindow1.Add(w1);
            this.vbox2.Add(this.scrolledwindow1);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(this.vbox2[this.scrolledwindow1]));
            w6.Position = 0;
            // Container child vbox2.Gtk.Box+BoxChild
            this.hbox2 = new Gtk.HBox();
            this.hbox2.Name = "hbox2";
            this.hbox2.Homogeneous = true;
            this.hbox2.Spacing = 6;
            // Container child hbox2.Gtk.Box+BoxChild
            this.newbutton = new Gtk.Button();
            this.newbutton.CanFocus = true;
            this.newbutton.Name = "newbutton";
            this.newbutton.UseUnderline = true;
            // Container child newbutton.Gtk.Container+ContainerChild
            Gtk.Alignment w7 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w8 = new Gtk.HBox();
            w8.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w9 = new Gtk.Image();
            w9.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-new", Gtk.IconSize.Button, 20);
            w8.Add(w9);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w11 = new Gtk.Label();
            w11.LabelProp = "";
            w8.Add(w11);
            w7.Add(w8);
            this.newbutton.Add(w7);
            this.hbox2.Add(this.newbutton);
            Gtk.Box.BoxChild w15 = ((Gtk.Box.BoxChild)(this.hbox2[this.newbutton]));
            w15.Position = 0;
            // Container child hbox2.Gtk.Box+BoxChild
            this.openbutton = new Gtk.Button();
            this.openbutton.CanFocus = true;
            this.openbutton.Name = "openbutton";
            this.openbutton.UseUnderline = true;
            // Container child openbutton.Gtk.Container+ContainerChild
            Gtk.Alignment w16 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w17 = new Gtk.HBox();
            w17.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w18 = new Gtk.Image();
            w18.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-open", Gtk.IconSize.Button, 20);
            w17.Add(w18);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w20 = new Gtk.Label();
            w20.LabelProp = "";
            w17.Add(w20);
            w16.Add(w17);
            this.openbutton.Add(w16);
            this.hbox2.Add(this.openbutton);
            Gtk.Box.BoxChild w24 = ((Gtk.Box.BoxChild)(this.hbox2[this.openbutton]));
            w24.Position = 1;
            // Container child hbox2.Gtk.Box+BoxChild
            this.savebutton = new Gtk.Button();
            this.savebutton.CanFocus = true;
            this.savebutton.Name = "savebutton";
            this.savebutton.UseUnderline = true;
            // Container child savebutton.Gtk.Container+ContainerChild
            Gtk.Alignment w25 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w26 = new Gtk.HBox();
            w26.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w27 = new Gtk.Image();
            w27.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-save", Gtk.IconSize.Button, 20);
            w26.Add(w27);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w29 = new Gtk.Label();
            w29.LabelProp = "";
            w26.Add(w29);
            w25.Add(w26);
            this.savebutton.Add(w25);
            this.hbox2.Add(this.savebutton);
            Gtk.Box.BoxChild w33 = ((Gtk.Box.BoxChild)(this.hbox2[this.savebutton]));
            w33.Position = 2;
            // Container child hbox2.Gtk.Box+BoxChild
            this.newvideobutton = new Gtk.Button();
            this.newvideobutton.CanFocus = true;
            this.newvideobutton.Name = "newvideobutton";
            this.newvideobutton.UseUnderline = true;
            // Container child newvideobutton.Gtk.Container+ContainerChild
            Gtk.Alignment w34 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w35 = new Gtk.HBox();
            w35.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w36 = new Gtk.Image();
            w36.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-media-record", Gtk.IconSize.Button, 20);
            w35.Add(w36);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w38 = new Gtk.Label();
            w38.LabelProp = "";
            w35.Add(w38);
            w34.Add(w35);
            this.newvideobutton.Add(w34);
            this.hbox2.Add(this.newvideobutton);
            Gtk.Box.BoxChild w42 = ((Gtk.Box.BoxChild)(this.hbox2[this.newvideobutton]));
            w42.Position = 3;
            // Container child hbox2.Gtk.Box+BoxChild
            this.closebutton = new Gtk.Button();
            this.closebutton.CanFocus = true;
            this.closebutton.Name = "closebutton";
            this.closebutton.UseUnderline = true;
            // Container child closebutton.Gtk.Container+ContainerChild
            Gtk.Alignment w43 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w44 = new Gtk.HBox();
            w44.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w45 = new Gtk.Image();
            w45.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-close", Gtk.IconSize.Button, 20);
            w44.Add(w45);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w47 = new Gtk.Label();
            w47.LabelProp = "";
            w44.Add(w47);
            w43.Add(w44);
            this.closebutton.Add(w43);
            this.hbox2.Add(this.closebutton);
            Gtk.Box.BoxChild w51 = ((Gtk.Box.BoxChild)(this.hbox2[this.closebutton]));
            w51.Position = 4;
            this.vbox2.Add(this.hbox2);
            Gtk.Box.BoxChild w52 = ((Gtk.Box.BoxChild)(this.vbox2[this.hbox2]));
            w52.Position = 1;
            w52.Expand = false;
            w52.Fill = false;
            this.Add(this.vbox2);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.closebutton.Hide();
            this.Show();
            this.playlisttreeview1.RowActivated += new Gtk.RowActivatedHandler(this.OnPlaylisttreeview1RowActivated);
            this.playlisttreeview1.DragEnd += new Gtk.DragEndHandler(this.OnPlaylisttreeview1DragEnd);
            this.newbutton.Clicked += new System.EventHandler(this.OnNewbuttonClicked);
            this.openbutton.Clicked += new System.EventHandler(this.OnOpenbuttonClicked);
            this.savebutton.Clicked += new System.EventHandler(this.OnSavebuttonClicked);
            this.newvideobutton.Clicked += new System.EventHandler(this.OnNewvideobuttonClicked);
            this.closebutton.Clicked += new System.EventHandler(this.OnClosebuttonClicked);
        }
    }
}
