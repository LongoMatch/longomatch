
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!




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
BACKGROUND_PNG_SOURCE=images/background.png
LGMLOGO_PNG_SOURCE=images/lgmlogo.png



AL=al2
SATELLITE_ASSEMBLY_NAME=.resources.dll

PROGRAMFILES = \
	$(DIRECTSHOWLIB_2005_DLL) \
	$(CESARPLAYER_DLL) \
	$(DB4OBJECTS_DB4O_DLL) 
	
	  

PROGRAMFILES_IMAGES = \
	$(BACKGROUND_PNG) \
	$(LGMLOGO_PNG)  

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
	EventsManager.cs \
	Gui/ButtonsWidget.cs \
	Gui/CalendarPopup.cs \
	Gui/DBManager.cs \
	Gui/ProjectListWidget.cs \
	Gui/FileDescriptionWidget.cs \
	Gui/MainWindow.cs \
	Gui/NewProjectDialog.cs \
	Gui/NewSectionsTemplatesFiles.cs \
	Gui/OpenProjectDialog.cs \
	Gui/PlayListTreeView.cs \
	Gui/PlayListWidget.cs \
	Gui/SectionsPropertiesWidget.cs \
	Gui/SectionsTemplates.cs \
	Gui/TemplateEditorDialog.cs \
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
	Video/AudioQuality.cs \
	Video/FFMPEGVideoEditor.cs \
	Video/IPlayList.cs \
	Video/IVideoEditor.cs \
	Video/PlayList.cs \
	Video/VideoQuality.cs \
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
	gtk-gui/LongoMatch.Gui.Component.ButtonsWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.ProjectListWidget.cs \
	gtk-gui/LongoMatch.Gui.Dialog.DBManager.cs \
	gtk-gui/LongoMatch.Gui.Component.FileDescriptionWidget.cs \
	gtk-gui/LongoMatch.Gui.Dialog.NewProjectDialog.cs \
	gtk-gui/LongoMatch.Gui.Dialog.OpenProjectDialog.cs \
	gtk-gui/LongoMatch.Gui.Component.SectionsPropertiesWidget.cs \
	gtk-gui/LongoMatch.Gui.Dialog.SectionsTemplates.cs \
	gtk-gui/LongoMatch.Gui.Dialog.TemplateEditorDialog.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeLineWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.PlayListWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeAdjustWidget.cs \
	gtk-gui/LongoMatch.Gui.Component.TimeNodeProperties.cs \
	gtk-gui/LongoMatch.Gui.Popup.CalendarPopup.cs \
	gtk-gui/LongoMatch.Gui.Dialog.NewSectionsTemplatesFiles.cs \
	gtk-gui/LongoMatch.Gui.Component.TreeWidget.cs 
	

DATA_FILES = \
	app.desktop \
	Db4objects.Db4o.dll \
	images/lgmlogo.png \
	images/background.png 

RESOURCES = \
	gtk-gui/objects.xml \
	gtk-gui/gui.stetic 

EXTRAS = \
	longomatch.in 

REFERENCES =  \
	System \
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
LGMLOGO_PNG = $(BUILD_DIR)/images/lgmlogo.png

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
