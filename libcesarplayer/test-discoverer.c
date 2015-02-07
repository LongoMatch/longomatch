/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * test-discoverer.c
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

#include "lgm-utils.h"

int
main (int argc, char *argv[])
{
  guint64 duration;
  guint width, height, fps_n, fps_d, par_n, par_d;
  gchar *container, *audio_codec, *video_codec;
  GstDiscovererResult res;
  GError *err = NULL;

  lgm_init_backend (0, NULL);

  if (argc != 2) {
    g_print ("Usage: test-discoverer file_uri\n");
    return 1;
  }

  g_print ("Discovering file %s\n", argv[1]);
  res = lgm_discover_uri (argv[1], &duration, &width, &height, &fps_n, &fps_d,
      &par_n, &par_d, &container, &video_codec, &audio_codec, &err);

  if (err != NULL) {
    g_print ("ERROR: %s\n", err->message);
    exit (1);
  } else if (res != GST_DISCOVERER_OK) {
    g_print ("ERROR: %d\n", res);
    exit (1);
  }

  g_print ("Duration: %" GST_TIME_FORMAT "\n", GST_TIME_ARGS (duration));
  if (container != NULL) {
    g_print ("Container: %s\n", container);
  }
  if (video_codec != NULL) {
    g_print ("Video: %s %dx%d@%d/%d\n", video_codec, width, height, fps_n,
        fps_d);
  }
  if (audio_codec != NULL) {
    g_print ("Audio: %s\n", audio_codec);
  }

}
