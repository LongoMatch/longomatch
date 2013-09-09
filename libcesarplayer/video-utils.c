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

#include "video-utils.h"

#include <glib/gi18n.h>
#include <libintl.h>

#include <gst/gst.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>
#include <stdio.h>

void
totem_gdk_window_set_invisible_cursor (GdkWindow * window)
{
  GdkCursor *cursor;
#ifdef WIN32
  cursor = gdk_cursor_new (GDK_X_CURSOR);
#else
  cursor = gdk_cursor_new (GDK_BLANK_CURSOR);
#endif
  gdk_window_set_cursor (window, cursor);
  gdk_cursor_unref (cursor);
}

void
totem_gdk_window_set_waiting_cursor (GdkWindow * window)
{
  GdkCursor *cursor;

  cursor = gdk_cursor_new (GDK_WATCH);
  gdk_window_set_cursor (window, cursor);
  gdk_cursor_unref (cursor);

  gdk_flush ();
}

gboolean
totem_display_is_local (void)
{
  const char *name, *work;
  int display, screen;
  gboolean has_hostname;

  name = gdk_display_get_name (gdk_display_get_default ());
  if (name == NULL)
    return TRUE;

  work = strstr (name, ":");
  if (work == NULL)
    return TRUE;

  has_hostname = (work - name) > 0;

  /* Get to the character after the colon */
  work++;
  if (*work == '\0')
    return TRUE;

  if (sscanf (work, "%d.%d", &display, &screen) != 2)
    return TRUE;

  if (has_hostname == FALSE)
    return TRUE;

  if (display < 10)
    return TRUE;

  return FALSE;
}

char *
totem_time_to_string (gint64 msecs)
{
  int sec, min, hour, time;

  time = (int) (msecs / 1000);
  sec = time % 60;
  time = time - sec;
  min = (time % (60 * 60)) / 60;
  time = time - (min * 60);
  hour = time / (60 * 60);

  if (hour > 0) {
    /* hour:minutes:seconds */
    /* Translators: This is a time format, like "9:05:02" for 9
     * hours, 5 minutes, and 2 seconds. You may change ":" to
     * the separator that your locale uses or use "%Id" instead
     * of "%d" if your locale uses localized digits.
     */
    return g_strdup_printf (C_ ("long time format", "%d:%02d:%02d"), hour,
        min, sec);
  }

  /* minutes:seconds */
  /* Translators: This is a time format, like "5:02" for 5
   * minutes and 2 seconds. You may change ":" to the
   * separator that your locale uses or use "%Id" instead of
   * "%d" if your locale uses localized digits.
   */
  return g_strdup_printf (C_ ("short time format", "%d:%02d"), min, sec);
}

gint64
totem_string_to_time (const char *time_string)
{
  int sec, min, hour, args;

  args =
      sscanf (time_string, C_ ("long time format", "%d:%02d:%02d"), &hour, &min,
      &sec);

  if (args == 3) {
    /* Parsed all three arguments successfully */
    return (hour * (60 * 60) + min * 60 + sec) * 1000;
  } else if (args == 2) {
    /* Only parsed the first two arguments; treat hour and min as min and sec, respectively */
    return (hour * 60 + min) * 1000;
  } else if (args == 1) {
    /* Only parsed the first argument; treat hour as sec */
    return hour * 1000;
  } else {
    /* Error! */
    return -1;
  }
}

char *
totem_time_to_string_text (gint64 msecs)
{
  char *secs, *mins, *hours, *string;
  int sec, min, hour, time;

  time = (int) (msecs / 1000);
  sec = time % 60;
  time = time - sec;
  min = (time % (60 * 60)) / 60;
  time = time - (min * 60);
  hour = time / (60 * 60);

  hours = g_strdup_printf (ngettext ("%d hour", "%d hours", hour), hour);

  mins = g_strdup_printf (ngettext ("%d minute", "%d minutes", min), min);

  secs = g_strdup_printf (ngettext ("%d second", "%d seconds", sec), sec);

  if (hour > 0) {
    /* hour:minutes:seconds */
    string = g_strdup_printf (_("%s %s %s"), hours, mins, secs);
  } else if (min > 0) {
    /* minutes:seconds */
    string = g_strdup_printf (_("%s %s"), mins, secs);
  } else if (sec > 0) {
    /* seconds */
    string = g_strdup_printf (_("%s"), secs);
  } else {
    /* 0 seconds */
    string = g_strdup (_("0 seconds"));
  }

  g_free (hours);
  g_free (mins);
  g_free (secs);

  return string;
}

