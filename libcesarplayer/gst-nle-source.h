/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * Gstreamer NLE source
 * Copyright (C)  Andoni Morales Alastruey 2013 <ylatuya@gmail.com>
 *
 * You may redistribute it and/or modify it under the terms of the
 * GNU General Public License, as published by the Free Software
 * Foundation; either version 2 of the License, or (at your option)
 * any later version.
 *
 * foob is distributed in the hope that it will be useful,
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

#ifndef _GST_NLE_SOURCE_H_
#define _GST_NLE_SOURCE_H_

#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

#include <glib-object.h>
#include "lgm-utils.h"

G_BEGIN_DECLS
#define GST_TYPE_NLE_SOURCE             (gst_nle_source_get_type ())
#define GST_NLE_SOURCE(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GST_TYPE_NLE_SOURCE, GstNleSource))
#define GST_NLE_SOURCE_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GST_TYPE_NLE_SOURCE, GstNleSourceClass))
#define GST_IS_NLE_SOURCE(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GST_TYPE_NLE_SOURCE))
#define GST_IS_NLE_SOURCE_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GST_TYPE_NLE_SOURCE))
#define GST_NLE_SOURCE_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GST_TYPE_NLE_SOURCE, GstNleSourceClass))
#define GCC_ERROR gst_nle_source_error_quark ()
typedef struct _GstNleSourceClass GstNleSourceClass;
typedef struct _GstNleSource GstNleSource;

struct _GstNleSourceClass
{
  GstBinClass parent_class;
};

struct _GstNleSource
{
  GstBin parent;

  guint width;
  guint height;
  guint fps_n;
  guint fps_d;
  guint title_size;
  gboolean overlay_title;
  gboolean with_audio;

  GstPad *video_srcpad;
  GstPad *video_sinkpad;
  GstPad *audio_srcpad;
  GstPad *audio_sinkpad;
  GstElement *videocrop;
  GstElement *textoverlay;
  gboolean video_linked;
  gboolean audio_linked;
  gboolean video_srcpad_added;
  gboolean audio_srcpad_added;

  GstElement *source;
  GstElement *decoder;

  guint64 accu_time;
  guint64 start_ts;
  guint64 video_ts;
  guint64 audio_ts;
  gboolean seek_done;
  gboolean audio_seek_done;
  gboolean video_seek_done;
  gboolean audio_eos;
  gboolean video_eos;
  gboolean item_setup;

  GMutex stream_lock;

  GList *queue;
  gint index;

  gint64 cached_duration;
};

typedef struct
{
  guint x;
  guint y;
  guint width;
  guint height;
} GstNleRectangle;

EXPORT GType gst_nle_source_get_type (void) G_GNUC_CONST;

EXPORT GstNleSource *gst_nle_source_new (void);

EXPORT void gst_nle_source_configure (GstNleSource *nlesrc,
                                      guint width, guint height,
                                      guint fps_n, guint fps_d,
                                      gboolean overlay_title,
                                      gboolean with_audio
                                     );

EXPORT void gst_nle_source_add_item   (GstNleSource *nlesrc,
                                       const gchar *file_path,
                                       const gchar *title,
                                       guint64 start,
                                       guint64 stop,
                                       gfloat rate,
                                       gboolean still_picturie,
                                       GstNleRectangle roi
                                     );

EXPORT void gst_nle_source_query_progress (GstNleSource * nlesrc,
                                                     gfloat * position);

G_END_DECLS
#endif /* _GST_NLE_SOURCE_H_ */

