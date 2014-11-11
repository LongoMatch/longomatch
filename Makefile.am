EXTRA_DIST =\
		expansions.m4 \
		env.in

ACLOCAL_AMFLAGS = -I build/m4 -I build/m4/shamrock

SUBDIRS = \
	build \
	data \
	images \
	pkgconfig \
	libcesarplayer \
	oxyplot \
	LongoMatch.Core \
	LongoMatch.Addins \
	LongoMatch.Multimedia \
	LongoMatch.Drawing \
	LongoMatch.Drawing.Cairo \
	LongoMatch.GUI.Helpers \
	LongoMatch.GUI.Multimedia \
	LongoMatch.GUI \
	LongoMatch.Plugins \
	LongoMatch.Plugins.Stats \
	LongoMatch.Services \
	LongoMatch \
	po

if WITH_MIGRATION_TOOL
SUBDIRS += LongoMatch.Migration
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

