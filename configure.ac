dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([LongoMatch], [0.17.6])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

AC_CONFIG_MACRO_DIR([build/m4])
AC_SUBST([ACLOCAL_AMFLAGS], ["-I build/m4/shamrock -I build/m4/shave \${ACLOCAL_FLAGS}"])

AM_PROG_CC_STDC
AC_ISC_POSIX
AC_PROG_CC
AC_C_CONST
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

dnl package checks, common for all configs
PKG_CHECK_MODULES([GLIBSHARP], [glib-sharp-2.0])
AC_SUBST(GLIBSHARP_LIBS)
PKG_CHECK_MODULES([GTKSHARP], [gtk-sharp-2.0])
AC_SUBST(GTKSHARP_LIBS)
PKG_CHECK_MODULES([DB4O], [db4o])
AC_SUBST(DB4O_LIBS)
PKG_CHECK_MODULES([MONO_ADDINS], [mono-addins])
AC_SUBST(MONO_ADDINS_LIBS)

dnl package checks for libcesarplayer
PKG_CHECK_MODULES(CESARPLAYER, [gtk+-2.0 >= 2.8 gdk-2.0 gio-2.0 glib-2.0 gstreamer-0.10 gstreamer-audio-0.10 gstreamer-video-0.10 gstreamer-pbutils-0.10 gobject-2.0 gstreamer-interfaces-0.10])
AC_SUBST(CESARPLAYER_CFLAGS)
AC_SUBST(CESARPLAYER_LIBS)

#SHAVE_INIT([build/m4/shave], [enable])

dnl package checks, per config

AC_CONFIG_FILES([env], [chmod +x env])
AC_CONFIG_FILES([
Makefile

build/Makefile
build/m4/Makefile
build/m4/shave/shave
build/m4/shave/shave-libtool

images/Makefile
libcesarplayer/Makefile

LongoMatch.Addins/Makefile
LongoMatch.Core/Makefile
LongoMatch.Multimedia/Makefile
LongoMatch.Multimedia/LongoMatch.Multimedia.dll.config
LongoMatch.GUI.Multimedia/Makefile
LongoMatch.GUI/Makefile
LongoMatch.Services/Makefile
LongoMatch/Makefile
LongoMatch/longomatch
LongoMatch/longomatch.desktop.in
LongoMatch/AssemblyInfo.cs

po/Makefile.in
])

AC_OUTPUT
