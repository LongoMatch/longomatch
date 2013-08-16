 /*GStreamer Video Editor Based On GNonlin
  * Copyright (C) 2007-2009 Andoni Morales Alastruey <ylatuya@gmail.com>
  *
  * This program is free software.
  *
  * You may redistribute it and/or modify it under the terms of the
  * GNU General Public License, as published by the Free Software
  * Foundation; either version 2 of the License, or (at your option)
  * any later version.
  *
  * Gstreamer Video Transcoderis distributed in the hope that it will be useful,
  * but WITHOUT ANY WARRANTY; without even the implied warranty of
  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
  * See the GNU General Public License for more details.
  *
  * You should have received a copy of the GNU General Public License
  * along with foob.  If not, write to:
  *     The Free Software Foundation, Inc.,
  *     51 Franklin Street, Fifth Floor
  *     Boston, MA  02110-1301, USA.
  */

#include <string.h>
#include <stdio.h>
#include <gst/gst.h>
#include "gst-video-editor.h"
#include "video-utils.h"


#define AUDIO_INT_CAPS "audio/x-raw-int, rate=44100, channels=2"
#define AUDIO_FLOAT "audio/x-raw-float, rate=44100, channels=2"

#define TIMEOUT 50

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_PERCENT_COMPLETED,
  LAST_SIGNAL
};

/* Properties */
enum
{
  PROP_0,
};

struct GstVideoEditorPrivate
{
  gint segments;
  gint active_segment;
  gint64 *stop_times;
  GList *titles;
  GList *gnl_video_filesources;
  GList *gnl_audio_filesources;
  gint64 duration;

  /* Properties */
  gboolean audio_enabled;
  gboolean title_enabled;
  gchar *output_file;
  guint audio_quality;
  guint video_quality;
  guint width;
  guint height;
  guint fps_n;
  guint fps_d;
  guint title_size;
  VideoEncoderType video_encoder_type;
  AudioEncoderType audio_encoder_type;
  VideoMuxerType muxer_type;

  /* Bins */
  GstElement *main_pipeline;
  GstElement *vencode_bin;
  GstElement *aencode_bin;

  /* Source */
  GstElement *gnl_video_composition;
  GstElement *gnl_audio_composition;

  /* Video */
  GstElement *identity;
  GstElement *ffmpegcolorspace;
  GstElement *videorate;
  GstElement *textoverlay;
  GstElement *videoscale;
  GstElement *capsfilter;
  GstElement *queue;
  GstElement *video_encoder;

  /* Audio */
  GstElement *audioidentity;
  GstElement *audioconvert;
  GstElement *audioresample;
  GstElement *audiocapsfilter;
  GstElement *audioqueue;
  GstElement *audioencoder;

  /* Sink */
  GstElement *muxer;
  GstElement *file_sink;

  GstBus *bus;
  gulong sig_bus_async;

  gint update_id;
};

static int gve_signals[LAST_SIGNAL] = { 0 };

static void gve_error_msg (GstVideoEditor * gve, GstMessage * msg);
static void new_decoded_pad_cb (GstElement * object, GstPad * arg0,
    gpointer user_data);
static void gve_bus_message_cb (GstBus * bus, GstMessage * message,
    gpointer data);
static gboolean gve_query_timeout (GstVideoEditor * gve);
static void gve_apply_title_size (GstVideoEditor * gve, gint size);
G_DEFINE_TYPE (GstVideoEditor, gst_video_editor, G_TYPE_OBJECT);



/* =========================================== */
/*                                             */
/*      Class Initialization/Finalization      */
/*                                             */
/* =========================================== */

static void
gst_video_editor_init (GstVideoEditor * object)
{
  GstVideoEditorPrivate *priv;
  object->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_VIDEO_EDITOR,
      GstVideoEditorPrivate);

  priv->output_file = "new_video.avi";

  priv->audio_quality = 50;
  priv->video_quality = 50;
  priv->height = 540;
  priv->width = 720;
  priv->title_size = 20;
  priv->title_enabled = TRUE;
  priv->audio_enabled = TRUE;

  priv->duration = 0;
  priv->segments = 0;
  priv->gnl_video_filesources = NULL;
  priv->gnl_audio_filesources = NULL;
  priv->titles = NULL;
  priv->stop_times = (gint64 *) malloc (200 * sizeof (gint64));

  priv->update_id = 0;
}

