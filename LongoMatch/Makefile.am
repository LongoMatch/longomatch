ASSEMBLY = LongoMatchGtk
TARGET = exe
LINK = $(REF_DEP_LONGOMATCH)

SOURCES = \
	AssemblyInfo.cs \
	Main.cs

IMAGES = $(top_srcdir)/images/longomatch.png \
         $(top_srcdir)/images/background.png

LOGO = $(top_srcdir)/images/logo_48x48.png

bin_SCRIPTS = longomatch

DESKTOP_FILE = longomatch.desktop.in

include $(top_srcdir)/build/build.mk

EXTRA_DIST += \
	longomatch.in\
	AssemblyInfo.cs.in

