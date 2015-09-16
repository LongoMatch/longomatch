/* 
 * Copyright (C) 2014  Andoni Morales Alastruey <ylatuya@gmail.com>
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
 */

#include "lgm-video-player.h"
#include "baconvideowidget-marshal.h"
#include "gstscreenshot.h"

#define LGM_PLAY_TIMEOUT 20
#define LGM_PAUSE_TIMEOUT 100

#define is_error(e, d, c) \
  (e->domain == GST_##d##_ERROR && \
   e->code == GST_##d##_ERROR_##c)

GST_DEBUG_CATEGORY (_lgm_debug_cat);
#define GST_CAT_DEFAULT _lgm_debug_cat

G_DEFINE_TYPE (LgmVideoPlayer, lgm_video_player, GST_TYPE_ELEMENT)

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_TICK,
  SIGNAL_STATE_CHANGE,
  SIGNAL_READY_TO_SEEK,
  LAST_SIGNAL
};

typedef enum
{
  GST_PLAY_FLAG_VIDEO = (1 << 0),
  GST_PLAY_FLAG_AUDIO = (1 << 1),
  GST_PLAY_FLAG_TEXT = (1 << 2),
  GST_PLAY_FLAG_VIS = (1 << 3),
  GST_PLAY_FLAG_SOFT_VOLUME = (1 << 4),
  GST_PLAY_FLAG_NATIVE_AUDIO = (1 << 5),
  GST_PLAY_FLAG_NATIVE_VIDEO = (1 << 6),
  GST_PLAY_FLAG_DOWNLOAD = (1 << 7),
  GST_PLAY_FLAG_BUFFERING = (1 << 8),
  GST_PLAY_FLAG_DEINTERLACE = (1 << 9),
  GST_PLAY_FLAG_SOFT_COLORBALANCE = (1 << 10)
} GstPlayFlags;


struct LgmVideoPlayerPrivate
{
  gchar *uri;
  LgmUseType use_type;

  GstElement *play;
  GstElement *video_sink;
  GstXOverlay *xoverlay;
  guintptr window_handle;
  gboolean window_set;
  GMutex overlay_lock;

  guint update_id;

  gint64 stream_length;
  gint64 current_time;
  gdouble current_position;
  gdouble rate;

  GstBus *bus;
  gulong sig_bus_async;
  gulong sig_bus_sync;

  gint video_fps_d;
  gint video_fps_n;

  GstState target_state;
};

static void lgm_video_player_finalize (GObject * object);
static gboolean lgm_query_timeout (LgmVideoPlayer * lvp);

static GError *lgm_error_from_gst_error (LgmVideoPlayer * lvp, GstMessage * m);

static GstElementClass *parent_class = NULL;

static int lgm_signals[LAST_SIGNAL] = { 0 };

static void
lgm_error_msg (LgmVideoPlayer * lvp, GstMessage * msg)
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
    GST_ERROR ("uri     = %s", GST_STR_NULL (lvp->priv->uri));

    g_message ("Error: %s\n%s\n", GST_STR_NULL (err->message),
        GST_STR_NULL (dbg));

    g_error_free (err);
  }
  g_free (dbg);
}

static void
lgm_reconfigure_tick_timeout (LgmVideoPlayer * lvp, guint msecs)
{
  /* Tick timeouts are not used anymore */
  return;

  if (lvp->priv->update_id != 0) {
    g_source_remove (lvp->priv->update_id);
    lvp->priv->update_id = 0;
  }
  if (msecs > 0) {
    lvp->priv->update_id =
        g_timeout_add (msecs, (GSourceFunc) lgm_query_timeout, lvp);
  }
}

static void
lgm_element_msg_sync_cb (GstBus * bus, GstMessage * msg, gpointer data)
{
  LgmVideoPlayer *lvp = LGM_VIDEO_PLAYER (data);

  g_assert (msg->type == GST_MESSAGE_ELEMENT);
  if (msg->structure == NULL)
    return;

  if (gst_structure_has_name (msg->structure, "prepare-xwindow-id")) {
    GstObject *sender = GST_MESSAGE_SRC (msg);

    if (sender && GST_IS_X_OVERLAY (sender)) {
      g_mutex_lock (&lvp->priv->overlay_lock);
      if (lvp->priv->xoverlay != NULL) {
        gst_object_unref (lvp->priv->xoverlay);
      }
      lvp->priv->xoverlay =
          (GstXOverlay *) gst_object_ref (GST_X_OVERLAY (sender));
      lgm_set_window_handle (lvp->priv->xoverlay, lvp->priv->window_handle);
      lvp->priv->window_set = TRUE;
      g_mutex_unlock (&lvp->priv->overlay_lock);
    }
  }
}

