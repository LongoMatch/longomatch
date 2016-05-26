SUBDIRS = VAS

EXTRA_DIST =\
	$(top_srcdir)/.version \
	$(top_srcdir)/expansions.m4 \
	$(top_srcdir)/env.in

BUILT_SOURCES = $(top_srcdir)/.version

$(top_srcdir)/.version:
	echo $(VERSION) > $@-t && mv $@-t $@

dist-hook:
	echo $(VERSION) > $(distdir)/.tarball-version

ACLOCAL_AMFLAGS = -I build/m4 -I build/m4/shamrock

SUBDIRS += \
	build \
	data \
	images \
	pkgconfig \
	libcesarplayer \
	.nuget \
	oxyplot \
	LongoMatch.Core \
	LongoMatch.DB \
	LongoMatch.Drawing \
	LongoMatch.Services \
	LongoMatch.Addins \
	LongoMatch.GUI.Multimedia \
	LongoMatch.GUI \
	LongoMatch.Plugins \
	LongoMatch.Plugins.GStreamer \
	LongoMatch.Plugins.Stats \
	LongoMatch \
	po

if WITH_MIGRATION_TOOL
SUBDIRS += LongoMatch.Migration
endif

if ENABLE_TESTS
SUBDIRS += Tests
endif

DISTCLEANFILES = 	intltool-extract\
			intltool-merge\
			intltool-update

# Build ChangeLog from GIT history
ChangeLog:
	@if test -f $(top_srcdir)/.git/HEAD; then \
		git log --pretty=format:'%ad %an <%ae>%n *%s ' --stat --after="Jul 01 23:47:57 2009" > $@; \
	fi

dist: ChangeLog

.PHONY: ChangeLog

include $(top_srcdir)/build/build_env.mk
install-data-hook:
	$(INSTALL) $(LM_DEPS) $(pkglibdir)
