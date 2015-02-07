/*
 * Gstreamer Multi File Source Bin
 * Copyright (C) 2013 Andoni Morales Alastruey <ylatuya@gmail.com>
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
 *      The Free Software Foundation, Inc.,
 *      51 Franklin Street, Fifth Floor
 *      Boston, MA  02110-1301, USA.
 */

#ifndef _GST_VIDEO_ENCODER_H_
#define _GST_VIDEO_ENCODER_H_

#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

#include <glib-object.h>

#include "lgm-utils.h"

G_BEGIN_DECLS
#define GST_TYPE_VIDEO_ENCODER             (gst_video_encoder_get_type ())
#define GST_VIDEO_ENCODER(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GST_TYPE_VIDEO_ENCODER, GstVideoEncoder))
#define GST_VIDEO_ENCODER_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GST_TYPE_VIDEO_ENCODER, GstVideoEncoderClass))
#define GST_IS_VIDEO_ENCODER(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GST_TYPE_VIDEO_ENCODER))
#define GST_IS_VIDEO_ENCODER_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GST_TYPE_VIDEO_ENCODER))
#define GST_VIDEO_ENCODER_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GST_TYPE_VIDEO_ENCODER, GstVideoEncoderClass))
#define GVE_ERROR gst_video_encoder_error_quark ()

typedef struct _GstVideoEncoderClass GstVideoEncoderClass;
typedef struct _GstVideoEncoder GstVideoEncoder;
typedef struct GstVideoEncoderPrivate GstVideoEncoderPrivate;


struct _GstVideoEncoderClass
{
  GObjectClass parent_class;

  void (*error) (GstVideoEncoder * gve, const char *message);
  void (*percent_completed) (GstVideoEncoder * gve, float percent);
};

struct _GstVideoEncoder
{
  GObject parent_instance;
  GstVideoEncoderPrivate *priv;
};

EXPORT GType gst_video_encoder_get_type (void) G_GNUC_CONST;

EXPORT void gst_video_encoder_init_backend                     (int *argc, char ***argv);

EXPORT GstVideoEncoder *gst_video_encoder_new                  (gchar *output_file, GError ** err);

EXPORT void gst_video_encoder_start                            (GstVideoEncoder * gve);

EXPORT void gst_video_encoder_cancel                           (GstVideoEncoder * gve);

EXPORT void gst_video_encoder_set_encoding_format              (GstVideoEncoder * gve,
                                                                VideoEncoderType video_codec,
                                                                AudioEncoderType audio_codec,
                                                                VideoMuxerType muxer,
                                                                guint video_quality,
                                                                guint audio_quality,
                                                                guint height,
                                                                guint width,
                                                                guint fps_n,
                                                                guint fps_d);

EXPORT void gst_video_encoder_add_file                         (GstVideoEncoder * gve,
                                                                const gchar * file,
                                                                guint64 duration);

EXPORT gboolean gst_video_encoder_dump_graph                   (GstVideoEncoder *gve);

G_END_DECLS
#endif /* _GST_VIDEO_ENCODER_H_ */