static void
lgm_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  LgmVideoPlayer *lvp = (LgmVideoPlayer *) data;
  GstMessageType msg_type;

  g_return_if_fail (lvp != NULL);
  g_return_if_fail (LGM_IS_VIDEO_WIDGET (lvp));

  msg_type = GST_MESSAGE_TYPE (message);

  switch (msg_type) {
    case GST_MESSAGE_ERROR:
    {
      GError *error;
      lgm_error_msg (lvp, message);

      error = lgm_error_from_gst_error (lvp, message);

      lvp->priv->target_state = GST_STATE_NULL;
      if (lvp->priv->play)
        gst_element_set_state (lvp->priv->play, GST_STATE_NULL);

      g_signal_emit (lvp, lgm_signals[SIGNAL_ERROR], 0,
          error->message, TRUE, FALSE);
      g_error_free (error);
      break;
    }
    case GST_MESSAGE_WARNING:
    {
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;
    }
    case GST_MESSAGE_EOS:
      GST_DEBUG ("EOS message");
      lgm_query_timeout (lvp);
      g_signal_emit (lvp, lgm_signals[SIGNAL_EOS], 0, FALSE);
      break;
    case GST_MESSAGE_STATE_CHANGED:
    {
      GstState old_state, new_state;
      gchar *src_name;

      gst_message_parse_state_changed (message, &old_state, &new_state, NULL);

      if (old_state == new_state)
        break;

      if (GST_MESSAGE_SRC (message) != GST_OBJECT (lvp->priv->play))
        break;

      src_name = gst_object_get_name (message->src);
      g_free (src_name);

      if (new_state <= GST_STATE_PAUSED) {
        lgm_query_timeout (lvp);
        lgm_reconfigure_tick_timeout (lvp, 0);
        g_signal_emit (lvp, lgm_signals[SIGNAL_STATE_CHANGE], 0, FALSE);
      } else if (new_state == GST_STATE_PAUSED) {
        lgm_reconfigure_tick_timeout (lvp, LGM_PAUSE_TIMEOUT);
        g_signal_emit (lvp, lgm_signals[SIGNAL_STATE_CHANGE], 0, FALSE);
      } else if (new_state > GST_STATE_PAUSED) {
        lgm_reconfigure_tick_timeout (lvp, LGM_PLAY_TIMEOUT);
        g_signal_emit (lvp, lgm_signals[SIGNAL_STATE_CHANGE], 0, TRUE);
      }
      if (old_state == GST_STATE_READY && new_state == GST_STATE_PAUSED) {
        lvp->priv->stream_length = 0;
        g_signal_emit (lvp, lgm_signals[SIGNAL_READY_TO_SEEK], 0, FALSE);
      }
      break;
    }

    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
}

static void
got_time_tick (GstElement * play, gint64 time_nanos, LgmVideoPlayer * lvp)
{
  g_return_if_fail (lvp != NULL);
  g_return_if_fail (LGM_IS_VIDEO_WIDGET (lvp));

  lvp->priv->current_time = time_nanos;

  if (lvp->priv->stream_length == 0) {
    lvp->priv->current_position = 0;
  } else {
    lvp->priv->current_position =
        (gdouble) lvp->priv->current_time / lvp->priv->stream_length;
  }

  g_signal_emit (lvp, lgm_signals[SIGNAL_TICK], 0,
      lvp->priv->current_time, lvp->priv->stream_length,
      lvp->priv->current_position);
}

static gboolean
lgm_query_timeout (LgmVideoPlayer * lvp)
{
  GstFormat fmt = GST_FORMAT_TIME;
  gint64 pos = -1, len = -1;

  if (gst_element_query_duration (lvp->priv->play, &fmt, &len)) {
    if (len != -1 && fmt == GST_FORMAT_TIME) {
      lvp->priv->stream_length = len;
    }
  } else {
    GST_INFO ("could not get duration");
  }

  if (gst_element_query_position (lvp->priv->video_sink, &fmt, &pos)) {
    if (pos != -1 && fmt == GST_FORMAT_TIME) {
      got_time_tick (GST_ELEMENT (lvp->priv->play), pos, lvp);
    }
  } else {
    GST_INFO ("could not get position");
  }

  return TRUE;
}

static void
lgm_parse_stream_caps (GstPad * pad, GstPad * peer, LgmVideoPlayer * lvp)
{
  GstCaps *caps;
  GstStructure *s;

  caps = gst_pad_get_negotiated_caps (pad);
  if (caps == NULL || gst_caps_is_empty (caps)) {
    return;
  }

  s = gst_caps_get_structure (caps, 0);
  if (gst_structure_has_field (s, "framerate")) {
    gst_structure_get_fraction (s, "framerate",
        &lvp->priv->video_fps_n, &lvp->priv->video_fps_d);
  }
}