static void
gst_video_editor_finalize (GObject * object)
{
  GstVideoEditor *gve = (GstVideoEditor *) object;

  if (gve->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
       called again (main loop might be run again to display error dialog) */
    gst_bus_set_flushing (gve->priv->bus, TRUE);

    if (gve->priv->sig_bus_async)
      g_signal_handler_disconnect (gve->priv->bus, gve->priv->sig_bus_async);
    gst_object_unref (gve->priv->bus);
    gve->priv->bus = NULL;
  }

  if (gve->priv->main_pipeline != NULL
      && GST_IS_ELEMENT (gve->priv->main_pipeline)) {
    gst_element_set_state (gve->priv->main_pipeline, GST_STATE_NULL);
    gst_object_unref (gve->priv->main_pipeline);
    gve->priv->main_pipeline = NULL;
  }

  g_free (gve->priv->output_file);
  g_list_free (gve->priv->gnl_video_filesources);
  g_list_free (gve->priv->gnl_audio_filesources);
  g_free (gve->priv->stop_times);
  g_list_free (gve->priv->titles);

  G_OBJECT_CLASS (gst_video_editor_parent_class)->finalize (object);
}


static void
gst_video_editor_class_init (GstVideoEditorClass * klass)
{
  GObjectClass *object_class = G_OBJECT_CLASS (klass);

  object_class->finalize = gst_video_editor_finalize;

  g_type_class_add_private (object_class, sizeof (GstVideoEditorPrivate));

  /* GObject */
  object_class->finalize = gst_video_editor_finalize;

  /* Signals */
  gve_signals[SIGNAL_ERROR] =
      g_signal_new ("error",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstVideoEditorClass, error),
      NULL, NULL,
      g_cclosure_marshal_VOID__STRING, G_TYPE_NONE, 1, G_TYPE_STRING);

  gve_signals[SIGNAL_PERCENT_COMPLETED] =
      g_signal_new ("percent_completed",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstVideoEditorClass, percent_completed),
      NULL, NULL, g_cclosure_marshal_VOID__FLOAT, G_TYPE_NONE, 1, G_TYPE_FLOAT);
}

/* =========================================== */
/*                                             */
/*               Private Methods               */
/*                                             */
/* =========================================== */

static void
gve_set_tick_timeout (GstVideoEditor * gve, guint msecs)
{
  g_return_if_fail (msecs > 0);

  GST_INFO ("adding tick timeout (at %ums)", msecs);
  gve->priv->update_id =
      g_timeout_add (msecs, (GSourceFunc) gve_query_timeout, gve);
}

static void
gve_apply_title_size (GstVideoEditor * gve, gint size)
{
  gchar *font;

  font = g_strdup_printf ("sans bold %d", size);
  g_object_set (G_OBJECT (gve->priv->textoverlay), "font-desc", font, NULL);
  g_free (font);
}

static void
gve_set_overlay_title (GstVideoEditor *gve, gchar *title)
{
  glong length;

  if (title == NULL)
    return;

  g_object_set (G_OBJECT (gve->priv->textoverlay), "text", title, NULL);

  length = g_utf8_strlen (title, -1);
  if (length * gve->priv->title_size > gve->priv->width) {
    gve_apply_title_size (gve, gve->priv->width / length - 1);
  } else {
    gve_apply_title_size (gve, gve->priv->title_size);
  }
}

GQuark
gst_video_editor_error_quark (void)
{
  static GQuark q;              /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("gve-error-quark");
  }
  return q;
}

