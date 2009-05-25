 /* GStreamer Non Linear Video Editor Based On GNonlin
 * Copyright (C)  Andoni Morales Alastruey 2009 <ylatuya@gmail.com>
 *
 * This program is free software.
 *
 * You may redistribute it and/or modify it under the terms of the
 * GNU General Public License, as published by the Free Software
 * Foundation; either version 2 of the License, or (at your option)
 * any later version.
 *
 * Gstreamer Video Transcoderis distributed in the hope that it will be useful,
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

#include <string.h>
#include <stdio.h>

#include <gst/gst.h>

#include "gst-video-capturer.h"

#define DEFAULT_VIDEO_ENCODER "schroenc"
#define DEFAULT_AUDIO_ENCODER "vorbisenc"
#define DEAFAULT_VIDEO_MUXER "matroskamux"

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_STATE_CHANGED,
  SIGNAL_PERCENT_COMPLETED,
  LAST_SIGNAL
};

/* Properties */
enum
{
  PROP_0,
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

	gchar	*output_file;
	gchar	*input_file;
	guint   encode_height;
	guint   encode_width;
	guint	audio_bitrate;
	guint	video_bitrate;
	
	GstElement *main_pipeline;	
	GstElement *decode_bin;
	GstElement *vencode_bin;
	GstElement *aencode_bin;
	GstElement *filesink_bin;	
	
	
	GstElement *gnl_filesource;
	GstElement *gnl_composition;
	
    GstElement *video_encoder;
	GstElement *audio_encoder;
	GstElement *muxer;
	GstElement *file_sink;
	
	GstBus                      *bus;
	gulong              sig_bus_async;

};

static int gvc_signals[LAST_SIGNAL] = { 0 };
static void gvc_error_msg (GstVideoCapturer * gvc, GstMessage * msg);
static void new_decoded_pad_cb (GstElement* object,GstPad* arg0,gboolean arg1,gpointer user_data);
static void gvc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data);
static void gst_video_capturer_get_property (GObject * object, guint property_id, GValue * value, GParamSpec * pspec);
static void gst_video_capturer_set_property (GObject * object, guint property_id,const GValue * value, GParamSpec * pspec);
static void gvc_element_msg_sync (GstBus *bus, GstMessage *msg, gpointer data);
static void gvc_set_video_encode_bin (GstVideoCapturer *gvc);
static void gvc_set_audio_encode_bin (GstVideoCapturer *gvc);
static void gvc_set_filesink_bin (GstVideoCapturer *gvc);

G_DEFINE_TYPE (GstVideoCapturer, gst_video_capturer, G_TYPE_OBJECT);



/* =========================================== */
/*                                             */
/*      Class Initialization/Finalization      */
/*                                             */
/* =========================================== */

static void
gst_video_capturer_init (GstVideoCapturer *object)
{
	GstVideoCapturerPrivate *priv;
  	object->priv = priv = G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_VIDEO_CAPTURER, GstVideoCapturerPrivate);
}

static void
gst_video_capturer_finalize (GObject *object)
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

   	if (gvc->priv->decode_bin != NULL && GST_IS_ELEMENT (gvc->priv->decode_bin )) {
	   	gst_object_unref (gvc->priv->decode_bin );
    	gvc->priv->decode_bin  = NULL;
  	}

	G_OBJECT_CLASS (gst_video_capturer_parent_class)->finalize (object);
}


