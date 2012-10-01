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
 * foob is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with foob.  If not, write to:
 * 	The Free Software Foundation, Inc.,
 * 	51 Franklin Street, Fifth Floor
 * 	Boston, MA  02110-1301, USA.
 */

#ifndef _GST_CAMERA_CAPTURER_H_
#define _GST_CAMERA_CAPTURER_H_

#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

#include <glib-object.h>
#include <gtk/gtk.h>
#include "common.h"

G_BEGIN_DECLS
#define GST_TYPE_CAMERA_CAPTURER             (gst_camera_capturer_get_type ())
#define GST_CAMERA_CAPTURER(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GST_TYPE_CAMERA_CAPTURER, GstCameraCapturer))
#define GST_CAMERA_CAPTURER_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GST_TYPE_CAMERA_CAPTURER, GstCameraCapturerClass))
#define GST_IS_CAMERA_CAPTURER(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GST_TYPE_CAMERA_CAPTURER))
#define GST_IS_CAMERA_CAPTURER_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GST_TYPE_CAMERA_CAPTURER))
#define GST_CAMERA_CAPTURER_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GST_TYPE_CAMERA_CAPTURER, GstCameraCapturerClass))
#define GCC_ERROR gst_camera_capturer_error_quark ()
typedef struct _GstCameraCapturerClass GstCameraCapturerClass;
typedef struct _GstCameraCapturer GstCameraCapturer;
typedef struct GstCameraCapturerPrivate GstCameraCapturerPrivate;


struct _GstCameraCapturerClass
{
  GtkHBoxClass parent_class;

  void (*eos) (GstCameraCapturer * gcc);
  void (*error) (GstCameraCapturer * gcc, const char *message);
  void (*device_change) (GstCameraCapturer * gcc, gint *device_change);
  void (*invalidsource) (GstCameraCapturer * gcc);
};

struct _GstCameraCapturer
{
  GtkEventBox parent;
  GstCameraCapturerPrivate *priv;
};

typedef enum
{
  GST_CAMERA_CAPTURE_SOURCE_TYPE_NONE = 0,
  GST_CAMERA_CAPTURE_SOURCE_TYPE_DV = 1,
  GST_CAMERA_CAPTURE_SOURCE_TYPE_SYSTEM = 2,
} GstCameraCaptureSourceType;

EXPORT GType
gst_camera_capturer_get_type (void)
G_GNUC_CONST;

void gst_camera_capturer_init_backend (int *argc, char ***argv);
GstCameraCapturer *gst_camera_capturer_new (gchar * filename, GstCameraCaptureSourceType source,
     VideoEncoderType venc, AudioEncoderType aenc, VideoMuxerType muxer, GError ** err);
void gst_camera_capturer_run (GstCameraCapturer * gcc);
void gst_camera_capturer_close (GstCameraCapturer * gcc);
void gst_camera_capturer_start (GstCameraCapturer * gcc);
void gst_camera_capturer_toggle_pause (GstCameraCapturer * gcc);
void gst_camera_capturer_stop (GstCameraCapturer * gcc);
GList *gst_camera_capturer_enum_audio_devices (void);
GList *gst_camera_capturer_enum_video_devices (void);
GdkPixbuf *gst_camera_capturer_get_current_frame (GstCameraCapturer
    * gcc);
void gst_camera_capturer_unref_pixbuf (GdkPixbuf * pixbuf);
void gst_camera_capturer_finalize (GObject * object);

G_END_DECLS
#endif /* _GST_CAMERA_CAPTURER_H_ */
