/*
 * Copyright (C) 2001,2002,2003,2004,2005 Bastien Nocera <hadess@hadess.net>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 * The Totem project hereby grant permission for non-gpl compatible GStreamer
 * plugins to be used and distributed together with GStreamer and Totem. This
 * permission are above and beyond the permissions granted by the GPL license
 * Totem is covered by.
 *
 * Monday 7th February 2005: Christian Schaller: Add excemption clause.
 * See license_change file for details.
 *
 */

#ifndef HAVE_BACON_VIDEO_WIDGET_H
#define HAVE_BACON_VIDEO_WIDGET_H

#ifdef WIN32
	#define EXPORT __declspec (dllexport)
#else
	#define EXPORT	
#endif


#include <gtk/gtkbox.h>
#include <gst/gst.h>
/* for optical disc enumeration type */
//#include "totem-disc.h"

G_BEGIN_DECLS

#define BACON_TYPE_VIDEO_WIDGET		     (bacon_video_widget_get_type ())
#define BACON_VIDEO_WIDGET(obj)              (G_TYPE_CHECK_INSTANCE_CAST ((obj), bacon_video_widget_get_type (), BaconVideoWidget))
#define BACON_VIDEO_WIDGET_CLASS(klass)      (G_TYPE_CHECK_CLASS_CAST ((klass), bacon_video_widget_get_type (), BaconVideoWidgetClass))
#define BACON_IS_VIDEO_WIDGET(obj)           (G_TYPE_CHECK_INSTANCE_TYPE (obj, bacon_video_widget_get_type ()))
#define BACON_IS_VIDEO_WIDGET_CLASS(klass)   (G_CHECK_INSTANCE_GET_CLASS ((klass), bacon_video_widget_get_type ()))
#define BVW_ERROR bacon_video_widget_error_quark ()

typedef struct BaconVideoWidgetPrivate BaconVideoWidgetPrivate;


typedef struct {
	GObject parent;
	BaconVideoWidgetPrivate *priv;
} BaconVideoWidget;

typedef struct {
	GObjectClass parent_class;

	void (*error) (BaconVideoWidget *bvw, const char *message);
	void (*eos) (BaconVideoWidget *bvw);
	void (*got_metadata) (BaconVideoWidget *bvw);
	void (*segment_done) (BaconVideoWidget *bvw);
	void (*got_redirect) (BaconVideoWidget *bvw, const char *mrl);
	void (*title_change) (BaconVideoWidget *bvw, const char *title);
	void (*channels_change) (BaconVideoWidget *bvw);
	void (*tick) (BaconVideoWidget *bvw, gint64 current_time, gint64 stream_length,
			float current_position, gboolean seekable);
	void (*buffering) (BaconVideoWidget *bvw, guint progress);
	void (*state_changed) (BaconVideoWidget *bvw, gboolean playing);
	void (*got_duration) (BaconVideoWidget *bvw);
	void (*ready_to_seek) (BaconVideoWidget *bvw);
} BaconVideoWidgetClass;

typedef enum {
	/* Plugins */
	BVW_ERROR_AUDIO_PLUGIN,
	BVW_ERROR_NO_PLUGIN_FOR_FILE,
	BVW_ERROR_VIDEO_PLUGIN,
	BVW_ERROR_AUDIO_BUSY,
	/* File */
	BVW_ERROR_BROKEN_FILE,
	BVW_ERROR_FILE_GENERIC,
	BVW_ERROR_FILE_PERMISSION,
	BVW_ERROR_FILE_ENCRYPTED,
	BVW_ERROR_FILE_NOT_FOUND,
	/* Devices */
	BVW_ERROR_DVD_ENCRYPTED,
	BVW_ERROR_INVALID_DEVICE,
	/* Network */
	BVW_ERROR_UNKNOWN_HOST,
	BVW_ERROR_NETWORK_UNREACHABLE,
	BVW_ERROR_CONNECTION_REFUSED,
	/* Generic */
	BVW_ERROR_UNVALID_LOCATION,
	BVW_ERROR_GENERIC,
	BVW_ERROR_CODEC_NOT_HANDLED,
	BVW_ERROR_AUDIO_ONLY,
	BVW_ERROR_CANNOT_CAPTURE,
	BVW_ERROR_READ_ERROR,
	BVW_ERROR_PLUGIN_LOAD,
	BVW_ERROR_EMPTY_FILE
} BvwError;

