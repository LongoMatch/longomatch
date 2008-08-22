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

	

	GstElement *video_bin;
	GstElement *audio_bin;
	GstElement *vencode_bin;
	GstElement *aencode_bin;
	GstElement *decode_bin;
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
    gst_bus_set_flushing (gvc->priv->bus, TRUE);

  
    if (gvc->priv->sig_bus_async)
      g_signal_handler_disconnect (gvc->priv->bus,gvc->priv->sig_bus_async);

		gst_object_unref (gvc->priv->bus);
		gvc->priv->bus = NULL;
	}

  	g_free (gvc->priv->output_file);
  	gvc->priv->output_file = NULL;
    
  	g_free (gvc->priv->input_file);
  	gvc->priv->input_file = NULL; 
  


	
  	
  	if (gvc->priv->main_pipeline != NULL && GST_IS_ELEMENT (gvc->priv->main_pipeline )) {
    	gst_element_set_state (gvc->priv->main_pipeline , GST_STATE_NULL);
    	gst_object_unref (gvc->priv->main_pipeline);
    	gvc->priv->main_pipeline = NULL;
  	}
  	
  
  	
  	if (gvc->priv->vencode_bin != NULL && GST_IS_ELEMENT (gvc->priv->vencode_bin )) {
    	gst_object_unref (gvc->priv->vencode_bin);
    	gvc->priv->vencode_bin = NULL;
  	}
  	
  	if (gvc->priv->aencode_bin != NULL && GST_IS_ELEMENT (gvc->priv->aencode_bin )) {
    	gst_object_unref (gvc->priv->aencode_bin);
    	gvc->priv->aencode_bin = NULL;
  	}
  	
  	if (gvc->priv->audio_bin != NULL && GST_IS_ELEMENT (gvc->priv->audio_bin )) {
	   	gst_object_unref (gvc->priv->audio_bin );
    	gvc->priv->audio_bin  = NULL;
  	}
  	
  	if (gvc->priv->video_bin != NULL && GST_IS_ELEMENT (gvc->priv->video_bin )) {
	   	gst_object_unref (gvc->priv->video_bin );
    	gvc->priv->video_bin  = NULL;
  	}
  	
  	if (gvc->priv->decode_bin != NULL && GST_IS_ELEMENT (gvc->priv->decode_bin )) {
	   	gst_object_unref (gvc->priv->decode_bin );
    	gvc->priv->decode_bin  = NULL;
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
      //gvc_video_capturer_set_display_height (gvc,
     // g_value_get_uint (value));
      break;
    case PROP_DISPLAY_WIDTH:
      //gvc_video_capturer_set_display_width (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_ENCODE_HEIGHT:
      //gvc_video_capturer_set_encode_height (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_ENCODE_WIDTH:
      //gvc_video_capturer_set_encode_width (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_VIDEO_BITRATE:
      //gvc_video_capturer_set_video_bit_rate (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_AUDIO_BITRATE:
      //gvc_video_capturer_set_audio_bit_rate (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_OUTPUT_FILE:
      //gvc_video_capturer_set_output_file(gvc,
      //g_value_get_string (value));
      break;
    case PROP_INPUT_FILE:
      //gvc_video_capturer_set_input_file (gvc,
      //g_value_get_string (value));
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
                                                        
                                                        
  /* Signals */
  gvc_signals[SIGNAL_ERROR] =
    g_signal_new ("error",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (GstVideoCapturerClass, error),
                  NULL, NULL,
                  g_cclosure_marshal_VOID__STRING,
                  G_TYPE_NONE, 1, G_TYPE_STRING);

  gvc_signals[SIGNAL_EOS] =
    g_signal_new ("eos",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (GstVideoCapturerClass, eos),
                  NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);
                                                        
}

void
gvc_video_capturer_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}



