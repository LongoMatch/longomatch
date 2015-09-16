/*
 * Copyright (C) 2014 Andoni Morales <ylatuya@gmail.com>
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
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
 *
 */

#ifndef HAVE_LGM_VIDEO_PLAYER_H
#define HAVE_LGM_VIDEO_PLAYER_H

#ifdef WIN32
#define EXPORT __declspec (dllexport)
#else
#define EXPORT
#endif

#include "lgm-utils.h"

G_BEGIN_DECLS
#define LGM_TYPE_VIDEO_WIDGET              (lgm_video_player_get_type ())
#define LGM_VIDEO_PLAYER(obj)              (G_TYPE_CHECK_INSTANCE_CAST ((obj), lgm_video_player_get_type (), LgmVideoPlayer))
#define LGM_VIDEO_PLAYER_CLASS(klass)      (G_TYPE_CHECK_CLASS_CAST ((klass), lgm_video_player_get_type (), LgmVideoPlayerClass))
#define LGM_IS_VIDEO_WIDGET(obj)           (G_TYPE_CHECK_INSTANCE_TYPE (obj, lgm_video_player_get_type ()))
#define LGM_IS_VIDEO_WIDGET_CLASS(klass)   (G_CHECK_INSTANCE_GET_CLASS ((klass), lgm_video_player_get_type ()))
#define LGM_ERROR lgm_video_player_error_quark ()
typedef struct LgmVideoPlayerPrivate LgmVideoPlayerPrivate;

typedef struct
{
  GstElement parent;
  LgmVideoPlayerPrivate *priv;
} LgmVideoPlayer;

typedef struct
{
  GstElementClass parent_class;

  void (*error) (LgmVideoPlayer * lvp, const char *message);
  void (*eos) (LgmVideoPlayer * lvp);
  void (*tick) (LgmVideoPlayer * lvp, gint64 current_time,
      gint64 stream_length, gdouble current_position);
  void (*state_change) (LgmVideoPlayer * lvp, gboolean playing);
  void (*ready_to_seek) (LgmVideoPlayer * lvp);
} LgmVideoPlayerClass;


EXPORT GQuark lgm_video_player_error_quark (void) G_GNUC_CONST;
EXPORT GType lgm_video_player_get_type (void) G_GNUC_CONST;

typedef enum
{
  LGM_USE_TYPE_VIDEO,
  LGM_USE_TYPE_CAPTURE,
} LgmUseType;


EXPORT LgmVideoPlayer *lgm_video_player_new               (LgmUseType type,
                                                           GError ** error);

EXPORT void lgm_video_player_set_window_handle           (LgmVideoPlayer *lvp,
                                                          guintptr windows_handle);

/* Actions */
EXPORT gboolean lgm_video_player_open                     (LgmVideoPlayer * lvp,
                                                           const char *mrl, GError ** error);

EXPORT gboolean lgm_video_player_play                     (LgmVideoPlayer * lvp,
                                                           gboolean synchronous);

EXPORT void lgm_video_player_pause                        (LgmVideoPlayer * lvp,
                                                           gboolean synchronous);

EXPORT void lgm_video_player_stop                         (LgmVideoPlayer * lvp,
                                                           gboolean synchronous);

EXPORT void lgm_video_player_close                        (LgmVideoPlayer * lvp);

EXPORT gboolean lgm_video_player_is_playing               (LgmVideoPlayer * lvp);

/* Seeking and length */
EXPORT gboolean lgm_video_player_is_seekable              (LgmVideoPlayer * lvp);

EXPORT gboolean lgm_video_player_seek_time                (LgmVideoPlayer * lvp,
                                                           gint64 time,
                                                           gboolean accurate,
                                                           gboolean synchronous);

EXPORT gboolean lgm_video_player_seek_to_next_frame       (LgmVideoPlayer * lvp);

EXPORT gboolean lgm_video_player_seek_to_previous_frame   (LgmVideoPlayer * lvp);

EXPORT gdouble lgm_video_player_get_position              (LgmVideoPlayer * lvp);

EXPORT gint64 lgm_video_player_get_current_time           (LgmVideoPlayer * lvp);

EXPORT gint64 lgm_video_player_get_stream_length          (LgmVideoPlayer * lvp);

EXPORT gboolean lgm_video_player_set_rate                 (LgmVideoPlayer * lvp,
                                                           gdouble rate);

/* Audio volume */
EXPORT void lgm_video_player_set_volume                   (LgmVideoPlayer * lvp,
                                                           gdouble volume);

EXPORT double lgm_video_player_get_volume                 (LgmVideoPlayer * lvp);

/* Screenshot functions */
EXPORT GdkPixbuf *lgm_video_player_get_current_frame      (LgmVideoPlayer * lvp);
EXPORT void lgm_video_player_unref_pixbuf                 (GdkPixbuf * pixbuf);

EXPORT void lgm_video_player_expose                       (LgmVideoPlayer * lvp);

G_END_DECLS
#endif /* HAVE_LGM_VIDEO_PLAYER_H */
