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
#include <gst/interfaces/xoverlay.h>
#include <gst/gst.h>
#include "gstvideowidget.h"
#include <string.h>
#include <gst/video/video.h>

/* gtk+/gnome */
#ifdef WIN32
	#include <gdk/gdkwin32.h>
#else
	#include <gdk/gdkx.h>
#endif


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

	gint                         video_width; /* Movie width */
  	gint                         video_height; /* Movie height */
  	const GValue                *movie_par; /* Movie pixel aspect ratio */
  	gint                         video_width_pixels; /* Scaled movie width */
  	gint                         video_height_pixels; /* Scaled movie height */
  	gint                         video_fps_n;
  	gint                         video_fps_d;
  	
	gboolean logo_mode;
	GdkPixbuf *logo_pixbuf;

	GstElement *video_bin;
	GstElement *audio_bin;
	GstElement *vencode_bin;
	GstElement *aencode_bin;
	GstElement *decode_bin;
	GstElement *main_pipeline;
	
	gboolean media_has_video;
	gboolean media_has_audio;
	
	GstXOverlay                 *xoverlay; /* protect with lock */
    GMutex                      *lock;
	
	GvcVideoEncoderType *video_encoder;
	GvcAudioEncoderType	*audio_encoder;
	
	GtkWidget *video_window;
	
	GstBus                      *bus;
	gulong              sig_bus_async;
	gulong             sig_bus_sync;
};

static int gvc_signals[LAST_SIGNAL] = { 0 };
static void gvc_error_msg (GstVideoCapturer * gvc, GstMessage * msg);
static void new_decoded_pad_cb (GstElement* object,GstPad* arg0,gboolean arg1,gpointer user_data);
static void gvc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data);
static void gst_video_capturer_get_property (GObject * object, guint property_id, GValue * value, GParamSpec * pspec);
static void gst_video_capturer_set_property (GObject * object, guint property_id,const GValue * value, GParamSpec * pspec);
static void gvc_element_msg_sync (GstBus *bus, GstMessage *msg, gpointer data);
static void get_media_size (GstVideoCapturer *gvc, gint *width, gint *height);
static void gvc_handle_application_message (GstVideoCapturer *gvc, GstMessage *msg);
static void gvc_update_interface_implementations (GstVideoCapturer *gvc);
static void decodebin_stream_info_notify_cb (GObject * obj, GParamSpec * pspec, gpointer data);
static GList * get_stream_info_objects_for_type (GstVideoCapturer * gvc,const gchar * typestr);
    
G_DEFINE_TYPE (GstVideoCapturer, gst_video_capturer, GTK_TYPE_WIDGET);




static void
gst_video_capturer_init (GstVideoCapturer *object)
{
	GstVideoCapturerPrivate *priv;
  	object->priv = priv = G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_VIDEO_CAPTURER, GstVideoCapturerPrivate);
    priv->lock = g_mutex_new ();
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
  	
  	g_mutex_free (gvc->priv->lock);
  	


	G_OBJECT_CLASS (gst_video_capturer_parent_class)->finalize (object);
}