void 
gvc_video_capturer_new (GvcUseType use_type, gchar **error )
{
	GstElement *source = NULL;
	GstElement *conv = NULL;
	GstPad *audiopad = NULL;
	GstElement *audiosink = NULL;
	GstPad *videopad = NULL;
	GstElement *deinterlacer = NULL;
	GstElement *videoscale = NULL;
	GstElement *videofilter = NULL;
	GstElement *videosink = NULL;
	GstElement *ffmpegcolorspace = NULL;	
	GstVideoCapturer *gvc = NULL;
	
	
	gvc = g_object_new(GST_TYPE_VIDEO_CAPTURER, NULL);
	
	gvc->priv->input_file = "/dev/null";
	gvc->priv->output_file = "/dev/null";
	
	gvc->priv->display_height= 576;
	gvc->priv->display_width= 720;
	gvc->priv->encode_height= 576;
	gvc->priv->encode_width= 720;
	gvc->priv->audio_bitrate= 128;
	gvc->priv->video_bitrate= 5000000;
	
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
  	
  	/* Setup*/
  	source = gst_element_factory_make ("dv1394src", "dvsource");  	
  	g_object_set (source,"use-avc", FALSE,NULL);
  	gvc->priv->decode_bin = gst_element_factory_make ("decodebin", "decoder");  	
	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));    	
  	g_signal_connect (gvc->priv->decode_bin, "new-decoded-pad",G_CALLBACK(new_decoded_pad_cb),gvc); 	
  	gst_bin_add_many(GST_BIN(gvc->priv->main_pipeline),source,gvc->priv->decode_bin,NULL);
  	gst_element_link(GST_ELEMENT(source),gvc->priv->decode_bin);
  	
  	
  	 /* create audio output */  	 
  	gvc->priv->audio_bin = gst_bin_new ("audiobin");
  	conv = gst_element_factory_make ("audioconvert", "aconv");
  	audiopad = gst_element_get_pad (conv, "sink");
  	audiosink = gst_element_factory_make ("autoaudiosink", "sink");
  	gst_bin_add_many (GST_BIN(gvc->priv->audio_bin), conv, audiosink, NULL);
  	gst_element_link (conv, audiosink);
  	gst_element_add_pad (GST_ELEMENT(gvc->priv->audio_bin),
      	gst_ghost_pad_new ("sink", audiopad));
  	gst_object_unref (audiopad);
  	gst_object_unref (conv);
  	gst_object_unref (audiosink);  	
  	gst_bin_add (GST_BIN (gvc->priv->main_pipeline), gvc->priv->audio_bin);


	/*Create video output*/
	gvc->priv->video_bin= gst_bin_new ("videobin");
  	deinterlacer = gst_element_factory_make ("ffdeinterlace", "deinterlace");
  	videopad = gst_element_get_pad (deinterlacer, "sink");  	
  	videoscale = gst_element_factory_make ("videoscale", "videoscale");
  	videofilter = gst_element_factory_make ("capsfilter", "filterscale");
  	g_object_set (G_OBJECT(videofilter), "caps",gst_caps_new_simple ("audio/x-raw-yuv",
      "width", G_TYPE_INT, gvc->priv->display_width, NULL),NULL);
    ffmpegcolorspace = gst_element_factory_make ("ffdeinterlace", "deinterlace");
  	videosink = gst_element_factory_make ("xvimagesink", "videosink");
  	g_object_set (videosink,"sync", FALSE,NULL);
  	gst_bin_add_many (GST_BIN(gvc->priv->video_bin), deinterlacer, videoscale,videofilter,
  		ffmpegcolorspace, videosink, NULL);  	
  	gst_element_link_many ( deinterlacer,videoscale,videofilter,ffmpegcolorspace,videosink,NULL);
  	gst_element_add_pad (gvc->priv->video_bin,
      	gst_ghost_pad_new ("sink", GST_PAD(videopad)));
  	gst_object_unref (deinterlacer);
  	gst_object_unref (videoscale);
  	gst_object_unref (videofilter);
  	gst_object_unref (ffmpegcolorspace);
	gst_object_unref (videosink);
  	gst_bin_add (GST_BIN (gvc->priv->main_pipeline),gvc->priv->video_bin );


	
	
	
	/*Connect bus signals*/
  	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));
  
  	gst_bus_add_signal_watch (gvc->priv->bus);	
  	gvc->priv->sig_bus_async = 
      g_signal_connect (gvc->priv->bus, "message", 
                        G_CALLBACK (gvc_bus_message_cb),
                        gvc);
	
	g_object_unref(gvc);
	
	
}

