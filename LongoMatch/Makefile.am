ASSEMBLY = LongoMatch
TARGET = exe
LINK = $(REF_DEP_LONGOMATCH)

SOURCES = \
	AssemblyInfo.cs \
	Common/Enums.cs \
	Common/Cairo.cs \
	Common/Constants.cs \
	Common/GStreamer.cs \
	Compat/0.0/DatabaseMigrator.cs \
	Compat/0.0/DB/DataBase.cs \
	Compat/0.0/DB/MediaFile.cs \
	Compat/0.0/DB/Project.cs \
	Compat/0.0/DB/Sections.cs \
	Compat/0.0/IO/SectionsReader.cs \
	Compat/0.0/Playlist/IPlayList.cs \
	Compat/0.0/PlayListMigrator.cs \
	Compat/0.0/Playlist/PlayList.cs \
	Compat/0.0/TemplatesMigrator.cs \
	Compat/0.0/Time/MediaTimeNode.cs \
	Compat/0.0/Time/PixbufTimeNode.cs \
	Compat/0.0/Time/PlayListTimeNode.cs \
	Compat/0.0/Time/SectionsTimeNode.cs \
	Compat/0.0/Time/Time.cs \
	Compat/0.0/Time/TimeNode.cs \
	Common/ConsoleCrayon.cs \
	Common/Log.cs \
	Common/SerializableObject.cs \
	DB/DataBase.cs \
	gtk-gui/generated.cs \
	gtk-gui/LongoMatch.Gui.Component.ButtonsWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.CategoryProperties.cs \
	gtk-gui/LongoMatch.Gui.Component.DrawingWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.DrawingToolBox.cs \
	gtk-gui/LongoMatch.Gui.Component.NotesWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.PlayerProperties.cs \
	gtk-gui/LongoMatch.Gui.Component.PlayersListTreeWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.PlayListWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.PlaysListTreeWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.ProjectDetailsWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.ProjectListWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.ProjectTemplateWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TaggerWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TagsTreeWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TeamTemplateWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeAdjustWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeLineWidget.cs \
	gtk-gui/LongoMatch.Gui.Dialog.BusyDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.DrawingTool.cs \
	gtk-gui/LongoMatch.Gui.Dialog.EditCategoryDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.EditPlayerDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.EndCaptureDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.EntryDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.FramesCaptureProgressDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.HotKeySelectorDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.NewProjectDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.OpenProjectDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.PlayersSelectionDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.ProjectsManager.cs \
	gtk-gui/LongoMatch.Gui.Dialog.ProjectTemplateEditorDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.ProjectSelectionDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.SnapshotsDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.TaggerDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.TeamTemplateEditor.cs \
	gtk-gui/LongoMatch.Gui.Dialog.TemplatesManager.cs \
	gtk-gui/LongoMatch.Gui.Dialog.UpdateDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.VideoEditionProperties.cs \
	gtk-gui/LongoMatch.Gui.Dialog.Win32CalendarDialog.cs \
	gtk-gui/LongoMatch.Gui.MainWindow.cs \
	gtk-gui/LongoMatch.Gui.Popup.CalendarPopup.cs \
	gtk-gui/LongoMatch.Gui.Popup.TransparentDrawingArea.cs \
	Gui/Component/ButtonsWidget.cs \
	Gui/Component/CategoriesScale.cs \
	Gui/Component/CategoryProperties.cs \
	Gui/Component/DrawingWidget.cs \
	Gui/Component/DrawingToolBox.cs \
	Gui/Component/NotesWidget.cs \
	Gui/Component/PlayerProperties.cs \
	Gui/Component/PlayersListTreeWidget.cs \
	Gui/Component/PlayListWidget.cs \
	Gui/Component/PlaysListTreeWidget.cs \
	Gui/Component/ProjectDetailsWidget.cs \
	Gui/Component/ProjectListWidget.cs \
	Gui/Component/ProjectTemplateWidget.cs \
	Gui/Component/TaggerWidget.cs\
	Gui/Component/TagsTreeWidget.cs\
	Gui/Component/TeamTemplateWidget.cs\
	Gui/Component/TimeAdjustWidget.cs \
	Gui/Component/TimeLineWidget.cs \
	Gui/Component/TimeReferenceWidget.cs \
	Gui/Component/TimeScale.cs \
	Gui/Dialog/BusyDialog.cs \
	Gui/Dialog/DrawingTool.cs \
	Gui/Dialog/EditCategoryDialog.cs \
	Gui/Dialog/EditPlayerDialog.cs \
	Gui/Dialog/EndCaptureDialog.cs \
	Gui/Dialog/EntryDialog.cs \
	Gui/Dialog/FramesCaptureProgressDialog.cs \
	Gui/Dialog/HotKeySelectorDialog.cs \
	Gui/Dialog/NewProjectDialog.cs \
	Gui/Dialog/OpenProjectDialog.cs \
	Gui/Dialog/PlayersSelectionDialog.cs \
	Gui/Dialog/ProjectsManager.cs \
	Gui/Dialog/ProjectTemplateEditorDialog.cs \
	Gui/Dialog/ProjectSelectionDialog.cs \
	Gui/Dialog/SnapshotsDialog.cs \
	Gui/Dialog/TaggerDialog.cs \
	Gui/Dialog/TemplatesEditor.cs \
	Gui/Dialog/TeamTemplateEditor.cs\
	Gui/Dialog/UpdateDialog.cs \
	Gui/Dialog/VideoEditionProperties.cs \
	Gui/Dialog/Win32CalendarDialog.cs \
	Gui/MainWindow.cs \
	Gui/Popup/CalendarPopup.cs \
	Gui/Popup/MessagePopup.cs \
	Gui/TransparentDrawingArea.cs \
	Gui/TreeView/CategoriesTreeView.cs \
	Gui/TreeView/PlayerPropertiesTreeView.cs \
	Gui/TreeView/PlayersTreeView.cs \
	Gui/TreeView/ListTreeViewBase.cs \
	Gui/TreeView/PlayListTreeView.cs \
	Gui/TreeView/PlaysTreeView.cs \
	Gui/TreeView/TagsTreeView.cs \
	Handlers/DrawingManager.cs \
	Handlers/EventsManager.cs \
	Handlers/Handlers.cs \
	Handlers/HotKeysManager.cs \
	Handlers/VideoDrawingsManager.cs\
	IO/CSVExport.cs \
	IO/XMLReader.cs \
	Main.cs \
	Playlist/IPlayList.cs \
	Playlist/PlayList.cs \
	Store/Category.cs \
	Store/SubCategory.cs \
	Store/Drawing.cs \
	Store/DrawingsList.cs \
	Store/HotKey.cs \
	Store/PixbufTimeNode.cs \
	Store/Play.cs \
	Store/PlayListPlay.cs \
	Store/Player.cs \
	Store/Project.cs \
	Store/ProjectDescription.cs \
	Store/Tag.cs \
	Store/Time.cs \
	Store/TimeNode.cs \
	Store/Templates/CategoriesTemplate.cs \
	Store/Templates/SubCategoriesTemplate.cs \
	Store/Templates/TeamTemplate.cs \
	Updates/Updater.cs \
	Updates/XmlUpdateParser.cs \
	Utils/ProjectUtils.cs

IMAGES = images/longomatch.png \
         images/background.png

LOGO = images/logo_48x48.png

RESOURCES = \
	gtk-gui/gui.stetic\
	images/longomatch.png\
	images/stock_draw-line-45.png\
	images/stock_draw-circle-unfilled.png\
	images/stock_draw-line-ends-with-arrow.png\
	images/stock_draw-rectangle-unfilled.png\
	images/stock_draw-freeform-line.png\
	images/camera-video.png\
	images/video.png 

bin_SCRIPTS = longomatch

DESKTOP_FILE = longomatch.desktop.in

include $(top_srcdir)/build/build.mk

EXTRA_DIST += \
	longomatch.in\
	AssemblyInfo.cs.in

