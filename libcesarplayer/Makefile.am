## Process this file with automake to produce Makefile.in


AM_CPPFLAGS = \
	-DPACKAGE_SRC_DIR=\""$(srcdir)"\" \
	-DPACKAGE_DATA_DIR=\""$(datadir)"\" \
	$(CESARPLAYER_CFLAGS)

if OSTYPE_OS_X
AM_CPPFLAGS += \
	-x objective-c
else
AM_CPPFLAGS += \
	-x c
endif

AM_CFLAGS =\
	 -Wall\
	 -g

BVWMARSHALFILES = baconvideowidget-marshal.c baconvideowidget-marshal.h
GLIB_GENMARSHAL=`pkg-config --variable=glib_genmarshal glib-2.0`
BUILT_SOURCES = $(BVWMARSHALFILES)

baconvideowidget-marshal.h: baconvideowidget-marshal.list
	( $(GLIB_GENMARSHAL) --prefix=baconvideowidget_marshal $(srcdir)/baconvideowidget-marshal.list --header > baconvideowidget-marshal.h )
baconvideowidget-marshal.c: baconvideowidget-marshal.h
	( $(GLIB_GENMARSHAL) --prefix=baconvideowidget_marshal $(srcdir)/baconvideowidget-marshal.list --body --header > baconvideowidget-marshal.c )


lib_LTLIBRARIES = \
	libcesarplayer.la

libcesarplayer_la_SOURCES = \
	$(BVWMARSHALFILES) \
	lgm-gtk-glue.c\
	lgm-gtk-glue.h\
	lgm-video-player.h\
	lgm-video-player.c\
	lgm-device.h\
	lgm-device.c\
	gstscreenshot.c \
	gstscreenshot.h \
	gst-camera-capturer.c\
	gst-camera-capturer.h\
	gst-remuxer.c\
	gst-remuxer.h\
	gst-video-editor.c\
	gst-video-editor.h\
	gst-nle-source.c\
	gst-nle-source.h\
	lgm-utils.m\
	lgm-utils.h

libcesarplayer_la_LDFLAGS = \
	$(CESARPLAYER_LIBS)

if OSTYPE_WINDOWS
libcesarplayer_la_LDFLAGS += -no-undefined
endif
	
CLEANFILES = $(BUILT_SOURCES)
	
EXTRA_DIST = \
	baconvideowidget-marshal.list


if WITH_MULTIMEDIA_TOOLS
noinst_PROGRAMS = test-capturer test-discoverer test-remuxer test-editor

test_capturer_SOURCES = test-capturer.c
test_capturer_LDADD =  libcesarplayer.la $(CESARPLAYER_LIBS)

test_discoverer_SOURCES = test-discoverer.c
test_discoverer_LDADD =  libcesarplayer.la $(CESARPLAYER_LIBS)

test_remuxer_SOURCES = test-remuxer.c
test_remuxer_LDADD =  libcesarplayer.la $(CESARPLAYER_LIBS)

test_editor_SOURCES = test-editor.c
test_editor_LDADD =  libcesarplayer.la $(CESARPLAYER_LIBS)
endif