void gst_video_encoder_set_encoder(GstVideoCapturer *gvc)
{
	GstElement *encoder = NULL;
	GstPad *encodepad = NULL;
	GstElement *videoscale = NULL;
	GstElement *videofilter = NULL;
	GstElement *queue = NULL;
	GstElement *muxer = NULL;
	GstElement *filesink = NULL;
	
	g_return_if_fail (GST_IS_VIDEO_CAPTURER(gvc));
	
	/*Create encodebin*/
	gvc->priv->vencode_bin= gst_bin_new ("encodebin");
  	queue = gst_element_factory_make ("queue", "encodequeue");
  	encodepad = gst_element_get_pad (queue, "sink");
  	videoscale = gst_element_factory_make ("videoscale", "videoscale");
  	videofilter = gst_element_factory_make ("capsfilter", "filterscale");
  	g_object_set (G_OBJECT(videofilter), "caps",gst_caps_new_simple ("audio/x-raw-yuv",
      "width", G_TYPE_INT, gvc->priv->encode_width, NULL),NULL);
    encoder= gst_element_factory_make ("xvidenc", "xvidencoder");
    g_object_set (G_OBJECT(encoder), "bitrate",gvc->priv->video_bitrate,NULL);
    muxer = gst_element_factory_make("avimux","muxer");
  	filesink = gst_element_factory_make ("filesink", "filesink");
  	g_object_set (G_OBJECT(filesink), "location",gvc->priv->output_file,NULL);

  	gst_bin_add_many (GST_BIN(gvc->priv->vencode_bin), queue, videoscale , videofilter,
  		encoder, filesink, NULL);  	
  	gst_element_link_many (  queue, videoscale , videofilter,encoder, filesink,NULL);
  	gst_element_add_pad (gvc->priv->vencode_bin,
      	gst_ghost_pad_new ("sink", GST_PAD(encodepad)));
  	gst_object_unref (encoder);
  	gst_object_unref (encodepad);
  	gst_object_unref (muxer);
  	gst_object_unref (queue);
  	gst_object_unref (videoscale);
  	gst_object_unref (videofilter);
  	gst_object_unref (filesink);
  	
	
}

static void new_decoded_pad_cb (GstElement* object,
                                           GstPad* pad,
                                           gboolean arg1,
                                           gpointer user_data)
{
	GstCaps *caps=NULL;
	GstStructure *str =NULL;
	GstPad *audiopad=NULL;
	GstPad *videopad=NULL;
	GstVideoCapturer *gvc=NULL;
	

  	g_return_if_fail (GST_IS_VIDEO_CAPTURER(user_data));
 	gvc = GST_VIDEO_CAPTURER (user_data);

	

  /* check media type */
  caps = gst_pad_get_caps (pad);
  str = gst_caps_get_structure (caps, 0);
  
  
  if (g_strrstr (gst_structure_get_name (str), "audio")) {	  
    /* only link once */
	audiopad = gst_element_get_pad (gvc->priv->audio_bin, "sink");
	if (GST_PAD_IS_LINKED (audiopad)) {
    	g_object_unref (audiopad);
    	gst_caps_unref (caps);
    	g_object_unref (gvc);
    	return;
  	}
  	/* link 'n play*/
  	gst_pad_link (pad, audiopad);
  	g_object_unref (audiopad);
    gst_caps_unref (caps);
  }
  
  else if (g_strrstr (gst_structure_get_name (str), "video")){
	      /* only link once */
	videopad = gst_element_get_pad (gvc->priv->video_bin, "sink");
	if (GST_PAD_IS_LINKED (videopad)) {
    	g_object_unref (videopad);
    	gst_caps_unref (caps);
    	g_object_unref (gvc);
    	return;
  	}
  	/* link 'n play*/
  	gst_pad_link (pad, videopad);
  	g_object_unref (videopad);
  	gst_caps_unref (caps);
  }
  
  gst_caps_unref (caps);

                                   
}

static void gvc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{

}