static void
gve_create_video_encode_bin (GstVideoEditor * gve)
{
  GstPad *sinkpad = NULL;
  GstPad *srcpad = NULL;
  GError *error = NULL;
  GstCaps *caps;

  gve->priv->vencode_bin = gst_element_factory_make ("bin", "vencodebin");
  gve->priv->identity = gst_element_factory_make ("identity", "identity");
  gve->priv->ffmpegcolorspace =
      gst_element_factory_make ("ffmpegcolorspace", "ffmpegcolorspace");
  gve->priv->videorate = gst_element_factory_make ("videorate", "videorate");
  gve->priv->videoscale = gst_element_factory_make ("videoscale", "videoscale");
  gve->priv->capsfilter = gst_element_factory_make ("capsfilter", "capsfilter");
  gve->priv->textoverlay =
      gst_element_factory_make ("textoverlay", "textoverlay");
  gve->priv->queue = gst_element_factory_make ("queue", "video-encode-queue");
  gve->priv->video_encoder = lgm_create_video_encoder (
      gve->priv->video_encoder_type, gve->priv->video_quality,
      GVE_ERROR, &error);
  if (error) {
    g_signal_emit (gve, gve_signals[SIGNAL_ERROR], 0, error->message);
    g_error_free (error);
    return;
  }

  caps = gst_caps_new_simple ("video/x-raw-yuv",
      "width", G_TYPE_INT, (gint) gve->priv->width,
      "height", G_TYPE_INT, (gint) gve->priv->height,
      "pixel-aspect-ratio", GST_TYPE_FRACTION, 1, 1,
      "framerate", GST_TYPE_FRACTION,
      (gint) gve->priv->fps_n, (gint) gve->priv->fps_d, NULL);
  g_object_set (G_OBJECT (gve->priv->capsfilter), "caps", caps, NULL);
  gst_caps_unref (caps);
  g_object_set (G_OBJECT (gve->priv->identity), "single-segment", TRUE, NULL);
  g_object_set (G_OBJECT (gve->priv->textoverlay),
      "valignment", 2, "halignment", 2, "auto-resize", TRUE, "wrap-mode", 0, NULL);
  gve_apply_title_size (gve, gve->priv->title_size);
  if (!gve->priv->title_enabled) {
    g_object_set (G_OBJECT (gve->priv->textoverlay), "silent", TRUE, NULL);
  }
  g_object_set (G_OBJECT (gve->priv->videoscale), "add-borders", TRUE, NULL);

  /*Add and link elements */
  gst_bin_add_many (GST_BIN (gve->priv->vencode_bin),
      gve->priv->identity,
      gve->priv->ffmpegcolorspace,
      gve->priv->videorate,
      gve->priv->videoscale,
      gve->priv->capsfilter,
      gve->priv->textoverlay, gve->priv->queue, gve->priv->video_encoder, NULL);
  gst_element_link_many (gve->priv->identity,
      gve->priv->ffmpegcolorspace,
      gve->priv->videoscale,
      gve->priv->videorate,
      gve->priv->capsfilter,
      gve->priv->textoverlay, gve->priv->queue, gve->priv->video_encoder, NULL);

  /*Create bin sink pad */
  sinkpad = gst_element_get_static_pad (gve->priv->identity, "sink");
  gst_pad_set_active (sinkpad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gve->priv->vencode_bin),
      gst_ghost_pad_new ("sink", sinkpad));

  /*Creat bin src pad */
  srcpad = gst_element_get_static_pad (gve->priv->video_encoder, "src");
  gst_pad_set_active (srcpad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gve->priv->vencode_bin),
      gst_ghost_pad_new ("src", srcpad));

  g_object_unref (srcpad);
  g_object_unref (sinkpad);
}

static void
gve_create_audio_encode_bin (GstVideoEditor * gve)
{
  GstPad *sinkpad = NULL;
  GstPad *srcpad = NULL;
  GError *error = NULL;
  GstCaps *caps;

  if (gve->priv->aencode_bin != NULL)
    return;

  gve->priv->aencode_bin = gst_element_factory_make ("bin", "aencodebin");
  gve->priv->audioidentity =
      gst_element_factory_make ("identity", "audio-identity");
  gve->priv->audioconvert =
      gst_element_factory_make ("audioconvert", "audioconvert");
  gve->priv->audioresample =
      gst_element_factory_make ("audioresample", "audioresample");
  gve->priv->audiocapsfilter =
      gst_element_factory_make ("capsfilter", "audiocapsfilter");
  gve->priv->audioqueue = gst_element_factory_make ("queue", "audio-queue");
  gve->priv->audioencoder = lgm_create_audio_encoder (
      gve->priv->audio_encoder_type, gve->priv->audio_quality, GVE_ERROR,
      &error);
  if (error) {
    g_signal_emit (gve, gve_signals[SIGNAL_ERROR], 0, error->message);
    g_error_free (error);
    return;
  }

  g_object_set (G_OBJECT (gve->priv->audioidentity), "single-segment", TRUE,
      NULL);
  caps = gst_caps_from_string (AUDIO_INT_CAPS ";" AUDIO_FLOAT);
  g_object_set (G_OBJECT (gve->priv->audiocapsfilter), "caps", caps, NULL);
  gst_caps_unref (caps);

  /*Add and link elements */
  gst_bin_add_many (GST_BIN (gve->priv->aencode_bin),
      gve->priv->audioidentity,
      gve->priv->audioconvert,
      gve->priv->audioresample,
      gve->priv->audiocapsfilter,
      gve->priv->audioqueue, gve->priv->audioencoder, NULL);

  gst_element_link_many (gve->priv->audioidentity,
      gve->priv->audioconvert,
      gve->priv->audioresample,
      gve->priv->audiocapsfilter,
      gve->priv->audioqueue, gve->priv->audioencoder, NULL);

  /*Create bin sink pad */
  sinkpad = gst_element_get_static_pad (gve->priv->audioidentity, "sink");
  gst_pad_set_active (sinkpad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gve->priv->aencode_bin),
      gst_ghost_pad_new ("sink", sinkpad));

  /*Creat bin src pad */
  srcpad = gst_element_get_static_pad (gve->priv->audioencoder, "src");
  gst_pad_set_active (srcpad, TRUE);
  gst_element_add_pad (GST_ELEMENT (gve->priv->aencode_bin),
      gst_ghost_pad_new ("src", srcpad));

  g_object_unref (srcpad);
  g_object_unref (sinkpad);
}

