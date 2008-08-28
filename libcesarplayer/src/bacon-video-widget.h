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
#define gvc_ERROR bacon_video_widget_error_quark ()

typedef struct BaconVideoWidgetPrivate BaconVideoWidgetPrivate;


typedef struct {
	GObject parent;
	BaconVideoWidgetPrivate *priv;
} BaconVideoWidget;

typedef struct {
	GObjectClass parent_class;

	void (*error) (BaconVideoWidget *gvc, const char *message);
	void (*eos) (BaconVideoWidget *gvc);
	void (*got_metadata) (BaconVideoWidget *gvc);
	void (*segment_done) (BaconVideoWidget *gvc);
	void (*got_redirect) (BaconVideoWidget *gvc, const char *mrl);
	void (*title_change) (BaconVideoWidget *gvc, const char *title);
	void (*channels_change) (BaconVideoWidget *gvc);
	void (*tick) (BaconVideoWidget *gvc, gint64 current_time, gint64 stream_length,
			float current_position, gboolean seekable);
	void (*buffering) (BaconVideoWidget *gvc, guint progress);
	void (*state_changed) (BaconVideoWidget *gvc, gboolean playing);
	void (*got_duration) (BaconVideoWidget *gvc);
	void (*ready_to_seek) (BaconVideoWidget *gvc);
} BaconVideoWidgetClass;

typedef enum {
	/* Plugins */
	gvc_ERROR_AUDIO_PLUGIN,
	gvc_ERROR_NO_PLUGIN_FOR_FILE,
	gvc_ERROR_VIDEO_PLUGIN,
	gvc_ERROR_AUDIO_BUSY,
	/* File */
	gvc_ERROR_BROKEN_FILE,
	gvc_ERROR_FILE_GENERIC,
	gvc_ERROR_FILE_PERMISSION,
	gvc_ERROR_FILE_ENCRYPTED,
	gvc_ERROR_FILE_NOT_FOUND,
	/* Devices */
	gvc_ERROR_DVD_ENCRYPTED,
	gvc_ERROR_INVALID_DEVICE,
	/* Network */
	gvc_ERROR_UNKNOWN_HOST,
	gvc_ERROR_NETWORK_UNREACHABLE,
	gvc_ERROR_CONNECTION_REFUSED,
	/* Generic */
	gvc_ERROR_UNVALID_LOCATION,
	gvc_ERROR_GENERIC,
	gvc_ERROR_CODEC_NOT_HANDLED,
	gvc_ERROR_AUDIO_ONLY,
	gvc_ERROR_CANNOT_CAPTURE,
	gvc_ERROR_READ_ERROR,
	gvc_ERROR_PLUGIN_LOAD,
	gvc_ERROR_EMPTY_FILE
} gvcError;

GQuark bacon_video_widget_error_quark		 (void) G_GNUC_CONST;
GType bacon_video_widget_get_type                (void);
GOptionGroup* bacon_video_widget_get_option_group (void);
/* This can be used if the app does not use popt */
EXPORT void bacon_video_widget_init_backend		 (int *argc, char ***argv);

typedef enum {
	gvc_USE_TYPE_VIDEO,
	gvc_USE_TYPE_AUDIO,
	gvc_USE_TYPE_CAPTURE,
	gvc_USE_TYPE_METADATA
} gvcUseType;

EXPORT BaconVideoWidget *bacon_video_widget_new		 (int width, int height,
						  gvcUseType type,
						  GError **error);

EXPORT char *bacon_video_widget_get_backend_name (BaconVideoWidget *gvc);

/* Actions */

EXPORT GtkWidget *bacon_video_widget_get_window (BaconVideoWidget *gvc);
EXPORT gboolean bacon_video_widget_open	 (BaconVideoWidget *gvc,
						  const char *mrl,
						  GError **error);
EXPORT gchar *bacon_video_widget_get_mrl (BaconVideoWidget * gvc);
EXPORT gboolean bacon_video_widget_play                 (BaconVideoWidget *gvc);
EXPORT void bacon_video_widget_pause			 (BaconVideoWidget *gvc);
EXPORT gboolean bacon_video_widget_is_playing           (BaconVideoWidget *gvc);
EXPORT void bacon_video_widget_stop                     (BaconVideoWidget *gvc);
EXPORT void bacon_video_widget_close                    (BaconVideoWidget *gvc);

/* Seeking and length */
EXPORT gboolean bacon_video_widget_is_seekable          (BaconVideoWidget *gvc);
EXPORT gboolean bacon_video_widget_seek		 (BaconVideoWidget *gvc,
						  float position);
EXPORT gboolean bacon_video_widget_seek_time		 	(BaconVideoWidget *gvc,
						  						gint64 time, gboolean accurate);
EXPORT gboolean bacon_video_widget_segment_seek 		(BaconVideoWidget *gvc,
												gint64 start, gint64 stop);
