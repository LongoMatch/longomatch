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

#include <string.h>
#include <stdio.h>

#include <gst/interfaces/xoverlay.h>
#include <gst/gst.h>
#include <gst/video/video.h>

#include "gst-video-capturer.h"
#include "gstvideowidget.h"


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
  	const GValue                 *movie_par; /* Movie pixel aspect ratio */
  	gint                         video_width_pixels; /* Scaled movie width */
  	gint                         video_height_pixels; /* Scaled movie height */
  	gint                         video_fps_n;
  	gint                         video_fps_d;
  	
	gboolean logo_mode;
	GdkPixbuf *logo_pixbuf;

	GstElement *video_bin;
	GstElement *audio_bin;
	GstElement *atee_bin;
	GstElement *vencode_bin;
	GstElement *aencode_bin;
	GstElement *decode_bin;
	GstElement *main_pipeline;
	GstElement *encode_pipeline;
	
	GstElement *file_sink;
	GstElement *source;
	GstElement *video_encoder;
	GstElement *audio_encoder;
	GstElement *videotee;
	
	
	gboolean media_has_video;
	gboolean media_has_audio;
	
	GstXOverlay                 *xoverlay; /* protect with lock */
    GMutex                      *lock;
	
	GvcVideoEncoderType *video_encoder_type;
	GvcAudioEncoderType	*audio_encoder_type;
	
	GtkWidget *video_window;

	gboolean link_encoder;
	
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
static void gvc_update_interface_implementations (GstVideoCapturer *gvc);
static int gvc_parse_video_stream_info (GstPad *pad, GstVideoCapturer * gvc);
static void gvc_link_encoder (GstVideoCapturer *gvc);
    
G_DEFINE_TYPE (GstVideoCapturer, gst_video_capturer, GTK_TYPE_HBOX);




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

  gtk_widget_destroy(gvc->priv->video_window);
  gtk_widget_unref(gvc->priv->video_window);
  gvc->priv->video_window = NULL;

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

static void gst_video_capturer_set_display_height (GstVideoCapturer *gvc,gint height)
{
	gvc->priv->display_height= height;	
}

static void gst_video_capturer_set_display_width (GstVideoCapturer *gvc,gint width)
{
	gvc->priv->display_width= width;
}

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
    	GST_INFO ("Changed video bitrate to :\n%s",bitrate);
   }
}

static void gst_video_capturer_set_audio_bit_rate (GstVideoCapturer *gvc,gint bitrate)
{
	GstState cur_state;

	gvc->priv->audio_bitrate= bitrate;
	gst_element_get_state (gvc->priv->vencode_bin, &cur_state, NULL, 0);
    if (cur_state <= GST_STATE_READY) {
	    g_object_set (gvc->priv->audio_encoder,"bitrate",bitrate,NULL);
    	GST_INFO ("Changed audio bitrate to :\n%s",bitrate);
   }
}

static void gst_video_capturer_set_output_file(GstVideoCapturer *gvc,const gchar *file)
{
	GstState cur_state;
	
	gvc->priv->output_file = g_strdup(file);
	gst_element_get_state (gvc->priv->vencode_bin, &cur_state, NULL, 0);
    if (cur_state == GST_STATE_NULL) {
	    g_object_set (gvc->priv->file_sink,"location",file,NULL);
    	GST_INFO ("Changed output file to :\n%s",file);
		
   }
}

static void gst_video_capturer_set_input_file(GstVideoCapturer *gvc,const gchar *file)
{
	GstState cur_state;
	
	gvc->priv->input_file = g_strdup(file);
	gst_element_get_state (gvc->priv->vencode_bin, &cur_state, NULL, 0);
    if (cur_state == GST_STATE_NULL) {	      
	    g_object_set (gvc->priv->source,"location",file,NULL);
	    gst_element_set_state(gvc->priv->main_pipeline,GST_STATE_PAUSED);
    	GST_INFO ("Changed source file to :\n%s",file);
   }
	
    
  
}
     
