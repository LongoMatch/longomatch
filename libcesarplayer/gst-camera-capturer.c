/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
* Gstreamer DV capturer
* Copyright (C)  Andoni Morales Alastruey 2008 <ylatuya@gmail.com>
*
* Gstreamer DV capturer is free software.
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

#include <string.h>
#include <stdio.h>

#include <gst/app/gstappsrc.h>
#include <gst/interfaces/xoverlay.h>
#include <gst/gst.h>
#include <gst/video/video.h>

#include "gst-camera-capturer.h"
#include "gstscreenshot.h"
#include "lgm-utils.h"
#include "baconvideowidget-marshal.h"


GST_DEBUG_CATEGORY (_cesarplayer_gst_debug_cat);
#define GST_CAT_DEFAULT _cesarplayer_gst_debug_cat

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_STATE_CHANGED,
  SIGNAL_DEVICE_CHANGE,
  SIGNAL_MEDIA_INFO,
  SIGNAL_READY_TO_CAPTURE,
  LAST_SIGNAL
};

struct GstCameraCapturerPrivate
{

  /*Encoding properties */
  gchar *output_file;
  gchar *device_id;
  gint source_width;
  gint source_height;
  gint source_fps_n;
  gint source_fps_d;
  guint output_height;
  guint output_width;
  guint audio_quality;
  guint video_quality;
  gboolean audio_enabled;
  VideoEncoderType video_encoder_type;
  AudioEncoderType audio_encoder_type;
  VideoMuxerType video_muxer_type;
  CaptureSourceType source_type;
  gchar *source_element_name;

  /*Video input info */
  gint video_width;             /* Movie width */
  gint video_height;            /* Movie height */
  gint video_par_n;
  gint video_par_d;
  gint video_fps_n;
  gint video_fps_d;

  /* Snapshots */
  GstBuffer *last_buffer;

  /*GStreamer elements */
  GstElement *main_pipeline;
  GstElement *source_bin;
  GstElement *video_converter_bin;
  GstElement *splitter_bin;
  GstElement *preview_bin;
  GstElement *encoder_bin;
  GstElement *source;
  GstElement *source_filter;
  GstElement *video_enc;
  GstElement *audio_enc;
  GstElement *muxer;
  GstElement *filesink;
  GstElement *video_appsrc;
  GstElement *audio_appsrc;

  gboolean has_video;
  gboolean has_audio;

  /* Recording */
  gboolean ready_to_capture;
  gboolean is_recording;
  gboolean closing_recording;
  gboolean video_needs_keyframe_sync;
  gboolean video_synced;
  GstClockTime accum_recorded_ts;
  GstClockTime last_accum_recorded_ts;
  GstClockTime current_recording_start_ts;
  GstClockTime last_video_buf_ts;
  GstClockTime last_audio_buf_ts;
  GMutex recording_lock;

  /*Overlay */
  GstXOverlay *xoverlay;        /* protect with lock */
  guintptr window_handle;

  /*GStreamer bus */
  GstBus *bus;
  gulong sig_bus_async;
  gulong sig_bus_sync;
};

static GObjectClass *parent_class = NULL;

static GThread *gui_thread;

static int gcc_signals[LAST_SIGNAL] = { 0 };

static void gcc_error_msg (GstCameraCapturer * gcc, GstMessage * msg);
static void gcc_bus_message_cb (GstBus * bus, GstMessage * message,
    gpointer data);
static void gcc_element_msg_sync (GstBus * bus, GstMessage * msg,
    gpointer data);
static gboolean gcc_get_video_stream_info (GstPad * pad, GstPad * peer,
    GstCameraCapturer * gcc);

G_DEFINE_TYPE (GstCameraCapturer, gst_camera_capturer, G_TYPE_OBJECT);

/***********************************
*
*     Class, Object and Properties
*
************************************/

static void
gst_camera_capturer_init (GstCameraCapturer * object)
{
  GstCameraCapturerPrivate *priv;
  object->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_CAMERA_CAPTURER,
      GstCameraCapturerPrivate);

  priv->output_height = 480;
  priv->output_width = 640;
  priv->audio_quality = 50;
  priv->video_quality = 50;
  priv->last_buffer = NULL;
  priv->current_recording_start_ts = GST_CLOCK_TIME_NONE;
  priv->accum_recorded_ts = GST_CLOCK_TIME_NONE;
  priv->last_accum_recorded_ts = GST_CLOCK_TIME_NONE;
  priv->last_video_buf_ts = GST_CLOCK_TIME_NONE;
  priv->last_audio_buf_ts = GST_CLOCK_TIME_NONE;
  priv->is_recording = FALSE;
  priv->ready_to_capture = FALSE;
  g_mutex_init (&priv->recording_lock);

  priv->video_encoder_type = VIDEO_ENCODER_VP8;
  priv->audio_encoder_type = AUDIO_ENCODER_VORBIS;
  priv->video_muxer_type = VIDEO_MUXER_WEBM;
  priv->source_type = CAPTURE_SOURCE_TYPE_SYSTEM;
  priv->source_element_name = SYSVIDEOSRC;
}

void
gst_camera_capturer_finalize (GObject * object)
{
  GstCameraCapturer *gcc = (GstCameraCapturer *) object;

  GST_DEBUG_OBJECT (gcc, "Finalizing.");
  if (gcc->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
     * called again (main loop might be run again to display error dialog) */
    gst_bus_set_flushing (gcc->priv->bus, TRUE);

    if (gcc->priv->sig_bus_async)
      g_signal_handler_disconnect (gcc->priv->bus, gcc->priv->sig_bus_async);

    if (gcc->priv->sig_bus_sync)
      g_signal_handler_disconnect (gcc->priv->bus, gcc->priv->sig_bus_sync);

    gst_object_unref (gcc->priv->bus);
    gcc->priv->bus = NULL;
  }

  if (gcc->priv->output_file) {
    g_free (gcc->priv->output_file);
    gcc->priv->output_file = NULL;
  }

  if (gcc->priv->source_element_name) {
    g_free (gcc->priv->source_element_name);
    gcc->priv->source_element_name = NULL;
  }

  if (gcc->priv->device_id) {
    g_free (gcc->priv->device_id);
    gcc->priv->device_id = NULL;
  }

  if (gcc->priv->last_buffer != NULL) {
    gst_buffer_unref (gcc->priv->last_buffer);
    gcc->priv->last_buffer = NULL;
  }

  if (gcc->priv->xoverlay != NULL) {
    gst_object_unref (gcc->priv->xoverlay);
    gcc->priv->xoverlay = NULL;
  }

  if (gcc->priv->main_pipeline != NULL
      && GST_IS_ELEMENT (gcc->priv->main_pipeline)) {
    gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_NULL);
    gst_object_unref (gcc->priv->main_pipeline);
    gcc->priv->main_pipeline = NULL;
  }

  g_mutex_clear (&gcc->priv->recording_lock);

  G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
gst_camera_capturer_class_init (GstCameraCapturerClass * klass)
{
  GObjectClass *object_class;

  object_class = (GObjectClass *) klass;
  parent_class = g_type_class_peek_parent (klass);

  g_type_class_add_private (object_class, sizeof (GstCameraCapturerPrivate));

  /* GObject */
  object_class->finalize = gst_camera_capturer_finalize;

  /* Signals */
  gcc_signals[SIGNAL_ERROR] =
      g_signal_new ("error",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstCameraCapturerClass, error),
      NULL, NULL,
      g_cclosure_marshal_VOID__STRING, G_TYPE_NONE, 1, G_TYPE_STRING);

  gcc_signals[SIGNAL_EOS] =
      g_signal_new ("eos",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstCameraCapturerClass, eos),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  gcc_signals[SIGNAL_DEVICE_CHANGE] =
      g_signal_new ("device-change",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstCameraCapturerClass, device_change),
      NULL, NULL, g_cclosure_marshal_VOID__INT, G_TYPE_NONE, 1, G_TYPE_INT);

  gcc_signals[SIGNAL_READY_TO_CAPTURE] =
      g_signal_new ("ready-to-capture",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstCameraCapturerClass, ready_to_capture),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  gcc_signals[SIGNAL_MEDIA_INFO] =
      g_signal_new ("media-info",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (GstCameraCapturerClass, media_info),
      NULL, NULL,
      baconvideowidget_marshal_VOID__INT_INT_INT_INT,
      G_TYPE_NONE, 4, G_TYPE_INT, G_TYPE_INT, G_TYPE_INT, G_TYPE_INT);
}

