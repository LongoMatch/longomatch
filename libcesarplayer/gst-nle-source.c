/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
* Gstreamer NLE source
* Copyright (C)  Andoni Morales Alastruey 2013 <ylatuya@gmail.com>
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
#include <gst/gst.h>
#include <gst/video/video.h>
#include <gst/app/gstappsink.h>

#include "lgm-utils.h"
#include "gst-nle-source.h"

GST_DEBUG_CATEGORY (_nlesrc_gst_debug_cat);
#define GST_CAT_DEFAULT _nlesrc_gst_debug_cat

#define CHANNELS 2
#define DEPTH 16
#define RATE 44100
#define BITS_PER_SAMPLE DEPTH*CHANNELS*RATE
#define AUDIO_CAPS_STR "audio/x-raw-int, endianness=1234, signed=true, "\
      " width=16, depth=16, rate=44100, channels=2"

static GstStaticPadTemplate video_sink_tpl = GST_STATIC_PAD_TEMPLATE ("video",
    GST_PAD_SINK,
    GST_PAD_ALWAYS,
    GST_STATIC_CAPS ("video/x-raw-yuv; video/x-raw-rgb"));

static GstStaticPadTemplate audio_sink_tpl = GST_STATIC_PAD_TEMPLATE ("audio",
    GST_PAD_SINK,
    GST_PAD_ALWAYS,
    GST_STATIC_CAPS ("audio/x-raw-int; audio/x-raw-float"));

static GstStaticPadTemplate video_src_tpl = GST_STATIC_PAD_TEMPLATE ("video",
    GST_PAD_SRC,
    GST_PAD_SOMETIMES,
    GST_STATIC_CAPS ("video/x-raw-yuv"));

static GstStaticPadTemplate audio_src_tpl = GST_STATIC_PAD_TEMPLATE ("audio",
    GST_PAD_SRC,
    GST_PAD_SOMETIMES,
    GST_STATIC_CAPS (AUDIO_CAPS_STR));

typedef struct
{
  gchar *file_path;
  gchar *title;
  guint64 start;
  guint64 stop;
  guint64 duration;
  gfloat rate;
  gboolean still_picture;
  GstNleRectangle roi;
} GstNleSrcItem;

static GstBinClass *parent_class = NULL;

static void gst_nle_source_dispose (GObject * object);
static GstStateChangeReturn gst_nle_source_change_state
    (GstElement * element, GstStateChange transition);
static void gst_nle_source_next (GstNleSource * nlesrc);
static void gst_nle_source_next_threaded (GstNleSource * nlesrc);
static void gst_nle_source_no_more_pads (GstElement * element,
    GstNleSource * nlesrc);
static void gst_nle_source_pad_added_cb (GstElement * element, GstPad * pad,
    GstNleSource * nlesrc);

G_DEFINE_TYPE (GstNleSource, gst_nle_source, GST_TYPE_BIN);

static GstNleSrcItem *
gst_nle_source_item_new (const gchar * file_path, const gchar * title,
    guint64 start, guint64 stop, gfloat rate, gboolean still_picture,
    GstNleRectangle roi)
{
  GstNleSrcItem *item;

  item = g_new0 (GstNleSrcItem, 1);
  item->file_path = g_strdup (file_path);
  item->title = g_strdup (title);
  item->start = start;
  item->stop = stop;
  item->rate = rate;
  item->still_picture = still_picture;
  if (still_picture) {
    item->rate = 1;
  }
  item->roi = roi;
  if (GST_CLOCK_TIME_IS_VALID (stop))
    item->duration = stop - start;
  else
    item->duration = GST_CLOCK_TIME_NONE;

  return item;
}

static void
gst_nle_source_item_free (GstNleSrcItem * item)
{
  if (item->file_path != NULL)
    g_free (item->file_path);
  if (item->title != NULL)
    g_free (item->title);
  g_free (item);
}

static void
gst_nle_source_init (GstNleSource * nlesrc)
{
  nlesrc->video_srcpad = gst_ghost_pad_new_no_target_from_template ("video",
      gst_static_pad_template_get (&video_src_tpl));
  nlesrc->audio_srcpad = gst_ghost_pad_new_no_target_from_template ("audio",
      gst_static_pad_template_get (&audio_src_tpl));
  nlesrc->video_sinkpad =
      gst_ghost_pad_new_no_target_from_template ("video_sink",
      gst_static_pad_template_get (&video_sink_tpl));
  nlesrc->audio_sinkpad =
      gst_ghost_pad_new_no_target_from_template ("audio_sink",
      gst_static_pad_template_get (&audio_sink_tpl));
  gst_pad_set_active (nlesrc->video_sinkpad, TRUE);
  gst_pad_set_active (nlesrc->audio_sinkpad, TRUE);
  gst_element_add_pad (GST_ELEMENT (nlesrc),
      gst_object_ref (nlesrc->video_sinkpad));
  gst_element_add_pad (GST_ELEMENT (nlesrc),
      gst_object_ref (nlesrc->audio_sinkpad));
  g_mutex_init (&nlesrc->stream_lock);
}

