#!/usr/bin/env python
#
# Copyright (c) 2011, Andoni Morales Alastruey <ylatuya@gmail.com>
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
# Free Software Foundation, Inc., 51 Franklin St, Fifth Floor,
# Boston, MA 02110-1301, USA.

import os
import sys
import shutil
import shlex
import subprocess
from optparse import OptionParser

WINFORMS_DEPS = ['System.Windows.Forms\\2.0.0.0__b77a5c561934e089\\System.Windows.Forms.dll',
                 'System.Drawing\\2.0.0.0__b03f5f7f11d50a3a\\System.Drawing.dll',
                 'Accessibility\\2.0.0.0__b03f5f7f11d50a3a\\Accessibility.dll']

GTK_DEPS = ['freetype6.dll', 'libatk-1.0-0.dll', 'libcairo-2.dll', 'libgailutil-18.dll',
            'libgdk_pixbuf-2.0-0.dll', 'libgdk-win32-2.0-0.dll', 'libgtk-win32-2.0-0.dll',
            'libpng14-14.dll', 'libfontconfig-1.dll', 'libpango-1.0-0.dll', 'libpangoft2-1.0-0.dll',
            'libpangocairo-1.0-0.dll', 'libpangowin32-1.0-0.dll', 'zlib1.dll' ]

MONO_DEPS = ['mono-2.0.dll', 'MonoPosixHelper.dll', 'pangosharpglue-2.dll', 'gtksharpglue-2.dll',
             'glibsharpglue-2.dll', 'gdksharpglue-2.dll', 'atksharpglue-2.dll', 'intl.dll']

MONO_LIB_DEPS = ['mscorlib.dll', 'Mono.Cairo.dll', 'System.dll', 'System.Core.dll', 'System.Configuration.dll',
                 'System.Xml.dll', 'System.Security.dll', 'Mono.Security.dll', 'Mono.Posix.dll', 'WindowsBase.dll',
		 'System.Xaml.dll']

MONO_GAC_DEPS = ['gdk-sharp', 'glib-sharp', 'pango-sharp', 'gtk-sharp', 'atk-sharp']

GTK_GAC_V = '2.12.0.0__35e10195dab3c99f'

GST_EXT_DEPS = ['avcodec-gpl-52.dll', 'avcore-gpl-0.dll', 'avdevice-gpl-52.dll', 'avfilter-gpl-1.dll', 
        'avformat-gpl-52.dll', 'avutil-gpl-50.dll', 'libFLAC-8.dll', 'liba52-0.dll',
        'libbz2.dll', 'libdca-0.dll', 'libfaac-0.dll', 'libfaad-2.dll', 'libgcrypt-11.dll', 'libjpeg-8.dll',
        'libogg-0.dll', 'libmp3lame-0.dll', 'libmpeg2-0.dll', 'liborc-0.4-0.dll', 'librsvg-2-2.dll',
        'libxml2-2.dll', 'libschroedinger-1.0-0.dll', 'libsoup-2.4-1.dll', 'libtheora-0.dll',
        'libtheoradec-1.dll', 'libvorbisenc-2.dll', 'libvorbis-0.dll', 'libx264-107.dll',
        'libgpg-error-0.dll', 'libexpat-1.dll', 'libgnutls-26.dll', 'libcroco-0.6-3.dll',
        'libtasn1-3.dll', 'xvidcore.dll', 'z.dll', 'pthreadGC2.dll', 'libiconv-2.dll',
	'ffmpeg-gpl.exe', 'swscale-gpl-0.dll', 'libgcc_s_sjlj-1.dll']

GST_PLUGINS_DEPS = ['libgnl.dll', 'libgsta52dec.dll', 'libgstadder.dll', 'libgstalpha.dll', 'libgstalphacolor.dll',
            'libgstasf.dll', 'libgstasfmux.dll', 'libgstaudioconvert.dll', 'libgstaudiorate.dll',
            'libgstaudioresample.dll', 'libgstaudiotestsrc.dll', 'libgstautoconvert.dll', 
            'libgstautodetect.dll', 'libgstavi.dll', 'libgstcairo.dll', 'libgstcamerabin.dll',
            'libgstcoreelements.dll', 'libgstd3dvideosink.dll', 'libgstdecodebin2.dll',
            'libgstdeinterlace.dll', 'libgstdirectsound.dll', 'libgstdirectsoundsrc.dll',
            'libgstdshowdecwrapper.dll', 'libgstdshowsrcwrapper.dll', 'libgstdtsdec.dll',
            'libgstfaac.dll', 'libgstfaad.dll', 'libgstffmpeg-gpl.dll', 'libgstffmpegcolorspace.dll',
            'libgstflac.dll', 'libgstflv.dll', 'libgstgio.dll', 'libgstjpeg.dll', 'libgstlame.dll',
            'libgstmatroska.dll', 'libgstmpeg2dec.dll', 'libgstmpeg4videoparse.dll',
            'libgstmpegaudioparse.dll', 'libgstmpegdemux.dll', 'libgstmpegstream.dll',
            'libgstmpegvideoparse.dll', 'libgstogg.dll', 'libgstplaybin.dll', 'libgstpng.dll',
            'libgstqtdemux.dll', 'libgstqtmux.dll', 'libgstrsvg.dll', 'libgstschro.dll',
            'libgstselector.dll', 'libgstsouphttpsrc.dll', 'libgsttheora.dll',
            'libgsttypefindfunctions.dll', 'libgstvideobalance.dll', 'libgstvideobox.dll',
            'libgstvideocrop.dll', 'libgstvideomixer.dll', 'libgstvideorate.dll',
            'libgstvideoscale.dll', 'libgstvideotestsrc.dll', 'libgstvolume.dll', 'libgstvorbis.dll',
            'libgstvp8.dll', 'libgstx264.dll', 'libgstxvid.dll', 'libgstpango.dll', 'libgstdshowvideosink.dll',
	    'libgstadpcmdec.dll']

