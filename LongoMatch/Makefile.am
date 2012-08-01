ASSEMBLY = LongoMatchGtk
TARGET = exe
LINK = $(REF_DEP_LONGOMATCH)

SOURCES = \
	AssemblyInfo.cs \
	Main.cs

IMAGES = $(top_srcdir)/images/longomatch.png \
         $(top_srcdir)/images/background.png

LOGO_48 = $(top_srcdir)/images/48x48/longomatch.png
LOGO = $(top_srcdir)/images/scalable/longomatch.svg

bin_SCRIPTS = longomatch

DESKTOP_FILE = longomatch.desktop.in

include $(top_srcdir)/build/build.mk

EXTRA_DIST += \
	longomatch.in\
	AssemblyInfo.cs.in