static void
gst_video_capturer_class_init (GstVideoCapturerClass *klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS (klass);

	object_class->finalize = gst_video_capturer_finalize;

	g_type_class_add_private (object_class, sizeof (GstVideoCapturerPrivate));

	 /* GObject */
  	object_class->set_property = gst_video_capturer_set_property;
 	object_class->get_property = gst_video_capturer_get_property;
 	object_class->finalize = gst_video_capturer_finalize;

 	/* Properties */
  	
  	g_object_class_install_property (object_class, PROP_ENCODE_HEIGHT,
                                   g_param_spec_uint ("encode_height", NULL,
                                                     NULL,180 , 5600, 576,
                                                     G_PARAM_READWRITE));
  	g_object_class_install_property (object_class, PROP_ENCODE_WIDTH,
                                   g_param_spec_uint ("encode_width", NULL,
                                                         NULL, 180,5600,780,
                                                         G_PARAM_READWRITE));
  	g_object_class_install_property (object_class, PROP_VIDEO_BITRATE,
                                   g_param_spec_uint ("video_bitrate", NULL,
                                                         NULL, 100, G_MAXUINT,1000,
                                                         G_PARAM_READWRITE));
  	g_object_class_install_property (object_class, PROP_AUDIO_BITRATE,
                                   g_param_spec_uint ("audio_bitrate", NULL,
                                                         NULL, 12, G_MAXUINT,128,
                                                         G_PARAM_READWRITE));
 	g_object_class_install_property (object_class, PROP_OUTPUT_FILE,
                                   g_param_spec_string ("output_file", NULL,
                                                        NULL, FALSE,
                                                        G_PARAM_READWRITE));
                                                        
    g_object_class_install_property (object_class, PROP_INPUT_FILE,
                                   g_param_spec_string ("input_file", NULL,
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
                  
  gvc_signals[SIGNAL_ERROR] =
    g_signal_new ("percent_completed",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (GstVideoCapturerClass, error),
                  NULL, NULL,
                  g_cclosure_marshal_VOID__INT,
                  G_TYPE_NONE, 1, G_TYPE_STRING);

  gvc_signals[SIGNAL_EOS] =
    g_signal_new ("eos",
                  G_TYPE_FROM_CLASS (object_class),
                  G_SIGNAL_RUN_LAST,
                  G_STRUCT_OFFSET (GstVideoCapturerClass, eos),
                  NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

}

/* =========================================== */
/*                                             */
/*                Properties                   */
/*                                             */
/* =========================================== */



static void gst_video_capturer_set_encode_width (GstVideoCapturer *gvc,gint width)
{


}

static void gst_video_capturer_set_encode_height (GstVideoCapturer *gvc,gint height)
{

}

static void gst_video_capturer_set_video_bit_rate (GstVideoCapturer *gvc,gint bitrate)
{
	GstState cur_state;

	gvc->priv->video_bitrate= bitrate;
	gst_element_get_state (gvc->priv->vencode_bin, &cur_state, NULL, 0);
    if (cur_state <= GST_STATE_READY) {
	    g_object_set (gvc->priv->video_encoder,"bitrate",bitrate,NULL);
    	GST_INFO ("Encoding video bitrate changed to :\n%d",bitrate);
   }
}

static void gst_video_capturer_set_audio_bit_rate (GstVideoCapturer *gvc,gint bitrate)
{
	GstState cur_state;

	gvc->priv->audio_bitrate= bitrate;
	gst_element_get_state (gvc->priv->aencode_bin, &cur_state, NULL, 0);
    if (cur_state <= GST_STATE_READY) {
	    g_object_set (gvc->priv->audio_encoder,"bitrate",bitrate,NULL);
    	GST_INFO ("Encoding audio bitrate to :\n%d",bitrate);
   }
}

static void gst_video_capturer_set_output_file(GstVideoCapturer *gvc,const gchar *file)
{
	GstState cur_state;

	gvc->priv->output_file = g_strdup(file);
	gst_element_get_state (gvc->priv->vencode_bin, &cur_state, NULL, 0);
    if (cur_state == GST_STATE_NULL) {
	    g_object_set (gvc->priv->file_sink,"location",file,NULL);
    	GST_INFO ("Output file changed to :\n%s",file);

   }
}

static void gst_video_capturer_set_input_file(GstVideoCapturer *gvc,const gchar *file)
{
	GstState cur_state;

	gvc->priv->input_file = g_strdup(file);
	gst_element_get_state (gvc->priv->vencode_bin, &cur_state, NULL, 0);
    if (cur_state == GST_STATE_NULL) {
	    g_object_set (gvc->priv->gnl_filesource,"location",file,NULL);
    	GST_INFO ("Input file changed to :\n%s",file);

   }
}


static void
gst_video_capturer_set_property (GObject * object, guint property_id,
                                 const GValue * value, GParamSpec * pspec)
{
	GstVideoCapturer *gvc;

	gvc = GST_VIDEO_CAPTURER (object);

  switch (property_id) {
    
    case PROP_ENCODE_HEIGHT:
      gst_video_capturer_set_encode_height (gvc,
      g_value_get_uint (value));
      break;
    case PROP_ENCODE_WIDTH:
      gst_video_capturer_set_encode_width (gvc,
      g_value_get_uint (value));
      break;
    case PROP_VIDEO_BITRATE:
      gst_video_capturer_set_video_bit_rate (gvc,
      g_value_get_uint (value));
      break;
    case PROP_AUDIO_BITRATE:
      gst_video_capturer_set_audio_bit_rate (gvc,
      g_value_get_uint (value));
      break;
    case PROP_OUTPUT_FILE:
      gst_video_capturer_set_output_file(gvc,
      g_value_get_string (value));
      break;
      
    case PROP_INPUT_FILE:
      gst_video_capturer_set_input_file(gvc,
      g_value_get_string (value));
      break;
   
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}

static void
gst_video_capturer_get_property (GObject * object, guint property_id,
                                 GValue * value, GParamSpec * pspec)
{
  GstVideoCapturer *gvc;

  gvc = GST_VIDEO_CAPTURER (object);

  switch (property_id) {
    case PROP_ENCODE_HEIGHT:
      g_value_set_uint (value,gvc->priv->encode_height);
      break;
   case PROP_AUDIO_BITRATE:
      g_value_set_uint (value,gvc->priv->audio_bitrate);
      break;
    case PROP_VIDEO_BITRATE:
      g_value_set_uint (value,gvc->priv->video_bitrate);
      break;
    case PROP_OUTPUT_FILE:
      g_value_set_string (value,gvc->priv->output_file);
      break;
    case PROP_INPUT_FILE:
      g_value_set_string (value,gvc->priv->input_file);
      break;
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}



void
gst_video_capturer_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

GstVideoCapturer *
gst_video_capturer_new (GError ** err)
{

	GstPad *videoteesrcpad=NULL;
	GstPad *audioteesrcpad=NULL;
	GstPad *pad=NULL;

	GstVideoCapturer *gvc = NULL;

	gvc = g_object_new(GST_TYPE_VIDEO_CAPTURER, NULL);


	gvc->priv->output_file = "/dev/null";

	/*Handled by Properties?*/
	gvc->priv->encode_height= 720;
	gvc->priv->encode_width= 1280;
	gvc->priv->audio_bitrate= 128;
	gvc->priv->video_bitrate= 5000000;
	
	gvc->priv->main_pipeline = gst_pipeline_new ("main_pipeline");

	if (!gvc->priv->main_pipeline ) {
    	/*g_set_error (err, GVC_ERROR, GVC_ERROR_PLUGIN_LOAD,
                	("Failed to create a GStreamer Bin. "
                    "Please check your GStreamer installation."));*/
    	g_object_ref_sink (gvc);
    	g_object_unref (gvc);
    	return NULL;
  	}

  	/* Setup*/

  	gvc->priv->gnl_composition = gst_element_factory_make("gnlcomposition","gnlcomposition");
 	gvc->priv->gnl_filesource = gst_element_factory_make ("gnlfilesource","gnlfilesource");
 	gst_bin_add (GST_BIN(gvc->priv->gnl_composition),gvc->priv->gnl_filesource);
	 
 
    GST_INFO("Initializing encoder");
    gvc_set_video_encoder_bin(gvc);
    gvc_set_audio_encoder_bin(gvc);
    gvc_set_filesink_bin(gvc);     

  	gst_bin_add_many(GST_BIN (gvc->priv->main_pipeline),gvc->priv->gnl_composition,NULL );


   
	/*Connect bus signals*/
	g_print("Initializing signals bus");

    /*We have to wait for a "new-decoded-pad" message to link the composition with
    the encoder tail*/
	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));
  	g_signal_connect (gvc->priv->gnl_composition, "new-decoded-pad",G_CALLBACK(new_decoded_pad_cb),gvc);
	gst_bus_add_signal_watch (gvc->priv->bus);
  	gvc->priv->sig_bus_async =	g_signal_connect (gvc->priv->bus, "message",
                        		G_CALLBACK (gvc_bus_message_cb),
                        		gvc);

	return gvc;


}

static void gvc_set_audio_encode_bin(GstVideoCapturer *gvc)
{
	GstElement *audioqueue = NULL;
	GstPad *sinkpad = NULL;
	GstPad *sourcepad = NULL;
	
	 g_return_if_fail(GST_IS_VIDEO_CAPTURER(gvc));

    gvc->priv->aencode_bin= gst_bin_new ("audoencodebin");
    
    audioqueue = gst_element_factory_make ("queue", "audioqueue");    
 
    gvc->priv->audio_encoder= gst_element_factory_make (DEFAULT_AUDIO_ENCODER, "audioencoder");
    g_object_set (G_OBJECT(gvc->priv->audio_encoder), "bitrate",gvc->priv->audio_bitrate,NULL);
    
    gst_bin_add_many (GST_BIN(gvc->priv->aencode_bin),audioqueue,gvc->priv->audio_encoder,NULL);
    gst_element_link_many(audioqueue,gvc->priv->audio_encoder, NULL);
    
    sinkpad = gst_element_get_pad (audioqueue, "sink");  
    gst_element_add_pad (gvc->priv->aencode_bin,
    					gst_ghost_pad_new ("sink", GST_PAD(sinkpad)));
    
    sourcepad = gst_element_get_pad (gvc->priv->audio_encoder, "src");
    gst_element_add_pad (gvc->priv->aencode_bin,
   						gst_ghost_pad_new ("src", GST_PAD(sourcepad)));

    gst_object_unref(sinkpad);
    gst_onbject_unref(sourcepad);
    
}


static void gvc_set_video_encode_bin (GstVideoCapturer *gvc)
{
   	GstElement *videoqueue = NULL;
	GstPad *sinkpad = NULL;
	GstPad *sourcepad = NULL;
	

    g_return_if_fail(GST_IS_VIDEO_CAPTURER(gvc));

    gvc->priv->vencode_bin= gst_bin_new ("videoencodebin");
	
    videoqueue = gst_element_factory_make ("queue", "encodequeue");    

    gvc->priv->video_encoder= gst_element_factory_make (DEFAULT_VIDEO_ENCODER, "videoencoder");
    g_object_set (G_OBJECT(gvc->priv->video_encoder), "bitrate",gvc->priv->video_bitrate,NULL);

    gst_bin_add_many (GST_BIN(gvc->priv->vencode_bin),videoqueue,gvc->priv->video_encoder,NULL);
    gst_element_link_many (videoqueue,gvc->priv->video_encoder,NULL);

	sinkpad = gst_element_get_pad (videoqueue, "sink");
    gst_element_add_pad(gvc->priv->vencode_bin,
    					gst_ghost_pad_new ("sink", GST_PAD(sinkpad)));
    
    sourcepad = gst_element_get_pad (gvc->priv->video_encoder, "src");
    gst_element_add_pad(gvc->priv->vencode_bin,
    					gst_ghost_pad_new ("src", GST_PAD(sourcepad)));
    
    gst_object_unref(sinkpad);
    gst_onbject_unref(sourcepad);

}

static void gvc_set_filesink_bin (GstVideoCapturer *gvc)

{
	GstElement *muxer = NULL;
	GstElement *filesink = NULL;
	GstPad *videosinkpad = NULL;
	GstPad *audiosinkpad = NULL;
	
	g_return_if_fail(GST_IS_VIDEO_CAPTURER(gvc));
	
	gvc->priv->filesink_bin = gst_bin_new ("filesinkbin");
	
	muxer = gst_element_factory_make (DEAFAULT_VIDEO_MUXER, "videomuxer");
	filesink = gst_element_factory_make ("filesink", "filesink");
	
	gst_bin_add_many(GST_BIN(gvc->priv->filesink_bin), muxer, filesink, NULL);
	gst_element_link(muxer, filesink);
	
	videosinkpad = gst_element_get_pad (muxer, "video_0");
    gst_element_add_pad (gvc->priv->filesink_bin,
    					gst_ghost_pad_new ("videosink", GST_PAD(videosinkpad)));
    
    audiosinkpad = gst_element_get_pad (muxer, "audio_0");
    gst_element_add_pad (gvc->priv->filesink_bin,
    					gst_ghost_pad_new ("audiosink", GST_PAD(audiosinkpad)));
    
    gst_object_unref(audiosinkpad);
    gst_object_unref(videosinkpad);
	
	
}

static void gvc_link_audio_to_file_sink(GstVideoCapturer *gvc){
	
	GstPad *audiosrcpad;
	GstPad *sinkpad;
	
	g_return_if_fail (GST_IS_VIDEO_CAPTURER(gvc));
	
	audiosrcpad = gst_element_get_pad (gvc->priv->aencode_bin, "src");
	sinkpad = gst_element_get_pad (gvc->priv->filesink_bin, "audio_sink");

	gst_pad_link (audiosrcpad, sinkpad);
	
	gst_object_unref(audiosrcpad);
	gst_object_unref (sinkpad);
	
	
}

static void gvc_link_video_to_file_sink(GstVideoCapturer *gvc){
	
	GstPad *videosrcpad;
	GstPad *sinkpad;
	
	g_return_if_fail (GST_IS_VIDEO_CAPTURER(gvc));
	
	videosrcpad = gst_element_get_pad (gvc->priv->vencode_bin, "src");
	sinkpad = gst_element_get_pad (gvc->priv->filesink_bin, "video_sink");

	gst_pad_link (videosrcpad, sinkpad);
	
	gst_object_unref(videosrcpad);
	gst_object_unref (sinkpad);
	
	
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
		audiopad = gst_element_get_pad (gvc->priv->aencode_bin, "sink");
		if (GST_PAD_IS_LINKED (audiopad)) {
    		g_object_unref (audiopad);
    		gst_caps_unref (caps);
    		g_object_unref (gvc);
    		return;
  		}

  		/* link 'n play*/
  		gst_pad_link (pad, audiopad);
  		gvc_link_audio_to_file_sink(gvc);
  		g_object_unref (audiopad);
  	}
	
	else if (g_strrstr (gst_structure_get_name (str), "video")){

		videopad = gst_element_get_pad (gvc->priv->vencode_bin, "sink");

		/* only link once */
		if (GST_PAD_IS_LINKED (videopad)) {
    		g_object_unref (videopad);
    		gst_caps_unref (caps);
    		return;
  		}
  		
  		/* link 'n play*/
    	gst_pad_link (pad,videopad);
    	gvc_link_video_to_file_sink(gvc);
		g_object_unref (videopad);

  }

  gst_caps_unref (caps);
}

static void gvc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  GstVideoCapturer *gvc = (GstVideoCapturer *) data;
  GstMessageType msg_type;

  g_return_if_fail (gvc!= NULL);
  g_return_if_fail (GST_IS_VIDEO_CAPTURER (gvc));

  msg_type = GST_MESSAGE_TYPE (message);

  switch (msg_type) {
    case GST_MESSAGE_ERROR: {

      gvc_error_msg (gvc, message);
      if (gvc->priv->main_pipeline)
          gst_element_set_state (gvc->priv->main_pipeline, GST_STATE_NULL);
      break;
    }
    case GST_MESSAGE_WARNING: {
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;
    }
    case GST_MESSAGE_EOS:{
      g_print ("EOS message");
      g_signal_emit (gvc, gvc_signals[SIGNAL_EOS], 0);
      break;
    }

    default:
      GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      break;
  }
 }

static void
gvc_error_msg (GstVideoCapturer * gvc, GstMessage * msg)
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


    g_message ("Error: %s\n%s\n", GST_STR_NULL (err->message),
        GST_STR_NULL (dbg));
    g_signal_emit (gvc, gvc_signals[SIGNAL_ERROR], 0,
                       err->message);
    g_error_free (err);
  }
  g_free (dbg);
}



