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
* Gstreamer DV is distributed in the hope that it will be useful,
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

#include "gst-camera-capturer.h"
#include "gstvideowidget.h"


/*Default video source*/
#ifdef WIN32
#define VIDEOSRC "ksvideosrc"
#else
#define VIDEOSRC "v4l2src"
#endif

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
	PROP_ENCODE_HEIGHT,
	PROP_ENCODE_WIDTH,
	PROP_VIDEO_BITRATE,
	PROP_AUDIO_BITRATE,
	PROP_OUTPUT_FILE,
	PROP_WITH_AUDIO
};

struct GstCameraCapturerPrivate
{

	/*Encoding properties*/
	gchar				*output_file;	
	guint				encode_height;
	guint				encode_width;
	guint				audio_bitrate;
	guint				video_bitrate; 
	GccVideoEncoderType video_encoder_type;
	GccAudioEncoderType	audio_encoder_type;

	/*Video input info*/
	gint				video_width; /* Movie width */
	gint				video_height; /* Movie height */
	const GValue		*movie_par; /* Movie pixel aspect ratio */
	gint				video_width_pixels; /* Scaled movie width */
	gint				video_height_pixels; /* Scaled movie height */
	gint				video_fps_n;
	gint				video_fps_d;
	gboolean			media_has_video;
	gboolean			media_has_audio;


	/*GStreamer elements*/
	GstElement			*main_pipeline;
	GstElement			*camerabin;
	GstElement			*videosrc;
	GstElement			*audiosrc;
	GstElement			*videoenc;
	GstElement			*audioenc;
	GstElement			*videomux;

	/*Overlay*/
	GstXOverlay			*xoverlay; /* protect with lock */
	GMutex				*lock;

	/*Videobox*/
	GtkWidget			*video_window;
	gboolean			logo_mode;
	GdkPixbuf			*logo_pixbuf;

	/*GStreamer bus*/
	GstBus				*bus;
	gulong				sig_bus_async;
	gulong				sig_bus_sync;
};

static int	gcc_signals[LAST_SIGNAL] = { 0 };
static void gcc_error_msg (GstCameraCapturer *gcc, GstMessage *msg);
static void gcc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data);
static void gst_camera_capturer_get_property (GObject * object, guint property_id, GValue * value, GParamSpec * pspec);
static void gst_camera_capturer_set_property (GObject * object, guint property_id,const GValue * value, GParamSpec * pspec);
static void gcc_element_msg_sync (GstBus *bus, GstMessage *msg, gpointer data);
static void gcc_update_interface_implementations (GstCameraCapturer *gcc);
static int	gcc_parse_video_stream_info (GstCaps *caps, GstCameraCapturer * gcc);

G_DEFINE_TYPE (GstCameraCapturer, gst_camera_capturer, GTK_TYPE_HBOX);




static void
gst_camera_capturer_init (GstCameraCapturer *object)
{
	GstCameraCapturerPrivate *priv;
	object->priv = priv = G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_CAMERA_CAPTURER, GstCameraCapturerPrivate);
	priv->lock = g_mutex_new ();
}