/***********************************
*
*           GStreamer
*
************************************/

static void
gst_camera_capturer_emit_ready_to_capture (GstCameraCapturer * gcc)
{
  if(G_UNLIKELY(!gcc->priv->ready_to_capture)){
    g_signal_emit (gcc, gcc_signals[SIGNAL_READY_TO_CAPTURE], 0);
    gcc->priv->ready_to_capture = TRUE;
  }
}

GQuark
gst_camera_capturer_error_quark (void)
{
  static GQuark q;              /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("gcc-error-quark");
  }
  return q;
}

static void
gst_camera_capturer_update_device_id (GstCameraCapturer * gcc)
{
  const gchar *prop_name;

  if (!g_strcmp0 (gcc->priv->source_element_name, "dv1394src"))
    prop_name = "guid";
  else if (!g_strcmp0 (gcc->priv->source_element_name, "v4l2src"))
    prop_name = "device";
  else if (!g_strcmp0 (gcc->priv->source_element_name, "avfvideosrc"))
    prop_name = "device";
  else if (!g_strcmp0 (gcc->priv->source_element_name, "filesrc"))
    prop_name = "location";
  else if (!g_strcmp0 (gcc->priv->source_element_name, "gsettingsvideosrc"))
    prop_name = NULL;
  else
    prop_name = "device-name";

  if (!g_strcmp0 (gcc->priv->source_element_name, "decklinkvideosrc")) {
    /* The blackmagic device name we use conforms the pattern "Blackmagic%d" */
    g_object_set (gcc->priv->source, "device-number",
        atoi (gcc->priv->device_id + 10), NULL);
  } else {
    if (prop_name)
      g_object_set (gcc->priv->source, prop_name, gcc->priv->device_id, NULL);
  }
}

static gboolean
gst_camera_capturer_create_converter_bin (GstCameraCapturer * gcc)
{
  GstElement *videoscale, *videorate, *filter, *bin;
  GstPad *sink_pad, *src_pad;
  GstCaps *caps;

  gcc->priv->video_converter_bin = bin = gst_bin_new ("video-converter");

  videorate = gst_element_factory_make ("videorate", NULL);
  videoscale = gst_element_factory_make ("videoscale", NULL);
  filter = gst_element_factory_make ("capsfilter", NULL);
  /* Set caps for the encoding resolution */
  caps = gst_caps_from_string ("video/x-raw-yuv; video/x-raw-rgb");
  if (gcc->priv->output_width != 0) {
    gst_caps_set_simple (caps, "width", G_TYPE_INT, gcc->priv->output_width,
        NULL);
  }
  if (gcc->priv->output_height != 0) {
    gst_caps_set_simple (caps, "height", G_TYPE_INT, gcc->priv->output_height,
        NULL);
  }
  if (gcc->priv->source_fps_n != 0 && gcc->priv->source_fps_d != 0) {
    gint fps_n, fps_d;

    /* If the source frame rate is 50 or 60, reduce it to 25 or 30 */
    fps_n = gcc->priv->source_fps_n;
    fps_d = gcc->priv->source_fps_d;
    if ((gfloat) fps_n / fps_d > 30) {
      fps_d = fps_d * 2;
    }
    gst_caps_set_simple (caps, "framerate", GST_TYPE_FRACTION,
        fps_n, fps_d, NULL);
  }
  g_object_set (filter, "caps", caps, NULL);
  gst_bin_add_many (GST_BIN (bin), videorate, videoscale, filter, NULL);

  gst_element_link_many (videorate, videoscale, filter, NULL);

  /* Create ghost pads */
  sink_pad = gst_element_get_static_pad (videorate, "sink");
  gst_element_add_pad (bin, gst_ghost_pad_new ("sink", sink_pad));
  src_pad = gst_element_get_static_pad (filter, "src");
  gst_element_add_pad (bin, gst_ghost_pad_new ("src", src_pad));
  gst_object_unref (sink_pad);
  gst_object_unref (src_pad);

  return TRUE;
}

static void
gst_camera_capturer_create_encoder_bin (GstCameraCapturer * gcc)
{
  GstElement *colorspace;
  GstPad *v_sink_pad;

  GST_INFO_OBJECT (gcc, "Creating encoder bin");
  gcc->priv->encoder_bin = gst_bin_new ("encoder_bin");

  colorspace = gst_element_factory_make ("ffmpegcolorspace", NULL);
  gcc->priv->filesink = gst_element_factory_make ("filesink", NULL);

  gst_bin_add_many (GST_BIN (gcc->priv->encoder_bin),
      colorspace, gcc->priv->video_enc,
      gcc->priv->muxer, gcc->priv->filesink, NULL);

  gst_element_link_many (colorspace, gcc->priv->video_enc,
      gcc->priv->muxer, gcc->priv->filesink, NULL);

  g_object_set (gcc->priv->filesink, "location", gcc->priv->output_file, NULL);

  /* Create ghost pads */
  v_sink_pad = gst_element_get_static_pad (colorspace, "sink");
  gst_element_add_pad (gcc->priv->encoder_bin, gst_ghost_pad_new ("video",
          v_sink_pad));
  gst_object_unref (GST_OBJECT (v_sink_pad));

  if (gcc->priv->audio_enabled) {
    GstElement *audioconvert, *audioresample;
    GstPad *a_sink_pad;

    audioconvert = gst_element_factory_make ("audioconvert", NULL);
    audioresample = gst_element_factory_make ("audioresample", NULL);

    gst_bin_add_many (GST_BIN (gcc->priv->encoder_bin), audioconvert,
        audioresample, gcc->priv->audio_enc, NULL);

    gst_element_link_many (audioconvert, audioresample, gcc->priv->audio_enc,
        gcc->priv->muxer, NULL);

    a_sink_pad = gst_element_get_static_pad (audioconvert, "sink");
    gst_element_add_pad (gcc->priv->encoder_bin, gst_ghost_pad_new ("audio",
            a_sink_pad));
    gst_object_unref (GST_OBJECT (a_sink_pad));
  }

  GST_INFO_OBJECT (gcc, "Encoder bin created successfully");
}

static void
gst_camera_capturer_create_remuxer_bin (GstCameraCapturer * gcc)
{
  GstElement *muxer;
  GstPad *v_sink_pad;

  GST_INFO_OBJECT (gcc, "Creating remuxer bin");
  gcc->priv->encoder_bin = gst_bin_new ("encoder_bin");
  muxer = gst_element_factory_make ("qtmux", NULL);
  gcc->priv->filesink = gst_element_factory_make ("filesink", NULL);
  g_object_set (gcc->priv->filesink, "location", gcc->priv->output_file, NULL);

  gst_bin_add_many (GST_BIN (gcc->priv->encoder_bin), muxer,
      gcc->priv->filesink, NULL);
  gst_element_link (muxer, gcc->priv->filesink);

  /* Create ghost pads */
  v_sink_pad = gst_element_get_request_pad (muxer, "video_%d");
  gst_element_add_pad (gcc->priv->encoder_bin, gst_ghost_pad_new ("video",
          v_sink_pad));
  gst_object_unref (v_sink_pad);

  if (gcc->priv->audio_enabled) {
    GstPad *a_sink_pad;

    /* Create ghost pads */
    a_sink_pad = gst_element_get_request_pad (muxer, "audio_%d");
    gst_element_add_pad (gcc->priv->encoder_bin, gst_ghost_pad_new ("audio",
            a_sink_pad));
    gst_object_unref (GST_OBJECT (v_sink_pad));
  }
}