/* ============================================================= */
/*                                                               */
/*                       Public Methods                          */
/*                                                               */
/* ============================================================= */


/* =========================================== */
/*                                             */
/*               Play/Pause, Stop              */
/*                                             */
/* =========================================== */

static GError *
lgm_error_from_gst_error (LgmVideoPlayer * lvp, GstMessage * err_msg)
{
  const gchar *src_typename;
  GError *ret = NULL;
  GError *e = NULL;

  GST_LOG ("resolving error message %" GST_PTR_FORMAT, err_msg);

  src_typename = (err_msg->src) ? G_OBJECT_TYPE_NAME (err_msg->src) : NULL;

  gst_message_parse_error (err_msg, &e, NULL);

  if (is_error (e, RESOURCE, NOT_FOUND) || is_error (e, RESOURCE, OPEN_READ)) {
    if (e->code == GST_RESOURCE_ERROR_NOT_FOUND) {
      ret = g_error_new_literal (LGM_ERROR, GST_ERROR_FILE_NOT_FOUND,
          _("Location not found."));
    } else {
      ret = g_error_new_literal (LGM_ERROR, GST_ERROR_FILE_PERMISSION,
          _("Could not open location; "
              "you might not have permission to open the file."));
    }
  } else if (e->domain == GST_RESOURCE_ERROR) {
    ret = g_error_new_literal (LGM_ERROR, GST_ERROR_FILE_GENERIC, e->message);
  } else if (is_error (e, CORE, MISSING_PLUGIN) ||
      is_error (e, STREAM, CODEC_NOT_FOUND)) {
    gchar *msg =
        "The playback of this movie requires a plugin which is not installed.";
    ret = g_error_new_literal (LGM_ERROR, GST_ERROR_CODEC_NOT_HANDLED, msg);
  } else if (is_error (e, STREAM, WRONG_TYPE) ||
      is_error (e, STREAM, NOT_IMPLEMENTED)) {
    if (src_typename) {
      ret = g_error_new (LGM_ERROR, GST_ERROR_CODEC_NOT_HANDLED, "%s: %s",
          src_typename, e->message);
    } else {
      ret = g_error_new_literal (LGM_ERROR, GST_ERROR_CODEC_NOT_HANDLED,
          e->message);
    }
  } else {
    /* generic error, no code; take message */
    ret = g_error_new_literal (LGM_ERROR, GST_ERROR_GENERIC, e->message);
  }
  g_error_free (e);

  return ret;
}


static gboolean
poll_for_state_change_full (LgmVideoPlayer * lvp, GstElement * element,
    GstState state, GstMessage ** err_msg, gint64 timeout)
{
  GstBus *bus;
  GstMessageType events;

  bus = gst_element_get_bus (element);
  events =
      (GstMessageType) (GST_MESSAGE_STATE_CHANGED | GST_MESSAGE_ERROR |
      GST_MESSAGE_EOS);

  while (TRUE) {
    GstMessage *message;
    GstElement *src;

    message = gst_bus_timed_pop_filtered (bus, timeout, events);

    if (!message)
      goto timed_out;

    src = (GstElement *) GST_MESSAGE_SRC (message);

    switch (GST_MESSAGE_TYPE (message)) {
      case GST_MESSAGE_STATE_CHANGED:
      {
        GstState old, new_state, pending;

        if (src == element) {
          gst_message_parse_state_changed (message, &old, &new_state, &pending);
          if (new_state == state) {
            gst_message_unref (message);
            goto success;
          }
        }
        break;
      }
      case GST_MESSAGE_ERROR:
      {
        lgm_error_msg (lvp, message);
        *err_msg = message;
        message = NULL;
        goto error;
        break;
      }
      case GST_MESSAGE_EOS:
      {
        GError *e = NULL;

        gst_message_unref (message);
        e = g_error_new_literal (LGM_ERROR, GST_ERROR_FILE_GENERIC,
            _("Media file could not be played."));
        *err_msg =
            gst_message_new_error (GST_OBJECT (lvp->priv->play), e, NULL);
        g_error_free (e);
        goto error;
        break;
      }
      default:
        g_assert_not_reached ();
        break;
    }

    gst_message_unref (message);
  }

  g_assert_not_reached ();

success:
  GST_DEBUG ("state change to %s succeeded",
      gst_element_state_get_name (state));
  return TRUE;

timed_out:
  GST_DEBUG ("state change to %s timed out, returning success and handling "
      "errors asynchronously", gst_element_state_get_name (state));
  return TRUE;

error:
  GST_DEBUG ("error while waiting for state change to %s: %" GST_PTR_FORMAT,
      gst_element_state_get_name (state), *err_msg);
  return FALSE;
}

