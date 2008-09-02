# -*- mode: makefile; coding: utf-8 -*-
# Copyright © 2002,2003 Colin Walters <walters@debian.org>
# Description: A class to configure and build GNU autoconf+automake packages
#
# This program is free software; you can redistribute it and/or
# modify it under the terms of the GNU General Public License as
# published by the Free Software Foundation; either version 2, or (at
# your option) any later version.
#
# This program is distributed in the hope that it will be useful, but
# WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA
# 02111-1307 USA.

_cdbs_scripts_path ?= /usr/lib/cdbs
_cdbs_rules_path ?= /usr/share/cdbs/1/rules
_cdbs_class_path ?= /usr/share/cdbs/1/class

ifndef _cdbs_class_autotools
_cdbs_class_autotools = 1

include $(_cdbs_class_path)/autotools-files.mk$(_cdbs_makefile_suffix)

# Overriden from makefile-vars.mk.  We pass CFLAGS and friends to ./configure, so
# no need to pass them to make.
DEB_MAKE_INVOKE = $(DEB_MAKE_ENVVARS) $(MAKE) -C $(DEB_BUILDDIR)

common-configure-arch common-configure-indep:: common-configure-impl
common-configure-impl:: $(DEB_BUILDDIR)/config.status
$(DEB_BUILDDIR)/config.status:
	chmod a+x $(DEB_CONFIGURE_SCRIPT)
	$(DEB_CONFIGURE_INVOKE) $(cdbs_configure_flags) $(DEB_CONFIGURE_EXTRA_FLAGS) $(DEB_CONFIGURE_USER_FLAGS)
	$(if $(filter post,$(DEB_AUTO_UPDATE_LIBTOOL)),if [ -e $(DEB_BUILDDIR)/libtool ]; then cp -f /usr/bin/libtool $(DEB_BUILDDIR)/libtool; fi)

endif