typedef struct _TotemPrefSize
{
  gint width, height;
  gulong sig_id;
} TotemPrefSize;

static gboolean
cb_unset_size (gpointer data)
{
  GtkWidget *widget = data;

  gtk_widget_queue_resize_no_redraw (widget);

  return FALSE;
}

static void
cb_set_preferred_size (GtkWidget * widget, GtkRequisition * req, gpointer data)
{
  TotemPrefSize *size = data;

  req->width = size->width;
  req->height = size->height;

  g_signal_handler_disconnect (widget, size->sig_id);
  g_free (size);
  g_idle_add (cb_unset_size, widget);
}

void
totem_widget_set_preferred_size (GtkWidget * widget, gint width, gint height)
{
  TotemPrefSize *size = g_new (TotemPrefSize, 1);

  size->width = width;
  size->height = height;
  size->sig_id = g_signal_connect (widget, "size-request",
      G_CALLBACK (cb_set_preferred_size), size);

  gtk_widget_queue_resize (widget);
}

gboolean
totem_ratio_fits_screen (GdkWindow * video_window, int video_width,
    int video_height, gfloat ratio)
{
  GdkRectangle fullscreen_rect;
  int new_w, new_h;
  GdkScreen *screen;

  if (video_width <= 0 || video_height <= 0)
    return TRUE;

  new_w = video_width * ratio;
  new_h = video_height * ratio;

  screen = gdk_drawable_get_screen (GDK_DRAWABLE (video_window));
  gdk_screen_get_monitor_geometry (screen,
      gdk_screen_get_monitor_at_window
      (screen, video_window), &fullscreen_rect);

  if (new_w > (fullscreen_rect.width - 128) ||
      new_h > (fullscreen_rect.height - 128)) {
    return FALSE;
  }

  return TRUE;
}

guintptr
gst_get_window_handle(GdkWindow *window)
{
  guintptr window_handle;

  /* Retrieve window handler from GDK */
#if defined (GDK_WINDOWING_WIN32)
  window_handle = (guintptr)GDK_WINDOW_HWND (window);
#elif defined (GDK_WINDOWING_QUARTZ)
  window_handle = gdk_quartz_window_get_nsview (window);
#elif defined (GDK_WINDOWING_X11)
  window_handle = GDK_WINDOW_XID (window);
#endif

  return window_handle;
}

void
gst_set_window_handle(GstXOverlay *xoverlay, guintptr window_handle)
{
  gst_x_overlay_set_window_handle (xoverlay, window_handle);
}

void
init_backend (int argc, char **argv)
{
  gst_init(&argc, &argv);
}

