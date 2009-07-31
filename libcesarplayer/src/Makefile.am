## Process this file with automake to produce Makefile.in

## Created by Anjuta

AM_CPPFLAGS = \
	-DPACKAGE_LOCALE_DIR=\""../LongoMatch/bin/Release"\" \
	-DPACKAGE_SRC_DIR=\""$(srcdir)"\" \
	-DPACKAGE_DATA_DIR=\""$(datadir)"\" \
	$(CESARPLAYER_CFLAGS)

AM_CFLAGS =\
	 -Wall\
	 -g


lib_LTLIBRARIES = \
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
	gst-video-splitter.c\
	gst-smart-video-scaler.c\
	gst-smart-video-scaler.c\
	gst-video.splitter.h\
	gstvideowidget.c \
	gstvideowidget.h \
	macros.h 

libcesarplayer_la_LDFLAGS = \
	$(CESARPLAYER_LIBS)