/* =========================================== */
/*                                             */
/*                Callbacks                    */
/*                                             */
/* =========================================== */

static void
new_decoded_pad_cb (GstElement * object, GstPad * pad, gpointer user_data)
{
  GstCaps *caps = NULL;
  GstStructure *str = NULL;
  GstPad *videopad = NULL;
  GstPad *audiopad = NULL;
  GstVideoEditor *gve = NULL;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (user_data));
  gve = GST_VIDEO_EDITOR (user_data);

  /* check media type */
  caps = gst_pad_get_caps (pad);
  str = gst_caps_get_structure (caps, 0);

  if (g_strrstr (gst_structure_get_name (str), "video")) {
    videopad =
        gst_element_get_compatible_pad (gve->priv->vencode_bin, pad, NULL);
    /* only link once */
    if (GST_PAD_IS_LINKED (videopad)) {
      g_object_unref (videopad);
      gst_caps_unref (caps);
      return;
    }
    /* link 'n play */
    GST_INFO ("Found video stream...%" GST_PTR_FORMAT, caps);
    gst_pad_link (pad, videopad);
    g_object_unref (videopad);
  }

  else if (g_strrstr (gst_structure_get_name (str), "audio")
      && gve->priv->audio_enabled) {
    audiopad =
        gst_element_get_compatible_pad (gve->priv->aencode_bin, pad, NULL);
    /* only link once */
    if (GST_PAD_IS_LINKED (audiopad)) {
      g_object_unref (audiopad);
      gst_caps_unref (caps);
      return;
    }
    /* link 'n play */
    GST_INFO ("Found audio stream...%" GST_PTR_FORMAT, caps);
    gst_pad_link (pad, audiopad);
    g_object_unref (audiopad);
  }

  gst_caps_unref (caps);
}