GQuark bacon_video_widget_error_quark		 (void) G_GNUC_CONST;
EXPORT GType bacon_video_widget_get_type                (void);
GOptionGroup* bacon_video_widget_get_option_group (void);
/* This can be used if the app does not use popt */
EXPORT void bacon_video_widget_init_backend		 (int *argc, char ***argv);

typedef enum {
	BVW_USE_TYPE_VIDEO,
	BVW_USE_TYPE_AUDIO,
	BVW_USE_TYPE_CAPTURE,
	BVW_USE_TYPE_METADATA
} BvwUseType;

EXPORT BaconVideoWidget *bacon_video_widget_new		 (int width, int height,
						  BvwUseType type,
						  GError **error);

EXPORT char *bacon_video_widget_get_backend_name (BaconVideoWidget *bvw);

/* Actions */

EXPORT GtkWidget *bacon_video_widget_get_window (BaconVideoWidget *bvw);
EXPORT gboolean bacon_video_widget_open	 (BaconVideoWidget *bvw,
						  const char *mrl,
						  GError **error);
EXPORT gchar *bacon_video_widget_get_mrl (BaconVideoWidget * Bvw);
EXPORT gboolean bacon_video_widget_play                 (BaconVideoWidget *bvw);
EXPORT void bacon_video_widget_pause			 (BaconVideoWidget *bvw);
EXPORT gboolean bacon_video_widget_is_playing           (BaconVideoWidget *bvw);
EXPORT void bacon_video_widget_stop                     (BaconVideoWidget *bvw);
EXPORT void bacon_video_widget_close                    (BaconVideoWidget *bvw);

/* Seeking and length */
EXPORT gboolean bacon_video_widget_is_seekable          (BaconVideoWidget *bvw);
EXPORT gboolean bacon_video_widget_seek		 (BaconVideoWidget *bvw,
						  float position);
EXPORT gboolean bacon_video_widget_seek_time		 	(BaconVideoWidget *bvw,
						  						gint64 time, gboolean accurate);
EXPORT gboolean bacon_video_widget_segment_seek 		(BaconVideoWidget *bvw,
												gint64 start, gint64 stop);
EXPORT gboolean bacon_video_widget_seek_in_segment     (BaconVideoWidget *bvw, gint64 pos );
EXPORT gboolean  bacon_video_widget_seek_to_next_frame (BaconVideoWidget *bvw, gboolean in_segment);
EXPORT gboolean  bacon_video_widget_seek_to_previous_frame (BaconVideoWidget *bvw, gboolean in_segment);
EXPORT gboolean bacon_video_widget_segment_stop_update(BaconVideoWidget *bvw, gint64 stop);
EXPORT gboolean bacon_video_widget_segment_start_update(BaconVideoWidget *bvw,gint64 start);
EXPORT gboolean bacon_video_widget_can_direct_seek	 	(BaconVideoWidget *bvw);
EXPORT float bacon_video_widget_get_position           (BaconVideoWidget *bvw);
EXPORT gint64 bacon_video_widget_get_current_time      (BaconVideoWidget *bvw);
EXPORT gint64 bacon_video_widget_get_accurate_current_time (BaconVideoWidget *bvw);
EXPORT gint64 bacon_video_widget_get_stream_length     (BaconVideoWidget *bvw);
EXPORT gboolean bacon_video_widget_set_rate     (BaconVideoWidget *bvw, gfloat rate);
EXPORT gboolean bacon_video_widget_set_rate_in_segment    (BaconVideoWidget *bvw, gfloat rate, gint64 stop);


/* Audio volume */
EXPORT gboolean bacon_video_widget_can_set_volume       (BaconVideoWidget *bvw);
EXPORT void bacon_video_widget_set_volume               (BaconVideoWidget *bvw,
						  int volume);
EXPORT int bacon_video_widget_get_volume                (BaconVideoWidget *bvw);

/* Properties */
EXPORT void bacon_video_widget_set_logo		 (BaconVideoWidget *bvw,
						  char *filename);

