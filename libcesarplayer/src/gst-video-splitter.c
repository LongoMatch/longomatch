 /*GStreamer Video Splitter Based On GNonlin
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

#include <glib/gprintf.h>


#include "gst-video-splitter.h"
#include "gst-smart-video-scaler.h"

#define DEFAULT_VIDEO_ENCODER "theoraenc"
#define DEFAULT_AUDIO_ENCODER "vorbisenc"
#define DEFAULT_VIDEO_MUXER "matroskamux"
#define FONT_SIZE_FACTOR 0.03

#define TIMEOUT 50

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
	PROP_ENABLE_AUDIO,
	PROP_ENABLE_TITLE,
	PROP_VIDEO_BITRATE,
	PROP_AUDIO_BITRATE,
	PROP_HEIGHT,
	PROP_WIDTH,
	PROP_OUTPUT_FILE
};

struct GstVideoSplitterPrivate
{
	gint 		segments;
	gint		active_segment;
	gint64		*stop_times;
	GList		*titles;
	GList		*gnl_filesources;
	gint64 		duration;
	
	gboolean 	audio_enabled;
	gboolean	title_enabled;	
	gchar		*output_file;
	gint		audio_bitrate;
	gint		video_bitrate;
	gint 		width;
	gint		height;
	
	GstElement 	*main_pipeline;	
		
	GstElement 	*gnl_composition;
	GstElement	*gnl_filesource;
	GstElement 	*identity;
	GstElement  *ffmpegcolorspace;
	GstElement 	*videorate;	
	GstElement  *textoverlay;
	GstElement  *videoscale;
	GstElement  *capsfilter;
	GstElement  *videobox;
	GstElement 	*queue;
    GstElement 	*video_encoder;
	GstElement 	*audio_encoder;
	GstElement 	*muxer;
	GstElement 	*file_sink;
	
	GstBus  	*bus;
	gulong   	sig_bus_async;
	
	gint 		update_id;

};

static int gvs_signals[LAST_SIGNAL] = { 0 };
static void gvs_error_msg (GstVideoSplitter * gvs, GstMessage * msg);
static void new_decoded_pad_cb (GstElement* object,GstPad* arg0,gpointer user_data);
static void gvs_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data);
static void gst_video_splitter_get_property (GObject * object, guint property_id, GValue * value, GParamSpec * pspec);
static void gst_video_splitter_set_property (GObject * object, guint property_id,const GValue * value, GParamSpec * pspec);
static gboolean  gvs_query_timeout (GstVideoSplitter * gvs);
static void gvs_apply_new_caps (GstVideoSplitter *gvs);
G_DEFINE_TYPE (GstVideoSplitter, gst_video_splitter, G_TYPE_OBJECT);



/* =========================================== */
/*                                             */
/*      Class Initialization/Finalization      */
/*                                             */
/* =========================================== */

static void
gst_video_splitter_init (GstVideoSplitter *object)
{
	GstVideoSplitterPrivate *priv;
  	object->priv = priv = G_TYPE_INSTANCE_GET_PRIVATE (object, GST_TYPE_VIDEO_SPLITTER, GstVideoSplitterPrivate);
		
	priv->output_file = "new_video.avi";
	
	priv->audio_bitrate = 128;
	priv->video_bitrate = 5000000;
	priv->height = 540;
	priv->width = 720;	
	priv->title_enabled = TRUE;
	priv->audio_enabled = TRUE;
	
	priv->duration = 0;
	priv->segments = 0;
	priv->gnl_filesources = NULL;
	priv->titles = NULL;
	priv->stop_times = (gint64 *)malloc (200*sizeof(gint64));
	
	priv->update_id = 0;
	
}

