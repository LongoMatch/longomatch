dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([LongoMatch], [0.5.2])
AM_INIT_AUTOMAKE([foreign])

AC_ISC_POSIX
AC_PROG_CC
AC_C_CONST
AM_PROG_CC_STDC
AC_HEADER_STDC



AM_PROG_LIBTOOL

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

AC_PATH_PROG(GMCS, gmcs, no)
if test "x$GMCS" = "xno"; then
        AC_MSG_ERROR([gmcs Not found])
fi


AC_ARG_ENABLE(debug,
	AC_HELP_STRING([--enable-debug],
		[Use 'DEBUG' Configuration [default=NO]]),
		enable_debug=yes, enable_debug=no)
AM_CONDITIONAL(ENABLE_DEBUG, test x$enable_debug = xyes)
if test "x$enable_debug" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
AC_ARG_ENABLE(release,
	AC_HELP_STRING([--enable-release],
		[Use 'RELEASE' Configuration [default=YES]]),
		enable_release=yes, enable_release=no)
AM_CONDITIONAL(ENABLE_RELEASE, test x$enable_release = xyes)
if test "x$enable_release" = "xyes" ; then
	CONFIG_REQUESTED="yes"
fi
if test -z "$CONFIG_REQUESTED" ; then
	AM_CONDITIONAL(ENABLE_RELEASE, true)
	enable_release=yes
fi


dnl package checks, common for all configs
PKG_CHECK_MODULES([GLIB_SHARP_20], [glib-sharp-2.0])
PKG_CHECK_MODULES([GTK_SHARP_20], [gtk-sharp-2.0])


PKG_CHECK_MODULES(CESARPLAYER, [gtk+-2.0 >= 2.8 gdk-2.0 gio-2.0 glib-2.0 gstreamer-0.10 gstreamer-audio-0.10 gstreamer-video-0.10 gobject-2.0 gstreamer-interfaces-0.10])
AC_SUBST(CESARPLAYER_CFLAGS)
AC_SUBST(CESARPLAYER_LIBS)

# Checks for library functions.
AC_CHECK_FUNCS([strchr strstr])

dnl package checks, per config


AC_CONFIG_FILES([
LongoMatch/Makefile
CesarPlayer/Makefile
libcesarplayer/Makefile
libcesarplayer/src/Makefile
LongoMatch/longomatch
Makefile
CesarPlayer/cesarplayer.pc
Translations/Makefile

])


AC_OUTPUT