gboolean
lgm_video_player_open (LgmVideoPlayer * lvp, const gchar * uri, GError ** error)
{
  GstMessage *err_msg = NULL;
  gboolean ret;

  g_return_val_if_fail (lvp != NULL, FALSE);
  g_return_val_if_fail (uri != NULL, FALSE);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), FALSE);
  g_return_val_if_fail (lvp->priv->play != NULL, FALSE);

  /* So we aren't closed yet... */
  if (lvp->priv->uri) {
    lgm_video_player_close (lvp);
  }

  GST_DEBUG ("uri = %s", GST_STR_NULL (uri));

  lvp->priv->uri = lgm_filename_to_uri (uri);
  g_object_set (lvp->priv->play, "uri", lvp->priv->uri, NULL);

  lvp->priv->stream_length = 0;
  lvp->priv->rate = 1.0;
  lvp->priv->target_state = GST_STATE_PAUSED;

  gst_element_set_state (lvp->priv->play, GST_STATE_PAUSED);

  if (lvp->priv->use_type == LGM_USE_TYPE_VIDEO) {
    ret = TRUE;
  } else {
    /* used as thumbnailer, wait for state change to finish. */
    GST_INFO ("waiting for state changed to PAUSED to complete");
    ret = poll_for_state_change_full (lvp, lvp->priv->play,
        GST_STATE_PAUSED, &err_msg, 5 * GST_SECOND);
    lgm_video_player_get_stream_length (lvp);
    GST_INFO ("stream length = %u", lvp->priv->stream_length);
  }

  if (!ret) {
    GST_INFO ("Error on open: %" GST_PTR_FORMAT, err_msg);
    lgm_video_player_close (lvp);
    g_free (lvp->priv->uri);
    lvp->priv->uri = NULL;
  }

  if (err_msg != NULL) {
    if (error) {
      *error = lgm_error_from_gst_error (lvp, err_msg);
    } else {
      GST_WARNING ("Got error, but caller is not collecting error details!");
    }
    gst_message_unref (err_msg);
  }
  return ret;
}

gboolean
lgm_video_player_play (LgmVideoPlayer * lvp, gboolean synchronous)
{

  GstState cur_state;

  g_return_val_if_fail (lvp != NULL, FALSE);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (lvp->priv->play), FALSE);
  g_return_val_if_fail (lvp->priv->uri != NULL, FALSE);

  lvp->priv->target_state = GST_STATE_PLAYING;

  if (lvp->priv->use_type == LGM_USE_TYPE_CAPTURE) {
    return TRUE;
  }

  gst_element_get_state (lvp->priv->play, &cur_state, NULL, 0);
  gst_element_set_state (lvp->priv->play, GST_STATE_PLAYING);
  if (synchronous) {
    gst_element_get_state (lvp->priv->play, NULL, NULL, 5 * GST_SECOND);
  }

  return TRUE;
}

gboolean
lgm_video_player_seek_time (LgmVideoPlayer * lvp, gint64 time,
    gboolean accurate, gboolean synchronous)
{
  guint32 flags;

  g_return_val_if_fail (lvp != NULL, FALSE);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (lvp->priv->play), FALSE);

  GST_DEBUG ("Seeking to %" GST_TIME_FORMAT, GST_TIME_ARGS (time));

  flags = GST_SEEK_FLAG_FLUSH;
  if (accurate) {
    flags |= GST_SEEK_FLAG_ACCURATE;
  } else {
    flags |= GST_SEEK_FLAG_KEY_UNIT;
  }

  gst_element_seek (lvp->priv->play, lvp->priv->rate,
      GST_FORMAT_TIME, flags, GST_SEEK_TYPE_SET, time,
      GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
  if (synchronous) {
    gst_element_get_state (lvp->priv->play, NULL, NULL, 5 * GST_SECOND);
  }
  got_time_tick (lvp->priv->play, time, lvp);
  return TRUE;
}

gboolean
lgm_video_player_set_rate (LgmVideoPlayer * lvp, gdouble rate)
{
  guint64 pos;

  g_return_val_if_fail (lvp != NULL, FALSE);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (lvp->priv->play), FALSE);

  pos = lgm_video_player_get_current_time (lvp);
  if (pos == 0)
    return FALSE;

  GST_DEBUG ("Setting rate to %f", rate);
  lvp->priv->rate = rate;
  lgm_video_player_seek_time (lvp, pos, TRUE, FALSE);

  return TRUE;
}