static void
gst_video_splitter_finalize (GObject *object)
{
  	GstVideoSplitter *gvs = (GstVideoSplitter *) object;
  
  	if (gvs->priv->bus) {
    	/* make bus drop all messages to make sure none of our callbacks is ever
     	 called again (main loop might be run again to display error dialog) */
    	gst_bus_set_flushing (gvs->priv->bus, TRUE);

    if (gvs->priv->sig_bus_async)
		g_signal_handler_disconnect (gvs->priv->bus,gvs->priv->sig_bus_async);
		gst_object_unref (gvs->priv->bus);
		gvs->priv->bus = NULL;
	}  	
  
  	if (gvs->priv->main_pipeline != NULL && GST_IS_ELEMENT (gvs->priv->main_pipeline )) {
    	gst_element_set_state (gvs->priv->main_pipeline , GST_STATE_NULL);
    	gst_object_unref (gvs->priv->main_pipeline);
    	gvs->priv->main_pipeline = NULL;
  	}
  	
  	g_free (gvs->priv->output_file);
  	g_list_free(gvs->priv->gnl_filesources);
  	g_free(gvs->priv->stop_times);
  	g_list_free(gvs->priv->titles);
  	 	
	G_OBJECT_CLASS (gst_video_splitter_parent_class)->finalize (object);
}


static void
gst_video_splitter_class_init (GstVideoSplitterClass *klass)
{
	GObjectClass* object_class = G_OBJECT_CLASS (klass);

	object_class->finalize = gst_video_splitter_finalize;

	g_type_class_add_private (object_class, sizeof (GstVideoSplitterPrivate));

	 /* GObject */
  	object_class->set_property = gst_video_splitter_set_property;
 	object_class->get_property = gst_video_splitter_get_property;
 	object_class->finalize = gst_video_splitter_finalize;

 	/* Properties */
  	
  	g_object_class_install_property (object_class, PROP_ENABLE_AUDIO,
                                   g_param_spec_boolean ("enable-audio", NULL,
                                                         NULL, TRUE,
                                                         G_PARAM_READWRITE));
                                                         
    g_object_class_install_property (object_class, PROP_ENABLE_TITLE,
                                   g_param_spec_boolean ("enable-title", NULL,
                                                         NULL, TRUE,
                                                         G_PARAM_READWRITE));
                                                         
  	g_object_class_install_property (object_class, PROP_VIDEO_BITRATE,
                                   g_param_spec_int ("video_bitrate", NULL,
                                                         NULL, 100, G_MAXINT,1000,
                                                         G_PARAM_READWRITE));
                                                         
  	g_object_class_install_property (object_class, PROP_AUDIO_BITRATE,
                                   g_param_spec_int ("audio_bitrate", NULL,
                                                         NULL, 12, G_MAXINT,128,
                                                         G_PARAM_READWRITE));
    g_object_class_install_property (object_class, PROP_HEIGHT,
                                   g_param_spec_int ("height", NULL,
                                                         NULL, 240, 1080,480,
                                                         G_PARAM_READWRITE));
    g_object_class_install_property (object_class, PROP_WIDTH,
                                   g_param_spec_int ("width", NULL,
                                                         NULL, 340, 1920,720,
                                                         G_PARAM_READWRITE));
                                                         
    g_object_class_install_property (object_class, PROP_OUTPUT_FILE,
                                   g_param_spec_string ("output_file", NULL,
                                                         NULL, "",
                                                         G_PARAM_READWRITE));
 	
  	/* Signals */
  	gvs_signals[SIGNAL_ERROR] =
    	g_signal_new ("error",
                  	G_TYPE_FROM_CLASS (object_class),
                  	G_SIGNAL_RUN_LAST,
                  	G_STRUCT_OFFSET (GstVideoSplitterClass, error),
                  	NULL, NULL,
                  	g_cclosure_marshal_VOID__STRING,
                  	G_TYPE_NONE, 1, G_TYPE_STRING);
                  
  	gvs_signals[SIGNAL_PERCENT_COMPLETED] =
    	g_signal_new ("percent_completed",
                  	G_TYPE_FROM_CLASS (object_class),
                  	G_SIGNAL_RUN_LAST,
                  	G_STRUCT_OFFSET (GstVideoSplitterClass, percent_completed),
                  	NULL, NULL,
                  	g_cclosure_marshal_VOID__FLOAT,
                  	G_TYPE_NONE, 1, G_TYPE_FLOAT);

 }

/* =========================================== */
/*                                             */
/*                Properties                   */
/*                                             */
/* =========================================== */

static void 
gst_video_splitter_set_enable_audio(GstVideoSplitter *gvs,gboolean audio_enabled)
{
	//TODO Not implemented
}

