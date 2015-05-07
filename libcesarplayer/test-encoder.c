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

#include <stdlib.h>
#include <unistd.h>
#include "gst-video-encoder.h"

static GMainLoop *loop;

static gboolean
percent_done_cb (GstVideoEncoder * remuxer, gfloat percent,
    GstVideoEncoder * editor)
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
error_cb (GstVideoEncoder * remuxer, gchar * error, GstVideoEncoder * editor)
{
  g_print ("ERROR: %s\n", error);
  g_main_loop_quit (loop);
  return TRUE;
}

int
main (int argc, char *argv[])
{
  GstVideoEncoder *encoder;
  VideoEncoderType video_encoder;
  VideoMuxerType video_muxer;
  AudioEncoderType audio_encoder;
  gchar *output_file;
  GError *err = NULL;
  gint i;

  gst_video_encoder_init_backend (&argc, &argv);

  if (argc < 3) {
    g_print ("Usage: test-remuxer output_file input_files \n");
    return 1;
  }

  output_file = argv[1];

  video_encoder = VIDEO_ENCODER_H264;
  video_muxer = VIDEO_MUXER_MP4;
  audio_encoder = AUDIO_ENCODER_AAC;

  encoder = gst_video_encoder_new (output_file, &err);
  gst_video_encoder_set_encoding_format (encoder, video_encoder,
      audio_encoder, video_muxer, 500, 128, 240, 180, 25, 1);

  for (i = 2; i < argc; i++) {
    guint64 duration;
    guint width, height, fps_n, fps_d, par_n, par_d;
    gchar *container, *video_codec, *audio_codec;
    GError *err = NULL;

    lgm_discover_uri (argv[i], &duration, &width, &height, &fps_n,
        &fps_d, &par_n, &par_d, &container, &video_codec, &audio_codec, &err);
    if (err != NULL) {
      g_print ("Error parsing file %s \n", argv[i]);
      exit (1);
    }
    gst_video_encoder_add_file (encoder, argv[i], duration / GST_MSECOND, width,
        height, (gdouble) par_n / par_d);
  }

  loop = g_main_loop_new (NULL, FALSE);
  g_signal_connect (encoder, "error", G_CALLBACK (error_cb), encoder);
  g_signal_connect (encoder, "percent_completed", G_CALLBACK (percent_done_cb),
      encoder);
  gst_video_encoder_start (encoder);
  g_main_loop_run (loop);

  return 0;
}