static void
gve_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  GstVideoEditor *gve = (GstVideoEditor *) data;
  GstMessageType msg_type;

  g_return_if_fail (gve != NULL);
  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  msg_type = GST_MESSAGE_TYPE (message);

  switch (msg_type) {
    case GST_MESSAGE_ERROR:
      gve_error_msg (gve, message);
      if (gve->priv->main_pipeline)
        gst_element_set_state (gve->priv->main_pipeline, GST_STATE_READY);
      break;
    case GST_MESSAGE_WARNING:
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;

    case GST_MESSAGE_STATE_CHANGED:
    {
      GstState old_state, new_state;
      gchar *src_name;

      gst_message_parse_state_changed (message, &old_state, &new_state, NULL);

      if (old_state == new_state)
        break;

      /* we only care about playbin (pipeline) state changes */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (gve->priv->main_pipeline))
        break;

      src_name = gst_object_get_name (message->src);

      GST_INFO ("%s changed state from %s to %s", src_name,
          gst_element_state_get_name (old_state),
          gst_element_state_get_name (new_state));
      g_free (src_name);

      if (new_state == GST_STATE_PLAYING)
        gve_set_tick_timeout (gve, TIMEOUT);
      if (old_state == GST_STATE_PAUSED && new_state == GST_STATE_READY) {
        if (gve->priv->update_id > 0) {
          g_source_remove (gve->priv->update_id);
          gve->priv->update_id = 0;
        }
      }
      if (old_state == GST_STATE_NULL && new_state == GST_STATE_READY)
        GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN (gve->priv->main_pipeline),
            GST_DEBUG_GRAPH_SHOW_ALL, "gst-camera-capturer-null-to-ready");
      if (old_state == GST_STATE_READY && new_state == GST_STATE_PAUSED)
        GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN (gve->priv->main_pipeline),
            GST_DEBUG_GRAPH_SHOW_ALL, "gst-camera-capturer-ready-to-paused");
      break;
    }
    case GST_MESSAGE_EOS:
      if (gve->priv->update_id > 0) {
        g_source_remove (gve->priv->update_id);
        gve->priv->update_id = 0;
      }
      gst_element_set_state (gve->priv->main_pipeline, GST_STATE_NULL);
      g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0, (gfloat) 1);
      gve->priv->active_segment = 0;
      /* Close file sink properly */
      g_object_set (G_OBJECT (gve->priv->file_sink), "location", "", NULL);
      break;
    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
}

static void
gve_error_msg (GstVideoEditor * gve, GstMessage * msg)
{
  GError *err = NULL;
  gchar *dbg = NULL;

  gst_message_parse_error (msg, &err, &dbg);
  if (err) {
    GST_ERROR ("message = %s", GST_STR_NULL (err->message));
    GST_ERROR ("domain  = %d (%s)", err->domain,
        GST_STR_NULL (g_quark_to_string (err->domain)));
    GST_ERROR ("code    = %d", err->code);
    GST_ERROR ("debug   = %s", GST_STR_NULL (dbg));
    GST_ERROR ("source  = %" GST_PTR_FORMAT, msg->src);

    g_message ("Error: %s\n%s\n", GST_STR_NULL (err->message),
        GST_STR_NULL (dbg));
    g_signal_emit (gve, gve_signals[SIGNAL_ERROR], 0, err->message);
    g_error_free (err);
  }
  g_free (dbg);
}

static gboolean
gve_query_timeout (GstVideoEditor * gve)
{
  GstFormat fmt = GST_FORMAT_TIME;
  gint64 pos = -1;
  gchar *title;
  gint64 stop_time = gve->priv->stop_times[gve->priv->active_segment];

  if (gst_element_query_position (gve->priv->main_pipeline, &fmt, &pos)) {
    if (pos != -1 && fmt == GST_FORMAT_TIME) {
      g_signal_emit (gve,
          gve_signals[SIGNAL_PERCENT_COMPLETED],
          0, (float) pos / (float) gve->priv->duration);
    }
  } else {
    GST_INFO ("could not get position");
  }

  if (gst_element_query_position (gve->priv->video_encoder, &fmt, &pos)) {
    if (stop_time - pos <= 0) {

      gve->priv->active_segment++;
      title =
          (gchar *) g_list_nth_data (gve->priv->titles,
          gve->priv->active_segment);
      gve_set_overlay_title (gve, title);
    }
  }

  return TRUE;
}

/* =========================================== */
/*                                             */
/*              Public Methods                 */
/*                                             */
/* =========================================== */


