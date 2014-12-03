/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
* Gstreamer Video Encoder
* Copyright (C)  Andoni Morales Alastruey 2013 <ylatuya@gmail.com>
*
* You may redistribute it and/or modify it under the terms of the
* GNU General Public License, as published by the Free Software
* Foundation; either version 2 of the License, or (at your option)
* any later version.
*
* Gstreamer DV is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
* See the GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with foob.  If not, write to:
*       The Free Software Foundation, Inc.,
*       51 Franklin Street, Fifth Floor
*       Boston, MA  02110-1301, USA.
*/

#include <gst/gst.h>

#include "gst-video-encoder.h"
#include "video-utils.h"


GST_DEBUG_CATEGORY (_video_encoder_gst_debug_cat);
#define GST_CAT_DEFAULT _video_encoder_gst_debug_cat

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_PERCENT_COMPLETED,
  LAST_SIGNAL
};

struct GstVideoEncoderPrivate
{
  /*Encoding properties */
  gchar *output_file;
  GList *input_files;
  GList *current_file;
  guint output_height;
  guint output_width;
  guint audio_quality;
  guint video_quality;
  guint fps_n;
  guint fps_d;
  VideoEncoderType video_encoder_type;
  AudioEncoderType audio_encoder_type;
  VideoMuxerType video_muxer_type;

  /*GStreamer elements */
  GstElement *main_pipeline;
  GstElement *source_bin;
  GstElement *encoder_bin;
  GstElement *video_enc;
  GstElement *audio_enc;
  GstElement *vqueue;
  GstElement *aqueue;
  GstElement *muxer;
  GstElement *filesink;

  /*GStreamer bus */
  GstBus *bus;
  gulong sig_bus_async;

  gboolean video_drained;
  gboolean audio_drained;
  GstPad *video_pad;
  GstPad *audio_pad;
  GstClockTime total_duration;
  guint update_id;
  guint64 last_buf_ts;
};

static GObjectClass *parent_class = NULL;

static int gve_signals[LAST_SIGNAL] = { 0 };

static void gve_error_msg (GstVideoEncoder * gcc, GstMessage * msg);
static void gve_bus_message_cb (GstBus * bus, GstMessage * message,
    gpointer data);
static gboolean gst_video_encoder_select_next_file (GstVideoEncoder * gve);

G_DEFINE_TYPE (GstVideoEncoder, gst_video_encoder, G_TYPE_OBJECT);

/***********************************
*
*     Class, Object and Properties
*
************************************/

static void
gst_video_encoder_init (GstVideoEncoder * object)
{
  GstVideoEncoderPrivate *priv;
  object->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_VIDEO_ENCODER,
      GstVideoEncoderPrivate);

  priv->output_height = 480;
  priv->output_width = 640;
  priv->audio_quality = 50;
  priv->video_quality = 50;
  priv->video_encoder_type = VIDEO_ENCODER_VP8;
  priv->audio_encoder_type = AUDIO_ENCODER_VORBIS;
  priv->video_muxer_type = VIDEO_MUXER_WEBM;
}

void
gst_video_encoder_finalize (GObject * object)
{
  GstVideoEncoder *gve = (GstVideoEncoder *) object;

  if (gve->priv->update_id != 0) {
    g_source_remove (gve->priv->update_id);
    gve->priv->update_id = 0;
  }

  GST_DEBUG_OBJECT (gve, "Finalizing.");
  if (gve->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
     * called again (main loop might be run again to display error dialog) */
    gst_bus_set_flushing (gve->priv->bus, TRUE);

    if (gve->priv->sig_bus_async)
      g_signal_handler_disconnect (gve->priv->bus, gve->priv->sig_bus_async);

    gst_object_unref (gve->priv->bus);
    gve->priv->bus = NULL;
  }

  if (gve->priv->output_file) {
    g_free (gve->priv->output_file);
    gve->priv->output_file = NULL;
  }

  if (gve->priv->input_files) {
    g_list_foreach (gve->priv->input_files, (GFunc) g_free, NULL);
    g_list_free (gve->priv->input_files);
    gve->priv->input_files = NULL;
  }

  if (gve->priv->main_pipeline != NULL
      && GST_IS_ELEMENT (gve->priv->main_pipeline)) {
    gst_element_set_state (gve->priv->main_pipeline, GST_STATE_NULL);
    gst_object_unref (gve->priv->main_pipeline);
    gve->priv->main_pipeline = NULL;
  }

  G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
gst_video_encoder_class_init (GstVideoEncoderClass * klass)
{
  GObjectClass *object_class;

  object_class = (GObjectClass *) klass;
  parent_class = g_type_class_peek_parent (klass);

  g_type_class_add_private (object_class, sizeof (GstVideoEncoderPrivate));

  /* GObject */
  object_class->finalize = gst_video_encoder_finalize;

  /* Signals */
  gve_signals[SIGNAL_ERROR] =
      g_signal_new ("error",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstVideoEncoderClass, error),
      NULL, NULL,
      g_cclosure_marshal_VOID__STRING, G_TYPE_NONE, 1, G_TYPE_STRING);

  gve_signals[SIGNAL_PERCENT_COMPLETED] =
      g_signal_new ("percent_completed",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstVideoEncoderClass, percent_completed),
      NULL, NULL, g_cclosure_marshal_VOID__FLOAT, G_TYPE_NONE, 1, G_TYPE_FLOAT);
}

