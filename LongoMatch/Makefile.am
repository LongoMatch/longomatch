
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG "-main:LongoMatch.MainClass"

ASSEMBLY = bin/Debug/LongoMatch.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../CesarPlayer/bin/Debug/CesarPlayer.exe
BUILD_DIR = bin/Debug

DIRECTSHOWLIB_2005_DLL_SOURCE=../CesarPlayer/DirectShowLib-2005.dll
CESARPLAYER_DLL=
DB4OBJECTS_DB4O_DLL_SOURCE=Db4objects.Db4o.dll
LONGOMATCH_DESKTOP_SOURCE=app.desktop
CESARPLAYER_EXE_SOURCE=../CesarPlayer/bin/Debug/CesarPlayer.exe
CESARPLAYER_EXE=$(BUILD_DIR)/CesarPlayer.exe
BACKGROUND_PNG_SOURCE=images/background.png
CESARPLAYER_EXE_MDB_SOURCE=../CesarPlayer/bin/Debug/CesarPlayer.exe.mdb
CESARPLAYER_EXE_MDB=$(BUILD_DIR)/CesarPlayer.exe.mdb
LGMLOGO_PNG_SOURCE=lgmlogo.png

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-main:LongoMatch.MainClass"

ASSEMBLY = bin/Release/LongoMatch.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = winexe
PROJECT_REFERENCES =  \
	../CesarPlayer/bin/Release/CesarPlayer.dll
BUILD_DIR = bin/Release

DIRECTSHOWLIB_2005_DLL_SOURCE=../CesarPlayer/DirectShowLib-2005.dll
CESARPLAYER_DLL_SOURCE=../CesarPlayer/bin/Release/CesarPlayer.dll
CESARPLAYER_DLL=$(BUILD_DIR)/CesarPlayer.dll
DB4OBJECTS_DB4O_DLL_SOURCE=Db4objects.Db4o.dll
LONGOMATCH_DESKTOP_SOURCE=app.desktop
CESARPLAYER_EXE=
BACKGROUND_PNG_SOURCE=images/background.png
CESARPLAYER_EXE_MDB=
LGMLOGO_PNG_SOURCE=lgmlogo.png

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(DIRECTSHOWLIB_2005_DLL) \
	$(CESARPLAYER_DLL) \
	$(DB4OBJECTS_DB4O_DLL) \
	$(CESARPLAYER_EXE) \
	$(CESARPLAYER_EXE_MDB) \
	$(LGMLOGO_PNG)  

PROGRAMFILES_IMAGES = \
	$(BACKGROUND_PNG)  

LINUX_DESKTOPAPPLICATIONS = \
	$(LONGOMATCH_DESKTOP)  

BINARIES = \
	$(LONGOMATCH)  


	
