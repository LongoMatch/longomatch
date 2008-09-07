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

#ifndef _GST_VIDEO_CAPTURER_H_
#define _GST_VIDEO_CAPTURER_H_

#include <glib-object.h>
#include <gtk/gtk.h>


G_BEGIN_DECLS

#define GST_TYPE_VIDEO_CAPTURER             (gst_video_capturer_get_type ())
#define GST_VIDEO_CAPTURER(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GST_TYPE_VIDEO_CAPTURER, GstVideoCapturer))
#define GST_VIDEO_CAPTURER_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GST_TYPE_VIDEO_CAPTURER, GstVideoCapturerClass))
#define GST_IS_VIDEO_CAPTURER(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GST_TYPE_VIDEO_CAPTURER))
#define GST_IS_VIDEO_CAPTURER_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GST_TYPE_VIDEO_CAPTURER))
#define GST_VIDEO_CAPTURER_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GST_TYPE_VIDEO_CAPTURER, GstVideoCapturerClass))
#define GVC_ERROR bacon_video_widget_error_quark ()

typedef struct _GstVideoCapturerClass GstVideoCapturerClass;
typedef struct _GstVideoCapturer GstVideoCapturer;
typedef struct GstVideoCapturerPrivate GstVideoCapturerPrivate;


struct _GstVideoCapturerClass
{
	GtkHBoxClass parent_class;
	
	void (*eos) (GstVideoCapturer *gvc);
	void (*error) (GstVideoCapturer *gvc, const char *message);
	void (*invalidsource) (GstVideoCapturer *gvc);
};

struct _GstVideoCapturer
{
	GtkHBox parent_instance;
	GstVideoCapturerPrivate *priv;
};

typedef enum {
	/* Plugins */
	GVC_ERROR_AUDIO_PLUGIN,
	GVC_ERROR_NO_PLUGIN_FOR_FILE,
	GVC_ERROR_VIDEO_PLUGIN,
	GVC_ERROR_AUDIO_BUSY,
	/* File */
	GVC_ERROR_BROKEN_FILE,
	GVC_ERROR_FILE_GENERIC,
	GVC_ERROR_FILE_PERMISSION,
	GVC_ERROR_FILE_ENCRYPTED,
	GVC_ERROR_FILE_NOT_FOUND,
	/* Devices */
	GVC_ERROR_DVD_ENCRYPTED,
	GVC_ERROR_INVALID_DEVICE,
	/* Network */
	GVC_ERROR_UNKNOWN_HOST,
	GVC_ERROR_NETWORK_UNREACHABLE,
	GVC_ERROR_CONNECTION_REFUSED,
	/* Generic */
	GVC_ERROR_UNVALID_LOCATION,
	GVC_ERROR_GENERIC,
	GVC_ERROR_CODEC_NOT_HANDLED,
	GVC_ERROR_AUDIO_ONLY,
	GVC_ERROR_CANNOT_CAPTURE,
	GVC_ERROR_READ_ERROR,
	GVC_ERROR_PLUGIN_LOAD,
	GVC_ERROR_EMPTY_FILE
} gvcError;

typedef enum {
	GVC_USE_TYPE_DEVICE_CAPTURE,
	GVC_USE_TYPE_VIDEO_TRANSCODE,
	GVC_USE_TYPE_TEST
} GvcUseType;

typedef enum{
	GVC_VIDEO_ENCODER_TYPE_MPEG4,
	GVC_VIDEO_ENCODER_TYPE_WMV,
	GVC_VIDEO_ENCODER_TYPE_MPEG2,
	GVC_VIDEO_ENCODER_TYPE_RV,	
	GVC_VIDEO_ENCODER_TYPE_XVID,
	GVC_VIDEO_ENCODER_TYPE_H264
}GvcVideoEncoderType;

typedef enum{
	GVC_AUDIO_ENCODER_MP3,
	GVC_AUDIO_ENCODER_AAC,
	GVC_AUDIO_ENCODER_WAV,
	GVC_AUDIO_ENCODER_MPEG1
}GvcAudioEncoderType;


GType gst_video_capturer_get_type (void) G_GNUC_CONST;

void gst_video_capturer_init_backend (int *argc, char ***argv);
GstVideoCapturer * gst_video_capturer_new (GvcUseType use_type, gchar *source,gchar *output_file,GError ** err );
void gst_video_capturer_set_encoder(GstVideoCapturer *gvc);
void gst_video_capturer_rec(GstVideoCapturer *gvc);

G_END_DECLS

#endif /* _GST_VIDEO_CAPTURER_H_ */