/***********************************
*
*           GStreamer
*
************************************/

void
gst_video_encoder_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

GQuark
gst_video_encoder_error_quark (void)
{
  static GQuark q;              /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("gve-error-quark");
  }
  return q;
}

static gboolean
gve_on_buffer_cb (GstPad * pad, GstBuffer * buf, GstVideoEncoder * gve)
{
  gve->priv->last_buf_ts = g_get_monotonic_time ();
  return TRUE;
}

static void
gst_video_encoder_create_encoder_bin (GstVideoEncoder * gve)
{
  GstElement *colorspace1, *videoscale, *framerate, *deinterlace;
  GstElement *colorspace2, *audioconvert, *audioresample;
  GstElement *aqueue, *vqueue;
  GstElement *v_identity, *a_identity;
  GstCaps *video_caps, *audio_caps, *h264_caps;
  GstPad *v_sink_pad, *a_sink_pad, *pad;

  GST_INFO_OBJECT (gve, "Creating encoder bin");
  gve->priv->encoder_bin = gst_bin_new ("encoder_bin");

  colorspace1 = gst_element_factory_make ("ffmpegcolorspace", NULL);
  deinterlace = gst_element_factory_make ("ffdeinterlace", NULL);
  if (deinterlace == NULL)
    deinterlace = gst_element_factory_make ("identity", NULL);
  colorspace2 = gst_element_factory_make ("ffmpegcolorspace", "colorspace2");
  videoscale = gst_element_factory_make ("videoscale", "gve_videoscale");
  framerate = gst_element_factory_make ("videorate", "gve_videorate");
  audioconvert = gst_element_factory_make ("audioconvert", NULL);
  audioresample = gst_element_factory_make ("audioresample", NULL);
  gve->priv->filesink = gst_element_factory_make ("filesink", NULL);
  aqueue = gst_element_factory_make ("queue2", "audio_queue");
  vqueue = gst_element_factory_make ("queue2", "video_queue");
  a_identity = gst_element_factory_make ("identity", "audio_identity");
  v_identity = gst_element_factory_make ("identity", "video_identity");
  gve->priv->aqueue = aqueue;
  gve->priv->vqueue = vqueue;


  /* Increase audio queue size for h264 encoding as the encoder queues 2 seconds
   * of video */
  g_object_set (aqueue, "max-size-bytes", 0, "max-size-buffers", 0,
      "max-size-time", 5 * GST_SECOND, NULL);
  g_object_set (vqueue, "max-size-bytes", 0, "max-size-buffers", 0,
      "max-size-time", 5 * GST_SECOND, NULL);

  /* Set caps for the encoding resolution */
  video_caps = gst_caps_new_simple ("video/x-raw-yuv", NULL);
  gst_caps_set_simple (video_caps, "format", GST_TYPE_FOURCC,
      GST_STR_FOURCC ("I420"), NULL);
  gst_caps_set_simple (video_caps, "pixel-aspect-ratio", GST_TYPE_FRACTION,
      1, 1, NULL);
  if (gve->priv->output_width != 0) {
    gst_caps_set_simple (video_caps, "width", G_TYPE_INT,
        gve->priv->output_width, NULL);
  }
  if (gve->priv->output_height != 0) {
    gst_caps_set_simple (video_caps, "height", G_TYPE_INT,
        gve->priv->output_height, NULL);
  }
  /* Set caps for the encoding framerate */
  if (gve->priv->fps_n != 0 && gve->priv->fps_d != 0) {
    gst_caps_set_simple (video_caps, "framerate", GST_TYPE_FRACTION,
        gve->priv->fps_n, gve->priv->fps_d, NULL);
  }

  /* Audio caps to fixate the channels and sample rate */
  audio_caps =
      gst_caps_from_string ("audio/x-raw-int, channels=(int)2, rate=(int)48000;"
      "audio/x-raw-float, channels=(int)2, rate=(int)48000");

  /* Set caps for the h264 profile */
  h264_caps = gst_caps_new_simple ("video/x-h264", NULL);
  gst_caps_set_simple (h264_caps, "profile", G_TYPE_STRING,
      "constrained-baseline", "stream-format", G_TYPE_STRING, "avc", NULL);

  g_object_set (a_identity, "single-segment", TRUE, NULL);
  g_object_set (v_identity, "single-segment", TRUE, NULL);

  gst_bin_add_many (GST_BIN (gve->priv->encoder_bin), v_identity, colorspace1,
      deinterlace, videoscale, framerate, colorspace2,
      gve->priv->video_enc, vqueue, gve->priv->muxer, gve->priv->filesink,
      a_identity, audioconvert, audioresample, gve->priv->audio_enc, aqueue,
      NULL);

  gst_element_link_many (v_identity, colorspace1, deinterlace, framerate,
      videoscale, colorspace2, NULL);
  gst_element_link_filtered (colorspace2, gve->priv->video_enc, video_caps);
  gst_element_link_filtered (gve->priv->video_enc, vqueue, h264_caps);
  gst_element_link (vqueue, gve->priv->muxer);
  gst_element_link_many (a_identity, audioconvert, audioresample, NULL);
  gst_element_link_filtered (audioresample, gve->priv->audio_enc, audio_caps);
  gst_element_link_many (gve->priv->audio_enc, aqueue, gve->priv->muxer, NULL);
  gst_element_link (gve->priv->muxer, gve->priv->filesink);

  gst_caps_unref (video_caps);
  gst_caps_unref (audio_caps);
  gst_caps_unref (h264_caps);
  g_object_set (gve->priv->filesink, "location", gve->priv->output_file, NULL);

  /* Create ghost pads */
  v_sink_pad = gst_element_get_static_pad (v_identity, "sink");
  a_sink_pad = gst_element_get_static_pad (a_identity, "sink");
  gst_element_add_pad (gve->priv->encoder_bin,
      gst_ghost_pad_new ("video", v_sink_pad));
  gst_element_add_pad (gve->priv->encoder_bin,
      gst_ghost_pad_new ("audio", a_sink_pad));
  gst_object_unref (GST_OBJECT (v_sink_pad));
  gst_object_unref (GST_OBJECT (a_sink_pad));

  /* Add a pad probe to detect deadlock as EOS are not sent correctly by some
   * muxers such as mpegpsdemux */
  pad = gst_element_get_static_pad (gve->priv->filesink, "sink");
  gst_pad_add_buffer_probe (pad, (GCallback) gve_on_buffer_cb, gve);
  gst_object_unref (pad);

  gst_bin_add (GST_BIN (gve->priv->main_pipeline), gve->priv->encoder_bin);
  GST_INFO_OBJECT (gve, "Encoder bin created successfully");
}