void
gst_video_editor_add_segment (GstVideoEditor * gve, gchar * file,
    gint64 start, gint64 duration, gdouble rate,
    gchar * title, gboolean hasAudio)
{
  GstState cur_state;
  GstElement *gnl_filesource = NULL;
  GstElement *audiotestsrc = NULL;
  GstCaps *filter = NULL;
  gchar *element_name = "";
  gint64 final_duration;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  gst_element_get_state (gve->priv->main_pipeline, &cur_state, NULL, 0);
  if (cur_state > GST_STATE_READY) {
    GST_WARNING ("Segments can only be added for a state <= GST_STATE_READY");
    return;
  }

  start = GST_MSECOND * start;
  duration = GST_MSECOND * duration;
  final_duration = duration / rate;

  /* Video */
  filter = gst_caps_from_string ("video/x-raw-rgb;video/x-raw-yuv");
  element_name = g_strdup_printf ("gnlvideofilesource%d", gve->priv->segments);
  gnl_filesource = gst_element_factory_make ("gnlfilesource", element_name);
  g_object_set (G_OBJECT (gnl_filesource), "location", file,
      "media-start", start,
      "media-duration", duration,
      "start", gve->priv->duration,
      "duration", final_duration, "caps", filter, NULL);
  gst_bin_add (GST_BIN (gve->priv->gnl_video_composition), gnl_filesource);
  gve->priv->gnl_video_filesources =
      g_list_append (gve->priv->gnl_video_filesources, gnl_filesource);

  /* Audio */
  if (gve->priv->audio_enabled) {
    if (hasAudio && rate == 1) {
      element_name =
          g_strdup_printf ("gnlaudiofilesource%d", gve->priv->segments);
      gnl_filesource = gst_element_factory_make ("gnlfilesource", element_name);
      g_object_set (G_OBJECT (gnl_filesource), "location", file, NULL);
    } else {
      /* If the file doesn't contain audio, something must be playing */
      /* We use an audiotestsrc mutted and with a low priority */
      element_name =
          g_strdup_printf ("gnlaudiofakesource%d", gve->priv->segments);
      gnl_filesource = gst_element_factory_make ("gnlsource", element_name);
      element_name = g_strdup_printf ("audiotestsource%d", gve->priv->segments);
      audiotestsrc = gst_element_factory_make ("audiotestsrc", element_name);
      g_object_set (G_OBJECT (audiotestsrc), "volume", (double) 0, NULL);
      gst_bin_add (GST_BIN (gnl_filesource), audiotestsrc);
    }
    filter = gst_caps_from_string ("audio/x-raw-float;audio/x-raw-int");
    g_object_set (G_OBJECT (gnl_filesource),
        "media-start", start,
        "media-duration", duration,
        "start", gve->priv->duration,
        "duration", final_duration, "caps", filter, NULL);
    gst_bin_add (GST_BIN (gve->priv->gnl_audio_composition), gnl_filesource);
    gve->priv->gnl_audio_filesources =
        g_list_append (gve->priv->gnl_audio_filesources, gnl_filesource);
  }

  GST_INFO ("New segment: start={%" GST_TIME_FORMAT "} duration={%"
      GST_TIME_FORMAT "} ", GST_TIME_ARGS (gve->priv->duration),
      GST_TIME_ARGS (final_duration));

  gve->priv->duration += final_duration;
  gve->priv->segments++;

  gve->priv->titles = g_list_append (gve->priv->titles, title);
  gve->priv->stop_times[gve->priv->segments - 1] = gve->priv->duration;

  g_free (element_name);
}


