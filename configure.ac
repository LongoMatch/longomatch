dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([LongoMatch], [1.0.2])
AM_INIT_AUTOMAKE([1.11 foreign])
AM_SILENT_RULES([yes])
AM_MAINTAINER_MODE([enable])

AC_CONFIG_MACRO_DIR([build/m4])
AC_SUBST([ACLOCAL_AMFLAGS], ["-I build/m4/shamrock \${ACLOCAL_FLAGS}"])

AC_PROG_CC
AC_PROG_OBJC
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

#*******************************************************************************
# Internationalization
#*******************************************************************************
GETTEXT_PACKAGE=longomatch
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE,"$GETTEXT_PACKAGE", [GETTEXT package name])

dnl Check for gettext utils
AC_PATH_PROG(MSGFMT, msgfmt, no)
if test "x$MSGFMT" = "xno"; then
	AC_MSG_ERROR([gettext not found])
else 
	AC_SUBST(MSGFMT,[msgfmt])
fi

IT_PROG_INTLTOOL([0.40.0])
AM_GLIB_GNU_GETTEXT


dnl Mono and C# compiler
dnl Check first for a 4.0 compiler or than fallback to 2.0
SHAMROCK_CHECK_MONO_MODULE(2.4.0)
PKG_CHECK_EXISTS([mono >= 2.8],
		[SHAMROCK_FIND_MONO_4_0_COMPILER
		SHAMROCK_CHECK_MONO_4_0_GAC_ASSEMBLIES([
			System.Data
			Mono.Cairo
			Mono.Posix
		])]
,
		[SHAMROCK_FIND_MONO_2_0_COMPILER
		SHAMROCK_CHECK_MONO_2_0_GAC_ASSEMBLIES([
			System.Data
			Mono.Cairo
			Mono.Posix
		])])
AC_SUBST(MCS, ["$MCS"])
SHAMROCK_FIND_MONO_RUNTIME

dnl NUnit (optional)
SHAMROCK_CHECK_NUNIT

dnl Moq (optional) disable test if not found
PKG_CHECK_MODULES(MOQ, moq,
	do_tests="yes", do_tests="no")
AC_SUBST(MOQ_LIBS)
AM_CONDITIONAL(ENABLE_TESTS, test "x$do_tests" = "xyes")
if test "x$do_tests" = "xno"; then
	AC_MSG_WARN([Could not find moq: tests will not be available])
fi


dnl Covem (optional)
AC_PATH_PROG(COVEM, covem, no)
AC_PATH_PROG(COV_GTK, cov-gtk, no)
AC_PATH_PROG(COV_HTML, cov-html, no)
AM_CONDITIONAL(HAVE_COVEM, test x$COVEM != xno)

dnl package checks, common for all configs
PKG_CHECK_MODULES([GLIBSHARP], [glib-sharp-2.0])
AC_SUBST(GLIBSHARP_LIBS)
PKG_CHECK_MODULES([GTKSHARP], [gtk-sharp-2.0])
AC_SUBST(GTKSHARP_LIBS)
PKG_CHECK_MODULES([JSON], [newtonsoft-json])
AC_SUBST(JSON_LIBS)
PKG_CHECK_MODULES([DB4O], [db4o],  [HAVE_DB4O=1], [HAVE_DB4O=0])
AC_SUBST(DB4O_LIBS)
PKG_CHECK_MODULES([MONO_ADDINS], [mono-addins])
AC_SUBST(MONO_ADDINS_LIBS)
PKG_CHECK_MODULES([COUCHBASE], [couchbase-lite])
AC_SUBST(COUCHBASE_LIBS)
COUCHBASE_LIBS_PATHS=`echo $COUCHBASE_LIBS | sed 's/-r://g'`
AC_SUBST(COUCHBASE_LIBS_PATHS)

AM_CONDITIONAL(WITH_MIGRATION_TOOL, [test "$HAVE_DB4O" -eq 1])

dnl package checks for libcesarplayer
PKG_CHECK_MODULES(CESARPLAYER, [gtk+-2.0 >= 2.8 gdk-2.0 gio-2.0 glib-2.0 gstreamer-0.10 gstreamer-audio-0.10 gstreamer-video-0.10 gstreamer-pbutils-0.10 gobject-2.0 gstreamer-interfaces-0.10 gstreamer-tag-0.10 gstreamer-app-0.10])
AC_SUBST(CESARPLAYER_CFLAGS)
AC_SUBST(CESARPLAYER_LIBS)

AC_MSG_CHECKING([for the OS type])
ostype=""

case "$host" in
  *-*-mingw*|*-*-cygwin*)
    ostype=Windows
    AC_DEFINE(OSTYPE_WINDOWS)
    GMCS_FLAGS="$GMCS_FLAGS -define:OSTYPE_WINDOWS"
    ;;
  *-*-darwin*)
    AC_DEFINE(OSTYPE_OS_X)
    ostype=OS_X
    GMCS_FLAGS="$GMCS_FLAGS -define:OSTYPE_OS_X"
    ;;
  *-*-linux*)
    AC_DEFINE(OSTYPE_LINUX)
    GMCS_FLAGS="$GMCS_FLAGS -define:OSTYPE_LINUX"
    ostype=Linux
    ;;
esac
AM_CONDITIONAL(OSTYPE_WINDOWS, test x$ostype = xWindows)
AM_CONDITIONAL(OSTYPE_OS_X, test x$ostype = xOS_X)
AM_CONDITIONAL(OSTYPE_LINUX, test x$ostype = xLinux)
AC_SUBST(GMCS_FLAGS, ["$GMCS_FLAGS"])
AC_MSG_RESULT($ostype)

dnl package checks, per config

AC_CONFIG_FILES([env], [chmod +x env])
AC_CONFIG_FILES([
Makefile

build/Makefile
build/m4/Makefile

images/Makefile
libcesarplayer/Makefile

oxyplot/Makefile
pkgconfig/Makefile
pkgconfig/longomatch-addins.pc
pkgconfig/longomatch-core.pc
pkgconfig/longomatch-drawing.pc
pkgconfig/longomatch-drawing-cairo.pc
pkgconfig/longomatch-gui.pc
pkgconfig/longomatch-gui-helpers.pc
pkgconfig/longomatch-oxyplot.pc

AssemblyInfo/AssemblyInfo.cs
LongoMatch.Addins/Makefile
LongoMatch.DB/Makefile
LongoMatch.Core/Makefile
LongoMatch.Drawing/Makefile
LongoMatch.Drawing.Cairo/Makefile
LongoMatch.Migration/Makefile
LongoMatch.Multimedia/Makefile
LongoMatch.GUI.Helpers/Makefile
LongoMatch.GUI.Multimedia/Makefile
LongoMatch.GUI/Makefile
LongoMatch.Plugins/Makefile
LongoMatch.Plugins.GStreamer/Makefile
LongoMatch.Plugins.Stats/Makefile
LongoMatch.Services/Makefile
LongoMatch/Makefile
LongoMatch/longomatch
LongoMatch/longomatch.desktop.in
Tests/Makefile
tools/deb-pkg.sh

data/Makefile
data/icons/Makefile
data/images/Makefile
data/theme/Makefile

po/Makefile.in
])

AC_OUTPUT