static gboolean
cb_handle_eos (GstPad * pad, GstEvent * event, GstVideoEncoder * gve)
{
  if (event->type == GST_EVENT_EOS) {
    GST_DEBUG_OBJECT (gve, "Dropping EOS on pad %s:%s",
        GST_DEBUG_PAD_NAME (pad));
    if (pad == gve->priv->audio_pad) {
      gve->priv->audio_drained = TRUE;
    } else if (pad == gve->priv->video_pad) {
      gve->priv->video_drained = TRUE;
    }
    if (gve->priv->audio_drained && gve->priv->video_drained) {
      g_idle_add ((GSourceFunc) gst_video_encoder_select_next_file, gve);
    }
    return FALSE;
  }
  return TRUE;
}

static void
cb_new_pad (GstElement * decodebin, GstPad * pad, GstVideoEncoder * gve)
{
  GstPad *epad = NULL;
  GstCaps *caps;
  const GstStructure *s;
  const gchar *mime;
  gboolean is_video;

  caps = gst_pad_get_caps_reffed (pad);
  s = gst_caps_get_structure (caps, 0);
  mime = gst_structure_get_name (s);

  if (g_strrstr (mime, "video")) {
    epad = gst_element_get_static_pad (gve->priv->encoder_bin, "video");
    is_video = TRUE;
  } else if (g_strrstr (mime, "audio")) {
    epad = gst_element_get_static_pad (gve->priv->encoder_bin, "audio");
    is_video = FALSE;
  }

  if (epad && !gst_pad_is_linked (epad)) {
    GST_INFO_OBJECT (gve, "Linking pad with caps %" GST_PTR_FORMAT, caps);
    if (gst_pad_link (pad, epad)) {
      g_signal_emit (gve, gve_signals[SIGNAL_ERROR], 0, "Error linking pads");
    } else {
      if (is_video) {
        gve->priv->video_pad = pad;
      } else {
        gve->priv->audio_pad = pad;
      }
      gst_pad_add_event_probe (pad, G_CALLBACK (cb_handle_eos), gve);
    }
  } else {
    GST_INFO_OBJECT (gve, "Dropping pad with caps %" GST_PTR_FORMAT, caps);
  }
  gst_caps_unref (caps);
}