static void
gst_nle_source_class_init (GstNleSourceClass * klass)
{
  GObjectClass *object_class;
  GstElementClass *element_class;

  object_class = (GObjectClass *) klass;
  element_class = (GstElementClass *) klass;
  parent_class = g_type_class_peek_parent (klass);

  /* GObject */
  object_class->dispose = gst_nle_source_dispose;

  /* GstElement */
  element_class->change_state = gst_nle_source_change_state;

  GST_DEBUG_CATEGORY_INIT (_nlesrc_gst_debug_cat, "longomatch", 0,
      "LongoMatch GStreamer Backend");
}

static void
gst_nle_source_bus_message (GstBus * bus, GstMessage * message,
    GstNleSource * nlesrc)
{
  switch (message->type) {
    case GST_MESSAGE_ERROR:
      gst_nle_source_next_threaded (nlesrc);
      break;
    default:
      break;
  }
}

static void
gst_nle_source_dispose (GObject * object)
{
  GstNleSource *nlesrc = GST_NLE_SOURCE (object);

  if (nlesrc->queue != NULL) {
    g_list_free_full (nlesrc->queue, (GDestroyNotify) gst_nle_source_item_free);
    nlesrc->queue = NULL;
  }

  if (nlesrc->source != NULL) {
    gst_object_unref (nlesrc->source);
    nlesrc->source = NULL;
  }

  if (nlesrc->decoder != NULL) {
    gst_element_set_state (nlesrc->decoder, GST_STATE_NULL);
    gst_object_unref (nlesrc->decoder);
    nlesrc->decoder = NULL;
  }

  gst_object_unref (nlesrc->video_srcpad);
  gst_object_unref (nlesrc->video_sinkpad);
  gst_object_unref (nlesrc->audio_srcpad);
  gst_object_unref (nlesrc->audio_sinkpad);

  G_OBJECT_CLASS (parent_class)->dispose (object);
}

static GstCaps *
gst_nle_source_get_audio_caps (GstNleSource * nlesrc)
{
  return gst_caps_from_string (AUDIO_CAPS_STR);
}

static void
gst_nle_source_setup (GstNleSource * nlesrc)
{
  GstElement *rotate, *videorate, *videoscale, *colorspace, *vident;
  GstElement *audiorate, *audioconvert, *audioresample, *aident;
  GstElement *a_capsfilter, *v_capsfilter;
  GstPad *v_pad, *a_pad;
  GstCaps *v_caps, *a_caps;

  rotate = gst_element_factory_make ("flurotate", NULL);
  videorate = gst_element_factory_make ("videorate", NULL);
  nlesrc->videocrop = gst_element_factory_make ("videocrop", NULL);
  videoscale = gst_element_factory_make ("videoscale", NULL);
  colorspace = gst_element_factory_make ("ffmpegcolorspace", NULL);
  v_capsfilter = gst_element_factory_make ("capsfilter", "video_capsfilter");
  nlesrc->textoverlay = gst_element_factory_make ("textoverlay", NULL);
  vident = gst_element_factory_make ("identity", NULL);

  v_caps = gst_caps_new_simple ("video/x-raw-yuv",
      "format", GST_TYPE_FOURCC, GST_STR_FOURCC ("I420"),
      "width", G_TYPE_INT, (gint) nlesrc->width,
      "height", G_TYPE_INT, (gint) nlesrc->height,
      "pixel-aspect-ratio", GST_TYPE_FRACTION, 1, 1,
      "framerate", GST_TYPE_FRACTION,
      (gint) nlesrc->fps_n, (gint) nlesrc->fps_d, NULL);

  if (rotate) {
    gst_caps_set_simple (v_caps, "rotation", G_TYPE_INT, (gint) 0, NULL);
  } else {
    rotate = gst_element_factory_make ("identity", NULL); 
  }

  gst_pad_set_caps (nlesrc->video_srcpad, v_caps);

  g_object_set (videoscale, "add-borders", TRUE, NULL);
  g_object_set (vident, "single-segment", TRUE, NULL);
  g_object_set (v_capsfilter, "caps", v_caps, NULL);
  g_object_set (nlesrc->textoverlay, "valignment", 2, "halignment", 2,
      "auto-resize", TRUE, "wrap-mode", 0, "silent", !nlesrc->overlay_title,
      NULL);

  /* As videorate can duplicate a lot of buffers we want to put it last in this
     transformation bin */
  gst_bin_add_many (GST_BIN (nlesrc), rotate, nlesrc->videocrop,
      videoscale, colorspace, nlesrc->textoverlay, videorate, v_capsfilter,
      vident, NULL);
  gst_element_link_many (rotate, nlesrc->videocrop, videoscale, colorspace,
      nlesrc->textoverlay, videorate, v_capsfilter, vident, NULL);
  /* Ghost source and sink pads */
  v_pad = gst_element_get_pad (vident, "src");
  gst_ghost_pad_set_target (GST_GHOST_PAD (nlesrc->video_srcpad), v_pad);
  gst_object_unref (v_pad);

  v_pad = gst_element_get_pad (rotate, "sink");
  gst_ghost_pad_set_target (GST_GHOST_PAD (nlesrc->video_sinkpad), v_pad);
  gst_object_unref (v_pad);

  if (nlesrc->with_audio) {
    audiorate = gst_element_factory_make ("audiorate", NULL);
    audioconvert = gst_element_factory_make ("audioconvert", NULL);
    audioresample = gst_element_factory_make ("audioresample", NULL);
    a_capsfilter = gst_element_factory_make ("capsfilter", NULL);
    aident = gst_element_factory_make ("identity", NULL);

    gst_bin_add_many (GST_BIN (nlesrc), audioresample, audioconvert,
        audiorate, a_capsfilter, aident, NULL);
    gst_element_link_many (audioresample, audioconvert,
        audiorate, a_capsfilter, aident, NULL);

    a_caps = gst_nle_source_get_audio_caps (nlesrc);
    gst_pad_set_caps (nlesrc->audio_srcpad, a_caps);
    g_object_set (a_capsfilter, "caps", a_caps, NULL);

    g_object_set (aident, "single-segment", TRUE, NULL);

    /* Ghost sink and source pads */
    a_pad = gst_element_get_pad (aident, "src");
    gst_ghost_pad_set_target (GST_GHOST_PAD (nlesrc->audio_srcpad), a_pad);
    gst_object_unref (a_pad);

    a_pad = gst_element_get_pad (audioresample, "sink");
    gst_ghost_pad_set_target (GST_GHOST_PAD (nlesrc->audio_sinkpad), a_pad);
    gst_object_unref (a_pad);
  }
  nlesrc->index = -1;
  nlesrc->accu_time = 0;
  nlesrc->video_srcpad_added = FALSE;
  nlesrc->audio_srcpad_added = FALSE;
}

