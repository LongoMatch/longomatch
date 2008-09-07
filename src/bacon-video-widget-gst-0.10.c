/* 
 * Copyright (C) 2003-2007 the GStreamer project
 *      Julien Moutte <julien@moutte.net>
 *      Ronald Bultje <rbultje@ronald.bitfreak.net>
 *      Tim-Philipp Müller <tim centricular net>
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
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 * The Totem project hereby grant permission for non-gpl compatible GStreamer
 * plugins to be used and distributed together with GStreamer and Totem. This
 * permission is above and beyond the permissions granted by the GPL license
 * Totem is covered by.
 *
 * Monday 7th February 2005: Christian Schaller: Add exemption clause.
 * See license_change file for details.
 *
 */





#include <gst/gst.h>

/* GStreamer Interfaces */
#include <gst/interfaces/xoverlay.h>
#include <gst/interfaces/navigation.h>
#include <gst/interfaces/colorbalance.h>
/* for detecting sources of errors */
#include <gst/video/gstvideosink.h>
#include <gst/video/video.h>
#include <gst/audio/gstbaseaudiosink.h>
/* for pretty multichannel strings */
#include <gst/audio/multichannel.h>



#if 0
/* for missing decoder/demuxer detection */
//include <gst/pbutils/pbutils.h>
#endif

/* system */
//#include <unistd.h>
#include <time.h>
#include <string.h>
#include <stdio.h>

/* gtk+/gnome */
#ifdef WIN32
	#include <gdk/gdkwin32.h>
#else
	#include <gdk/gdkx.h>
#endif
#include <gtk/gtk.h>
#include <glib/gi18n.h>


//#include <gconf/gconf-client.h>

#include "bacon-video-widget.h"
#include "baconvideowidget-marshal.h"
#include "gstscreenshot.h"
#include "gstvideowidget.h"

#define DEFAULT_HEIGHT 420
#define DEFAULT_WIDTH  315

#define is_error(e, d, c) \
  (e->domain == GST_##d##_ERROR && \
   e->code == GST_##d##_ERROR_##c)

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_SEGMENT_DONE,
  SIGNAL_REDIRECT,
  SIGNAL_TITLE_CHANGE,
  SIGNAL_CHANNELS_CHANGE,
  SIGNAL_TICK,
  SIGNAL_GOT_METADATA,
  SIGNAL_BUFFERING,
  SIGNAL_MISSING_PLUGINS,
  SIGNAL_STATE_CHANGED,
  SIGNAL_GOT_DURATION,
  SIGNAL_READY_TO_SEEK,
  LAST_SIGNAL
};

/* Properties */
enum
{
  PROP_0,
  PROP_LOGO_MODE,
  PROP_POSITION,
  PROP_CURRENT_TIME,
  PROP_STREAM_LENGTH,
  PROP_PLAYING,
  PROP_SEEKABLE,
  PROP_SHOWCURSOR,
  PROP_MEDIADEV,
  PROP_VOLUME
};



struct BaconVideoWidgetPrivate
{
  BaconVideoWidgetAspectRatio  ratio_type;
  
  char 						  *mrl;

  GstElement                  *play;
  GstXOverlay                 *xoverlay; /* protect with lock */
  GstColorBalance             *balance;  /* protect with lock */
  GMutex                      *lock;

  guint                        update_id;

  GdkPixbuf                   *logo_pixbuf;

  gboolean                     media_has_video;
  gboolean                     media_has_audio;
  gint                         seekable; /* -1 = don't know, FALSE = no */
  gint64                       stream_length;
  gint64                       current_time_nanos;
  gint64                       current_time;
  gfloat                       current_position;

  GstTagList                  *tagcache;
  GstTagList                  *audiotags;
  GstTagList                  *videotags;

  gboolean                     got_redirect;

  GtkWidget                  *video_window;
  GtkAllocation                video_window_allocation;



  /* Other stuff */
  gint                         xpos, ypos;
  gboolean                     logo_mode;
  gboolean                     cursor_shown;
  gboolean                     fullscreen_mode;
  gboolean                     auto_resize;
  gboolean                     have_xvidmode;
  gboolean                     uses_fakesink;
  
  gint                         video_width; /* Movie width */
  gint                         video_height; /* Movie height */
  const GValue                *movie_par; /* Movie pixel aspect ratio */
  gint                         video_width_pixels; /* Scaled movie width */
  gint                         video_height_pixels; /* Scaled movie height */
  gint                         video_fps_n;
  gint                         video_fps_d;

  guint                        init_width;
  guint                        init_height;
  
  gchar                       *media_device;

  BaconVideoWidgetAudioOutType speakersetup;
 

  GstMessageType               ignore_messages_mask;



  GstBus                      *bus;
  gulong                       sig_bus_sync;
  gulong                       sig_bus_async;

  gvcUseType                   use_type;

  gint                         eos_id;

  /* state we want to be in, as opposed to actual pipeline state
   * which may change asynchronously or during buffering */
  GstState                     target_state;
  gboolean                     buffering;

  /* for easy codec installation */
  GList                       *missing_plugins;   /* GList of GstMessages */
  gboolean                     plugin_install_in_progress;
};

static void bacon_video_widget_set_property (GObject * object,
                                             guint property_id,
                                             const GValue * value,
                                             GParamSpec * pspec);
static void bacon_video_widget_get_property (GObject * object,
                                             guint property_id,
                                             GValue * value,
                                             GParamSpec * pspec);

static void bacon_video_widget_finalize (GObject * object);

static void gvc_update_interface_implementations (BaconVideoWidget *gvc);

static void gvc_process_pending_tag_messages (BaconVideoWidget * gvc);
static void gvc_stop_play_pipeline (BaconVideoWidget * gvc);
static gboolean gvc_expose_event (GtkWidget *widget, GdkEventExpose *event,gpointer user_data);


static GError* gvc_error_from_gst_error (BaconVideoWidget *gvc, GstMessage *m);
static GList * get_stream_info_objects_for_type (BaconVideoWidget * gvc,
    const gchar * typestr);

static GtkWidgetClass *parent_class = NULL;

static int gvc_signals[LAST_SIGNAL] = { 0 };

GST_DEBUG_CATEGORY (_totem_gst_debug_cat);
#define GST_CAT_DEFAULT _totem_gst_debug_cat

/* FIXME: temporary utility functions so we don't have to up the GStreamer
 * requirements to core/base CVS (0.10.11.1) before the next totem release */
#define gst_pb_utils_init() /* noop */
#define gst_is_missing_plugin_message(msg) \
	gvc_is_missing_plugin_message(msg)
#define gst_missing_plugin_message_get_description \
	gvc_missing_plugin_message_get_description
#define gst_missing_plugin_message_get_installer_detail \
    gvc_missing_plugin_message_get_installer_detail

static gboolean
gvc_is_missing_plugin_message (GstMessage * msg)
{
  g_return_val_if_fail (msg != NULL, FALSE);
  g_return_val_if_fail (GST_IS_MESSAGE (msg), FALSE);

  if (GST_MESSAGE_TYPE (msg) != GST_MESSAGE_ELEMENT || msg->structure == NULL)
    return FALSE;

  return gst_structure_has_name (msg->structure, "missing-plugin");
}

static gchar *
gvc_missing_plugin_message_get_description (GstMessage * msg)
{
  g_return_val_if_fail (gvc_is_missing_plugin_message (msg), NULL);

  return g_strdup (gst_structure_get_string (msg->structure, "name"));
}

static gchar *
gvc_missing_plugin_message_get_installer_detail (GstMessage * msg)
{
  const GValue *val;
  const gchar *type;
  gchar *desc, *ret, *details;

  g_return_val_if_fail (gvc_is_missing_plugin_message (msg), NULL);

  type = gst_structure_get_string (msg->structure, "type");
  g_return_val_if_fail (type != NULL, NULL);
  val = gst_structure_get_value (msg->structure, "detail");
  g_return_val_if_fail (val != NULL, NULL);
  if (G_VALUE_HOLDS (val, GST_TYPE_CAPS)) {
    details = gst_caps_to_string (gst_value_get_caps (val));
  } else if (G_VALUE_HOLDS (val, G_TYPE_STRING)) {
    details = g_value_dup_string (val);
  } else {
    g_return_val_if_reached (NULL);
  }
  desc = gvc_missing_plugin_message_get_description (msg);
  ret = g_strdup_printf ("gstreamer.net|0.10|totem|%s|%s-%s",
      (desc) ? desc : "", type, (details) ? details: "");
  g_free (desc);
  g_free (details);
  return ret;
}

typedef gchar * (* MsgToStrFunc) (GstMessage * msg);

static gchar **
gvc_get_missing_plugins_foo (const GList * missing_plugins, MsgToStrFunc func)
{
  GPtrArray *arr = g_ptr_array_new ();

  while (missing_plugins != NULL) {
    g_ptr_array_add (arr, func (GST_MESSAGE (missing_plugins->data)));
    missing_plugins = missing_plugins->next;
  }
  g_ptr_array_add (arr, NULL);
  return (gchar **) g_ptr_array_free (arr, FALSE);
}

static gchar **
gvc_get_missing_plugins_details (const GList * missing_plugins)
{
  return gvc_get_missing_plugins_foo (missing_plugins,
      gst_missing_plugin_message_get_installer_detail);
}

static gchar **
gvc_get_missing_plugins_descriptions (const GList * missing_plugins)
{
  return gvc_get_missing_plugins_foo (missing_plugins,
      gst_missing_plugin_message_get_description);
}

static void
gvc_clear_missing_plugins_messages (BaconVideoWidget * gvc)
{
  g_list_foreach (gvc->priv->missing_plugins,
                  (GFunc) gst_mini_object_unref, NULL);
  g_list_free (gvc->priv->missing_plugins);
  gvc->priv->missing_plugins = NULL;
}

static void
gvc_check_if_video_decoder_is_missing (BaconVideoWidget * gvc)
{
  GList *l;

  if (gvc->priv->media_has_video || gvc->priv->missing_plugins == NULL)
    return;

  for (l = gvc->priv->missing_plugins; l != NULL; l = l->next) {
    GstMessage *msg = GST_MESSAGE (l->data);
    gchar *d, *f;

    if ((d = gst_missing_plugin_message_get_installer_detail (msg))) {
      if ((f = strstr (d, "|decoder-")) && strstr (f, "video")) {
        GError *err;

        /* create a fake GStreamer error so we get a nice warning message */
        err = g_error_new (GST_CORE_ERROR, GST_CORE_ERROR_MISSING_PLUGIN, "x");
        msg = gst_message_new_error (GST_OBJECT (gvc->priv->play), err, NULL);
        g_error_free (err);
        err = gvc_error_from_gst_error (gvc, msg);
        gst_message_unref (msg);
        g_signal_emit (gvc, gvc_signals[SIGNAL_ERROR], 0, err->message);
        g_error_free (err);
        g_free (d);
        break;
      }
      g_free (d);
    }
  }
}

static void
gvc_error_msg (BaconVideoWidget * gvc, GstMessage * msg)
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
    GST_ERROR ("uri     = %s", GST_STR_NULL (gvc->priv->mrl));

    g_message ("Error: %s\n%s\n", GST_STR_NULL (err->message),
        GST_STR_NULL (dbg));

    g_error_free (err);
  }
  g_free (dbg);
}

static void
get_media_size (BaconVideoWidget *gvc, gint *width, gint *height)
{
  if (gvc->priv->logo_mode) {
    if (gvc->priv->logo_pixbuf) {
      *width = gdk_pixbuf_get_width (gvc->priv->logo_pixbuf);
      *height = gdk_pixbuf_get_height (gvc->priv->logo_pixbuf);
    } else {
      *width = 0;
      *height = 0;
    }
  } else {
    if (gvc->priv->media_has_video) {
      GValue * disp_par = NULL;
      guint movie_par_n, movie_par_d, disp_par_n, disp_par_d, num, den;
      
      /* Create and init the fraction value */
      disp_par = g_new0 (GValue, 1);
      g_value_init (disp_par, GST_TYPE_FRACTION);

      /* Square pixel is our default */
      gst_value_set_fraction (disp_par, 1, 1);
    
      /* Now try getting display's pixel aspect ratio */
      if (gvc->priv->xoverlay) {
        GObjectClass *klass;
        GParamSpec *pspec;

        klass = G_OBJECT_GET_CLASS (gvc->priv->xoverlay);
        pspec = g_object_class_find_property (klass, "pixel-aspect-ratio");
      
        if (pspec != NULL) {
          GValue disp_par_prop = { 0, };

          g_value_init (&disp_par_prop, pspec->value_type);
          g_object_get_property (G_OBJECT (gvc->priv->xoverlay),
              "pixel-aspect-ratio", &disp_par_prop);

          if (!g_value_transform (&disp_par_prop, disp_par)) {
            GST_WARNING ("Transform failed, assuming pixel-aspect-ratio = 1/1");
            gst_value_set_fraction (disp_par, 1, 1);
          }
        
          g_value_unset (&disp_par_prop);
        }
      }
      
      disp_par_n = gst_value_get_fraction_numerator (disp_par);
      disp_par_d = gst_value_get_fraction_denominator (disp_par);
      
      GST_INFO ("display PAR is %d/%d", disp_par_n, disp_par_d);
      
      /* If movie pixel aspect ratio is enforced, use that */
      if (gvc->priv->ratio_type != gvc_RATIO_AUTO) {
        switch (gvc->priv->ratio_type) {
          case gvc_RATIO_SQUARE:
            movie_par_n = 1;
            movie_par_d = 1;
            break;
          case gvc_RATIO_FOURBYTHREE:
            movie_par_n = 4 * gvc->priv->video_height;
            movie_par_d = 3 * gvc->priv->video_width;
            break;
          case gvc_RATIO_ANAMORPHIC:
            movie_par_n = 16 * gvc->priv->video_height;
            movie_par_d = 9 * gvc->priv->video_width;
            break;
          case gvc_RATIO_DVB:
            movie_par_n = 20 * gvc->priv->video_height;
            movie_par_d = 9 * gvc->priv->video_width;
            break;
          /* handle these to avoid compiler warnings */
          case gvc_RATIO_AUTO:
          default:
            movie_par_n = 0;
            movie_par_d = 0;
            g_assert_not_reached ();
        }
      }
      else {
        /* Use the movie pixel aspect ratio if any */
        if (gvc->priv->movie_par) {
          movie_par_n = gst_value_get_fraction_numerator (gvc->priv->movie_par);
          movie_par_d =
              gst_value_get_fraction_denominator (gvc->priv->movie_par);
        }
        else {
          /* Square pixels */
          movie_par_n = 1;
          movie_par_d = 1;
        }
      }
      
      GST_INFO ("movie PAR is %d/%d", movie_par_n, movie_par_d);

      if (!gst_video_calculate_display_ratio (&num, &den,
          gvc->priv->video_width, gvc->priv->video_height,
          movie_par_n, movie_par_d, disp_par_n, disp_par_d)) {
        GST_WARNING ("overflow calculating display aspect ratio!");
        num = 1;   /* FIXME: what values to use here? */
        den = 1;
      }

      GST_INFO ("calculated scaling ratio %d/%d for video %dx%d", num, den,
          gvc->priv->video_width, gvc->priv->video_height);
      
      /* now find a width x height that respects this display ratio.
       * prefer those that have one of w/h the same as the incoming video
       * using wd / hd = num / den */
    
      /* start with same height, because of interlaced video */
      /* check hd / den is an integer scale factor, and scale wd with the PAR */
      if (gvc->priv->video_height % den == 0) {
        GST_INFO ("keeping video height");
        gvc->priv->video_width_pixels =
            (guint) gst_util_uint64_scale (gvc->priv->video_height, num, den);
        gvc->priv->video_height_pixels = gvc->priv->video_height;
      } else if (gvc->priv->video_width % num == 0) {
        GST_INFO ("keeping video width");
        gvc->priv->video_width_pixels = gvc->priv->video_width;
        gvc->priv->video_height_pixels =
            (guint) gst_util_uint64_scale (gvc->priv->video_width, den, num);
      } else {
        GST_INFO ("approximating while keeping video height");
        gvc->priv->video_width_pixels =
            (guint) gst_util_uint64_scale (gvc->priv->video_height, num, den);
        gvc->priv->video_height_pixels = gvc->priv->video_height;
      }
      GST_INFO ("scaling to %dx%d", gvc->priv->video_width_pixels,
          gvc->priv->video_height_pixels);
      
      *width = gvc->priv->video_width_pixels;
      *height = gvc->priv->video_height_pixels;
      
      /* Free the PAR fraction */
      g_value_unset (disp_par);
      g_free (disp_par);
    }
    else {
      *width = 0;
      *height = 0;
    }
  }
}