static void
gst_video_encoder_create_source (GstVideoEncoder * gve, gchar * location)
{
  GST_INFO_OBJECT (gve, "Creating source");

  if (gve->priv->source_bin != NULL) {
    gst_element_set_state (gve->priv->source_bin, GST_STATE_NULL);
    gst_bin_remove (GST_BIN (gve->priv->main_pipeline), gve->priv->source_bin);
  }
  gve->priv->source_bin = gst_element_factory_make ("uridecodebin", NULL);
  g_object_set (gve->priv->source_bin, "uri", location, NULL);
  g_signal_connect (gve->priv->source_bin, "pad-added", G_CALLBACK (cb_new_pad),
      gve);
  g_signal_connect (gve->priv->source_bin, "autoplug-select",
      G_CALLBACK (lgm_filter_video_decoders), gve);
  gst_bin_add (GST_BIN (gve->priv->main_pipeline), gve->priv->source_bin);
  gst_element_sync_state_with_parent (gve->priv->source_bin);
  gve->priv->audio_drained = FALSE;
  gve->priv->video_drained = FALSE;
}

static gboolean
gst_video_encoder_select_next_file (GstVideoEncoder * gve)
{
  GstPad *audio_pad, *video_pad;

  audio_pad = gst_element_get_static_pad (gve->priv->encoder_bin, "audio");
  video_pad = gst_element_get_static_pad (gve->priv->encoder_bin, "video");

  if (gve->priv->current_file == NULL) {
    gve->priv->current_file = gve->priv->input_files;
  } else {
    gve->priv->current_file = g_list_next (gve->priv->current_file);
  }

  if (gve->priv->current_file != NULL) {
    GstPad *a_peer, *v_peer;

    GST_INFO_OBJECT (gve, "Selecting next file: %s",
        (gchar *) gve->priv->current_file->data);
    a_peer = gst_pad_get_peer (audio_pad);
    if (a_peer) {
      gst_pad_unlink (a_peer, audio_pad);
      gst_object_unref (a_peer);
    }

    v_peer = gst_pad_get_peer (video_pad);
    if (v_peer) {
      gst_pad_unlink (v_peer, video_pad);
      gst_object_unref (v_peer);
    }
    gst_video_encoder_create_source (gve,
        (gchar *) gve->priv->current_file->data);
  } else {
    GST_INFO_OBJECT (gve, "No more files, sending EOS");
    if (gve->priv->update_id != 0) {
      g_source_remove (gve->priv->update_id);
      gve->priv->update_id = 0;
    }
    /* Enlarge queues to avoid deadlocks */
    g_object_set (gve->priv->aqueue, "max-size-time", 0,
        "max-size-bytes", 0, "max-size-buffers", 0, NULL);
    g_object_set (gve->priv->vqueue, "max-size-time", 0,
        "max-size-bytes", 0, "max-size-buffers", 0, NULL);
    gst_pad_send_event (audio_pad, gst_event_new_eos ());
    gst_pad_send_event (video_pad, gst_event_new_eos ());
  }
  return FALSE;
}

