
// This file has been generated by the GUI designer. Do not modify.
namespace LongoMatch.Gui.Panel
{
	public partial class ProjectsManagerPanel
	{
		private global::Gtk.VBox vbox3;

		private global::LongoMatch.Gui.Panel.PanelHeader panelheader1;

		private global::Gtk.HBox pm_hbox;

		private global::Gtk.VBox vbox2;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Image focusimage;

		private global::Gtk.Entry filterEntry;

		private global::Gtk.ComboBox sortcombobox;

		private global::Gtk.ScrolledWindow projectsScrolledWindow;

		private global::Gtk.Alignment projectpropertiesalignment;

		private global::Gtk.VBox rbox;

		private global::Gtk.ScrolledWindow scrolledwindow3;

		private global::Gtk.VBox projectbox;

		private global::LongoMatch.Gui.Component.GameDescriptionHeader gamedescriptionheader1;

		private global::Gtk.EventBox infoeventbox;

		private global::Gtk.Label infolabel;

		private global::Gtk.Table table1;

		private global::Gtk.Entry competitionentry;

		private global::Gtk.Label Competitionlabel;

		private global::VAS.UI.UI.Component.DatePicker datepicker;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gtk.TextView desctextview;

		private global::Gtk.Label label14;

		private global::Gtk.Label label5;

		private global::Gtk.Label label9;

		private global::Gtk.Entry seasonentry;

		private global::Gtk.Label seasonlabel;

		private global::Gtk.Label templatelabel;

		private global::Gtk.EventBox videoseventbox;

		private global::Gtk.Label videoslabel;

		private global::Gtk.Alignment videofileinfo_alignment;

		private global::Gtk.VBox videofileinfo_vbox;

		private global::Gtk.HButtonBox hbuttonbox1;

		private global::Gtk.Button openbutton;

		private global::Gtk.Image openbuttonimage;

		private global::Gtk.Button savebutton;

		private global::Gtk.Image savebuttonimage;

		private global::Gtk.Button exportbutton;

		private global::Gtk.Image exportbuttonimage;

		private global::Gtk.Button resyncbutton;

		private global::Gtk.Image resyncbuttonimage;

		private global::Gtk.Button deletebutton;