static void
gst_camera_capturer_finalize (GObject *object)
{
	GstCameraCapturer *gcc = (GstCameraCapturer *) object;

	gtk_widget_destroy(gcc->priv->video_window);
	gtk_widget_unref(gcc->priv->video_window);
	gcc->priv->video_window = NULL;

	if (gcc->priv->bus) {
		/* make bus drop all messages to make sure none of our callbacks is ever
		* called again (main loop might be run again to display error dialog) */
		gst_bus_set_flushing (gcc->priv->bus, TRUE);

		if (gcc->priv->sig_bus_async)
			g_signal_handler_disconnect (gcc->priv->bus,gcc->priv->sig_bus_async);
		gst_object_unref (gcc->priv->bus);
		gcc->priv->bus = NULL;
	}

	g_free (gcc->priv->output_file);
	gcc->priv->output_file = NULL;


	if (gcc->priv->main_pipeline != NULL && GST_IS_ELEMENT (gcc->priv->main_pipeline )) {
		gst_element_set_state (gcc->priv->main_pipeline , GST_STATE_NULL);
		gst_object_unref (gcc->priv->main_pipeline);
		gcc->priv->main_pipeline = NULL;
	}  

	if (gcc->priv->camerabin!= NULL && GST_IS_ELEMENT (gcc->priv->camerabin )) {
		gst_object_unref (gcc->priv->camerabin);
		gcc->priv->camerabin = NULL;
	}

	if (gcc->priv->videosrc!= NULL && GST_IS_ELEMENT (gcc->priv->videosrc )) {
		gst_object_unref (gcc->priv->videosrc);
		gcc->priv->videosrc = NULL;
	}

	if (gcc->priv->audiosrc != NULL && GST_IS_ELEMENT (gcc->priv->audiosrc )) {
		gst_object_unref (gcc->priv->audiosrc );
		gcc->priv->audiosrc  = NULL;
	}

	if (gcc->priv->videoenc!= NULL && GST_IS_ELEMENT (gcc->priv->videoenc )) {
		gst_object_unref (gcc->priv->videoenc);
		gcc->priv->videoenc = NULL;
	}

	if (gcc->priv->audioenc!= NULL && GST_IS_ELEMENT (gcc->priv->audioenc )) {
		gst_object_unref (gcc->priv->audioenc );
		gcc->priv->audioenc  = NULL;
	}

	g_mutex_free (gcc->priv->lock);

	G_OBJECT_CLASS (gst_camera_capturer_parent_class)->finalize (object);
}



static void gst_camera_capturer_set_encode_width (GstCameraCapturer *gcc,gint width)
{	

}

static void gst_camera_capturer_set_encode_height (GstCameraCapturer *gcc,gint height)
{

}

static void gst_camera_capturer_set_video_bit_rate (GstCameraCapturer *gcc,gint bitrate)
{
	gcc->priv->video_bitrate= bitrate;
	g_object_set (gcc->priv->videoenc,"bitrate",gcc->priv->video_bitrate,NULL);
	GST_INFO ("Changed video bitrate to :\n%d",gcc->priv->video_bitrate);

}

static void gst_camera_capturer_set_audio_bit_rate (GstCameraCapturer *gcc,gint bitrate)
{

	gcc->priv->audio_bitrate= bitrate;
	g_object_set (gcc->priv->audioenc,"bitrate",bitrate,NULL);
	GST_INFO ("Changed audio bitrate to :\n%d",gcc->priv->audio_bitrate);

}

static void gst_camera_capturer_set_output_file(GstCameraCapturer *gcc,const gchar *file)
{
	gcc->priv->output_file = g_strdup(file);
	g_object_set (gcc->priv->camerabin,"filename",file,NULL);
	GST_INFO ("Changed output filename to :\n%s",file);		

}



static void
gst_camera_capturer_set_property (GObject * object, guint property_id,
								  const GValue * value, GParamSpec * pspec)
{
	GstCameraCapturer *gcc;

	gcc = GST_CAMERA_CAPTURER (object);  

	switch (property_id) {    
case PROP_ENCODE_HEIGHT:
	gst_camera_capturer_set_encode_height (gcc,
		g_value_get_uint (value));
	break;
case PROP_ENCODE_WIDTH:
	gst_camera_capturer_set_encode_width (gcc,
		g_value_get_uint (value));
	break;
case PROP_VIDEO_BITRATE:
	gst_camera_capturer_set_video_bit_rate (gcc,
		g_value_get_uint (value));
	break;
case PROP_AUDIO_BITRATE:
	gst_camera_capturer_set_audio_bit_rate (gcc,
		g_value_get_uint (value));
	break;
case PROP_OUTPUT_FILE:
	gst_camera_capturer_set_output_file(gcc,
		g_value_get_string (value));
	break;    
default:
	G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
	break;
	}
}

static void
gst_camera_capturer_get_property (GObject * object, guint property_id,
								  GValue * value, GParamSpec * pspec)
{
	GstCameraCapturer *gcc;

	gcc = GST_CAMERA_CAPTURER (object);

	switch (property_id) {
case PROP_ENCODE_HEIGHT:
	g_value_set_uint (value,gcc->priv->encode_height);
	break;
case PROP_ENCODE_WIDTH:
	g_value_set_uint (value,gcc->priv->encode_width);
	break;
case PROP_AUDIO_BITRATE:
	g_value_set_uint (value,gcc->priv->audio_bitrate);
	break;
case PROP_VIDEO_BITRATE:
	g_value_set_uint (value,gcc->priv->video_bitrate);
	break;   
case PROP_OUTPUT_FILE:
	g_value_set_string (value,gcc->priv->output_file);
	break;    
default:
	G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
	break;
	}
}

