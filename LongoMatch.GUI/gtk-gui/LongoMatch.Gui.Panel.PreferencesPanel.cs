
// This file has been generated by the GUI designer. Do not modify.
namespace LongoMatch.Gui.Panel
{
	public partial class PreferencesPanel
	{
		private global::Gtk.VBox vbox2;
		private global::LongoMatch.Gui.Panel.PanelHeader panelheader1;
		private global::Gtk.HBox hbox1;
		private global::Gtk.ScrolledWindow scrolledwindow1;
		private global::Gtk.TreeView treeview;
		private global::Gtk.VBox propsvbox;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget LongoMatch.Gui.Panel.PreferencesPanel
			global::Stetic.BinContainer.Attach (this);
			this.Name = "LongoMatch.Gui.Panel.PreferencesPanel";
			// Container child LongoMatch.Gui.Panel.PreferencesPanel.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.panelheader1 = new global::LongoMatch.Gui.Panel.PanelHeader ();
			this.panelheader1.Events = ((global::Gdk.EventMask)(256));
			this.panelheader1.Name = "panelheader1";
			this.vbox2.Add (this.panelheader1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.panelheader1]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			this.treeview = new global::Gtk.TreeView ();
			this.treeview.CanFocus = true;
			this.treeview.Name = "treeview";
			this.scrolledwindow1.Add (this.treeview);
			this.hbox1.Add (this.scrolledwindow1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.scrolledwindow1]));
			w3.Position = 0;
			w3.Expand = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.propsvbox = new global::Gtk.VBox ();
			this.propsvbox.Name = "propsvbox";
			this.propsvbox.Spacing = 6;
			this.hbox1.Add (this.propsvbox);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.propsvbox]));
			w4.Position = 1;
			this.vbox2.Add (this.hbox1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox1]));
			w5.Position = 1;
			this.Add (this.vbox2);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
		}
	}
}