static gboolean
gst_camera_capturer_encoding_retimestamper (GstCameraCapturer * gcc,
    GstBuffer * prev_buf, gboolean is_video)
{
  GstClockTime buf_ts, new_buf_ts, duration;
  GstBuffer *enc_buf;

  g_mutex_lock (&gcc->priv->recording_lock);

  gst_camera_capturer_emit_ready_to_capture(gcc);
  if (!gcc->priv->is_recording) {
    /* Drop buffers if we are not recording */
    GST_LOG_OBJECT (gcc, "Dropping buffer on %s pad",
        is_video ? "video" : "audio");
    goto done;
  }

  /* If we are just remuxing, drop everything until we see a keyframe */
  if (gcc->priv->video_needs_keyframe_sync && !gcc->priv->video_synced) {
    if (is_video
        && !GST_BUFFER_FLAG_IS_SET (prev_buf, GST_BUFFER_FLAG_DELTA_UNIT)) {
      gcc->priv->video_synced = TRUE;
    } else {
      GST_LOG_OBJECT (gcc, "Waiting for a keyframe, "
          "dropping buffer on %s pad", is_video ? "video" : "audio");
      goto done;
    }
  }

  enc_buf = gst_buffer_create_sub (prev_buf, 0, GST_BUFFER_SIZE (prev_buf));
  buf_ts = GST_BUFFER_TIMESTAMP (prev_buf);
  duration = GST_BUFFER_DURATION (prev_buf);
  if (duration == GST_CLOCK_TIME_NONE)
    duration = 0;

  /* Check if it's the first buffer after starting or restarting the capture
   * and update the timestamps accordingly */
  if (G_UNLIKELY (gcc->priv->current_recording_start_ts == GST_CLOCK_TIME_NONE)) {
    gcc->priv->current_recording_start_ts = buf_ts;
    gcc->priv->last_accum_recorded_ts = gcc->priv->accum_recorded_ts;
    GST_INFO_OBJECT (gcc, "Starting recording at %" GST_TIME_FORMAT,
        GST_TIME_ARGS (gcc->priv->last_accum_recorded_ts));
  }

  /* Clip buffers that are not in the segment */
  if (buf_ts < gcc->priv->current_recording_start_ts) {
    GST_WARNING_OBJECT (gcc, "Discarding buffer out of segment");
    goto done;
  }

  if (buf_ts != GST_CLOCK_TIME_NONE) {
    /* Get the buffer timestamp with respect of the encoding time and not
     * the playing time for a continous stream in the encoders input */
    new_buf_ts =
        buf_ts - gcc->priv->current_recording_start_ts +
        gcc->priv->last_accum_recorded_ts;

    /* Store the last timestamp seen on this pad */
    if (is_video)
      gcc->priv->last_video_buf_ts = new_buf_ts;
    else
      gcc->priv->last_audio_buf_ts = new_buf_ts;

    /* Update the highest encoded timestamp */
    if (new_buf_ts + duration > gcc->priv->accum_recorded_ts)
      gcc->priv->accum_recorded_ts = new_buf_ts + duration;
  } else {
    /* h264parse only sets the timestamp on the first buffer if a frame is
     * split in several ones. Other parsers might do the same. We only set
     * the last timestamp seen on the pad */
    if (is_video)
      new_buf_ts = gcc->priv->last_video_buf_ts;
    else
      new_buf_ts = gcc->priv->last_audio_buf_ts;
  }

  GST_BUFFER_TIMESTAMP (enc_buf) = new_buf_ts;

  GST_LOG_OBJECT (gcc, "Pushing %s frame to the encoder in ts:% "
      GST_TIME_FORMAT " out ts: %" GST_TIME_FORMAT, is_video ? "video" :
      "audio", GST_TIME_ARGS (buf_ts), GST_TIME_ARGS (new_buf_ts));

  if (is_video)
    gst_app_src_push_buffer (GST_APP_SRC (gcc->priv->video_appsrc), enc_buf);
  else
    gst_app_src_push_buffer (GST_APP_SRC (gcc->priv->audio_appsrc), enc_buf);

done:
  {
    g_mutex_unlock (&gcc->priv->recording_lock);
    return TRUE;
  }
}

static gboolean
gst_camera_capturer_audio_encoding_probe (GstPad * pad, GstBuffer * buf,
    GstCameraCapturer * gcc)
{
  return gst_camera_capturer_encoding_retimestamper (gcc, buf, FALSE);
}

static gboolean
gst_camera_capturer_video_encoding_probe (GstPad * pad, GstBuffer * buf,
    GstCameraCapturer * gcc)
{
  return gst_camera_capturer_encoding_retimestamper (gcc, buf, TRUE);
}

static void
gst_camera_capturer_create_splitter_bin (GstCameraCapturer * gcc)
{
  /*   "videosink" --> video_converter --> video_preview_queue --> "video_preview"
   *              |
   *   "audiosink" --> audio_preview_queue --> "audiosrc" --> "audio_preview"
   *
   *            video_appsrc  --> video_queue --> "videosrc"
   *            audio_appsrc  --> audio_queue --> "audosrc"
   */

  GstElement *v_queue, *v_prev_queue;
  GstPad *v_queue_pad, *v_prev_queue_pad, *v_sink_pad;


  GST_INFO_OBJECT (gcc, "Creating splitter bin");
  /* Create elements */
  gcc->priv->splitter_bin = gst_bin_new ("splitter");
  v_prev_queue = gst_element_factory_make ("queue", "video-preview-queue");
  gcc->priv->video_appsrc = gst_element_factory_make ("appsrc", "video-appsrc");
  v_queue = gst_element_factory_make ("queue", "video-queue");

  gst_app_src_set_max_bytes ((GstAppSrc *) gcc->priv->video_appsrc,
      (guint64) 20 * 1024 * 1024);
  g_object_set (gcc->priv->video_appsrc, "block", TRUE, NULL);

  gst_camera_capturer_create_converter_bin (gcc);
  gst_bin_add_many (GST_BIN (gcc->priv->splitter_bin),
      gcc->priv->video_converter_bin, v_prev_queue,
      gcc->priv->video_appsrc, v_queue, NULL);

  /* link converter to the preview-queue */
  gst_element_link (gcc->priv->video_converter_bin, v_prev_queue);

  /* Link video appsrc to the queue */
  gst_element_link (gcc->priv->video_appsrc, v_queue);

  /* Create source ghost pads */
  v_queue_pad = gst_element_get_static_pad (v_queue, "src");
  v_prev_queue_pad = gst_element_get_static_pad (v_prev_queue, "src");
  gst_element_add_pad (gcc->priv->splitter_bin, gst_ghost_pad_new ("videosrc",
          v_queue_pad));
  gst_element_add_pad (gcc->priv->splitter_bin,
      gst_ghost_pad_new ("video_preview", v_prev_queue_pad));
  gst_object_unref (v_queue_pad);
  gst_object_unref (v_prev_queue_pad);

  /* Create sink ghost pads */
  v_sink_pad =
      gst_element_get_static_pad (gcc->priv->video_converter_bin, "sink");
  gst_element_add_pad (gcc->priv->splitter_bin, gst_ghost_pad_new ("videosink",
          v_sink_pad));
  gst_object_unref (v_sink_pad);

  /* Add pad probes for the encoding branch */
  v_prev_queue_pad = gst_element_get_static_pad (v_prev_queue, "src");
  gst_pad_add_buffer_probe (v_prev_queue_pad,
      (GCallback) gst_camera_capturer_video_encoding_probe, gcc);
  gst_object_unref (v_prev_queue_pad);

}

static void
gst_camera_capturer_fill_audio_splitter_bin (GstCameraCapturer * gcc)
{
  GstElement *a_queue, *a_prev_queue;
  GstPad *a_queue_pad, *a_prev_queue_pad, *a_sink_pad, *ghost_pad;

  /* Create elements */
  gcc->priv->audio_appsrc = gst_element_factory_make ("appsrc", "audio-appsrc");
  a_queue = gst_element_factory_make ("queue", "audio-queue");
  a_prev_queue = gst_element_factory_make ("queue", "audio-preview-queue");

  g_object_set (a_queue, "max-size-time", 1 * GST_SECOND, NULL);

  gst_bin_add_many (GST_BIN (gcc->priv->splitter_bin), gcc->priv->audio_appsrc,
      a_queue, a_prev_queue, NULL);

  /* Link appsrc to the queue */
  gst_element_link (gcc->priv->audio_appsrc, a_queue);

  /* Create src ghost pads */
  a_queue_pad = gst_element_get_static_pad (a_queue, "src");
  a_prev_queue_pad = gst_element_get_static_pad (a_prev_queue, "src");
  ghost_pad = gst_ghost_pad_new ("audiosrc", a_queue_pad);
  gst_pad_set_active (ghost_pad, TRUE);
  gst_element_add_pad (gcc->priv->splitter_bin, ghost_pad);
  ghost_pad = gst_ghost_pad_new ("audio_preview", a_prev_queue_pad);
  gst_pad_set_active (ghost_pad, TRUE);
  gst_element_add_pad (gcc->priv->splitter_bin, ghost_pad);
  gst_object_unref (a_queue_pad);
  gst_object_unref (a_prev_queue_pad);

  /* Create sink ghost pads */
  a_sink_pad = gst_element_get_static_pad (a_prev_queue, "sink");
  ghost_pad = gst_ghost_pad_new ("audiosink", a_sink_pad);
  gst_pad_set_active (ghost_pad, TRUE);
  gst_element_add_pad (gcc->priv->splitter_bin, ghost_pad);
  gst_object_unref (a_sink_pad);

  /* Add pad probes for the encoding branch */
  a_prev_queue_pad = gst_element_get_static_pad (a_prev_queue, "src");
  gst_pad_add_buffer_probe (a_prev_queue_pad,
      (GCallback) gst_camera_capturer_audio_encoding_probe, gcc);
  gst_object_unref (a_prev_queue_pad);
}

