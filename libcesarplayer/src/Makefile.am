## Process this file with automake to produce Makefile.in


AM_CPPFLAGS = \
	-DPACKAGE_LOCALE_DIR=\""../LongoMatch/bin/Release"\" \
	-DPACKAGE_SRC_DIR=\""$(srcdir)"\" \
	-DPACKAGE_DATA_DIR=\""$(datadir)"\" \
	$(CESARPLAYER_CFLAGS)

AM_CFLAGS =\
	 -Wall\
	 -g


pkglib_LTLIBRARIES = \
	libcesarplayer.la

libcesarplayer_la_SOURCES = \
	bacon-video-widget.h\
	bacon-video-widget-gst-0.10.c\
	baconvideowidget-marshal.c \
	baconvideowidget-marshal.h \
	gstscreenshot.c \
	gstscreenshot.h \
	gst-video-capturer.c \
	gst-video-capturer.h \
	gst-video-editor.c\
	gst-video-editor.h\
	bacon-resize.c\
	bacon-resize.h\
	video-utils.c\
	video-utils.h\
	macros.h 

libcesarplayer_la_LDFLAGS = \
	$(CESARPLAYER_LIBS)
