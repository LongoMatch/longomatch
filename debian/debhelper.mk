# -*- mode: makefile; coding: utf-8 -*-
# Copyright © 2002,2003 Colin Walters <walters@debian.org>
# Description: Uses Debhelper to implement the binary package building stage
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

####
# General variables implemented by this rule file:
#
# DEB_INSTALL_DIRS_ALL
#   Subdirectories to create in installation staging directory for every package
# DEB_INSTALL_DIRS_<package>
#   Like the above, but for a particular package <package>.
# DEB_INSTALL_DOCS_ALL
#   Files which should go in /usr/share/doc/<package> for every package
# DEB_INSTALL_DOCS_<package>
#   Files which should go in /usr/share/doc/<package> for package
# DEB_INSTALL_CHANGELOGS_ALL
#   File which should be interpreted as upstream changelog
# DEB_COMPRESS_EXCLUDE
#   Regular expressions matching files which should not be compressed.
# DEB_FIXPERMS_EXCLUDE
#   Regular expressions matching files which should not have their permissions changed.
# DEB_CLEAN_EXCLUDE
#   Regular expressions matching files which should not be cleaned.
# DEB_DH_ALWAYS_EXCLUDE
#   Force builddeb to exclude files.  See the DH_ALWAYS_EXCLUDE section
#   in debhelper(7) for more details.
# DEB_SHLIBDEPS_LIBRARY_package
#   The name of the current library package
# DEB_SHLIBDEPS_INCLUDE
#   A space-separated list of library paths to search for dependency info
# DEB_SHLIBDEPS_INCLUDE_package
#   Like the above, but for a particular package.
# DEB_PERL_INCLUDE
#   A space-separated list of paths to search for perl modules
# DEB_PERL_INCLUDE_package
#   Like the above, but for a particular package.
# DEB_UPDATE_RCD_PARAMS
#   Arguments to pass to update-rc.d in init scripts
####
# Special variables used by this rule file:
#
# DEB_DH_MAKESHLIBS_ARGS_ALL
#   Arguments passed directly to dh_makeshlibs, for all packages
# DEB_DH_MAKESHLIBS_ARGS_<package>
#   Arguments passed directly to dh_makeshlibs, for a particular package <package>
# DEB_DH_MAKESHLIBS_ARGS
#   Completely override argument passing to dh_makeshlibs. 
# DEB_DH_SHLIBDEPS_ARGS_ALL
#   Arguments passed directly to dh_shlibdeps, for all packages
# DEB_DH_SHLIBDEPS_ARGS_<package>
#   Arguments passed directly to dh_shlibdeps, for a particular package <package>
# DEB_DH_SHLIBDEPS_ARGS
#   Completely override argument passing to dh_shlibdeps.
# DEB_DH_PERL_ARGS
#   Completely override argument passing to dh_perl.
####

_cdbs_scripts_path ?= /usr/lib/cdbs
_cdbs_rules_path ?= /usr/share/cdbs/1/rules
_cdbs_class_path ?= /usr/share/cdbs/1/class

ifndef _cdbs_rules_debhelper
_cdbs_rules_debhelper = 1

include $(_cdbs_rules_path)/buildcore.mk$(_cdbs_makefile_suffix)

DH_COMPAT ?= $(shell cat debian/compat 2>/dev/null)
ifeq (,$(DH_COMPAT))
DH_COMPAT = 5
endif

ifeq ($(DH_COMPAT),4)
CDBS_BUILD_DEPENDS := $(CDBS_BUILD_DEPENDS), debhelper (>= 4.2.0)
endif
ifeq ($(DH_COMPAT),5)
CDBS_BUILD_DEPENDS := $(CDBS_BUILD_DEPENDS), debhelper (>= 5)
endif
ifeq ($(DH_COMPAT),6)
CDBS_BUILD_DEPENDS := $(CDBS_BUILD_DEPENDS), debhelper (>= 5.0.44)
endif

ifeq ($(DEB_VERBOSE_ALL), yes)
DH_VERBOSE = 1
endif

is_debug_package=$(if $(patsubst %-dbg,,$(cdbs_curpkg)),,yes)

