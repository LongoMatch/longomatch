BUILD_DIR ?= $(top_builddir)/bin
ASSEMBLY_FILE ?= $(BUILD_DIR)/$(ASSEMBLY)
OUTPUT_DIR ?= $(pkglibdir)


XBUILD_CMD = $(XBUILD) \
 /property:DefineConstants="$(XBUILD_CONSTANTS)" \
 /property:BuildType=makefiles \
 /property:BuildProjectReferences=false \
 /property:OutDir=$(BUILD_DIR)/ \
 $(PROJECT_FILE) $(XBUILD_EXTRA_FLAGS)

OUTPUT_FILES ?= \
	$(ASSEMBLY_FILE) \
	$(ASSEMBLY_FILE).mdb

.PHONY: $(ASSEMBLY_FILE)

all: $(ASSEMBLY_FILE)

$(ASSEMBLY_FILE):
	$(AM_V_GEN) $(XBUILD_CMD)
	@if [ ! -z "$(EXTRA_BUNDLE)" ]; then \
		cp $(EXTRA_BUNDLE) $(BUILD_DIR); \
	fi;

clean-local:
	$(AM_V_GEN) $(XBUILD_CMD) /t:Clean
	rm -rf obj/

moduledir = $(OUTPUT_DIR)
module_SCRIPTS = $(OUTPUT_FILES) $(DLLCONFIG)

@INTLTOOL_DESKTOP_RULE@
desktopdir = $(datadir)/applications
desktop_in_files = $(DESKTOP_FILE)
desktop_DATA = $(desktop_in_files:.desktop.in=.desktop)

imagesdir = @datadir@/@PACKAGE@/images
images_DATA = $(IMAGES)

logo_48dir = @datadir@/icons/hicolor/48x48/apps
logo_48_DATA = $(LOGO_48)

logodir = @datadir@/icons/hicolor/scalable/apps
logo_DATA = $(LOGO)

CLEANFILES = $(OUTPUT_FILES)
MAINTAINERCLEANFILES = Makefile.in
