
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = $(CSC)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG "-main:LongoMatch.MainClass"
ASSEMBLY = bin/Debug/LongoMatch.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../CesarPlayer/bin/Debug/CesarPlayer.dll
BUILD_DIR = bin/Debug
CESARPLAYER_DLL_SOURCE=../CesarPlayer/bin/Debug/CesarPlayer.dll
CESARPLAYER_DLL=$(BUILD_DIR)/CesarPlayer.dll
CESARPLAYER_DLL_MDB_SOURCE=../CesarPlayer/bin/Debug/CesarPlayer.dll.mdb
CESARPLAYER_DLL_MDB=$(BUILD_DIR)/CesarPlayer.dll.mdb
LONGOMATCH_PNG_SOURCE=images/longomatch.png
LONGOMATCH_EXE_MDB_SOURCE=bin/Debug/LongoMatch.exe.mdb
LONGOMATCH_EXE_MDB = $(BUILD_DIR)/LongoMatch.exe.mdb
LONGOMATCH_DESKTOP_SOURCE=longomatch.desktop
BACKGROUND_PNG_SOURCE=images/background.png

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = $(CSC)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-main:LongoMatch.MainClass"
ASSEMBLY = bin/Release/LongoMatch.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../CesarPlayer/bin/Release/CesarPlayer.dll
BUILD_DIR = bin/Release
CESARPLAYER_DLL_SOURCE=../CesarPlayer/bin/Release/CesarPlayer.dll
CESARPLAYER_DLL=$(BUILD_DIR)/CesarPlayer.dll
LONGOMATCH_PNG_SOURCE=images/longomatch.png
LONGOMATCH_LOGO_SOURCE=images/logo_48x48.png
LONGOMATCH_EXE_MDB_SOURCE=
LONGOMATCH_EXE_MDB = 
LONGOMATCH_DESKTOP_SOURCE=longomatch.desktop
CESARPLAYER_DLL_MDB_SOURCE=
CESARPLAYER_DLL_MDB=
BACKGROUND_PNG_SOURCE=images/background.png

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

COMMONAPPLICATIONDATAROOT_IMAGES = \
	$(LONGOMATCH_PNG) \
	$(BACKGROUND_PNG)  

COMMONAPPLICATIONDATAROOT_LOGO = \
	$(LONGOMATCH_LOGO)
	

PROGRAMFILES = \
	$(CESARPLAYER_DLL) \
	$(LONGOMATCH_EXE_MDB) \
	$(CESARPLAYER_DLL_MDB) 

LINUX_DESKTOPAPPLICATIONS = \
	$(LONGOMATCH_DESKTOP)  

BINARIES = \
	$(LONGOMATCH)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(COMMONAPPLICATIONDATAROOT_IMAGES)  $(COMMONAPPLICATIONDATAROOT_LOGO) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