void
gst_video_editor_add_image_segment (GstVideoEditor * gve, gchar * file,
    guint64 start, gint64 duration, gchar * title)
{
  GstState cur_state;
  GstElement *gnl_filesource = NULL;
  GstElement *imagesourcebin = NULL;
  GstElement *audiotestsrc = NULL;
  GstCaps *filter = NULL;
  gchar *element_name = NULL;
  gchar *desc = NULL;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  gst_element_get_state (gve->priv->main_pipeline, &cur_state, NULL, 0);
  if (cur_state > GST_STATE_READY) {
    GST_WARNING ("Segments can only be added for a state <= GST_STATE_READY");
    return;
  }

  duration = duration * GST_MSECOND;
  start = start * GST_MSECOND;

  /* Video */
  /* gnlsource */
  filter = gst_caps_from_string ("video/x-raw-rgb;video/x-raw-yuv");
  element_name = g_strdup_printf ("gnlvideofilesource%d", gve->priv->segments);
  gnl_filesource = gst_element_factory_make ("gnlsource", element_name);
  g_object_set (G_OBJECT (gnl_filesource),
      "media-start", start,
      "media-duration", duration,
      "start", gve->priv->duration,
      "duration", duration, "caps", filter, NULL);
  g_free(element_name);
  /* filesrc ! pngdec ! ffmpegcolorspace ! imagefreeze */
  desc = g_strdup_printf("filesrc location=%s ! pngdec ! videoscale ! ffmpegcolorspace ! video/x-raw-rgb, pixel-aspect-ratio=1/1 ! imagefreeze  ", file);
  imagesourcebin = gst_parse_bin_from_description(desc, TRUE, NULL);
  g_free(desc);
  gst_bin_add (GST_BIN (gnl_filesource), imagesourcebin);
  gst_bin_add (GST_BIN (gve->priv->gnl_video_composition), gnl_filesource);
  gve->priv->gnl_video_filesources =
      g_list_append (gve->priv->gnl_video_filesources, gnl_filesource);

  /* Audio */
  if (gve->priv->audio_enabled) {
    element_name =
        g_strdup_printf ("gnlaudiofakesource%d", gve->priv->segments);
    gnl_filesource = gst_element_factory_make ("gnlsource", element_name);
    g_free (element_name);
    element_name = g_strdup_printf ("audiotestsource%d", gve->priv->segments);
    audiotestsrc = gst_element_factory_make ("audiotestsrc", element_name);
    g_free (element_name);
    g_object_set (G_OBJECT (audiotestsrc), "volume", (double) 0, NULL);
    gst_bin_add (GST_BIN (gnl_filesource), audiotestsrc);
    filter = gst_caps_from_string ("audio/x-raw-float;audio/x-raw-int");
    g_object_set (G_OBJECT (gnl_filesource),
        "media-start", start,
        "media-duration", duration,
        "start", gve->priv->duration,
        "duration", duration, "caps", filter, NULL);
    gst_bin_add (GST_BIN (gve->priv->gnl_audio_composition), gnl_filesource);
    gve->priv->gnl_audio_filesources =
        g_list_append (gve->priv->gnl_audio_filesources, gnl_filesource);
  }

  GST_INFO ("New segment: start={%" GST_TIME_FORMAT "} duration={%"
      GST_TIME_FORMAT "} ", GST_TIME_ARGS (gve->priv->duration),
      GST_TIME_ARGS (duration));

  gve->priv->duration += duration;
  gve->priv->segments++;

  gve->priv->titles = g_list_append (gve->priv->titles, title);
  gve->priv->stop_times[gve->priv->segments - 1] = gve->priv->duration;

}

void
gst_video_editor_clear_segments_list (GstVideoEditor * gve)
{
  GList *tmp = NULL;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  GST_INFO_OBJECT (gve, "Clearing list of segments");

  gst_video_editor_cancel (gve);

  tmp = gve->priv->gnl_video_filesources;

  for (; tmp; tmp = g_list_next (tmp)) {
    GstElement *object = (GstElement *) tmp->data;
    if (object)
      gst_element_set_state (object, GST_STATE_NULL);
    gst_bin_remove (GST_BIN (gve->priv->gnl_video_composition), object);
  }

  tmp = gve->priv->gnl_audio_filesources;

  for (; tmp; tmp = g_list_next (tmp)) {
    GstElement *object = (GstElement *) tmp->data;
    if (object)
      gst_element_set_state (object, GST_STATE_NULL);
    gst_bin_remove (GST_BIN (gve->priv->gnl_audio_composition), object);
  }

  g_list_free (tmp);
  g_list_free (gve->priv->gnl_video_filesources);
  g_list_free (gve->priv->gnl_audio_filesources);
  g_free (gve->priv->stop_times);
  g_list_free (gve->priv->titles);

  gve->priv->gnl_video_filesources = NULL;
  gve->priv->gnl_audio_filesources = NULL;
  gve->priv->stop_times = (gint64 *) malloc (200 * sizeof (gint64));
  gve->priv->titles = NULL;

  gve->priv->duration = 0;
  gve->priv->active_segment = 0;
}

void
gst_video_editor_start (GstVideoEditor * gve)
{
  GError *error = NULL;

  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  GST_INFO_OBJECT(gve, "Starting");

  /* Create elements */
  gve->priv->muxer = lgm_create_muxer (gve->priv->muxer_type, GVE_ERROR,
      &error);
  if (error) {
    g_signal_emit (gve, gve_signals[SIGNAL_ERROR], 0, error->message);
    g_error_free (error);
    return;
  }
  gve->priv->file_sink = gst_element_factory_make ("filesink", "filesink");
  gve_create_video_encode_bin (gve);

  /* Set elements properties */
  g_object_set (G_OBJECT (gve->priv->file_sink), "location",
      gve->priv->output_file, NULL);

  /* Link elements */
  gst_bin_add_many (GST_BIN (gve->priv->main_pipeline),
      gve->priv->gnl_video_composition,
      gve->priv->vencode_bin,
      gve->priv->muxer, gve->priv->file_sink, NULL);

  gst_element_link_many (gve->priv->vencode_bin,
      gve->priv->muxer, gve->priv->file_sink, NULL);

  if (gve->priv->audio_enabled) {
    gst_bin_add (GST_BIN (gve->priv->main_pipeline),
      gve->priv->gnl_audio_composition);
    gve_create_audio_encode_bin (gve);
    gst_bin_add (GST_BIN (gve->priv->main_pipeline),
        gve->priv->aencode_bin);
    gst_element_link (gve->priv->aencode_bin, gve->priv->muxer);
    g_print ("ADDing audio encode bin and linking to muxer\n");
  }

  gve_set_overlay_title (gve, (gchar *) g_list_nth_data (gve->priv->titles, 0));
  gst_element_set_state (gve->priv->main_pipeline, GST_STATE_PLAYING);
  g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0, (gfloat) 0);
}