DEB_INSTALL_DIRS_ALL =
DEB_INSTALL_CHANGELOGS_ALL =
DEB_INSTALL_DOCS_ALL = $(filter-out $(DEB_INSTALL_CHANGELOGS_ALL),$(shell for f in README NEWS TODO BUGS AUTHORS THANKS; do if test -s $(DEB_SRCDIR)/$$f; then echo $(DEB_SRCDIR)/$$f; fi; done))

cdbs_add_dashx = $(foreach i,$(1),$(patsubst %,-X %,$(i)))
cdbs_strip_quotes = $(subst ',,$(subst ",,$(1)))
# hello emacs '

DEB_DH_MAKESHLIBS_ARGS = $(DEB_DH_MAKESHLIBS_ARGS_ALL) $(DEB_DH_MAKESHLIBS_ARGS_$(cdbs_curpkg))
DEB_DH_SHLIBDEPS_ARGS = $(if $(DEB_SHLIBDEPS_LIBRARY_$(cdbs_curpkg)),-L $(DEB_SHLIBDEPS_LIBRARY_$(cdbs_curpkg))) $(if $(DEB_SHLIBDEPS_INCLUDE_$(cdbs_curpkg))$(DEB_SHLIBDEPS_INCLUDE),-l $(shell echo $(DEB_SHLIBDEPS_INCLUDE_$(cdbs_curpkg)):$(DEB_SHLIBDEPS_INCLUDE) | perl -pe 's/ /:/g;')) $(DEB_DH_SHLIBDEPS_ARGS_ALL) $(DEB_DH_SHLIBDEPS_ARGS_$(cdbs_curpkg))

DEB_DH_BUILDDEB_ENV = $(if $(DEB_DH_ALWAYS_EXCLUDE),DH_ALWAYS_EXCLUDE=$(DEB_DH_ALWAYS_EXCLUDE),)
DEB_DH_PERL_ARGS = $(if $(DEB_PERL_INCLUDE_$(cdbs_curpkg))$(DEB_PERL_INCLUDE),$(shell echo $(DEB_PERL_INCLUDE_$(cdbs_curpkg)) $(DEB_PERL_INCLUDE)))

ifneq ($(words $(DEB_DBG_PACKAGES)),0)
ifeq ($(words $(DEB_DBG_PACKAGES)),1)
DEB_DBG_PACKAGE_ALL = $(if $(strip $(foreach x,$(DEB_PACKAGES),$(DEB_DBG_PACKAGE_$(x)))),,$(DEB_DBG_PACKAGES))
else
ifeq (yes,$(if $(findstring no,$(foreach x,$(DEB_DBG_PACKAGES),$(if $(filter $(x:-dbg=),$(DEB_PACKAGES)),yes,no))),no,yes))
define cdbs_deb_dbg_package_assign
DEB_DBG_PACKAGE_$(1:-dbg=) = $(1)
endef
$(foreach x,$(DEB_DBG_PACKAGES),$(eval $(call cdbs_deb_dbg_package_assign,$(value x))))
endif
endif
cdbs_dbg_package = $(if $(DEB_DBG_PACKAGE_$(cdbs_curpkg)),$(DEB_DBG_PACKAGE_$(cdbs_curpkg)),$(DEB_DBG_PACKAGE_ALL))
cdbs_dbg_package_option = $(if $(cdbs_dbg_package),$(shell if [ "$(DH_COMPAT)" -ge 5 ]; then echo "--dbg-package=$(strip $(cdbs_dbg_package))"; fi))
DEB_DH_STRIP_ARGS = $(cdbs_dbg_package_option)
endif

pre-build:: debian/compat

debian/compat:
	$(if $(DEB_DH_COMPAT_DISABLE),,echo $(DH_COMPAT) >$@)

clean::
	dh_clean $(call cdbs_add_dashx,$(DEB_CLEAN_EXCLUDE))

common-install-prehook-arch common-install-prehook-indep:: common-install-prehook-impl
common-install-prehook-impl::
	dh_clean -k $(call cdbs_add_dashx,$(DEB_CLEAN_EXCLUDE))
	dh_installdirs -A $(DEB_INSTALL_DIRS_ALL)

$(patsubst %,install/%,$(DEB_ALL_PACKAGES)) :: install/%:
	dh_installdirs -p$(cdbs_curpkg) $(DEB_INSTALL_DIRS_$(cdbs_curpkg))

# Create .debs or .udebs as we see fit
$(patsubst %,binary/%,$(DEB_ALL_PACKAGES)) :: binary/% : binary-makedeb/%

####
# General Debian package creation rules.
####

# This rule is called once for each package.  It does the work
# of installing to debian/<packagename>; this includes running
# dh_install to split the source from debian/tmp, as well as installing
# ChangeLogs and the like.
$(patsubst %,binary-install/%,$(DEB_ALL_PACKAGES)) :: binary-install/%:
	dh_installdocs -p$(cdbs_curpkg) $(DEB_INSTALL_DOCS_ALL) $(DEB_INSTALL_DOCS_$(cdbs_curpkg)) 
	dh_installexamples -p$(cdbs_curpkg) $(DEB_INSTALL_EXAMPLES_$(cdbs_curpkg))
	dh_installman -p$(cdbs_curpkg) $(DEB_INSTALL_MANPAGES_$(cdbs_curpkg)) 
	dh_installinfo -p$(cdbs_curpkg) $(DEB_INSTALL_INFO_$(cdbs_curpkg)) 
	dh_installmenu -p$(cdbs_curpkg) $(DEB_DH_INSTALL_MENU_ARGS)
	dh_installcron -p$(cdbs_curpkg) $(DEB_DH_INSTALL_CRON_ARGS)
	dh_installinit -p$(cdbs_curpkg) $(if $(DEB_UPDATE_RCD_PARAMS),--update-rcd-params="$(call cdbs_strip_quotes,$(DEB_UPDATE_RCD_PARAMS))",$(if $(DEB_UPDATE_RCD_PARAMS_$(cdbs_curpkg)),--update-rcd-params="$(call cdbs_strip_quotes,$(DEB_UPDATE_RCD_PARAMS_$(cdbs_curpkg)))")) $(DEB_DH_INSTALLINIT_ARGS) 
	dh_installdebconf -p$(cdbs_curpkg) $(DEB_DH_INSTALLDEBCONF_ARGS)
	dh_installemacsen -p$(cdbs_curpkg) $(if $(DEB_EMACS_PRIORITY),--priority=$(DEB_EMACS_PRIORITY)) $(if $(DEB_EMACS_FLAVOR),--flavor=$(DEB_EMACS_FLAVOR)) $(DEB_DH_INSTALLEMACSEN_ARGS)
	dh_installcatalogs -p$(cdbs_curpkg) $(DEB_DH_INSTALLCATALOGS_ARGS)
	dh_installpam -p$(cdbs_curpkg) $(DEB_DH_INSTALLPAM_ARGS)
	dh_installlogrotate -p$(cdbs_curpkg) $(DEB_DH_INSTALLLOGROTATE_ARGS)
	dh_installlogcheck -p$(cdbs_curpkg) $(DEB_DH_INSTALLLOGCHECK_ARGS)
	dh_installmime -p$(cdbs_curpkg) $(DEB_DH_INSTALLMIME_ARGS)
	dh_installchangelogs -p$(cdbs_curpkg) $(DEB_DH_INSTALLCHANGELOGS_ARGS) $(DEB_INSTALL_CHANGELOGS_ALL) $(DEB_INSTALL_CHANGELOGS_$(cdbs_curpkg))
	$(if $(wildcard /usr/bin/dh_installudev),dh_installudev -p$(cdbs_curpkg) $(DEB_DH_INSTALLUDEV_ARGS))
	dh_install -p$(cdbs_curpkg) $(if $(DEB_DH_INSTALL_SOURCEDIR),--sourcedir=$(DEB_DH_INSTALL_SOURCEDIR)) $(DEB_DH_INSTALL_ARGS)
	dh_link -p$(cdbs_curpkg) $(DEB_DH_LINK_ARGS) $(DEB_DH_LINK_$(cdbs_curpkg))

# This rule is called after all packages have been installed, and their
# post-install hooks have been run.
common-binary-post-install-arch:: $(patsubst %,binary-post-install/%,$(DEB_ARCH_PACKAGES))
common-binary-post-install-indep:: $(patsubst %,binary-post-install/%,$(DEB_INDEP_PACKAGES))

# This rule is called once for each package; it's a general hook
# to do things like remove files, etc.
$(patsubst %,binary-post-install/%,$(DEB_ALL_PACKAGES)) :: binary-post-install/%: binary-install/%

# This rule is called after installation and the post-install hooks,
# to strip files.
$(patsubst %,binary-strip/%,$(DEB_ARCH_PACKAGES)) :: binary-strip/%: common-binary-post-install-arch binary-strip-IMPL/%
$(patsubst %,binary-strip/%,$(DEB_INDEP_PACKAGES)) :: binary-strip/%: common-binary-post-install-indep binary-strip-IMPL/%
$(patsubst %,binary-strip-IMPL/%,$(DEB_ALL_PACKAGES)) :: binary-strip-IMPL/%: 
	$(if $(is_debug_package),,dh_strip -p$(cdbs_curpkg) $(call cdbs_add_dashx,$(DEB_STRIP_EXCLUDE)) $(DEB_DH_STRIP_ARGS))

# This rule is called right before generating debs {post,pre}{inst,rm} and controls, deps, are calculated
# for each package, but after the binary-fixup hooks have been run.
# (necessary for dh_shlibdeps to work on our own dh_makeshlibs'ed libs)
common-binary-fixup-arch:: $(patsubst %,binary-fixup/%,$(DEB_ARCH_PACKAGES))
common-binary-fixup-indep:: $(patsubst %,binary-fixup/%,$(DEB_INDEP_PACKAGES))

# This rule is called after stripping; it compresses, fixes permissions,
# and sets up shared library information.
$(patsubst %,binary-fixup/%,$(DEB_ALL_PACKAGES)) :: binary-fixup/%: binary-strip/%
	dh_compress -p$(cdbs_curpkg) $(call cdbs_add_dashx,$(DEB_COMPRESS_EXCLUDE)) $(DEB_DH_COMPRESS_ARGS)
	dh_fixperms -p$(cdbs_curpkg) $(call cdbs_add_dashx,$(DEB_FIXPERMS_EXCLUDE)) $(DEB_DH_FIXPERMS_ARGS)
	$(if $(is_debug_package),,dh_makeshlibs -p$(cdbs_curpkg) $(DEB_DH_MAKESHLIBS_ARGS))

# This rule is called right before building the binary .deb packages
# for each package, but after the binary-predeb hooks have been run.
common-binary-predeb-arch:: $(patsubst %,binary-predeb/%,$(DEB_ARCH_PACKAGES))
common-binary-predeb-indep:: $(patsubst %,binary-predeb/%,$(DEB_INDEP_PACKAGES))

# This rule is called right before a packages' .deb file is made.
# It is a good place to make programs setuid, change the scripts in DEBIAN/, etc. 
$(patsubst %,binary-predeb/%,$(DEB_ARCH_PACKAGES)) :: binary-predeb/%: common-binary-fixup-arch binary-predeb-IMPL/%
$(patsubst %,binary-predeb/%,$(DEB_INDEP_PACKAGES)) :: binary-predeb/%: common-binary-fixup-indep binary-predeb-IMPL/%
$(patsubst %,binary-predeb-IMPL/%,$(DEB_ALL_PACKAGES)) :: binary-predeb-IMPL/%:
	dh_installdeb -p$(cdbs_curpkg) $(DEB_DH_INSTALLDEB_ARGS)
	dh_perl -p$(cdbs_curpkg) $(DEB_DH_PERL_ARGS)
	dh_shlibdeps -p$(cdbs_curpkg) $(DEB_DH_SHLIBDEPS_ARGS)

# This rule is called to create a package.  Generally it's not going to be
# useful to hook things onto this rule.
$(patsubst %,binary-makedeb/%,$(DEB_ARCH_PACKAGES)) :: binary-makedeb/% : common-binary-predeb-arch binary-makedeb-IMPL/%
$(patsubst %,binary-makedeb/%,$(DEB_INDEP_PACKAGES)) :: binary-makedeb/% : common-binary-predeb-indep binary-makedeb-IMPL/%
$(patsubst %,binary-makedeb-IMPL/%,$(DEB_ALL_PACKAGES)) :: binary-makedeb-IMPL/% : 
	dh_gencontrol -p$(cdbs_curpkg) $(DEB_DH_GENCONTROL_ARGS)

	# symlink identical documentation to depending packages
	[ -n "$$CDBS_NO_DOC_SYMLINKING" ] || \
	[ -h debian/$(cdbs_curpkg)/usr/share/doc ] || \
	[ ! -d debian/$(cdbs_curpkg)/usr/share/doc ] || \
	for dep in `perl -ne 'if (/^(Pre-)?Depends:/) {s/^\w+://; foreach (split /,/) { split; print($$_[0], "\n"); } }' debian/$(cdbs_curpkg)/DEBIAN/control`; do \
	    if [ -d debian/$$dep/usr/share/doc ]; then \
                echo "Searching for duplicated docs in dependency $$dep..."; \
                rootdir=`pwd`; \
                (cd debian/$(cdbs_curpkg)/usr/share/doc/$(cdbs_curpkg); find -type f ! -name copyright | while read f; do \
                    thisfile="$$rootdir/debian/$(cdbs_curpkg)/usr/share/doc/$(cdbs_curpkg)/$$f"; \
                    depfile="$$rootdir/debian/$$dep/usr/share/doc/$$dep/$$f"; \
                    if [ -f $$depfile -o -L $$depfile ] && zcmp $$thisfile $$depfile >/dev/null; then \
                        echo "  symlinking $$f in $(cdbs_curpkg) to file in $$dep"; \
                        rm $$thisfile; ln -s /usr/share/doc/$$dep/$$f $$thisfile; \
                    fi; \
                done ); \
            fi; \
	done

	# symlink identical Gnome help files within packages
	if [ -z "$$CDBS_NO_GNOME_HELP_SYMLINKING" ] && [ -d debian/$(cdbs_curpkg)/usr/share/gnome/help ]; then \
            cd debian/$(cdbs_curpkg) && LC_ALL=C fdupes -r1nq usr/share/gnome/help | while read s; do \
                set -- $$(echo $$s | tr ' ' '\n' | sort); \
                f=$$1; shift; \
                for d; do \
                    echo "symlinking duplicate Gnome help file $$d to $$f"; \
                    rm $$d; ln -s /$$f $$d; \
                done; \
            done; \
	fi

        # fix absolute symlinks created above to be relative
	dh_link -p $(cdbs_curpkg)

	dh_md5sums -p$(cdbs_curpkg) $(DEB_DH_MD5SUMS_ARGS)
	$(DEB_DH_BUILDDEB_ENV) dh_builddeb -p$(cdbs_curpkg) $(DEB_DH_BUILDDEB_ARGS)

## Deprecated
common-binary-post-install:: common-binary-post-install-arch common-binary-post-install-indep
common-binary-predeb:: common-binary-predeb-arch common-binary-predeb-indep

## Deprecated special handling of .udebs
$(patsubst %,binary/%,$(DEB_UDEB_PACKAGES)) :: binary/% : binary-makeudeb/%
$(patsubst %,binary-install-udeb/%,$(DEB_UDEB_PACKAGES)) :: binary-install-udeb/%:
common-binary-post-install-udeb:: $(patsubst %,binary-post-install-udeb/%,$(DEB_UDEB_PACKAGES))
$(patsubst %,binary-post-install-udeb/%,$(DEB_UDEB_PACKAGES)) :: binary-post-install-udeb/%: binary-install-udeb/%
$(patsubst %,binary-makeudeb/%,$(DEB_UDEB_PACKAGES)) :: binary-makeudeb/% : common-binary-post-install-udeb

endif