static void
gst_video_capturer_set_property (GObject * object, guint property_id,
                                 const GValue * value, GParamSpec * pspec)
{
   GstVideoCapturer *gvc;

  gvc = GST_VIDEO_CAPTURER (object);

  
  
  switch (property_id) {
    case PROP_DISPLAY_HEIGHT:
      //gst_video_capturer_set_display_height (gvc,
     // g_value_get_uint (value));
      break;
    case PROP_DISPLAY_WIDTH:
      //gst_video_capturer_set_display_width (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_ENCODE_HEIGHT:
      //gst_video_capturer_set_encode_height (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_ENCODE_WIDTH:
      //gst_video_capturer_set_encode_width (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_VIDEO_BITRATE:
      //gst_video_capturer_set_video_bit_rate (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_AUDIO_BITRATE:
      //gst_video_capturer_set_audio_bit_rate (gvc,
      //g_value_get_uint (value));
      break;
    case PROP_OUTPUT_FILE:
      //gst_video_capturer_set_output_file(gvc,
      //g_value_get_string (value));
      break;
    case PROP_INPUT_FILE:
      //gst_video_capturer_set_input_file (gvc,
      //g_value_get_string (value));
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
gst_video_capturer_class_init (GstVideoCapturerClass *klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS (klass);
	//GtkWidgetClass* parent_class = GTK_WIDGET_CLASS (klass);


	object_class->finalize = gst_video_capturer_finalize;
	
	g_type_class_add_private (object_class, sizeof (GstVideoCapturerPrivate));
	
	 /* GObject */
  	object_class->set_property = gst_video_capturer_set_property;
 	object_class->get_property = gst_video_capturer_get_property;
 	object_class->finalize = gst_video_capturer_finalize;
 	
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
gst_video_capturer_init_backend (int *argc, char ***argv)
{
  gst_init (argc, argv);
}


static void gvc_window_construct(int width, int weight,  GstVideoCapturer *gvc){
	
	//Create the Video Widget
	gvc->priv->video_window = gst_video_widget_new();
	gtk_container_add(GTK_CONTAINER(gvc),gvc->priv->video_window);
	gst_video_widget_set_minimum_size (GST_VIDEO_WIDGET (gvc->priv->video_window),
            width, weight);
	gst_video_widget_set_source_size (GST_VIDEO_WIDGET (gvc->priv->video_window), width,weight );
	/*g_signal_connect (G_OBJECT (gvc->priv->video_window), "expose_event",
		    G_CALLBACK (gvc_expose_event), gvc);*/


}

GstVideoCapturer *
gst_video_capturer_new (GvcUseType use_type, gchar **error )
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
	
	/*Handled by Properties?*/
	/*gvc->priv->display_height= 576;
	gvc->priv->display_width= 720;
	gvc->priv->encode_height= 576;
	gvc->priv->encode_width= 720;
	gvc->priv->audio_bitrate= 128;
	gvc->priv->video_bitrate= 5000000;*/
	
	gvc->priv->use_type = use_type;
	
	gvc_window_construct(gvc->priv->display_width,gvc->priv->display_height,gvc);
	
	gvc->priv->main_pipeline = gst_pipeline_new ("main_pipeline");
	
	if (!gvc->priv->main_pipeline) {
	 
    	/*g_set_error (err, gvc_ERROR, gvc_ERROR_PLUGIN_LOAD,
                	_("Failed to create a GStreamer Bin. "
                    "Please check your GStreamer installation."));*/
    	g_object_ref_sink (gvc);
    	g_object_unref (gvc);
    	return NULL;
  	}
  	
  	/* Setup*/
  	GST_INFO("Initializing decodebin");
  	source = gst_element_factory_make ("dv1394src", "dvsource");  	
  	g_object_set (source,"use-avc", FALSE,NULL);
  	gvc->priv->decode_bin = gst_element_factory_make ("decodebin", "decoder");  	
	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));    	
  	g_signal_connect (gvc->priv->decode_bin, "new-decoded-pad",G_CALLBACK(new_decoded_pad_cb),gvc); 	
  	gst_bin_add_many(GST_BIN(gvc->priv->main_pipeline),source,gvc->priv->decode_bin,NULL);
  	gst_element_link(GST_ELEMENT(source),gvc->priv->decode_bin);
  	
  	
  	GST_INFO("Initializing audiobin");
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


	GST_INFO("Initializing videobin");
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
	GST_INFO("Initializing signals bus");
  	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));
  
  	gst_bus_add_signal_watch (gvc->priv->bus);	
  	gvc->priv->sig_bus_async = 
      g_signal_connect (gvc->priv->bus, "message", 
                        G_CALLBACK (gvc_bus_message_cb),
                        gvc);
     g_signal_connect (gvc->priv->decode_bin, "notify::stream-info",
      G_CALLBACK (decodebin_stream_info_notify_cb), gvc);
      
    gvc_update_interface_implementations (gvc);

  

  /* we want to catch "prepare-xwindow-id" element messages synchronously */
  gst_bus_set_sync_handler (gvc->priv->bus, gst_bus_sync_signal_handler, gvc);

  gvc->priv->sig_bus_sync = 
      g_signal_connect (gvc->priv->bus, "sync-message::element",
                        G_CALLBACK (gvc_element_msg_sync), gvc);
	
	return gvc;
	
	
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
	if (gvc->priv->vencode_bin != NULL){
		GST_INFO("Initializing encoder");
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
  	
  	GST_INFO("Linking Audio Pad");
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
  	GST_INFO("Linking Video Pad");
  	gst_pad_link (pad, videopad);
  	g_object_unref (videopad);
  	gst_caps_unref (caps);
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
      GST_INFO ("EOS message");
      g_signal_emit (gvc, gvc_signals[SIGNAL_EOS], 0);
      break;
      }	

    case GST_MESSAGE_APPLICATION: {
      gvc_handle_application_message (gvc, message);
      break;
    }
    
    case GST_MESSAGE_STATE_CHANGED: {
      GstState old_state, new_state;
      gchar *src_name;

      gst_message_parse_state_changed (message, &old_state, &new_state, NULL);
	
      if (old_state == new_state)
        break;

      /* we only care about playbin (pipeline) state changes */
      if (GST_MESSAGE_SRC (message) != GST_OBJECT (gvc->priv->main_pipeline))
        break;

      src_name = gst_object_get_name (message->src);

      GST_INFO ("%s changed state from %s to %s", src_name,
          gst_element_state_get_name (old_state),
          gst_element_state_get_name (new_state));
      g_free (src_name);     
      break;
    }   

    /*case GST_MESSAGE_DURATION: {
      //force _get_stream_length() to do new duration query 
      gvc->priv->stream_length = 0;
      if (bacon_video_widget_get_stream_length (gvc) == 0) {
        GST_INFO ("Failed to query duration after DURATION message?!");
      }
      else
      	g_signal_emit (gvc, gvc_signals[SIGNAL_GOT_DURATION], 0, NULL);
      break;
    }*/

   
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