static gboolean
gvc_boolean_handled_accumulator (GSignalInvocationHint * ihint,
    GValue * return_accu, const GValue * handler_return, gpointer foobar)
{
  gboolean continue_emission;
  gboolean signal_handled;
  
  signal_handled = g_value_get_boolean (handler_return);
  g_value_set_boolean (return_accu, signal_handled);
  continue_emission = !signal_handled;
  
  return continue_emission;
}

static void
bacon_video_widget_class_init (BaconVideoWidgetClass * klass)
{
  GObjectClass *object_class;


  object_class = (GObjectClass *) klass;


  parent_class = g_type_class_peek_parent (klass);

  g_type_class_add_private (object_class, sizeof (BaconVideoWidgetPrivate));

  

  /* GObject */
  object_class->set_property = bacon_video_widget_set_property;
  object_class->get_property = bacon_video_widget_get_property;
  object_class->finalize = bacon_video_widget_finalize;

  /* Properties */
  g_object_class_install_property (object_class, PROP_LOGO_MODE,
                                   g_param_spec_boolean ("logo_mode", NULL,
                                                         NULL, FALSE,
                                                         G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_POSITION,
                                   g_param_spec_int ("position", NULL, NULL,
                                                     0, G_MAXINT, 0,
                                                     G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_STREAM_LENGTH,
                                   g_param_spec_int64 ("stream_length", NULL,
                                                     NULL, 0, G_MAXINT64, 0,
                                                     G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_PLAYING,
                                   g_param_spec_boolean ("playing", NULL,
                                                         NULL, FALSE,
                                                         G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_SEEKABLE,
                                   g_param_spec_boolean ("seekable", NULL,
                                                         NULL, FALSE,
                                                         G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_VOLUME,
                                     g_param_spec_int ("volume", NULL, NULL,
                                                     0, 100, 0,
                                                     G_PARAM_READABLE));
  g_object_class_install_property (object_class, PROP_SHOWCURSOR,
                                   g_param_spec_boolean ("showcursor", NULL,
                                                         NULL, FALSE,
                                                         G_PARAM_READWRITE));
  g_object_class_install_property (object_class, PROP_MEDIADEV,
                                   g_param_spec_string ("mediadev", NULL,
                                                        NULL, FALSE,
                                                        G_PARAM_READWRITE));
  

  /* Signals */
  gvc_signals[SIGNAL_ERROR] =
    g_signal_new ("error",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, error),
                  NULL, NULL,
                  g_cclosure_marshal_VOID__STRING,
                  G_TYPE_NONE, 1, G_TYPE_STRING);

  gvc_signals[SIGNAL_EOS] =
    g_signal_new ("eos",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, eos),
                  NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);
  
  gvc_signals[SIGNAL_SEGMENT_DONE] =
    g_signal_new ("segment_done",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, segment_done),
                  NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);  
                  
    gvc_signals[SIGNAL_READY_TO_SEEK] =
    g_signal_new ("ready_to_seek",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, ready_to_seek),
                  NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);
                  
  gvc_signals[SIGNAL_GOT_DURATION] =
    g_signal_new ("got_duration",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, got_duration),
                  NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  gvc_signals[SIGNAL_GOT_METADATA] =
    g_signal_new ("got-metadata",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, got_metadata),
                  NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  gvc_signals[SIGNAL_REDIRECT] =
    g_signal_new ("got-redirect",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, got_redirect),
                  NULL, NULL, g_cclosure_marshal_VOID__STRING,
                  G_TYPE_NONE, 1, G_TYPE_STRING);

  gvc_signals[SIGNAL_TITLE_CHANGE] =
    g_signal_new ("title-change",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, title_change),
                  NULL, NULL,
                  g_cclosure_marshal_VOID__STRING,
                  G_TYPE_NONE, 1, G_TYPE_STRING);

  gvc_signals[SIGNAL_CHANNELS_CHANGE] =
    g_signal_new ("channels-change",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, channels_change),
                  NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

  gvc_signals[SIGNAL_TICK] =
    g_signal_new ("tick",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, tick),
                  NULL, NULL,
                  baconvideowidget_marshal_VOID__INT64_INT64_FLOAT_BOOLEAN,
                  G_TYPE_NONE, 4, G_TYPE_INT64, G_TYPE_INT64, G_TYPE_FLOAT,
                  G_TYPE_BOOLEAN);

  gvc_signals[SIGNAL_BUFFERING] =
    g_signal_new ("buffering",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, buffering),
                  NULL, NULL,
                  g_cclosure_marshal_VOID__INT, G_TYPE_NONE, 1, G_TYPE_INT);
                  
    gvc_signals[SIGNAL_STATE_CHANGED] =
    g_signal_new ("state_changed",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (BaconVideoWidgetClass, state_changed),
                  NULL, NULL,
                  g_cclosure_marshal_VOID__BOOLEAN,
                  G_TYPE_NONE, 1, G_TYPE_BOOLEAN);

  /* missing plugins signal:
   *  - string array: details of missing plugins for libgimme-codec
   *  - string array: details of missing plugins (human-readable strings)
   *  - bool: if we managed to start playing something even without those plugins
   *  return value: callback must return TRUE to indicate that it took some
   *                action, FALSE will be interpreted as no action taken
   */
  gvc_signals[SIGNAL_MISSING_PLUGINS] =
    g_signal_new ("missing-plugins",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  0, /* signal is enough, we don't need a vfunc */
                  gvc_boolean_handled_accumulator, NULL,
                  baconvideowidget_marshal_BOOLEAN__BOXED_BOXED_BOOLEAN,
                  G_TYPE_BOOLEAN, 3, G_TYPE_STRV, G_TYPE_STRV, G_TYPE_BOOLEAN);
}

static void
bacon_video_widget_init (BaconVideoWidget * gvc)
{
  BaconVideoWidgetPrivate *priv;



  gvc->priv = priv = G_TYPE_INSTANCE_GET_PRIVATE (gvc, BACON_TYPE_VIDEO_WIDGET, BaconVideoWidgetPrivate);

  
  priv->update_id = 0;
  priv->tagcache = NULL;
  priv->audiotags = NULL;
  priv->videotags = NULL;

  priv->lock = g_mutex_new ();

  gvc->priv->missing_plugins = NULL;
  gvc->priv->plugin_install_in_progress = FALSE;
}



static gboolean gvc_query_timeout (BaconVideoWidget *gvc);
static void parse_stream_info (BaconVideoWidget *gvc);

static void
gvc_update_stream_info (BaconVideoWidget *gvc)
{
  parse_stream_info (gvc);

  /* if we're not interactive, we want to announce metadata
   * only later when we can be sure we got it all */
  if (gvc->priv->use_type == gvc_USE_TYPE_VIDEO ||
      gvc->priv->use_type == gvc_USE_TYPE_AUDIO) {
    g_signal_emit (gvc, gvc_signals[SIGNAL_GOT_METADATA], 0, NULL);
    g_signal_emit (gvc, gvc_signals[SIGNAL_CHANNELS_CHANGE], 0);
  }
}

static void
gvc_handle_application_message (BaconVideoWidget *gvc, GstMessage *msg)
{
  const gchar *msg_name;
  gint h;
  gint w;

  msg_name = gst_structure_get_name (msg->structure);
  g_return_if_fail (msg_name != NULL);

  GST_INFO ("Handling application message: %" GST_PTR_FORMAT, msg->structure);

  if (strcmp (msg_name, "notify-streaminfo") == 0) {
    gvc_update_stream_info (gvc);
  } 
  else if (strcmp (msg_name, "video-size") == 0) {
    /* if we're not interactive, we want to announce metadata
     * only later when we can be sure we got it all */
    if (gvc->priv->use_type == gvc_USE_TYPE_VIDEO ||
        gvc->priv->use_type == gvc_USE_TYPE_AUDIO) {
      g_signal_emit (gvc, gvc_signals[SIGNAL_GOT_METADATA], 0, NULL);
    }

      get_media_size (gvc, &w, &h);
      gst_video_widget_set_source_size (GST_VIDEO_WIDGET(gvc->priv->video_window), w, h);
   
  } else {
    g_message ("Unhandled application message %s", msg_name);
  }
}

static void
gvc_handle_element_message (BaconVideoWidget *gvc, GstMessage *msg)
{
  const gchar *type_name = NULL;
  gchar *src_name;

  src_name = gst_object_get_name (msg->src);
  if (msg->structure)
    type_name = gst_structure_get_name (msg->structure);

  GST_INFO ("from %s: %" GST_PTR_FORMAT, src_name, msg->structure);

  if (type_name == NULL)
    goto unhandled;

  if (strcmp (type_name, "redirect") == 0) {
    const gchar *new_location;

    new_location = gst_structure_get_string (msg->structure, "new-location");
    GST_INFO ("Got redirect to '%s'", GST_STR_NULL (new_location));

    if (new_location && *new_location) {
      g_signal_emit (gvc, gvc_signals[SIGNAL_REDIRECT], 0, new_location);
      goto done;
    }
  } else if (strcmp (type_name, "progress") == 0) {
    /* this is similar to buffering messages, but shouldn't affect pipeline
     * state; qtdemux emits those when headers are after movie data and
     * it is in streaming mode and has to receive all the movie data first */
    if (!gvc->priv->buffering) {
      gint percent = 0;

      if (gst_structure_get_int (msg->structure, "percent", &percent))
        g_signal_emit (gvc, gvc_signals[SIGNAL_BUFFERING], 0, percent);
    }
    goto done;
  } else if (strcmp (type_name, "prepare-xwindow-id") == 0 ||
      strcmp (type_name, "have-xwindow-id") == 0) {
    /* we handle these synchronously or want to ignore them */
    goto done;
  } else if (gst_is_missing_plugin_message (msg)) {
    gvc->priv->missing_plugins =
      g_list_prepend (gvc->priv->missing_plugins, gst_message_ref (msg));
    goto done;
  }

unhandled:
  GST_WARNING ("Unhandled element message %s from %s: %" GST_PTR_FORMAT,
      GST_STR_NULL (type_name), GST_STR_NULL (src_name), msg);

done:
  g_free (src_name);
}

/* This is a hack to avoid doing poll_for_state_change() indirectly
 * from the bus message callback (via EOS => totem => close => wait for ready)
 * and deadlocking there. We need something like a
 * gst_bus_set_auto_flushing(bus, FALSE) ... */
static gboolean
gvc_signal_eos_delayed (gpointer user_data)
{
  BaconVideoWidget *gvc = BACON_VIDEO_WIDGET (user_data);
  g_print("EOS delayed\n");
  g_signal_emit (gvc, gvc_signals[SIGNAL_EOS], 0, NULL);
  gvc->priv->eos_id = 0;
  return FALSE;
}

static void
gvc_reconfigure_tick_timeout (BaconVideoWidget *gvc, guint msecs)
{
  if (gvc->priv->update_id != 0) {
    GST_INFO ("removing tick timeout");
    g_source_remove (gvc->priv->update_id);
    gvc->priv->update_id = 0;
  }
  if (msecs > 0) {
    GST_INFO ("adding tick timeout (at %ums)", msecs);
    gvc->priv->update_id =
      g_timeout_add (msecs, (GSourceFunc) gvc_query_timeout, gvc);
  }
}

static gboolean
gvc_expose_event (GtkWidget *widget, GdkEventExpose *event, gpointer user_data)
{
	BaconVideoWidget *gvc;

	
	g_return_val_if_fail (widget != NULL, FALSE);
  	g_return_val_if_fail (GST_IS_VIDEO_WIDGET (widget), FALSE);
  	g_return_val_if_fail (event != NULL, FALSE);
  	
	gvc = BACON_VIDEO_WIDGET (user_data);  	

	
	 //Pass the expose to the widget
	gst_video_widget_force_expose(widget,event);
	 
   if (gvc->priv->xoverlay != NULL && !gvc->priv->logo_mode)
      gst_x_overlay_expose (gvc->priv->xoverlay);
  
	
		
   
   return TRUE;


	
}


/* returns TRUE if the error/signal has been handled and should be ignored */
static gboolean
gvc_emit_missing_plugins_signal (BaconVideoWidget * gvc, gboolean prerolled)
{
  gboolean handled = FALSE;
  gchar **descriptions, **details;

  details = gvc_get_missing_plugins_details (gvc->priv->missing_plugins);
  descriptions = gvc_get_missing_plugins_descriptions (gvc->priv->missing_plugins);

  GST_LOG ("emitting missing-plugins signal (prerolled=%d)", prerolled);

  g_signal_emit (gvc, gvc_signals[SIGNAL_MISSING_PLUGINS], 0,
      details, descriptions, prerolled, &handled);
  GST_INFO ("missing-plugins signal was %shandled", (handled) ? "" : "not ");

  g_strfreev (descriptions);
  g_strfreev (details);

  if (handled) {
    gvc->priv->plugin_install_in_progress = TRUE;
    gvc_clear_missing_plugins_messages (gvc);
  }

  /* if it wasn't handled, we might need the list of missing messages again
   * later to create a proper error message with details of what's missing */

  return handled;
}

