// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace LongoMatch.Gui.Component {
    
    
    public partial class TimeNodeProperties {
        
        private Gtk.Frame frame1;
        
        private Gtk.Alignment GtkAlignment;
        
        private Gtk.HBox hbox1;
        
        private Gtk.VBox vbox2;
        
        private Gtk.Button newleftbutton;
        
        private Gtk.Button newleftbutton1;
        
        private Gtk.Button deletebutton;
        
        private Gtk.VBox vbox3;
        
        private Gtk.HBox hbox4;
        
        private Gtk.Label label1;
        
        private Gtk.Entry nameentry;
        
        private LongoMatch.Gui.Component.TimeAdjustWidget timeadjustwidget1;
        
        private Gtk.HBox hbox2;
        
        private Gtk.Label label4;
        
        private Gtk.ColorButton colorbutton1;
        
        private Gtk.Button changebuton;
        
        private Gtk.Label hotKeyLabel;
        
        private Gtk.Label label6;
        
        private Gtk.Label titlelabel;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget LongoMatch.Gui.Component.TimeNodeProperties
            Stetic.BinContainer.Attach(this);
            this.Name = "LongoMatch.Gui.Component.TimeNodeProperties";
            // Container child LongoMatch.Gui.Component.TimeNodeProperties.Gtk.Container+ContainerChild
            this.frame1 = new Gtk.Frame();
            this.frame1.Name = "frame1";
            this.frame1.ShadowType = ((Gtk.ShadowType)(0));
            // Container child frame1.Gtk.Container+ContainerChild
            this.GtkAlignment = new Gtk.Alignment(0F, 0F, 1F, 1F);
            this.GtkAlignment.Name = "GtkAlignment";
            this.GtkAlignment.LeftPadding = ((uint)(12));
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            this.hbox1 = new Gtk.HBox();
            this.hbox1.Name = "hbox1";
            this.hbox1.Spacing = 6;
            // Container child hbox1.Gtk.Box+BoxChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            // Container child vbox2.Gtk.Box+BoxChild
            this.newleftbutton = new Gtk.Button();
            this.newleftbutton.TooltipMarkup = "Insert After";
            this.newleftbutton.CanFocus = true;
            this.newleftbutton.Name = "newleftbutton";
            this.newleftbutton.UseUnderline = true;
            // Container child newleftbutton.Gtk.Container+ContainerChild
            Gtk.Alignment w1 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w2 = new Gtk.HBox();
            w2.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w3 = new Gtk.Image();
            w3.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-goto-last", Gtk.IconSize.Menu, 16);
            w2.Add(w3);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w5 = new Gtk.Label();
            w2.Add(w5);
            w1.Add(w2);
            this.newleftbutton.Add(w1);
            this.vbox2.Add(this.newleftbutton);
            Gtk.Box.BoxChild w9 = ((Gtk.Box.BoxChild)(this.vbox2[this.newleftbutton]));
            w9.Position = 0;
            w9.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.newleftbutton1 = new Gtk.Button();
            this.newleftbutton1.TooltipMarkup = "InsertBefore";
            this.newleftbutton1.CanFocus = true;
            this.newleftbutton1.Name = "newleftbutton1";
            this.newleftbutton1.UseUnderline = true;
            // Container child newleftbutton1.Gtk.Container+ContainerChild
            Gtk.Alignment w10 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w11 = new Gtk.HBox();
            w11.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w12 = new Gtk.Image();
            w12.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-goto-first", Gtk.IconSize.Menu, 16);
            w11.Add(w12);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w14 = new Gtk.Label();
            w11.Add(w14);
            w10.Add(w11);
            this.newleftbutton1.Add(w10);
            this.vbox2.Add(this.newleftbutton1);
            Gtk.Box.BoxChild w18 = ((Gtk.Box.BoxChild)(this.vbox2[this.newleftbutton1]));
            w18.Position = 1;
            w18.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.deletebutton = new Gtk.Button();
            this.deletebutton.TooltipMarkup = "Delete";
            this.deletebutton.CanFocus = true;
            this.deletebutton.Name = "deletebutton";
            this.deletebutton.UseUnderline = true;
            // Container child deletebutton.Gtk.Container+ContainerChild
            Gtk.Alignment w19 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w20 = new Gtk.HBox();
            w20.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w21 = new Gtk.Image();
            w21.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", Gtk.IconSize.Menu, 16);
            w20.Add(w21);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w23 = new Gtk.Label();
            w20.Add(w23);
            w19.Add(w20);
            this.deletebutton.Add(w19);
            this.vbox2.Add(this.deletebutton);
            Gtk.Box.BoxChild w27 = ((Gtk.Box.BoxChild)(this.vbox2[this.deletebutton]));
            w27.Position = 2;
            w27.Expand = false;
            w27.Fill = false;
            this.hbox1.Add(this.vbox2);
            Gtk.Box.BoxChild w28 = ((Gtk.Box.BoxChild)(this.hbox1[this.vbox2]));
            w28.Position = 0;
            w28.Expand = false;
            w28.Fill = false;
            // Container child hbox1.Gtk.Box+BoxChild
            this.vbox3 = new Gtk.VBox();
            this.vbox3.Name = "vbox3";
            this.vbox3.Spacing = 6;
            // Container child vbox3.Gtk.Box+BoxChild
            this.hbox4 = new Gtk.HBox();
            this.hbox4.Name = "hbox4";
            this.hbox4.Spacing = 6;
            // Container child hbox4.Gtk.Box+BoxChild
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("Name:");
            this.hbox4.Add(this.label1);
            Gtk.Box.BoxChild w29 = ((Gtk.Box.BoxChild)(this.hbox4[this.label1]));
            w29.Position = 0;
            w29.Expand = false;
            w29.Fill = false;
            // Container child hbox4.Gtk.Box+BoxChild
            this.nameentry = new Gtk.Entry();
            this.nameentry.CanFocus = true;
            this.nameentry.Name = "nameentry";
            this.nameentry.IsEditable = true;
            this.nameentry.InvisibleChar = '●';
            this.hbox4.Add(this.nameentry);
            Gtk.Box.BoxChild w30 = ((Gtk.Box.BoxChild)(this.hbox4[this.nameentry]));
            w30.Position = 1;
            this.vbox3.Add(this.hbox4);
            Gtk.Box.BoxChild w31 = ((Gtk.Box.BoxChild)(this.vbox3[this.hbox4]));
            w31.Position = 0;
            w31.Fill = false;
            // Container child vbox3.Gtk.Box+BoxChild
            this.timeadjustwidget1 = new LongoMatch.Gui.Component.TimeAdjustWidget();
            this.timeadjustwidget1.Events = ((Gdk.EventMask)(256));
            this.timeadjustwidget1.Name = "timeadjustwidget1";
            this.vbox3.Add(this.timeadjustwidget1);
            Gtk.Box.BoxChild w32 = ((Gtk.Box.BoxChild)(this.vbox3[this.timeadjustwidget1]));
            w32.Position = 1;
            w32.Fill = false;
            // Container child vbox3.Gtk.Box+BoxChild
            this.hbox2 = new Gtk.HBox();
            this.hbox2.Name = "hbox2";
            this.hbox2.Spacing = 6;
            // Container child hbox2.Gtk.Box+BoxChild
            this.label4 = new Gtk.Label();
            this.label4.Name = "label4";
            this.label4.LabelProp = Mono.Unix.Catalog.GetString("Color:        ");
            this.hbox2.Add(this.label4);
            Gtk.Box.BoxChild w33 = ((Gtk.Box.BoxChild)(this.hbox2[this.label4]));
            w33.Position = 0;
            w33.Expand = false;
            w33.Fill = false;
            // Container child hbox2.Gtk.Box+BoxChild
            this.colorbutton1 = new Gtk.ColorButton();
            this.colorbutton1.CanFocus = true;
            this.colorbutton1.Events = ((Gdk.EventMask)(784));
            this.colorbutton1.Name = "colorbutton1";
            this.hbox2.Add(this.colorbutton1);
            Gtk.Box.BoxChild w34 = ((Gtk.Box.BoxChild)(this.hbox2[this.colorbutton1]));
            w34.Position = 1;
            w34.Expand = false;
            w34.Fill = false;
            // Container child hbox2.Gtk.Box+BoxChild
            this.changebuton = new Gtk.Button();
            this.changebuton.CanFocus = true;
            this.changebuton.Name = "changebuton";
            this.changebuton.UseUnderline = true;
            this.changebuton.Label = Mono.Unix.Catalog.GetString("Change");
            this.hbox2.Add(this.changebuton);
            Gtk.Box.BoxChild w35 = ((Gtk.Box.BoxChild)(this.hbox2[this.changebuton]));
            w35.PackType = ((Gtk.PackType)(1));
            w35.Position = 2;
            w35.Expand = false;
            w35.Fill = false;
            // Container child hbox2.Gtk.Box+BoxChild
            this.hotKeyLabel = new Gtk.Label();
            this.hotKeyLabel.Name = "hotKeyLabel";
            this.hotKeyLabel.LabelProp = Mono.Unix.Catalog.GetString("none");
            this.hbox2.Add(this.hotKeyLabel);
            Gtk.Box.BoxChild w36 = ((Gtk.Box.BoxChild)(this.hbox2[this.hotKeyLabel]));
            w36.PackType = ((Gtk.PackType)(1));
            w36.Position = 3;
            w36.Expand = false;
            w36.Fill = false;
            // Container child hbox2.Gtk.Box+BoxChild
            this.label6 = new Gtk.Label();
            this.label6.Name = "label6";
            this.label6.LabelProp = Mono.Unix.Catalog.GetString("HotKey:");
            this.hbox2.Add(this.label6);
            Gtk.Box.BoxChild w37 = ((Gtk.Box.BoxChild)(this.hbox2[this.label6]));
            w37.PackType = ((Gtk.PackType)(1));
            w37.Position = 4;
            w37.Expand = false;
            w37.Fill = false;
            this.vbox3.Add(this.hbox2);
            Gtk.Box.BoxChild w38 = ((Gtk.Box.BoxChild)(this.vbox3[this.hbox2]));
            w38.Position = 2;
            w38.Fill = false;
            this.hbox1.Add(this.vbox3);
            Gtk.Box.BoxChild w39 = ((Gtk.Box.BoxChild)(this.hbox1[this.vbox3]));
            w39.Position = 1;
            w39.Expand = false;
            w39.Fill = false;
            this.GtkAlignment.Add(this.hbox1);
            this.frame1.Add(this.GtkAlignment);
            this.titlelabel = new Gtk.Label();
            this.titlelabel.Name = "titlelabel";
            this.titlelabel.LabelProp = Mono.Unix.Catalog.GetString("<b>frame1</b>");
            this.titlelabel.UseMarkup = true;
            this.frame1.LabelWidget = this.titlelabel;
            this.Add(this.frame1);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.Show();
            this.newleftbutton.Clicked += new System.EventHandler(this.OnNewleftbuttonClicked);
            this.newleftbutton1.Clicked += new System.EventHandler(this.OnNewleftbutton1Clicked);
            this.deletebutton.Clicked += new System.EventHandler(this.OnDeletebuttonClicked);
            this.changebuton.Clicked += new System.EventHandler(this.OnChangebutonClicked);
        }
    }
}