static void parse_stream_info (GstVideoCapturer *gvc);

static void
gvc_update_stream_info (GstVideoCapturer *gvc)
{
  parse_stream_info (gvc);

  /* if we're not interactive, we want to announce metadata
   * only later when we can be sure we got it all */
  /*if (gvc->priv->use_type == gvc_USE_TYPE_VIDEO ||
      gvc->priv->use_type == gvc_USE_TYPE_AUDIO) {
    g_signal_emit (gvc, gvc_signals[SIGNAL_GOT_METADATA], 0, NULL);
    g_signal_emit (gvc, gvc_signals[SIGNAL_CHANNELS_CHANGE], 0);
  }*/
}

static void
gvc_handle_application_message (GstVideoCapturer *gvc, GstMessage *msg)
{
  const gchar *msg_name;
  gint h;
  gint w;

  msg_name = gst_structure_get_name (msg->structure);
  g_return_if_fail (msg_name != NULL);

  GST_INFO ("Handling application message: %" GST_PTR_FORMAT, msg->structure);

  /*if (strcmp (msg_name, "notify-streaminfo") == 0) {
    gvc_update_stream_info (gvc);
  } */
  if (strcmp (msg_name, "notify-streaminfo") == 0) {
    gvc_update_stream_info (gvc);
  } 
  else if (strcmp (msg_name, "video-size") == 0) {
    /* if we're not interactive, we want to announce metadata
     * only later when we can be sure we got it all */
   
      //g_signal_emit (gvc, gvc_signals[SIGNAL_GOT_METADATA], 0, NULL);
    

      get_media_size (gvc, &w, &h);
      gst_video_widget_set_source_size (GST_VIDEO_WIDGET(gvc->priv->video_window), w, h);
   
  } else {
    g_message ("Unhandled application message %s", msg_name);
  }
}