static void
gst_nle_source_apply_title_size (GstNleSource * nlesrc, gint size)
{
  gchar *font;

  font = g_strdup_printf ("sans bold %d", size);
  g_object_set (G_OBJECT (nlesrc->textoverlay), "font-desc", font, NULL);
  g_free (font);
}

static void
gst_nle_source_update_overlay_title (GstNleSource * nlesrc)
{
  glong length;
  GstNleSrcItem *item;

  item = (GstNleSrcItem *) g_list_nth_data (nlesrc->queue, nlesrc->index);
  g_object_set (G_OBJECT (nlesrc->textoverlay), "text", item->title, NULL);

  length = g_utf8_strlen (item->title, -1);
  length *= 2;
  if (length * nlesrc->title_size > nlesrc->height) {
    gst_nle_source_apply_title_size (nlesrc, nlesrc->height / length - 1);
  } else {
    gst_nle_source_apply_title_size (nlesrc, nlesrc->title_size);
  }
}

static void
gst_nle_source_update_videocrop (GstNleSource * nlesrc, GstCaps * caps)
{
  GstNleSrcItem *item;
  gint left, right, top, bottom;

  left = right = top = bottom = 0;

  item = (GstNleSrcItem *) g_list_nth_data (nlesrc->queue, nlesrc->index);

  GST_DEBUG_OBJECT (nlesrc, "Applying roi %d %d %dX%d\n",
      item->roi.x, item->roi.y, item->roi.width, item->roi.height);

  if (item->roi.width && item->roi.height) {
    GstStructure *structure;
    gint vwidth = 0, vheight = 0;

    structure = gst_caps_get_structure (caps, 0);

    if (gst_structure_get_int (structure, "width", &vwidth) &&
        gst_structure_get_int (structure, "height", &vheight)) {
      left = item->roi.x;
      top = item->roi.y;
      right = vwidth - item->roi.x - item->roi.width;
      bottom = vheight - item->roi.y - item->roi.height;
    }
  }

  GST_DEBUG_OBJECT (nlesrc, "Configuring videocrop left:%d "
      "right:%d top:%d left:%d\n", left, right, top, bottom);
  g_object_set (G_OBJECT (nlesrc->videocrop), "left", left, "right", right,
      "top", top, "bottom", bottom, NULL);
}