static void
gst_camera_capturer_link_encoder_bin (GstCameraCapturer * gcc)
{
  GstPad *v_dec_pad, *v_enc_pad;

  GST_INFO_OBJECT (gcc, "Linking encoder bin");

  gst_bin_add (GST_BIN (gcc->priv->main_pipeline), gcc->priv->encoder_bin);

  v_dec_pad = gst_element_get_static_pad (gcc->priv->splitter_bin, "videosrc");
  v_enc_pad = gst_element_get_static_pad (gcc->priv->encoder_bin, "video");
  gst_pad_link (v_dec_pad, v_enc_pad);
  gst_object_unref (v_dec_pad);
  gst_object_unref (v_enc_pad);

  if (gcc->priv->audio_enabled) {
    GstPad *a_dec_pad, *a_enc_pad;

    a_dec_pad =
        gst_element_get_static_pad (gcc->priv->splitter_bin, "audiosrc");
    a_enc_pad = gst_element_get_static_pad (gcc->priv->encoder_bin, "audio");
    gst_pad_link (a_dec_pad, a_enc_pad);
    gst_object_unref (a_dec_pad);
    gst_object_unref (a_enc_pad);
  }

  gst_element_set_state (gcc->priv->encoder_bin, GST_STATE_PLAYING);
}

static void
gst_camera_capturer_link_preview (GstCameraCapturer * gcc)
{
  GstPad *v_dec_prev_pad, *v_prev_pad, *v_src_pad, *v_split_pad;

  GST_INFO_OBJECT (gcc, "Linking preview bin");

  /* Link source to splitter */
  gst_bin_add (GST_BIN (gcc->priv->main_pipeline), gcc->priv->splitter_bin);
  v_src_pad = gst_element_get_static_pad (gcc->priv->source_bin, "video");
  v_split_pad =
      gst_element_get_static_pad (gcc->priv->splitter_bin, "videosink");
  gst_pad_link (v_src_pad, v_split_pad);
  gst_object_unref (v_src_pad);
  gst_object_unref (v_split_pad);

  /* Link splitter to preview */
  v_dec_prev_pad =
      gst_element_get_static_pad (gcc->priv->splitter_bin, "video_preview");
  v_prev_pad = gst_element_get_static_pad (gcc->priv->preview_bin, "video");
  gst_pad_link (v_dec_prev_pad, v_prev_pad);
  gst_object_unref (v_dec_prev_pad);
  gst_object_unref (v_prev_pad);
}

static void
gst_camera_capturer_link_audio_preview (GstCameraCapturer * gcc)
{
  GstPad *a_dec_prev_pad, *a_prev_pad, *a_src_pad, *a_split_pad;

  /* Link audio source to splitter */
  a_src_pad = gst_element_get_static_pad (gcc->priv->source_bin, "audio");
  a_split_pad =
      gst_element_get_static_pad (gcc->priv->splitter_bin, "audiosink");
  gst_pad_link (a_src_pad, a_split_pad);
  gst_object_unref (a_src_pad);
  gst_object_unref (a_split_pad);

  /* Link audio splitter to preview */
  a_dec_prev_pad =
      gst_element_get_static_pad (gcc->priv->splitter_bin, "audio_preview");
  a_prev_pad = gst_element_get_static_pad (gcc->priv->preview_bin, "audio");
  gst_pad_link (a_dec_prev_pad, a_prev_pad);
  gst_object_unref (a_dec_prev_pad);
  gst_object_unref (a_prev_pad);
}

static gboolean
cb_last_buffer (GstPad * pad, GstBuffer * buf, GstCameraCapturer * gcc)
{
  if (buf != NULL) {
    if (gcc->priv->last_buffer != NULL) {
      gst_buffer_unref (gcc->priv->last_buffer);
    }
    gcc->priv->last_buffer = gst_buffer_ref (buf);
  }
  return TRUE;
}

static void
cb_new_prev_pad (GstElement * element, GstPad * pad, GstElement * bin)
{
  GstPad *sink_pad;

  sink_pad = gst_element_get_static_pad (bin, "sink");
  gst_pad_link (pad, sink_pad);
  gst_object_unref (sink_pad);
}

static void
gst_camera_capturer_create_preview (GstCameraCapturer * gcc)
{
  GstElement *v_decoder, *video_bin;
  GstPad *video_pad;

  GST_INFO_OBJECT (gcc, "Create preview bin");
  v_decoder = gst_element_factory_make ("decodebin2", "preview-decoder");

  video_bin =
      gst_parse_bin_from_description ("videoscale ! ffmpegcolorspace ! "
      DEFAULT_VIDEO_SINK " name=videosink sync=false", TRUE, NULL);

  gcc->priv->preview_bin = gst_bin_new ("preview_bin");
  gst_bin_add_many (GST_BIN (gcc->priv->preview_bin), v_decoder, video_bin,
      NULL);

  g_signal_connect (v_decoder, "pad-added", G_CALLBACK (cb_new_prev_pad),
      video_bin);

  video_pad = gst_element_get_static_pad (video_bin, "sink");
  g_signal_connect (video_pad, "notify::caps",
      G_CALLBACK (gcc_get_video_stream_info), gcc);
  gst_pad_add_buffer_probe (video_pad, (GCallback) cb_last_buffer, gcc);
  gst_object_unref (video_pad);

  /* Create ghost pads */
  video_pad = gst_element_get_static_pad (v_decoder, "sink");
  gst_element_add_pad (gcc->priv->preview_bin, gst_ghost_pad_new ("video",
          video_pad));
  gst_object_unref (GST_OBJECT (video_pad));

  gst_bin_add (GST_BIN (gcc->priv->main_pipeline), gcc->priv->preview_bin);
  gst_element_set_state (gcc->priv->preview_bin, GST_STATE_PLAYING);
}

static void
gst_camera_capturer_fill_audio_preview (GstCameraCapturer * gcc)
{
  GstElement *a_decoder, *audio_bin;
  GstPad *audio_pad, *ghost_pad;

  a_decoder = gst_element_factory_make ("decodebin2", NULL);

  audio_bin =
      gst_parse_bin_from_description
      ("audioconvert ! audioresample ! autoaudiosink name=audiosink", TRUE,
      NULL);

  gst_bin_add_many (GST_BIN (gcc->priv->preview_bin), a_decoder, audio_bin,
      NULL);

  g_signal_connect (a_decoder, "pad-added", G_CALLBACK (cb_new_prev_pad),
      audio_bin);

  /* Create ghost pads */
  audio_pad = gst_element_get_static_pad (a_decoder, "sink");
  ghost_pad = gst_ghost_pad_new ("audio", audio_pad);
  gst_pad_set_active (ghost_pad, TRUE);
  gst_element_add_pad (gcc->priv->preview_bin, ghost_pad);
  gst_object_unref (GST_OBJECT (audio_pad));
}

static void
gst_camera_capturer_fill_audio_source_bin (GstCameraCapturer * gcc)
{
  GstPad *audio_pad, *ghost_pad;
  GstElement *identity;

  identity = gst_element_factory_make ("identity", "audio-pad");
  gst_bin_add (GST_BIN (gcc->priv->source_bin), identity);

  /* add ghostpad */
  audio_pad = gst_element_get_static_pad (identity, "src");
  ghost_pad = gst_ghost_pad_new ("audio", audio_pad);
  gst_pad_set_active (ghost_pad, TRUE);
  gst_element_add_pad (gcc->priv->source_bin, ghost_pad);
  gst_object_unref (audio_pad);
}

