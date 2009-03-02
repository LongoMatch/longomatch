
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = a.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = .


endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = bin/Release/CesarPlayer.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release


endif

AL=al2
SATELLITE_ASSEMBLY_NAME=longomatch.resources.dll


	
all: $(ASSEMBLY) 

FILES = \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
	Handlers/ErrorHandler.cs \
	Handlers/GotRedirectHandler.cs \
	Handlers/Handlers.cs \
	Handlers/TickHandler.cs \
	Handlers/TitleChangeHandler.cs \
	Handlers/BufferingHandler.cs \
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
	Capturer/GstVideoCapturer.cs \
	Capturer/GvcAudioEncoderType.cs \
	Capturer/GvcUseType.cs \
	Capturer/GvcVideoEncoderType.cs \
	Capturer/ICapturer.cs \
	Gui/CapturerBin.cs \
	gtk-gui/LongoMatch.Gui.CapturerBin.cs \
	PlayerMaker.cs \
	Player/ObjectManager.cs \
	Capturer/ObjectManager.cs \
	Utils/IFramesCapturer.cs \
	Utils/FramesCapturer.cs \
	Utils/IMetadataReader.cs \
	Utils/GstPlayerMetadataType.cs \
	Gui/PlayerBin.cs 

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

CLEANFILES = 

include $(top_srcdir)/Makefile.include




$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(dir $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
