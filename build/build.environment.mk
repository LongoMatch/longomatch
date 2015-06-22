# Initializers
MONO_BASE_PATH = 
MONO_ADDINS_PATH =

# Install Paths
DEFAULT_INSTALL_DIR = $(pkglibdir)

# Build Paths
DEFAULT_BUILD_DIR = bin

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
LINK_JSON = $(JSON_LIBS)
LINK_NUNIT = $(NUNIT_LIBS)
LINK_MOQ = $(MOQ_LIBS)
LINK_OSXYPLOT = -r:$(DIR_BIN)/OxyPlotMono.dll
LINK_COUCHBASE = $(COUCHBASE_LIBS)
LINK_LONGOMATCH_ADDINS = -r:$(DIR_BIN)/LongoMatch.Addins.dll
LINK_LONGOMATCH_DB = -r:$(DIR_BIN)/LongoMatch.DB.dll
LINK_LONGOMATCH_CORE = -r:$(DIR_BIN)/LongoMatch.Core.dll
LINK_LONGOMATCH_MULTIMEDIA = -r:$(DIR_BIN)/LongoMatch.Multimedia.dll
LINK_LONGOMATCH_GUI_MULTIMEDIA = -r:$(DIR_BIN)/LongoMatch.GUI.Multimedia.dll
LINK_LONGOMATCH_GUI = -r:$(DIR_BIN)/LongoMatch.GUI.dll
LINK_LONGOMATCH_GUI_HELPERS = -r:$(DIR_BIN)/LongoMatch.GUI.Helpers.dll
LINK_LONGOMATCH_DRAWING = -r:$(DIR_BIN)/LongoMatch.Drawing.dll
LINK_LONGOMATCH_DRAWING_CAIRO = -r:$(DIR_BIN)/LongoMatch.Drawing.Cairo.dll
LINK_LONGOMATCH_SERVICES = -r:$(DIR_BIN)/LongoMatch.Services.dll


REF_DEP_LONGOMATCH_ADDINS = \
                     $(LINK_MONO_ADDINS) \
                     $(LINK_JSON) \
                     $(LINK_LONGOMATCH_SERVICES) \
                     $(LINK_LONGOMATCH_CORE)

REF_DEP_LONGOMATCH_PLUGINS = \
                     $(LINK_MONO_ADDINS) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_ADDINS)

REF_DEP_LONGOMATCH_CORE = \
                     $(LINK_SYSTEM_DRAWING) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_JSON) \
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
                     $(LINK_LONGOMATCH_MULTIMEDIA) \
                     $(LINK_LONGOMATCH_GUI_HELPERS) \
                     $(LINK_LONGOMATCH_SERVICES) \
                     $(LINK_LONGOMATCH_DRAWING) \
                     $(LINK_LONGOMATCH_DRAWING_CAIRO)

REF_DEP_LONGOMATCH_GUI_HELPERS = \
                     $(LINK_SYSTEM_DRAWING) \
                     $(LINK_SYSTEM_CORE) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_ATK) \
                     $(LINK_CAIRO) \
                     $(LINK_LONGOMATCH_CORE)

REF_DEP_LONGOMATCH_GUI = \
                     $(LINK_SYSTEM_DRAWING) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_MONO_ADDINS) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_CAIRO) \
                     $(LINK_LONGOMATCH_ADDINS) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_MULTIMEDIA) \
                     $(LINK_LONGOMATCH_GUI_MULTIMEDIA) \
                     $(LINK_LONGOMATCH_GUI_HELPERS) \
                     $(LINK_LONGOMATCH_DRAWING) \
                     $(LINK_LONGOMATCH_DRAWING_CAIRO) \
                     $(LINK_OSXYPLOT)

REF_DEP_LONGOMATCH_DRAWING = \
                     $(LINK_SYSTEM) \
                     $(LINK_SYSTEM_CORE) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GTK) \
                     $(LINK_LONGOMATCH_CORE)

REF_DEP_LONGOMATCH_DRAWING_CAIRO = \
                     $(LINK_SYSTEM) \
                     $(LINK_GTK) \
                     $(LINK_ATK) \
                     $(LINK_CAIRO) \
                     $(LINK_LONGOMATCH_CORE)

REF_DEP_LONGOMATCH_SERVICES = \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_DRAWING) \
                     $(LINK_JSON)

REF_DEP_LONGOMATCH = \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_LONGOMATCH_ADDINS) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_DRAWING_CAIRO) \
                     $(LINK_LONGOMATCH_GUI) \
                     $(LINK_LONGOMATCH_GUI_HELPERS) \
                     $(LINK_LONGOMATCH_GUI_MULTIMEDIA) \
                     $(LINK_LONGOMATCH_MULTIMEDIA) \
                     $(LINK_LONGOMATCH_SERVICES)

REF_DEP_LONGOMATCH_MIGRATION = \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_SYSTEM) \
                     $(LINK_SYSTEM_CORE) \
                     $(LINK_SYSTEM_DRAWING) \
                     $(LINK_DB4O) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_ATK) \
                     $(LINK_DB40) \
                     $(LINK_JSON)

REF_DEP_OXYPLOT = \
                     $(LINK_SYSTEM) \
                     $(LINK_SYSTEM_CORE)

REF_DEP_LONGOMATCH_PLUGINS_GSTREAMER = \
                     $(LINK_MONO_ADDINS) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_ADDINS)

REF_DEP_LONGOMATCH_PLUGINS_STATS = \
                     $(LINK_MONO_ADDINS) \
                     $(LINK_MONO_POSIX) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_CAIRO) \
                     $(LINK_OSXYPLOT) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_GUI) \
                     $(LINK_LONGOMATCH_GUI_HELPERS) \
                     $(LINK_LONGOMATCH_ADDINS)

REF_DEP_LONGOMATCH_DB = \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_JSON) \
                     $(LINK_COUCHBASE) \
                     $(LINK_LONGOMATCH_CORE)

REF_DEP_TESTS = \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_LONGOMATCH_CORE) \
                     $(LINK_LONGOMATCH_SERVICES) \
                     $(LINK_LONGOMATCH_DB) \
                     $(LINK_JSON) \
                     $(LINK_COUCHBASE) \
                     $(LINK_MOQ) \
                     $(LINK_NUNIT)

DIR_BIN = $(top_builddir)/$(DEFAULT_BUILD_DIR)

# Cute hack to replace a space with something
colon:= :
empty:=
space:= $(empty) $(empty)

# Build path to allow running uninstalled
RUN_PATH = $(subst $(space),$(colon), $(MONO_BASE_PATH))

