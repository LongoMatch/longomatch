/* 
 * Copyright (C) 2003-2007 the GStreamer project
 *      Julien Moutte <julien@moutte.net>
 *      Ronald Bultje <rbultje@ronald.bitfreak.net>
 * Copyright (C) 2005-2008 Tim-Philipp Müller <tim centricular net>
 * Copyright (C) 2009 Sebastian Dröge <sebastian.droege@collabora.co.uk>
 * Copyright (C) 2009  Andoni Morales Alastruey <ylatuya@gmail.com> 
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
 *
 * The Totem project hereby grant permission for non-gpl compatible GStreamer
 * plugins to be used and distributed together with GStreamer and Totem. This
 * permission is above and beyond the permissions granted by the GPL license
 * Totem is covered by.
 *
 */

#include <gst/gst.h>
#include <gst/interfaces/xoverlay.h>
#include <gst/pbutils/pbutils.h>
#include <gdk/gdk.h>
#include "common.h"


#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

 G_BEGIN_DECLS

typedef enum {
  GST_AUTOPLUG_SELECT_TRY,
  GST_AUTOPLUG_SELECT_EXPOSE,
  GST_AUTOPLUG_SELECT_SKIP
} GstAutoplugSelectResult;


EXPORT void lgm_init_backend (int argc, char **argv);
EXPORT guintptr lgm_get_window_handle (GdkWindow *window);
EXPORT void lgm_set_window_handle (GstXOverlay *overlay, guintptr window_handle);
EXPORT void lgm_init_debug();
EXPORT gchar * lgm_filename_to_uri (const gchar *filena);

EXPORT GstDiscovererResult lgm_discover_uri (const gchar *uri, guint64 *duration,
    guint *width, guint *height, guint *fps_n, guint *fps_d, guint *par_n,
    guint *par_d, gchar **container, gchar **video_codec, gchar **audio_codec,
    GError **err);
EXPORT GstElement * lgm_create_video_encoder (VideoEncoderType type, guint quality,
    gboolean realtime, GQuark quark, GError **err);
EXPORT GstElement * lgm_create_audio_encoder (AudioEncoderType type, guint quality,
    GQuark quark, GError **err);
EXPORT GstElement * lgm_create_muxer (VideoMuxerType type,
    GQuark quark, GError **err);
EXPORT GstAutoplugSelectResult lgm_filter_video_decoders (GstElement* object,
    GstPad* arg0, GstCaps* arg1, GstElementFactory* arg2, gpointer user_data);

G_END_DECLS