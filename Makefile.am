
EXTRA_DIST =  expansions.m4 

#Warning: This is an automatically generated file, do not edit!
if ENABLE_DEBUG
 SUBDIRS =  libcesarplayer CesarPlayer LongoMatch po
endif
if ENABLE_RELEASE
 SUBDIRS =  libcesarplayer CesarPlayer LongoMatch po
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