static GstCaps *
gst_camera_capturer_source_caps (GstCameraCapturer * gcc)
{
  gchar *caps_str;
  GstCaps *caps;

  caps = gst_caps_from_string ("video/x-raw-yuv;video/x-raw-rgb;"
      "video/x-dv, systemstream=true");
  if (gcc->priv->source_width != 0 && gcc->priv->source_height != 0) {
    gst_caps_set_simple (caps, "width", G_TYPE_INT, gcc->priv->source_width,
        "height", G_TYPE_INT, gcc->priv->source_height, NULL);
  }
  if (gcc->priv->source_fps_n != 0 && gcc->priv->source_fps_d != 0) {
    gst_caps_set_simple (caps, "framerate", GST_TYPE_FRACTION,
        gcc->priv->source_fps_n, gcc->priv->source_fps_d, NULL);
  }
  caps_str = gst_caps_to_string (caps);
  GST_INFO_OBJECT (gcc, "Source caps configured to: %s", caps_str);
  g_free (caps_str);
  return caps;
}

static void
gst_camera_capturer_create_remainig (GstCameraCapturer * gcc)
{
  gst_camera_capturer_create_splitter_bin (gcc);
  gst_camera_capturer_create_preview (gcc);

  gst_camera_capturer_link_preview (gcc);
  gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_PLAYING);
}

static void
cb_no_more_pads (GstElement * element, GstCameraCapturer * gcc)
{
  if (!gcc->priv->has_video) {
    g_signal_emit (gcc, gcc_signals[SIGNAL_ERROR], 0,
        "The stream does not contains a video track");
    return;
  }
  if (!gcc->priv->has_audio) {
    GST_INFO_OBJECT (gcc,
        "Source does not have audio: disabling audio encoding");
    gcc->priv->audio_enabled = FALSE;
  }
}

static void
cb_new_pad (GstElement * element, GstPad * pad, GstCameraCapturer * gcc)
{
  GstCaps *caps;
  const gchar *mime;
  GstElement *sink = NULL;
  GstPad *epad;
  GstBin *bin = GST_BIN (gcc->priv->source_bin);

  caps = gst_pad_get_caps_reffed (pad);
  mime = gst_structure_get_name (gst_caps_get_structure (caps, 0));
  if (!gcc->priv->has_video && g_strrstr (mime, "video")) {
    GST_INFO_OBJECT (gcc, "Found video stream");
    sink = gst_bin_get_by_name (bin, "video-pad");
    gcc->priv->has_video = TRUE;
  } else if (!gcc->priv->has_audio && g_strrstr (mime, "audio") &&
      gcc->priv->audio_enabled) {
    GST_INFO_OBJECT (gcc, "Found audio stream: enable audio branch");
    gcc->priv->has_audio = TRUE;
    gst_camera_capturer_fill_audio_source_bin (gcc);
    gst_camera_capturer_fill_audio_splitter_bin (gcc);
    gst_camera_capturer_fill_audio_preview (gcc);
    gst_camera_capturer_link_audio_preview (gcc);
    gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_PLAYING);
    sink = gst_bin_get_by_name (bin, "audio-pad");
  }

  if (sink != NULL) {
    epad = gst_element_get_static_pad (sink, "sink");
    gst_pad_link (pad, epad);
    gst_object_unref (epad);
    gst_object_unref (sink);
  }
  GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN (gcc->priv->main_pipeline),
      GST_DEBUG_GRAPH_SHOW_ALL, "longomatch-capture-link");
  gst_caps_unref (caps);
}

static gboolean
gst_camera_capturer_fill_decodebin_source (GstCameraCapturer * gcc, GstCaps * filter)
{
  GstElement *bin, *decodebin, *colorspace, *deinterlacer;
  GstPad *video_pad;

  GST_INFO_OBJECT (gcc, "Creating dv source");

  gcc->priv->video_needs_keyframe_sync = FALSE;

  bin = gcc->priv->source_bin;
  decodebin = gst_element_factory_make ("decodebin2", NULL);
  g_signal_connect (decodebin, "autoplug-select",
      G_CALLBACK (lgm_filter_video_decoders), gcc);
  colorspace = gst_element_factory_make ("ffmpegcolorspace", "video-pad");
  deinterlacer = gst_element_factory_make ("ffdeinterlace", NULL);
  if (deinterlacer == NULL)
    deinterlacer = gst_element_factory_make ("identity", NULL);

  gst_bin_add_many (GST_BIN (bin), decodebin, colorspace, deinterlacer, NULL);
  gst_element_link_filtered (gcc->priv->source, decodebin, filter);
  gst_element_link (colorspace, deinterlacer);

  /* add ghostpad */
  video_pad = gst_element_get_static_pad (deinterlacer, "src");
  gst_element_add_pad (bin, gst_ghost_pad_new ("video", video_pad));
  gst_object_unref (GST_OBJECT (video_pad));

  g_signal_connect (decodebin, "pad-added", G_CALLBACK (cb_new_pad), gcc);
  g_signal_connect (decodebin, "no-more-pads", G_CALLBACK (cb_no_more_pads),
      gcc);
  gst_bin_add (GST_BIN (gcc->priv->main_pipeline), bin);
  gst_camera_capturer_create_remainig (gcc);

  return TRUE;
}

static GstElement *
gst_camera_capturer_prepare_mpegts_source (GstCameraCapturer * gcc)
{
  GstElement *bin, *demuxer, *video, *video_parser;
  GstPad *video_pad, *src_pad;

  GST_INFO_OBJECT (gcc, "Creating mpegts source");

  gcc->priv->video_needs_keyframe_sync = TRUE;
  gcc->priv->video_synced = FALSE;

  /* We don't want to reencode, only remux */
  bin = gcc->priv->source_bin;
  demuxer = gst_element_factory_make ("mpegtsdemux", NULL);
  video_parser = gst_element_factory_make ("h264parse", "video-pad");
  video = gst_element_factory_make ("capsfilter", NULL);
  g_object_set (video, "caps",
      gst_caps_from_string ("video/x-h264, stream-format=avc, alignment=au"),
      NULL);

  gst_bin_add_many (GST_BIN (bin), demuxer, video_parser, video, NULL);
  gst_element_link (gcc->priv->source, demuxer);
  gst_element_link (video_parser, video);

  /* add ghostpad */
  video_pad = gst_element_get_static_pad (video, "src");
  gst_element_add_pad (bin, gst_ghost_pad_new ("video", video_pad));
  gst_object_unref (GST_OBJECT (video_pad));
  src_pad = gst_element_get_static_pad (demuxer, "sink");
  gst_element_add_pad (bin, gst_ghost_pad_new ("sink", src_pad));
  gst_object_unref (GST_OBJECT (src_pad));

  g_signal_connect (demuxer, "pad-added", G_CALLBACK (cb_new_pad), gcc);
  g_signal_connect (demuxer, "no-more-pads", G_CALLBACK (cb_no_more_pads), gcc);
  gst_camera_capturer_create_remuxer_bin (gcc);

  return bin;
}

static gboolean
gcc_fix_caps (GstPad * pad, GstPad * peer, GstCameraCapturer * gcc)
{
  GstCaps *caps, *setter_caps;
  const GstStructure *s;
  gint fps_n = 0, fps_d = 0;
  GstElement *capssetter, *uridecodebin;

  caps = gst_pad_get_negotiated_caps (pad);

  s = gst_caps_get_structure (caps, 0);

  /* Multipart mjpeg streams outputs buffers without timestamps
   * and with framerate=0/1. The capssetter fixes the framerate
   * forcing it to 25/1 and  and configure uridecodebin to */
  if (gst_structure_get_fraction (s, "framerate", &fps_d, &fps_d) &&
      (fps_n == 0 && fps_d == 1)) {
    capssetter = gst_bin_get_by_name (GST_BIN (gcc->priv->source_bin),
        "video-pad");
    uridecodebin = gst_bin_get_by_name (GST_BIN (gcc->priv->source_bin),
        "uridecodebin");
    g_object_set (uridecodebin, "buffer-time", 20 * GST_MSECOND, NULL);

    setter_caps = gst_caps_make_writable (caps);
    gst_caps_set_simple (setter_caps, "framerate", GST_TYPE_FRACTION, 25, 1,
        NULL);
    g_object_set (capssetter, "caps", setter_caps, NULL);
    gst_caps_unref (setter_caps);
  }

  return TRUE;
}

