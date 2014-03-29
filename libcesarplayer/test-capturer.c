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

#include <gtk/gtk.h>
#include <stdlib.h>
#include <unistd.h>
#include "gst-camera-capturer.h"


static void
rec_clicked_cb (GtkButton *b, GstCameraCapturer *gcc)
{
  gst_camera_capturer_start (gcc);
}

static void
stop_clicked (GtkButton *b, GstCameraCapturer *gcc)
{
  gst_camera_capturer_stop (gcc);
}

void
create_window (GstCameraCapturer * gvc)
{
  GtkWidget *window, *recbutton, *stopbutton, *vbox, *hbox;

  /* Create a new window */
  window = gtk_window_new (GTK_WINDOW_TOPLEVEL);
  gtk_window_set_title (GTK_WINDOW (window), "Capturer");
  vbox = gtk_vbox_new (TRUE, 0);
  hbox = gtk_hbox_new (TRUE, 0);
  recbutton = gtk_button_new_from_stock ("gtk-rec");
  stopbutton = gtk_button_new_from_stock ("gtk-stop");

  gtk_container_add (GTK_CONTAINER (window), GTK_WIDGET (vbox));
  gtk_box_pack_start (GTK_BOX(vbox), GTK_WIDGET (gvc), TRUE, TRUE, 0);
  gtk_box_pack_start (GTK_BOX(vbox), GTK_WIDGET (hbox), FALSE, FALSE, 0);
  gtk_box_pack_start(GTK_BOX(hbox), recbutton, TRUE, TRUE, 0);
  gtk_box_pack_start(GTK_BOX(hbox), stopbutton, TRUE, TRUE, 0);
  gtk_widget_show_all (window);

  g_signal_connect (G_OBJECT (recbutton), "clicked",
      G_CALLBACK (rec_clicked_cb), gvc);
  g_signal_connect (G_OBJECT (stopbutton), "clicked",
      G_CALLBACK (stop_clicked), gvc);
}


int
main (int argc, char *argv[])
{
  GstCameraCapturer *gvc;
  GError *error = NULL;

  if (argc != 4) {
    g_print("Usage: test-encoder output_file device_type device-id\n");
    return 1;
  }
  gtk_init (&argc, &argv);

  /*Create GstVideoCapturer */
  gst_camera_capturer_init_backend (&argc, &argv);
  gvc = gst_camera_capturer_new ("test", &error);

  gst_camera_capturer_set_source (gvc, atoi(argv[2]), SYSVIDEOSRC);
  gst_camera_capturer_set_video_encoder (gvc, VIDEO_ENCODER_H264);
  gst_camera_capturer_set_audio_encoder (gvc, AUDIO_ENCODER_AAC);
  gst_camera_capturer_set_video_muxer (gvc, VIDEO_MUXER_MP4);
  g_object_set (gvc, "device-id", argv[3], NULL);
  g_object_set (gvc, "output_file", argv[1], NULL);

  create_window (gvc);
  gst_camera_capturer_run(gvc);
  gtk_main ();

  return 0;
}
