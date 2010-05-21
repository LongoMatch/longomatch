// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace LongoMatch.Gui.Dialog {
    
    
    public partial class DrawingTool {
        
        private Gtk.HBox hbox1;
        
        private Gtk.VBox vbox2;
        
        private LongoMatch.Gui.Component.DrawingToolBox drawingtoolbox1;
        
        private Gtk.Button savetoprojectbutton;
        
        private Gtk.Button savebutton;
        
        private LongoMatch.Gui.Component.DrawingWidget drawingwidget1;
        
        private Gtk.Button button271;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget LongoMatch.Gui.Dialog.DrawingTool
            this.Name = "LongoMatch.Gui.Dialog.DrawingTool";
            this.Title = Mono.Unix.Catalog.GetString("Drawing Tool");
            this.Icon = Gdk.Pixbuf.LoadFromResource("longomatch.png");
            this.WindowPosition = ((Gtk.WindowPosition)(4));
            this.Modal = true;
            this.Gravity = ((Gdk.Gravity)(5));
            this.SkipPagerHint = true;
            this.SkipTaskbarHint = true;
            // Internal child LongoMatch.Gui.Dialog.DrawingTool.VBox
            Gtk.VBox w1 = this.VBox;
            w1.Name = "dialog1_VBox";
            w1.BorderWidth = ((uint)(2));
            // Container child dialog1_VBox.Gtk.Box+BoxChild
            this.hbox1 = new Gtk.HBox();
            this.hbox1.Name = "hbox1";
            this.hbox1.Spacing = 6;
            // Container child hbox1.Gtk.Box+BoxChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            // Container child vbox2.Gtk.Box+BoxChild
            this.drawingtoolbox1 = new LongoMatch.Gui.Component.DrawingToolBox();
            this.drawingtoolbox1.Events = ((Gdk.EventMask)(256));
            this.drawingtoolbox1.Name = "drawingtoolbox1";
            this.vbox2.Add(this.drawingtoolbox1);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.vbox2[this.drawingtoolbox1]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.savetoprojectbutton = new Gtk.Button();
            this.savetoprojectbutton.CanFocus = true;
            this.savetoprojectbutton.Name = "savetoprojectbutton";
            this.savetoprojectbutton.UseUnderline = true;
            // Container child savetoprojectbutton.Gtk.Container+ContainerChild
            Gtk.Alignment w3 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w4 = new Gtk.HBox();
            w4.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w5 = new Gtk.Image();
            w5.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-save", Gtk.IconSize.Menu, 16);
            w4.Add(w5);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w7 = new Gtk.Label();
            w7.LabelProp = Mono.Unix.Catalog.GetString("Save to Project");
            w7.UseUnderline = true;
            w4.Add(w7);
            w3.Add(w4);
            this.savetoprojectbutton.Add(w3);
            this.vbox2.Add(this.savetoprojectbutton);
            Gtk.Box.BoxChild w11 = ((Gtk.Box.BoxChild)(this.vbox2[this.savetoprojectbutton]));
            w11.PackType = ((Gtk.PackType)(1));
            w11.Position = 1;
            w11.Expand = false;
            w11.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.savebutton = new Gtk.Button();
            this.savebutton.CanFocus = true;
            this.savebutton.Name = "savebutton";
            this.savebutton.UseUnderline = true;
            // Container child savebutton.Gtk.Container+ContainerChild
            Gtk.Alignment w12 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment.Gtk.Container+ContainerChild
            Gtk.HBox w13 = new Gtk.HBox();
            w13.Spacing = 2;
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Image w14 = new Gtk.Image();
            w14.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-save", Gtk.IconSize.Menu, 16);
            w13.Add(w14);
            // Container child GtkHBox.Gtk.Container+ContainerChild
            Gtk.Label w16 = new Gtk.Label();
            w16.LabelProp = Mono.Unix.Catalog.GetString("Save to File");
            w16.UseUnderline = true;
            w13.Add(w16);
            w12.Add(w13);
            this.savebutton.Add(w12);
            this.vbox2.Add(this.savebutton);
            Gtk.Box.BoxChild w20 = ((Gtk.Box.BoxChild)(this.vbox2[this.savebutton]));
            w20.PackType = ((Gtk.PackType)(1));
            w20.Position = 2;
            w20.Expand = false;
            w20.Fill = false;
            this.hbox1.Add(this.vbox2);
            Gtk.Box.BoxChild w21 = ((Gtk.Box.BoxChild)(this.hbox1[this.vbox2]));
            w21.Position = 0;
            w21.Expand = false;
            w21.Fill = false;
            // Container child hbox1.Gtk.Box+BoxChild
            this.drawingwidget1 = new LongoMatch.Gui.Component.DrawingWidget();
            this.drawingwidget1.Events = ((Gdk.EventMask)(256));
            this.drawingwidget1.Name = "drawingwidget1";
            this.hbox1.Add(this.drawingwidget1);
            Gtk.Box.BoxChild w22 = ((Gtk.Box.BoxChild)(this.hbox1[this.drawingwidget1]));
            w22.Position = 1;
            w1.Add(this.hbox1);
            Gtk.Box.BoxChild w23 = ((Gtk.Box.BoxChild)(w1[this.hbox1]));
            w23.Position = 0;
            // Internal child LongoMatch.Gui.Dialog.DrawingTool.ActionArea
            Gtk.HButtonBox w24 = this.ActionArea;
            w24.Name = "dialog1_ActionArea";
            w24.Spacing = 6;
            w24.BorderWidth = ((uint)(5));
            w24.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.button271 = new Gtk.Button();
            this.button271.CanFocus = true;
            this.button271.Name = "button271";
            this.button271.UseUnderline = true;
            this.button271.Label = "";
            this.AddActionWidget(this.button271, 0);
            Gtk.ButtonBox.ButtonBoxChild w25 = ((Gtk.ButtonBox.ButtonBoxChild)(w24[this.button271]));
            w25.Expand = false;
            w25.Fill = false;
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.DefaultWidth = 600;
            this.DefaultHeight = 579;
            this.savetoprojectbutton.Hide();
            this.button271.Hide();
            this.Show();
            this.drawingtoolbox1.LineWidthChanged += new LongoMatch.Handlers.LineWidthChangedHandler(this.OnDrawingtoolbox1LineWidthChanged);
            this.drawingtoolbox1.ColorChanged += new LongoMatch.Handlers.ColorChangedHandler(this.OnDrawingtoolbox1ColorChanged);
            this.drawingtoolbox1.VisibilityChanged += new LongoMatch.Handlers.VisibilityChangedHandler(this.OnDrawingtoolbox1VisibilityChanged);
            this.drawingtoolbox1.ClearDrawing += new LongoMatch.Handlers.ClearDrawingHandler(this.OnDrawingtoolbox1ClearDrawing);
            this.savebutton.Clicked += new System.EventHandler(this.OnSavebuttonClicked);
            this.savetoprojectbutton.Clicked += new System.EventHandler(this.OnSavetoprojectbuttonClicked);
        }
    }
}