static void 
gst_video_splitter_set_enable_title(GstVideoSplitter *gvs,gboolean title_enabled)
{
	gvs->priv->title_enabled = title_enabled;	
	g_object_set (G_OBJECT(gvs->priv->textoverlay), "silent",!gvs->priv->title_enabled,NULL);
	
}
static void 
gst_video_splitter_set_video_bit_rate (GstVideoSplitter *gvs,gint bitrate)
{
	GstState cur_state;

	gvs->priv->video_bitrate= bitrate;
	gst_element_get_state (gvs->priv->video_encoder, &cur_state, NULL, 0);
    if (cur_state <= GST_STATE_READY) {
	    g_object_set (gvs->priv->video_encoder,"bitrate",bitrate,NULL);
    	GST_INFO ("Encoding video bitrate changed to :\n%d",bitrate);
   	}
}

static void 
gst_video_splitter_set_audio_bit_rate (GstVideoSplitter *gvs,gint bitrate)
{
	//TODO Not implemented
   
   
}

static void 
gst_video_splitter_set_width (GstVideoSplitter *gvs,gint width)
{
	gvs->priv->width = width;
	gvs_apply_new_caps (gvs);  
}

static void 
gst_video_splitter_set_height (GstVideoSplitter *gvs,gint height)
{
	gvs->priv->height = height;
	gvs_apply_new_caps(gvs);   
}

static void 
gst_video_splitter_set_output_file (GstVideoSplitter *gvs,const char *output_file)
{
	GstState cur_state;

	gvs->priv->output_file = g_strdup(output_file);
	gst_element_get_state (gvs->priv->file_sink, &cur_state, NULL, 0);
    if (cur_state <= GST_STATE_READY) {
	    gst_element_set_state(gvs->priv->file_sink,GST_STATE_NULL);
	    g_object_set (gvs->priv->file_sink,"location",gvs->priv->output_file,NULL);
    	GST_INFO ("Ouput File changed to :\n%s",gvs->priv->output_file);
   	}
}
static void
gst_video_splitter_set_property (GObject * object, guint property_id,
                                 const GValue * value, GParamSpec * pspec)
{
	GstVideoSplitter *gvs;

	gvs = GST_VIDEO_SPLITTER (object);

 	switch (property_id) {      
    	case PROP_ENABLE_AUDIO:
    		gst_video_splitter_set_enable_audio (gvs,
    		g_value_get_boolean (value));
    		break;
    	case PROP_ENABLE_TITLE:
    		gst_video_splitter_set_enable_title (gvs,
    		g_value_get_boolean (value));
    		break;
    	case PROP_VIDEO_BITRATE:
      		gst_video_splitter_set_video_bit_rate (gvs,
      		g_value_get_int (value));
      		break;
    	case PROP_AUDIO_BITRATE:
      		gst_video_splitter_set_audio_bit_rate (gvs,
      		g_value_get_int (value));
      		break;
      	case PROP_WIDTH:
      		gst_video_splitter_set_width (gvs,
      		g_value_get_int (value));
      		break;
    	case PROP_HEIGHT:
      		gst_video_splitter_set_height (gvs,
      		g_value_get_int (value));
      		break;
      	case PROP_OUTPUT_FILE:
      		gst_video_splitter_set_output_file (gvs,
      		g_value_get_string (value));
      		break;
    	default:
      		G_OBJECT_WARN_INVALID_PROPERTY_ID (object, property_id, pspec);
      	break;
  }
}