static gboolean
gst_camera_capturer_create_uri_source (GstCameraCapturer * gcc, GError ** err)
{
  GstElement *bin, *decodebin, *capssetter;
  GstPad *setter_pad, *video_pad;

  GST_INFO_OBJECT (gcc, "Creating URI source %s\n", gcc->priv->device_id);

  gcc->priv->video_needs_keyframe_sync = FALSE;

  gcc->priv->source_bin = bin = gst_bin_new ("source");
  decodebin = gst_element_factory_make ("uridecodebin", "uridecodebin");
  g_object_set (decodebin, "uri", gcc->priv->device_id, NULL);
  g_signal_connect (decodebin, "autoplug-select",
      G_CALLBACK (lgm_filter_video_decoders), gcc);
  capssetter = gst_element_factory_make ("capssetter", "video-pad");
  g_object_set (capssetter, "replace", TRUE, NULL);


  gst_bin_add_many (GST_BIN (bin), decodebin, capssetter, NULL);

  /* Add callback to replace invalid framerate for mjpeg streams */
  setter_pad = gst_element_get_static_pad (capssetter, "sink");
  g_signal_connect (setter_pad, "notify::caps", G_CALLBACK (gcc_fix_caps), gcc);

  /* add ghostpad */
  video_pad = gst_element_get_static_pad (capssetter, "src");
  gst_element_add_pad (bin, gst_ghost_pad_new ("video", video_pad));
  gst_object_unref (GST_OBJECT (video_pad));

  g_signal_connect (decodebin, "pad-added", G_CALLBACK (cb_new_pad), gcc);
  g_signal_connect (decodebin, "no-more-pads", G_CALLBACK (cb_no_more_pads),
      gcc);
  gst_bin_add (GST_BIN (gcc->priv->main_pipeline), bin);
  gst_camera_capturer_create_remainig (gcc);
  return TRUE;
}

static gboolean
gcc_source_caps_set (GstPad * pad, GstPad * peer, GstCameraCapturer * gcc)
{
  if (gcc->priv->source_filter) {
    GstCaps *caps;

    /* We only use the caps filter to select an output from the device
     * but the image size can change and we need to be able to adapt
     * the filter to these changes (like a camera going from 4:3 to 16:9)
     */
    caps = gst_pad_get_negotiated_caps (pad);
    g_object_set (gcc->priv->source_filter, "caps", caps, NULL);
  }

  return TRUE;
}

static gboolean
gst_camera_capturer_create_source (GstCameraCapturer * gcc, GError ** err)
{
  GstElement *bin;
  GstElement *source;
  GstCaps *source_caps;
  GstPad *source_pad;
  GstCaps *raw_caps = gst_caps_from_string ("video/x-raw-yuv; video/x-raw-rgb");
  GstCaps *dv_caps = gst_caps_from_string ("video/x-dv, systemstream=true");
  gboolean res = FALSE;

  gcc->priv->source_bin = bin = gst_bin_new ("source");
  gcc->priv->source = source =
      gst_element_factory_make (gcc->priv->source_element_name, "video-source");
  if (!source) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the %s element. "
        "Please check your GStreamer installation.",
        gcc->priv->source_element_name);
    goto beach;
  }
  gst_bin_add (GST_BIN (bin), source);

  /* dshowvideosrc's device must be set before linking the element
   * since the device is set in getcaps and can't be changed later */
  gst_camera_capturer_update_device_id (gcc);

  /* So the internal device of the source is opened and the following getcaps
   * returns something sensible. */
  gst_element_set_state (source, GST_STATE_READY);

  source_pad = gst_element_get_static_pad (source, "src");
  source_caps = gst_pad_get_caps_reffed (source_pad);

  g_signal_connect (source_pad, "notify::caps",
      G_CALLBACK (gcc_source_caps_set), gcc);

  if (gst_caps_can_intersect (source_caps, raw_caps)) {
    GstPad *filter_pad;
    GstElement *filter;
    GstCaps *link_caps;

    filter = gcc->priv->source_filter =
        gst_element_factory_make ("capsfilter", NULL);
    gst_bin_add (GST_BIN (bin), filter);
    gst_element_link (source, filter);
    link_caps = gst_camera_capturer_source_caps (gcc);
    if (link_caps) {
      g_object_set (filter, "caps", link_caps, NULL);
      gst_caps_unref (link_caps);
    }
    filter_pad = gst_element_get_static_pad (filter, "src");
    gst_element_add_pad (bin, gst_ghost_pad_new ("video", filter_pad));
    gst_bin_add (GST_BIN (gcc->priv->main_pipeline), gcc->priv->source_bin);
    gst_camera_capturer_create_remainig (gcc);
    gcc->priv->audio_enabled = FALSE;
    res = TRUE;
  } else if (gst_caps_can_intersect (source_caps, dv_caps)) {
    gst_camera_capturer_fill_decodebin_source (gcc, dv_caps);
    res = TRUE;
  }
  gst_object_unref (source_pad);
  gst_caps_unref (source_caps);

  GST_INFO_OBJECT (gcc, "Created video source %s",
      gcc->priv->source_element_name);

beach:
  gst_caps_unref (raw_caps);
  gst_caps_unref (dv_caps);
  return res;
}

static gboolean
gst_camera_capturer_create_video_source (GstCameraCapturer * gcc,
    CaptureSourceType type, GError ** err)
{
  gboolean ret;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  switch (type) {
    case CAPTURE_SOURCE_TYPE_FILE:
      gcc->priv->source_element_name = g_strdup ("filesrc");
    case CAPTURE_SOURCE_TYPE_DV:
    case CAPTURE_SOURCE_TYPE_SYSTEM:
      ret = gst_camera_capturer_create_source (gcc, err);
      break;
    case CAPTURE_SOURCE_TYPE_URI:
      ret = gst_camera_capturer_create_uri_source (gcc, err);
      break;
    default:
      g_assert_not_reached ();
  }
  return ret;

}

static gboolean
gst_camera_capturer_create_video_encoder (GstCameraCapturer * gcc,
    VideoEncoderType type, GError ** err, gboolean hardware_acceleration)
{
  GstElement *encoder = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  encoder = lgm_create_video_encoder (type, gcc->priv->video_quality, TRUE,
      GCC_ERROR, err, hardware_acceleration);
  if (!encoder) {
    return FALSE;
  }

  gcc->priv->video_encoder_type = type;
  gcc->priv->video_enc = encoder;
  return TRUE;
}

static gboolean
gst_camera_capturer_create_audio_encoder (GstCameraCapturer * gcc,
    AudioEncoderType type, GError ** err)
{
  GstElement *encoder = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  encoder = lgm_create_audio_encoder (type, gcc->priv->audio_quality,
      GCC_ERROR, err);
  if (!encoder) {
    return FALSE;
  }

  gcc->priv->audio_encoder_type = type;
  gcc->priv->audio_enc = encoder;
  return TRUE;
}

static gboolean
gst_camera_capturer_create_video_muxer (GstCameraCapturer * gcc,
    VideoMuxerType type, GError ** err)
{
  GstElement *muxer = NULL;

  g_return_val_if_fail (gcc != NULL, FALSE);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), FALSE);

  muxer = lgm_create_muxer (type, GCC_ERROR, err);
  if (!muxer) {
    return FALSE;
  }
  gcc->priv->video_muxer_type = type;
  gcc->priv->muxer = muxer;
  return TRUE;
}

static void
gst_camera_capturer_initialize (GstCameraCapturer * gcc, gboolean hardware_acceleration)
{
  GError *err = NULL;

  GST_INFO_OBJECT (gcc, "Initializing encoders");
  if (!gst_camera_capturer_create_video_encoder (gcc,
          gcc->priv->video_encoder_type, &err, hardware_acceleration))
    goto missing_plugin;
  if (!gst_camera_capturer_create_audio_encoder (gcc,
          gcc->priv->audio_encoder_type, &err))
    goto missing_plugin;
  if (!gst_camera_capturer_create_video_muxer (gcc,
          gcc->priv->video_muxer_type, &err))
    goto missing_plugin;

  GST_INFO_OBJECT (gcc, "Initializing source");
  if (!gst_camera_capturer_create_video_source (gcc,
          gcc->priv->source_type, &err))
    goto missing_plugin;

  return;

missing_plugin:
  g_signal_emit (gcc, gcc_signals[SIGNAL_ERROR], 0, err->message);
  g_error_free (err);
}