GST_BIN_DEPS = ['gst-inspect.exe', 'gst-launch.exe', 'gst-typefind.exe']

GST_DLL_DEPS = ['libgstapp-0.10.dll', 'libgstaudio-0.10.dll', 'libgstbase-0.10.dll',
        'libgstbasevideo-0.10.dll', 'libgstcdda-0.10.dll', 'libgstcontroller-0.10.dll',
        'libgstdataprotocol-0.10.dll', 'libgstsdp-0.10.dll', 'libgstfft-0.10.dll',
        'libgstinterfaces-0.10.dll', 'libgstnet-0.10.dll', 'libgstnetbuffer-0.10.dll',
        'libgstpbutils-0.10.dll', 'libgstphotography-0.10.dll', 'libgstreamer-0.10.dll',
        'libgsttag-0.10.dll', 'libgstrtsp-0.10.dll', 'libgstrtp-0.10.dll', 'libgstriff-0.10.dll',
        'libgstvideo-0.10.dll']

GLIB_DEPS = ['libgio-2.0-0.dll', 'libglib-2.0-0.dll', 'libgmodule-2.0-0.dll', 'libgobject-2.0-0.dll', 'libgthread-2.0-0.dll']

MSYS_DEPS = ['sh.exe', 'rxvt.exe', 'sh.exe', 'msys-1.0.dll', 'msys-regex-1.dll', 'msys-termcap-0.dll']

IMAGES = ['background.png', 'logo.svg']