static gboolean
gst_video_encoder_create_video_encoder (GstVideoEncoder * gve,
    VideoEncoderType type, GError ** err)
{
  GstElement *encoder = NULL;

  g_return_val_if_fail (gve != NULL, FALSE);
  g_return_val_if_fail (GST_IS_VIDEO_ENCODER (gve), FALSE);

  encoder = lgm_create_video_encoder (type, gve->priv->video_quality,
      FALSE, GVE_ERROR, err);
  if (!encoder) {
    return FALSE;
  }

  gve->priv->video_encoder_type = type;
  gve->priv->video_enc = encoder;
  return TRUE;
}

static gboolean
gst_video_encoder_create_audio_encoder (GstVideoEncoder * gve,
    AudioEncoderType type, GError ** err)
{
  GstElement *encoder = NULL;

  g_return_val_if_fail (gve != NULL, FALSE);
  g_return_val_if_fail (GST_IS_VIDEO_ENCODER (gve), FALSE);

  encoder = lgm_create_audio_encoder (type, gve->priv->audio_quality,
      GVE_ERROR, err);
  if (!encoder) {
    return FALSE;
  }

  gve->priv->audio_encoder_type = type;
  gve->priv->audio_enc = encoder;
  return TRUE;
}

static gboolean
gst_video_encoder_create_video_muxer (GstVideoEncoder * gve,
    VideoMuxerType type, GError ** err)
{
  GstElement *muxer = NULL;

  g_return_val_if_fail (gve != NULL, FALSE);
  g_return_val_if_fail (GST_IS_VIDEO_ENCODER (gve), FALSE);

  muxer = lgm_create_muxer (type, GVE_ERROR, err);
  if (!muxer) {
    return FALSE;
  }
  gve->priv->video_muxer_type = type;
  gve->priv->muxer = muxer;
  return TRUE;
}

static void
gst_video_encoder_initialize (GstVideoEncoder * gve)
{
  GError *err = NULL;

  GST_INFO_OBJECT (gve, "Initializing encoders");
  if (!gst_video_encoder_create_video_encoder (gve,
          gve->priv->video_encoder_type, &err))
    goto missing_plugin;
  if (!gst_video_encoder_create_audio_encoder (gve,
          gve->priv->audio_encoder_type, &err))
    goto missing_plugin;
  if (!gst_video_encoder_create_video_muxer (gve,
          gve->priv->video_muxer_type, &err))
    goto missing_plugin;

  gst_video_encoder_create_encoder_bin (gve);
  gst_video_encoder_select_next_file (gve);
  gst_element_set_state (gve->priv->main_pipeline, GST_STATE_PLAYING);
  return;

missing_plugin:
  g_signal_emit (gve, gve_signals[SIGNAL_ERROR], 0, err->message);
  g_error_free (err);
}

static void
gve_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  GstVideoEncoder *gve = (GstVideoEncoder *) data;
  GstMessageType msg_type;

  g_return_if_fail (gve != NULL);
  g_return_if_fail (GST_IS_VIDEO_ENCODER (gve));

  msg_type = GST_MESSAGE_TYPE (message);

  switch (msg_type) {
    case GST_MESSAGE_ERROR:
    {
      if (gve->priv->main_pipeline) {
        gst_video_encoder_cancel (gve);
        gst_element_set_state (gve->priv->main_pipeline, GST_STATE_NULL);
      }
      gve_error_msg (gve, message);
      break;
    }

    case GST_MESSAGE_WARNING:
    {
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;
    }

    case GST_MESSAGE_EOS:
    {
      GST_INFO_OBJECT (gve, "EOS message");
      g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0, (gfloat) 1);
      break;
    }

    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
}

static void
gve_error_msg (GstVideoEncoder * gve, GstMessage * msg)
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
gst_video_encoder_query_timeout (GstVideoEncoder * gve)
{
  GstFormat fmt;
  gint64 pos;

  fmt = GST_FORMAT_TIME;
  pos = -1;

  gst_element_query_position (gve->priv->main_pipeline, &fmt, &pos);

  g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0,
      MIN (0.99, (gfloat) pos / (gfloat) gve->priv->total_duration));

  if (g_get_monotonic_time () - gve->priv->last_buf_ts > 4 * 1000000) {
    g_idle_add ((GSourceFunc) gst_video_encoder_select_next_file, gve);
  }

  return TRUE;
}

/*******************************************
 *
 *         Public methods
 *
 * ****************************************/