static GstFlowReturn
gst_nle_source_push_buffer (GstNleSource * nlesrc, GstBuffer * buf,
    gboolean is_audio)
{
  GstPad *sinkpad;
  gboolean push_buf;
  guint64 buf_ts, buf_rel_ts, last_ts;
  GstNleSrcItem *item;
  GstFlowReturn ret;

  item = (GstNleSrcItem *) g_list_nth_data (nlesrc->queue, nlesrc->index);
  buf_ts = GST_BUFFER_TIMESTAMP (buf);

  if (buf_ts < item->start) {
    GST_LOG_OBJECT (nlesrc, "Discard early %s buffer with ts: %"
        GST_TIME_FORMAT " start: %" GST_TIME_FORMAT,
        is_audio ? "audio" : "video", GST_TIME_ARGS (buf_ts),
        GST_TIME_ARGS (item->start));
    gst_buffer_unref (buf);
    return GST_FLOW_OK;
  }
  buf_rel_ts = buf_ts - item->start;

  g_mutex_lock (&nlesrc->stream_lock);

  if (is_audio) {
    push_buf = nlesrc->audio_seek_done;
    last_ts = nlesrc->audio_ts;
    nlesrc->audio_ts = buf_ts;
    sinkpad = nlesrc->audio_sinkpad;
  } else {
    push_buf = nlesrc->video_seek_done;
    last_ts = nlesrc->video_ts;
    nlesrc->video_ts = buf_ts;
    sinkpad = nlesrc->video_sinkpad;
  }

  if (push_buf && GST_BUFFER_TIMESTAMP (buf) >= last_ts) {
    /* Retimestamps buffer */
    guint64 new_ts = nlesrc->start_ts + buf_rel_ts / item->rate;

    GST_BUFFER_TIMESTAMP (buf) = new_ts;
    GST_LOG_OBJECT (nlesrc, "Pushing %s buffer with ts: %" GST_TIME_FORMAT
        " dur:%" GST_TIME_FORMAT " orig:%" GST_TIME_FORMAT,
        is_audio ? "audio" : "video", GST_TIME_ARGS (new_ts),
        GST_TIME_ARGS (GST_BUFFER_DURATION (buf)), GST_TIME_ARGS (buf_ts));
    if (GST_BUFFER_DURATION_IS_VALID (buf)) {
      new_ts += GST_BUFFER_DURATION (buf);
    }
    if (new_ts >= nlesrc->accu_time) {
      nlesrc->accu_time = new_ts;
    }

    if (G_UNLIKELY (!nlesrc->item_setup) && !is_audio) {
      GST_DEBUG_OBJECT (nlesrc,
          "Applying roi and title properties for this segment");
      gst_nle_source_update_videocrop (nlesrc, GST_BUFFER_CAPS (buf));
      gst_nle_source_update_overlay_title (nlesrc);
      nlesrc->item_setup = TRUE;
    }

    /* We need to unlock before pushing since push_buffer can block */
    g_mutex_unlock (&nlesrc->stream_lock);

    ret = gst_pad_chain (sinkpad, buf);
    if (ret != GST_FLOW_OK) {
      GST_WARNING_OBJECT (nlesrc, "pushing buffer returned %s",
          gst_flow_get_name (ret));
    }
    return ret;
  } else {
    GST_LOG_OBJECT (nlesrc, "Discard %s buffer with ts: %" GST_TIME_FORMAT,
        is_audio ? "audio" : "video", GST_TIME_ARGS (buf_ts));
    gst_buffer_unref (buf);
    g_mutex_unlock (&nlesrc->stream_lock);
    return GST_FLOW_OK;
  }
}

static GstBuffer *
gst_nle_source_audio_silence_buf (GstNleSource * nlesrc, guint64 start,
    guint64 duration)
{
  GstBuffer *buf;
  GstCaps *caps;

  buf = gst_buffer_new_and_alloc (BITS_PER_SAMPLE / 8 * duration / GST_SECOND);
  memset (GST_BUFFER_DATA (buf), '\0', GST_BUFFER_SIZE (buf));
  GST_BUFFER_TIMESTAMP (buf) = start;
  GST_BUFFER_DURATION (buf) = duration;
  caps = gst_nle_source_get_audio_caps (nlesrc);
  gst_buffer_set_caps (buf, caps);
  gst_caps_unref (caps);
  return buf;
}