/* returns TRUE if the error has been handled and should be ignored */
static gboolean
gvc_check_missing_plugins_error (BaconVideoWidget * gvc, GstMessage * err_msg)
{
  gboolean error_src_is_playbin;
  gboolean ret = FALSE;
  GError *err = NULL;

  if (gvc->priv->missing_plugins == NULL) {
    GST_INFO ("no missing-plugin messages");
    return FALSE;
  }

  gst_message_parse_error (err_msg, &err, NULL);

  error_src_is_playbin = (err_msg->src == GST_OBJECT_CAST (gvc->priv->play));

  /* If we get a WRONG_TYPE error from playbin itself it's most likely because
   * there is a subtitle stream we can decode, but no video stream to overlay
   * it on. Since there were missing-plugins messages, we'll assume this is
   * because we cannot decode the video stream (this should probably be fixed
   * in playbin, but for now we'll work around it here) */
  if (is_error (err, CORE, MISSING_PLUGIN) ||
      is_error (err, STREAM, CODEC_NOT_FOUND) ||
      (is_error (err, STREAM, WRONG_TYPE) && error_src_is_playbin)) {
    ret = gvc_emit_missing_plugins_signal (gvc, FALSE);
    if (ret) {
      /* If it was handled, stop playback to make sure we're not processing any
       * other error messages that might also be on the bus */
      bacon_video_widget_stop (gvc);
    }
  } else {
    GST_INFO ("not an error code we are looking for, doing nothing");
  }

  g_error_free (err);
  return ret;
}

/* returns TRUE if the error/signal has been handled and should be ignored */
static gboolean
gvc_check_missing_plugins_on_preroll (BaconVideoWidget * gvc)
{
  if (gvc->priv->missing_plugins == NULL) {
    GST_INFO ("no missing-plugin messages");
    return FALSE;
  }

  return gvc_emit_missing_plugins_signal (gvc, TRUE); 
}

static void
gvc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  BaconVideoWidget *gvc = (BaconVideoWidget *) data;
  GstMessageType msg_type;

  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));

  msg_type = GST_MESSAGE_TYPE (message);

  /* somebody else is handling the message, probably in poll_for_state_change */
  if (gvc->priv->ignore_messages_mask & msg_type) {
    GST_LOG ("Ignoring %s message from element %" GST_PTR_FORMAT
        " as requested: %" GST_PTR_FORMAT, GST_MESSAGE_TYPE_NAME (message),
        message->src, message);
    return;
  }

  if (msg_type != GST_MESSAGE_STATE_CHANGED) {
    gchar *src_name = gst_object_get_name (message->src);
    GST_LOG ("Handling %s message from element %s",
        gst_message_type_get_name (msg_type), src_name);
    g_free (src_name);
  }

  switch (msg_type) {
    case GST_MESSAGE_ERROR: {

      gvc_error_msg (gvc, message);

      if (!gvc_check_missing_plugins_error (gvc, message)) {
        GError *error;

        error = gvc_error_from_gst_error (gvc, message);

        g_signal_emit (gvc, gvc_signals[SIGNAL_ERROR], 0,
                       error->message);

        if (gvc->priv->play)
          gst_element_set_state (gvc->priv->play, GST_STATE_NULL);

        gvc->priv->target_state = GST_STATE_NULL;
        gvc->priv->buffering = FALSE;
        g_error_free (error);
      }
      break;
    }
    case GST_MESSAGE_WARNING: {
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;
    }
    case GST_MESSAGE_TAG: {
      GstTagList *tag_list, *result;
      GstElementFactory *f;

      gst_message_parse_tag (message, &tag_list);

      GST_INFO ("Tags: %" GST_PTR_FORMAT, tag_list);

      /* all tags (replace previous tags, title/artist/etc. might change
       * in the middle of a stream, e.g. with radio streams) */
      result = gst_tag_list_merge (gvc->priv->tagcache, tag_list,
                                   GST_TAG_MERGE_REPLACE);
      if (gvc->priv->tagcache)
        gst_tag_list_free (gvc->priv->tagcache);
      gvc->priv->tagcache = result;

      /* media-type-specific tags */
      if (GST_IS_ELEMENT (message->src) &&
          (f = gst_element_get_factory (GST_ELEMENT (message->src)))) {
        const gchar *klass = gst_element_factory_get_klass (f);
        GstTagList **cache = NULL;

        if (g_strrstr (klass, "Video")) {
          cache = &gvc->priv->videotags;
        } else if (g_strrstr (klass, "Audio")) {
          cache = &gvc->priv->audiotags;
        }

        if (cache) {
          result = gst_tag_list_merge (*cache, tag_list, GST_TAG_MERGE_REPLACE);
          if (*cache)
            gst_tag_list_free (*cache);
          *cache = result;
        }
      }

      /* clean up */
      gst_tag_list_free (tag_list);

      /* if we're not interactive, we want to announce metadata
       * only later when we can be sure we got it all */
      if (gvc->priv->use_type == gvc_USE_TYPE_VIDEO ||
          gvc->priv->use_type == gvc_USE_TYPE_AUDIO) {
        g_signal_emit (gvc, gvc_signals[SIGNAL_GOT_METADATA], 0);
      }
      break;
    }
    case GST_MESSAGE_EOS:
      GST_INFO ("EOS message");
      /* update slider one last time */
      gvc_query_timeout (gvc);
      if (gvc->priv->eos_id == 0)
        gvc->priv->eos_id = g_idle_add (gvc_signal_eos_delayed, gvc);
      break;
	  
	case GST_MESSAGE_SEGMENT_DONE:
		g_signal_emit (gvc, gvc_signals[SIGNAL_SEGMENT_DONE], 0);
		break;
	
    case GST_MESSAGE_BUFFERING: {
      gint percent = 0;

      /* FIXME: use gst_message_parse_buffering() once core 0.10.11 is out */
      gst_structure_get_int (message->structure, "buffer-percent", &percent);
      g_signal_emit (gvc, gvc_signals[SIGNAL_BUFFERING], 0, percent);

      if (percent >= 100) {
        /* a 100% message means buffering is done */
        gvc->priv->buffering = FALSE;
        /* if the desired state is playing, go back */
        if (gvc->priv->target_state == GST_STATE_PLAYING) {
          GST_INFO ("Buffering done, setting pipeline back to PLAYING");
          gst_element_set_state (gvc->priv->play, GST_STATE_PLAYING);
        } else {
          GST_INFO ("Buffering done, keeping pipeline PAUSED");
        }
      } else if (gvc->priv->buffering == FALSE &&
          gvc->priv->target_state == GST_STATE_PLAYING) {
        GstState cur_state;

        gst_element_get_state (gvc->priv->play, &cur_state, NULL, 0);
        if (cur_state == GST_STATE_PLAYING) {
          GST_INFO ("Buffering ... temporarily pausing playback");
          gst_element_set_state (gvc->priv->play, GST_STATE_PAUSED);
        } else {
          GST_INFO ("Buffering ... prerolling, not doing anything");
        }
        gvc->priv->buffering = TRUE;
      } else {
        GST_LOG ("Buffering ... %d", percent);
      }
      break;
    }
    case GST_MESSAGE_APPLICATION: {
      gvc_handle_application_message (gvc, message);
      break;
    }
    case GST_MESSAGE_STATE_CHANGED: {
      GstState old_state, new_state;
      gchar *src_name;

      gst_message_parse_state_changed (message, &old_state, &new_state, NULL);
	
      if (old_state == new_state)
        break;

      /* we only care about playbin (pipeline) state changes */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (gvc->priv->play))
        break;

      src_name = gst_object_get_name (message->src);

      GST_INFO ("%s changed state from %s to %s", src_name,
          gst_element_state_get_name (old_state),
          gst_element_state_get_name (new_state));
      g_free (src_name);

      /* now do stuff */
      if (new_state < GST_STATE_PAUSED) {
        gvc_reconfigure_tick_timeout (gvc, 0);
        
        g_signal_emit (gvc, gvc_signals[SIGNAL_STATE_CHANGED], 0, FALSE);
      } else if (new_state == GST_STATE_PAUSED) {
        // yes, we need to keep the tick timeout running in PAUSED state
        //  as well, totem depends on that (use lower frequency though) 
       
	    gvc_reconfigure_tick_timeout (gvc, 500);
	    g_signal_emit (gvc, gvc_signals[SIGNAL_STATE_CHANGED], 0, FALSE);
	    g_signal_emit (gvc, gvc_signals[SIGNAL_READY_TO_SEEK], 0, FALSE);
      } else if (new_state > GST_STATE_PAUSED) {
        gvc_reconfigure_tick_timeout (gvc, 200);
        g_signal_emit (gvc, gvc_signals[SIGNAL_STATE_CHANGED], 0, TRUE);
      }

      if (old_state == GST_STATE_READY && new_state == GST_STATE_PAUSED) {
        gvc_update_stream_info (gvc);
        if (!gvc_check_missing_plugins_on_preroll (gvc)) {
          /* show a non-fatal warning message if we can't decode the video */
          gvc_check_if_video_decoder_is_missing (gvc);
        }
      } else if (old_state == GST_STATE_PAUSED && new_state == GST_STATE_READY) {
        gvc->priv->media_has_video = FALSE;
        gvc->priv->media_has_audio = FALSE;

        /* clean metadata cache */
        if (gvc->priv->tagcache) {
          gst_tag_list_free (gvc->priv->tagcache);
          gvc->priv->tagcache = NULL;
        }
        if (gvc->priv->audiotags) {
          gst_tag_list_free (gvc->priv->audiotags);
          gvc->priv->audiotags = NULL;
        }
        if (gvc->priv->videotags) {
          gst_tag_list_free (gvc->priv->videotags);
          gvc->priv->videotags = NULL;
        }

        gvc->priv->video_width = 0;
        gvc->priv->video_height = 0;
      }
      break;
    }
    case GST_MESSAGE_ELEMENT:{
      gvc_handle_element_message (gvc, message);
      break;
    }

    case GST_MESSAGE_DURATION: {
      /* force _get_stream_length() to do new duration query */
      gvc->priv->stream_length = 0;
      if (bacon_video_widget_get_stream_length (gvc) == 0) {
        GST_INFO ("Failed to query duration after DURATION message?!");
      }
      else
      	g_signal_emit (gvc, gvc_signals[SIGNAL_GOT_DURATION], 0, NULL);
      break;
    }

    case GST_MESSAGE_CLOCK_PROVIDE:
    case GST_MESSAGE_CLOCK_LOST:
    case GST_MESSAGE_NEW_CLOCK:
    case GST_MESSAGE_STATE_DIRTY:
      break;

    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
}

/* FIXME: how to recognise this in 0.9? */
#if 0
static void
group_switch (GstElement *play, BaconVideoWidget *gvc)
{
  GstMessage *msg;

  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));

  if (gvc->priv->tagcache) {
    gst_tag_list_free (gvc->priv->tagcache);
    gvc->priv->tagcache = NULL;
  }
  if (gvc->priv->audiotags) {
    gst_tag_list_free (gvc->priv->audiotags);
    gvc->priv->audiotags = NULL;
  }
  if (gvc->priv->videotags) {
    gst_tag_list_free (gvc->priv->videotags);
    gvc->priv->videotags = NULL;
  }

  msg = gst_message_new_application (GST_OBJECT (gvc->priv->play),
      gst_structure_new ("notify-streaminfo", NULL));
  gst_element_post_message (gvc->priv->play, msg);
}
#endif

static void
got_video_size (BaconVideoWidget * gvc)
{
  GstMessage *msg;

  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));

  msg = gst_message_new_application (GST_OBJECT (gvc->priv->play),
      gst_structure_new ("video-size", "width", G_TYPE_INT,
          gvc->priv->video_width, "height", G_TYPE_INT,
          gvc->priv->video_height, NULL));
  gst_element_post_message (gvc->priv->play, msg);
}

static void
got_time_tick (GstElement * play, gint64 time_nanos, BaconVideoWidget * gvc)
{
  gboolean seekable;

  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));

  if (gvc->priv->logo_mode != FALSE)
    return;

  gvc->priv->current_time_nanos = time_nanos;

  gvc->priv->current_time = (gint64) time_nanos / GST_MSECOND;

  if (gvc->priv->stream_length == 0) {
    gvc->priv->current_position = 0;
  } else {
    gvc->priv->current_position =
      (gfloat) gvc->priv->current_time / gvc->priv->stream_length;
  }

  if (gvc->priv->stream_length == 0) {
    seekable = bacon_video_widget_is_seekable (gvc);
  } else {
    seekable = TRUE;
  }

/*
  GST_INFO ("%" GST_TIME_FORMAT ",%" GST_TIME_FORMAT " %s",
      GST_TIME_ARGS (gvc->priv->current_time),
      GST_TIME_ARGS (gvc->priv->stream_length),
      (seekable) ? "TRUE" : "FALSE");
*/
  
  g_signal_emit (gvc, gvc_signals[SIGNAL_TICK], 0,
                 gvc->priv->current_time, gvc->priv->stream_length,
                 gvc->priv->current_position,
                 seekable);
}

static void
playbin_source_notify_cb (GObject *play, GParamSpec *p, BaconVideoWidget *gvc)
{
  GObject *source = NULL;

  /* CHECKME: do we really need these taglist frees here (tpm)? */
  if (gvc->priv->tagcache) {
    gst_tag_list_free (gvc->priv->tagcache);
    gvc->priv->tagcache = NULL;
  }
  if (gvc->priv->audiotags) {
    gst_tag_list_free (gvc->priv->audiotags);
    gvc->priv->audiotags = NULL;
  }
  if (gvc->priv->videotags) {
    gst_tag_list_free (gvc->priv->videotags);
    gvc->priv->videotags = NULL;
  }

  g_object_get (play, "source", &source, NULL);
  if (!source)
    return;

  GST_INFO ("Got source of type %s", G_OBJECT_TYPE_NAME (source));

  if (gvc->priv->media_device) {
    if (g_object_class_find_property (G_OBJECT_GET_CLASS (source), "device")) {
      GST_INFO ("Setting device to '%s'", gvc->priv->media_device);
      g_object_set (source, "device", gvc->priv->media_device, NULL);
    }
  }

  g_object_unref (source);
}

static gboolean
gvc_query_timeout (BaconVideoWidget *gvc)
{
  GstFormat fmt = GST_FORMAT_TIME;
  gint64 prev_len = -1;
  gint64 pos = -1, len = -1;
  
  /* check length/pos of stream */
  prev_len = gvc->priv->stream_length;
  if (gst_element_query_duration (gvc->priv->play, &fmt, &len)) {
    if (len != -1 && fmt == GST_FORMAT_TIME) {
      gvc->priv->stream_length = len / GST_MSECOND;
      if (gvc->priv->stream_length != prev_len) {
        g_signal_emit (gvc, gvc_signals[SIGNAL_GOT_METADATA], 0, NULL);
      }
    }
  } else {
    GST_INFO ("could not get duration");
  }

  if (gst_element_query_position (gvc->priv->play, &fmt, &pos)) {
    if (pos != -1 && fmt == GST_FORMAT_TIME) {
      got_time_tick (GST_ELEMENT (gvc->priv->play), pos, gvc);
    }
  } else {
    GST_INFO ("could not get position");
  }

  return TRUE;
}

