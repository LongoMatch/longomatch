# Initializers
MONO_BASE_PATH = 
MONO_ADDINS_PATH =

# Install Paths
DEFAULT_INSTALL_DIR = $(pkglibdir)

# External libraries to link against, generated from configure
LINK_SYSTEM = -r:System
LINK_CAIRO = -r:Mono.Cairo
LINK_MONO_POSIX = -r:Mono.Posix
LINK_MONO_ZEROCONF = $(MONO_ZEROCONF_LIBS)
LINK_GLIB = $(GLIBSHARP_LIBS)
LINK_GTK = $(GTKSHARP_LIBS)
LINK_GCONF = $(GCONFSHARP_LIBS)
LINK_DB40 = $(DB4O_LIBS)
LINK_CESARPLAYER = -r:$(DIR_BIN)/CesarPlayer.dll

REF_DEP_CESARPLAYER = $(LINK_GLIB) \
                      $(LINK_GTK) \
                      $(LINK_MONO_POSIX)

REF_DEP_LONGOMATCH = \
                     $(LINK_MONO_POSIX) \
                     $(LINK_DB40) \
                     $(LINK_GLIB) \
                     $(LINK_GTK) \
                     $(LINK_CAIRO) \
                     $(LINK_CESARPLAYER)

DIR_BIN = $(top_builddir)/bin

# Cute hack to replace a space with something
colon:= :
empty:=
space:= $(empty) $(empty)

# Build path to allow running uninstalled
RUN_PATH = $(subst $(space),$(colon), $(MONO_BASE_PATH))