class Deploy():

    def __init__(self, gst_path, gtk_path, mono_path, msys_path):
        self.gst_path = gst_path
        self.gtk_path = gtk_path
        self.mono_path = mono_path
        self.msys_path = msys_path
        self.check_paths()
        self.set_path_variables()
        self.create_deployment_folder()
        self.deploy_glib()
        self.deploy_gtk()
        self.deploy_gstreamer()
        self.deploy_mono()
        self.deploy_msys()
        self.deploy_images()
        self.deploy_themes()
        self.deploy_longomatch()
        self.close()

    def close(self, message=None):
        if message is not None:
            print 'ERROR: %s' % message
            exit(1)
        else:
            exit(0)

    def check_paths(self):
        for name in [self.mono_path, self.gst_path, self.gtk_path,
                     self.msys_path]:
            if not os.path.exists(name):
                self.close('%s not found' % name)

    def set_path_variables(self):
        self.curr_dir = os.getcwd()
        if not self.curr_dir.endswith('win32'):
            self.close("The script must be run from the 'win32' folder")
        self.root_dir = os.path.abspath(os.path.join(self.curr_dir,'..'))
        self.deps_dir = os.path.join(self.root_dir, 'win32', 'deps')
        self.dist_dir = os.path.join(self.root_dir, 'win32', 'dist')
        self.bin_dir = os.path.join(self.dist_dir, 'bin')
        self.etc_dir = os.path.join(self.dist_dir, 'etc')
        self.share_dir = os.path.join(self.dist_dir, 'share')
        self.lib_dir = os.path.join(self.dist_dir, 'lib')
        self.images_dir = os.path.join (self.share_dir, 'longomatch', 'images')
        self.plugins_dir = os.path.join(self.lib_dir, 'gstreamer-0.10')
        self.mono_lib_dir = os.path.join(self.lib_dir, 'mono', '4.0')

    def create_deployment_folder(self):
        print 'Create deployment directory'
        if os.path.exists(self.dist_dir):
            try:
                shutil.rmtree(self.dist_dir)
            except :
                self.close("ERROR: Can't delete folder %s" % self.dist_dir)
        for path in [self.dist_dir, self.bin_dir, self.etc_dir,
                     self.images_dir, self.lib_dir, self.plugins_dir,
                     self.mono_lib_dir]:
            try:   
                os.makedirs(path)
            except:
                pass

    def deploy_gtk(self):
        print 'Deploying Gtk dependencies'
        # Copy Gtk files to the dist folder
        for name in ['fonts', 'pango', 'gtk-2.0']:
            shutil.copytree(os.path.join(self.gtk_path, 'etc', name),
                     os.path.join(self.etc_dir, name))
        shutil.copytree(os.path.join(self.gtk_path, 'lib', 'gtk-2.0'),
            os.path.join(self.lib_dir, name))
        for name in GTK_DEPS:
            shutil.copy(os.path.join(self.gtk_path, 'bin', name), self.bin_dir)

    def deploy_mono(self):
        print 'Deploying Mono dependencies'
        for name in MONO_DEPS:
            shutil.copy(os.path.join(self.mono_path, 'bin', name), self.bin_dir)
        shutil.copy(os.path.join(self.mono_path, 'lib', 'mono', '4.0', 'mscorlib.dll'),
                        self.mono_lib_dir)
        for name in MONO_LIB_DEPS:
            shutil.copy(os.path.join(self.mono_path, 'lib', 'mono', '4.0', name),
                        self.bin_dir)
        for name in MONO_GAC_DEPS:
            shutil.copy(os.path.join(self.mono_path, 'lib', 'mono', 'gac', name,
                                     GTK_GAC_V, '%s.dll' % name),
                        self.bin_dir)
        # Gtk.sharp load them dinamically before 2.12.10.
        # FIXME: Delete that when gtk-sharp 2.12.10 is released
        for name in WINFORMS_DEPS:
            shutil.copy(os.path.join(self.mono_path, 'lib\\mono\\gac', name), self.bin_dir)
        shutil.copy(os.path.join(self.deps_dir, 'Mono.Addins.dll'), self.bin_dir)
        shutil.copy(os.path.join(self.deps_dir, 'EPPlus.dll'), self.bin_dir)

    def deploy_themes(self):
        print 'Deploying theming support'
        deps_engines_dir = os.path.join(self.deps_dir, 'engines')
        for filename in os.listdir(deps_engines_dir):
            shutil.copy(os.path.join(deps_engines_dir, filename),
                        os.path.join(self.lib_dir, 'gtk-2.0', '2.10.0', 'engines'))
        shutil.copytree(os.path.join(self.deps_dir, 'themes'),
                        os.path.join(self.share_dir, 'themes'))
        shutil.copy(os.path.join(self.deps_dir, 'gtkrc'),
                    os.path.join(self.etc_dir, 'gtk-2.0'))
        shutil.copy(os.path.join(self.deps_dir, 'ThemeSelector.exe'), self.bin_dir)

    def deploy_images(self):
        print 'Deploying images'
        lgm_images_dir = os.path.join(self.root_dir, 'Images')
        for name in IMAGES:
            shutil.copy(os.path.join(lgm_images_dir, name), self.images_dir)
        shutil.copytree(os.path.join(self.deps_dir, 'icons'),
                        os.path.join(self.share_dir, 'icons'))

    def deploy_glib(self):
        print 'Deploying GLib dependencies'
        for dll in GLIB_DEPS:
            shutil.copy (os.path.join(self.gst_path, 'bin', dll), self.bin_dir)

    def deploy_gstreamer(self):
        print 'Deploying GStreamer dependencies'
        for deps in [GST_BIN_DEPS, GST_EXT_DEPS, GST_DLL_DEPS]:
            for dll in deps:
                shutil.copy (os.path.join(self.gst_path, 'bin', dll), self.bin_dir)
        for dll in GST_PLUGINS_DEPS:
            shutil.copy (os.path.join(self.gst_path, 'lib', 'gstreamer-0.10', dll),
                 self.plugins_dir)
        shutil.move(os.path.join(self.bin_dir, 'ffmpeg-gpl.exe'),
                    os.path.join(self.bin_dir, 'ffmpeg.exe'))
    
    def deploy_longomatch(self):
        print 'Deploying LongoMatch'
        args =shlex.split('make -f Makefile.win32 install')
        p = subprocess.call(args, cwd=os.path.join(self.curr_dir, '..'))
        shutil.copy(os.path.join(self.deps_dir, 'Db4objects.Db4o.dll'), self.bin_dir)
        shutil.copy(os.path.join(self.curr_dir, 'checks.sh'), self.bin_dir)

    def deploy_msys(self):
        print 'Deploying msys'
        for dll in MSYS_DEPS:
            shutil.copy (os.path.join(self.msys_path, 'bin', dll), self.bin_dir)
         

def main():
    usage = "usage: %prog [options]"
    parser = OptionParser(usage)
    parser.add_option("-g", "--gst-path", action="store",
            dest="gst_path",default="c:\\gstreamer", type="string",
            help="GStreamer installation path")
    parser.add_option("-k", "--gtk-path", action="store",
            dest="gtk_path",default="c:\\gtk", type="string",
            help="GTK+ installation path")
    parser.add_option("-m", "--mono-path", action="store",
            dest="mono_path",default="c:\\mono", type="string",
            help="Mono installation path")
    parser.add_option("-s", "--msys_path", action="store",
            dest="msys_path",default="c:\\msys\\1.0", type="string",
            help="MSYS installation path")

    (options, args) = parser.parse_args()
    Deploy(options.gst_path, options.gtk_path, options.mono_path, options.msys_path)

if __name__ == "__main__":
    main()
