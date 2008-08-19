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

/* Properties */
enum
{
  PROP_0,
  PROP_DISPLAY_HEIGHT,
  PROP_DISPLAY_WIDTH,
  PROP_ENCODE_HEIGHT,
  PROP_ENCODE_WIDTH,
  PROP_VIDEO_BITRATE,
  PROP_AUDIO_BITRATE,
  PROP_OUTPUT_FILE,
  PROP_INPUT_FILE,
  PROP_WITH_AUDIO
};

struct GstVideoCapturerPrivate
{
	GvcUseType use_type;
	gchar 	*input_file;
	gchar	*output_file;
	
	guint	display_height;
	guint	display_width;
	guint   encode_height;
	guint   encode_width;
	guint	audio_bitrate;
	guint	video_bitrate; 

	
	GstBin *display_bin;
	GstBin *encode_bin;
	GstBin *decode_bin;
	GstElement *main_pipeline;
	
	GvcVideoEncoderType *video_encoder;
	GvcAudioEncoderType	*audio_encoder;
	
	GstBus                      *bus;
	gulong              sig_bus_async;
};

static int gvc_signals[LAST_SIGNAL] = { 0 };
static void new_decoded_pad_cb (GstElement* object,GstPad* arg0,gboolean arg1,gpointer user_data);
static void gvc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data);
static void gvc_video_capturer_get_property (GObject * object, guint property_id, GValue * value, GParamSpec * pspec);
static void gvc_video_capturer_set_property (GObject * object, guint property_id,const GValue * value, GParamSpec * pspec);


G_DEFINE_TYPE (GstVideoCapturer, gvc_video_capturer, G_TYPE_OBJECT);




static void
gvc_video_capturer_init (GstVideoCapturer *object)
{
	GstVideoCapturerPrivate *priv;
  	object->priv = priv = G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_VIDEO_CAPTURER, GstVideoCapturerPrivate);

}

static void
gvc_video_capturer_finalize (GObject *object)
{
	  GstVideoCapturer *gvc = (GstVideoCapturer *) object;



  if (gvc->priv->bus) {
    /* make bus drop all messages to make sure none of our callbacks is ever
     * called again (main loop might be run again to display error dialog) */
    gvc_bus_set_flushing (gvc->priv->bus, TRUE);

  
    if (gvc->priv->sig_bus_async)
      g_signal_handler_disconnect (gvc->priv->bus,gvc->priv->sig_bus_async);

    gvc_object_unref (gvc->priv->bus);
    gvc->priv->bus = NULL;
  }

  g_free (gvc->priv->output_file);
  gvc->priv->output_file = NULL;
    
  g_free (gvc->priv->input_file);
  gvc->priv->input_file = NULL; 
  


	
  	
  	if (gvc->priv->main_pipeline != NULL && GST_IS_ELEMENT (gvc->priv->main_pipeline )) {
    	gvc_element_set_state (gvc->priv->main_pipeline , GST_STATE_NULL);
    	gvc_object_unref (gvc->priv->main_pipeline);
    	gvc->priv->main_pipeline = NULL;
  	}
  	
  	if (gvc->priv->decode_bin != NULL && GST_IS_ELEMENT (gvc->priv->decode_bin )) {
	   	gvc_object_unref (gvc->priv->decode_bin );
    	gvc->priv->decode_bin  = NULL;
  	}
  	
  	if (gvc->priv->encode_bin != NULL && GST_IS_ELEMENT (gvc->priv->encode_bin )) {
    	gvc_object_unref (gvc->priv->encode_bin);
    	gvc->priv->encode_bin = NULL;
  	}
  	
  	if (gvc->priv->display_bin != NULL && GST_IS_ELEMENT (gvc->priv->display_bin )) {
	   	gvc_object_unref (gvc->priv->display_bin );
    	gvc->priv->display_bin  = NULL;
  	}
  	


	G_OBJECT_CLASS (gvc_video_capturer_parent_class)->finalize (object);
}