gboolean
lgm_video_player_seek_to_next_frame (LgmVideoPlayer * lvp)
{
  gint64 pos = -1;
  gboolean ret;

  g_return_val_if_fail (lvp != NULL, FALSE);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (lvp->priv->play), FALSE);

  GST_DEBUG ("Seeking to next frame");

  lgm_video_player_pause (lvp, FALSE);
  pos = lgm_video_player_get_current_time (lvp);
  if (pos == 0)
    return FALSE;

  gst_element_send_event (lvp->priv->video_sink,
      gst_event_new_step (GST_FORMAT_BUFFERS, 1, 1.0, TRUE, FALSE));

  pos = lgm_video_player_get_current_time (lvp);
  got_time_tick (GST_ELEMENT (lvp->priv->play), pos, lvp);
  lgm_video_player_expose (lvp);

  return ret;
}

gboolean
lgm_video_player_seek_to_previous_frame (LgmVideoPlayer * lvp)
{
  gint fps;
  gint64 pos;
  gint64 final_pos;
  gboolean ret;

  g_return_val_if_fail (lvp != NULL, FALSE);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (lvp->priv->play), FALSE);

  GST_DEBUG ("Seeking to previous frame");
  //Round framerate to the nearest integer
  fps = (lvp->priv->video_fps_n + lvp->priv->video_fps_d / 2) /
      lvp->priv->video_fps_d;
  pos = lgm_video_player_get_current_time (lvp);
  final_pos = pos - 1 * GST_SECOND / fps;

  if (pos == 0)
    return FALSE;

  if (lgm_video_player_is_playing (lvp))
    lgm_video_player_pause (lvp, FALSE);

  lgm_video_player_seek_time (lvp, final_pos, TRUE, FALSE);
  got_time_tick (GST_ELEMENT (lvp->priv->play), pos, lvp);
  lgm_video_player_expose (lvp);

  return ret;
}

static void
lgm_stop_play_pipeline (LgmVideoPlayer * lvp)
{
  GstState cur_state;

  gst_element_get_state (lvp->priv->play, &cur_state, NULL, 0);
  if (cur_state > GST_STATE_READY) {
    GstMessage *msg;
    GstBus *bus;

    GST_INFO ("stopping");
    gst_element_set_state (lvp->priv->play, GST_STATE_READY);

    /* process all remaining state-change messages so everything gets
     * cleaned up properly (before the state change to NULL flushes them) */
    GST_INFO ("processing pending state-change messages");
    bus = gst_element_get_bus (lvp->priv->play);
    while ((msg =
            gst_bus_timed_pop_filtered (bus, 0, GST_MESSAGE_STATE_CHANGED))) {
      gst_bus_async_signal_func (bus, msg, NULL);
      gst_message_unref (msg);
    }
    gst_object_unref (bus);
  }

  gst_element_set_state (lvp->priv->play, GST_STATE_NULL);
  lvp->priv->target_state = GST_STATE_NULL;
}

void
lgm_video_player_stop (LgmVideoPlayer * lvp, gboolean synchronous)
{
  g_return_if_fail (lvp != NULL);
  g_return_if_fail (LGM_IS_VIDEO_WIDGET (lvp));
  g_return_if_fail (GST_IS_ELEMENT (lvp->priv->play));

  gst_element_set_state (lvp->priv->play, GST_STATE_NULL);
  lvp->priv->target_state = GST_STATE_NULL;

  if (synchronous) {
    gst_element_get_state (lvp->priv->play, NULL, NULL, 5 * GST_SECOND);
  }
  got_time_tick (GST_ELEMENT (lvp->priv->play), 0, lvp);
}

void
lgm_video_player_close (LgmVideoPlayer * lvp)
{
  g_return_if_fail (lvp != NULL);
  g_return_if_fail (LGM_IS_VIDEO_WIDGET (lvp));
  g_return_if_fail (GST_IS_ELEMENT (lvp->priv->play));

  GST_LOG ("Closing");
  lgm_stop_play_pipeline (lvp);

  if (lvp->priv->uri != NULL) {
    g_free (lvp->priv->uri);
    lvp->priv->uri = NULL;
  }

  if (lvp->priv->xoverlay != NULL) {
    gst_object_unref (lvp->priv->xoverlay);
    lvp->priv->xoverlay = NULL;
  }

  got_time_tick (GST_ELEMENT (lvp->priv->play), 0, lvp);
}