EXPORT gboolean bacon_video_widget_seek_in_segment     (BaconVideoWidget *gvc, gint64 pos );
EXPORT gboolean bacon_video_widget_can_direct_seek	 	(BaconVideoWidget *gvc);
EXPORT float bacon_video_widget_get_position           (BaconVideoWidget *gvc);
EXPORT gint64 bacon_video_widget_get_current_time      (BaconVideoWidget *gvc);
EXPORT gint64 bacon_video_widget_get_accurate_current_time (BaconVideoWidget *gvc);
EXPORT gint64 bacon_video_widget_get_stream_length     (BaconVideoWidget *gvc);
EXPORT gboolean bacon_video_widget_set_rate     (BaconVideoWidget *gvc, gfloat rate);
EXPORT gboolean bacon_video_widget_set_rate_in_segment    (BaconVideoWidget *gvc, gfloat rate, gint64 stop);



/* Audio volume */
EXPORT gboolean bacon_video_widget_can_set_volume       (BaconVideoWidget *gvc);
EXPORT void bacon_video_widget_set_volume               (BaconVideoWidget *gvc,
						  int volume);
EXPORT int bacon_video_widget_get_volume                (BaconVideoWidget *gvc);

/* Properties */
EXPORT void bacon_video_widget_set_logo		 (BaconVideoWidget *gvc,
						  char *filename);

EXPORT void  bacon_video_widget_set_logo_mode		 (BaconVideoWidget *gvc,
						  gboolean logo_mode);
EXPORT gboolean bacon_video_widget_get_logo_mode	 (BaconVideoWidget *gvc);

EXPORT void bacon_video_widget_set_fullscreen		 (BaconVideoWidget *gvc,
						  gboolean fullscreen);

EXPORT void bacon_video_widget_set_show_cursor          (BaconVideoWidget *gvc,
						  gboolean use_cursor);
EXPORT gboolean bacon_video_widget_get_show_cursor      (BaconVideoWidget *gvc);

EXPORT gboolean bacon_video_widget_get_auto_resize	 (BaconVideoWidget *gvc);
EXPORT void bacon_video_widget_set_auto_resize		 (BaconVideoWidget *gvc,
						  gboolean auto_resize);


EXPORT void bacon_video_widget_set_media_device         (BaconVideoWidget *gvc,
						  const char *path);


/* Metadata */
typedef enum {
	gvc_INFO_TITLE,
	gvc_INFO_ARTIST,
	gvc_INFO_YEAR,
	gvc_INFO_ALBUM,
	gvc_INFO_DURATION,
	gvc_INFO_TRACK_NUMBER,
	/* Video */
	gvc_INFO_HAS_VIDEO,
	gvc_INFO_DIMENSION_X,
	gvc_INFO_DIMENSION_Y,
	gvc_INFO_VIDEO_BITRATE,
	gvc_INFO_VIDEO_CODEC,
	gvc_INFO_FPS,
	/* Audio */
	gvc_INFO_HAS_AUDIO,
	gvc_INFO_AUDIO_BITRATE,
	gvc_INFO_AUDIO_CODEC,
	gvc_INFO_AUDIO_SAMPLE_RATE,
	gvc_INFO_AUDIO_CHANNELS
} BaconVideoWidgetMetadataType;

EXPORT void bacon_video_widget_get_metadata		 (BaconVideoWidget *gvc,
						  BaconVideoWidgetMetadataType
						  type,
						  GValue *value);



typedef enum {
	gvc_RATIO_AUTO,
	gvc_RATIO_SQUARE,
	gvc_RATIO_FOURBYTHREE,
	gvc_RATIO_ANAMORPHIC,
	gvc_RATIO_DVB
} BaconVideoWidgetAspectRatio;



void bacon_video_widget_set_aspect_ratio         (BaconVideoWidget *gvc,
						  BaconVideoWidgetAspectRatio
						  ratio);
BaconVideoWidgetAspectRatio bacon_video_widget_get_aspect_ratio
						 (BaconVideoWidget *gvc);

void bacon_video_widget_set_scale_ratio          (BaconVideoWidget *gvc,
						  float ratio);

gboolean bacon_video_widget_can_set_zoom	 (BaconVideoWidget *gvc);
void bacon_video_widget_set_zoom		 (BaconVideoWidget *gvc,
						  int zoom);
int bacon_video_widget_get_zoom			 (BaconVideoWidget *gvc);





/* Screenshot functions */
gboolean bacon_video_widget_can_get_frames       (BaconVideoWidget *gvc,
						  GError **error);
EXPORT GdkPixbuf *bacon_video_widget_get_current_frame (BaconVideoWidget *gvc);




/* Audio-out functions */
typedef enum {
	gvc_AUDIO_SOUND_STEREO,
	gvc_AUDIO_SOUND_4CHANNEL,
	gvc_AUDIO_SOUND_41CHANNEL,
	gvc_AUDIO_SOUND_5CHANNEL,
	gvc_AUDIO_SOUND_51CHANNEL,
	gvc_AUDIO_SOUND_AC3PASSTHRU
} BaconVideoWidgetAudioOutType;

BaconVideoWidgetAudioOutType bacon_video_widget_get_audio_out_type
						 (BaconVideoWidget *gvc);
gboolean bacon_video_widget_set_audio_out_type   (BaconVideoWidget *gvc,
						  BaconVideoWidgetAudioOutType
						  type);

G_END_DECLS

#endif				/* HAVE_BACON_VIDEO_WIDGET_H */