all: $(ASSEMBLY) $(PROGRAMFILES) $(PROGRAMFILES_IMAGES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

FILES = \
	gtk-gui/generated.cs \
	gtk-gui/LongoMatch.MainWindow.cs \
	AssemblyInfo.cs \
	Main.cs \
	Widgets/ButtonsWidget.cs \
	Widgets/CalendarPopup.cs \
	Widgets/DBManager.cs \
	Widgets/FileDataListWidget.cs \
	Widgets/FileDescriptionWidget.cs \
	Widgets/MainWindow.cs \
	Widgets/NewProjectDialog.cs \
	Widgets/NewSectionsTemplatesFiles.cs \
	Widgets/OpenProjectDialog.cs \
	Widgets/PlayListTreeView.cs \
	Widgets/PlayListWidget.cs \
	Widgets/SectionsPropertiesWidget.cs \
	Widgets/SectionsTemplates.cs \
	Widgets/TemplateEditorDialog.cs \
	Widgets/TimeAdjustWidget.cs \
	Widgets/TimeNodesTreeView.cs \
	Widgets/TreeWidget.cs \
	Widgets/TimeNodeProperties.cs \
	PlayList/PlayList.cs \
	DB/FileData.cs \
	DB/Sections.cs \
	DB/DataBase.cs \
	Handlers/Handlers.cs \
	IO/SectionsWriter.cs \
	IO/SectionsReader.cs \
	PlayList/IPlayList.cs \
	IO/XMLReader.cs \
	DB/MediaFile.cs \
	Widgets/TimeLineWidget.cs \
	Widgets/TimeReferenceWidget.cs \
	Widgets/TimeScale.cs \
	Time/MediaTimeNode.cs \
	Time/PixbufTimeNode.cs \
	Time/SectionsTimeNode.cs \
	Time/TimeNode.cs \
	Time/Time.cs \
	Time/PlayListTimeNode.cs \
	gtk-gui/LongoMatch.Widgets.Component.ButtonsWidget.cs \
	gtk-gui/LongoMatch.Widgets.Component.FileDataListWidget.cs \
	gtk-gui/LongoMatch.Widgets.Dialog.DBManager.cs \
	gtk-gui/LongoMatch.Widgets.Component.FileDescriptionWidget.cs \
	gtk-gui/LongoMatch.Widgets.Dialog.NewProjectDialog.cs \
	gtk-gui/LongoMatch.Widgets.Dialog.OpenProjectDialog.cs \
	gtk-gui/LongoMatch.Widgets.Component.SectionsPropertiesWidget.cs \
	gtk-gui/LongoMatch.Widgets.Dialog.SectionsTemplates.cs \
	gtk-gui/LongoMatch.Widgets.Dialog.TemplateEditorDialog.cs \
	gtk-gui/LongoMatch.Widgets.Component.TimeLineWidget.cs \
	gtk-gui/LongoMatch.Widgets.Component.PlayListWidget.cs \
	gtk-gui/LongoMatch.Widgets.Component.TimeAdjustWidget.cs \
	gtk-gui/LongoMatch.Widgets.Component.TimeNodeProperties.cs \
	gtk-gui/LongoMatch.Widgets.Popup.CalendarPopup.cs \
	gtk-gui/LongoMatch.Widgets.Dialog.NewSectionsTemplatesFiles.cs \
	gtk-gui/LongoMatch.Widgets.Component.TreeWidget.cs \
	Widgets/fakeColorBin.cs \
	gtk-gui/LongoMatch.Widgets.Component.fakeColorBin.cs 

DATA_FILES = \
	app.desktop \
	Db4objects.Db4o.dll \
	lgmlogo.png \
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
	Mono.Cairo

DLL_REFERENCES =  \
	Db4objects.Db4o.dll

CLEANFILES = $(PROGRAMFILES) $(PROGRAMFILES_IMAGES) $(LINUX_DESKTOPAPPLICATIONS) $(BINARIES) 

include $(top_srcdir)/Makefile.include

DIRECTSHOWLIB_2005_DLL = $(BUILD_DIR)/DirectShowLib-2005.dll
LONGOMATCH = $(BUILD_DIR)/longomatch
DB4OBJECTS_DB4O_DLL = $(BUILD_DIR)/Db4objects.Db4o.dll
LONGOMATCH_DESKTOP = $(BUILD_DIR)/longomatch.desktop
BACKGROUND_PNG = $(BUILD_DIR)/images/background.png
LGMLOGO_PNG = $(BUILD_DIR)/lgmlogo.png

$(eval $(call emit-deploy-target,DIRECTSHOWLIB_2005_DLL))
$(eval $(call emit-deploy-target,CESARPLAYER_DLL))
$(eval $(call emit-deploy-wrapper,LONGOMATCH,longomatch,x))
$(eval $(call emit-deploy-target,DB4OBJECTS_DB4O_DLL))
$(eval $(call emit-deploy-target,LONGOMATCH_DESKTOP))
$(eval $(call emit-deploy-target,CESARPLAYER_EXE))
$(eval $(call emit-deploy-target,BACKGROUND_PNG))
$(eval $(call emit-deploy-target,CESARPLAYER_EXE_MDB))
$(eval $(call emit-deploy-target,LGMLOGO_PNG))


$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(dir $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
