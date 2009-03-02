
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG "-main:LongoMatch.MainClass"

ASSEMBLY = bin/Debug/LongoMatch.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../CesarPlayer/a.dll
BUILD_DIR = bin/Debug

CESARPLAYER_DLL=
A_DLL_SOURCE=../CesarPlayer/a.dll
A_DLL=$(BUILD_DIR)/a.dll
LONGOMATCH_DESKTOP_SOURCE=app.desktop
CESARPLAYER_DLL_MDB=
A_DLL_MDB_SOURCE=../CesarPlayer/a.dll.mdb
A_DLL_MDB=$(BUILD_DIR)/a.dll.mdb
BACKGROUND_PNG_SOURCE=images/background.png
LGMLOGO_PNG_SOURCE=images/lgmlogo.png

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG "-main:LongoMatch.MainClass"

ASSEMBLY = bin/Release/LongoMatch.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES =  \
	../CesarPlayer/bin/Release/CesarPlayer.dll
BUILD_DIR = bin/Release

CESARPLAYER_DLL_SOURCE=../CesarPlayer/bin/Release/CesarPlayer.dll
CESARPLAYER_DLL=$(BUILD_DIR)/CesarPlayer.dll
A_DLL=
LONGOMATCH_DESKTOP_SOURCE=app.desktop
CESARPLAYER_DLL_MDB_SOURCE=../CesarPlayer/bin/Release/CesarPlayer.dll.mdb
CESARPLAYER_DLL_MDB=$(BUILD_DIR)/CesarPlayer.dll.mdb
A_DLL_MDB=
BACKGROUND_PNG_SOURCE=images/background.png
LGMLOGO_PNG_SOURCE=images/lgmlogo.png

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

COMMONAPPLICATIONDATAROOT_IMAGES = \
	$(BACKGROUND_PNG) \
	$(LGMLOGO_PNG)  

PROGRAMFILES = \
	$(CESARPLAYER_DLL) \
	$(A_DLL) \
	$(CESARPLAYER_DLL_MDB) \
	$(A_DLL_MDB)  

LINUX_DESKTOPAPPLICATIONS = \
	$(LONGOMATCH_DESKTOP)  

BINARIES = \
	$(LONGOMATCH)  


	
all: $(ASSEMBLY) $(COMMONAPPLICATIONDATAROOT_IMAGES) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

FILES = \
	gtk-gui/generated.cs \
	AssemblyInfo.cs \
	Main.cs \
	Gui/ButtonsWidget.cs \
	Gui/CalendarPopup.cs \
	Gui/DBManager.cs \
	Gui/ProjectListWidget.cs \
	Gui/FileDescriptionWidget.cs \
	Gui/FramesCaptureProgressDialog.cs\
	Gui/MainWindow.cs \
	Gui/NewProjectDialog.cs \
	Gui/OpenProjectDialog.cs \
	Gui/PlayListTreeView.cs \
	Gui/PlayListWidget.cs \
	Gui/SectionsPropertiesWidget.cs \
	Gui/SectionsTemplates.cs \
	Gui/SectionsPropertiesWidget.cs\
	Gui/TemplateEditorDialog.cs \
	Gui/TemplateNameSelectionDialog.cs\
	Gui/TimeAdjustWidget.cs \
	Gui/TimeNodesTreeView.cs \
	Gui/TreeWidget.cs \
	Gui/TimeNodeProperties.cs \
	DB/Project.cs \
	DB/Sections.cs \
	DB/DataBase.cs \
	Handlers/Handlers.cs \
	IO/SectionsWriter.cs \
	IO/SectionsReader.cs \
	IO/XMLReader.cs \
	DB/MediaFile.cs \
	Gui/TimeLineWidget.cs \
	Gui/TimeReferenceWidget.cs \
	Gui/TimeScale.cs \
	Time/MediaTimeNode.cs \
	Time/PixbufTimeNode.cs \
	Time/SectionsTimeNode.cs \
	Time/TimeNode.cs \
	Time/Time.cs \
	Time/PlayListTimeNode.cs \
	EventsManager.cs \
	Video/AudioQuality.cs \
	Video/FFMPEGVideoEditor.cs \
	Video/IVideoEditor.cs \
	Video/VideoQuality.cs \
	gtk-gui/LongoMatch.Gui.Component.FileDescriptionWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.ProjectListWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TreeWidget.cs \
	gtk-gui/LongoMatch.Gui.Popup.CalendarPopup.cs \
	gtk-gui/LongoMatch.Gui.Component.ButtonsWidget.cs \
	gtk-gui/LongoMatch.Gui.Dialog.DBManager.cs \
	gtk-gui/LongoMatch.Gui.Dialog.OpenProjectDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.NewProjectDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.FramesCaptureProgressDialog.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeAdjustWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.SectionsPropertiesWidget.cs \
	gtk-gui/LongoMatch.Gui.Dialog.SectionsTemplates.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeNodeProperties.cs \
	gtk-gui/LongoMatch.Gui.Dialog.TemplateNameSelectionDialog.cs\
	gtk-gui/LongoMatch.Gui.Dialog.TemplateEditorDialog.cs \
	gtk-gui/LongoMatch.Gui.Component.PlayListWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeLineWidget.cs \
	gtk-gui/LongoMatch.Gui.Dialog.VideoEditionProperties.cs \
	Gui/VideoEditionProperties.cs \
	gtk-gui/LongoMatch.Gui.Dialog.SnapshotsDialog.cs \
	Gui/SnapshotsDialog.cs \
	Playlist/IPlayList.cs \
	Playlist/PlayList.cs \
	Transcoder.cs \
	gtk-gui/LongoMatch.Gui.Dialog.Transcoder.cs \
	gtk-gui/LongoMatch.Gui.Component.NotesWidget.cs \
	gtk-gui/LongoMatch.Gui.MainWindow.cs \
	Gui/NotesWidget.cs 

DATA_FILES = \
	app.desktop \
	images/lgmlogo.png \
	images/background.png 

RESOURCES = \
	gtk-gui/objects.xml \
	gtk-gui/gui.stetic 

EXTRAS = \
	longomatch.in 

REFERENCES =  \
	Mono.Posix \
	System.Xml \
	$(GTK_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS) \
	Mono.Cairo \
	System \
	$(DB4O_LIBS)

DLL_REFERENCES = 

CLEANFILES = $(COMMONAPPLICATIONDATAROOT_IMAGES) $(PROGRAMFILES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

include $(top_srcdir)/Makefile.include

LONGOMATCH = $(BUILD_DIR)/longomatch
LONGOMATCH_DESKTOP = $(BUILD_DIR)/longomatch.desktop
BACKGROUND_PNG = $(BUILD_DIR)/images/background.png
LGMLOGO_PNG = $(BUILD_DIR)/images/lgmlogo.png

$(eval $(call emit-deploy-target,CESARPLAYER_DLL))
$(eval $(call emit-deploy-wrapper,LONGOMATCH,longomatch,x))
$(eval $(call emit-deploy-target,A_DLL))
$(eval $(call emit-deploy-target,LONGOMATCH_DESKTOP))
$(eval $(call emit-deploy-target,CESARPLAYER_DLL_MDB))
$(eval $(call emit-deploy-target,A_DLL_MDB))
$(eval $(call emit-deploy-target,BACKGROUND_PNG))
$(eval $(call emit-deploy-target,LGMLOGO_PNG))


$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(dir $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
