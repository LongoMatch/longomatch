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
  void (*invalidsource) (GstCameraCapturer * gcc);
};

struct _GstCameraCapturer
{
  GtkHBox parent_instance;
  GstCameraCapturerPrivate *priv;
};

typedef enum
{
  /* Plugins */
  GCC_ERROR_AUDIO_PLUGIN,
  GCC_ERROR_NO_PLUGIN_FOR_FILE,
  GCC_ERROR_VIDEO_PLUGIN,
  GCC_ERROR_AUDIO_BUSY,
  /* File */
  GCC_ERROR_BROKEN_FILE,
  GCC_ERROR_FILE_GENERIC,
  GCC_ERROR_FILE_PERMISSION,
  GCC_ERROR_FILE_ENCRYPTED,
  GCC_ERROR_FILE_NOT_FOUND,
  /* Devices */
  GCC_ERROR_DVD_ENCRYPTED,
  GCC_ERROR_INVALID_DEVICE,
  /* Network */
  GCC_ERROR_UNKNOWN_HOST,
  GCC_ERROR_NETWORK_UNREACHABLE,
  GCC_ERROR_CONNECTION_REFUSED,
  /* Generic */
  GCC_ERROR_UNVALID_LOCATION,
  GCC_ERROR_GENERIC,
  GCC_ERROR_CODEC_NOT_HANDLED,
  GCC_ERROR_AUDIO_ONLY,
  GCC_ERROR_CANNOT_CAPTURE,
  GCC_ERROR_READ_ERROR,
  GCC_ERROR_PLUGIN_LOAD,
  GCC_ERROR_EMPTY_FILE
} GccError;

typedef enum
{
  GCC_VIDEO_ENCODER_TYPE_MPEG4,
  GCC_VIDEO_ENCODER_TYPE_XVID,
  GCC_VIDEO_ENCODER_TYPE_THEORA,
  GCC_VIDEO_ENCODER_TYPE_H264
} GccVideoEncoderType;

typedef enum
{
  GCC_AUDIO_ENCODER_MP3,
  GCC_AUDIO_ENCODER_AAC,
  GCC_AUDIO_ENCODER_VORBIS
} GccAudioEncoderType;

typedef enum
{
  GCC_VIDEO_MUXER_AVI,
  GCC_VIDEO_MUXER_MATROSKA,
  GCC_VIDEO_MUXER_OGG
} GccVideoMuxerType;


EXPORT GType
gst_camera_capturer_get_type (void)
  G_GNUC_CONST;

     EXPORT void gst_camera_capturer_init_backend (int *argc, char ***argv);
     EXPORT GstCameraCapturer *gst_camera_capturer_new (gchar * filename,
							GError ** err);
     EXPORT void gst_camera_capturer_run (GstCameraCapturer * gcc);
     EXPORT void gst_camera_capturer_start (GstCameraCapturer * gcc);
     EXPORT void gst_camera_capturer_toggle_pause (GstCameraCapturer * gcc);
     EXPORT void gst_camera_capturer_stop (GstCameraCapturer * gcc);
     EXPORT gboolean gst_camera_capturer_set_video_encoder (GstCameraCapturer
							    * gcc,
							    GccVideoEncoderType
							    type);
     EXPORT gboolean gst_camera_capturer_set_audio_encoder (GstCameraCapturer
							    * gcc,
							    GccAudioEncoderType
							    type);
     EXPORT gboolean gst_camera_capturer_set_video_muxer (GstCameraCapturer *
							  gcc,
							  GccVideoMuxerType
							  type);



G_END_DECLS
#endif /* _GST_CAMERA_CAPTURER_H_ */
