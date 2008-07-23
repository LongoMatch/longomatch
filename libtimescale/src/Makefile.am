## Process this file with automake to produce Makefile.in

## Created by Anjuta

AM_CPPFLAGS = \
	-DPACKAGE_LOCALE_DIR=\""$(prefix)/$(DATADIRNAME)/locale"\" \
	-DPACKAGE_SRC_DIR=\""$(srcdir)"\" \
	-DPACKAGE_DATA_DIR=\""$(datadir)"\" $(LIBTIMESCALE_CFLAGS)

AM_CFLAGS =\
	 -Wall\
	 -g

lib_LTLIBRARIES = \
	libtimescale.la

libtimescale_la_SOURCES = \
	gtkenhancedscale.c\
	gtkenhancedscale.h\
	gtk-timescale.c  \
	gtk-timescale.h

libtimescale_la_LIBADD = \
	$(LIBTIMESCALE_LIBS)

