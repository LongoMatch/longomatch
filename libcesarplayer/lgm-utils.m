/*
 * Copyright (C) 2009-2014  Andoni Morales Alastruey <ylatuya@gmail.com>
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

#include "lgm-utils.h"

#if defined (GDK_WINDOWING_X11)
#include <gdk/gdkx.h>
#elif defined (GDK_WINDOWING_WIN32)
#include <gdk/gdkwin32.h>
#elif defined (GDK_WINDOWING_QUARTZ)
#include <gdk/gdkquartz.h>
#endif

GstAutoplugSelectResult
lgm_filter_video_decoders (GstElement * object, GstPad * arg0,
    GstCaps * arg1, GstElementFactory * arg2, gpointer user_data)
{
  const gchar *name = gst_plugin_feature_get_name (GST_PLUGIN_FEATURE (arg2));
  if (!g_strcmp0 (name, "fluvadec")) {
    return GST_AUTOPLUG_SELECT_SKIP;
  }
  return GST_AUTOPLUG_SELECT_TRY;
}

guintptr
lgm_get_window_handle (GdkWindow * window)
{
  guintptr window_handle;

  gdk_window_ensure_native (window);

  /* Retrieve window handler from GDK */
#if defined (GDK_WINDOWING_WIN32)
  window_handle = (guintptr) GDK_WINDOW_HWND (window);
#elif defined (GDK_WINDOWING_QUARTZ)
  window_handle = (guintptr) gdk_quartz_window_get_nsview (window);
#elif defined (GDK_WINDOWING_X11)
  window_handle = (guintptr) GDK_WINDOW_XID (window);
#endif

  return window_handle;
}

void
lgm_set_window_handle (GstXOverlay * xoverlay, guintptr window_handle)
{
  gst_x_overlay_set_window_handle (xoverlay, window_handle);
}

void
lgm_init_backend (int argc, char **argv)
{
  gst_init (&argc, &argv);
}