static void
caps_set (GObject * obj,
    GParamSpec * pspec, BaconVideoWidget * gvc)
{
  GstPad *pad = GST_PAD (obj);
  GstStructure *s;
  GstCaps *caps;

  if (!(caps = gst_pad_get_negotiated_caps (pad)))
    return;

  /* Get video decoder caps */
  s = gst_caps_get_structure (caps, 0);
  if (s) {
    /* We need at least width/height and framerate */
    if (!(gst_structure_get_fraction (s, "framerate", &gvc->priv->video_fps_n, 
          &gvc->priv->video_fps_d) &&
          gst_structure_get_int (s, "width", &gvc->priv->video_width) &&
          gst_structure_get_int (s, "height", &gvc->priv->video_height)))
      return;
    
    /* Get the movie PAR if available */
    gvc->priv->movie_par = gst_structure_get_value (s, "pixel-aspect-ratio");
    
    /* Now set for real */
    bacon_video_widget_set_aspect_ratio (gvc, gvc->priv->ratio_type);
  }

  gst_caps_unref (caps);
}


static void
parse_stream_info (BaconVideoWidget *gvc)
{
  GList *audio_streams, *video_streams, *l;
  GstPad *videopad = NULL;

  audio_streams = get_stream_info_objects_for_type (gvc, "audio");
  video_streams = get_stream_info_objects_for_type (gvc, "video");

  gvc->priv->media_has_video = FALSE;
  if (video_streams) {
    gvc->priv->media_has_video = TRUE;
    for (l = video_streams; videopad == NULL && l != NULL; l = l->next) {
      g_object_get (l->data, "object", &videopad, NULL);
		//Aquí habría que volver a mostrar la ventana
    }
  }

  gvc->priv->media_has_audio = FALSE;
  if (audio_streams) {
    gvc->priv->media_has_audio = TRUE;
    if (!gvc->priv->media_has_video && gvc->priv->video_window) {
      //Aquí habría que ocultar la ventana      
    }
  }

  if (videopad) {
    GstCaps *caps;

    if ((caps = gst_pad_get_negotiated_caps (videopad))) {
      caps_set (G_OBJECT (videopad), NULL, gvc);
      gst_caps_unref (caps);
    }
    g_signal_connect (videopad, "notify::caps",
        G_CALLBACK (caps_set), gvc);
    gst_object_unref (videopad);
  } 

  g_list_foreach (audio_streams, (GFunc) g_object_unref, NULL);
  g_list_free (audio_streams);
  g_list_foreach (video_streams, (GFunc) g_object_unref, NULL);
  g_list_free (video_streams);
}

static void
playbin_stream_info_notify_cb (GObject * obj, GParamSpec * pspec, gpointer data)
{
  BaconVideoWidget *gvc = BACON_VIDEO_WIDGET (data);
  GstMessage *msg;

  /* we're being called from the streaming thread, so don't do anything here */
  GST_LOG ("stream info changed");
  msg = gst_message_new_application (GST_OBJECT (gvc->priv->play),
      gst_structure_new ("notify-streaminfo", NULL));
  gst_element_post_message (gvc->priv->play, msg);
}

static void
bacon_video_widget_finalize (GObject * object)
{
  BaconVideoWidget *gvc = (BaconVideoWidget *) object;

  GST_INFO ("finalizing");

  if (gvc->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
     * called again (main loop might be run again to display error dialog) */
    gst_bus_set_flushing (gvc->priv->bus, TRUE);

    if (gvc->priv->sig_bus_sync)
      g_signal_handler_disconnect (gvc->priv->bus, gvc->priv->sig_bus_sync);

    if (gvc->priv->sig_bus_async)
      g_signal_handler_disconnect (gvc->priv->bus, gvc->priv->sig_bus_async);

    gst_object_unref (gvc->priv->bus);
    gvc->priv->bus = NULL;
  }

  g_free (gvc->priv->media_device);
  gvc->priv->media_device = NULL;
    
  g_free (gvc->priv->mrl);
  gvc->priv->mrl = NULL;
  
  
 
  

  if (gvc->priv->play != NULL && GST_IS_ELEMENT (gvc->priv->play)) {
    gst_element_set_state (gvc->priv->play, GST_STATE_NULL);
    gst_object_unref (gvc->priv->play);
    gvc->priv->play = NULL;
  }

  if (gvc->priv->update_id) {
    g_source_remove (gvc->priv->update_id);
    gvc->priv->update_id = 0;
  }

  if (gvc->priv->tagcache) {
    gst_tag_list_free (gvc->priv->tagcache);
    gvc->priv->tagcache = NULL;
  }
  if (gvc->priv->audiotags) {
    gst_tag_list_free (gvc->priv->audiotags);
    gvc->priv->audiotags = NULL;
  }
  if (gvc->priv->videotags) {
    gst_tag_list_free (gvc->priv->videotags);
    gvc->priv->videotags = NULL;
  }

  if (gvc->priv->eos_id != 0)
    g_source_remove (gvc->priv->eos_id);

  g_mutex_free (gvc->priv->lock);



  G_OBJECT_CLASS (parent_class)->finalize (object);
}

static void
bacon_video_widget_set_property (GObject * object, guint property_id,
                                 const GValue * value, GParamSpec * pspec)
{
  BaconVideoWidget *gvc;

  gvc = BACON_VIDEO_WIDGET (object);

  switch (property_id) {
    case PROP_LOGO_MODE:
      bacon_video_widget_set_logo_mode (gvc,
      g_value_get_boolean (value));
      break;
    case PROP_SHOWCURSOR:
      bacon_video_widget_set_show_cursor (gvc,
      g_value_get_boolean (value));
      break;
    case PROP_MEDIADEV:
      bacon_video_widget_set_media_device (gvc,
      g_value_get_string (value));
      break;

    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

static void
bacon_video_widget_get_property (GObject * object, guint property_id,
                                 GValue * value, GParamSpec * pspec)
{
  BaconVideoWidget *gvc;

  gvc = BACON_VIDEO_WIDGET (object);

  switch (property_id) {
    case PROP_LOGO_MODE:
      g_value_set_boolean (value,
      bacon_video_widget_get_logo_mode (gvc));
      break;
    case PROP_POSITION:
      g_value_set_int64 (value, bacon_video_widget_get_position (gvc));
      break;
    case PROP_STREAM_LENGTH:
      g_value_set_int64 (value,
      bacon_video_widget_get_stream_length (gvc));
      break;
    case PROP_PLAYING:
      g_value_set_boolean (value,
      bacon_video_widget_is_playing (gvc));
      break;
    case PROP_SEEKABLE:
      g_value_set_boolean (value,
      bacon_video_widget_is_seekable (gvc));
      break;
    case PROP_SHOWCURSOR:
      g_value_set_boolean (value,
      bacon_video_widget_get_show_cursor (gvc));
      break;
    case PROP_MEDIADEV:
      g_value_set_string (value, gvc->priv->media_device);
      break;
    case PROP_VOLUME:
      g_value_set_int (value, bacon_video_widget_get_volume (gvc));
      break;
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

/* ============================================================= */
/*                                                               */
/*                       Public Methods                          */
/*                                                               */
/* ============================================================= */

char *
bacon_video_widget_get_backend_name (BaconVideoWidget * gvc)
{
  return gst_version_string ();
}

static GList *
get_stream_info_objects_for_type (BaconVideoWidget * gvc, const gchar * typestr)
{
  GValueArray *info_arr = NULL;
  GList *ret = NULL;
  guint i;

  if (gvc->priv->play == NULL || gvc->priv->mrl == NULL)
    return NULL;

  g_object_get (gvc->priv->play, "stream-info-value-array", &info_arr, NULL);
  if (info_arr == NULL)
    return NULL;

  for (i = 0; i < info_arr->n_values; ++i) {
    GObject *info_obj;
    GValue *val;

    val = g_value_array_get_nth (info_arr, i);
    info_obj = g_value_get_object (val);
    if (info_obj) {
      GParamSpec *pspec;
      GEnumValue *val;
      gint type = -1;

      g_object_get (info_obj, "type", &type, NULL);
      pspec = g_object_class_find_property (G_OBJECT_GET_CLASS (info_obj), "type");
      val = g_enum_get_value (G_PARAM_SPEC_ENUM (pspec)->enum_class, type);
      if (val) {
        if (g_ascii_strcasecmp (val->value_nick, typestr) == 0 ||
            g_ascii_strcasecmp (val->value_name, typestr) == 0) {
          ret = g_list_prepend (ret, g_object_ref (info_obj));
        }
      }
    }
  }
  g_value_array_free (info_arr);

  return g_list_reverse (ret);
}








/* =========================================== */
/*                                             */
/*               Play/Pause, Stop              */
/*                                             */
/* =========================================== */

static GError*
gvc_error_from_gst_error (BaconVideoWidget *gvc, GstMessage * err_msg)
{
  const gchar *src_typename;
  GError *ret = NULL;
  GError *e = NULL;

  GST_LOG ("resolving error message %" GST_PTR_FORMAT, err_msg);

  src_typename = (err_msg->src) ? G_OBJECT_TYPE_NAME (err_msg->src) : NULL;

  gst_message_parse_error (err_msg, &e, NULL);

  if (is_error (e, RESOURCE, NOT_FOUND) ||
      is_error (e, RESOURCE, OPEN_READ)) {
#if 0
    if (strchr (mrl, ':') &&
        (g_str_has_prefix (mrl, "dvd") ||
         g_str_has_prefix (mrl, "cd") ||
         g_str_has_prefix (mrl, "vcd"))) {
      ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_INVALID_DEVICE,
                                 e->message);
    } else {
#endif
      if (e->code == GST_RESOURCE_ERROR_NOT_FOUND) {
        if (GST_IS_BASE_AUDIO_SINK (err_msg->src)) {
          ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_AUDIO_PLUGIN,
              _("The requested audio output was not found. "
                "Please select another audio output in the Multimedia "
                "Systems Selector."));
        } else {
          ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_FILE_NOT_FOUND,
                                     _("Location not found."));
        }
      } else {
        ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_FILE_PERMISSION,
            _("Could not open location; "
              "You may not have permission to open the file."));
      }
#if 0
    }
#endif
  } else if (is_error (e, RESOURCE, BUSY)) {
    if (GST_IS_VIDEO_SINK (err_msg->src)) {
      /* a somewhat evil check, but hey.. */
      ret = g_error_new_literal (gvc_ERROR,
          gvc_ERROR_VIDEO_PLUGIN,
          _("The video output is in use by another application. "
            "Please close other video applications, or select "
            "another video output in the Multimedia Systems Selector."));
    } else if (GST_IS_BASE_AUDIO_SINK (err_msg->src)) {
      ret = g_error_new_literal (gvc_ERROR,
          gvc_ERROR_AUDIO_BUSY,
           _("The audio output is in use by another application. "
             "Please select another audio output in the Multimedia Systems Selector. "
             "You may want to consider using a sound server."));
    }
  } else if (e->domain == GST_RESOURCE_ERROR) {
    ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_FILE_GENERIC,
                               e->message);
  } else if (is_error (e, CORE, MISSING_PLUGIN) ||
             is_error (e, STREAM, CODEC_NOT_FOUND)) {
    if (gvc->priv->missing_plugins != NULL) {
      gchar **descs, *msg = NULL;
      guint num;

      descs = gvc_get_missing_plugins_descriptions (gvc->priv->missing_plugins);
      num = g_list_length (gvc->priv->missing_plugins);

      if (is_error (e, CORE, MISSING_PLUGIN)) {
        /* should be exactly one missing thing (source or converter) */
        msg = g_strdup_printf (_("The playback of this movie requires a %s "
          "plugin which is not installed."), descs[0]);
      } else {
        gchar *desc_list;

        desc_list = g_strjoinv ("\n", descs);
        msg = g_strdup_printf (ngettext (_("The playback of this movie "
            "requires a %s plugin which is not installed."), _("The playback "
            "of this movie requires the following decoders which are not "
            "installed:\n\n%s"), num), (num == 1) ? descs[0] : desc_list);
        g_free (desc_list);
      }
      ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_CODEC_NOT_HANDLED, msg);
      g_free (msg);
      g_strfreev (descs);
    } else {
      GST_LOG ("no missing plugin messages, posting generic error");
      ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_CODEC_NOT_HANDLED,
          e->message);
    }
  } else if (is_error (e, STREAM, WRONG_TYPE) ||
             is_error (e, STREAM, NOT_IMPLEMENTED)) {
    if (src_typename) {
      ret = g_error_new (gvc_ERROR, gvc_ERROR_CODEC_NOT_HANDLED, "%s: %s",
          src_typename, e->message);
    } else {
      ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_CODEC_NOT_HANDLED,
          e->message);
    }
  } else if (is_error (e, STREAM, FAILED) &&
             src_typename && strncmp (src_typename, "GstTypeFind", 11) == 0) {
    ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_READ_ERROR,
        _("Cannot play this file over the network. "
          "Try downloading it to disk first."));
  } else {
    /* generic error, no code; take message */
    ret = g_error_new_literal (gvc_ERROR, gvc_ERROR_GENERIC,
                               e->message);
  }
  g_error_free (e);
  gvc_clear_missing_plugins_messages (gvc);

  return ret;
}



