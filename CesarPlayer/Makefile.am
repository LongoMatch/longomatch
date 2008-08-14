
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ -debug -define:DEBUG
ASSEMBLY = bin/Debug/CesarPlayer.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = module
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

DIRECTSHOWLIB_2005_DLL_SOURCE=DirectShowLib-2005.dll

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+
ASSEMBLY = bin/Release/CesarPlayer.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

DIRECTSHOWLIB_2005_DLL_SOURCE=DirectShowLib-2005.dll

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=longomatch.resources.dll

PROGRAMFILES = \
	$(DIRECTSHOWLIB_2005_DLL)  

LINUX_PKGCONFIG = \
	$(CESARPLAYER_PC)  


	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	AssemblyInfo.cs \
	IPlayer.cs \
	TimeString.cs \
	SignalUtils.cs \
	PlayerMaker.cs \
	VolumeWindow.cs \
	PlayerBin.cs \
	Handlers.cs \
	BufferingHandler.cs \
	ErrorHandler.cs \
	GotRedirectHandler.cs \
	GstError.cs \
	GstPlayerAspectRatio.cs \
	GstPlayerMetadataType.cs \
	GstUseType.cs \
	ObjectManager.cs \
	StateChangedHandler.cs \
	TitleChangeHandler.cs \
	TickHandler.cs \
	GstPlayer.cs \
	gtk-gui/generated.cs \
	IMetadataReader.cs \
	gtk-gui/CesarPlayer.VolumeWindow.cs \
	gtk-gui/CesarPlayer.PlayerBin.cs \
	DSPlayer.cs 

DATA_FILES = 

RESOURCES = \
	gtk-gui/objects.xml \
	gtk-gui/gui.stetic 

EXTRAS = \
	DirectShowLib-2005.dll \
	cesarplayer.pc.in 

REFERENCES =  \
	$(GTK_SHARP_20_LIBS) \
	System \
	Mono.Posix \
	System.Drawing \
	$(GLIB_SHARP_20_LIBS) \
	System.Windows.Forms

DLL_REFERENCES =  \
	DirectShowLib-2005.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

DIRECTSHOWLIB_2005_DLL = $(BUILD_DIR)/DirectShowLib-2005.dll

$(eval $(call emit-deploy-target,DIRECTSHOWLIB_2005_DLL))
$(eval $(call emit-deploy-wrapper,CESARPLAYER_PC,cesarplayer.pc))


$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(dir $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