void
gst_video_editor_cancel (GstVideoEditor * gve)
{
  g_return_if_fail (GST_IS_VIDEO_EDITOR (gve));

  GST_INFO_OBJECT(gve, "Cancelling");
  if (gve->priv->update_id > 0) {
    g_source_remove (gve->priv->update_id);
    gve->priv->update_id = 0;
  }
  gst_element_set_state (gve->priv->main_pipeline, GST_STATE_NULL);
  g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0, (gfloat) - 1);
}

void
gst_video_editor_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

void
gst_video_editor_set_encoding_format (GstVideoEditor * gve,
    gchar *output_file,
    VideoEncoderType video_codec, AudioEncoderType audio_codec,
    VideoMuxerType muxer, guint video_quality, guint audio_quality,
    guint width, guint height, guint fps_n, guint fps_d,
    gboolean enable_audio, gboolean enable_title)
{
  gve->priv->output_file = g_strdup (output_file);
  gve->priv->video_encoder_type = video_codec;
  gve->priv->audio_encoder_type = audio_codec;
  gve->priv->muxer_type = muxer;
  gve->priv->video_quality = video_quality;
  gve->priv->audio_quality = audio_quality;
  gve->priv->width = width;
  gve->priv->height = height;
  gve->priv->fps_n = fps_n;
  gve->priv->fps_d = fps_d;
  gve->priv->audio_enabled = enable_audio;
  gve->priv->title_enabled = enable_title;
}

GstVideoEditor *
gst_video_editor_new (GError ** err)
{
  GstVideoEditor *gve = NULL;

  gve = g_object_new (GST_TYPE_VIDEO_EDITOR, NULL);

  gve->priv->main_pipeline = gst_pipeline_new ("main_pipeline");

  if (!gve->priv->main_pipeline) {
    g_set_error (err, GVE_ERROR, GST_ERROR_PLUGIN_LOAD,
        ("Failed to create a GStreamer Bin. "
            "Please check your GStreamer installation."));
    g_object_ref_sink (gve);
    g_object_unref (gve);
    return NULL;
  }

  /* Create elements */
  gve->priv->gnl_video_composition =
      gst_element_factory_make ("gnlcomposition", "gnl-video-composition");
  gve->priv->gnl_audio_composition =
      gst_element_factory_make ("gnlcomposition", "gnl-audio-composition");
  if (!gve->priv->gnl_video_composition || !gve->priv->gnl_audio_composition) {
    g_set_error (err, GVE_ERROR, GST_ERROR_PLUGIN_LOAD,
        ("Failed to create a Gnonlin element. "
            "Please check your GStreamer installation."));
    g_object_ref_sink (gve);
    g_object_unref (gve);
    return NULL;
  }

  /*Connect bus signals */
  /*Wait for a "new-decoded-pad" message to link the composition with
     the encoder tail */
  gve->priv->bus = gst_element_get_bus (GST_ELEMENT (gve->priv->main_pipeline));
  g_signal_connect (gve->priv->gnl_video_composition, "pad-added",
      G_CALLBACK (new_decoded_pad_cb), gve);
  g_signal_connect (gve->priv->gnl_audio_composition, "pad-added",
      G_CALLBACK (new_decoded_pad_cb), gve);

  gst_bus_add_signal_watch (gve->priv->bus);
  gve->priv->sig_bus_async = g_signal_connect (gve->priv->bus, "message",
      G_CALLBACK (gve_bus_message_cb), gve);
  return gve;
}