void
lgm_video_player_pause (LgmVideoPlayer * lvp, gboolean synchronous)
{
  g_return_if_fail (lvp != NULL);
  g_return_if_fail (LGM_IS_VIDEO_WIDGET (lvp));
  g_return_if_fail (GST_IS_ELEMENT (lvp->priv->play));
  g_return_if_fail (lvp->priv->uri != NULL);

  gst_element_set_state (lvp->priv->play, GST_STATE_PAUSED);
  lvp->priv->target_state = GST_STATE_PAUSED;
  if (synchronous) {
    gst_element_get_state (lvp->priv->play, NULL, NULL, 5 * GST_SECOND);
  }
}

void
lgm_video_player_set_volume (LgmVideoPlayer * lvp, double volume)
{
  g_return_if_fail (LGM_IS_VIDEO_WIDGET (lvp));
  g_return_if_fail (GST_IS_ELEMENT (lvp->priv->play));

  volume = CLAMP (volume, 0.0, 1.0);
  g_object_set (lvp->priv->play, "volume", (gdouble) volume, NULL);
}

gdouble
lgm_video_player_get_volume (LgmVideoPlayer * lvp)
{
  gdouble vol;

  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), 0.0);
  g_return_val_if_fail (GST_IS_ELEMENT (lvp->priv->play), 0.0);

  g_object_get (G_OBJECT (lvp->priv->play), "volume", &vol, NULL);

  return vol;
}

gint64
lgm_video_player_get_current_time (LgmVideoPlayer * lvp)
{
  GstFormat fmt;
  gint64 pos;

  g_return_val_if_fail (lvp != NULL, -1);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), -1);

  fmt = GST_FORMAT_TIME;
  pos = -1;

  gst_element_query_position (lvp->priv->video_sink, &fmt, &pos);

  return pos != -1 ? pos : lvp->priv->current_time;
}

gint64
lgm_video_player_get_stream_length (LgmVideoPlayer * lvp)
{
  g_return_val_if_fail (lvp != NULL, -1);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), -1);

  if (lvp->priv->stream_length == 0 && lvp->priv->play != NULL) {
    GstFormat fmt = GST_FORMAT_TIME;
    gint64 len = -1;

    if (gst_element_query_duration (lvp->priv->play, &fmt, &len)
        && len != -1) {
      lvp->priv->stream_length = len;
    }
  }

  return lvp->priv->stream_length;
}

gboolean
lgm_video_player_is_playing (LgmVideoPlayer * lvp)
{
  gboolean ret;

  g_return_val_if_fail (lvp != NULL, FALSE);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (lvp->priv->play), FALSE);

  ret = (lvp->priv->target_state == GST_STATE_PLAYING);
  GST_LOG ("%splaying", (ret) ? "" : "not ");

  return ret;
}

void
lgm_video_player_expose (LgmVideoPlayer * lvp)
{
  g_return_if_fail (lvp != NULL);

  if (!lvp->priv->window_set) {
    return;
  }
  g_mutex_lock (&lvp->priv->overlay_lock);
  if (lvp->priv->xoverlay != NULL && GST_IS_X_OVERLAY (lvp->priv->xoverlay)) {
    gst_x_overlay_expose (lvp->priv->xoverlay);
  }
  g_mutex_unlock (&lvp->priv->overlay_lock);
}

static void
destroy_pixbuf (guchar * pix, gpointer data)
{
  gst_buffer_unref (GST_BUFFER (data));
}

void
lgm_video_player_unref_pixbuf (GdkPixbuf * pixbuf)
{
  g_object_unref (pixbuf);
}