static gboolean
poll_for_state_change_full (BaconVideoWidget *gvc, GstElement *element,
    GstState state, GstMessage ** err_msg, gint64 timeout)
{
  GstBus *bus;
  GstMessageType events, saved_events;

  g_assert (err_msg != NULL);

  bus = gst_element_get_bus (element);

  events = GST_MESSAGE_STATE_CHANGED | GST_MESSAGE_ERROR | GST_MESSAGE_EOS;

  saved_events = gvc->priv->ignore_messages_mask;

  if (element != NULL && element == gvc->priv->play) {
    /* we do want the main handler to process state changed messages for
     * playbin as well, otherwise it won't hook up the timeout etc. */
    gvc->priv->ignore_messages_mask |= (events ^ GST_MESSAGE_STATE_CHANGED);
  } else {
    gvc->priv->ignore_messages_mask |= events;
  }

  while (TRUE) {
    GstMessage *message;
    GstElement *src;

    message = gst_bus_poll (bus, events, timeout);
    
    if (!message)
      goto timed_out;
    
    src = (GstElement*)GST_MESSAGE_SRC (message);

    switch (GST_MESSAGE_TYPE (message)) {
    case GST_MESSAGE_STATE_CHANGED: {
      GstState old, new, pending;

      if (src == element) {
        gst_message_parse_state_changed (message, &old, &new, &pending);
        if (new == state) {
          gst_message_unref (message);
          goto success;
        }
      }
      break;
    }
    case GST_MESSAGE_ERROR: {
      gvc_error_msg (gvc, message);
      *err_msg = message;
      message = NULL;
      goto error;
      break;
    }
    case GST_MESSAGE_EOS: {
      GError *e = NULL;

      gst_message_unref (message);
      e = g_error_new_literal (gvc_ERROR, gvc_ERROR_FILE_GENERIC,
          _("Media file could not be played."));
      *err_msg = gst_message_new_error (GST_OBJECT (gvc->priv->play), e, NULL);
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
  /* state change succeeded */
  GST_INFO ("state change to %s succeeded", gst_element_state_get_name (state));
  gvc->priv->ignore_messages_mask = saved_events;
  return TRUE;

timed_out:
  /* it's taking a long time to open -- just tell totem it was ok, this allows
   * the user to stop the loading process with the normal stop button */
  GST_INFO ("state change to %s timed out, returning success and handling "
      "errors asynchronously", gst_element_state_get_name (state));
  gvc->priv->ignore_messages_mask = saved_events;
  return TRUE;

error:
  GST_INFO ("error while waiting for state change to %s: %" GST_PTR_FORMAT,
      gst_element_state_get_name (state), *err_msg);
  /* already set *err_msg */
  gvc->priv->ignore_messages_mask = saved_events;
  return FALSE;
}

gboolean
bacon_video_widget_open(BaconVideoWidget * gvc,
     const gchar * mrl, GError **error)
{
  
  GstMessage *err_msg = NULL;
  gboolean ret;

  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (mrl != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (gvc->priv->play != NULL, FALSE);
  
  
  /* So we aren't closed yet... */
  if (gvc->priv->mrl) {
    bacon_video_widget_close (gvc);
  }
  
  GST_INFO ("mrl = %s", GST_STR_NULL (mrl));


  /* hmm... */
  	if (gvc->priv->mrl && strcmp (gvc->priv->mrl, mrl) == 0) {
    	GST_INFO ("same as current mrl");
    	/* FIXME: shouldn't we ensure playing state here? */
    	return TRUE;
  	}
  

  	/* this allows non-URI type of files in the thumbnailer and so on */
  	g_free (gvc->priv->mrl);
  	if (mrl[0] == '/') {
   	 gvc->priv->mrl = g_strdup_printf ("file://%s", mrl);
  	} else {
    	if (strchr (mrl, ':')) {
      	gvc->priv->mrl = g_strdup (mrl);
    	} else {
      	gchar *cur_dir = g_get_current_dir ();

      	if (!cur_dir) {
        	g_set_error (error, gvc_ERROR, gvc_ERROR_GENERIC,
                     _("Failed to retrieve working directory"));
        	return FALSE;
      	}
      	gvc->priv->mrl = g_strdup_printf ("file://%s/%s", cur_dir, mrl);
      	g_free (cur_dir);
    	}
  	}


  /* No path? Choke on your errors already! */
  if (gvc->priv->mrl == NULL)
    gvc->priv->mrl = g_strdup (mrl);

  if (g_str_has_prefix (mrl, "icy:") != FALSE) {
    /* Handle "icy://" URLs from QuickTime */
    g_free (gvc->priv->mrl);
    gvc->priv->mrl = g_strdup_printf ("http:%s", mrl + 4);
  } else if (g_str_has_prefix (mrl, "icyx:") != FALSE) {
    /* Handle "icyx://" URLs from Orban/Coding Technologies AAC/aacPlus Player */
    g_free (gvc->priv->mrl);
    gvc->priv->mrl = g_strdup_printf ("http:%s", mrl + 5);
  } else if (g_str_has_prefix (mrl, "dvd:///")) {
    /* this allows to play backups of dvds */
    g_free (gvc->priv->mrl);
    gvc->priv->mrl = g_strdup ("dvd://");
    g_free (gvc->priv->media_device);
    gvc->priv->media_device = g_strdup (mrl + strlen ("dvd://"));
  } else if (g_str_has_prefix (mrl, "vcd:///")) {
    /* this allows to play backups of vcds */
    g_free (gvc->priv->mrl);
    gvc->priv->mrl = g_strdup ("vcd://");
    g_free (gvc->priv->media_device);
    gvc->priv->media_device = g_strdup (mrl + strlen ("vcd://"));
  }

  
  	gvc->priv->got_redirect = FALSE;
  	gvc->priv->media_has_video = FALSE;
  	gvc->priv->media_has_audio = FALSE;
  	gvc->priv->stream_length = 0;
  	gvc->priv->ignore_messages_mask = 0;  
 	g_object_set (gvc->priv->play, "uri", gvc->priv->mrl,NULL);

 

  gvc->priv->seekable = -1;
  gvc->priv->target_state = GST_STATE_PAUSED;
  gvc_clear_missing_plugins_messages (gvc);

  gst_element_set_state (gvc->priv->play, GST_STATE_PAUSED);

  if (gvc->priv->use_type == gvc_USE_TYPE_AUDIO ||
      gvc->priv->use_type == gvc_USE_TYPE_VIDEO) {
    GST_INFO ("normal playback, handling all errors asynchroneously");
    ret = TRUE;
  } else {
    /* used as thumbnailer or metadata extractor for properties dialog. In
     * this case, wait for any state change to really finish and process any
     * pending tag messages, so that the information is available right away */
    GST_INFO ("waiting for state changed to PAUSED to complete");
    ret = poll_for_state_change_full (gvc, gvc->priv->play,
        GST_STATE_PAUSED, &err_msg, -1);

    gvc_process_pending_tag_messages (gvc);
    bacon_video_widget_get_stream_length (gvc);
    GST_INFO ("stream length = %u", gvc->priv->stream_length);

    /* even in case of an error (e.g. no decoders installed) we might still
     * have useful metadata (like codec types, duration, etc.) */
    g_signal_emit (gvc, gvc_signals[SIGNAL_GOT_METADATA], 0, NULL);
  }

  if (ret) {
    g_signal_emit (gvc, gvc_signals[SIGNAL_CHANNELS_CHANGE], 0);
  } else {
    GST_INFO ("Error on open: %" GST_PTR_FORMAT, err_msg);
    if (gvc_check_missing_plugins_error (gvc, err_msg)) {
      /* totem will try to start playing, so ignore all messages on the bus */
      gvc->priv->ignore_messages_mask |= GST_MESSAGE_ERROR;
      GST_LOG ("missing plugins handled, ignoring error and returning TRUE");
      gst_message_unref (err_msg);
      err_msg = NULL;
      ret = TRUE;
    } else {
      gvc->priv->ignore_messages_mask |= GST_MESSAGE_ERROR;
      gvc_stop_play_pipeline (gvc);
      g_free (gvc->priv->mrl);
      gvc->priv->mrl = NULL;
    }
  }

  /* When opening a new media we want to redraw ourselves */
  gtk_widget_queue_draw (GTK_WIDGET (gvc->priv->video_window));

  if (err_msg != NULL) {
    if (error) {
      *error = gvc_error_from_gst_error (gvc, err_msg);

    } else {
      GST_WARNING ("Got error, but caller is not collecting error details!");
    }
    gst_message_unref (err_msg);
  }
    
  
  return ret;
}

gboolean
bacon_video_widget_play (BaconVideoWidget * gvc)
{
  
  GstState cur_state;

  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);
  g_return_val_if_fail (gvc->priv->mrl != NULL, FALSE);

  gvc->priv->target_state = GST_STATE_PLAYING;

  /* no need to actually go into PLAYING in capture/metadata mode (esp.
   * not with sinks that don't sync to the clock), we'll get everything
   * we need by prerolling the pipeline, and that is done in _open() */
  if (gvc->priv->use_type == gvc_USE_TYPE_CAPTURE ||
      gvc->priv->use_type == gvc_USE_TYPE_METADATA) {
    return TRUE;
  }

  /* just lie and do nothing in this case */
  gst_element_get_state (gvc->priv->play, &cur_state, NULL, 0);
  if (gvc->priv->plugin_install_in_progress && cur_state != GST_STATE_PAUSED) {
    GST_INFO ("plugin install in progress and nothing to play, doing nothing");
    return TRUE;
  }

  GST_INFO ("play");
  gst_element_set_state (gvc->priv->play, GST_STATE_PLAYING);

  /* will handle all errors asynchroneously */
  return TRUE;
}

gboolean
bacon_video_widget_can_direct_seek (BaconVideoWidget *gvc)
{
  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  g_return_val_if_fail (gvc != NULL, FALSE);

	if (gvc->priv->mrl == NULL)
		return FALSE;

	/* (instant seeking only make sense with video,
	 * hence no cdda:// here) */
	if (g_str_has_prefix (gvc->priv->mrl, "file://") ||
			g_str_has_prefix (gvc->priv->mrl, "dvd://") ||
			g_str_has_prefix (gvc->priv->mrl, "vcd://"))
		return TRUE;

	return FALSE;
}

//If we want to seek throug a seekbar we want speed, so we use the KEY_UNIT flag
//Sometimes accurate position is requested so we use the ACCURATE flag
gboolean
bacon_video_widget_seek_time (BaconVideoWidget *gvc, gint64 time, gboolean accurate)
{


  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  GST_LOG ("Seeking to %" GST_TIME_FORMAT, GST_TIME_ARGS (time * GST_MSECOND));
  
  if (time > gvc->priv->stream_length
      && gvc->priv->stream_length > 0
      && !g_str_has_prefix (gvc->priv->mrl, "dvd:")
      && !g_str_has_prefix (gvc->priv->mrl, "vcd:")) {
    if (gvc->priv->eos_id == 0)
      gvc->priv->eos_id = g_idle_add (gvc_signal_eos_delayed, gvc);
    return TRUE;
  }


  if(accurate)
  	gst_element_seek (gvc->priv->play, 1.0,
      	GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_ACCURATE,
      	GST_SEEK_TYPE_SET, time * GST_MSECOND,
      	GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
  else {
  
	 /* Emit a time tick of where we are going, we are paused */

  	got_time_tick (gvc->priv->play, time * GST_MSECOND, gvc);
	gst_element_seek (gvc->priv->play, 1.0,
      	GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_KEY_UNIT,
      	GST_SEEK_TYPE_SET, time * GST_MSECOND,
      	GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
  }
  
  gst_element_get_state (gvc->priv->play, NULL, NULL, 100 * GST_MSECOND);

  return TRUE;
}





gboolean
bacon_video_widget_seek (BaconVideoWidget *gvc, float position )
{

  gint64 seek_time, length_nanos;

  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  length_nanos = (gint64) (gvc->priv->stream_length * GST_MSECOND);
  seek_time = (gint64) (length_nanos * position);

  GST_LOG ("Seeking to %3.2f%% %" GST_TIME_FORMAT, position,
      GST_TIME_ARGS (seek_time));

  return bacon_video_widget_seek_time (gvc, seek_time / GST_MSECOND, FALSE);
}

gboolean
bacon_video_widget_seek_in_segment (BaconVideoWidget *gvc, gint64 pos )
{

  	g_return_val_if_fail (gvc != NULL, FALSE);
  	g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  	g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  	GST_LOG ("Segment seeking from %" GST_TIME_FORMAT, GST_TIME_ARGS (pos * GST_MSECOND));
  
  	if (pos > gvc->priv->stream_length
      	&& gvc->priv->stream_length > 0
      	&& !g_str_has_prefix (gvc->priv->mrl, "dvd:")
     	&& !g_str_has_prefix (gvc->priv->mrl, "vcd:")) {
    	if (gvc->priv->eos_id == 0)
     		gvc->priv->eos_id = g_idle_add (gvc_signal_eos_delayed, gvc);
    	return TRUE;
  	}

	got_time_tick (gvc->priv->play, pos * GST_MSECOND, gvc);
	gst_element_seek (gvc->priv->play, 1.0,
      	GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT | GST_SEEK_FLAG_ACCURATE,
      	GST_SEEK_TYPE_SET, pos * GST_MSECOND,
      	GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
    
    return TRUE;
}

gboolean
bacon_video_widget_set_rate_in_segment (BaconVideoWidget *gvc, gfloat rate, gint64 stop)
{

  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  //GST_LOG ("Seeking to %" GST_TIME_FORMAT, GST_TIME_ARGS (time * GST_MSECOND));
 

   gst_element_seek (gvc->priv->play, rate,
      	GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_ACCURATE | GST_SEEK_FLAG_SEGMENT,
      	GST_SEEK_TYPE_SET,bacon_video_widget_get_accurate_current_time(gvc) * GST_MSECOND,
      	GST_SEEK_TYPE_SET, stop * GST_MSECOND);
      
  return TRUE;
}

gboolean
bacon_video_widget_set_rate (BaconVideoWidget *gvc, gfloat rate)
{

  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  //GST_LOG ("Seeking to %" GST_TIME_FORMAT, GST_TIME_ARGS (time * GST_MSECOND));
 

   gst_element_seek (gvc->priv->play, rate,
      	GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_ACCURATE,
      	GST_SEEK_TYPE_SET,bacon_video_widget_get_accurate_current_time(gvc) * GST_MSECOND,
      	GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
      
  return TRUE;
}

 
gboolean 
bacon_video_widget_new_file_seek (BaconVideoWidget *gvc,gint64 start,gint64 stop)
{
	
  	g_return_val_if_fail (gvc != NULL, FALSE);
  	g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  	g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

	
  	GST_LOG ("Segment seeking from %" GST_TIME_FORMAT, GST_TIME_ARGS (start * GST_MSECOND));
  	
  	GstState cur_state;
  
  	
  	if (start > gvc->priv->stream_length
      	&& gvc->priv->stream_length > 0
      	&& !g_str_has_prefix (gvc->priv->mrl, "dvd:")
     	&& !g_str_has_prefix (gvc->priv->mrl, "vcd:")) {
    	if (gvc->priv->eos_id == 0)
     		gvc->priv->eos_id = g_idle_add (gvc_signal_eos_delayed, gvc);
    	return TRUE;
  	}
	 
		do{
			 gst_element_get_state (gvc->priv->play, &cur_state, NULL, 0);

		}while(cur_state <= GST_STATE_READY);
       
		got_time_tick (gvc->priv->play, start * GST_MSECOND, gvc);
		gst_element_seek (gvc->priv->play, 1.0,
      	GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT | GST_SEEK_FLAG_ACCURATE,
      	GST_SEEK_TYPE_SET, start * GST_MSECOND,
      	GST_SEEK_TYPE_SET, stop * GST_MSECOND);
    
    return TRUE;
  }

gboolean 
bacon_video_widget_segment_seek (BaconVideoWidget *gvc,gint64 start,gint64 stop)
{
	
  	g_return_val_if_fail (gvc != NULL, FALSE);
  	g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  	g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  	GST_LOG ("Segment seeking from %" GST_TIME_FORMAT, GST_TIME_ARGS (start * GST_MSECOND));
  
  	
  	if (start > gvc->priv->stream_length
      	&& gvc->priv->stream_length > 0
      	&& !g_str_has_prefix (gvc->priv->mrl, "dvd:")
     	&& !g_str_has_prefix (gvc->priv->mrl, "vcd:")) {
    	if (gvc->priv->eos_id == 0)
     		gvc->priv->eos_id = g_idle_add (gvc_signal_eos_delayed, gvc);
    	return TRUE;
  	}
	
	got_time_tick (gvc->priv->play, start * GST_MSECOND, gvc);
	gst_element_seek (gvc->priv->play, 1.0,
      	GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT | GST_SEEK_FLAG_ACCURATE,
      	GST_SEEK_TYPE_SET, start * GST_MSECOND,
      	GST_SEEK_TYPE_SET, stop * GST_MSECOND);
    
    return TRUE;
  }
  
 gboolean 
 bacon_video_widget_segment_stop_update(BaconVideoWidget *gvc, gint64 stop)
 {
	 	g_return_val_if_fail (gvc != NULL, FALSE);
  		g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  		g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);
	 	gst_element_seek (gvc->priv->play, 1.0,
      	GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT | GST_SEEK_FLAG_ACCURATE,
      	GST_SEEK_TYPE_SET, stop * GST_MSECOND-1,
      	GST_SEEK_TYPE_SET, stop * GST_MSECOND);
      	return TRUE;
 }
 gboolean 
 bacon_video_widget_segment_start_update(BaconVideoWidget *gvc,gint64 start)
 {
	g_return_val_if_fail (gvc != NULL, FALSE);
  	g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  	g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);
  	
  	gst_element_seek (gvc->priv->play, 1.0,
      	GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH | GST_SEEK_FLAG_SEGMENT | GST_SEEK_FLAG_ACCURATE,
      	GST_SEEK_TYPE_SET, start * GST_MSECOND,
      	GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
  	

  	
  	return TRUE;
	  
 }


static void
gvc_stop_play_pipeline (BaconVideoWidget * gvc)
{
  GstState cur_state;

  gst_element_get_state (gvc->priv->play, &cur_state, NULL, 0);
  if (cur_state > GST_STATE_READY) {
    GstMessage *msg;
    GstBus *bus;

    GST_INFO ("stopping");
    gst_element_set_state (gvc->priv->play, GST_STATE_READY);

    /* process all remaining state-change messages so everything gets
     * cleaned up properly (before the state change to NULL flushes them) */
    GST_INFO ("processing pending state-change messages");
    bus = gst_element_get_bus (gvc->priv->play);
    while ((msg = gst_bus_poll (bus, GST_MESSAGE_STATE_CHANGED, 0))) {
      gst_bus_async_signal_func (bus, msg, NULL);
      gst_message_unref (msg);
    }
    gst_object_unref (bus);
  }

  gst_element_set_state (gvc->priv->play, GST_STATE_NULL);
  gvc->priv->target_state = GST_STATE_NULL;
  gvc->priv->buffering = FALSE;
  gvc->priv->plugin_install_in_progress = FALSE;
  gvc->priv->ignore_messages_mask = 0;
  GST_INFO ("stopped");
}

void
bacon_video_widget_stop (BaconVideoWidget * gvc)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  g_return_if_fail (GST_IS_ELEMENT (gvc->priv->play));

  GST_LOG ("Stopping");
  gvc_stop_play_pipeline (gvc);
  
  /* Reset position to 0 when stopping */
  got_time_tick (GST_ELEMENT (gvc->priv->play), 0, gvc);
}

void
bacon_video_widget_close (BaconVideoWidget * gvc)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  g_return_if_fail (GST_IS_ELEMENT (gvc->priv->play));
  
  GST_LOG ("Closing");
  gvc_stop_play_pipeline (gvc);

  if (gvc->priv->mrl) {
    g_free (gvc->priv->mrl);
    gvc->priv->mrl = NULL;
  }

  g_signal_emit (gvc, gvc_signals[SIGNAL_CHANNELS_CHANGE], 0);
}


void
bacon_video_widget_set_logo (BaconVideoWidget * gvc, gchar * filename)
{
  GError *error = NULL;

  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  g_return_if_fail (filename != NULL);

  if (gvc->priv->logo_pixbuf != NULL)
    g_object_unref (gvc->priv->logo_pixbuf);

  gvc->priv->logo_pixbuf = gdk_pixbuf_new_from_file (filename, &error);

  if (error) {
    g_warning ("An error occurred trying to open logo %s: %s",
               filename, error->message);
    g_error_free (error);
  }
  else {
	  gst_video_widget_set_logo(GST_VIDEO_WIDGET(gvc->priv->video_window),gvc->priv->logo_pixbuf);
}
}


void
bacon_video_widget_set_logo_mode (BaconVideoWidget * gvc, gboolean logo_mode)
{
	g_return_if_fail (gvc != NULL);
 	g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  	g_return_if_fail (gvc->priv->logo_pixbuf != NULL);
	
	gst_video_widget_set_logo_focus(GST_VIDEO_WIDGET(gvc->priv->video_window),logo_mode);
	gvc->priv->logo_mode = logo_mode;  
}
  


gboolean
bacon_video_widget_get_logo_mode (BaconVideoWidget * gvc)
{
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);

  return gvc->priv->logo_mode;
}

void
bacon_video_widget_pause (BaconVideoWidget * gvc)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  g_return_if_fail (GST_IS_ELEMENT (gvc->priv->play));
  g_return_if_fail (gvc->priv->mrl != NULL);

  GST_LOG ("Pausing");
  gst_element_set_state (GST_ELEMENT (gvc->priv->play), GST_STATE_PAUSED);
  gvc->priv->target_state = GST_STATE_PAUSED;
}

void
bacon_video_widget_set_subtitle_font (BaconVideoWidget * gvc,
                                          const gchar * font)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  g_return_if_fail (GST_IS_ELEMENT (gvc->priv->play));

  if (!g_object_class_find_property (G_OBJECT_GET_CLASS (gvc->priv->play), "subtitle-font-desc"))
    return;
  g_object_set (gvc->priv->play, "subtitle-font-desc", font, NULL);
}

void
bacon_video_widget_set_subtitle_encoding (BaconVideoWidget *gvc,
                                          const char *encoding)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  g_return_if_fail (GST_IS_ELEMENT (gvc->priv->play));

  if (!g_object_class_find_property (G_OBJECT_GET_CLASS (gvc->priv->play), "subtitle-encoding"))
    return;
  g_object_set (gvc->priv->play, "subtitle-encoding", encoding, NULL);
}

