 /*GStreamer Video splitter Based On GNonlin
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

#define DEFAULT_VIDEO_ENCODER "ffenc_mpeg4"
#define DEFAULT_AUDIO_ENCODER "lame"
#define DEAFAULT_VIDEO_MUXER "avimux"

/* Signals */
enum
{
  SIGNAL_ERROR,
  SIGNAL_EOS,
  SIGNAL_PERCENT_COMPLETED,
  LAST_SIGNAL
};

/* Properties */
enum
{
  PROP_0, 
  PROP_VIDEO_BITRATE,
  PROP_AUDIO_BITRATE,
  PROP_OUTPUT_FILE, 
  PROP_ENABLE_AUDIO
};

struct GstVideoCapturerPrivate
{
	
	gint64 		last_stop;
	gint 		segments;

	gchar		*output_file;
	guint		audio_bitrate;
	guint		video_bitrate;
	
	GstElement 	*main_pipeline;	
		
	GstElement 	*gnl_composition;
	GstElement 	*identity;
	GstElement 	*videorate;	
	GstElement 	*queue;
    GstElement 	*video_encoder;
	GstElement 	*audio_encoder;
	GstElement 	*muxer;
	GstElement 	*file_sink;
	
	GstBus  	*bus;
	gulong   	sig_bus_async;
	
	gint 		update_id;

};