GdkPixbuf *
lgm_video_player_get_current_frame (LgmVideoPlayer * lvp)
{
  GstStructure *s;
  GstBuffer *buf = NULL;
  GdkPixbuf *pixbuf;
  GstCaps *to_caps;
  gint outwidth = 0;
  gint outheight = 0;

  g_return_val_if_fail (lvp != NULL, NULL);
  g_return_val_if_fail (LGM_IS_VIDEO_WIDGET (lvp), NULL);
  g_return_val_if_fail (GST_IS_ELEMENT (lvp->priv->play), NULL);

  gst_element_get_state (lvp->priv->play, NULL, NULL, 1 * GST_SECOND);

  /* get frame */
  g_object_get (lvp->priv->play, "frame", &buf, NULL);

  if (!buf) {
    GST_DEBUG ("Could not take screenshot: %s", "no last video frame");
    return NULL;
  }

  if (GST_BUFFER_CAPS (buf) == NULL) {
    GST_DEBUG ("Could not take screenshot: %s", "no caps on buffer");
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

  GST_DEBUG ("frame caps: %" GST_PTR_FORMAT, GST_BUFFER_CAPS (buf));
  GST_DEBUG ("pixbuf caps: %" GST_PTR_FORMAT, to_caps);

  buf = bvw_frame_conv_convert (buf, to_caps);

  gst_caps_unref (to_caps);

  if (!buf) {
    GST_DEBUG ("Could not take screenshot: %s", "conversion failed");
    return NULL;
  }

  if (!GST_BUFFER_CAPS (buf)) {
    GST_DEBUG ("Could not take screenshot: %s", "no caps on output buffer");
    return NULL;
  }

  s = gst_caps_get_structure (GST_BUFFER_CAPS (buf), 0);
  gst_structure_get_int (s, "width", &outwidth);
  gst_structure_get_int (s, "height", &outheight);
  g_return_val_if_fail (outwidth > 0 && outheight > 0, NULL);

  /* create pixbuf from that - use our own destroy function */
  pixbuf = gdk_pixbuf_new_from_data (GST_BUFFER_DATA (buf),
      GDK_COLORSPACE_RGB, FALSE, 8, outwidth,
      outheight, GST_ROUND_UP_4 (outwidth * 3), destroy_pixbuf, buf);

  if (!pixbuf) {
    GST_DEBUG ("Could not take screenshot: %s", "could not create pixbuf");
    gst_buffer_unref (buf);
  }

  return pixbuf;
}

void
lgm_video_player_set_window_handle (LgmVideoPlayer * lvp,
    guintptr window_handle)
{
  g_mutex_lock (&lvp->priv->overlay_lock);
  lvp->priv->window_handle = window_handle;
  if (lvp->priv->xoverlay != NULL) {
    lgm_set_window_handle (lvp->priv->xoverlay, lvp->priv->window_handle);
    lvp->priv->window_set = TRUE;
  }
  g_mutex_unlock (&lvp->priv->overlay_lock);
}

LgmVideoPlayer *
lgm_video_player_new (LgmUseType type, GError ** err)
{
  LgmVideoPlayer *lvp;
  GstElement *video_sink, *audio_sink;
  GstPad *pad;
  gint flags;

  lvp = (LgmVideoPlayer *) g_object_new (lgm_video_player_get_type (), NULL);

  lvp->priv->use_type = type;
  GST_INFO ("use_type = %d", type);

  lvp->priv->play = gst_element_factory_make ("playbin2", "play");
  if (!lvp->priv->play) {
    g_set_error (err, LGM_ERROR, GST_ERROR_PLUGIN_LOAD,
        _("Failed to create a GStreamer play object. "
            "Please check your GStreamer installation."));
    g_object_ref_sink (lvp);
    g_object_unref (lvp);
    return NULL;
  }

  g_object_get (lvp->priv->play, "flags", &flags, NULL);
  flags |= GST_PLAY_FLAG_DEINTERLACE;
  g_object_set (lvp->priv->play, "flags", flags, NULL);

  lvp->priv->bus = gst_element_get_bus (lvp->priv->play);
  gst_bus_add_signal_watch (lvp->priv->bus);

  lvp->priv->sig_bus_async =
      g_signal_connect (lvp->priv->bus, "message",
      G_CALLBACK (lgm_bus_message_cb), lvp);

  /* we want to catch "prepare-xwindow-id" element messages synchronously */
  gst_bus_set_sync_handler (lvp->priv->bus, gst_bus_sync_signal_handler, lvp);
  lvp->priv->sig_bus_async =
      g_signal_connect (lvp->priv->bus, "sync-message::element",
      G_CALLBACK (lgm_element_msg_sync_cb), lvp);

  if (type == LGM_USE_TYPE_VIDEO) {
    video_sink = gst_element_factory_make ("autovideosink", "video-sink");
    audio_sink = gst_element_factory_make ("autoaudiosink", "audio-sink");
    if (gst_element_set_state (audio_sink, GST_STATE_READY) != GST_STATE_CHANGE_SUCCESS) {
      gst_object_unref (audio_sink);
      audio_sink = gst_element_factory_make ("fakesink", "audio-fake-sink");
    }
  } else {
    video_sink = gst_element_factory_make ("fakesink", "video-fake-sink");
    audio_sink = gst_element_factory_make ("fakesink", "audio-fake-sink");
    if (video_sink)
      g_object_set (video_sink, "sync", TRUE, NULL);
    if (audio_sink)
      g_object_set (audio_sink, "sync", TRUE, NULL);
  }

  if (!video_sink || !audio_sink) {
    g_set_error (err, LGM_ERROR, GST_ERROR_VIDEO_PLUGIN,
        _("No valid sink found."));
    goto sink_error;
  }

  lvp->priv->video_sink = video_sink;
  pad = gst_element_get_static_pad (video_sink, "sink");
  g_signal_connect (pad, "notify::caps",
      G_CALLBACK (lgm_parse_stream_caps), lvp);
  gst_object_unref (pad);
  g_object_set (lvp->priv->play, "video-sink", video_sink, NULL);
  g_object_set (lvp->priv->play, "audio-sink", audio_sink, NULL);

  return lvp;

  /* errors */
sink_error:
  {
    if (video_sink) {
      gst_element_set_state (video_sink, GST_STATE_NULL);
      gst_object_unref (video_sink);
    }
    if (audio_sink) {
      gst_element_set_state (audio_sink, GST_STATE_NULL);
      gst_object_unref (audio_sink);
    }

    g_object_ref (lvp);
    g_object_ref_sink (G_OBJECT (lvp));
    g_object_unref (lvp);
    return NULL;
  }
}

/* =========================================== */
/*                                             */
/*          GObject type                       */
/*                                             */
/* =========================================== */

GQuark
lgm_video_player_error_quark (void)
{
  static GQuark q;              /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("lvp-error-quark");
  }
  return q;
}

