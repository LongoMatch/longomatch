/*
 * Copyright (C) 2009-2015  Andoni Morales Alastruey <ylatuya@gmail.com>
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

#ifndef __LGM_UTILS_H__
#define __LGM_UTILS_H__

#include <gst/gst.h>
#include <gst/interfaces/xoverlay.h>
#include <gst/pbutils/pbutils.h>
#include <gio/gio.h>
#include <glib/gi18n.h>
#include <gdk/gdk.h>

#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

G_BEGIN_DECLS

/* Default video/audio sinks */
#if defined(OSTYPE_WINDOWS)
#define DEFAULT_VIDEO_SINK "d3dvideosink"
#define BACKUP_VIDEO_SINK "autovideosink"
#elif defined(OSTYPE_OS_X)
#define DEFAULT_VIDEO_SINK "osxvideosink"
#define BACKUP_VIDEO_SINK "autovideosink"
#elif defined(OSTYPE_LINUX)
#define DEFAULT_VIDEO_SINK "autovideosink"
#define BACKUP_VIDEO_SINK "autovideosink"
#endif

/*Default video/audio source*/
#if defined(OSTYPE_WINDOWS)
#define DVVIDEOSRC "ksvideosrc"
#define SYSVIDEOSRC "ksvideosrc"
#define AUDIOSRC "dshowaudiosrc"
#elif defined(OSTYPE_OS_X)
#define DVVIDEOSRC "avfvideosrc"
#define SYSVIDEOSRC "avfvideosrc"
#define AUDIOSRC "osxaudiosrc"
#elif defined(OSTYPE_LINUX)
#define DVVIDEOSRC "dv1394src"
#define SYSVIDEOSRC "gsettingsvideosrc"
#define AUDIOSRC "gsettingsaudiosrc"
#endif

typedef enum
{
  /* Plugins */
  GST_ERROR_AUDIO_PLUGIN,
  GST_ERROR_NO_PLUGIN_FOR_FILE,
  GST_ERROR_VIDEO_PLUGIN,
  GST_ERROR_AUDIO_BUSY,
  /* File */
  GST_ERROR_BROKEN_FILE,
  GST_ERROR_FILE_GENERIC,
  GST_ERROR_FILE_PERMISSION,
  GST_ERROR_FILE_ENCRYPTED,
  GST_ERROR_FILE_NOT_FOUND,
  /* Devices */
  GST_ERROR_DVD_ENCRYPTED,
  GST_ERROR_INVALID_DEVICE,
  GST_ERROR_DEVICE_BUSY,
  /* Network */
  GST_ERROR_UNKNOWN_HOST,
  GST_ERROR_NETWORK_UNREACHABLE,
  GST_ERROR_CONNECTION_REFUSED,
  /* Generic */
  GST_ERROR_INVALID_LOCATION,
  GST_ERROR_GENERIC,
  GST_ERROR_CODEC_NOT_HANDLED,
  GST_ERROR_AUDIO_ONLY,
  GST_ERROR_CANNOT_CAPTURE,
  GST_ERROR_READ_ERROR,
  GST_ERROR_PLUGIN_LOAD,
  GST_ERROR_EMPTY_FILE
} Error;


typedef enum
{
  VIDEO_ENCODER_MPEG4,
  VIDEO_ENCODER_XVID,
  VIDEO_ENCODER_THEORA,
  VIDEO_ENCODER_H264,
  VIDEO_ENCODER_MPEG2,
  VIDEO_ENCODER_VP8
} VideoEncoderType;

typedef enum
{
  AUDIO_ENCODER_MP3,
  AUDIO_ENCODER_AAC,
  AUDIO_ENCODER_VORBIS
} AudioEncoderType;

typedef enum
{
  VIDEO_MUXER_AVI,
  VIDEO_MUXER_MP4,
  VIDEO_MUXER_MATROSKA,
  VIDEO_MUXER_OGG,
  VIDEO_MUXER_MPEG_PS,
  VIDEO_MUXER_WEBM
} VideoMuxerType;

typedef enum
{
  CAPTURE_SOURCE_TYPE_NONE = 0,
  CAPTURE_SOURCE_TYPE_DV = 1,
  CAPTURE_SOURCE_TYPE_SYSTEM = 2,
  CAPTURE_SOURCE_TYPE_URI = 3,
  CAPTURE_SOURCE_TYPE_FILE = 4,
} CaptureSourceType;

typedef enum {
  GST_AUTOPLUG_SELECT_TRY,
  GST_AUTOPLUG_SELECT_EXPOSE,
  GST_AUTOPLUG_SELECT_SKIP
} GstAutoplugSelectResult;


EXPORT void lgm_init_backend (int argc, char **argv);
EXPORT GstDiscovererResult lgm_discover_uri (const gchar *uri, guint64 *duration,
    guint *width, guint *height, guint *fps_n, guint *fps_d, guint *par_n,
    guint *par_d, gchar **container, gchar **video_codec, gchar **audio_codec,
    GError **err);
EXPORT guintptr lgm_get_window_handle (GdkWindow *window);
EXPORT void lgm_set_window_handle (GstXOverlay *overlay, guintptr window_handle);

void lgm_init_debug();
gchar * lgm_filename_to_uri (const gchar *filena);
GstElement * lgm_create_video_encoder (VideoEncoderType type, guint quality,
    gboolean realtime, GQuark quark, GError **err, gboolean hardware_acceleration);
GstElement * lgm_create_audio_encoder (AudioEncoderType type, guint quality,
    GQuark quark, GError **err);
GstElement * lgm_create_muxer (VideoMuxerType type,
    GQuark quark, GError **err);
GstAutoplugSelectResult lgm_filter_video_decoders (GstElement* object,
    GstPad* arg0, GstCaps* arg1, GstElementFactory* arg2, gpointer user_data);

G_END_DECLS
#endif