static void
gst_nle_source_no_more_pads (GstElement * element, GstNleSource * nlesrc)
{
  /* If the input stream doesn't contain audio or it's a still picture we fill
   * the gap with a dummy audio buffer with silence */
  if (nlesrc->with_audio && !nlesrc->audio_linked) {
    GstBuffer *buf;
    GstNleSrcItem *item;
    guint64 duration;

    GST_INFO_OBJECT (nlesrc, "Pushing dummy audio buffer");

    nlesrc->audio_seek_done = TRUE;

    if (!nlesrc->audio_srcpad_added) {
      gst_pad_set_active (nlesrc->audio_srcpad, TRUE);
      gst_element_add_pad (GST_ELEMENT (nlesrc),
          gst_object_ref (nlesrc->audio_srcpad));
      nlesrc->audio_srcpad_added = TRUE;
    }
    item = (GstNleSrcItem *) g_list_nth_data (nlesrc->queue, nlesrc->index);
    if (GST_CLOCK_TIME_IS_VALID (item->duration))
      duration = item->duration / item->rate;
    else
      duration = 60 * GST_MSECOND;

    /* Push the start buffer and last 2 ones and let audiorate fill the gap */
    buf =
        gst_nle_source_audio_silence_buf (nlesrc, item->start,
        20 * GST_MSECOND);
    gst_nle_source_push_buffer (nlesrc, buf, TRUE);

    buf = gst_nle_source_audio_silence_buf (nlesrc,
        item->start + duration - 40 * GST_MSECOND, 20 * GST_MSECOND);
    gst_nle_source_push_buffer (nlesrc, buf, TRUE);

    buf = gst_nle_source_audio_silence_buf (nlesrc,
        item->start + duration - 20 * GST_MSECOND, 20 * GST_MSECOND);
    gst_nle_source_push_buffer (nlesrc, buf, TRUE);
  }
}

static GstFlowReturn
gst_nle_source_on_preroll_buffer (GstAppSink * appsink, gpointer data)
{
  gst_buffer_unref (gst_app_sink_pull_preroll (appsink));
  return GST_FLOW_OK;
}

static GstFlowReturn
gst_nle_source_push_still_picture (GstNleSource * nlesrc, GstNleSrcItem * item,
    GstBuffer * buf)
{
  GstCaps *bcaps, *ncaps;
  guint64 buf_dur;
  gint i, n_bufs;
  GstFlowReturn ret = GST_FLOW_OK;

  buf_dur = GST_SECOND * nlesrc->fps_d / nlesrc->fps_n;
  n_bufs = item->duration / buf_dur;

  bcaps = gst_buffer_get_caps (buf);
  ncaps = gst_caps_make_writable (bcaps);
  gst_caps_set_simple (ncaps, "pixel-aspect-ratio", GST_TYPE_FRACTION,
      1, 1, NULL);
  gst_buffer_set_caps (buf, ncaps);
  gst_caps_unref (ncaps);

  nlesrc->video_seek_done = TRUE;
  for (i = 0; i < n_bufs; i++) {
    GstBuffer *new_buf;

    new_buf = gst_buffer_copy (buf);
    GST_BUFFER_TIMESTAMP (new_buf) = item->start + buf_dur * i;
    GST_BUFFER_DURATION (new_buf) = buf_dur;
    ret = gst_nle_source_push_buffer (nlesrc, new_buf, FALSE);
    if (ret <= GST_FLOW_UNEXPECTED) {
      break;
    }
  }

  gst_buffer_unref (buf);
  return ret;
}

static GstFlowReturn
gst_nle_source_on_video_buffer (GstAppSink * appsink, gpointer data)
{
  GstNleSrcItem *item;
  GstNleSource *nlesrc;
  GstBuffer *buf;
  GstFlowReturn ret;

  nlesrc = GST_NLE_SOURCE (data);
  item = (GstNleSrcItem *) g_list_nth_data (nlesrc->queue, nlesrc->index);

  buf = gst_app_sink_pull_buffer (appsink);

  if (item->still_picture) {
    ret = gst_nle_source_push_still_picture (nlesrc, item, buf);
  } else {
    ret = gst_nle_source_push_buffer (nlesrc, buf, FALSE);
  }

  return ret;
}

static GstFlowReturn
gst_nle_source_on_audio_buffer (GstAppSink * appsink, gpointer data)
{
  GstNleSource *nlesrc = GST_NLE_SOURCE (data);

  return gst_nle_source_push_buffer (nlesrc,
      gst_app_sink_pull_buffer (appsink), TRUE);
}

static void
gst_nle_source_check_eos (GstNleSource * nlesrc)
{
  g_mutex_lock (&nlesrc->stream_lock);
  if (nlesrc->video_eos && nlesrc->audio_eos) {
    nlesrc->audio_eos = FALSE;
    nlesrc->video_eos = FALSE;
    nlesrc->cached_duration = 0;
    GST_DEBUG_OBJECT (nlesrc, "All pads are EOS");
    gst_nle_source_next_threaded (nlesrc);
  }
  g_mutex_unlock (&nlesrc->stream_lock);
}

static void
gst_nle_source_on_video_eos (GstAppSink * appsink, gpointer data)
{
  GstNleSource *nlesrc = GST_NLE_SOURCE (data);

  GST_DEBUG_OBJECT (nlesrc, "Video pad is EOS");
  nlesrc->video_eos = TRUE;
  gst_nle_source_check_eos (nlesrc);
}

static void
gst_nle_source_on_audio_eos (GstAppSink * appsink, gpointer data)
{
  GstNleSource *nlesrc = GST_NLE_SOURCE (data);

  GST_DEBUG_OBJECT (nlesrc, "Audio pad is EOS");
  nlesrc->audio_eos = TRUE;
  gst_nle_source_check_eos (nlesrc);
}

