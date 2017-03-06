/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * main.c
 * Copyright (C) Andoni Morales Alastruey 2008 <ylatuya@gmail.com>
 * 
 * main.c is free software: you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the
 * Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * main.c is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along
 * with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

/* Compile with:
 * gcc -o test-editor test-editor.c gst-video-editor.c `pkg-config --cflags --libs gstreamer-0.10 gtk+-2.0` -DOSTYPE_LINUX -O0 -g
 */

#include <stdlib.h>
#include <unistd.h>
#include <gst/gst.h>
#include "gst-video-editor.h"

static GMainLoop *loop;

static gboolean
percent_done_cb (GstVideoEditor * remuxer, gfloat percent,
    GstVideoEditor * editor)
{
  if (percent == 1) {
    g_print ("SUCESS!\n");
    g_main_loop_quit (loop);
  } else {
    g_print ("----> %f%%\n", percent);
  }
  return TRUE;
}

static gboolean
error_cb (GstVideoEditor * remuxer, gchar * error, GstVideoEditor * editor)
{
  g_print ("ERROR: %s\n", error);
  g_main_loop_quit (loop);
  return FALSE;
}

int
main (int argc, char *argv[])
{
  GstVideoEditor *editor;
  VideoEncoderType video_encoder;
  VideoMuxerType video_muxer;
  AudioEncoderType audio_encoder;
  gchar *input_file, *output_file, *format;
  gchar *err = NULL;
  guint64 start, stop;
  gboolean with_audio, with_title;
  gint i, bitrate;

  gst_video_editor_init_backend (&argc, &argv);

  if (argc < 9) {
    g_print
        ("Usage: test-remuxer output_file format bitrate with_audio with_title input_file start stop\n");
    return 1;
  }
  output_file = argv[1];
  format = argv[2];
  bitrate = (gint) g_strtod (argv[3], NULL);
  with_audio = (gboolean) g_strtod (argv[4], NULL);
  with_title = (gboolean) g_strtod (argv[5], NULL);

  if (!g_strcmp0 (format, "mp4")) {
    video_encoder = VIDEO_ENCODER_H264;
    video_muxer = VIDEO_MUXER_MP4;
    audio_encoder = AUDIO_ENCODER_AAC;
  } else if (!g_strcmp0 (format, "avi")) {
    video_encoder = VIDEO_ENCODER_MPEG4;
    video_muxer = VIDEO_MUXER_AVI;
    audio_encoder = AUDIO_ENCODER_MP3;
  } else {
    err = g_strdup_printf ("Invalid format %s\n", format);
    goto error;
  }

  editor = gst_video_editor_new (NULL);
  gst_video_editor_set_encoding_format (editor, output_file, video_encoder,
      audio_encoder, video_muxer, bitrate, 128, 1280, 720, 25, 1, with_audio,
      with_title);

  for (i = 8; i <= argc; i = i + 3) {
    gchar *title;
    title = g_strdup_printf ("Title %d", i - 4);

    input_file = argv[i - 2];
    start = (guint64) g_strtod (argv[i - 1], NULL);
    stop = (guint64) g_strtod (argv[i], NULL);
    if (g_str_has_suffix (input_file, ".png")) {
      gst_video_editor_add_image_segment (editor, input_file, start,
          stop - start, title, 0, 0, 0, 0);
    } else {
      gst_video_editor_add_segment (editor, input_file, start, stop - start,
          (gfloat) 1, title, TRUE, 0, 0, 0, 0);
    }
    g_free (title);
  }

  loop = g_main_loop_new (NULL, FALSE);
  g_signal_connect (editor, "error", G_CALLBACK (error_cb), editor);
  g_signal_connect (editor, "percent_completed", G_CALLBACK (percent_done_cb),
      editor);
  gst_video_editor_start (editor, FALSE);
  g_main_loop_run (loop);

  return 0;

error:
  g_print ("ERROR: %s", err);
  return 1;

}