FILES = \
	AssemblyInfo.cs \
	Common/Enums.cs \
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
	DB/DataBase.cs \
	DB/Project.cs \
	DB/ProjectDescription.cs\
	DB/Sections.cs \
	DB/TagsTemplate.cs \
	DB/TeamTemplate.cs \
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
	gtk-gui/LongoMatch.Gui.Component.TeamTemplateWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeAdjustWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeLineWidget.cs \
	gtk-gui/LongoMatch.Gui.Dialog.DrawingTool.cs \
	gtk-gui/LongoMatch.Gui.Dialog.EditCategoryDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.EditPlayerDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.EntryDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.FramesCaptureProgressDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.HotKeySelectorDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.Migrator.cs \
	gtk-gui/LongoMatch.Gui.Dialog.NewProjectDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.OpenProjectDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.PlayersSelectionDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.ProjectsManager.cs \
	gtk-gui/LongoMatch.Gui.Dialog.ProjectTemplateEditorDialog.cs \
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
	Gui/Component/TeamTemplateWidget.cs\
	Gui/Component/TimeAdjustWidget.cs \
	Gui/Component/TimeLineWidget.cs \
	Gui/Component/TimeReferenceWidget.cs \
	Gui/Component/TimeScale.cs \
	Gui/Dialog/DrawingTool.cs \
	Gui/Dialog/EditCategoryDialog.cs \
	Gui/Dialog/EditPlayerDialog.cs \
	Gui/Dialog/EntryDialog.cs \
	Gui/Dialog/FramesCaptureProgressDialog.cs \
	Gui/Dialog/HotKeySelectorDialog.cs \
	Gui/Dialog/Migrator.cs \
	Gui/Dialog/NewProjectDialog.cs \
	Gui/Dialog/OpenProjectDialog.cs \
	Gui/Dialog/PlayersSelectionDialog.cs \
	Gui/Dialog/ProjectsManager.cs \
	Gui/Dialog/ProjectTemplateEditorDialog.cs \
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
	Gui/TreeView/PlayListTreeView.cs \
	Gui/TreeView/PlaysTreeView.cs \
	Handlers/DrawingManager.cs \
	Handlers/EventsManager.cs \
	Handlers/Handlers.cs \
	Handlers/HotKeysManager.cs \
	Handlers/VideoDrawingsManager.cs\
	IO/CSVExport.cs \
	IO/SectionsReader.cs \
	IO/SectionsWriter.cs \
	IO/XMLReader.cs \
	Main.cs \
	Playlist/IPlayList.cs \
	Playlist/PlayList.cs \
	Time/Tag.cs \
	Time/Drawing.cs\
	Time/HotKey.cs \
	Time/MediaTimeNode.cs \
	Time/PixbufTimeNode.cs \
	Time/Player.cs \
	Time/PlayListTimeNode.cs \
	Time/SectionsTimeNode.cs \
	Time/Time.cs \
	Time/TimeNode.cs \
	Updates/Updater.cs \
	Updates/XmlUpdateParser.cs
	
DATA_FILES = 

RESOURCES = \
	gtk-gui/objects.xml \
	gtk-gui/gui.stetic\
	images/longomatch.png,longomatch.png\
	images/stock_draw-line-45.png\
	images/stock_draw-circle-unfilled.png\
	images/stock_draw-line-ends-with-arrow.png\
	images/stock_draw-rectangle-unfilled.png\
	images/stock_draw-freeform-line.png 

EXTRAS = \
	images/longomatch.png\
	images/background.png\
	images/logo_48x48.png \
	longomatch.desktop\
	longomatch.in\
	AssemblyInfo.cs.in

REFERENCES =  \
	Mono.Posix \
	System.Xml \
	$(GTK_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS) \
	Mono.Cairo \
	System \
	$(DB4O_60_LIBS)

DLL_REFERENCES = 

CLEANFILES = $(COMMONAPPLICATIONDATAROOT_IMAGES)  $(COMMONAPPLICATIONDATAROOT_LOGO) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

include $(top_srcdir)/Makefile.include

LONGOMATCH_PNG = $(BUILD_DIR)/images/longomatch.png
LONGOMATCH_LOGO = $(BUILD_DIR)/icons/hicolor/48x48/apps/longomatch.png
LONGOMATCH = $(BUILD_DIR)/longomatch
LONGOMATCH_DESKTOP = $(BUILD_DIR)/longomatch.desktop
BACKGROUND_PNG = $(BUILD_DIR)/images/background.png

$(eval $(call emit-deploy-target,CESARPLAYER_DLL))
$(eval $(call emit-deploy-target,LONGOMATCH_PNG))
$(eval $(call emit-deploy-target,LONGOMATCH_LOGO))
$(eval $(call emit-deploy-target,BACKGROUND_PNG))
$(eval $(call emit-deploy-wrapper,LONGOMATCH,longomatch,x))
$(eval $(call emit-deploy-target,LONGOMATCH_DESKTOP))
$(eval $(call emit-deploy-target,CESARPLAYER_DLL_MDB))



$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
