/*
 * Gstreamer Video Splitter
 * Copyright (C)  Andoni Morales Alastruey 2009 <ylatuya@gmail.com>
 * 
 * Gstreamer Video Splitter is free software.
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

#ifndef _GST_VIDEO_SPLITTER_H_
#define _GST_VIDEO_SPLITTER_H_

#ifdef WIN32
	#define EXPORT __declspec (dllexport)
#else
	#define EXPORT	
#endif

#include <glib-object.h>
#include <gtk/gtk.h>


G_BEGIN_DECLS

#define GST_TYPE_VIDEO_SPLITTER             (gst_video_splitter_get_type ())
#define GST_VIDEO_SPLITTER(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GST_TYPE_VIDEO_SPLITTER, GstVideoSplitter))
#define GST_VIDEO_SPLITTER_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GST_TYPE_VIDEO_SPLITTER, GstVideoSplitterClass))
#define GST_IS_VIDEO_SPLITTER(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GST_TYPE_VIDEO_SPLITTER))
#define GST_IS_VIDEO_SPLITTER_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GST_TYPE_VIDEO_SPLITTER))
#define GST_VIDEO_SPLITTER_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GST_TYPE_VIDEO_SPLITTER, GstVideoSplitterClass))
#define GVC_ERROR gst_video_splitter_error_quark ()

typedef struct _GstVideoSplitterClass GstVideoSplitterClass;
typedef struct _GstVideoSplitter GstVideoSplitter;
typedef struct GstVideoSplitterPrivate GstVideoSplitterPrivate;


struct _GstVideoSplitterClass
{
	GtkHBoxClass parent_class;
	
	void (*error) (GstVideoSplitter *gvs, const char *message);
	void (*percent_completed) (GstVideoSplitter *gvs, float percent);
};

struct _GstVideoSplitter
{
	GtkHBox parent_instance;
	GstVideoSplitterPrivate *priv;
};

typedef enum
{
	THEORA = 1,
	H264 = 2,
	XVID = 3,
	MPEG2_VIDEO = 4	
}GvsVideoCodec;

typedef enum
{
	VORBIS= 1,
	AAC = 2,
	MP3 = 3,
	MPEG2_AUDIO  = 4
}GvsAudioCodec;

typedef enum{
	MKV = 1,
	AVI = 2,
	DVD = 3
}GvsVideoMuxer;


EXPORT GType gst_video_splitter_get_type (void) G_GNUC_CONST;

EXPORT void gst_video_splitter_init_backend (int *argc, char ***argv);
EXPORT GstVideoSplitter * gst_video_splitter_new (GError ** err);
EXPORT void gst_video_splitter_start(GstVideoSplitter *gvs);
EXPORT void gst_video_splitter_cancel(GstVideoSplitter *gvs);
EXPORT void gst_video_splitter_set_video_encoder(GstVideoSplitter *gvs, GvsVideoCodec codec);
EXPORT void gst_video_splitter_set_audio_encoder(GstVideoSplitter *gvs, GvsAudioCodec codec);
EXPORT void gst_video_splitter_set_video_muxer(GstVideoSplitter *gvs, GvsVideoMuxer codec);
EXPORT void gst_video_splitter_set_segment(GstVideoSplitter *gvs, gchar *file, gint64 start, gint64 duration, gdouble rate, gchar *title);
G_END_DECLS

#endif /* _GST_VIDEO_SPLITTER_H_ */