static void
gvc_video_capturer_set_property (GObject * object, guint property_id,
                                 const GValue * value, GParamSpec * pspec)
{
   GstVideoCapturer *gvc;

  gvc = GST_VIDEO_CAPTURER (object);

  
  
  switch (property_id) {
    case PROP_DISPLAY_HEIGHT:
      gvc_video_capturer_set_display_height (gvc,
      g_value_get_uint (value));
      break;
    case PROP_DISPLAY_WIDTH:
      gvc_video_capturer_set_display_width (gvc,
      g_value_get_uint (value));
      break;
    case PROP_ENCODE_HEIGHT:
      gvc_video_capturer_set_encode_height (gvc,
      g_value_get_uint (value));
      break;
    case PROP_ENCODE_WIDTH:
      gvc_video_capturer_set_encode_width (gvc,
      g_value_get_uint (value));
      break;
    case PROP_VIDEO_BITRATE:
      gvc_video_capturer_set_video_bit_rate (gvc,
      g_value_get_uint (value));
      break;
    case PROP_AUDIO_BITRATE:
      gvc_video_capturer_set_audio_bit_rate (gvc,
      g_value_get_uint (value));
      break;
    case PROP_OUTPUT_FILE:
      gvc_video_capturer_set_output_file(gvc,
      g_value_get_string (value));
      break;
    case PROP_INPUT_FILE:
      gvc_video_capturer_set_input_file (gvc,
      g_value_get_string (value));
      break;

    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

static void
gvc_video_capturer_get_property (GObject * object, guint property_id,
                                 GValue * value, GParamSpec * pspec)
{
  GstVideoCapturer *gvc;

  gvc = GST_VIDEO_CAPTURER (object);

  switch (property_id) {
    case PROP_DISPLAY_HEIGHT:
      g_value_set_uint (value,gvc->priv->display_height);
      break;
    case PROP_DISPLAY_WIDTH:
      g_value_set_uint (value,gvc->priv->display_width);
      break;
    case PROP_ENCODE_HEIGHT:
      g_value_set_uint (value,gvc->priv->encode_height);
      break;
    case PROP_ENCODE_WIDTH:
      g_value_set_uint (value,gvc->priv->display_width);
      break;
    case PROP_AUDIO_BITRATE:
      g_value_set_uint (value,gvc->priv->audio_bitrate);
      break;
     case PROP_VIDEO_BITRATE:
      g_value_set_uint (value,gvc->priv->video_bitrate);
      break;   
    case PROP_INPUT_FILE:
      g_value_set_string (value,gvc->priv->input_file);
      break;
     case PROP_OUTPUT_FILE:
      g_value_set_string (value,gvc->priv->output_file);
      break;    
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

static void
gvc_video_capturer_class_init (GstVideoCapturerClass *klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS (klass);
	GObjectClass* parent_class = G_OBJECT_CLASS (klass);

	object_class->finalize = gvc_video_capturer_finalize;
	
	g_type_class_add_private (object_class, sizeof (GstVideoCapturerPrivate));
	
	 /* GObject */
  	object_class->set_property = gvc_video_capturer_set_property;
 	object_class->get_property = gvc_video_capturer_get_property;
 	object_class->finalize = gvc_video_capturer_finalize;
 	
 	/* Properties */
  	g_object_class_install_property (object_class, PROP_DISPLAY_HEIGHT,
                                   g_param_spec_uint ("display_height", NULL,
                                                         NULL, 180,5600,576,
                                                         G_PARAM_READWRITE));
  	g_object_class_install_property (object_class, PROP_DISPLAY_WIDTH,
                                   g_param_spec_uint ("display_width", NULL, NULL,
                                                     240, 4200, 720,
                                                     G_PARAM_READWRITE));
  	g_object_class_install_property (object_class, PROP_ENCODE_HEIGHT,
                                   g_param_spec_uint ("encode_height", NULL,
                                                     NULL,180 , 5600, 576,
                                                     G_PARAM_READWRITE));
  	g_object_class_install_property (object_class, PROP_ENCODE_WIDTH,
                                   g_param_spec_uint ("encode_width", NULL,
                                                         NULL, 180,5600,576,
                                                         G_PARAM_READWRITE));
  	g_object_class_install_property (object_class, PROP_VIDEO_BITRATE,
                                   g_param_spec_uint ("video_bitrate", NULL,
                                                         NULL, 100, G_MAXUINT,1000,
                                                         G_PARAM_READWRITE));
  	g_object_class_install_property (object_class, PROP_AUDIO_BITRATE,
                                   g_param_spec_uint ("audio_bitrate", NULL,
                                                         NULL, 12, G_MAXUINT,128,
                                                         G_PARAM_READWRITE));
 	g_object_class_install_property (object_class, PROP_INPUT_FILE,
                                   g_param_spec_string ("input_file", NULL,
                                                        NULL, FALSE,
                                                        G_PARAM_READWRITE));  
  g_object_class_install_property (object_class, PROP_OUTPUT_FILE,
                                   g_param_spec_string ("output_file", NULL,
                                                        NULL, FALSE,
                                                        G_PARAM_READWRITE));
                                                        
}

void
gvc_video_capturer_init_backend (int *argc, char ***argv)
{
  gvc_init (argc, argv);
}

void 
gvc_video_capturer_set_encoder( gchar *output_file, guint height, 
								guint width, guint bitrate,
								GvcVideoEncoderType video_encoder, 
								GvcAudioEncoderType	audio_encoder,
								gboolean audio_enabled)
{
}

void 
gvc_video_capturer_new (gchar *mrl, GvcUseType use_type, gchar **error )
{
	GstElement *source = NULL;
	GstElement *tee = NULL;
	GstElement *queue = NULL;
	GstVideoCapturer *gvc;
	
	
	gvc = g_object_new(GST_TYPE_VIDEO_CAPTURER, NULL);
	
	gvc->priv->use_type = use_type;
	
	gvc->priv->main_pipeline = gst_pipeline_new ("main_pipeline");
	
	if (!gvc->priv->main_pipeline) {
	 
    	/*g_set_error (err, BVW_ERROR, BVW_ERROR_PLUGIN_LOAD,
                	_("Failed to create a GStreamer Bin. "
                    "Please check your GStreamer installation."));*/
    	g_object_ref_sink (gvc);
    	g_object_unref (gvc);
    	return;
  	}
  	
  	source = gst_element_factory_make ("dv1394src", "dvsource");
  	
  	gvc->priv->decode_bin = GST_BIN(gst_element_factory_make ("decodebin", "decoder"));
  	
	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));
  
  	gst_bus_add_signal_watch (gvc->priv->bus);

  	gvc->priv->sig_bus_async = 
      g_signal_connect (gvc->priv->bus, "message", 
                        G_CALLBACK (gvc_bus_message_cb),
                        gvc);
  	
  	g_signal_connect (gvc->priv->decode_bin, "new-decoded-pad",G_CALLBACK(new_decoded_pad_cb),gvc);
  	
  	
  	queue = gst_element_factory_make ("queue", "decode queue");
  	
  	tee = gst_element_factory_make ("tee","tee");
  	
  	gst_element_link(GST_ELEMENT(source),GST_ELEMENT(gvc->priv->decode_bin));
  	
  	gst_bin_add_many(GST_BIN(gvc->priv->main_pipeline),source,GST_ELEMENT(gvc->priv->decode_bin),queue);
	
	


  	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));
  
  	gvc_bus_add_signal_watch (gvc->priv->bus);
	
  	gvc->priv->sig_bus_async = 
      g_signal_connect (gvc->priv->bus, "message", 
                        G_CALLBACK (gvc_bus_message_cb),
                        gvc);
	
	
	
	
}

static void new_decoded_pad_cb (GstElement* object,
                                           GstPad* arg0,
                                           gboolean arg1,
                                           gpointer user_data)
{
                                   
}

static void gvc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{

}