gchar *
lgm_filename_to_uri (const gchar * filename)
{
  gchar *uri, *path;
  GError *err = NULL;

#ifdef G_OS_WIN32
  if (g_path_is_absolute (filename) || !gst_uri_is_valid (filename)) {
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
lgm_discover_uri (const gchar * filename, guint64 * duration, guint * width,
    guint * height, guint * fps_n, guint * fps_d, guint * par_n, guint * par_d,
    gchar ** container, gchar ** video_codec, gchar ** audio_codec,
    GError ** err)
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

    caps =
        gst_discoverer_stream_info_get_caps (GST_DISCOVERER_STREAM_INFO
        (sinfo));
    *container = gst_pb_utils_get_codec_description (caps);
    gst_caps_unref (caps);
  }

  if (GST_IS_DISCOVERER_AUDIO_INFO (sinfo)) {
    ainfo = GST_DISCOVERER_AUDIO_INFO (sinfo);
  } else {
    audios = gst_discoverer_info_get_audio_streams (info);
    if (audios != NULL) {
      ainfo = (GstDiscovererAudioInfo *) audios->data;
    }
  }

  if (ainfo != NULL) {
    GstCaps *caps;

    caps =
        gst_discoverer_stream_info_get_caps (GST_DISCOVERER_STREAM_INFO
        (ainfo));
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
      vinfo = (GstDiscovererVideoInfo *) videos->data;
    }
  }

  if (vinfo != NULL) {
    GstCaps *caps;

    caps =
        gst_discoverer_stream_info_get_caps (GST_DISCOVERER_STREAM_INFO
        (vinfo));
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

GstElement *
lgm_create_video_encoder (VideoEncoderType type, guint quality,
    gboolean realtime, GQuark quark, GError ** err, gboolean hardware_acceleration)
{
  GstElement *encoder = NULL;
  gchar *name = NULL;

  switch (type) {
    case VIDEO_ENCODER_MPEG4:
      encoder = gst_element_factory_make ("ffenc_mpeg4", "video-encoder");
      g_object_set (encoder, "pass", 512,
          "max-key-interval", -1, "bitrate", quality * 1000, NULL);
      name = "FFmpeg mpeg4 video encoder";
      break;

    case VIDEO_ENCODER_XVID:
      encoder = gst_element_factory_make ("xvidenc", "video-encoder");
      g_object_set (encoder, "pass", 3,
          "profile", 146, "max-key-interval", -1,
          "bitrate", quality * 1000, NULL);
      name = "Xvid video encoder";
      break;

    case VIDEO_ENCODER_H264:{
      GstElement *parse;

      if (hardware_acceleration) {
        encoder = gst_element_factory_make ("fluvah264enc", "video-encoder");

        if (encoder != NULL) {
          g_object_set (encoder, "bitrate", quality, "keyframe-period", 1000,
              "rate-control", 1, "entropy-mode", 0, "allow-b-frames", FALSE,
              "profile", 1, NULL);
          if (realtime) {
              g_object_set (encoder, "realtime", TRUE, NULL);
          }
          name = "Fluendo VA H264 video encoder";
        } else {
          encoder = gst_element_factory_make ("fluh264enc", "video-encoder");
          if (encoder != NULL) {
            g_object_set (encoder, "keyframe-period", 1000, "rate-control", 1,
                "max-threads", 0, "profile", 1, "entropy-mode", 0,
                "quality-speed", 0, "bitrate", quality, NULL);
            name = "Fluendo IPP H264 video encoder";
          }
        }
      }
      parse = gst_element_factory_make ("h264parse", NULL);
      if (encoder != NULL && parse != NULL) {
        GstPad *encoder_sink_pad, *parse_src_pad;
        GstElement *bin = gst_bin_new (NULL);


        gst_bin_add_many (GST_BIN (bin), encoder, parse, NULL);
        gst_element_link (encoder, parse);

        encoder_sink_pad = gst_element_get_static_pad (encoder, "sink");
        parse_src_pad = gst_element_get_static_pad (parse, "src");

        gst_element_add_pad (bin, gst_ghost_pad_new ("sink", encoder_sink_pad));
        gst_element_add_pad (bin, gst_ghost_pad_new ("src", parse_src_pad));

        gst_object_unref (encoder_sink_pad);
        gst_object_unref (parse_src_pad);

        encoder = bin;
      } else {
        gchar *stats_file;

        if (encoder)
          gst_object_unref (encoder);
        if (parse)
          gst_object_unref (parse);
        stats_file = g_build_path (G_DIR_SEPARATOR_S, g_get_tmp_dir (),
            "x264.log", NULL);
        encoder = gst_element_factory_make ("x264enc", "video-encoder");
        g_object_set (encoder, "key-int-max", 25, "pass", 17,
            "speed-preset", 5, "stats-file", stats_file,
            "bitrate", quality, NULL);
        g_free (stats_file);
        name = "X264 video encoder";
      }
      break;
    }

    case VIDEO_ENCODER_THEORA:
      encoder = gst_element_factory_make ("theoraenc", "video-encoder");
      g_object_set (encoder, "keyframe-auto", FALSE,
          "keyframe-force", 25, "bitrate", quality, NULL);
      name = "Theora video encoder";
      break;

    case VIDEO_ENCODER_VP8:
    default:
      encoder = gst_element_factory_make ("vp8enc", "video-encoder");
      g_object_set (encoder, "speed", 2, "threads", 8,
          "max-keyframe-distance", 25,
          "bitrate", (gdouble) quality * 1000, NULL);
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

GstElement *
lgm_create_audio_encoder (AudioEncoderType type, guint quality,
    GQuark quark, GError ** err)
{
  GstElement *encoder = NULL;
  gchar *name = NULL;

  switch (type) {
    case AUDIO_ENCODER_MP3:
      encoder = gst_element_factory_make ("flump3enc", "audio-encoder");
      if (encoder != NULL) {
        g_object_set (encoder, "bitrate", 128, NULL);
      } else {
        encoder = gst_element_factory_make ("lamemp3enc", "audio-encoder");
        g_object_set (encoder, "target", 0, "quality", (gfloat) 4, NULL);
      }
      name = "Mp3 audio encoder";
      break;

    case AUDIO_ENCODER_AAC:
      encoder = gst_element_factory_make ("fluaacenc", "audio-encoder");
      if (encoder != NULL) {
        g_object_set (encoder, "bitrate", 128, "outputformat", 2, NULL);
      } else {
        encoder = gst_element_factory_make ("faac", "audio-encoder");
        g_object_set (encoder, "bitrate", 128000, NULL);
      }
      name = "AAC audio encoder";
      break;

    case AUDIO_ENCODER_VORBIS:
    default:
      encoder = gst_element_factory_make ("vorbisenc", "audio-encoder");
      g_object_set (encoder, "quality", 0.3, NULL);
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

GstElement *
lgm_create_muxer (VideoMuxerType type, GQuark quark, GError ** err)
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
