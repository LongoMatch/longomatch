UNIQUE_FILTER_PIPE = tr [:space:] \\n | sort | uniq
BUILD_DATA_DIR = $(top_builddir)/bin/share/$(PACKAGE)

SOURCES_BUILD = $(addprefix $(srcdir)/, $(SOURCES))
#SOURCES_BUILD += $(top_srcdir)/src/AssemblyInfo.cs

RESOURCES_EXPANDED = $(addprefix $(srcdir)/, $(RESOURCES))
RESOURCES_BUILD = $(foreach resource, $(RESOURCES_EXPANDED), \
	-resource:$(resource),$(notdir $(resource)))

INSTALL_ICONS = $(top_srcdir)/build/private-icon-theme-installer "$(mkinstalldirs)" "$(INSTALL_DATA)"
THEME_ICONS_SOURCE = $(wildcard $(srcdir)/ThemeIcons/*/*/*.png) $(wildcard $(srcdir)/ThemeIcons/scalable/*/*.svg)
THEME_ICONS_RELATIVE = $(subst $(srcdir)/ThemeIcons/, , $(THEME_ICONS_SOURCE))

BUILD_DIR_RESOLVED = $(firstword $(subst , $(DEFAULT_BUILD_DIR), $(BUILD_DIR)))
BUILD_DIR_ABS = $(top_builddir)/$(BUILD_DIR_RESOLVED)

ASSEMBLY_EXTENSION = $(strip $(patsubst library, dll, $(TARGET)))
ASSEMBLY_FILE := $(top_builddir)/$(BUILD_DIR_RESOLVED)/$(ASSEMBLY).$(ASSEMBLY_EXTENSION)

INSTALL_DIR_RESOLVED = $(firstword $(subst , $(DEFAULT_INSTALL_DIR), $(INSTALL_DIR)))

FILTERED_LINK = $(shell echo "$(LINK)" | $(UNIQUE_FILTER_PIPE))
DEP_LINK = $(shell echo "$(LINK)" | $(UNIQUE_FILTER_PIPE) | sed s,-r:,,g | grep '$(BUILD_DIR_ABS)')

OUTPUT_FILES = \
	$(ASSEMBLY_FILE) \
	$(ASSEMBLY_FILE).mdb

moduledir = $(INSTALL_DIR_RESOLVED)
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

all: $(ASSEMBLY_FILE) theme-icons

run: 
	@pushd $(top_builddir); \
	make run; \
	popd;

test:
	@pushd $(top_builddir)/tests; \
	make $(ASSEMBLY); \
	popd;

build-debug:
	@echo $(DEP_LINK)

$(ASSEMBLY_FILE).mdb: $(ASSEMBLY_FILE)

$(ASSEMBLY_FILE): $(SOURCES_BUILD) $(RESOURCES_EXPANDED) $(DEP_LINK)
	@mkdir -p $(BUILD_DIR_ABS)/migration
	$(AM_V_GEN) $(MCS) \
		$(GMCS_FLAGS) \
		$(ASSEMBLY_BUILD_FLAGS) \
		-nowarn:0278 -nowarn:0078 $$warn -unsafe \
		-define:HAVE_GTK -codepage:utf8 \
		-debug -target:$(TARGET) -out:$@ \
		$(BUILD_DEFINES) $(ENABLE_TESTS_FLAG) $(ENABLE_ATK_FLAG) \
		$(FILTERED_LINK) $(RESOURCES_BUILD) $(SOURCES_BUILD)
	@if [ ! -z "$(EXTRA_BUNDLE)" ]; then \
		cp $(EXTRA_BUNDLE) $(BUILD_DIR_ABS); \
	fi;

theme-icons: $(THEME_ICONS_SOURCE)
	@$(INSTALL_ICONS) -il "$(BUILD_DATA_DIR)" "$(srcdir)" $(THEME_ICONS_RELATIVE)

install-data-hook: $(THEME_ICONS_SOURCE)
	@$(INSTALL_ICONS) -i "$(DESTDIR)$(pkgdatadir)" "$(srcdir)" $(THEME_ICONS_RELATIVE)
	$(EXTRA_INSTALL_DATA_HOOK)

uninstall-hook: $(THEME_ICONS_SOURCE)
	@$(INSTALL_ICONS) -u "$(DESTDIR)$(pkgdatadir)" "$(srcdir)" $(THEME_ICONS_RELATIVE)
	$(EXTRA_UNINSTALL_HOOK)

EXTRA_DIST = $(SOURCES_BUILD) $(RESOURCES_EXPANDED) $(THEME_ICONS_SOURCE) $(IMAGES) $(LOGO) $(LOGO_48) $(desktop_in_files)

CLEANFILES = $(OUTPUT_FILES)
DISTCLEANFILES = *.pidb $(desktop_DATA)
MAINTAINERCLEANFILES = Makefile.in