static gboolean
gst_nle_source_video_pad_probe_cb (GstPad * pad, GstEvent * event,
    GstNleSource * nlesrc)
{
  if (event->type == GST_EVENT_NEWSEGMENT) {
    g_mutex_lock (&nlesrc->stream_lock);
    if (!nlesrc->video_seek_done && nlesrc->seek_done) {
      GST_DEBUG_OBJECT (nlesrc, "NEWSEGMENT on the video pad");
      nlesrc->video_seek_done = TRUE;
    }
    g_mutex_unlock (&nlesrc->stream_lock);
  }

  return TRUE;
}

static gboolean
gst_nle_source_audio_pad_probe_cb (GstPad * pad, GstEvent * event,
    GstNleSource * nlesrc)
{
  if (event->type == GST_EVENT_NEWSEGMENT) {
    g_mutex_lock (&nlesrc->stream_lock);
    if (!nlesrc->audio_seek_done && nlesrc->seek_done) {
      GST_DEBUG_OBJECT (nlesrc, "NEWSEGMENT on the audio pad");
      nlesrc->audio_seek_done = TRUE;
    }
    g_mutex_unlock (&nlesrc->stream_lock);
  }
  return TRUE;
}

static void
gst_nle_source_pad_added_cb (GstElement * element, GstPad * pad,
    GstNleSource * nlesrc)
{
  GstCaps *caps;
  const GstStructure *s;
  const gchar *mime;
  GstElement *appsink = NULL;
  GstPad *sink_pad;
  GstAppSinkCallbacks appsink_cbs;
  GstNleSrcItem *item;

  item = (GstNleSrcItem *) g_list_nth_data (nlesrc->queue, nlesrc->index);

  caps = gst_pad_get_caps_reffed (pad);
  s = gst_caps_get_structure (caps, 0);
  mime = gst_structure_get_name (s);
  GST_DEBUG_OBJECT (nlesrc, "Found mime type: %s", mime);

  if (g_strrstr (mime, "video") && !nlesrc->video_linked) {
    appsink = gst_element_factory_make ("appsink", NULL);
    memset (&appsink_cbs, 0, sizeof (appsink_cbs));
    appsink_cbs.eos = gst_nle_source_on_video_eos;
    appsink_cbs.new_preroll = gst_nle_source_on_preroll_buffer;
    appsink_cbs.new_buffer = gst_nle_source_on_video_buffer;
    nlesrc->video_linked = TRUE;
    if (!nlesrc->video_srcpad_added) {
      gst_pad_set_active (nlesrc->video_srcpad, TRUE);
      gst_element_add_pad (GST_ELEMENT (nlesrc),
          gst_object_ref (nlesrc->video_srcpad));
      nlesrc->video_srcpad_added = TRUE;
    }
    gst_pad_add_event_probe (GST_BASE_SINK_PAD (GST_BASE_SINK (appsink)),
        (GCallback) gst_nle_source_video_pad_probe_cb, nlesrc);
    nlesrc->video_eos = FALSE;
  } else if (g_strrstr (mime, "audio") && nlesrc->with_audio
      && !nlesrc->audio_linked && (item ? item->rate == 1.0 : TRUE)) {
    appsink = gst_element_factory_make ("appsink", NULL);
    memset (&appsink_cbs, 0, sizeof (appsink_cbs));
    appsink_cbs.eos = gst_nle_source_on_audio_eos;
    appsink_cbs.new_preroll = gst_nle_source_on_preroll_buffer;
    appsink_cbs.new_buffer = gst_nle_source_on_audio_buffer;
    nlesrc->audio_linked = TRUE;
    if (!nlesrc->audio_srcpad_added) {
      gst_pad_set_active (nlesrc->audio_srcpad, TRUE);
      gst_element_add_pad (GST_ELEMENT (nlesrc),
          gst_object_ref (nlesrc->audio_srcpad));
      nlesrc->audio_srcpad_added = TRUE;
    }
    gst_pad_add_event_probe (GST_BASE_SINK_PAD (GST_BASE_SINK (appsink)),
        (GCallback) gst_nle_source_audio_pad_probe_cb, nlesrc);
    nlesrc->audio_eos = FALSE;
  }
  if (appsink != NULL) {
    g_object_set (appsink, "sync", FALSE, NULL);
    gst_app_sink_set_callbacks (GST_APP_SINK (appsink), &appsink_cbs, nlesrc,
        NULL);
    gst_bin_add (GST_BIN (nlesrc->decoder), appsink);
    sink_pad = gst_element_get_static_pad (appsink, "sink");
    gst_pad_link (pad, sink_pad);
    gst_element_sync_state_with_parent (appsink);
    gst_object_unref (sink_pad);
  }
}

