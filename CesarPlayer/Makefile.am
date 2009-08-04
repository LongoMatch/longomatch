
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = $(GMCS)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = a.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = .

CESARPLAYER_DLL_MDB=
A_DLL_MDB_SOURCE=a.dll.mdb
A_DLL_MDB=$(BUILD_DIR)/a.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = $(GMCS)
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -unsafe -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = bin/Release/CesarPlayer.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

CESARPLAYER_DLL_MDB_SOURCE=bin/Release/CesarPlayer.dll.mdb
CESARPLAYER_DLL_MDB=$(BUILD_DIR)/CesarPlayer.dll.mdb
A_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(CESARPLAYER_DLL_MDB) \
	$(A_DLL_MDB)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) 

FILES = \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
	Handlers/ErrorHandler.cs \
	Handlers/Handlers.cs \
	Handlers/TickHandler.cs \
	Player/GstPlayer.cs \
	Player/IPlayer.cs \
	Player/GstPlayerAspectRatio.cs \
	Player/GstUseType.cs \
	Handlers/StateChangedHandler.cs \
	Utils/TimeString.cs \
	gtk-gui/LongoMatch.Gui.PlayerBin.cs \
	gtk-gui/LongoMatch.Gui.VolumeWindow.cs \
	Gui/VolumeWindow.cs \
	Capturer/ErrorHandler.cs \
	Capturer/ICapturer.cs \
	Gui/CapturerBin.cs \
	gtk-gui/LongoMatch.Gui.CapturerBin.cs \
	MultimediaFactory.cs \
	Player/ObjectManager.cs \
	Capturer/ObjectManager.cs \
	Utils/IFramesCapturer.cs \
	Utils/FramesCapturer.cs \
	Utils/IMetadataReader.cs \
	Utils/GstPlayerMetadataType.cs \
	Gui/PlayerBin.cs \
	Capturer/GccAudioEncoderType.cs \
	Capturer/GccError.cs \
	Capturer/GccVideoEncoderType.cs \
	Capturer/GccVideoMuxerType.cs \
	Capturer/GstCameraCapturer.cs \
	Editor/ErrorHandler.cs \
	Editor/IVideoEditor.cs \
	Editor/AudioQuality.cs \
	Editor/VideoQuality.cs \
	Editor/PercentCompletedHandler.cs \
	Editor/GstVideoSplitter.cs \
	Editor/GnonlinEditor.cs \
	Editor/VideoSegment.cs \
	Editor/EditorState.cs \
	Editor/VideoFormat.cs \
	Editor/VideoMuxer.cs \
	Editor/VideoCodec.cs \
	Editor/IMerger.cs \
	Editor/MatroskaMerger.cs \
	Editor/AudioCodec.cs \
	Editor/IVideoSplitter.cs \
	Editor/VideoSplitterType.cs \
	Editor/ConcatMerger.cs \
	Editor/GenericMerger.cs \
	Editor/AviMerger.cs \
	Utils/MediaFile.cs \
	Editor/AvidemuxMerger.cs 

DATA_FILES = 

RESOURCES = \
	gtk-gui/objects.xml \
	gtk-gui/gui.stetic 

EXTRAS = \
	cesarplayer.key 

REFERENCES =  \
	System \
	Mono.Posix \
	System.Drawing \
	$(GTK_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS)

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) 

include $(top_srcdir)/Makefile.include




$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