static void
get_media_size (GstVideoCapturer *gvc, gint *width, gint *height)
{
  if (gvc->priv->logo_mode) {
    if (gvc->priv->logo_pixbuf) {
      *width = gdk_pixbuf_get_width (gvc->priv->logo_pixbuf);
      *height = gdk_pixbuf_get_height (gvc->priv->logo_pixbuf);
    } else {
      *width = 0;
      *height = 0;
    }
  } else {
    if (gvc->priv->media_has_video) {
      GValue * disp_par = NULL;
      guint movie_par_n, movie_par_d, disp_par_n, disp_par_d, num, den;
      
      /* Create and init the fraction value */
      disp_par = g_new0 (GValue, 1);
      g_value_init (disp_par, GST_TYPE_FRACTION);

      /* Square pixel is our default */
      gst_value_set_fraction (disp_par, 1, 1);
    
      /* Now try getting display's pixel aspect ratio */
      if (gvc->priv->xoverlay) {
        GObjectClass *klass;
        GParamSpec *pspec;

        klass = G_OBJECT_GET_CLASS (gvc->priv->xoverlay);
        pspec = g_object_class_find_property (klass, "pixel-aspect-ratio");
      
        if (pspec != NULL) {
          GValue disp_par_prop = { 0, };

          g_value_init (&disp_par_prop, pspec->value_type);
          g_object_get_property (G_OBJECT (gvc->priv->xoverlay),
              "pixel-aspect-ratio", &disp_par_prop);

          if (!g_value_transform (&disp_par_prop, disp_par)) {
            GST_WARNING ("Transform failed, assuming pixel-aspect-ratio = 1/1");
            gst_value_set_fraction (disp_par, 1, 1);
          }
        
          g_value_unset (&disp_par_prop);
        }
      }
      
      disp_par_n = gst_value_get_fraction_numerator (disp_par);
      disp_par_d = gst_value_get_fraction_denominator (disp_par);
      
      GST_INFO ("display PAR is %d/%d", disp_par_n, disp_par_d);
      
   
     
        /* Use the movie pixel aspect ratio if any */
        if (gvc->priv->movie_par) {
          movie_par_n = gst_value_get_fraction_numerator (gvc->priv->movie_par);
          movie_par_d =
              gst_value_get_fraction_denominator (gvc->priv->movie_par);
        }
        else {
          /* Square pixels */
          movie_par_n = 1;
          movie_par_d = 1;
        }
      
      
      GST_INFO ("movie PAR is %d/%d", movie_par_n, movie_par_d);

      if (!gst_video_calculate_display_ratio (&num, &den,
          gvc->priv->video_width, gvc->priv->video_height,
          movie_par_n, movie_par_d, disp_par_n, disp_par_d)) {
        GST_WARNING ("overflow calculating display aspect ratio!");
        num = 1;   /* FIXME: what values to use here? */
        den = 1;
      }

      GST_INFO ("calculated scaling ratio %d/%d for video %dx%d", num, den,
          gvc->priv->video_width, gvc->priv->video_height);
      
      /* now find a width x height that respects this display ratio.
       * prefer those that have one of w/h the same as the incoming video
       * using wd / hd = num / den */
    
      /* start with same height, because of interlaced video */
      /* check hd / den is an integer scale factor, and scale wd with the PAR */
      if (gvc->priv->video_height % den == 0) {
        GST_INFO ("keeping video height");
        gvc->priv->video_width_pixels =
            (guint) gst_util_uint64_scale (gvc->priv->video_height, num, den);
        gvc->priv->video_height_pixels = gvc->priv->video_height;
      } else if (gvc->priv->video_width % num == 0) {
        GST_INFO ("keeping video width");
        gvc->priv->video_width_pixels = gvc->priv->video_width;
        gvc->priv->video_height_pixels =
            (guint) gst_util_uint64_scale (gvc->priv->video_width, den, num);
      } else {
        GST_INFO ("approximating while keeping video height");
        gvc->priv->video_width_pixels =
            (guint) gst_util_uint64_scale (gvc->priv->video_height, num, den);
        gvc->priv->video_height_pixels = gvc->priv->video_height;
      }
      GST_INFO ("scaling to %dx%d", gvc->priv->video_width_pixels,
          gvc->priv->video_height_pixels);
      
      *width = gvc->priv->video_width_pixels;
      *height = gvc->priv->video_height_pixels;
      
      /* Free the PAR fraction */
      g_value_unset (disp_par);
      g_free (disp_par);
    }
    else {
      *width = 0;
      *height = 0;
    }
  }
}