static void
gst_video_capturer_set_property (GObject * object, guint property_id,
                                 const GValue * value, GParamSpec * pspec)
{
	GstVideoCapturer *gvc;

	gvc = GST_VIDEO_CAPTURER (object);  
  
  switch (property_id) {
    case PROP_DISPLAY_HEIGHT:
      gst_video_capturer_set_display_height (gvc,
      g_value_get_uint (value));
      break;
    case PROP_DISPLAY_WIDTH:
      gst_video_capturer_set_display_width (gvc,
      g_value_get_uint (value));
      break;
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
      gst_video_capturer_set_input_file (gvc,
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
	//GtkHBoxClass* parent_class = GTK_WIDGET_CLASS (klass);


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

static gboolean
gvc_expose_event (GtkWidget *widget, GdkEventExpose *event, gpointer user_data)
{
	GstVideoCapturer *gvc;
	
	g_return_val_if_fail (widget != NULL, FALSE);
  	g_return_val_if_fail (GST_IS_VIDEO_WIDGET (widget), FALSE);
  	g_return_val_if_fail (event != NULL, FALSE);
  	
	gvc = GST_VIDEO_CAPTURER (user_data);
	
	 //Delegate the expose to the widget
	gst_video_widget_force_expose(widget,event);
   if (gvc->priv->xoverlay != NULL && !gvc->priv->logo_mode){
	   g_print("haciendo overlay");
      gst_x_overlay_expose (gvc->priv->xoverlay);   
     }
   return TRUE;
}

static void gvc_window_construct(int width, int weight,  GstVideoCapturer *gvc){
	
	//Create the Video Widget
	gvc->priv->video_window = gst_video_widget_new();
	gtk_container_add(GTK_CONTAINER(gvc),gvc->priv->video_window);
	gst_video_widget_set_minimum_size (GST_VIDEO_WIDGET (gvc->priv->video_window),
            120, 80);
      gst_video_widget_set_source_size(GST_VIDEO_WIDGET (gvc->priv->video_window),
            720, 576);    
	gtk_widget_show(gvc->priv->video_window);
	g_signal_connect (G_OBJECT (gvc->priv->video_window), "expose_event",
		    G_CALLBACK (gvc_expose_event), gvc);
}

GQuark
gst_video_capturer_error_quark (void)
{
  static GQuark q; /* 0 */

  if (G_UNLIKELY (q == 0)) {
    q = g_quark_from_static_string ("gvc-error-quark");
  }
  return q;
}

GstVideoCapturer *
gst_video_capturer_new (GvcUseType use_type,GError ** err )
{
	
	//CRear error si es del tipo transcode y no hay archivo de fuente o no hay archivo de salida
	
	GstPad *audiopad = NULL;
	GstPad *videopad = NULL;
	GstPad *videoteesrc1pad=NULL;
	GstPad *encodepad = NULL;	
	GstPad *pad1 = NULL;
	GstPad *pad2 = NULL;

	GstElement *conv = NULL;	
	GstElement *audiosink = NULL;		
	GstElement *videosink = NULL;
	GstElement *ffmpegcolorspace = NULL;
	GstElement *queue = NULL;
	GstElement *muxer = NULL;
	GstElement *deinterlacer = NULL;
	GstElement *videorate = NULL;
	GstElement *queue2 = NULL;


	GstCaps *filter = NULL;

	GstVideoCapturer *gvc = NULL;	
	
	
	gvc = g_object_new(GST_TYPE_VIDEO_CAPTURER, NULL);
	
	gvc->priv->input_file = "/dev/null";
	gvc->priv->output_file = "/dev/null";
	
	/*Handled by Properties?*/
	gvc->priv->display_height= 576;
	gvc->priv->display_width= 720;
	gvc->priv->encode_height= 576;
	gvc->priv->encode_width= 720;
	gvc->priv->audio_bitrate= 128;
	gvc->priv->video_bitrate= 5000000;
	
	gvc->priv->use_type = use_type;
	
	gvc_window_construct(gvc->priv->display_width,gvc->priv->display_height,gvc);
	
	gvc->priv->main_pipeline = gst_pipeline_new ("main_pipeline");
	
	if (!gvc->priv->main_pipeline ) {	 
    	g_set_error (err, GVC_ERROR, GVC_ERROR_PLUGIN_LOAD,
                	("Failed to create a GStreamer Bin. "
                    "Please check your GStreamer installation."));
    	g_object_ref_sink (gvc);
    	g_object_unref (gvc);
    	return NULL;
  	}
  	
  	/* Setup*/
  	

  	if (use_type == GVC_USE_TYPE_DEVICE_CAPTURE){
#ifdef WIN32
		gvc->priv->source = gst_element_factory_make ("ksvideosrc", "source");  
		g_print("Initializing ksvideosrc");
  		
#else
		gvc->priv->source = gst_element_factory_make ("dv1394src", "source");  
		g_print("Initializing dv1394src");
		g_object_set (gvc->priv->source,"use-avc", FALSE,NULL);
		
#endif

  		
 	}
 	else if (use_type == GVC_USE_TYPE_VIDEO_TRANSCODE){
	 	gvc->priv->source = gst_element_factory_make ("filesrc","source");

 	}
 	else if (use_type == GVC_USE_TYPE_TEST){
	 	gvc->priv->source = gst_element_factory_make ("fakesource","source");
 	}
  	
	gvc->priv->decode_bin = gst_element_factory_make ("decodebin", "decoder");  	
	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));    	
  	 	
  	
  	
  	g_print("Initializing tee");
  	gvc->priv->videotee = gst_element_factory_make("tee","videotee");

	g_print("Initializing tee");
  	
  	
  	g_print("Initializing audiobin");
  	 /* create audio output */  	 
  	gvc->priv->audio_bin = gst_bin_new ("audiobin");
  	conv = gst_element_factory_make ("audioconvert", "aconv");
  	audiopad = gst_element_get_pad (conv, "sink");
  	audiosink = gst_element_factory_make ("autoaudiosink", "sink");
  	gst_bin_add_many (GST_BIN(gvc->priv->audio_bin), conv, audiosink, NULL);
  	gst_element_link (conv, audiosink);
  	gst_element_add_pad (GST_ELEMENT(gvc->priv->audio_bin),
    gst_ghost_pad_new ("sink", audiopad));
  	
  	
  	 	 

	g_print("Initializing videobin");
	/*Create video output*/
	gvc->priv->video_bin= gst_bin_new ("videobin");
	queue2 = gst_element_factory_make ("queue", "queue");
	videopad = gst_element_get_pad (queue2, "sink");   	ffmpegcolorspace = gst_element_factory_make ("ffmpegcolorspace", "ffmpegcolorspace");
   	videosink = gst_element_factory_make ("autovideosink", "videosink");
   	//g_object_set (videosink,"sync", FALSE,NULL);  
  	gst_bin_add_many (GST_BIN(gvc->priv->video_bin),queue2,ffmpegcolorspace, videosink, NULL); 
  	gst_element_link_many (queue2,ffmpegcolorspace,videosink,NULL);
  	gst_element_add_pad (gvc->priv->video_bin,
    gst_ghost_pad_new ("sink", GST_PAD(videopad)));   	


	
	if (videosink) {
    GstStateChangeReturn ret;

    /* need to set bus explicitly as it's not in a bin yet and
     * poll_for_state_change() needs one to catch error messages */
    gst_element_set_bus (videosink, gvc->priv->bus);
    /* state change NULL => READY should always be synchronous */
    ret = gst_element_set_state (videosink, GST_STATE_READY);
    if (ret == GST_STATE_CHANGE_FAILURE) {
      /* Drop this video sink */
      gst_element_set_state (videosink, GST_STATE_NULL);
      gst_object_unref (videosink);
      /* Try again with autovideosink */
      videosink = gst_element_factory_make ("autovideosink", "videosink");
      gst_element_set_bus (videosink, gvc->priv->bus);
      ret = gst_element_set_state (videosink, GST_STATE_READY);
      if (ret == GST_STATE_CHANGE_FAILURE) {
        
          g_set_error (err, GVC_ERROR, GVC_ERROR_VIDEO_PLUGIN,
               ("Failed to open video output. It may not be available. "
                 "Please select another video output in the Multimedia "
                 "Systems Selector."));
       
        goto sink_error;
      }
    }
  } else {
    g_set_error (err, GVC_ERROR, GVC_ERROR_VIDEO_PLUGIN,
                 ("Could not find the video output. "
                   "You may need to install additional GStreamer plugins, "
                   "or select another video output in the Multimedia Systems "
                   "Selector."));
    goto sink_error;
  }
  	

  g_print("Initializing encoder");
		gvc->priv->vencode_bin= gst_bin_new ("encodebin");
  		queue = gst_element_factory_make ("queue", "encodequeue");
  		encodepad = gst_element_get_pad (queue, "sink");  		 		
    	gvc->priv->video_encoder= gst_element_factory_make ("ffenc_mpeg4", "xvidencoder");    	
    	g_object_set (G_OBJECT(gvc->priv->video_encoder), "bitrate",gvc->priv->video_bitrate,NULL);
    	muxer = gst_element_factory_make("avimux","muxer");
  		gvc->priv->file_sink = gst_element_factory_make ("filesink", "filesink");
		
  		if (use_type == GVC_USE_TYPE_DEVICE_CAPTURE){
  			deinterlacer = gst_element_factory_make("ffdeinterlace","deinterlacer");
			videorate = gst_element_factory_make("videorate","videorate");
			filter = gst_caps_new_simple ("video/x-raw-yuv","framerate", GST_TYPE_FRACTION, 25, 1,NULL);
			gst_bin_add_many (GST_BIN(gvc->priv->vencode_bin),queue,deinterlacer,videorate,gvc->priv->video_encoder,muxer,gvc->priv->file_sink, NULL);
			gst_element_link_many (queue,deinterlacer,videorate,NULL);
			gst_element_link_filtered(videorate,gvc->priv->video_encoder,filter);
			gst_element_link_many (gvc->priv->video_encoder,muxer,gvc->priv->file_sink, NULL);
 		}
 		else{
 			gst_bin_add_many (GST_BIN(gvc->priv->vencode_bin),queue,gvc->priv->video_encoder,muxer,gvc->priv->file_sink, NULL);  
 			gst_element_link_many (queue,gvc->priv->video_encoder,muxer, gvc->priv->file_sink,NULL);
		}
  		gst_element_add_pad (gvc->priv->vencode_bin,
      	gst_ghost_pad_new ("sink", GST_PAD(encodepad))); 

  	
  	gst_bin_add_many(GST_BIN (gvc->priv->main_pipeline),gvc->priv->source,gvc->priv->decode_bin,gvc->priv->video_bin,gvc->priv->videotee,NULL );  	
  	
	gst_element_link(gvc->priv->source,gvc->priv->decode_bin);

	videoteesrc1pad = gst_element_get_request_pad  (gvc->priv->videotee, "src%d");
    pad2 = gst_element_get_pad  (gvc->priv->video_bin, "sink");
    gst_pad_link (videoteesrc1pad,pad2);

	pad1 = gst_element_get_pad(gvc->priv->decode_bin,"src");  	
  	pad2 = gst_element_get_pad (gvc->priv->videotee, "sink");
	gst_pad_link (pad1,pad2 ); 
   

	/*Connect bus signals*/
	g_print("Initializing signals bus");
	
	gvc->priv->bus = gst_element_get_bus (GST_ELEMENT(gvc->priv->main_pipeline));    	
  	g_signal_connect (gvc->priv->decode_bin, "new-decoded-pad",G_CALLBACK(new_decoded_pad_cb),gvc); 
	gst_bus_add_signal_watch (gvc->priv->bus);	
  	gvc->priv->sig_bus_async = 
      g_signal_connect (gvc->priv->bus, "message", 
                        G_CALLBACK (gvc_bus_message_cb),
                        gvc);
     
    gvc_update_interface_implementations (gvc);
  

  /* we want to catch "prepare-xwindow-id" element messages synchronously */
  gst_bus_set_sync_handler (gvc->priv->bus, gst_bus_sync_signal_handler, gvc);

  gvc->priv->sig_bus_sync = 
      g_signal_connect (gvc->priv->bus, "sync-message::element",
                        G_CALLBACK (gvc_element_msg_sync), gvc);
    
	if (use_type == GVC_USE_TYPE_DEVICE_CAPTURE || use_type == GVC_USE_TYPE_TEST)
		gst_element_set_state (gvc->priv->main_pipeline, GST_STATE_PLAYING);
	else 
		gst_element_set_state (gvc->priv->main_pipeline, GST_STATE_PLAYING);
	
	return gvc;
	
	sink_error:
  {
    if (videosink) {
      gst_element_set_state (videosink, GST_STATE_NULL);
      gst_object_unref (videosink);
    }
    if (audiosink) {
      gst_element_set_state (audiosink, GST_STATE_NULL);
      gst_object_unref (audiosink);
    }
	
    g_object_ref (gvc);
    g_object_ref_sink (G_OBJECT (gvc));
    g_object_unref (gvc);

    return NULL;
  }

}

static void gvc_link_encoder (GstVideoCapturer *gvc)
{
	GstPad *pad1;
	GstPad *pad2;

	g_return_if_fail(GST_IS_VIDEO_CAPTURER(gvc));
	g_return_if_fail(gvc->priv->videotee != NULL);

	pad1 = gst_element_get_pad(gvc->priv->vencode_bin,"sink");  	
  	pad2 = gst_element_get_request_pad  (gvc->priv->videotee, "src%d");	

	
	if (GST_PAD_IS_LINKED (pad1)){
		gst_object_unref(pad1);
		gst_object_unref(pad2);
		return;
	}
	gst_element_set_state (gvc->priv->vencode_bin,GST_STATE_PAUSED);

	gst_bin_add(GST_BIN(gvc->priv->main_pipeline),gvc->priv->vencode_bin);
  	gst_pad_link (pad2,pad1 ); 
	
  	gst_object_unref(pad1);
	gst_object_unref(pad2);	

}
	
void gst_video_capturer_start_rec (GstVideoCapturer *gvc)
{
	g_return_if_fail(GST_IS_VIDEO_CAPTURER(gvc));

	gvc->priv->link_encoder = TRUE;

	gst_element_set_state (gvc->priv->main_pipeline,GST_STATE_NULL);
	gst_element_get_state (gvc->priv->main_pipeline,NULL,NULL,-1);
	gst_element_set_state (gvc->priv->main_pipeline,GST_STATE_PLAYING);
	

}


void gst_video_capturer_rec(GstVideoCapturer *gvc)
{
	GstPad *pad1;
	GstPad *pad2;
	
	g_return_if_fail(GST_IS_VIDEO_CAPTURER(gvc));

	pad1 = gst_element_get_pad(gvc->priv->vencode_bin,"sink");  	
  	pad2 = gst_element_get_pad (gvc->priv->videotee, "src1");
	
	if (GST_PAD_IS_LINKED (pad1)){
		gst_object_unref(pad1);
		gst_object_unref(pad2);
		return;
	}
	
	gst_pad_link (pad2,pad1 );
	gst_element_set_locked_state(gvc->priv->vencode_bin, FALSE);
	gst_element_set_state(gvc->priv->vencode_bin,GST_STATE_PLAYING);
	  	
  	gst_object_unref(pad1);
	gst_object_unref(pad2);

	
}

void gst_video_capturer_pause(GstVideoCapturer *gvc)
{
	GstPad *pad1;
	GstPad *pad2;
	
	g_return_if_fail(GST_IS_VIDEO_CAPTURER(gvc));	
	
	pad1 = gst_element_get_pad(gvc->priv->vencode_bin,"sink");  	
  	pad2 = gst_element_get_pad (gvc->priv->videotee, "src1");
	
	if (!GST_PAD_IS_LINKED (pad1)){
		gst_object_unref(pad1);
		gst_object_unref(pad2);
		return;
	}	
	gst_pad_unlink (pad2,pad1 );
	gst_element_set_locked_state(gvc->priv->vencode_bin, TRUE);
	gst_element_set_state(gvc->priv->vencode_bin,GST_STATE_PAUSED);
	  	
  	gst_object_unref(pad1);
	gst_object_unref(pad2);	


		
	
}

void gst_video_capturer_stop(GstVideoCapturer *gvc)
{
	GstPad *pad1;


	GstEvent *eos = gst_event_new_eos();


	pad1 = gst_element_get_pad(gvc->priv->vencode_bin,"sink"); 
	
	gst_element_set_state(gvc->priv->main_pipeline, GST_STATE_READY);
	gst_element_get_state(gvc->priv->main_pipeline, NULL, NULL, -1);
	// Sending an EOS event to finalize the video file
	//gst_pad_push_event(pad1, eos);
    gst_element_send_event (gvc->priv->main_pipeline, eos); 
	
	g_object_set(gvc,"input_file","",NULL);
	
	gst_object_unref(pad1);


	
}

static void new_decoded_pad_cb (GstElement* object,
                                           GstPad* pad,
                                           gboolean arg1,
                                           gpointer user_data)
{
	GstCaps *caps=NULL;
	GstStructure *str =NULL;
	GstPad *audiopad=NULL;
	GstPad *videoteepad=NULL;
	GstVideoCapturer *gvc=NULL;
	

  	g_return_if_fail (GST_IS_VIDEO_CAPTURER(user_data));
 	gvc = GST_VIDEO_CAPTURER (user_data);	

	/* check media type */
	caps = gst_pad_get_caps (pad);
	str = gst_caps_get_structure (caps, 0);
  
	g_print("Processing decoder caps\n"); 
  
	if (g_strrstr (gst_structure_get_name (str), "audio")) {	  
		/* only link once */
		audiopad = gst_element_get_pad (gvc->priv->audio_bin, "sink");
		if (GST_PAD_IS_LINKED (audiopad)) {
    		g_object_unref (audiopad);
    		gst_caps_unref (caps);
    		g_object_unref (gvc);
    		return;
  		}
  	
  	g_print("Linking Audio Pad");
  	/* link 'n play*/
  	gst_bin_add (GST_BIN (gvc->priv->main_pipeline), gvc->priv->audio_bin);
  	gst_pad_link (pad, audiopad);
  	g_object_unref (audiopad); 
  }
  
  else if (g_strrstr (gst_structure_get_name (str), "video")){	      
	
  	gvc->priv->media_has_video = TRUE;  
	videoteepad = gst_element_get_pad (gvc->priv->videotee, "sink");
	
	/* only link once */
	if (GST_PAD_IS_LINKED (videoteepad)) {
    	g_object_unref (videoteepad);
    	gst_caps_unref (caps);    	
    	return;
  	}  	

  	/* link 'n play*/	
    gst_pad_link (pad,videoteepad); 
	//while (!GST_PAD_IS_LINKED(pad));
    /* Getting stream info*/
	gvc_parse_video_stream_info(pad,gvc);
	g_object_unref (videoteepad);	

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
	  if (new_state == GST_STATE_PLAYING && gvc->priv->link_encoder){
		  gvc_link_encoder(gvc);
		  gvc->priv->link_encoder = FALSE;
	  }
      g_print ("%s changed state from %s to %s", src_name,
          gst_element_state_get_name (old_state),
          gst_element_state_get_name (new_state));
      g_free (src_name);     
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


static void
gvc_update_interface_implementations (GstVideoCapturer *gvc)
{

  GstXOverlay *old_xoverlay = gvc->priv->xoverlay;
  GstElement *video_sink = NULL;
  GstElement *element = NULL;

  video_sink = gst_bin_get_by_name (GST_BIN(gvc->priv->video_bin),"videosink");
  g_assert (video_sink != NULL);


  /* We try to get an element supporting XOverlay interface */
  if (GST_IS_BIN (video_sink)) {
    g_print ("Retrieving xoverlay from bin ...");
    element = gst_bin_get_by_interface (GST_BIN (video_sink),
                                        GST_TYPE_X_OVERLAY);
  } else {
    element = video_sink;
  }

  if (GST_IS_X_OVERLAY (element)) {
    g_print ("Found xoverlay: %s", GST_OBJECT_NAME (element));
    gvc->priv->xoverlay = GST_X_OVERLAY (element);
    
  } else {
    g_print ("No xoverlay found");
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
	GstPad *pad =gst_element_get_pad(gvc->priv->video_bin,"sink");
    gvc_parse_video_stream_info(pad,gvc);
    
    g_print ("Handling sync prepare-xwindow-id message");
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
	
	
	g_print("\nSetting Source size: %d",gvc->priv->video_width);
    //gst_video_widget_set_source_size (GST_VIDEO_WIDGET (gvc->priv->video_window),gvc->priv->video_width,gvc->priv->video_height);


	
	
     

  }
}


static int
gvc_parse_video_stream_info (GstPad *pad, GstVideoCapturer * gvc)
{
  GstStructure *s;
  GstCaps *caps;
 
  g_object_get(G_OBJECT(pad),"caps",&caps,NULL);
  
  g_print("prcessing caps");
  if (!(caps))
    return -1;
  g_print("nocaps");
  /* Get video decoder caps */
  s = gst_caps_get_structure (caps, 0);
  if (s) {
    /* We need at least width/height and framerate */
    if (!(gst_structure_get_fraction (s, "framerate", &gvc->priv->video_fps_n, 
          &gvc->priv->video_fps_d) &&
          gst_structure_get_int (s, "width", &gvc->priv->video_width) &&
          gst_structure_get_int (s, "height", &gvc->priv->video_height)))
      return -1;
    
    /* Get the movie PAR if available */
    gvc->priv->movie_par = gst_structure_get_value (s, "pixel-aspect-ratio"); 
    
    g_print("Height:%d\nWidth:%d\nFramerate%d",gvc->priv->video_width,gvc->priv->video_height,gvc->priv->video_fps_n);
  }
 
  gst_caps_unref (caps);
  return 1;
}