static void
gst_camera_capturer_class_init (GstCameraCapturerClass *klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS (klass);

	object_class->finalize = gst_camera_capturer_finalize;

	g_type_class_add_private (object_class, sizeof (GstCameraCapturerPrivate));

	/* GObject */
	object_class->set_property = gst_camera_capturer_set_property;
	object_class->get_property = gst_camera_capturer_get_property;
	object_class->finalize = gst_camera_capturer_finalize;

	/* Properties */
	g_object_class_install_property (object_class, PROP_ENCODE_HEIGHT,
		g_param_spec_uint ("encode_height", NULL,
		NULL,180 , 5600, 576,
		G_PARAM_READWRITE));
	g_object_class_install_property (object_class, PROP_ENCODE_WIDTH,
		g_param_spec_uint ("encode_width", NULL,
		NULL, 180,5600,720,
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


	/* Signals */
	gcc_signals[SIGNAL_ERROR] =
		g_signal_new ("error",
		G_TYPE_FROM_CLASS (object_class),
		G_SIGNAL_RUN_LAST,
		G_STRUCT_OFFSET (GstCameraCapturerClass, error),
		NULL, NULL,
		g_cclosure_marshal_VOID__STRING,
		G_TYPE_NONE, 1, G_TYPE_STRING);

	gcc_signals[SIGNAL_EOS] =
		g_signal_new ("eos",
		G_TYPE_FROM_CLASS (object_class),
		G_SIGNAL_RUN_LAST,
		G_STRUCT_OFFSET (GstCameraCapturerClass, eos),
		NULL, NULL, g_cclosure_marshal_VOID__VOID, G_TYPE_NONE, 0);

}

void
gst_camera_capturer_init_backend (int *argc, char ***argv)
{
	gst_init (argc, argv);
}

static gboolean
gcc_expose_event (GtkWidget *widget, GdkEventExpose *event, gpointer user_data)
{
	GstCameraCapturer *gcc;

	g_return_val_if_fail (widget != NULL, FALSE);
	g_return_val_if_fail (GST_IS_VIDEO_WIDGET (widget), FALSE);
	g_return_val_if_fail (event != NULL, FALSE);

	gcc = GST_CAMERA_CAPTURER (user_data);

	//Delegate the expose to the widget
	gst_video_widget_force_expose(widget,event);
	if (gcc->priv->xoverlay != NULL && !gcc->priv->logo_mode){
		gst_x_overlay_expose (gcc->priv->xoverlay);   
	}
	return TRUE;
}

static void gcc_window_construct(int width, int height,  GstCameraCapturer *gcc){

	//Create the Video Widget
	gcc->priv->video_window = gst_video_widget_new();
	gtk_container_add(GTK_CONTAINER(gcc),gcc->priv->video_window);
	gst_video_widget_set_minimum_size (GST_VIDEO_WIDGET (gcc->priv->video_window),
		120, 80);
	gst_video_widget_set_source_size(GST_VIDEO_WIDGET (gcc->priv->video_window),
		720, 576);    
	gtk_widget_show(gcc->priv->video_window);
	g_signal_connect (G_OBJECT (gcc->priv->video_window), "expose_event",
		G_CALLBACK (gcc_expose_event), gcc);
}

GQuark
gst_camera_capturer_error_quark (void)
{
	static GQuark q; /* 0 */

	if (G_UNLIKELY (q == 0)) {
		q = g_quark_from_static_string ("gcc-error-quark");
	}
	return q;
}

GstCameraCapturer *
gst_camera_capturer_new (gchar *filename, GError ** err )
{
	GstCameraCapturer *gcc = NULL;
	GstState state;

	gcc = g_object_new(GST_TYPE_CAMERA_CAPTURER, NULL);	

	/*Handled by Properties?*/
	gcc->priv->encode_height= 576;
	gcc->priv->encode_width= 720;
	gcc->priv->audio_bitrate= 128;
	gcc->priv->video_bitrate= 5000;

	gcc_window_construct(720,576,gcc);

	gcc->priv->main_pipeline = gst_pipeline_new ("main_pipeline");

	if (!gcc->priv->main_pipeline ) {	 
		g_set_error (err, GCC_ERROR, GCC_ERROR_PLUGIN_LOAD,
			("Failed to create a GStreamer Bin. "
			"Please check your GStreamer installation."));
		g_object_ref_sink (gcc);
		g_object_unref (gcc);
		return NULL;
	}

	/* Setup*/
	GST_INFO("Initializing camerabin");
	gcc->priv->camerabin = gst_element_factory_make ("camerabin","camerabin");
	gst_bin_add(GST_BIN(gcc->priv->main_pipeline),gcc->priv->camerabin);

	GST_INFO("Setting capture mode to \"video\"");
	g_object_set (gcc->priv->camerabin,"mode",1,NULL);

	GST_INFO("Setting video source ");
	gcc->priv->videosrc = gst_element_factory_make (VIDEOSRC, "videosource"); 
	g_object_set (gcc->priv->camerabin,"videosrc",gcc->priv->videosrc,NULL);
	g_object_set (gcc->priv->videosrc,"do-timestamp",TRUE,NULL);

	GST_INFO("Setting audio source ");
	gcc->priv->audiosrc = gst_element_factory_make ("dshowaudiosrc", "audiosource"); 
	g_object_set (gcc->priv->camerabin,"audiosrc",gcc->priv->audiosrc,NULL);

	/*gcc->priv->videoenc = gst_element_factory_make ("ffenc_mpeg4","videoenc");
	g_object_set (gcc->priv->camerabin,"videoenc",gcc->priv->videoenc,NULL);
	gcc->priv->audioenc = gst_element_factory_make ("faac","audioenc");
	g_object_set (gcc->priv->camerabin,"audioenc",gcc->priv->audioenc,NULL);
	gcc->priv->videomux = gst_element_factory_make ("avimux","videomux");
	g_object_set (gcc->priv->camerabin,"videomux",gcc->priv->videomux,NULL);*/



	GST_INFO("Setting capture mode to \"video\"");
	g_object_set (gcc->priv->camerabin,"mode",1,NULL);

	g_object_set (gcc->priv->camerabin,"mute",TRUE,NULL);

	/*Connect bus signals*/
	GST_INFO("Connecting bus signals");	
	gcc->priv->bus = gst_element_get_bus (GST_ELEMENT(gcc->priv->main_pipeline));    	
	gst_bus_add_signal_watch (gcc->priv->bus);	
	gcc->priv->sig_bus_async = 
		g_signal_connect (gcc->priv->bus, "message", 
		G_CALLBACK (gcc_bus_message_cb),
		gcc);     

	/* we want to catch "prepare-xwindow-id" element messages synchronously */
	gst_bus_set_sync_handler (gcc->priv->bus, gst_bus_sync_signal_handler, gcc);

	gcc->priv->sig_bus_sync = 
		g_signal_connect (gcc->priv->bus, "sync-message::element",
		G_CALLBACK (gcc_element_msg_sync), gcc);


	/*gst_element_set_state (gcc->priv->camerabin, GST_STATE_NULL);
	do
	{
	gst_element_get_state(gcc->priv->camerabin, &state, NULL, 
	GST_CLOCK_TIME_NONE);

	}
	while(state != GST_STATE_NULL);	*/



	return gcc;
}


void gst_camera_capturer_run(GstCameraCapturer *gcc)
{	
	gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_PLAYING);
}

void gst_camera_capturer_start (GstCameraCapturer *gcc)
{
	g_return_if_fail(GST_IS_CAMERA_CAPTURER(gcc));	
	g_signal_emit_by_name (G_OBJECT(gcc->priv->camerabin),"user-start",0,0);
}


void gst_camera_capturer_toggle_pause(GstCameraCapturer *gcc)
{
	g_return_if_fail(GST_IS_CAMERA_CAPTURER(gcc));
	g_signal_emit_by_name (G_OBJECT(gcc->priv->camerabin),"user-pause",0,0);
}

void gst_camera_capturer_stop(GstCameraCapturer *gcc)
{
	g_return_if_fail(GST_IS_CAMERA_CAPTURER(gcc));	
	g_signal_emit_by_name (G_OBJECT(gcc->priv->camerabin),"user-stop",0,0);
}


gboolean gst_camera_capturer_set_video_encoder(GstCameraCapturer *gcc,GccVideoEncoderType type)
{
	g_return_val_if_fail(GST_IS_CAMERA_CAPTURER(gcc),FALSE);
	switch (type){
case GCC_VIDEO_ENCODER_TYPE_MPEG4:
	gcc->priv->videoenc = gst_element_factory_make ("ffenc_mpeg4","videoenc");
	g_object_set (gcc->priv->camerabin,"videoenc",gcc->priv->videoenc,NULL);
	break;

case GCC_VIDEO_ENCODER_TYPE_XVID:
	gcc->priv->videoenc = gst_element_factory_make ("xvid_enc","videoenc");
	g_object_set (gcc->priv->camerabin,"videoenc",gcc->priv->videoenc,NULL);
	break;

case GCC_VIDEO_ENCODER_TYPE_THEORA:
	gcc->priv->videoenc = gst_element_factory_make ("theoraenc","videoenc");
	g_object_set (gcc->priv->camerabin,"videoenc",gcc->priv->videoenc,NULL);
	break;

case GCC_VIDEO_ENCODER_TYPE_H264:
	gcc->priv->videoenc = gst_element_factory_make ("x264enc","videoenc");
	g_object_set (gcc->priv->camerabin,"videoenc",gcc->priv->videoenc,NULL);
	break;

	}
	return TRUE;
}

gboolean  gst_camera_capturer_set_audio_encoder(GstCameraCapturer *gcc,GccAudioEncoderType type)
{
	g_return_val_if_fail(GST_IS_CAMERA_CAPTURER(gcc),FALSE);

	switch (type){
case GCC_AUDIO_ENCODER_MP3:
	gcc->priv->audioenc = gst_element_factory_make ("ffenc_libmp3lame","audioenc");
	g_object_set (gcc->priv->camerabin,"audioenc",gcc->priv->audioenc,NULL);
	break;

case GCC_AUDIO_ENCODER_AAC:
	gcc->priv->audioenc = gst_element_factory_make ("faac","audioenc");
	g_object_set (gcc->priv->camerabin,"audioenc",gcc->priv->audioenc,NULL);
	break;

case GCC_AUDIO_ENCODER_VORBIS:	
	gcc->priv->audioenc = gst_element_factory_make ("vorbisenc","audioenc");
	g_object_set (gcc->priv->camerabin,"audioenc",gcc->priv->audioenc,NULL);
	break;
	}

	return TRUE;

}

gboolean gst_camera_capturer_set_video_muxer(GstCameraCapturer *gcc,GccVideoMuxerType type)
{
	g_return_val_if_fail(GST_IS_CAMERA_CAPTURER(gcc),FALSE);

	switch (type){
case GCC_VIDEO_MUXER_OGG:
	gcc->priv->videomux = gst_element_factory_make ("oggmux","videomux");
	g_object_set (gcc->priv->camerabin,"videomux",gcc->priv->audioenc,NULL);
	break;
case GCC_VIDEO_MUXER_AVI:
	gcc->priv->videomux = gst_element_factory_make ("avimux","videomux");
	g_object_set (gcc->priv->camerabin,"videomux",gcc->priv->audioenc,NULL);
	break;
case GCC_VIDEO_MUXER_MATROSKA:	
	gcc->priv->videomux = gst_element_factory_make ("matroskamux","videomux");
	g_object_set (gcc->priv->camerabin,"videomux",gcc->priv->audioenc,NULL);
	break;
	}

	return TRUE;
}





static void gcc_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
	GstCameraCapturer *gcc = (GstCameraCapturer *) data;
	GstMessageType msg_type;

	g_return_if_fail (gcc!= NULL);
	g_return_if_fail (GST_IS_CAMERA_CAPTURER (gcc));

	msg_type = GST_MESSAGE_TYPE (message);

	switch (msg_type) {
case GST_MESSAGE_ERROR: {
	gcc_error_msg (gcc, message);
	if (gcc->priv->main_pipeline)
		gst_element_set_state (gcc->priv->main_pipeline, GST_STATE_NULL);              
	break;
						}

case GST_MESSAGE_WARNING: {
	GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
	break;
						  } 

case GST_MESSAGE_EOS:{
	GST_INFO("EOS message");
	g_signal_emit (gcc, gcc_signals[SIGNAL_EOS], 0);
	break;
					 }			  

default:
	GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
	break;
	}
}

