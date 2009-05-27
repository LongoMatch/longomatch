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

#ifndef _GST_VIDEO_CAPTURER_H_
#define _GST_VIDEO_CAPTURER_H_

#ifdef WIN32
	#define EXPORT __declspec (dllexport)
#else
	#define EXPORT	
#endif

#include <glib-object.h>
#include <gtk/gtk.h>


G_BEGIN_DECLS

#define GST_TYPE_VIDEO_CAPTURER             (gst_video_capturer_get_type ())
#define GST_VIDEO_CAPTURER(obj)             (G_TYPE_CHECK_INSTANCE_CAST ((obj), GST_TYPE_VIDEO_CAPTURER, GstVideoCapturer))
#define GST_VIDEO_CAPTURER_CLASS(klass)     (G_TYPE_CHECK_CLASS_CAST ((klass), GST_TYPE_VIDEO_CAPTURER, GstVideoCapturerClass))
#define GST_IS_VIDEO_CAPTURER(obj)          (G_TYPE_CHECK_INSTANCE_TYPE ((obj), GST_TYPE_VIDEO_CAPTURER))
#define GST_IS_VIDEO_CAPTURER_CLASS(klass)  (G_TYPE_CHECK_CLASS_TYPE ((klass), GST_TYPE_VIDEO_CAPTURER))
#define GST_VIDEO_CAPTURER_GET_CLASS(obj)   (G_TYPE_INSTANCE_GET_CLASS ((obj), GST_TYPE_VIDEO_CAPTURER, GstVideoCapturerClass))
#define GVC_ERROR gst_video_capturer_error_quark ()

typedef struct _GstVideoCapturerClass GstVideoCapturerClass;
typedef struct _GstVideoCapturer GstVideoCapturer;
typedef struct GstVideoCapturerPrivate GstVideoCapturerPrivate;


struct _GstVideoCapturerClass
{
	GtkHBoxClass parent_class;
	
	void (*eos) (GstVideoCapturer *gvc);
	void (*error) (GstVideoCapturer *gvc, const char *message);
	void (*invalidsource) (GstVideoCapturer *gvc);
};

struct _GstVideoCapturer
{
	GtkHBox parent_instance;
	GstVideoCapturerPrivate *priv;
};



EXPORT GType gst_video_capturer_get_type (void) G_GNUC_CONST;

EXPORT void gst_video_capturer_init_backend (int *argc, char ***argv);
EXPORT GstVideoCapturer * gst_video_capturer_new (gchar *file_source, gchar *output_file,GError ** err);
EXPORT void gst_video_capturer_start(GstVideoCapturer *gvc);
EXPORT void gst_video_capturer_set_segment(GstVideoCapturer *gvc, gint64 start, gint64 duration, gdouble rate);
EXPORT void gst_video_capturer_add_segment(GstVideoCapturer *gvc, gint64 start, gint64 duration, gdouble rate, gchar *title);
G_END_DECLS

#endif /* _GST_VIDEO_CAPTURER_H_ */