gboolean
bacon_video_widget_can_set_volume (BaconVideoWidget * gvc)
{
  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  if (gvc->priv->speakersetup == gvc_AUDIO_SOUND_AC3PASSTHRU)
    return FALSE;

  return !gvc->priv->uses_fakesink;
}

void
bacon_video_widget_set_volume (BaconVideoWidget * gvc, gint volume)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  g_return_if_fail (GST_IS_ELEMENT (gvc->priv->play));

  if (bacon_video_widget_can_set_volume (gvc) != FALSE)
  {
    volume = CLAMP (volume, 0, 100);
    g_object_set (gvc->priv->play, "volume",
                  (gdouble) (1. * volume / 100), NULL);
    g_object_notify (G_OBJECT (gvc), "volume");
  }
}

int
bacon_video_widget_get_volume (BaconVideoWidget * gvc)
{
  gdouble vol;

  g_return_val_if_fail (gvc != NULL, -1);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), -1);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), -1);

  g_object_get (G_OBJECT (gvc->priv->play), "volume", &vol, NULL);

  return (gint) (vol * 100 + 0.5);
}


void
bacon_video_widget_set_fullscreen (BaconVideoWidget * gvc,
                                   gboolean fullscreen)
{
 /* g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));

  if (gvc->priv->have_xvidmode == FALSE )
    return;

  gvc->priv->fullscreen_mode = fullscreen;

  if (fullscreen == FALSE){
	bacon_restore ();
	  
  } else if (gvc->priv->have_xvidmode != FALSE) {
    bacon_resize ();
	  
  }*/
}

void
bacon_video_widget_set_show_cursor (BaconVideoWidget * gvc,
                                    gboolean show_cursor)
{
  /*g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  
  gvc->priv->cursor_shown = show_cursor;
  
  if (!GTK_WIDGET (gvc)->window) {
    return;
  }

  if (show_cursor == FALSE) {
    totem_gdk_window_set_invisible_cursor (GTK_WIDGET (gvc)->window);
  } else {
    gdk_window_set_cursor (GTK_WIDGET (gvc)->window, NULL);
  }*/
}

gboolean
bacon_video_widget_get_show_cursor (BaconVideoWidget * gvc)
{
  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);

  return gvc->priv->cursor_shown;
}


void
bacon_video_widget_set_media_device (BaconVideoWidget * gvc, const char *path)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  g_return_if_fail (GST_IS_ELEMENT (gvc->priv->play));

  /* FIXME: totally not thread-safe, used in the notify::source callback */  
  g_free (gvc->priv->media_device);
  gvc->priv->media_device = g_strdup (path);
}







gboolean
bacon_video_widget_get_auto_resize (BaconVideoWidget * gvc)
{
  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);

  return gvc->priv->auto_resize;
}

void
bacon_video_widget_set_auto_resize (BaconVideoWidget * gvc,
                                    gboolean auto_resize)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));

  gvc->priv->auto_resize = auto_resize;

  /* this will take effect when the next media file loads */
}

void
bacon_video_widget_set_aspect_ratio (BaconVideoWidget *gvc,
                                BaconVideoWidgetAspectRatio ratio)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));

  gvc->priv->ratio_type = ratio;
  got_video_size (gvc);
}

BaconVideoWidgetAspectRatio
bacon_video_widget_get_aspect_ratio (BaconVideoWidget *gvc)
{
  g_return_val_if_fail (gvc != NULL, 0);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), 0);

  return gvc->priv->ratio_type;
}

void
bacon_video_widget_set_scale_ratio (BaconVideoWidget * gvc, gfloat ratio)
{
}

gboolean
bacon_video_widget_can_set_zoom (BaconVideoWidget *gvc)
{
  return FALSE;
}

void
bacon_video_widget_set_zoom (BaconVideoWidget *gvc,
                             int               zoom)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));

  /* implement me */
}

int
bacon_video_widget_get_zoom (BaconVideoWidget *gvc)
{
  g_return_val_if_fail (gvc != NULL, 100);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), 100);

  return 100;
}




float
bacon_video_widget_get_position (BaconVideoWidget * gvc)
{
  g_return_val_if_fail (gvc != NULL, -1);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), -1);
  return gvc->priv->current_position;
}

gint64
bacon_video_widget_get_current_time (BaconVideoWidget * gvc)
{
  g_return_val_if_fail (gvc != NULL, -1);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), -1);
  return gvc->priv->current_time;
}

gint64
bacon_video_widget_get_accurate_current_time(BaconVideoWidget *gvc)
{ 
	g_return_val_if_fail (gvc != NULL, -1);
  	g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), -1);

	GstFormat fmt = GST_FORMAT_TIME;
   	gint64 pos = -1;

  	gst_element_query_position(gvc->priv->play, &fmt, &pos);
  	
  	return pos/GST_MSECOND;
 
}

gint64
bacon_video_widget_get_stream_length (BaconVideoWidget * gvc)
{
  g_return_val_if_fail (gvc != NULL, -1);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), -1);

  if (gvc->priv->stream_length == 0 && gvc->priv->play != NULL) {
    GstFormat fmt = GST_FORMAT_TIME;
    gint64 len = -1;

    if (gst_element_query_duration (gvc->priv->play, &fmt, &len) && len != -1) {
      gvc->priv->stream_length = len / GST_MSECOND;
    }
  }

  return gvc->priv->stream_length;
}

gboolean
bacon_video_widget_is_playing (BaconVideoWidget * gvc)
{
  gboolean ret;

  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  ret = (gvc->priv->target_state == GST_STATE_PLAYING);
  GST_LOG ("%splaying", (ret) ? "" : "not ");

  return ret;
}

gboolean
bacon_video_widget_is_seekable (BaconVideoWidget * gvc)
{
  gboolean res;

  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  if (gvc->priv->seekable == -1) {
    GstQuery *query;

    query = gst_query_new_seeking (GST_FORMAT_TIME);
    if (gst_element_query (gvc->priv->play, query)) {
      gst_query_parse_seeking (query, NULL, &res, NULL, NULL);
      gvc->priv->seekable = (res) ? 1 : 0;
    } else {
      GST_INFO ("seeking query failed");
    }
    gst_query_unref (query);
  }

  if (gvc->priv->seekable != -1) {
    res = (gvc->priv->seekable != 0);
    goto done;
  }

  /* try to guess from duration (this is very unreliable though) */
  if (gvc->priv->stream_length == 0) {
    res = (bacon_video_widget_get_stream_length (gvc) > 0);
  } else {
    res = (gvc->priv->stream_length > 0);
  }

done:

  GST_INFO ("stream is%s seekable", (res) ? "" : " not");
  return res;
}



gchar *
bacon_video_widget_get_mrl (BaconVideoWidget * gvc)
{
	return g_strdup(gvc->priv->mrl);
}



static struct _metadata_map_info {
  BaconVideoWidgetMetadataType type;
  const gchar *str;
} metadata_str_map[] = {
  { gvc_INFO_TITLE, "title" },
  { gvc_INFO_ARTIST, "artist" },
  { gvc_INFO_YEAR, "year" },
  { gvc_INFO_ALBUM, "album" },
  { gvc_INFO_DURATION, "duration" },
  { gvc_INFO_TRACK_NUMBER, "track-number" },
  { gvc_INFO_HAS_VIDEO, "has-video" },
  { gvc_INFO_DIMENSION_X, "dimension-x" },
  { gvc_INFO_DIMENSION_Y, "dimension-y" },
  { gvc_INFO_VIDEO_BITRATE, "video-bitrate" },
  { gvc_INFO_VIDEO_CODEC, "video-codec" },
  { gvc_INFO_FPS, "fps" },
  { gvc_INFO_HAS_AUDIO, "has-audio" },
  { gvc_INFO_AUDIO_BITRATE, "audio-bitrate" },
  { gvc_INFO_AUDIO_CODEC, "audio-codec" },
  { gvc_INFO_AUDIO_SAMPLE_RATE, "samplerate" },
  { gvc_INFO_AUDIO_CHANNELS, "channels" }
};

static const gchar *
get_metadata_type_name (BaconVideoWidgetMetadataType type)
{
  guint i;
  for (i = 0; i < G_N_ELEMENTS (metadata_str_map); ++i) {
    if (metadata_str_map[i].type == type)
      return metadata_str_map[i].str;
  }
  return "unknown";
}

static GObject *
gvc_get_stream_info_of_current_stream (BaconVideoWidget * gvc,
    const gchar *stream_type)
{
  GObject *current_info;
  GList *streams;
  gchar *lower, *cur_prop_str;
  gint stream_num = -1;

  if (gvc->priv->play == NULL)
    return NULL;

  lower = g_ascii_strdown (stream_type, -1);
  cur_prop_str = g_strconcat ("current-", lower, NULL);
  g_object_get (gvc->priv->play, cur_prop_str, &stream_num, NULL);
  g_free (cur_prop_str);
  g_free (lower);

  GST_LOG ("current %s stream: %d", stream_type, stream_num);
  if (stream_num < 0)
    return NULL;

  streams = get_stream_info_objects_for_type (gvc, stream_type);
  current_info = g_list_nth_data (streams, stream_num);
  if (current_info != NULL)
    g_object_ref (current_info);
  g_list_foreach (streams, (GFunc) g_object_unref, NULL);
  g_list_free (streams);
  GST_LOG ("current %s stream info object %p", stream_type, current_info);
  return current_info;
}

static GstCaps *
gvc_get_caps_of_current_stream (BaconVideoWidget * gvc,
    const gchar *stream_type)
{
  GstCaps *caps = NULL;
  GObject *current;

  current = gvc_get_stream_info_of_current_stream (gvc, stream_type);
  if (current != NULL) {
    GstObject *obj = NULL;

    /* we get the caps from the pad here instead of using the "caps" property
     * directly since the latter will not give us fixed/negotiated caps
     * (playbin bug as of gst-plugins-base 0.10.10) */
    g_object_get (G_OBJECT (current), "object", &obj, NULL);
    if (obj) {
      if (GST_IS_PAD (obj)) {
        caps = gst_pad_get_negotiated_caps (GST_PAD_CAST (obj));
      }
      gst_object_unref (obj);
    }
    gst_object_unref (current);
  }
  GST_LOG ("current %s stream caps: %" GST_PTR_FORMAT, stream_type, caps);
  return caps;
}

