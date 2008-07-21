/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * foob
 * Copyright (C)  2008 <>
 * 
 * foob is free software.
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

#include "gst-video-capturer.h"
#include <gst/gst.h>

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_STATE_CHANGED,
  LAST_SIGNAL
};

struct GstVideoCapturerPrivate
{
	GvcUseType use_type;
	gchar 	*source;
	gchar	*output_file;
	
	guint	height;
	guint	width;
	guint	audiobitrate;
	guint	videobitrate; 
	GvcEncodingPassType	pass;
	
	GvcVideoEncoderType video_encoder;
	GvcAudioEncoderType	audio_encoder;
	
};

static int gvc_signals[LAST_SIGNAL] = { 0 };

G_DEFINE_TYPE (GstVideoCapturer, gst_video_capturer, G_TYPE_OBJECT);


static void 
gvc_parse_source(GstVideoCapturer *gvc, gchar *source){
	
	if (g_str_has_prefix (source, "v4l://")) {
	    gvc->priv->source = g_strdup_printf ("%s", source+6);
	}
	else if (g_str_has_prefix (source, "dvd:///")) {
    	gvc->priv->source = g_strdup ("dvd://");
    }
    else if (g_str_has_prefix (source, "/dev")){
	    gvc->priv->source = g_strdup_printf ("%s", source);
    }
    else if (source[0] == '/') {
   	 	gvc->priv->source = g_strdup_printf ("file://%s", source);
  	}
    
}

static void
gst_video_capturer_init (GstVideoCapturer *object)
{
	GstVideoCapturerPrivate *priv;
  	object->priv = priv = G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_VIDEO_CAPTURER, GstVideoCapturerPrivate);

}

static void
gst_video_capturer_finalize (GObject *object)
{
	/* TODO: Add deinitalization code here */

	G_OBJECT_CLASS (gst_video_capturer_parent_class)->finalize (object);
}

static void
gst_video_capturer_class_init (GstVideoCapturerClass *klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS (klass);
	GObjectClass* parent_class = G_OBJECT_CLASS (klass);

	object_class->finalize = gst_video_capturer_finalize;
	
	g_type_class_add_private (object_class, sizeof (GstVideoCapturerPrivate));
}


void
gst_video_capturer_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

void 
gst_video_capturer_set_encoder( gchar *output_file, guint height, 
								guint width, guint bitrate, GvcEncodingPassType	pass,
								GvcVideoEncoderType video_encoder, GvcAudioEncoderType	audio_encoder,
								gboolean audio_enabled)
{
}

void 
gst_video_capturer_new (gchar *mrl, GvcUseType use_type, gchar **error )
{
	GstElement *source = NULL, *ffmpegcolorspace = NULL;
	GstElement *pipeline = NULL, *tee = NULL;
	GstElement *queu1 = NULL , *queue2 = NULL;
	GstVideoCapturer *gvc;
	
	
	gvc = g_object_new(GST_TYPE_VIDEO_CAPTURER, NULL);
	
	gvc->priv->use_type = use_type;
	
	
	
	
}