static void
gst_nle_source_push_eos (GstNleSource * nlesrc)
{
  GST_INFO_OBJECT (nlesrc, "All items rendered, pushing eos");

  /* push on both sink pads of our A/V prep bins */
  gst_pad_send_event (nlesrc->video_sinkpad, gst_event_new_eos ());
  gst_pad_send_event (nlesrc->audio_sinkpad, gst_event_new_eos ());
}

static void
gst_nle_source_next_threaded (GstNleSource * nlesrc)
{
  g_thread_new ("next", (GThreadFunc) gst_nle_source_next, nlesrc);
}

static void
gst_nle_source_on_source_setup (GstElement * uridecodebin, GstElement * source,
    GstNleSource * nlesrc)
{
  if (nlesrc->source != NULL) {
    gst_object_unref (nlesrc->source);
    nlesrc->source = NULL;
  }
  nlesrc->source = g_object_ref (source);
}

static void
gst_nle_source_next (GstNleSource * nlesrc)
{
  GstNleSrcItem *item;
  GstStateChangeReturn ret;
  GstElement *uridecodebin;
  GstBus *bus;
  GstState state;

  nlesrc->index++;

  if (nlesrc->index >= g_list_length (nlesrc->queue)) {
    gst_nle_source_push_eos (nlesrc);
    return;
  }

  if (nlesrc->source != NULL) {
    gst_object_unref (nlesrc->source);
    nlesrc->source = NULL;
  }

  if (nlesrc->decoder != NULL) {
    gst_element_set_state (GST_ELEMENT (nlesrc->decoder), GST_STATE_NULL);
    gst_element_get_state (GST_ELEMENT (nlesrc->decoder), NULL, NULL, 0);
    gst_object_unref (nlesrc->decoder);
  }

  nlesrc->decoder = gst_pipeline_new ("decoder");
  uridecodebin = gst_element_factory_make ("uridecodebin", NULL);
  /* Connect signal to recover source element for queries in bytes */
  g_signal_connect (uridecodebin, "source-setup",
      G_CALLBACK (gst_nle_source_on_source_setup), nlesrc); 

  gst_bin_add (GST_BIN (nlesrc->decoder), uridecodebin);

  g_signal_connect (uridecodebin, "autoplug-select",
      G_CALLBACK (lgm_filter_video_decoders), nlesrc);
  g_signal_connect (uridecodebin, "pad-added",
      G_CALLBACK (gst_nle_source_pad_added_cb), nlesrc);
  g_signal_connect (uridecodebin, "no-more-pads",
      G_CALLBACK (gst_nle_source_no_more_pads), nlesrc);

  bus = GST_ELEMENT_BUS (nlesrc->decoder);
  gst_bus_add_signal_watch (bus);
  g_signal_connect (bus, "message", G_CALLBACK (gst_nle_source_bus_message),
      nlesrc);
  item = (GstNleSrcItem *) g_list_nth_data (nlesrc->queue, nlesrc->index);

  GST_INFO_OBJECT (nlesrc, "Starting next item with uri:%s", item->file_path);
  GST_INFO_OBJECT (nlesrc, "start:%" GST_TIME_FORMAT " stop:%"
      GST_TIME_FORMAT " rate:%f", GST_TIME_ARGS (item->start),
      GST_TIME_ARGS (item->stop), item->rate);

  g_object_set (uridecodebin, "uri", item->file_path, NULL);

  nlesrc->seek_done = FALSE;
  if (GST_CLOCK_TIME_IS_VALID (item->stop)) {
    nlesrc->video_seek_done = FALSE;
    nlesrc->audio_seek_done = FALSE;
  } else {
    nlesrc->video_seek_done = TRUE;
    nlesrc->audio_seek_done = TRUE;
  }
  nlesrc->audio_eos = TRUE;
  nlesrc->video_eos = TRUE;
  nlesrc->audio_ts = 0;
  nlesrc->video_ts = 0;
  nlesrc->start_ts = nlesrc->accu_time;
  nlesrc->video_linked = FALSE;
  nlesrc->audio_linked = FALSE;
  nlesrc->item_setup = FALSE;
  nlesrc->cached_duration = 0;

  GST_DEBUG_OBJECT (nlesrc, "Start ts:%" GST_TIME_FORMAT,
      GST_TIME_ARGS (nlesrc->start_ts));
  gst_element_set_state (nlesrc->decoder, GST_STATE_PLAYING);
  ret = gst_element_get_state (nlesrc->decoder, &state, NULL, 5 * GST_SECOND);
  if (ret == GST_STATE_CHANGE_FAILURE) {
    GST_WARNING_OBJECT (nlesrc, "Error changing state, selecting next item.");
    gst_nle_source_check_eos (nlesrc);
    return;
  }

  nlesrc->seek_done = TRUE;
  if (!item->still_picture && GST_CLOCK_TIME_IS_VALID (item->stop)) {
    GST_DEBUG_OBJECT (nlesrc, "Sending seek event");
    gst_element_seek (nlesrc->decoder, 1, GST_FORMAT_TIME,
        GST_SEEK_FLAG_ACCURATE,
        GST_SEEK_TYPE_SET, item->start, GST_SEEK_TYPE_SET, item->stop);
  }
}