gchar *
lgm_filename_to_uri (const gchar *filename)
{
  gchar *uri, *path;
  GError *err = NULL;

#ifdef G_OS_WIN32
  if (g_path_is_absolute(filename) || !gst_uri_is_valid (filename)) {
#else
  if (!gst_uri_is_valid (filename)) {
#endif
    if (!g_path_is_absolute (filename)) {
      gchar *cur_dir;

      cur_dir = g_get_current_dir ();
      path = g_build_filename (cur_dir, filename, NULL);
      g_free (cur_dir);
    } else {
      path = g_strdup (filename);
    }

    uri = g_filename_to_uri (path, NULL, &err);
    g_free (path);
    path = NULL;

    if (err != NULL) {
      g_error_free (err);
      return NULL;
    }
  } else {
    uri = g_strdup (filename);
  }
  return uri;
}

GstDiscovererResult
lgm_discover_uri (
    const gchar *filename, guint64 *duration, guint *width,
    guint *height, guint *fps_n, guint *fps_d, guint *par_n, guint *par_d,
    gchar **container, gchar **video_codec, gchar **audio_codec,
    GError **err)
{
  GstDiscoverer *discoverer;
  GstDiscovererInfo *info;
  GList *videos = NULL, *audios = NULL;
  GstDiscovererStreamInfo *sinfo = NULL;
  GstDiscovererVideoInfo *vinfo = NULL;
  GstDiscovererAudioInfo *ainfo = NULL;
  GstDiscovererResult ret;
  gchar *uri;

  uri = lgm_filename_to_uri (filename);
  if (uri == NULL) {
    return GST_DISCOVERER_URI_INVALID;
  }

  *duration = *width = *height = *fps_n = *fps_d = *par_n = *par_d = 0;
  *container = *audio_codec = *video_codec = NULL;

  discoverer = gst_discoverer_new (4 * GST_SECOND, err);
  if (*err != NULL) {
    g_free (uri);
    return GST_DISCOVERER_ERROR;
  }

  info = gst_discoverer_discover_uri (discoverer, uri, err);
  g_free (uri);
  if (*err != NULL) {
    if (info != NULL) {
      return gst_discoverer_info_get_result (info);
    } else {
      return GST_DISCOVERER_ERROR;
    }
  }

  sinfo = gst_discoverer_info_get_stream_info (info);
  *duration = gst_discoverer_info_get_duration (info);

  if (GST_IS_DISCOVERER_CONTAINER_INFO (sinfo)) {
    GstCaps *caps;

    caps = gst_discoverer_stream_info_get_caps (
        GST_DISCOVERER_STREAM_INFO(sinfo));
    *container = gst_pb_utils_get_codec_description (caps);
    gst_caps_unref (caps);
  }

  if (GST_IS_DISCOVERER_AUDIO_INFO (sinfo)) {
    ainfo = GST_DISCOVERER_AUDIO_INFO (sinfo);
  } else {
    audios = gst_discoverer_info_get_audio_streams (info);
    if (audios != NULL) {
      ainfo = audios->data;
    }
  }

  if (ainfo != NULL) {
    GstCaps *caps;

    caps = gst_discoverer_stream_info_get_caps (
        GST_DISCOVERER_STREAM_INFO (ainfo));
    *audio_codec = gst_pb_utils_get_codec_description (caps);
    gst_caps_unref (caps);
  }
  if (audios != NULL) {
    gst_discoverer_stream_info_list_free (audios);
  }

  if (GST_IS_DISCOVERER_VIDEO_INFO (sinfo)) {
    vinfo = GST_DISCOVERER_VIDEO_INFO (sinfo);
  } else {
    videos = gst_discoverer_info_get_video_streams (info);
    if (videos != NULL) {
      vinfo = videos->data;
    }
  }

  if (vinfo != NULL) {
    GstCaps *caps;

    caps = gst_discoverer_stream_info_get_caps (
        GST_DISCOVERER_STREAM_INFO (vinfo));
    *video_codec = gst_pb_utils_get_codec_description (caps);
    gst_caps_unref (caps);
    *height = gst_discoverer_video_info_get_height (vinfo);
    *width = gst_discoverer_video_info_get_width (vinfo);
    *fps_n = gst_discoverer_video_info_get_framerate_num (vinfo);
    *fps_d = gst_discoverer_video_info_get_framerate_denom (vinfo);
    *par_n = gst_discoverer_video_info_get_par_num (vinfo);
    *par_d = gst_discoverer_video_info_get_par_denom (vinfo);
  }
  if (videos != NULL) {
    gst_discoverer_stream_info_list_free (videos);
  }

  ret = gst_discoverer_info_get_result (info);
  gst_discoverer_info_unref (info);
  g_object_unref (discoverer);

  return ret;
}

GstElement * lgm_create_video_encoder (VideoEncoderType type, guint quality,
    GQuark quark, GError ** err)
{
  GstElement * encoder = NULL;
  gchar *name = NULL;

  switch (type) {
    case VIDEO_ENCODER_MPEG4:
      encoder = gst_element_factory_make ("ffenc_mpeg4", "video-encoder");
      g_object_set (encoder, "pass", 512,
          "max-key-interval", -1,
          "bitrate", 4000000 / 100, NULL);
      name = "FFmpeg mpeg4 video encoder";
      break;

    case VIDEO_ENCODER_XVID:
      encoder = gst_element_factory_make ("xvidenc", "video-encoder");
      g_object_set (encoder, "pass", 3,
          "profile", 146, "max-key-interval", -1,
          "quantizer", quality * 31 / 100, NULL);
      name = "Xvid video encoder";
      break;

    case VIDEO_ENCODER_H264:
      encoder = gst_element_factory_make ("x264enc", "video-encoder");
      g_object_set (encoder, "key-int-max", 25, "pass", 17,
          "speed-preset", 3,
          "bitrate", 4000 * quality / 100, NULL);
      name = "X264 video encoder";
      break;

    case VIDEO_ENCODER_THEORA:
      encoder = gst_element_factory_make ("theoraenc", "video-encoder");
      g_object_set (encoder, "keyframe-auto", FALSE,
          "keyframe-force", 25,
          "quality", quality * 63 / 100, NULL);
      name = "Theora video encoder";
      break;

    case VIDEO_ENCODER_VP8:
    default:
      encoder = gst_element_factory_make ("vp8enc", "video-encoder");
      g_object_set (encoder, "speed", 2, "threads", 8,
          "max-keyframe-distance", 25,
          "quality", (gdouble) quality * 10 / 100, NULL);
      name = "VP8 video encoder";
      break;

  }
  if (!encoder) {
    g_set_error (err,
        quark,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", name);
    return NULL;
  }

  return encoder;
}

GstElement * lgm_create_audio_encoder (AudioEncoderType type, guint quality,
    GQuark quark, GError ** err)
{
  GstElement *encoder = NULL;
  gchar *name = NULL;

  switch (type) {
    case AUDIO_ENCODER_MP3:
      encoder = gst_element_factory_make ("lamemp3enc", "audio-encoder");
      g_object_set (encoder, "target", 0,
          "quality", (gfloat) quality * 10 / 100, NULL);
      name = "Mp3 audio encoder";
      break;

    case AUDIO_ENCODER_AAC:
      encoder = gst_element_factory_make ("faac", "audio-encoder");
      g_object_set (encoder, "bitrate", quality *  320000 / 100, NULL);
      name = "AAC audio encoder";
      break;

    case AUDIO_ENCODER_VORBIS:
    default:
      encoder = gst_element_factory_make ("vorbisenc", "audio-encoder");
      g_object_set (encoder, "quality", (gfloat) quality / 100, NULL);
      name = "Vorbis audio encoder";
      break;
  }

  if (!encoder) {
    g_set_error (err,
        quark,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", name);
    return NULL;
  }

  return encoder;
}

GstElement * lgm_create_muxer (VideoMuxerType type, GQuark quark, GError **err)
{
  GstElement *muxer = NULL;
  gchar *name = NULL;

  switch (type) {
    case VIDEO_MUXER_OGG:
      name = "OGG muxer";
      muxer = gst_element_factory_make ("oggmux", "video-muxer");
      break;
    case VIDEO_MUXER_AVI:
      name = "AVI muxer";
      muxer = gst_element_factory_make ("avimux", "video-muxer");
      break;
    case VIDEO_MUXER_MATROSKA:
      name = "Matroska muxer";
      muxer = gst_element_factory_make ("matroskamux", "video-muxer");
      break;
    case VIDEO_MUXER_MP4:
      name = "MP4 muxer";
      muxer = gst_element_factory_make ("qtmux", "video-muxer");
      break;
    case VIDEO_MUXER_WEBM:
    default:
      name = "WebM muxer";
      muxer = gst_element_factory_make ("webmmux", "video-muxer");
      break;
  }

  if (!muxer) {
    g_set_error (err,
        quark,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.", name);
    return muxer;
  }

  return muxer;
}