static void
gst_video_splitter_get_property (GObject * object, guint property_id,
                                 GValue * value, GParamSpec * pspec)
{
  	GstVideoSplitter *gvs;

  	gvs = GST_VIDEO_SPLITTER (object);

  	switch (property_id) {
		case PROP_ENABLE_AUDIO:
      		g_value_set_boolean (value,gvs->priv->audio_enabled);
      		break;
    	case PROP_ENABLE_TITLE:
      		g_value_set_boolean (value,gvs->priv->title_enabled);
      		break;
    	case PROP_AUDIO_BITRATE:
      		g_value_set_int (value,gvs->priv->audio_bitrate);
      		break;
    	case PROP_VIDEO_BITRATE:
      		g_value_set_int (value,gvs->priv->video_bitrate);
      		break;
    	case PROP_WIDTH:
      		g_value_set_int (value,gvs->priv->width);
      		break;
    	case PROP_HEIGHT:
      		g_value_set_int (value,gvs->priv->height);
      		break;
    	case PROP_OUTPUT_FILE:
      		g_value_set_string (value,gvs->priv->output_file);
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
gvs_rewrite_headers (GstVideoSplitter *gvs)
{
	gst_element_set_state(gvs->priv->file_sink,GST_STATE_NULL);
	gst_element_set_state(gvs->priv->file_sink,GST_STATE_READY);

}
static void
gvs_set_tick_timeout (GstVideoSplitter *gvs , guint msecs)
{
	g_return_if_fail (GST_IS_VIDEO_SPLITTER(gvs));

  	if (msecs > 0) {
    	GST_INFO ("adding tick timeout (at %ums)", msecs);
    	gvs->priv->update_id =
      		g_timeout_add (msecs, (GSourceFunc) gvs_query_timeout, gvs);
  	}
}

static void 
gvs_apply_new_caps (GstVideoSplitter *gvs)
{
	GstCaps *caps;
	gchar *font;
	
	caps = gst_caps_new_simple ("video/x-raw-yuv",
      "width", G_TYPE_INT, gvs->priv->width,
      "height", G_TYPE_INT, gvs->priv->height,
      "framerate",GST_TYPE_FRACTION,25,1,
       NULL);
       
  	g_object_set (G_OBJECT(gvs->priv->capsfilter), "caps",caps,NULL);
	font = g_strdup_printf("sans bold %d",(int)(gvs->priv->width*FONT_SIZE_FACTOR));
	g_object_set (G_OBJECT(gvs->priv->textoverlay), "font-desc",font,NULL);
	g_free(font);
	gst_caps_unref(caps);
}

GQuark
gst_video_splitter_error_quark (void)
{
  	static GQuark q; /* 0 */

  	if (G_UNLIKELY (q == 0)) {
    	q = g_quark_from_static_string ("gvs-error-quark");
  	}
  	return q;
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
	GstPad *videopad=NULL;
	GstVideoSplitter *gvs=NULL;

  	g_return_if_fail (GST_IS_VIDEO_SPLITTER(user_data));
 	gvs = GST_VIDEO_SPLITTER (user_data);

	/* check media type */
	caps = gst_pad_get_caps (pad);
	str = gst_caps_get_structure (caps, 0);	
	
	if (g_strrstr (gst_structure_get_name (str), "video")){

		videopad = gst_element_get_compatible_pad (gvs->priv->identity, pad, NULL);
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
gvs_bus_message_cb (GstBus * bus, GstMessage * message, gpointer data)
{
  	GstVideoSplitter *gvs = (GstVideoSplitter *) data;
  	GstMessageType msg_type;

  	g_return_if_fail (gvs!= NULL);
  	g_return_if_fail (GST_IS_VIDEO_SPLITTER (gvs));

  	msg_type = GST_MESSAGE_TYPE (message);

  	switch (msg_type) {
    	case GST_MESSAGE_ERROR: 
      		gvs_error_msg (gvs, message);
      		if (gvs->priv->main_pipeline)
          		gst_element_set_state (gvs->priv->main_pipeline, GST_STATE_READY);
     		 break;    	
    	case GST_MESSAGE_WARNING: 
      		GST_WARNING ("Warning message: %" GST_PTR_FORMAT, message);
      		break;
	    case GST_MESSAGE_EOS:
      		if (gvs->priv->update_id > 0){
				g_source_remove (gvs->priv->update_id);
				gvs->priv->update_id = 0;
	  		}
	  		gst_element_set_state (gvs->priv->main_pipeline, GST_STATE_NULL);
	  		g_signal_emit (gvs, gvs_signals[SIGNAL_PERCENT_COMPLETED],0,(gfloat)1);
	  		gvs->priv->active_segment = 0;
	  		g_object_set (G_OBJECT(gvs->priv->file_sink), "location","",NULL);
      		break;
    	default:
      		GST_LOG ("Unhandled message: %" GST_PTR_FORMAT, message);
      		break;
  }
 }

static void
gvs_error_msg (GstVideoSplitter * gvs, GstMessage * msg)
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
    	
    	g_message ("Error: %s\n%s\n", GST_STR_NULL (err->message),GST_STR_NULL (dbg));
    	g_signal_emit (gvs, gvs_signals[SIGNAL_ERROR], 0,err->message);
   	 	g_error_free (err);
  	}
  	g_free (dbg);
}

static gboolean  
gvs_query_timeout (GstVideoSplitter * gvs)
{
	GstFormat fmt = GST_FORMAT_TIME;
    gint64 pos = -1;
    gchar *title;
    gint64 stop_time = gvs->priv->stop_times[gvs->priv->active_segment];    
	
	if (gst_element_query_position (gvs->priv->main_pipeline, &fmt, &pos)) {
    	if (pos != -1 && fmt == GST_FORMAT_TIME) {
      		g_signal_emit 	(gvs, 
      						gvs_signals[SIGNAL_PERCENT_COMPLETED], 
      						0, 
      						(float) pos / (float)gvs->priv->duration);      		    		
    	}    	   	
  	} 
  	else {
    	GST_INFO ("could not get position");
  	}
  	
  	if (gst_element_query_position (gvs->priv->video_encoder, &fmt, &pos)) {
	  	if ( stop_time - pos <= 0){
	      		gvs->priv->active_segment++;
	      		title = (gchar*) g_list_nth_data (gvs->priv->titles, gvs->priv->active_segment); 
	      		g_object_set (G_OBJECT(gvs->priv->textoverlay), "text",title,NULL);
      	}  
  	}
  	  
  	
	return TRUE;
}

/* =========================================== */
/*                                             */
/*              Public Methods                 */
/*                                             */
/* =========================================== */


void 
gst_video_splitter_add_segment (GstVideoSplitter *gvs , gchar *file, gint64 start, gint64 duration, gdouble rate, gchar *title)
{	
	GstState cur_state;
	GstElement *gnl_filesource;
	GstCaps *filter;
		
	gchar *element_name = "";
	gint64 final_duration;	
	
	g_return_if_fail (GST_IS_VIDEO_SPLITTER(gvs));		
	
	gst_element_get_state (gvs->priv->gnl_composition, &cur_state, NULL, 0);
    if (cur_state <= GST_STATE_READY) {	
    	final_duration = GST_MSECOND * duration / rate; 
    	 
   		filter = gst_caps_new_simple ("video/x-raw-yuv",NULL);    	
       	element_name = g_strdup_printf("gnlfilesource%d",gvs->priv->segments);
		gnl_filesource = gst_element_factory_make ("gnlfilesource", element_name);		
		g_object_set (G_OBJECT(gnl_filesource), "location",file,NULL);
		g_object_set (G_OBJECT(gnl_filesource), "media-start",GST_MSECOND*start,NULL);
		g_object_set (G_OBJECT(gnl_filesource), "media-duration",GST_MSECOND*duration,NULL);
		g_object_set (G_OBJECT(gnl_filesource), "start",gvs->priv->duration,NULL);
		g_object_set (G_OBJECT(gnl_filesource), "duration",final_duration,NULL);
		g_object_set (G_OBJECT(gnl_filesource), "caps",filter,NULL);	
		if (gvs->priv->segments == 0){
			g_object_set (G_OBJECT(gvs->priv->textoverlay), "text",title,NULL);
		}		
		gst_bin_add (GST_BIN(gvs->priv->gnl_composition), gnl_filesource);
				
		gvs->priv->duration += final_duration;	
		gvs->priv->segments++;	
		
		gvs->priv->gnl_filesources = g_list_append(gvs->priv->gnl_filesources,gnl_filesource);
		gvs->priv->titles = g_list_append(gvs->priv->titles,title);
		gvs->priv->stop_times[gvs->priv->segments-1] = gvs->priv->duration;		
			
		GST_INFO("New segment: start={%" GST_TIME_FORMAT "} duration={%" GST_TIME_FORMAT "} ",GST_TIME_ARGS(start * GST_MSECOND), GST_TIME_ARGS(duration * GST_MSECOND));
    }
    else
    	GST_WARNING("Segments can only be added for a state <= GST_STATE_READY");
	g_free(element_name);	
}

void
gst_video_splitter_clear_segments_list (GstVideoSplitter *gvs)
{	

	GList *tmp = NULL; 

	g_return_if_fail (GST_IS_VIDEO_SPLITTER(gvs));
	
	gst_video_splitter_cancel(gvs);
	
	tmp = gvs->priv->gnl_filesources;

	for (; tmp; tmp = g_list_next (tmp)) {
    	GstElement *object = (GstElement *) tmp->data;
		if (object)
			gst_element_set_state (object, GST_STATE_NULL);
			gst_bin_remove(GST_BIN(gvs->priv->gnl_composition), object);					
	}
	
	g_list_free(tmp);
	g_list_free(gvs->priv->gnl_filesources);
	g_free(gvs->priv->stop_times);
	g_list_free(gvs->priv->titles);
	
	gvs->priv->gnl_filesources = NULL;
	gvs->priv->stop_times = (gint64 *)malloc (200*sizeof(gint64));
	gvs->priv->titles = NULL;	
	
	gvs->priv->duration = 0;
	gvs->priv->active_segment= 0;		
}


void 
gst_video_splitter_set_video_encoder (GstVideoSplitter *gvs, gchar **err, GvsVideoCodec codec)
{
	GstElement *encoder = NULL;
	GstState cur_state;
	gchar *encoder_name="";
	gchar *error;
	
	g_return_if_fail (GST_IS_VIDEO_SPLITTER(gvs));
	
	gst_element_get_state (gvs->priv->main_pipeline, &cur_state, NULL, 0);
	
	if (cur_state <= GST_STATE_READY) {		
		switch (codec){
			case H264:		
				encoder_name = "x264enc";		
				encoder = gst_element_factory_make (encoder_name,encoder_name);
				g_object_set (G_OBJECT(encoder), "pass",17,NULL);	//Variable Bitrate-Pass 1
				break;
			case XVID:
				encoder_name = "xvidenc";
				encoder = gst_element_factory_make (encoder_name,encoder_name);
				g_object_set (G_OBJECT(encoder), "pass",1,NULL);	//Variable Bitrate-Pass 1
				
				break;
			case MPEG2_VIDEO:
				encoder_name = "mpeg2enc";
				encoder = gst_element_factory_make (encoder_name,encoder_name);
				g_object_set (G_OBJECT(encoder), "format",9,NULL);	//DVD compilant
				g_object_set (G_OBJECT(encoder), "framerate",3,NULL);	//25 FPS (PAL/SECAM)	
				break;
			case THEORA:
				encoder_name = "theoraenc";
				encoder = gst_element_factory_make (encoder_name,encoder_name);				
				break;				
		}		
	
		if (encoder){
			if (!g_strcmp0(gst_element_get_name(gvs->priv->video_encoder),encoder_name)){
				GST_WARNING("The video encoder is not changed because it is already in use.");
				gst_object_unref(encoder);
				return;
			}		
			gst_element_unlink(gvs->priv->queue,gvs->priv->video_encoder);
			gst_element_unlink(gvs->priv->video_encoder,gvs->priv->muxer);
			gst_element_set_state(gvs->priv->video_encoder,GST_STATE_NULL);
			gst_bin_remove(GST_BIN(gvs->priv->main_pipeline),gvs->priv->video_encoder);
			g_object_set (G_OBJECT(encoder), "bitrate",gvs->priv->video_bitrate,NULL);
			gst_bin_add(GST_BIN(gvs->priv->main_pipeline),encoder);
			gst_element_link_many(gvs->priv->queue,encoder,gvs->priv->muxer,NULL);		
			gvs->priv->video_encoder = encoder;	
			gvs_rewrite_headers(gvs);		
		}
		
		else {
			error = g_strdup_printf("The %s encoder element is not avalaible. Check your GStreamer installation", encoder_name);
			GST_ERROR (error);
			*err = g_strdup(error);
			g_free(error);
		}
	}
	else
    	GST_WARNING("The video encoder cannot be changed for a state <= GST_STATE_READY");
}

void 
gst_video_splitter_set_audio_encoder (GstVideoSplitter *gvs, gchar **err, GvsAudioCodec codec)
{
	//TODO not implemented
}

void 
gst_video_splitter_set_video_muxer (GstVideoSplitter *gvs, gchar **err, GvsVideoMuxer muxerType)
{
	GstElement *muxer = NULL;
	GstState cur_state;
	gchar *muxer_name="";
	gchar *error;
	
	g_return_if_fail (GST_IS_VIDEO_SPLITTER(gvs));
	
	gst_element_get_state (gvs->priv->main_pipeline, &cur_state, NULL, 0);
	
	if (cur_state <= GST_STATE_READY) {		
		switch (muxerType){
			case MKV:		
				muxer_name = "matroskamux";		
				muxer = gst_element_factory_make ("matroskamux",muxer_name);
				break;
			case AVI:
				muxer_name = "avimux";
				muxer = gst_element_factory_make ("avimux",muxer_name);
				break;
			case OGG:
				muxer_name = "oggmux";
				muxer = gst_element_factory_make ("oggmux",muxer_name);
				break;
			case DVD:
				muxer_name = "ffmux_dvd";
				//We don't want to mux anything yet as ffmux_dvd is buggy
				//FIXME: Until we don't have audio save the mpeg-ps stream without mux.
				muxer = gst_element_factory_make ("identity",muxer_name);
				break;
		}		
	
		if (muxer){	
			if (!g_strcmp0(gst_element_get_name(gvs->priv->muxer),muxer_name)){
				GST_WARNING("Not changing the video muxer as the new one is the same in use.");
				gst_object_unref(muxer);
				return;
			}	
			gst_element_unlink(gvs->priv->video_encoder,gvs->priv->muxer);
			//gst_element_unlink(gvs->priv->audio_encoder,gvs->priv->muxer);
			gst_element_unlink(gvs->priv->muxer,gvs->priv->file_sink);
			gst_element_set_state(gvs->priv->muxer,GST_STATE_NULL);
			gst_bin_remove(GST_BIN(gvs->priv->main_pipeline),gvs->priv->muxer);
			gst_bin_add(GST_BIN(gvs->priv->main_pipeline),muxer);
			gst_element_link_many(gvs->priv->video_encoder,muxer,gvs->priv->file_sink,NULL);	
			//gst_element_link(gvs->priv->audio_encoder,muxer);	
			gvs->priv->muxer = muxer;
			gvs_rewrite_headers(gvs);				
		}
		
		else {
			error = g_strdup_printf( "The %s muxer element is not avalaible. Check your GStreamer installation", muxer_name);
			GST_ERROR (error);
			*err = g_strdup(error);
			g_free(error);
		}			
	}
	else
    	GST_WARNING("The video muxer cannot be changed for a state <= GST_STATE_READY");
}

void 
gst_video_splitter_start(GstVideoSplitter *gvs)
{	
	g_return_if_fail (GST_IS_VIDEO_SPLITTER(gvs));
	
	gst_element_set_state(gvs->priv->main_pipeline, GST_STATE_PLAYING);	
	g_signal_emit (gvs, gvs_signals[SIGNAL_PERCENT_COMPLETED],0,(gfloat)0);	
	gvs_set_tick_timeout(gvs,TIMEOUT);
}

void 
gst_video_splitter_cancel(GstVideoSplitter *gvs)
{	
	g_return_if_fail (GST_IS_VIDEO_SPLITTER(gvs));
	if (gvs->priv->update_id > 0){
		g_source_remove (gvs->priv->update_id);
		gvs->priv->update_id = 0;
	}
	gst_element_set_state(gvs->priv->main_pipeline, GST_STATE_NULL);
	g_signal_emit (gvs, gvs_signals[SIGNAL_PERCENT_COMPLETED],0,(gfloat)-1);
}

void
gst_video_splitter_init_backend (int *argc, char ***argv)
{
  	gst_init (argc, argv);
}

GstVideoSplitter *
gst_video_splitter_new (GError ** err)
{
	GstVideoSplitter *gvs = NULL;
	GstCaps *filter=NULL;

	gvs = g_object_new(GST_TYPE_VIDEO_SPLITTER, NULL);
	
	gvs->priv->main_pipeline = gst_pipeline_new ("main_pipeline");

	if (!gvs->priv->main_pipeline ) {
    	g_set_error (err, GVC_ERROR, GVC_ERROR_PLUGIN_LOAD,
                	("Failed to create a GStreamer Bin. "
                    "Please check your GStreamer installation."));
    	g_object_ref_sink (gvs);
    	g_object_unref (gvs);
    	return NULL;
  	}

  	/* Create elements*/

  	gvs->priv->gnl_composition = gst_element_factory_make("gnlcomposition","gnlcomposition");
  	if (!gvs->priv->gnl_composition){	
  		g_set_error (err, GVC_ERROR, GVC_ERROR_PLUGIN_LOAD,
                 ("Failed to create a Gnonlin element. "
                   "Please check your GStreamer installation."));
        g_object_ref_sink (gvs);
    	g_object_unref (gvs);
        return NULL;                   
  	}  	  	
    gvs->priv->identity = gst_element_factory_make ("identity", "identity");  
    gvs->priv->ffmpegcolorspace = gst_element_factory_make ("ffmpegcolorspace", "ffmpegcolorspace");   
    gvs->priv->videorate = gst_element_factory_make ("videorate", "videorate");
    gvs->priv->videoscale = gst_element_factory_make ("videoscale", "videoscale"); 
    gvs->priv->capsfilter = gst_element_factory_make ("capsfilter", "capsfilter");     
	gvs->priv->videobox = gst_element_factory_make ("videobox", "videobox");    
    gvs->priv->textoverlay = gst_element_factory_make ("textoverlay","textoverlay");
    gvs->priv->queue =  gst_element_factory_make ("queue", "queue");     
    gvs->priv->video_encoder= gst_element_factory_make (DEFAULT_VIDEO_ENCODER, "theoraenc");
    gvs->priv->muxer = gst_element_factory_make (DEFAULT_VIDEO_MUXER, "videomuxer");
    gvs->priv->file_sink = gst_element_factory_make ("filesink", "filesink");
    
    /* Set elements properties*/
    
    g_object_set (G_OBJECT(gvs->priv->identity), "single-segment",TRUE,NULL);
   	g_object_set (G_OBJECT(gvs->priv->textoverlay), "font-desc","sans bold 20",NULL);
   	g_object_set (G_OBJECT(gvs->priv->textoverlay), "shaded-background",TRUE,NULL);
	g_object_set (G_OBJECT(gvs->priv->textoverlay), "valignment",2,NULL);
	g_object_set (G_OBJECT(gvs->priv->textoverlay), "halignment",2,NULL);  
    g_object_set (G_OBJECT(gvs->priv->video_encoder), "bitrate",gvs->priv->video_bitrate,NULL); 
    g_object_set (G_OBJECT(gvs->priv->file_sink), "location",gvs->priv->output_file ,NULL); 
    
    /* Link elements*/

	gst_bin_add_many (	GST_BIN (gvs->priv->main_pipeline),
		gvs->priv->gnl_composition,	
		gvs->priv->identity,	
		gvs->priv->ffmpegcolorspace,
		gvs->priv->videorate,
		gvs->priv->videoscale,
		gvs->priv->capsfilter,
		gvs->priv->videobox,
		gvs->priv->textoverlay,		
		gvs->priv->queue,
		gvs->priv->video_encoder,
		gvs->priv->muxer,						
		gvs->priv->file_sink,
		NULL);
		
	gst_element_link_many(	gvs->priv->identity,
		gvs->priv->ffmpegcolorspace,
		gvs->priv->videorate,		
		gvs->priv->videoscale,
		gvs->priv->capsfilter,
		gvs->priv->videobox,
		gvs->priv->textoverlay,
		gvs->priv->queue,
		gvs->priv->video_encoder,
		gvs->priv->muxer,						
		gvs->priv->file_sink,
		NULL);
		
   
	/*Connect bus signals*/
	
    /*We have to wait for a "new-decoded-pad" message to link the composition with
    the encoder tail*/
	gvs->priv->bus = gst_element_get_bus (GST_ELEMENT(gvs->priv->main_pipeline));
  	g_signal_connect (gvs->priv->gnl_composition, "pad-added",G_CALLBACK(new_decoded_pad_cb),gvs);
	gst_bus_add_signal_watch (gvs->priv->bus);
  	gvs->priv->sig_bus_async =	g_signal_connect (gvs->priv->bus, "message",
                        		G_CALLBACK (gvs_bus_message_cb),
                        		gvs);                        		
                        		
	gst_element_set_state(gvs->priv->main_pipeline,GST_STATE_READY);
	
	return gvs;
}