static void
lgm_video_player_finalize (GObject * object)
{
  LgmVideoPlayer *lvp = (LgmVideoPlayer *) object;

  GST_INFO ("finalizing");

  if (lvp->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
     * called again (main loop might be run again to display error dialog) */
    gst_bus_set_flushing (lvp->priv->bus, TRUE);

    if (lvp->priv->sig_bus_async)
      g_signal_handler_disconnect (lvp->priv->bus, lvp->priv->sig_bus_async);

    if (lvp->priv->sig_bus_sync)
      g_signal_handler_disconnect (lvp->priv->bus, lvp->priv->sig_bus_sync);

    gst_object_unref (lvp->priv->bus);
    lvp->priv->bus = NULL;
  }

  if (lvp->priv->uri) {
    g_free (lvp->priv->uri);
    lvp->priv->uri = NULL;
  }

  g_mutex_clear (&lvp->priv->overlay_lock);

  if (lvp->priv->play != NULL && GST_IS_ELEMENT (lvp->priv->play)) {
    gst_element_set_state (lvp->priv->play, GST_STATE_NULL);
    gst_object_unref (lvp->priv->play);
    lvp->priv->play = NULL;
  }

  G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
lgm_video_player_init (LgmVideoPlayer * lvp)
{
  LgmVideoPlayerPrivate *priv;

  lvp->priv = priv =
      G_TYPE_INSTANCE_GET_PRIVATE (lvp, LGM_TYPE_VIDEO_WIDGET,
      LgmVideoPlayerPrivate);

  priv->uri = NULL;
  priv->video_fps_n = 25;
  priv->video_fps_d = 1;
  g_mutex_init (&lvp->priv->overlay_lock);
}

static void
lgm_video_player_class_init (LgmVideoPlayerClass * klass)
{
  GObjectClass *object_class;

  object_class = (GObjectClass *) klass;
  parent_class = (GstElementClass *) g_type_class_peek_parent (klass);
  g_type_class_add_private (object_class, sizeof (LgmVideoPlayerPrivate));

  if (_lgm_debug_cat == NULL) {
    GST_DEBUG_CATEGORY_INIT (_lgm_debug_cat, "longomatch", 0,
        "LongoMatch GStreamer Backend");
  }

  /* GObject */
  object_class->finalize = lgm_video_player_finalize;

  /* Signals */
  lgm_signals[SIGNAL_ERROR] =
      g_signal_new ("error",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (LgmVideoPlayerClass, error),
      NULL, NULL,
      g_cclosure_marshal_VOID__STRING, G_TYPE_NONE, 1, G_TYPE_STRING);

  lgm_signals[SIGNAL_EOS] =
      g_signal_new ("eos",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (LgmVideoPlayerClass, eos),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  lgm_signals[SIGNAL_READY_TO_SEEK] =
      g_signal_new ("ready_to_seek",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (LgmVideoPlayerClass, ready_to_seek),
      NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  lgm_signals[SIGNAL_TICK] =
      g_signal_new ("tick",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (LgmVideoPlayerClass, tick),
      NULL, NULL,
      baconvideowidget_marshal_VOID__INT64_INT64_DOUBLE,
      G_TYPE_NONE, 3, G_TYPE_INT64, G_TYPE_INT64, G_TYPE_DOUBLE);


  lgm_signals[SIGNAL_STATE_CHANGE] =
      g_signal_new ("state_change",
      G_TYPE_FROM_CLASS (object_class),
      G_SIGNAL_RUN_LAST,
      G_STRUCT_OFFSET (LgmVideoPlayerClass, state_change),
      NULL, NULL,
      g_cclosure_marshal_VOID__BOOLEAN, G_TYPE_NONE, 1, G_TYPE_BOOLEAN);
}