static gboolean
audio_caps_have_LFE (GstStructure * s)
{
  GstAudioChannelPosition *positions;
  gint i, channels;

  if (!gst_structure_get_value (s, "channel-positions") ||
      !gst_structure_get_int (s, "channels", &channels)) {
    return FALSE;
  }

  positions = gst_audio_get_channel_positions (s);
  if (positions == NULL)
    return FALSE;

  for (i = 0; i < channels; ++i) {
    if (positions[i] == GST_AUDIO_CHANNEL_POSITION_LFE) {
      g_free (positions);
      return TRUE;
    }
  }

  g_free (positions);
  return FALSE;
}

static void
bacon_video_widget_get_metadata_string (BaconVideoWidget * gvc,
                                        BaconVideoWidgetMetadataType type,
                                        GValue * value)
{
  char *string = NULL;
  gboolean res = FALSE;

  g_value_init (value, G_TYPE_STRING);

  if (gvc->priv->play == NULL) {
    g_value_set_string (value, NULL);
    return;
  }

  switch (type) {
    case gvc_INFO_TITLE:
      if (gvc->priv->tagcache != NULL) {
        res = gst_tag_list_get_string_index (gvc->priv->tagcache,
                                             GST_TAG_TITLE, 0, &string);
      }
      break;
    case gvc_INFO_ARTIST:
      if (gvc->priv->tagcache != NULL) {
        res = gst_tag_list_get_string_index (gvc->priv->tagcache,
                                             GST_TAG_ARTIST, 0, &string);
      }
      break;
    case gvc_INFO_YEAR:
      if (gvc->priv->tagcache != NULL) {
        GDate *date;

        if ((res = gst_tag_list_get_date (gvc->priv->tagcache,
                                          GST_TAG_DATE, &date))) {
          string = g_strdup_printf ("%d", g_date_get_year (date));
          g_date_free (date);
        }
      }
      break;
    case gvc_INFO_ALBUM:
      if (gvc->priv->tagcache != NULL) {
        res = gst_tag_list_get_string_index (gvc->priv->tagcache,
                                             GST_TAG_ALBUM, 0, &string);
      }
      break;
    case gvc_INFO_VIDEO_CODEC: {
      GObject *info;

      /* try to get this from the stream info first */
      if ((info = gvc_get_stream_info_of_current_stream (gvc, "video"))) {
        g_object_get (info, "codec", &string, NULL);
        res = (string != NULL);
        gst_object_unref (info);
      }

      /* if that didn't work, try the aggregated tags */
      if (!res && gvc->priv->tagcache != NULL) {
        res = gst_tag_list_get_string (gvc->priv->tagcache,
            GST_TAG_VIDEO_CODEC, &string);
      }
      break;
    }
    case gvc_INFO_AUDIO_CODEC: {
      GObject *info;

      /* try to get this from the stream info first */
      if ((info = gvc_get_stream_info_of_current_stream (gvc, "audio"))) {
        g_object_get (info, "codec", &string, NULL);
        res = (string != NULL);
        gst_object_unref (info);
      }

      /* if that didn't work, try the aggregated tags */
      if (!res && gvc->priv->tagcache != NULL) {
        res = gst_tag_list_get_string (gvc->priv->tagcache,
            GST_TAG_AUDIO_CODEC, &string);
      }
      break;
    }
    case gvc_INFO_AUDIO_CHANNELS: {
      GstStructure *s;
      GstCaps *caps;

      caps = gvc_get_caps_of_current_stream (gvc, "audio");
      if (caps) {
        gint channels = 0;

        s = gst_caps_get_structure (caps, 0);
        if ((res = gst_structure_get_int (s, "channels", &channels))) {
          /* FIXME: do something more sophisticated - but what? */
          if (channels > 2 && audio_caps_have_LFE (s)) {
            string = g_strdup_printf ("%d.1", channels - 1);
          } else {
            string = g_strdup_printf ("%d", channels);
          }
        }
        gst_caps_unref (caps);
      }
      break;
    }
    default:
      g_assert_not_reached ();
    }

  if (res && string && g_utf8_validate (string, -1, NULL)) {
    g_value_take_string (value, string);
    GST_INFO ("%s = '%s'", get_metadata_type_name (type), string);
  } else {
    g_value_set_string (value, NULL);
    g_free (string);
  }

  return;
}

static void
bacon_video_widget_get_metadata_int (BaconVideoWidget * gvc,
                                     BaconVideoWidgetMetadataType type,
                                     GValue * value)
{
  int integer = 0;

  g_value_init (value, G_TYPE_INT);

  if (gvc->priv->play == NULL) {
    g_value_set_int (value, 0);
    return;
  }

  switch (type) {
    case gvc_INFO_DURATION:
      integer = bacon_video_widget_get_stream_length (gvc) / 1000;
      break;
    case gvc_INFO_TRACK_NUMBER:
      if (gvc->priv->tagcache == NULL)
        break;
      if (!gst_tag_list_get_uint (gvc->priv->tagcache,
                                  GST_TAG_TRACK_NUMBER, (guint *) &integer))
        integer = 0;
      break;
    case gvc_INFO_DIMENSION_X:
      integer = gvc->priv->video_width;
      break;
    case gvc_INFO_DIMENSION_Y:
      integer = gvc->priv->video_height;
      break;
    case gvc_INFO_FPS:
      if (gvc->priv->video_fps_d > 0) {
        /* Round up/down to the nearest integer framerate */
        integer = (gvc->priv->video_fps_n + gvc->priv->video_fps_d/2) /
                  gvc->priv->video_fps_d;
      }
      else{
	      g_print("mal");
        integer = 0;
       }
      break;
    case gvc_INFO_AUDIO_BITRATE:
      if (gvc->priv->audiotags == NULL)
        break;
      if (gst_tag_list_get_uint (gvc->priv->audiotags, GST_TAG_BITRATE,
          (guint *)&integer) ||
          gst_tag_list_get_uint (gvc->priv->audiotags, GST_TAG_NOMINAL_BITRATE,
          (guint *)&integer)) {
        integer /= 1000;
      }
      break;
    case gvc_INFO_VIDEO_BITRATE:
      if (gvc->priv->videotags == NULL)
        break;
      if (gst_tag_list_get_uint (gvc->priv->videotags, GST_TAG_BITRATE,
          (guint *)&integer) ||
          gst_tag_list_get_uint (gvc->priv->videotags, GST_TAG_NOMINAL_BITRATE,
          (guint *)&integer)) {
        integer /= 1000;
      }
      break;
    case gvc_INFO_AUDIO_SAMPLE_RATE: {
      GstStructure *s;
      GstCaps *caps;

      caps = gvc_get_caps_of_current_stream (gvc, "audio");
      if (caps) {
        s = gst_caps_get_structure (caps, 0);
        gst_structure_get_int (s, "rate", &integer);
        gst_caps_unref (caps);
      }
      break;
    }
    default:
      g_assert_not_reached ();
    }

  g_value_set_int (value, integer);
  GST_INFO ("%s = %d", get_metadata_type_name (type), integer);

  return;
}

static void
bacon_video_widget_get_metadata_bool (BaconVideoWidget * gvc,
                                      BaconVideoWidgetMetadataType type,
                                      GValue * value)
{
  gboolean boolean = FALSE;

  g_value_init (value, G_TYPE_BOOLEAN);

  if (gvc->priv->play == NULL) {
    g_value_set_boolean (value, FALSE);
    return;
  }

  GST_INFO ("tagcache  = %" GST_PTR_FORMAT, gvc->priv->tagcache);
  GST_INFO ("videotags = %" GST_PTR_FORMAT, gvc->priv->videotags);
  GST_INFO ("audiotags = %" GST_PTR_FORMAT, gvc->priv->audiotags);

  switch (type)
  {
    case gvc_INFO_HAS_VIDEO:
      boolean = gvc->priv->media_has_video;
      /* if properties dialog, show the metadata we
       * have even if we cannot decode the stream */
      if (!boolean && gvc->priv->use_type == gvc_USE_TYPE_METADATA &&
          gvc->priv->tagcache != NULL &&
          gst_structure_has_field ((GstStructure *) gvc->priv->tagcache,
                                   GST_TAG_VIDEO_CODEC)) {
        boolean = TRUE;
      }
      break;
    case gvc_INFO_HAS_AUDIO:
      boolean = gvc->priv->media_has_audio;
      /* if properties dialog, show the metadata we
       * have even if we cannot decode the stream */
      if (!boolean && gvc->priv->use_type == gvc_USE_TYPE_METADATA &&
          gvc->priv->tagcache != NULL &&
          gst_structure_has_field ((GstStructure *) gvc->priv->tagcache,
                                   GST_TAG_AUDIO_CODEC)) {
        boolean = TRUE;
      }
      break;
    default:
      g_assert_not_reached ();
  }

  g_value_set_boolean (value, boolean);
  GST_INFO ("%s = %s", get_metadata_type_name (type), (boolean) ? "yes" : "no");

  return;
}

static void
gvc_process_pending_tag_messages (BaconVideoWidget * gvc)
{
  GstMessageType events;
  GstMessage *msg;
  GstBus *bus;
    
  /* process any pending tag messages on the bus NOW, so we can get to
   * the information without/before giving control back to the main loop */

  /* application message is for stream-info */
  events = GST_MESSAGE_TAG | GST_MESSAGE_DURATION | GST_MESSAGE_APPLICATION;
  bus = gst_element_get_bus (gvc->priv->play);
  while ((msg = gst_bus_poll (bus, events, 0))) {
    gst_bus_async_signal_func (bus, msg, NULL);
  }
  gst_object_unref (bus);
}

void
bacon_video_widget_get_metadata (BaconVideoWidget * gvc,
                                 BaconVideoWidgetMetadataType type,
                                 GValue * value)
{
  g_return_if_fail (gvc != NULL);
  g_return_if_fail (BACON_IS_VIDEO_WIDGET (gvc));
  g_return_if_fail (GST_IS_ELEMENT (gvc->priv->play));

  switch (type)
    {
    case gvc_INFO_TITLE:
    case gvc_INFO_ARTIST:
    case gvc_INFO_YEAR:
    case gvc_INFO_ALBUM:
    case gvc_INFO_VIDEO_CODEC:
    case gvc_INFO_AUDIO_CODEC:
    case gvc_INFO_AUDIO_CHANNELS:
      bacon_video_widget_get_metadata_string (gvc, type, value);
      break;
    case gvc_INFO_DURATION:
      bacon_video_widget_get_metadata_int (gvc, type, value);
      break;
    case gvc_INFO_DIMENSION_X:
    case gvc_INFO_DIMENSION_Y:
    case gvc_INFO_FPS:
      bacon_video_widget_get_metadata_int (gvc, type, value);
      break;    
    case gvc_INFO_AUDIO_BITRATE:
    case gvc_INFO_VIDEO_BITRATE:
    case gvc_INFO_TRACK_NUMBER:
    case gvc_INFO_AUDIO_SAMPLE_RATE:
      bacon_video_widget_get_metadata_int (gvc, type, value);
      break;
    case gvc_INFO_HAS_VIDEO:
      bacon_video_widget_get_metadata_bool (gvc, type, value);
      break;
    case gvc_INFO_HAS_AUDIO:
      bacon_video_widget_get_metadata_bool (gvc, type, value);
      break;
    default:
      g_return_if_reached ();
    }

  return;
}

/* Screenshot functions */
gboolean
bacon_video_widget_can_get_frames (BaconVideoWidget * gvc, GError ** error)
{
  g_return_val_if_fail (gvc != NULL, FALSE);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), FALSE);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), FALSE);

  /* check for version */
  if (!g_object_class_find_property (
           G_OBJECT_GET_CLASS (gvc->priv->play), "frame")) {
    g_set_error (error, gvc_ERROR, gvc_ERROR_GENERIC,
        _("Too old version of GStreamer installed."));
    return FALSE;
  }

  /* check for video */
  if (!gvc->priv->media_has_video) {
    g_set_error (error, gvc_ERROR, gvc_ERROR_GENERIC,
        _("Media contains no supported video streams."));
  }

  return gvc->priv->media_has_video;
}

static void
destroy_pixbuf (guchar *pix, gpointer data)
{
  gst_buffer_unref (GST_BUFFER (data));
}

GdkPixbuf *
bacon_video_widget_get_current_frame (BaconVideoWidget * gvc)
{
  GstStructure *s;
  GstBuffer *buf = NULL;
  GdkPixbuf *pixbuf;
  GstCaps *to_caps;
  gint outwidth = 0;
  gint outheight = 0;

  g_return_val_if_fail (gvc != NULL, NULL);
  g_return_val_if_fail (BACON_IS_VIDEO_WIDGET (gvc), NULL);
  g_return_val_if_fail (GST_IS_ELEMENT (gvc->priv->play), NULL);

  /* when used as thumbnailer, wait for pending seeks to complete */
  if (gvc->priv->use_type == gvc_USE_TYPE_CAPTURE) {
    gst_element_get_state (gvc->priv->play, NULL, NULL, -1);
  }

  /* no video info */
  if (!gvc->priv->video_width || !gvc->priv->video_height) {
    GST_INFO ("Could not take screenshot: %s", "no video info");
    g_warning ("Could not take screenshot: %s", "no video info");
    return NULL;
  }

  /* get frame */
  g_object_get (gvc->priv->play, "frame", &buf, NULL);

  if (!buf) {
    GST_INFO ("Could not take screenshot: %s", "no last video frame");
    g_warning ("Could not take screenshot: %s", "no last video frame");
    return NULL;
  }

  if (GST_BUFFER_CAPS (buf) == NULL) {
    GST_INFO ("Could not take screenshot: %s", "no caps on buffer");
    g_warning ("Could not take screenshot: %s", "no caps on buffer");
    return NULL;
  }

  /* convert to our desired format (RGB24) */
  to_caps = gst_caps_new_simple ("video/x-raw-rgb",
      "bpp", G_TYPE_INT, 24,
      "depth", G_TYPE_INT, 24,
      /* Note: we don't ask for a specific width/height here, so that
       * videoscale can adjust dimensions from a non-1/1 pixel aspect
       * ratio to a 1/1 pixel-aspect-ratio */
      "framerate", GST_TYPE_FRACTION, 
      gvc->priv->video_fps_n, gvc->priv->video_fps_d,
      "pixel-aspect-ratio", GST_TYPE_FRACTION, 1, 1,
      "endianness", G_TYPE_INT, G_BIG_ENDIAN,
      "red_mask", G_TYPE_INT, 0xff0000,
      "green_mask", G_TYPE_INT, 0x00ff00,
      "blue_mask", G_TYPE_INT, 0x0000ff,
      NULL);

  GST_INFO ("frame caps: %" GST_PTR_FORMAT, GST_BUFFER_CAPS (buf));
  GST_INFO ("pixbuf caps: %" GST_PTR_FORMAT, to_caps);

  /* gvc_frame_conv_convert () takes ownership of the buffer passed */
  buf = gvc_frame_conv_convert (buf, to_caps);

  gst_caps_unref (to_caps);

  if (!buf) {
    GST_INFO ("Could not take screenshot: %s", "conversion failed");
    g_warning ("Could not take screenshot: %s", "conversion failed");
    return NULL;
  }

  if (!GST_BUFFER_CAPS (buf)) {
    GST_INFO ("Could not take screenshot: %s", "no caps on output buffer");
    g_warning ("Could not take screenshot: %s", "no caps on output buffer");
    return NULL;
  }

  s = gst_caps_get_structure (GST_BUFFER_CAPS (buf), 0);
  gst_structure_get_int (s, "width", &outwidth);
  gst_structure_get_int (s, "height", &outheight);
  g_return_val_if_fail (outwidth > 0 && outheight > 0, FALSE);

  /* create pixbuf from that - use our own destroy function */
  pixbuf = gdk_pixbuf_new_from_data (GST_BUFFER_DATA (buf),
      GDK_COLORSPACE_RGB, FALSE, 8, outwidth, outheight,
      GST_ROUND_UP_4 (outwidth * 3), destroy_pixbuf, buf);

  if (!pixbuf) {
    GST_INFO ("Could not take screenshot: %s", "could not create pixbuf");
    g_warning ("Could not take screenshot: %s", "could not create pixbuf");
    gst_buffer_unref (buf);
  }

  return pixbuf;
}