static void
gcc_encoder_send_event (GstCameraCapturer * gcc, GstEvent * event)
{
  GstPad *video_pad, *audio_pad;

  if (gcc->priv->encoder_bin == NULL)
    return;

  if (gcc->priv->audio_enabled) {
    gst_event_ref (event);
    audio_pad = gst_element_get_static_pad (gcc->priv->encoder_bin, "audio");
    gst_pad_send_event (audio_pad, event);
    gst_object_unref (audio_pad);
  }

  video_pad = gst_element_get_static_pad (gcc->priv->encoder_bin, "video");
  gst_pad_send_event (video_pad, event);
  gst_object_unref (video_pad);
}

static void
gcc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  GstCameraCapturer *gcc = (GstCameraCapturer *) data;
  GstMessageType msg_type;

  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  msg_type = GST_MESSAGE_TYPE (message);

  switch (msg_type) {
    case GST_MESSAGE_ERROR:
    {
      if (gcc->priv->main_pipeline) {
        gst_camera_capturer_stop (gcc);
        gst_camera_capturer_close (gcc);
        gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_NULL);
      }
      gcc_error_msg (gcc, message);
      break;
    }

    case GST_MESSAGE_WARNING:
    {
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;
    }

    case GST_MESSAGE_EOS:
    {
      GST_INFO_OBJECT (gcc, "EOS message");
      g_signal_emit (gcc, gcc_signals[SIGNAL_EOS], 0);
      break;
    }

    case GST_MESSAGE_STATE_CHANGED:
    {
      GstState old_state, new_state;

      gst_message_parse_state_changed (message, &old_state, &new_state, NULL);

      if (old_state == new_state)
        break;

      /* we only care about playbin (pipeline) state changes */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (gcc->priv->main_pipeline))
        break;

      if (new_state == GST_STATE_PLAYING) {
        GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN (gcc->priv->main_pipeline),
            GST_DEBUG_GRAPH_SHOW_ALL, "longomatch-capture-playing");
      }
    }

    case GST_MESSAGE_ELEMENT:
    {
      const GstStructure *s;
      gint device_change = 0;

      /* We only care about messages sent by the device source */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (gcc->priv->source))
        break;

      s = gst_message_get_structure (message);
      /* check if it's bus reset message and it contains the
       * 'current-device-change' field */
      if (g_strcmp0 (gst_structure_get_name (s), "ieee1394-bus-reset"))
        break;
      if (!gst_structure_has_field (s, "current-device-change"))
        break;


      /* emit a signal if the device was connected or disconnected */
      gst_structure_get_int (s, "current-device-change", &device_change);

      if (device_change != 0)
        g_signal_emit (gcc, gcc_signals[SIGNAL_DEVICE_CHANGE], 0,
            device_change);
      break;
    }

    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
}

static void
gcc_error_msg (GstCameraCapturer * gcc, GstMessage * msg)
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
    GST_DEBUG_BIN_TO_DOT_FILE (GST_BIN (gcc->priv->main_pipeline),
        GST_DEBUG_GRAPH_SHOW_ALL, "longomatch-capture-error");
    g_signal_emit (gcc, gcc_signals[SIGNAL_ERROR], 0, err->message);
    g_error_free (err);
  }
  g_free (dbg);
}

static void
gcc_element_msg_sync (GstBus * bus, GstMessage * msg, gpointer data)
{
  GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (data);

  g_assert (msg->type == GST_MESSAGE_ELEMENT);

  if (msg->structure == NULL)
    return;

  /* This only gets sent if we haven't set an ID yet. This is our last
   * chance to set it before the video sink will create its own window */
  if (gst_structure_has_name (msg->structure, "prepare-xwindow-id")) {

    if (gcc->priv->xoverlay == NULL) {
      GstObject *sender = GST_MESSAGE_SRC (msg);
      if (sender && GST_IS_X_OVERLAY (sender)) {
        gcc->priv->xoverlay = GST_X_OVERLAY (gst_object_ref (sender));
        g_object_set (gcc->priv->xoverlay, "sync", FALSE, NULL);
      }
    }

    g_return_if_fail (gcc->priv->xoverlay != NULL);
    g_return_if_fail (gcc->priv->window_handle != 0);

    g_object_set (GST_ELEMENT (gcc->priv->xoverlay), "force-aspect-ratio",
        FALSE, NULL);
    lgm_set_window_handle (gcc->priv->xoverlay, gcc->priv->window_handle);
  }
}

static gboolean
gcc_get_video_stream_info (GstPad * pad, GstPad * peer, GstCameraCapturer * gcc)
{
  GstStructure *s;
  GstCaps *caps;

  caps = gst_pad_get_negotiated_caps (pad);

  if (!(caps)) {
    GST_WARNING_OBJECT (gcc, "Could not get stream info");
    return FALSE;
  }

  /* Get the source caps */
  s = gst_caps_get_structure (caps, 0);
  if (s) {
    /* We need at least width/height and framerate */
    if (!
        (gst_structure_get_fraction
            (s, "framerate", &gcc->priv->video_fps_n, &gcc->priv->video_fps_d)
            && gst_structure_get_fraction (s, "pixel-aspect-ratio",
                &gcc->priv->video_par_n, &gcc->priv->video_par_d)
            && gst_structure_get_int (s, "width", &gcc->priv->video_width)
            && gst_structure_get_int (s, "height", &gcc->priv->video_height)))
      return FALSE;
    g_signal_emit (gcc, gcc_signals[SIGNAL_MEDIA_INFO], 0,
        gcc->priv->video_width, gcc->priv->video_height,
        gcc->priv->video_par_n, gcc->priv->video_par_d);
  }
  return FALSE;
}

/*******************************************
 *
 *         Public methods
 *
 * ****************************************/

void
gst_camera_capturer_run (GstCameraCapturer * gcc, gboolean hardware_acceleration)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  gst_camera_capturer_initialize (gcc, hardware_acceleration);
  gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_PLAYING);
}

void
gst_camera_capturer_close (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_NULL);
  gst_element_get_state (gcc->priv->main_pipeline, NULL, NULL, -1);
  if (gcc->priv->xoverlay != NULL) {
    gst_object_unref (gcc->priv->xoverlay);
    gcc->priv->xoverlay = NULL;
  }
}

void
gst_camera_capturer_start (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  GST_INFO_OBJECT (gcc, "Started capture");
  g_mutex_lock (&gcc->priv->recording_lock);
  if (!gcc->priv->is_recording
      && gcc->priv->accum_recorded_ts == GST_CLOCK_TIME_NONE) {
    gcc->priv->accum_recorded_ts = 0;
    gcc->priv->is_recording = TRUE;
    gst_camera_capturer_create_encoder_bin (gcc);
    gst_camera_capturer_link_encoder_bin (gcc);
  }
  g_mutex_unlock (&gcc->priv->recording_lock);
}

void
gst_camera_capturer_toggle_pause (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

  g_mutex_lock (&gcc->priv->recording_lock);
  if (!gcc->priv->is_recording) {
    gcc->priv->current_recording_start_ts = GST_CLOCK_TIME_NONE;
    gcc->priv->is_recording = TRUE;
  } else {
    gcc->priv->is_recording = FALSE;
    gcc->priv->video_synced = FALSE;
  }
  g_mutex_unlock (&gcc->priv->recording_lock);

  GST_INFO_OBJECT (gcc, "Capture state changed to %s",
      gcc->priv->is_recording ? "recording" : "paused");
}

static void
destroy_pixbuf (guchar * pix, gpointer data)
{
  gst_buffer_unref (GST_BUFFER (data));
}

void
gst_camera_capturer_unref_pixbuf (GdkPixbuf * pixbuf)
{
  g_object_unref (pixbuf);
}

