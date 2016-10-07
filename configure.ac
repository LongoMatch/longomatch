dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([LongoMatch],
	m4_esyscmd([build/git-version-gen .tarball-version]),
	[contact@longomatch.com])
AC_CONFIG_AUX_DIR([.])
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

ASSEMBLY_VERSION=`echo $PACKAGE_VERSION |cut -d'-' -f1`
AC_SUBST(ASSEMBLY_VERSION)

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

dnl Check for xbuild
AC_PATH_PROG(XBUILD, xbuild)

dnl Covem (optional)
AC_PATH_PROG(COVEM, covem, no)
AC_PATH_PROG(COV_GTK, cov-gtk, no)
AC_PATH_PROG(COV_HTML, cov-html, no)
AM_CONDITIONAL(HAVE_COVEM, test x$COVEM != xno)

dnl Disable multimedia tools
AC_ARG_WITH([multimedia-tools],
	AS_HELP_STRING([--without-multimedia-tools], [Disable multimedia tools]))
AM_CONDITIONAL(WITH_MULTIMEDIA_TOOLS, test x$with_multimedia_tools != xno)

dnl package checks, common for all configs
PKG_CHECK_MODULES([GLIBSHARP], [glib-sharp-2.0])
AC_SUBST(GLIBSHARP_LIBS)
PKG_CHECK_MODULES([GTKSHARP], [gtk-sharp-2.0])
AC_SUBST(GTKSHARP_LIBS)
PKG_CHECK_MODULES([MONO_ADDINS], [mono-addins])
AC_SUBST(MONO_ADDINS_LIBS)

AC_MSG_CHECKING([for the OS type])
ostype=""

XBUILD_CONSTANTS=HAVE_GTK

case "$host" in
  *-*-mingw*|*-*-cygwin*)
    ostype=Windows
    AC_DEFINE(OSTYPE_WINDOWS)
    XBUILD_CONSTANTS="$XBUILD_CONSTANTS OSTYPE_WINDOWS"
    ;;
  *-*-darwin*)
    AC_DEFINE(OSTYPE_OS_X)
    ostype=OS_X
    XBUILD_CONSTANTS="$XBUILD_CONSTANTS OSTYPE_OS_X"
    ;;
  *-*-linux*)
    AC_DEFINE(OSTYPE_LINUX)
    XBUILD_CONSTANTS="$XBUILD_CONSTANTS OSTYPE_LINUX"
    ostype=Linux
    ;;
esac
AM_CONDITIONAL(OSTYPE_WINDOWS, test x$ostype = xWindows)
AM_CONDITIONAL(OSTYPE_OS_X, test x$ostype = xOS_X)
AM_CONDITIONAL(OSTYPE_LINUX, test x$ostype = xLinux)
AC_SUBST(XBUILD_CONSTANTS, ["$XBUILD_CONSTANTS"])
AC_MSG_RESULT($ostype)

dnl package checks, per config

AC_CONFIG_SUBDIRS([VAS])

AC_CONFIG_FILES([env], [chmod +x env])
AC_CONFIG_FILES([
Makefile

build/Makefile
build/m4/Makefile

images/Makefile

.nuget/Makefile

AssemblyInfo/AssemblyInfo.cs
LongoMatch.Addins/Makefile
LongoMatch.DB/Makefile
LongoMatch.Core/Makefile
LongoMatch.Drawing/Makefile
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
