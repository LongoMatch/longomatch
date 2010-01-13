#!/usr/bin/env python
# PiTiVi , Non-linear video editor
#
# Copyright (c) 2009, Andoni Morales Alastruey <ylatuya@gmail.com>
#
# This program is free software; you can redistribute it and/or
# modify it under the terms of the GNU Lesser General Public
# License as published by the Free Software Foundation; either
# version 2.1 of the License, or (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Lesser General Public License for more details.
#
# You should have received a copy of the GNU Lesser General Public
# License along with this program; if not, write to the
# Free Software Foundation, Inc., 59 Temple Place - Suite 330,
# Boston, MA 02111-1307, USA.

import os
import sys
import shutil

MONO_PATH = 'c:\\mono'
GST_PATH = 'c:\\gstreamer'
GTK_PATH = 'c:\\gtk'

for name in [MONO_PATH, GST_PATH, GTK_PATH]:
    if not os.path.exists(name):
        print '%s not found' % name
        exit(1)

GTK_DEPS = ['freetype6.dll', 'libatk-1.0-0.dll', 'libcairo-2.dll', 'libgailutil-18.dll',
            'libgdk_pixbuf-2.0-0.dll', 'libgdk-win32-2.0-0.dll', 'libgtk-win32-2.0-0.dll',
            'libjpeg-7.dll', 'libpng12-0.dll', 'libtiff-3.dll', 'libtiffxx-3.dll' ]

MONO_DEPS = ['mono.dll', 'MonoPosixHelper.dll', 'pangosharpglue-2.dll', 'gtksharpglue-2.dll',
             'glibsharpglue-2.dll', 'gdksharpglue-2.dll', 'atksharpglue-2.dll', 'intl.dll']

IMAGES = ['background.png', 'longomatch.png']

LINGUAS = ['es', 'de']

# Set-up working folder
root_dir = os.path.abspath(os.path.join(os.getcwd(), '..'))
deps_dir = os.path.join(root_dir, 'win32', 'deps')
# Create a Unix-like diretory tree to deploy LongoMatch
print ('Create deployment tree')
dist_dir = os.path.join (root_dir, 'win32', 'dist')
if os.path.exists(dist_dir):
     try:
          shutil.rmtree(dist_dir)
     except:
          print "ERROR: Can't delete folder %s" % dist_dir
          exit(1)

bin_dir = os.path.join (dist_dir, 'bin')
etc_dir = os.path.join (dist_dir, 'etc')
share_dir = os.path.join (dist_dir, 'share')
locale_dir = os.path.join (share_dir, 'locale')
lib_dir = os.path.join (dist_dir, 'lib')
images_dir = os.path.join (share_dir, 'longomatch', 'images')

for path in [dist_dir, bin_dir, etc_dir, images_dir, locale_dir, lib_dir]:
     try:
        os.makedirs(path)
     except:
        pass

print ('Deploying GStreamer dependencies')
# Copy the gstreamer's binaries to the dist folder
for name in os.listdir(os.path.join(GST_PATH, 'bin')):
     shutil.copy (os.path.join(GST_PATH, 'bin', name), bin_dir)
shutil.copytree(os.path.join(GST_PATH, 'lib', 'gstreamer-0.10'),
                os.path.join(lib_dir, 'gstreamer-0.10'))

print ('Deploying Gtk dependencies')
# Copy Gtk deps to the dist folder
for name in ['fonts', 'pango', 'gtk-2.0']:
     shutil.copytree(os.path.join(GTK_PATH, 'etc', name),
                     os.path.join(etc_dir, name))
shutil.copytree(os.path.join(GTK_PATH, 'lib', 'gtk-2.0'),
                os.path.join(lib_dir, name))
for name in LINGUAS:
    shutil.copytree(os.path.join(GTK_PATH, 'share', 'locale', name),
                    os.path.join(share_dir, 'locale', name))

for name in GTK_DEPS:
    shutil.copy(os.path.join(GTK_PATH, 'bin', name), bin_dir)

print ('Deploying Mono dependences')
# Copy Mono deps to the dist folder
for name in MONO_DEPS:
    shutil.copy(os.path.join(MONO_PATH, 'bin', name), bin_dir)
# Gtk.sharp load them dinamically before 2.12.10.
# FIXME: Delete that when gtk-sharp 2.12.10 is released
for name in ['System.Windows.Forms\\2.0.0.0__b77a5c561934e089\\System.Windows.Forms.dll',
             'System.Drawing\\2.0.0.0__b03f5f7f11d50a3a\\System.Drawing.dll',
             'Accessibility\\2.0.0.0__b03f5f7f11d50a3a\\Accessibility.dll']:
    shutil.copy(os.path.join(MONO_PATH, 'lib\\mono\\gac', name), bin_dir)

print ('Deploying images')
lgm_images_dir = os.path.join(root_dir, 'LongoMatch', 'Images')
for name in IMAGES:
    shutil.copy(os.path.join(lgm_images_dir, name), images_dir)
shutil.copytree(os.path.join(deps_dir, 'icons'),
                os.path.join(share_dir, 'icons'))

print ('Deploying theming support')
deps_engines_dir = os.path.join(deps_dir, 'engines')
for name in os.listdir(deps_engines_dir):
    shutil.copy(os.path.join(deps_engines_dir, name),
                os.path.join(lib_dir, 'gtk-2.0', '2.10.0', 'engines'))
shutil.copytree(os.path.join(deps_dir, 'themes'),
                os.path.join(share_dir, 'themes'))
shutil.copy(os.path.join(deps_dir, 'gtkrc'),
            os.path.join(etc_dir, 'gtk-2.0'))
shutil.copy(os.path.join(deps_dir, 'ThemeSelector.exe'), bin_dir)





