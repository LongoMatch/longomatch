EXTRA_DIST =\
		expansions.m4 \
		env.in

SUBDIRS = \
	build \
	images \
	libcesarplayer \
	LongoMatch.Core \
	LongoMatch.Addins \
	LongoMatch.Multimedia \
	LongoMatch.GUI.Multimedia \
	LongoMatch.GUI \
	LongoMatch.Services \
	LongoMatch \
	po

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

