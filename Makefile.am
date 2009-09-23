
EXTRA_DIST =  expansions.m4 

#Warning: This is an automatically generated file, do not edit!
if ENABLE_DEBUG
 SUBDIRS =  libcesarplayer Translations CesarPlayer LongoMatch
endif
if ENABLE_RELEASE
 SUBDIRS =  libcesarplayer Translations CesarPlayer LongoMatch
endif

# Build ChangeLog from GIT history
ChangeLog:
	@if test -f $(top_srcdir)/.git/HEAD; then \
		git log --pretty=format:'%ad %an <%ae>%n%s ' --stat --after="Jul 01 23:47:57 2009" > $@; \
	fi

dist: ChangeLog

.PHONY: ChangeLog