void
gst_video_encoder_cancel (GstVideoEncoder * gve)
{
  g_return_if_fail (gve != NULL);
  g_return_if_fail (GST_IS_VIDEO_ENCODER (gve));

  g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0, (gfloat) - 1);
  gst_element_set_state (gve->priv->main_pipeline, GST_STATE_NULL);
  gst_element_get_state (gve->priv->main_pipeline, NULL, NULL, -1);
  gst_bin_remove (GST_BIN (gve->priv->main_pipeline), gve->priv->source_bin);
  gst_bin_remove (GST_BIN (gve->priv->main_pipeline), gve->priv->encoder_bin);
  gve->priv->total_duration = 0;
  if (gve->priv->update_id != 0) {
    g_source_remove (gve->priv->update_id);
    gve->priv->update_id = 0;
  }
}

void
gst_video_encoder_start (GstVideoEncoder * gve)
{
  g_return_if_fail (gve != NULL);
  g_return_if_fail (GST_IS_VIDEO_ENCODER (gve));

  GST_INFO_OBJECT (gve, "Starting encoding");
  g_signal_emit (gve, gve_signals[SIGNAL_PERCENT_COMPLETED], 0, (gfloat) 0);
  gst_video_encoder_initialize (gve);
  gve->priv->last_buf_ts = g_get_monotonic_time ();
  gve->priv->update_id =
      g_timeout_add (100, (GSourceFunc) gst_video_encoder_query_timeout, gve);

}

void
gst_video_encoder_add_file (GstVideoEncoder * gve, const gchar * file,
    guint64 duration)
{
  gchar *uri;
  g_return_if_fail (gve != NULL);
  g_return_if_fail (GST_IS_VIDEO_ENCODER (gve));

  GST_INFO_OBJECT (gve, "Adding file %s", file);
  uri = lgm_filename_to_uri (file);
  if (uri == NULL) {
    GST_ERROR_OBJECT (gve, "Invalid filename %s", file);
  }
  gve->priv->input_files = g_list_append (gve->priv->input_files, uri);
  gve->priv->total_duration += duration * GST_MSECOND;
}

gboolean
gst_video_encoder_dump_graph (GstVideoEncoder * gve)
{
  GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN (gve->priv->main_pipeline),
      GST_DEBUG_GRAPH_SHOW_ALL, "gst-video-encoder.dot");
  return FALSE;
}

void
gst_video_encoder_set_encoding_format (GstVideoEncoder * gve,
    VideoEncoderType video_codec, AudioEncoderType audio_codec,
    VideoMuxerType muxer, guint video_quality, guint audio_quality,
    guint width, guint height, guint fps_n, guint fps_d)
{
  gve->priv->video_encoder_type = video_codec;
  gve->priv->audio_encoder_type = audio_codec;
  gve->priv->video_muxer_type = muxer;
  gve->priv->video_quality = video_quality;
  gve->priv->audio_quality = audio_quality;
  gve->priv->output_width = width;
  gve->priv->output_height = height;
  gve->priv->fps_n = fps_n;
  gve->priv->fps_d = fps_d;

}

GstVideoEncoder *
gst_video_encoder_new (gchar * filename, GError ** err)
{
  GstVideoEncoder *gve = NULL;

#ifndef GST_DISABLE_GST_INFO
  if (_video_encoder_gst_debug_cat == NULL) {
    GST_DEBUG_CATEGORY_INIT (_video_encoder_gst_debug_cat, "longomatch", 0,
        "LongoMatch GStreamer Backend");
  }
#endif

  gve = g_object_new (GST_TYPE_VIDEO_ENCODER, NULL);

  gve->priv->output_file = g_strdup (filename);
  gve->priv->main_pipeline = gst_pipeline_new ("main_pipeline");

  if (!gve->priv->main_pipeline) {
    g_set_error (err,
        GVE_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the pipeline element. "
        "Please check your GStreamer installation.");
    goto missing_plugin;
  }

  /*Connect bus signals */
  GST_INFO_OBJECT (gve, "Connecting bus signals");
  gve->priv->bus = gst_element_get_bus (GST_ELEMENT (gve->priv->main_pipeline));
  gst_bus_add_signal_watch (gve->priv->bus);
  gve->priv->sig_bus_async =
      g_signal_connect (gve->priv->bus, "message",
      G_CALLBACK (gve_bus_message_cb), gve);

  return gve;

/* Missing plugin */
missing_plugin:
  {
    g_object_ref_sink (gve);
    g_object_unref (gve);
    return NULL;
  }
}