static void
gvc_update_interface_implementations (GstVideoCapturer *gvc)
{

  GstXOverlay *old_xoverlay = gvc->priv->xoverlay;
  GstElement *video_sink = NULL;
  GstElement *element = NULL;



  g_object_get (gvc->priv->main_pipeline, "video-sink", &video_sink, NULL);
  g_assert (video_sink != NULL);


  /* We try to get an element supporting XOverlay interface */
  if (GST_IS_BIN (video_sink)) {
    GST_INFO ("Retrieving xoverlay from bin ...");
    element = gst_bin_get_by_interface (GST_BIN (video_sink),
                                        GST_TYPE_X_OVERLAY);
  } else {
    element = video_sink;
  }

  if (GST_IS_X_OVERLAY (element)) {
    GST_INFO ("Found xoverlay: %s", GST_OBJECT_NAME (element));
    gvc->priv->xoverlay = GST_X_OVERLAY (element);
  } else {
    GST_INFO ("No xoverlay found");
    gvc->priv->xoverlay = NULL;
  }
  if (old_xoverlay)
    gst_object_unref (GST_OBJECT (old_xoverlay));


  gst_object_unref (video_sink);
}

static void
gvc_element_msg_sync (GstBus *bus, GstMessage *msg, gpointer data)
{
  
  GstVideoCapturer *gvc = GST_VIDEO_CAPTURER (data);

  g_assert (msg->type == GST_MESSAGE_ELEMENT);

  if (msg->structure == NULL)
    return;

  /* This only gets sent if we haven't set an ID yet. This is our last
   * chance to set it before the video sink will create its own window */
  if (gst_structure_has_name (msg->structure, "prepare-xwindow-id")) {
    GdkWindow *window;

    GST_INFO ("Handling sync prepare-xwindow-id message");

    g_mutex_lock (gvc->priv->lock);
    gvc_update_interface_implementations (gvc);
    g_mutex_unlock (gvc->priv->lock);

    g_return_if_fail (gvc->priv->xoverlay != NULL);
    g_return_if_fail (gvc->priv->video_window != NULL);

    window = gst_video_widget_get_video_window (GST_VIDEO_WIDGET(gvc->priv->video_window));
    #ifdef WIN32
   	  gst_x_overlay_set_xwindow_id (gvc->priv->xoverlay, GDK_WINDOW_HWND(window));
	#else
	  gst_x_overlay_set_xwindow_id (gvc->priv->xoverlay, GDK_WINDOW_XID (window));
	#endif
	if (window != NULL)
		g_object_unref(window);

  }
}

static void
caps_set (GObject * obj,
    GParamSpec * pspec, GstVideoCapturer * gvc)
{
  GstPad *pad = GST_PAD (obj);
  GstStructure *s;
  GstCaps *caps;

  if (!(caps = gst_pad_get_negotiated_caps (pad)))
    return;

  /* Get video decoder caps */
  s = gst_caps_get_structure (caps, 0);
  if (s) {
    /* We need at least width/height and framerate */
    if (!(gst_structure_get_fraction (s, "framerate", &gvc->priv->video_fps_n, 
          &gvc->priv->video_fps_d) &&
          gst_structure_get_int (s, "width", &gvc->priv->video_width) &&
          gst_structure_get_int (s, "height", &gvc->priv->video_height)))
      return;
    
    /* Get the movie PAR if available */
    gvc->priv->movie_par = gst_structure_get_value (s, "pixel-aspect-ratio");
    
    
  }

  gst_caps_unref (caps);
}