static void
gcc_error_msg (GstCameraCapturer * gcc, GstMessage * msg)
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
		g_signal_emit (gcc, gcc_signals[SIGNAL_ERROR], 0,
			err->message);
		g_error_free (err);
	}
	g_free (dbg);
}


static void
gcc_update_interface_implementations (GstCameraCapturer *gcc)
{

	GstXOverlay *old_xoverlay = gcc->priv->xoverlay;
	GstElement *element = NULL;

	GST_INFO("Retrieving xoverlay from bin ...");
	element = gst_bin_get_by_interface (GST_BIN (gcc->priv->camerabin),
		GST_TYPE_X_OVERLAY);

	if (GST_IS_X_OVERLAY (element)) {
		gcc->priv->xoverlay = GST_X_OVERLAY (element);
	} else {
		gcc->priv->xoverlay = NULL;
	}
	if (old_xoverlay)
		gst_object_unref (GST_OBJECT (old_xoverlay));

}

static void
gcc_element_msg_sync (GstBus *bus, GstMessage *msg, gpointer data)
{

	GstCameraCapturer *gcc = GST_CAMERA_CAPTURER (data);
	GstElement *video_sink = NULL;
	GstCaps *caps = NULL;

	g_object_get(gcc->priv->camerabin,"vfsink",&video_sink,NULL);

	g_assert (msg->type == GST_MESSAGE_ELEMENT);

	if (msg->structure == NULL)
		return;

	/* This only gets sent if we haven't set an ID yet. This is our last
	* chance to set it before the video sink will create its own window */
	if (gst_structure_has_name (msg->structure, "prepare-xwindow-id")) {
		GdkWindow *window;
		g_object_get(gcc->priv->camerabin,"filter-caps",&caps,NULL);
		gcc_parse_video_stream_info(caps,gcc);
		gst_caps_unref(caps);

		g_mutex_lock (gcc->priv->lock);
		gcc_update_interface_implementations (gcc);
		g_mutex_unlock (gcc->priv->lock);

		g_return_if_fail (gcc->priv->xoverlay != NULL);
		g_return_if_fail (gcc->priv->video_window != NULL);

		GST_INFO("Setting xwindow: %d",gcc->priv->video_width);

		window = gst_video_widget_get_video_window (GST_VIDEO_WIDGET(gcc->priv->video_window));
#ifdef WIN32
		gst_x_overlay_set_xwindow_id (gcc->priv->xoverlay, GDK_WINDOW_HWND(window));
#else
		gst_x_overlay_set_xwindow_id (gcc->priv->xoverlay, GDK_WINDOW_XID (window));
#endif

		GST_INFO("\nSetting Source size: %d",gcc->priv->video_width);
		gst_video_widget_set_source_size (GST_VIDEO_WIDGET (gcc->priv->video_window),gcc->priv->video_width,gcc->priv->video_height);
	}
}


static int
gcc_parse_video_stream_info (GstCaps *caps, GstCameraCapturer * gcc)
{
	GstStructure *s;

	if (!(caps))
		return -1;

	/* Get video decoder caps */
	s = gst_caps_get_structure (caps, 0);
	if (s) {
		/* We need at least width/height and framerate */
		if (!(gst_structure_get_fraction (s, "framerate", &gcc->priv->video_fps_n, 
			&gcc->priv->video_fps_d) &&
			gst_structure_get_int (s, "width", &gcc->priv->video_width) &&
			gst_structure_get_int (s, "height", &gcc->priv->video_height)))
			return -1;
		/* Get the movie PAR if available */
		gcc->priv->movie_par = gst_structure_get_value (s, "pixel-aspect-ratio"); 
	}
	return 1;
}