EXPORT void  bacon_video_widget_set_logo_mode		 (BaconVideoWidget *bvw,
						  gboolean logo_mode);
EXPORT gboolean bacon_video_widget_get_logo_mode	 (BaconVideoWidget *bvw);

EXPORT void bacon_video_widget_set_fullscreen		 (BaconVideoWidget *bvw,
						  gboolean fullscreen);

EXPORT void bacon_video_widget_set_show_cursor          (BaconVideoWidget *bvw,
						  gboolean use_cursor);
EXPORT gboolean bacon_video_widget_get_show_cursor      (BaconVideoWidget *bvw);

EXPORT gboolean bacon_video_widget_get_auto_resize	 (BaconVideoWidget *bvw);
EXPORT void bacon_video_widget_set_auto_resize		 (BaconVideoWidget *bvw,
						  gboolean auto_resize);


EXPORT void bacon_video_widget_set_media_device         (BaconVideoWidget *bvw,
						  const char *path);


/* Metadata */
typedef enum {
	BVW_INFO_TITLE,
	BVW_INFO_ARTIST,
	BVW_INFO_YEAR,
	BVW_INFO_ALBUM,
	BVW_INFO_DURATION,
	BVW_INFO_TRACK_NUMBER,
	/* Video */
	BVW_INFO_HAS_VIDEO,
	BVW_INFO_DIMENSION_X,
	BVW_INFO_DIMENSION_Y,
	BVW_INFO_VIDEO_BITRATE,
	BVW_INFO_VIDEO_CODEC,
	BVW_INFO_FPS,
	/* Audio */
	BVW_INFO_HAS_AUDIO,
	BVW_INFO_AUDIO_BITRATE,
	BVW_INFO_AUDIO_CODEC,
	BVW_INFO_AUDIO_SAMPLE_RATE,
	BVW_INFO_AUDIO_CHANNELS
} BaconVideoWidgetMetadataType;

EXPORT void bacon_video_widget_get_metadata		 (BaconVideoWidget *bvw,
						  BaconVideoWidgetMetadataType
						  type,
						  GValue *value);



typedef enum {
	BVW_RATIO_AUTO,
	BVW_RATIO_SQUARE,
	BVW_RATIO_FOURBYTHREE,
	BVW_RATIO_ANAMORPHIC,
	BVW_RATIO_DVB
} BaconVideoWidgetAspectRatio;

void bacon_video_widget_redraw_last_frame (BaconVideoWidget *bvw);

void bacon_video_widget_set_aspect_ratio         (BaconVideoWidget *bvw,
						  BaconVideoWidgetAspectRatio
						  ratio);
BaconVideoWidgetAspectRatio bacon_video_widget_get_aspect_ratio
						 (BaconVideoWidget *bvw);

void bacon_video_widget_set_scale_ratio          (BaconVideoWidget *bvw,
						  float ratio);

gboolean bacon_video_widget_can_set_zoom	 (BaconVideoWidget *bvw);
void bacon_video_widget_set_zoom		 (BaconVideoWidget *bvw,
						  int zoom);
int bacon_video_widget_get_zoom			 (BaconVideoWidget *bvw);





/* Screenshot functions */
gboolean bacon_video_widget_can_get_frames       (BaconVideoWidget *bvw,
						  GError **error);
EXPORT GdkPixbuf *bacon_video_widget_get_current_frame (BaconVideoWidget *bvw);




/* Audio-out functions */
typedef enum {
	BVW_AUDIO_SOUND_STEREO,
	BVW_AUDIO_SOUND_4CHANNEL,
	BVW_AUDIO_SOUND_41CHANNEL,
	BVW_AUDIO_SOUND_5CHANNEL,
	BVW_AUDIO_SOUND_51CHANNEL,
	BVW_AUDIO_SOUND_AC3PASSTHRU
} BaconVideoWidgetAudioOutType;

BaconVideoWidgetAudioOutType bacon_video_widget_get_audio_out_type
						 (BaconVideoWidget *bvw);
gboolean bacon_video_widget_set_audio_out_type   (BaconVideoWidget *bvw,
						  BaconVideoWidgetAudioOutType
						  type);

G_END_DECLS

#endif				/* HAVE_BACON_VIDEO_WIDGET_H */