/* =========================================== */
/*                                             */
/*          Widget typing & Creation           */
/*                                             */
/* =========================================== */

GType
bacon_video_widget_get_type (void)
{
	static GType type = 0;

	if (!type) {
		static const GTypeInfo info = {
			sizeof (BaconVideoWidgetClass),
				NULL,           /* base_init */
				NULL,           /* base_finalize */
				(GClassInitFunc) bacon_video_widget_class_init,
				NULL,           /* class_finalize */
				NULL,           /* class_data */
				sizeof (BaconVideoWidget),
				0,
				(GInstanceInitFunc) bacon_video_widget_init,
				NULL
			};

			type = g_type_register_static (G_TYPE_OBJECT,
						       "BaconVideoWidget",
				                       &info, 0);
	}

	return type;
}

/* applications must use exactly one of bacon_video_widget_get_option_group()
 * OR bacon_video_widget_init_backend(), but not both */

GOptionGroup*
bacon_video_widget_get_option_group (void)
{
  return gst_init_get_option_group ();
}

void
bacon_video_widget_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

GQuark
bacon_video_widget_error_quark (void)
{
  static GQuark q; /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("gvc-error-quark");
  }
  return q;
}

static void
gvc_update_interface_implementations (BaconVideoWidget *gvc)
{

  GstXOverlay *old_xoverlay = gvc->priv->xoverlay;
  GstElement *video_sink = NULL;
  GstElement *element = NULL;



  g_object_get (gvc->priv->play, "video-sink", &video_sink, NULL);
  g_assert (video_sink != NULL);


  /* We try to get an element supporting XOverlay interface */
  if (GST_IS_BIN (video_sink)) {
    GST_INFO ("Retrieving xoverlay from bin ...");
    element = gst_bin_get_by_interface (GST_BIN (video_sink),
                                        GST_TYPE_X_OVERLAY);
  } else {
    element = video_sink;
  }

  if (GST_IS_X_OVERLAY (element)) {
    GST_INFO ("Found xoverlay: %s", GST_OBJECT_NAME (element));
    gvc->priv->xoverlay = GST_X_OVERLAY (element);
  } else {
    GST_INFO ("No xoverlay found");
    gvc->priv->xoverlay = NULL;
  }
  if (old_xoverlay)
    gst_object_unref (GST_OBJECT (old_xoverlay));


  gst_object_unref (video_sink);
}

static void
gvc_element_msg_sync (GstBus *bus, GstMessage *msg, gpointer data)
{
  
  BaconVideoWidget *gvc = BACON_VIDEO_WIDGET (data);

  g_assert (msg->type == GST_MESSAGE_ELEMENT);

  if (msg->structure == NULL)
    return;

  /* This only gets sent if we haven't set an ID yet. This is our last
   * chance to set it before the video sink will create its own window */
  if (gst_structure_has_name (msg->structure, "prepare-xwindow-id")) {
    GdkWindow *window;

    GST_INFO ("Handling sync prepare-xwindow-id message");

    g_mutex_lock (gvc->priv->lock);
    gvc_update_interface_implementations (gvc);
    g_mutex_unlock (gvc->priv->lock);

    g_return_if_fail (gvc->priv->xoverlay != NULL);
    g_return_if_fail (gvc->priv->video_window != NULL);

    window = gst_video_widget_get_video_window (GST_VIDEO_WIDGET(gvc->priv->video_window));
    #ifdef WIN32
   	  gst_x_overlay_set_xwindow_id (gvc->priv->xoverlay, GDK_WINDOW_HWND(window));
	#else
	  gst_x_overlay_set_xwindow_id (gvc->priv->xoverlay, GDK_WINDOW_XID (window));
	#endif

  }
}

static void
got_new_video_sink_bin_element (GstBin *video_sink, GstElement *element,
                                gpointer data)
{
  BaconVideoWidget *gvc = BACON_VIDEO_WIDGET (data);

  g_mutex_lock (gvc->priv->lock);
  gvc_update_interface_implementations (gvc);
  g_mutex_unlock (gvc->priv->lock);

}

static void gvc_window_construct(int width, int weight,  BaconVideoWidget *gvc){
	
	//Create the Video Widget
	gvc->priv->video_window = gst_video_widget_new();
	
	gst_video_widget_set_minimum_size (GST_VIDEO_WIDGET (gvc->priv->video_window),
            width, weight);
	gst_video_widget_set_source_size (GST_VIDEO_WIDGET (gvc->priv->video_window), width,weight );
	g_signal_connect (G_OBJECT (gvc->priv->video_window), "expose_event",
		    G_CALLBACK (gvc_expose_event), gvc);


}

BaconVideoWidget *
bacon_video_widget_new (int width, int height,
                        gvcUseType type, GError ** err)
{
  
  BaconVideoWidget *gvc;
  GstElement *audio_sink = NULL, *video_sink = NULL;
  gchar *version_str;

#ifndef GST_DISABLE_GST_INFO
  if (_totem_gst_debug_cat == NULL) {
    GST_DEBUG_CATEGORY_INIT (_totem_gst_debug_cat, "totem", 0,
        "Totem GStreamer Backend");
  }
 
#endif

  version_str = gst_version_string ();
  GST_INFO ("Initialised %s", version_str);
  g_free (version_str);

  gst_pb_utils_init ();

  gvc = g_object_new(bacon_video_widget_get_type (), NULL);
  gvc_window_construct(width,height,gvc);

  gvc->priv->use_type = type;
  
  GST_INFO ("use_type = %d", type);

  gvc->priv->play = gst_element_factory_make ("playbin", "play");
  if (!gvc->priv->play) {

    g_set_error (err, gvc_ERROR, gvc_ERROR_PLUGIN_LOAD,
                 _("Failed to create a GStreamer play object. "
                   "Please check your GStreamer installation."));
    g_object_ref_sink (gvc);
    g_object_unref (gvc);
    return NULL;
  }

  gvc->priv->bus = gst_element_get_bus (gvc->priv->play);
  
  gst_bus_add_signal_watch (gvc->priv->bus);

  gvc->priv->sig_bus_async = 
      g_signal_connect (gvc->priv->bus, "message", 
                        G_CALLBACK (gvc_bus_message_cb),
                        gvc);

  gvc->priv->speakersetup = gvc_AUDIO_SOUND_STEREO;
  gvc->priv->media_device = g_strdup ("/dev/dvd");
  gvc->priv->init_width = 240;
  gvc->priv->init_height = 180;
  gvc->priv->ratio_type = gvc_RATIO_AUTO;

  gvc->priv->cursor_shown = TRUE;
  gvc->priv->logo_mode = FALSE;
  gvc->priv->auto_resize = TRUE;



  if (type == gvc_USE_TYPE_VIDEO || type == gvc_USE_TYPE_AUDIO) {
    audio_sink = gst_element_factory_make ("autoaudiosink", "audio-sink");
    if (audio_sink == NULL) {
      g_warning ("Could not create element 'autoaudiosink'");
    } 
  } else {
    audio_sink = gst_element_factory_make ("fakesink", "audio-fake-sink");
  }

  if (type == gvc_USE_TYPE_VIDEO) {   
      video_sink = gst_element_factory_make ("autovideosink", "video-sink");
      if (video_sink == NULL) {
        g_warning ("Could not create element 'autovideosink'");
        /* Try to fallback on ximagesink */
        video_sink = gst_element_factory_make ("ximagesink", "video-sink");
      }  

  } else {
    video_sink = gst_element_factory_make ("fakesink", "video-fake-sink");
    if (video_sink)
      g_object_set (video_sink, "sync", TRUE, NULL);
  }

  if (video_sink) {
    GstStateChangeReturn ret;

    /* need to set bus explicitly as it's not in a bin yet and
     * poll_for_state_change() needs one to catch error messages */
    gst_element_set_bus (video_sink, gvc->priv->bus);
    /* state change NULL => READY should always be synchronous */
    ret = gst_element_set_state (video_sink, GST_STATE_READY);
    if (ret == GST_STATE_CHANGE_FAILURE) {
      /* Drop this video sink */
      gst_element_set_state (video_sink, GST_STATE_NULL);
      gst_object_unref (video_sink);
      /* Try again with autovideosink */
      video_sink = gst_element_factory_make ("autovideosink", "video-sink");
      gst_element_set_bus (video_sink, gvc->priv->bus);
      ret = gst_element_set_state (video_sink, GST_STATE_READY);
      if (ret == GST_STATE_CHANGE_FAILURE) {
        GstMessage *err_msg;

        err_msg = gst_bus_poll (gvc->priv->bus, GST_MESSAGE_ERROR, 0);
        if (err_msg == NULL) {
          g_warning ("Should have gotten an error message, please file a bug.");
          g_set_error (err, gvc_ERROR, gvc_ERROR_VIDEO_PLUGIN,
               _("Failed to open video output. It may not be available. "
                 "Please select another video output in the Multimedia "
                 "Systems Selector."));
        } else if (err_msg) {
          *err = gvc_error_from_gst_error (gvc, err_msg);
          gst_message_unref (err_msg);
        }
        goto sink_error;
      }
    }
  } else {
    g_set_error (err, gvc_ERROR, gvc_ERROR_VIDEO_PLUGIN,
                 _("Could not find the video output. "
                   "You may need to install additional GStreamer plugins, "
                   "or select another video output in the Multimedia Systems "
                   "Selector."));
    goto sink_error;
  }

  if (audio_sink) {
    GstStateChangeReturn ret;
    GstBus *bus;

    /* need to set bus explicitly as it's not in a bin yet and
     * we need one to catch error messages */
    bus = gst_bus_new ();
    gst_element_set_bus (audio_sink, bus);

    /* state change NULL => READY should always be synchronous */
    ret = gst_element_set_state (audio_sink, GST_STATE_READY);
    gst_element_set_bus (audio_sink, NULL);

    if (ret == GST_STATE_CHANGE_FAILURE) {
      /* doesn't work, drop this audio sink */
      gst_element_set_state (audio_sink, GST_STATE_NULL);
      gst_object_unref (audio_sink);
      audio_sink = NULL;
      /* Hopefully, fakesink should always work */
      if (type != gvc_USE_TYPE_AUDIO)
        audio_sink = gst_element_factory_make ("fakesink", "audio-sink");
      if (audio_sink == NULL) {
        GstMessage *err_msg;

        err_msg = gst_bus_poll (bus, GST_MESSAGE_ERROR, 0);
        if (err_msg == NULL) {
          g_warning ("Should have gotten an error message, please file a bug.");
          g_set_error (err, gvc_ERROR, gvc_ERROR_AUDIO_PLUGIN,
                       _("Failed to open audio output. You may not have "
                         "permission to open the sound device, or the sound "
                         "server may not be running. "
                         "Please select another audio output in the Multimedia "
                         "Systems Selector."));
        } else if (err) {
          *err = gvc_error_from_gst_error (gvc, err_msg);
          gst_message_unref (err_msg);
        }
        gst_object_unref (bus);
        goto sink_error;
      }
      /* make fakesink sync to the clock like a real sink */
      g_object_set (audio_sink, "sync", TRUE, NULL);
      GST_INFO ("audio sink doesn't work, using fakesink instead");
      gvc->priv->uses_fakesink = TRUE;
    }
    gst_object_unref (bus);
  } else {
    g_set_error (err, gvc_ERROR, gvc_ERROR_AUDIO_PLUGIN,
                 _("Could not find the audio output. "
                   "You may need to install additional GStreamer plugins, or "
                   "select another audio output in the Multimedia Systems "
                   "Selector."));
    goto sink_error;
  }

  
  /* now tell playbin */
  g_object_set (gvc->priv->play, "video-sink", video_sink, NULL);
  g_object_set (gvc->priv->play, "audio-sink", audio_sink, NULL);
  
 

  g_signal_connect (gvc->priv->play, "notify::source",
      G_CALLBACK (playbin_source_notify_cb), gvc);
  g_signal_connect (gvc->priv->play, "notify::stream-info",
      G_CALLBACK (playbin_stream_info_notify_cb), gvc);

  if (type == gvc_USE_TYPE_VIDEO) {
    GstStateChangeReturn ret;

    /* wait for video sink to finish changing to READY state, 
     * otherwise we won't be able to detect the colorbalance interface */
    ret = gst_element_get_state (video_sink, NULL, NULL, 5 * GST_SECOND);

    if (ret != GST_STATE_CHANGE_SUCCESS) {
      GST_WARNING ("Timeout setting videosink to READY");
      g_set_error (err, gvc_ERROR, gvc_ERROR_VIDEO_PLUGIN,
          _("Failed to open video output. It may not be available. "
          "Please select another video output in the Multimedia Systems Selector."));
      return NULL;
    }
	 gvc_update_interface_implementations (gvc);

  }

  /* we want to catch "prepare-xwindow-id" element messages synchronously */
  gst_bus_set_sync_handler (gvc->priv->bus, gst_bus_sync_signal_handler, gvc);

  gvc->priv->sig_bus_sync = 
      g_signal_connect (gvc->priv->bus, "sync-message::element",
                        G_CALLBACK (gvc_element_msg_sync), gvc);

  if (GST_IS_BIN (video_sink)) {
    /* video sink bins like gconfvideosink might remove their children and
     * create new ones when set to NULL state, and they are currently set
     * to NULL state whenever playbin re-creates its internal video bin
     * (it sets all elements to NULL state before gst_bin_remove()ing them) */
    g_signal_connect (video_sink, "element-added",
                      G_CALLBACK (got_new_video_sink_bin_element), gvc);
  }

    
  return gvc;

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
	
    g_object_ref (gvc);
    g_object_ref_sink (G_OBJECT (gvc));
    g_object_unref (gvc);

    return NULL;
  }
}

GtkWidget 
* bacon_video_widget_get_window (BaconVideoWidget *gvc){
	return gvc->priv->video_window;
}