static void
parse_stream_info (GstVideoCapturer *gvc)
{
  GList *audio_streams, *video_streams, *l;
  GstPad *videopad = NULL;

  audio_streams = get_stream_info_objects_for_type (gvc, "audio");
  video_streams = get_stream_info_objects_for_type (gvc, "video");

  gvc->priv->media_has_video = FALSE;
  if (video_streams) {
    gvc->priv->media_has_video = TRUE;
    for (l = video_streams; videopad == NULL && l != NULL; l = l->next) {
      g_object_get (l->data, "object", &videopad, NULL);
		//Aquí habría que volver a mostrar la ventana
    }
  }

  gvc->priv->media_has_audio = FALSE;
  if (audio_streams) {
    gvc->priv->media_has_audio = TRUE;
    if (!gvc->priv->media_has_video && gvc->priv->video_window) {
      //Aquí habría que ocultar la ventana      
    }
  }

  if (videopad) {
    GstCaps *caps;

    if ((caps = gst_pad_get_negotiated_caps (videopad))) {
      caps_set (G_OBJECT (videopad), NULL, gvc);
      gst_caps_unref (caps);
    }
    g_signal_connect (videopad, "notify::caps",
        G_CALLBACK (caps_set), gvc);
    gst_object_unref (videopad);
  } 

  g_list_foreach (audio_streams, (GFunc) g_object_unref, NULL);
  g_list_free (audio_streams);
  g_list_foreach (video_streams, (GFunc) g_object_unref, NULL);
  g_list_free (video_streams);
}

static void
decodebin_stream_info_notify_cb (GObject * obj, GParamSpec * pspec, gpointer data)
{
  GstVideoCapturer *gvc = GST_VIDEO_CAPTURER (data);
  GstMessage *msg;

  /* we're being called from the streaming thread, so don't do anything here */
  GST_LOG ("stream info changed");
  msg = gst_message_new_application (GST_OBJECT (gvc->priv->main_pipeline),
      gst_structure_new ("notify-streaminfo", NULL));
  gst_element_post_message (gvc->priv->main_pipeline, msg);
}

static GList *
get_stream_info_objects_for_type (GstVideoCapturer * gvc, const gchar * typestr)
{
  GValueArray *info_arr = NULL;
  GList *ret = NULL;
  guint i;

  if (gvc->priv->decode_bin == NULL )
    return NULL;

  g_object_get (gvc->priv->decode_bin, "stream-info-value-array", &info_arr, NULL);
  if (info_arr == NULL)
    return NULL;

  for (i = 0; i < info_arr->n_values; ++i) {
    GObject *info_obj;
    GValue *val;

    val = g_value_array_get_nth (info_arr, i);
    info_obj = g_value_get_object (val);
    if (info_obj) {
      GParamSpec *pspec;
      GEnumValue *val;
      gint type = -1;

      g_object_get (info_obj, "type", &type, NULL);
      pspec = g_object_class_find_property (G_OBJECT_GET_CLASS (info_obj), "type");
      val = g_enum_get_value (G_PARAM_SPEC_ENUM (pspec)->enum_class, type);
      if (val) {
        if (g_ascii_strcasecmp (val->value_nick, typestr) == 0 ||
            g_ascii_strcasecmp (val->value_name, typestr) == 0) {
          ret = g_list_prepend (ret, g_object_ref (info_obj));
        }
      }
    }
  }
  g_value_array_free (info_arr);

  return g_list_reverse (ret);
}