GdkPixbuf *
gst_camera_capturer_get_current_frame (GstCameraCapturer * gcc)
{
  GstStructure *s;
  GdkPixbuf *pixbuf;
  GstBuffer *last_buffer;
  GstBuffer *buf;
  GstCaps *to_caps;
  gint outwidth = 0;
  gint outheight = 0;

  g_return_val_if_fail (gcc != NULL, NULL);
  g_return_val_if_fail (GST_IS_CAMERA_CAPTURER (gcc), NULL);

  gst_element_get_state (gcc->priv->main_pipeline, NULL, NULL, -1);

  /* no video info */
  if (!gcc->priv->video_width || !gcc->priv->video_height) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s", "no video info");
    g_warning ("Could not take screenshot: %s", "no video info");
    return NULL;
  }

  /* get frame */
  last_buffer = gcc->priv->last_buffer;
  gst_buffer_ref (last_buffer);

  if (!last_buffer) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "no last video frame");
    g_warning ("Could not take screenshot: %s", "no last video frame");
    return NULL;
  }

  if (GST_BUFFER_CAPS (last_buffer) == NULL) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "no caps on buffer");
    g_warning ("Could not take screenshot: %s", "no caps on buffer");
    return NULL;
  }

  /* convert to our desired format (RGB24) */
  to_caps = gst_caps_new_simple ("video/x-raw-rgb",
      "bpp", G_TYPE_INT, 24, "depth", G_TYPE_INT, 24,
      /* Note: we don't ask for a specific width/height here, so that
       * videoscale can adjust dimensions from a non-1/1 pixel aspect
       * ratio to a 1/1 pixel-aspect-ratio */
      "pixel-aspect-ratio", GST_TYPE_FRACTION, 1,
      1, "endianness", G_TYPE_INT, G_BIG_ENDIAN,
      "red_mask", G_TYPE_INT, 0xff0000,
      "green_mask", G_TYPE_INT, 0x00ff00,
      "blue_mask", G_TYPE_INT, 0x0000ff, NULL);

  if (gcc->priv->video_fps_n > 0 && gcc->priv->video_fps_d > 0) {
    gst_caps_set_simple (to_caps, "framerate", GST_TYPE_FRACTION,
        gcc->priv->video_fps_n, gcc->priv->video_fps_d, NULL);
  }

  GST_DEBUG_OBJECT (gcc, "frame caps: %" GST_PTR_FORMAT,
      GST_BUFFER_CAPS (gcc->priv->last_buffer));
  GST_DEBUG_OBJECT (gcc, "pixbuf caps: %" GST_PTR_FORMAT, to_caps);

  /* bvw_frame_conv_convert () takes ownership of the buffer passed */
  buf = bvw_frame_conv_convert (last_buffer, to_caps);

  gst_caps_unref (to_caps);
  gst_buffer_unref (last_buffer);

  if (!buf) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "conversion failed");
    g_warning ("Could not take screenshot: %s", "conversion failed");
    return NULL;
  }

  if (!GST_BUFFER_CAPS (buf)) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "no caps on output buffer");
    g_warning ("Could not take screenshot: %s", "no caps on output buffer");
    return NULL;
  }

  s = gst_caps_get_structure (GST_BUFFER_CAPS (buf), 0);
  gst_structure_get_int (s, "width", &outwidth);
  gst_structure_get_int (s, "height", &outheight);
  g_return_val_if_fail (outwidth > 0 && outheight > 0, NULL);

  /* create pixbuf from that - we don't want to use the gstreamer's buffer
   * because the GTK# bindings won't call the destroy funtion */
  pixbuf = gdk_pixbuf_new_from_data (GST_BUFFER_DATA (buf),
      GDK_COLORSPACE_RGB, FALSE, 8, outwidth,
      outheight, GST_ROUND_UP_4 (outwidth * 3), destroy_pixbuf, buf);

  if (!pixbuf) {
    GST_DEBUG_OBJECT (gcc, "Could not take screenshot: %s",
        "could not create pixbuf");
    g_warning ("Could not take screenshot: %s", "could not create pixbuf");
  }

  return pixbuf;
}


void
gst_camera_capturer_stop (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);
  g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

#ifdef WIN32
  //On windows we can't handle device disconnections until dshowvideosrc
  //supports it. When a device is disconnected, the source is locked
  //in ::create(), blocking the streaming thread. We need to change its
  //state to null, this way camerabin doesn't block in ::do_stop().
  gst_element_set_state (gcc->priv->source, GST_STATE_NULL);
#endif

  GST_INFO_OBJECT (gcc, "Closing capture");
  g_mutex_lock (&gcc->priv->recording_lock);
  gcc->priv->closing_recording = TRUE;
  gcc->priv->is_recording = FALSE;
  g_mutex_unlock (&gcc->priv->recording_lock);

  gcc_encoder_send_event (gcc, gst_event_new_eos ());
}

void
gst_camera_capturer_expose (GstCameraCapturer * gcc)
{
  g_return_if_fail (gcc != NULL);

  if (gcc->priv->xoverlay != NULL && GST_IS_X_OVERLAY (gcc->priv->xoverlay)) {
    gst_x_overlay_expose (gcc->priv->xoverlay);
  }
}

void
gst_camera_capturer_configure (GstCameraCapturer * gcc,
    const gchar * filename, CaptureSourceType source,
    const gchar * source_element, const gchar * device_id,
    gint source_width, gint source_height, gint source_fps_n, gint source_fps_d,
    VideoEncoderType video_encoder, AudioEncoderType audio_encoder,
    VideoMuxerType muxer, guint video_bitrate, guint audio_bitrate,
    guint record_audio, guint output_width, guint output_height,
    guintptr window_handle)
{
  gcc->priv->output_file = g_strdup (filename);
  gcc->priv->source_type = source;
  gcc->priv->device_id = g_strdup (device_id);
  gcc->priv->source_element_name = g_strdup (source_element);
  gcc->priv->video_encoder_type = video_encoder;
  gcc->priv->audio_encoder_type = audio_encoder;
  gcc->priv->video_muxer_type = muxer;
  gcc->priv->video_quality = video_bitrate;
  gcc->priv->audio_quality = audio_bitrate;
  gcc->priv->audio_enabled = record_audio;
  gcc->priv->output_height = output_height;
  gcc->priv->output_width = output_width;
  gcc->priv->window_handle = window_handle;
  gcc->priv->source_width = source_width;
  gcc->priv->source_height = source_height;
  gcc->priv->source_fps_n = source_fps_n;
  gcc->priv->source_fps_d = source_fps_d;
}

GstCameraCapturer *
gst_camera_capturer_new (GError ** err)
{
  GstCameraCapturer *gcc = NULL;

  if (_cesarplayer_gst_debug_cat == NULL) {
    GST_DEBUG_CATEGORY_INIT (_cesarplayer_gst_debug_cat, "longomatch", 0,
        "LongoMatch GStreamer Backend");
  }

  gcc = g_object_new (GST_TYPE_CAMERA_CAPTURER, NULL);

  gcc->priv->main_pipeline = gst_pipeline_new ("main_pipeline");

  if (!gcc->priv->main_pipeline) {
    g_set_error (err,
        GCC_ERROR,
        GST_ERROR_PLUGIN_LOAD,
        "Failed to create the pipeline element. "
        "Please check your GStreamer installation.");
    goto missing_plugin;
  }

  /* assume we're always called from the main Gtk+ GUI thread */
  gui_thread = g_thread_self ();

  /*Connect bus signals */
  GST_INFO_OBJECT (gcc, "Connecting bus signals");
  gcc->priv->bus = gst_element_get_bus (GST_ELEMENT (gcc->priv->main_pipeline));
  gst_bus_add_signal_watch (gcc->priv->bus);
  gcc->priv->sig_bus_async =
      g_signal_connect (gcc->priv->bus, "message",
      G_CALLBACK (gcc_bus_message_cb), gcc);

  /* we want to catch "prepare-xwindow-id" element messages synchronously */
  gst_bus_set_sync_handler (gcc->priv->bus, gst_bus_sync_signal_handler, gcc);

  gcc->priv->sig_bus_sync =
      g_signal_connect (gcc->priv->bus, "sync-message::element",
      G_CALLBACK (gcc_element_msg_sync), gcc);

  return gcc;

/* Missing plugin */
missing_plugin:
  {
    g_object_ref_sink (gcc);
    g_object_unref (gcc);
    return NULL;
  }
}