		private global::Gtk.Image deletebuttonimage;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget LongoMatch.Gui.Panel.ProjectsManagerPanel
			global::Stetic.BinContainer.Attach (this);
			this.Name = "LongoMatch.Gui.Panel.ProjectsManagerPanel";
			// Container child LongoMatch.Gui.Panel.ProjectsManagerPanel.Gtk.Container+ContainerChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.Name = "vbox3";
			// Container child vbox3.Gtk.Box+BoxChild
			this.panelheader1 = new global::LongoMatch.Gui.Panel.PanelHeader ();
			this.panelheader1.Events = ((global::Gdk.EventMask)(256));
			this.panelheader1.Name = "panelheader1";
			this.vbox3.Add (this.panelheader1);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.panelheader1]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.pm_hbox = new global::Gtk.HBox ();
			this.pm_hbox.Name = "pm_hbox";
			this.pm_hbox.Spacing = 6;
			// Container child pm_hbox.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.focusimage = new global::Gtk.Image ();
			this.focusimage.Name = "focusimage";
			this.hbox1.Add (this.focusimage);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.focusimage]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.filterEntry = new global::Gtk.Entry ();
			this.filterEntry.WidthRequest = 100;
			this.filterEntry.CanFocus = true;
			this.filterEntry.Name = "filterEntry";
			this.filterEntry.IsEditable = true;
			this.filterEntry.InvisibleChar = '●';
			this.hbox1.Add (this.filterEntry);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.filterEntry]));
			w3.Position = 1;
			// Container child hbox1.Gtk.Box+BoxChild
			this.sortcombobox = global::Gtk.ComboBox.NewText ();
			this.sortcombobox.AppendText (global::VAS.Core.Catalog.GetString ("Sort by name"));
			this.sortcombobox.AppendText (global::VAS.Core.Catalog.GetString ("Sort by date"));
			this.sortcombobox.AppendText (global::VAS.Core.Catalog.GetString ("Sort by modification date"));
			this.sortcombobox.AppendText (global::VAS.Core.Catalog.GetString ("Sort by season"));
			this.sortcombobox.AppendText (global::VAS.Core.Catalog.GetString ("Sort by competition"));
			this.sortcombobox.WidthRequest = 150;
			this.sortcombobox.Name = "sortcombobox";
			this.sortcombobox.Active = 0;
			this.hbox1.Add (this.sortcombobox);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.sortcombobox]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			this.vbox2.Add (this.hbox1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox1]));
			w5.Position = 0;
			w5.Expand = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.projectsScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.projectsScrolledWindow.CanFocus = true;
			this.projectsScrolledWindow.Name = "projectsScrolledWindow";
			this.projectsScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			this.vbox2.Add (this.projectsScrolledWindow);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.projectsScrolledWindow]));
			w6.Position = 1;
			this.pm_hbox.Add (this.vbox2);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.pm_hbox [this.vbox2]));
			w7.Position = 0;
			// Container child pm_hbox.Gtk.Box+BoxChild
			this.projectpropertiesalignment = new global::Gtk.Alignment (0.5F, 0.5F, 1F, 1F);
			this.projectpropertiesalignment.Name = "projectpropertiesalignment";
			this.projectpropertiesalignment.BottomPadding = ((uint)(12));
			// Container child projectpropertiesalignment.Gtk.Container+ContainerChild
			this.rbox = new global::Gtk.VBox ();
			this.rbox.Name = "rbox";
			this.rbox.Spacing = 6;
			// Container child rbox.Gtk.Box+BoxChild
			this.scrolledwindow3 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow3.CanFocus = true;
			this.scrolledwindow3.Name = "scrolledwindow3";
			this.scrolledwindow3.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			// Container child scrolledwindow3.Gtk.Container+ContainerChild
			global::Gtk.Viewport w8 = new global::Gtk.Viewport ();
			w8.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport1.Gtk.Container+ContainerChild
			this.projectbox = new global::Gtk.VBox ();
			this.projectbox.Name = "projectbox";
			// Container child projectbox.Gtk.Box+BoxChild
			this.gamedescriptionheader1 = new global::LongoMatch.Gui.Component.GameDescriptionHeader ();
			this.gamedescriptionheader1.HeightRequest = 60;
			this.gamedescriptionheader1.Events = ((global::Gdk.EventMask)(256));
			this.gamedescriptionheader1.Name = "gamedescriptionheader1";
			this.projectbox.Add (this.gamedescriptionheader1);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.projectbox [this.gamedescriptionheader1]));
			w9.Position = 0;
			w9.Expand = false;
			w9.Fill = false;
			w9.Padding = ((uint)(50));
			// Container child projectbox.Gtk.Box+BoxChild
			this.infoeventbox = new global::Gtk.EventBox ();
			this.infoeventbox.Name = "infoeventbox";
			// Container child infoeventbox.Gtk.Container+ContainerChild
			this.infolabel = new global::Gtk.Label ();
			this.infolabel.Name = "infolabel";
			this.infolabel.LabelProp = global::VAS.Core.Catalog.GetString ("Information");
			this.infolabel.UseMarkup = true;
			this.infoeventbox.Add (this.infolabel);
			this.projectbox.Add (this.infoeventbox);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.projectbox [this.infoeventbox]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			// Container child projectbox.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(3)), ((uint)(4)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(5));
			this.table1.ColumnSpacing = ((uint)(5));
			// Container child table1.Gtk.Table+TableChild
			this.competitionentry = new global::Gtk.Entry ();
			this.competitionentry.CanFocus = true;
			this.competitionentry.Name = "competitionentry";
			this.competitionentry.IsEditable = true;
			this.competitionentry.InvisibleChar = '●';
			this.table1.Add (this.competitionentry);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1 [this.competitionentry]));
			w12.LeftAttach = ((uint)(3));
			w12.RightAttach = ((uint)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.Competitionlabel = new global::Gtk.Label ();
			this.Competitionlabel.Name = "Competitionlabel";
			this.Competitionlabel.Xalign = 1F;
			this.Competitionlabel.LabelProp = global::VAS.Core.Catalog.GetString ("Competition:");
			this.table1.Add (this.Competitionlabel);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1 [this.Competitionlabel]));
			w13.LeftAttach = ((uint)(2));
			w13.RightAttach = ((uint)(3));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.datepicker = new global::VAS.UI.UI.Component.DatePicker ();
			this.datepicker.WidthRequest = 150;
			this.datepicker.Events = ((global::Gdk.EventMask)(256));
			this.datepicker.Name = "datepicker";
			this.datepicker.Date = new global::System.DateTime (0);
			this.table1.Add (this.datepicker);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1 [this.datepicker]));
			w14.TopAttach = ((uint)(1));
			w14.BottomAttach = ((uint)(2));
			w14.LeftAttach = ((uint)(3));
			w14.RightAttach = ((uint)(4));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(0));
			// Container child table1.Gtk.Table+TableChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.desctextview = new global::Gtk.TextView ();
			this.desctextview.CanFocus = true;
			this.desctextview.Name = "desctextview";
			this.GtkScrolledWindow.Add (this.desctextview);
			this.table1.Add (this.GtkScrolledWindow);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1 [this.GtkScrolledWindow]));
			w16.TopAttach = ((uint)(2));
			w16.BottomAttach = ((uint)(3));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label14 = new global::Gtk.Label ();
			this.label14.Name = "label14";
			this.label14.Xalign = 1F;
			this.label14.LabelProp = global::VAS.Core.Catalog.GetString ("Description:");
			this.table1.Add (this.label14);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1 [this.label14]));
			w17.TopAttach = ((uint)(2));
			w17.BottomAttach = ((uint)(3));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label ();
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.LabelProp = global::VAS.Core.Catalog.GetString ("Date:");
			this.table1.Add (this.label5);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table1 [this.label5]));
			w18.TopAttach = ((uint)(1));
			w18.BottomAttach = ((uint)(2));
			w18.LeftAttach = ((uint)(2));
			w18.RightAttach = ((uint)(3));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label9 = new global::Gtk.Label ();
			this.label9.Name = "label9";
			this.label9.Xalign = 1F;
			this.label9.LabelProp = global::VAS.Core.Catalog.GetString ("Analysis dashboard:");
			this.table1.Add (this.label9);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table1 [this.label9]));
			w19.TopAttach = ((uint)(1));
			w19.BottomAttach = ((uint)(2));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.seasonentry = new global::Gtk.Entry ();
			this.seasonentry.CanFocus = true;
			this.seasonentry.Name = "seasonentry";
			this.seasonentry.IsEditable = true;
			this.seasonentry.InvisibleChar = '●';
			this.table1.Add (this.seasonentry);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table1 [this.seasonentry]));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(2));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.seasonlabel = new global::Gtk.Label ();
			this.seasonlabel.Name = "seasonlabel";
			this.seasonlabel.Xalign = 1F;
			this.seasonlabel.LabelProp = global::VAS.Core.Catalog.GetString ("Season:");
			this.table1.Add (this.seasonlabel);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table1 [this.seasonlabel]));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.templatelabel = new global::Gtk.Label ();
			this.templatelabel.Name = "templatelabel";
			this.templatelabel.Xalign = 0F;
			this.table1.Add (this.templatelabel);
			global::Gtk.Table.TableChild w22 = ((global::Gtk.Table.TableChild)(this.table1 [this.templatelabel]));
			w22.TopAttach = ((uint)(1));
			w22.BottomAttach = ((uint)(2));
			w22.LeftAttach = ((uint)(1));
			w22.RightAttach = ((uint)(2));
			w22.XOptions = ((global::Gtk.AttachOptions)(4));
			w22.YOptions = ((global::Gtk.AttachOptions)(4));
			this.projectbox.Add (this.table1);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.projectbox [this.table1]));
			w23.Position = 2;
			w23.Expand = false;
			w23.Padding = ((uint)(20));
			// Container child projectbox.Gtk.Box+BoxChild
			this.videoseventbox = new global::Gtk.EventBox ();
			this.videoseventbox.Name = "videoseventbox";
			// Container child videoseventbox.Gtk.Container+ContainerChild
			this.videoslabel = new global::Gtk.Label ();
			this.videoslabel.Name = "videoslabel";
			this.videoslabel.LabelProp = global::VAS.Core.Catalog.GetString ("Videos");
			this.videoslabel.UseMarkup = true;
			this.videoseventbox.Add (this.videoslabel);
			this.projectbox.Add (this.videoseventbox);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.projectbox [this.videoseventbox]));
			w25.Position = 3;
			w25.Expand = false;
			w25.Fill = false;
			// Container child projectbox.Gtk.Box+BoxChild
			this.videofileinfo_alignment = new global::Gtk.Alignment (0F, 0F, 1F, 0F);
			this.videofileinfo_alignment.Name = "videofileinfo_alignment";
			// Container child videofileinfo_alignment.Gtk.Container+ContainerChild
			this.videofileinfo_vbox = new global::Gtk.VBox ();
			this.videofileinfo_vbox.Name = "videofileinfo_vbox";
			this.videofileinfo_alignment.Add (this.videofileinfo_vbox);
			this.projectbox.Add (this.videofileinfo_alignment);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.projectbox [this.videofileinfo_alignment]));
			w27.Position = 4;
			w27.Padding = ((uint)(20));
			w8.Add (this.projectbox);
			this.scrolledwindow3.Add (w8);
			this.rbox.Add (this.scrolledwindow3);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.rbox [this.scrolledwindow3]));
			w30.Position = 0;
			// Container child rbox.Gtk.Box+BoxChild
			this.hbuttonbox1 = new global::Gtk.HButtonBox ();
			// Container child hbuttonbox1.Gtk.ButtonBox+ButtonBoxChild
			this.openbutton = new global::Gtk.Button ();
			this.openbutton.TooltipMarkup = "Open";
			this.openbutton.Sensitive = false;
			this.openbutton.CanFocus = true;
			this.openbutton.Name = "openbutton";
			// Container child openbutton.Gtk.Container+ContainerChild
			this.openbuttonimage = new global::Gtk.Image ();
			this.openbuttonimage.Name = "openbuttonimage";
			this.openbutton.Add (this.openbuttonimage);
			this.hbuttonbox1.Add (this.openbutton);
			global::Gtk.ButtonBox.ButtonBoxChild w32 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox1 [this.openbutton]));
			w32.Expand = false;
			w32.Fill = false;
			// Container child hbuttonbox1.Gtk.ButtonBox+ButtonBoxChild
			this.savebutton = new global::Gtk.Button ();
			this.savebutton.TooltipMarkup = "Save";
			this.savebutton.Sensitive = false;
			this.savebutton.CanFocus = true;
			this.savebutton.Name = "savebutton";
			// Container child savebutton.Gtk.Container+ContainerChild
			this.savebuttonimage = new global::Gtk.Image ();
			this.savebuttonimage.Name = "savebuttonimage";
			this.savebutton.Add (this.savebuttonimage);
			this.hbuttonbox1.Add (this.savebutton);
			global::Gtk.ButtonBox.ButtonBoxChild w34 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox1 [this.savebutton]));
			w34.Position = 1;
			w34.Expand = false;
			w34.Fill = false;
			// Container child hbuttonbox1.Gtk.ButtonBox+ButtonBoxChild
			this.exportbutton = new global::Gtk.Button ();
			this.exportbutton.TooltipMarkup = "Export";
			this.exportbutton.Sensitive = false;
			this.exportbutton.CanFocus = true;
			this.exportbutton.Name = "exportbutton";
			// Container child exportbutton.Gtk.Container+ContainerChild
			this.exportbuttonimage = new global::Gtk.Image ();
			this.exportbuttonimage.Name = "exportbuttonimage";
			this.exportbutton.Add (this.exportbuttonimage);
			this.hbuttonbox1.Add (this.exportbutton);
			global::Gtk.ButtonBox.ButtonBoxChild w36 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox1 [this.exportbutton]));
			w36.Position = 2;
			w36.Expand = false;
			w36.Fill = false;
			// Container child hbuttonbox1.Gtk.ButtonBox+ButtonBoxChild
			this.resyncbutton = new global::Gtk.Button ();
			this.resyncbutton.TooltipMarkup = "Adjust periods and secondary videos synchronisation";
			this.resyncbutton.Sensitive = false;
			this.resyncbutton.CanFocus = true;
			this.resyncbutton.Name = "resyncbutton";
			// Container child resyncbutton.Gtk.Container+ContainerChild
			this.resyncbuttonimage = new global::Gtk.Image ();
			this.resyncbuttonimage.Name = "resyncbuttonimage";
			this.resyncbutton.Add (this.resyncbuttonimage);
			this.hbuttonbox1.Add (this.resyncbutton);
			global::Gtk.ButtonBox.ButtonBoxChild w38 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox1 [this.resyncbutton]));
			w38.Position = 3;
			w38.Expand = false;
			w38.Fill = false;
			// Container child hbuttonbox1.Gtk.ButtonBox+ButtonBoxChild
			this.deletebutton = new global::Gtk.Button ();
			this.deletebutton.TooltipMarkup = "Delete";
			this.deletebutton.Sensitive = false;
			this.deletebutton.CanFocus = true;
			this.deletebutton.Name = "deletebutton";
			// Container child deletebutton.Gtk.Container+ContainerChild
			this.deletebuttonimage = new global::Gtk.Image ();
			this.deletebuttonimage.Name = "deletebuttonimage";
			this.deletebutton.Add (this.deletebuttonimage);
			this.hbuttonbox1.Add (this.deletebutton);
			global::Gtk.ButtonBox.ButtonBoxChild w40 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox1 [this.deletebutton]));
			w40.Position = 4;
			w40.Expand = false;
			w40.Fill = false;
			this.rbox.Add (this.hbuttonbox1);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.rbox [this.hbuttonbox1]));
			w41.Position = 1;
			w41.Expand = false;
			w41.Fill = false;
			this.projectpropertiesalignment.Add (this.rbox);
			this.pm_hbox.Add (this.projectpropertiesalignment);
			global::Gtk.Box.BoxChild w43 = ((global::Gtk.Box.BoxChild)(this.pm_hbox [this.projectpropertiesalignment]));
			w43.Position = 1;
			this.vbox3.Add (this.pm_hbox);
			global::Gtk.Box.BoxChild w44 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.pm_hbox]));
			w44.Position = 1;
			this.Add (this.vbox3);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.projectbox.Hide ();
			this.Hide ();
		}
	}
}