static int gvc_signals[LAST_SIGNAL] = { 0 };
static void gvc_error_msg (GstVideoCapturer * gvc, GstMessage * msg);
static void new_decoded_pad_cb (GstElement* object,GstPad* arg0,gpointer user_data);
static void gvc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data);
static void gst_video_capturer_get_property (GObject * object, guint property_id, GValue * value, GParamSpec * pspec);
static void gst_video_capturer_set_property (GObject * object, guint property_id,const GValue * value, GParamSpec * pspec);
static void gvc_element_msg_sync (GstBus *bus, GstMessage *msg, gpointer data);
static gboolean  gvc_query_timeout (GstVideoCapturer * gvc);
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
  
  	if (gvc->priv->main_pipeline != NULL && GST_IS_ELEMENT (gvc->priv->main_pipeline )) {
    	gst_element_set_state (gvc->priv->main_pipeline , GST_STATE_NULL);
    	gst_object_unref (gvc->priv->main_pipeline);
    	gvc->priv->main_pipeline = NULL;
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
  	
  	
  	g_object_class_install_property (object_class, PROP_VIDEO_BITRATE,
                                   g_param_spec_uint ("video_bitrate", NULL,
                                                         NULL, 100, G_MAXUINT,1000,
                                                         G_PARAM_READWRITE));
  	g_object_class_install_property (object_class, PROP_AUDIO_BITRATE,
                                   g_param_spec_uint ("audio_bitrate", NULL,
                                                         NULL, 12, G_MAXUINT,128,
                                                         G_PARAM_READWRITE));
                                                         
    g_object_class_install_property (object_class, PROP_AUDIO_BITRATE,
                                   g_param_spec_string ("output_file", NULL,
                                                         NULL, "",
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
                  
  gvc_signals[SIGNAL_PERCENT_COMPLETED] =
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

static void 
gst_video_capturer_set_video_bit_rate (GstVideoCapturer *gvc,gint bitrate)
{
	GstState cur_state;

	gvc->priv->video_bitrate= bitrate;
	gst_element_get_state (gvc->priv->video_encoder, &cur_state, NULL, 0);
    if (cur_state <= GST_STATE_READY) {
	    g_object_set (gvc->priv->video_encoder,"bitrate",bitrate,NULL);
    	GST_INFO ("Encoding video bitrate changed to :\n%d",bitrate);
   }
}

static void 
gst_video_capturer_set_audio_bit_rate (GstVideoCapturer *gvc,gint bitrate)
{
	//TODO Not implemented
   
   
}

static void 
gst_video_capturer_set_output_file (GstVideoCapturer *gvc,const char *output_file)
{
	GstState cur_state;

	gvc->priv->output_file = g_strdup(output_file);
	gst_element_get_state (gvc->priv->file_sink, &cur_state, NULL, 0);
    if (cur_state <= GST_STATE_READY) {
	    g_object_set (gvc->priv->file_sink,"location",gvc->priv->output_file,NULL);
    	GST_INFO ("Ouput File changed to :\n%s",gvc->priv->output_file);
   }
}
static void
gst_video_capturer_set_property (GObject * object, guint property_id,
                                 const GValue * value, GParamSpec * pspec)
{
	GstVideoCapturer *gvc;

	gvc = GST_VIDEO_CAPTURER (object);

 	switch (property_id) {  
    
    	case PROP_VIDEO_BITRATE:
      		gst_video_capturer_set_video_bit_rate (gvc,
      		g_value_get_uint (value));
      		break;
    	case PROP_AUDIO_BITRATE:
      		gst_video_capturer_set_audio_bit_rate (gvc,
      		g_value_get_uint (value));
      	break;
      	case PROP_OUTPUT_FILE:
      		gst_video_capturer_set_output_file (gvc,
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
    case PROP_AUDIO_BITRATE:
      g_value_set_uint (value,gvc->priv->audio_bitrate);
      break;
    case PROP_VIDEO_BITRATE:
      g_value_set_uint (value,gvc->priv->video_bitrate);
      break;
    default:
      G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      break;
  }
}



/* =========================================== */
/*                                             */
/*               Private Methods               */
/*                                             */
/* =========================================== */

static void
gvc_set_tick_timeout (GstVideoCapturer *gvc , guint msecs)
{
	g_return_if_fail (GST_IS_VIDEO_CAPTURER(gvc));

  	if (msecs > 0) {
    	GST_INFO ("adding tick timeout (at %ums)", msecs);
    	gvc->priv->update_id =
      		g_timeout_add (msecs, (GSourceFunc) gvc_query_timeout, gvc);
  }
}



/* =========================================== */
/*                                             */
/*                Callbacks                    */
/*                                             */
/* =========================================== */

static void 
new_decoded_pad_cb (GstElement* object,
                                           GstPad* pad,
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
	
	if (g_strrstr (gst_structure_get_name (str), "video")){

		videopad = gst_element_get_compatible_pad (gvc->priv->identity, pad, NULL);
		/* only link once */
		if (GST_PAD_IS_LINKED (videopad)) {
    		g_object_unref (videopad);
    		gst_caps_unref (caps);
    		return;
  		}
  		
  		/* link 'n play*/
  		GST_INFO ("Found video stream...");
    	gst_pad_link (pad,videopad);    	
		g_object_unref (videopad);

  }

  gst_caps_unref (caps);
}

static void 
gvc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
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
          gst_element_set_state (gvc->priv->main_pipeline, GST_STATE_READY);
      break;
    }
    case GST_MESSAGE_WARNING: {
      GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      break;
    }
    case GST_MESSAGE_EOS:{
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

static gboolean  
gvc_query_timeout (GstVideoCapturer * gvc)
{
}


/* =========================================== */
/*                                             */
/*              Public Methods                 */
/*                                             */
/* =========================================== */

void 
gst_video_capturer_add_segment (GstVideoCapturer *gvc , gchar *file, gint64 start, gint64 duration, gdouble rate, gchar *title)
{
	
	
	GstState cur_state;
	GstElement *gnl_filesource;
	gchar *element_name;
	gint64 final_duration;
	
	
	g_return_if_fail (GST_IS_VIDEO_CAPTURER(gvc));
	GST_INFO("Adding new segment");
	gst_element_get_state (gvc->priv->gnl_composition, &cur_state, NULL, 0);
    if (cur_state <= GST_STATE_READY) {	  
    	final_duration = GST_MSECOND * duration / rate;
       	element_name = g_strdup_printf("filesource%d",gvc->priv->segments);
		gnl_filesource = gst_element_factory_make ("gnlfilesource", element_name);
		g_object_set (G_OBJECT(gnl_filesource), "location",file,NULL);
		g_object_set (G_OBJECT(gnl_filesource), "media-start",GST_MSECOND*start,NULL);
		g_object_set (G_OBJECT(gnl_filesource), "media-duration",GST_MSECOND*duration,NULL);
		g_object_set (G_OBJECT(gnl_filesource), "start",gvc->priv->last_stop,NULL);
		g_object_set (G_OBJECT(gnl_filesource), "duration",final_duration,NULL);
		gvc->priv->last_stop += final_duration;		
		gst_bin_add (GST_BIN(gvc->priv->gnl_composition), gnl_filesource);
		gvc->priv->segments ++;
		GST_INFO("New segment: start={%" GST_TIME_FORMAT "} duration={%" GST_TIME_FORMAT "} ",GST_TIME_ARGS(start * GST_MSECOND), GST_TIME_ARGS(duration * GST_MSECOND));
    }
    else
    	GST_WARNING("Segments can only be defined in GST_STATE_NULL or GST_STATE_READY state");
	g_free(element_name);
	
}

void 
gst_video_capturer_start(GstVideoCapturer *gvc)
{
	
	g_return_if_fail (GST_IS_VIDEO_CAPTURER(gvc));
	
	gst_element_set_state(gvc->priv->main_pipeline, GST_STATE_PLAYING);
	
}

void
gst_video_capturer_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}

GstVideoCapturer *
gst_video_capturer_new (GError ** err)
{
	GstElement *muxer = NULL;
	GstVideoCapturer *gvc = NULL;
	GstElement *queue = NULL;

	gvc = g_object_new(GST_TYPE_VIDEO_CAPTURER, NULL);

	gvc->priv->last_stop = 0;
	gvc->priv->segments = 0;
	
	gvc->priv->output_file = "new_video";

	/*Handled by Properties?*/
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
  	
    gvc->priv->identity = gst_element_factory_make ("identity", "identity");
    g_object_set (G_OBJECT(gvc->priv->identity), "single-segment",TRUE,NULL);
    
    gvc->priv->videorate = gst_element_factory_make ("videorate", "videorate");  
      
    gvc->priv->queue =  gst_element_factory_make ("queue", "queue"); 
    
    gvc->priv->video_encoder= gst_element_factory_make (DEFAULT_VIDEO_ENCODER, "videoencoder");
    g_object_set (G_OBJECT(gvc->priv->video_encoder), "bitrate",gvc->priv->video_bitrate,NULL); 
    
    gvc->priv->muxer = gst_element_factory_make (DEAFAULT_VIDEO_MUXER, "videomuxer");
    
	gvc->priv->file_sink = gst_element_factory_make ("filesink", "filesink");	
	g_object_set (G_OBJECT(gvc->priv->file_sink), "location",gvc->priv->output_file ,NULL); 

	gst_bin_add_many (	GST_BIN (gvc->priv->main_pipeline),
						gvc->priv->gnl_composition,
						gvc->priv->identity,
						gvc->priv->videorate,
						gvc->priv->queue,
						gvc->priv->video_encoder,
						gvc->priv->muxer,						
						gvc->priv->file_sink,
						NULL
						);
	gst_element_link_many(	gvc->priv->identity,
							gvc->priv->videorate,
							gvc->priv->queue,
							gvc->priv->video_encoder,
							gvc->priv->muxer,						
							gvc->priv->file_sink,
							NULL
							);
   
	/*Connect bus signals*/
    /*We have to wait for a "new-decoded-pad" message to link the composition with
    the encoder tail*/
	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));
  	g_signal_connect (gvc->priv->gnl_composition, "pad-added",G_CALLBACK(new_decoded_pad_cb),gvc);
	gst_bus_add_signal_watch (gvc->priv->bus);
  	gvc->priv->sig_bus_async =	g_signal_connect (gvc->priv->bus, "message",
                        		G_CALLBACK (gvc_bus_message_cb),
                        		gvc);
	gst_element_set_state(gvc->priv->main_pipeline,GST_STATE_READY);
	
	return gvc;
}