static GstStateChangeReturn
gst_nle_source_change_state (GstElement * element, GstStateChange transition)
{
  GstNleSource *nlesrc;
  GstStateChangeReturn res;

  nlesrc = GST_NLE_SOURCE (element);

  switch (transition) {
    case GST_STATE_CHANGE_NULL_TO_READY:
      /* We do our setup (adding internal elements) here */
      gst_nle_source_setup (nlesrc);
      break;
    default:
      break;
  }

  res = GST_ELEMENT_CLASS (parent_class)->change_state (element, transition);
  if (res == GST_STATE_CHANGE_FAILURE)
    return res;

  switch (transition) {
    case GST_STATE_CHANGE_READY_TO_PAUSED:
      /* Process next source when children elements have successfully changed state */
      gst_nle_source_next (nlesrc);
      break;
    case GST_STATE_CHANGE_PLAYING_TO_PAUSED:
      if (nlesrc->decoder) {
        gst_element_set_state (nlesrc->decoder, GST_STATE_PAUSED);
      }
      break;
    case GST_STATE_CHANGE_PAUSED_TO_READY:
      if (nlesrc->decoder) {
        gst_element_set_state (nlesrc->decoder, GST_STATE_READY);
      }
      if (nlesrc->queue != NULL) {
        g_list_free_full (nlesrc->queue,
            (GDestroyNotify) gst_nle_source_item_free);
        nlesrc->queue = NULL;
      }
      break;
    case GST_STATE_CHANGE_READY_TO_NULL:
      if (nlesrc->source) {
        gst_object_unref (nlesrc->source);
        nlesrc->source = NULL;
      }
      if (nlesrc->decoder) {
        gst_element_set_state (nlesrc->decoder, GST_STATE_NULL);
        gst_object_unref (nlesrc->decoder);
        nlesrc->decoder = NULL;
      }
      break;
    default:
      break;
  }

  return res;
}

void
gst_nle_source_query_progress (GstNleSource * nlesrc, gfloat * progress)
{
  gint64 position = 0, duration = 0;
  gfloat tmp = 0.0;
  guint num_items = g_list_length (nlesrc->queue);

  *progress = (gfloat) nlesrc->index / (gfloat) num_items;

  if (nlesrc->source) {
    GstFormat format = GST_FORMAT_BYTES;
    if (nlesrc->cached_duration > 0)
      duration = nlesrc->cached_duration;
    else {
      gst_element_query_duration (nlesrc->source, &format, &duration);
      nlesrc->cached_duration = duration;
    }
    if (duration > 0) {
      gst_element_query_position (nlesrc->source, &format, &position);
      tmp = (gfloat) position / (gfloat) duration;
      tmp *= 1.0 / (gfloat) num_items;
    }
  }

  *progress += tmp;

  GST_LOG ("progress(%d): %" G_GINT64_FORMAT "/%" G_GINT64_FORMAT "(%g%%)",
      nlesrc->index, position, duration, *progress * 100);
}

void
gst_nle_source_add_item (GstNleSource * nlesrc, const gchar * file_path,
    const gchar * title, guint64 start, guint64 stop, gfloat rate,
    gboolean still_picture, GstNleRectangle roi)
{
  GstNleSrcItem *item;
  gchar *uri;

  uri = lgm_filename_to_uri (file_path);
  item = gst_nle_source_item_new (uri, title, start, stop, rate, still_picture,
      roi);
  g_free (uri);
  nlesrc->queue = g_list_append (nlesrc->queue, item);

  GST_INFO_OBJECT (nlesrc, "Added new item to the queue start:%"
      GST_TIME_FORMAT " stop:%" GST_TIME_FORMAT " rate:%f",
      GST_TIME_ARGS (start), GST_TIME_ARGS (stop), rate);
}

void
gst_nle_source_configure (GstNleSource * nlesrc, guint width, guint height,
    guint fps_n, guint fps_d, gboolean overlay_title, gboolean with_audio)
{
  nlesrc->width = width;
  nlesrc->height = height;
  nlesrc->fps_n = fps_n;
  nlesrc->fps_d = fps_d;
  nlesrc->overlay_title = overlay_title;
  nlesrc->with_audio = with_audio;
  nlesrc->title_size = 15;

  GST_INFO_OBJECT (nlesrc, "Configuring source with %dx%d@%d/%dfps t:%d a:%d",
      width, height, fps_n, fps_d, overlay_title, with_audio);
}

GstNleSource *
gst_nle_source_new (void)
{
  GstNleSource *nlesrc;

  nlesrc = g_object_new (GST_TYPE_NLE_SOURCE, NULL);
  return nlesrc;
}
