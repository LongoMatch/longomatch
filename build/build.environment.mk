# Initializers
MONO_BASE_PATH = 
MONO_ADDINS_PATH =

# Install Paths
DEFAULT_INSTALL_DIR = $(pkglibdir)

# External libraries to link against, generated from configure
LINK_SYSTEM = -r:System
LINK_SYSTEM_CORE = -r:System.Core
LINK_SYSTEM_DRAWING = -r:System.Drawing
LINK_CAIRO = -r:Mono.Cairo
LINK_MONO_POSIX = -r:Mono.Posix
LINK_MONO_ADDINS = $(MONO_ADDINS_LIBS)
LINK_MONO_ZEROCONF = $(MONO_ZEROCONF_LIBS)
LINK_GLIB = $(GLIBSHARP_LIBS)
LINK_GTK = $(GTKSHARP_LIBS)
LINK_GCONF = $(GCONFSHARP_LIBS)
LINK_DB40 = $(DB4O_LIBS)
LINK_LONGOMATCH_ADDINS = -r:$(DIR_BIN)/LongoMatch.Addins.dll
LINK_LONGOMATCH_CORE = -r:$(DIR_BIN)/LongoMatch.dll
LINK_LONGOMATCH_MULTIMEDIA = -r:$(DIR_BIN)/LongoMatch.Multimedia.dll
LINK_LONGOMATCH_GUI_MULTIMEDIA = -r:$(DIR_BIN)/LongoMatch.GUI.Multimedia.dll
LINK_LONGOMATCH_GUI = -r:$(DIR_BIN)/LongoMatch.GUI.dll
LINK_LONGOMATCH_SERVICES = -r:$(DIR_BIN)/LongoMatch.Services.dll


REF_DEP_LONGOMATCH_ADDINS = \
                     $(LINK_MONO_ADDINS) \
                     $(LINK_LONGOMATCH_CORE)

REF_DEP_LONGOMATCH_CORE = \
                     $(LINK_SYSTEM_DRAWING) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK)

REF_DEP_LONGOMATCH_MULTIMEDIA = \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_LONGOMATCH_CORE)

REF_DEP_LONGOMATCH_GUI_MULTIMEDIA = \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_MULTIMEDIA)

REF_DEP_LONGOMATCH_GUI = \
                     $(LINK_SYSTEM_DRAWING) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_CAIRO) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_MULTIMEDIA) \
                     $(LINK_LONGOMATCH_GUI_MULTIMEDIA)

REF_DEP_LONGOMATCH_SERVICES = \
                     $(LINK_MONO_POSIX) \
                     $(LINK_DB40) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_CAIRO) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_MULTIMEDIA) \
                     $(LINK_LONGOMATCH_GUI) \
                     $(LINK_LONGOMATCH_GUI_MULTIMEDIA)

REF_DEP_LONGOMATCH = \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_LONGOMATCH_ADDINS) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_MULTIMEDIA) \
                     $(LINK_LONGOMATCH_SERVICES) \
                     $(LINK_LONGOMATCH_GUI)


DIR_BIN = $(top_builddir)/bin

# Cute hack to replace a space with something
colon:= :
empty:=
space:= $(empty) $(empty)

# Build path to allow running uninstalled
RUN_PATH = $(subst $(space),$(colon), $(MONO_BASE_PATH))

